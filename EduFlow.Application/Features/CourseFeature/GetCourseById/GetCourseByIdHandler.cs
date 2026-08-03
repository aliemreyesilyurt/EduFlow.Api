namespace EduFlow.Application.Features.CourseFeature.GetCourseById;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record GetCourseByIdRequest(Guid Id);

public sealed record GetCourseByIdResponse(
    Guid Id,
    string Title,
    string? Description,
    Guid InstructorId,
    CourseStatus Status,
    DateTime? PublishedOn);

public sealed class GetCourseByIdHandler(
    IRepository<Course> courseRepository,
    ITenantContext tenantContext) : IHandler<GetCourseByIdRequest, Result<GetCourseByIdResponse>>
{
    public async Task<Result<GetCourseByIdResponse>> HandleAsync(GetCourseByIdRequest command, CancellationToken cancellationToken)
    {
        var course = await courseRepository.FindAsync(c => c.Id == command.Id, cancellationToken);

        if (course is null || !CourseAccess.CanView(course, tenantContext))
        {
            return CourseErrors.NotFound(command.Id);
        }

        return Result.Success(new GetCourseByIdResponse(
            course.Id, course.Title, course.Description, course.InstructorId, course.Status, course.PublishedOn));
    }
}
