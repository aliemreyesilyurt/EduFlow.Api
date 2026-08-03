namespace EduFlow.Application.Features.CourseFeature.GetAllCourses;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record GetAllCoursesRequest;

public sealed record CourseSummary(
    Guid Id,
    string Title,
    string? Description,
    Guid InstructorId,
    CourseStatus Status,
    DateTime? PublishedOn);

public sealed record GetAllCoursesResponse(IReadOnlyList<CourseSummary> Courses);

public sealed class GetAllCoursesHandler(
    IRepository<Course> courseRepository,
    ITenantContext tenantContext) : IHandler<GetAllCoursesRequest, Result<GetAllCoursesResponse>>
{
    public async Task<Result<GetAllCoursesResponse>> HandleAsync(GetAllCoursesRequest command, CancellationToken cancellationToken)
    {
        var courses = await courseRepository.GetAllAsync(cancellationToken);

        var visible = courses
            .Where(c => CourseAccess.CanView(c, tenantContext))
            .OrderByDescending(c => c.CreatedOn)
            .Select(c => new CourseSummary(c.Id, c.Title, c.Description, c.InstructorId, c.Status, c.PublishedOn))
            .ToList();

        return Result.Success(new GetAllCoursesResponse(visible));
    }
}
