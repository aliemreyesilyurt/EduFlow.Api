namespace EduFlow.Application.Features.CourseFeature.CreateCourse;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record CreateCourseRequest(string Title, string? Description);

public sealed record CreateCourseResponse(
    Guid Id,
    string Title,
    string? Description,
    Guid InstructorId,
    CourseStatus Status);

public sealed class CreateCourseHandler(
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<CreateCourseRequest, Result<CreateCourseResponse>>
{
    public async Task<Result<CreateCourseResponse>> HandleAsync(CreateCourseRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.TenantId is null || tenantContext.UserId is not { } instructorId)
        {
            return CourseErrors.Forbidden;
        }

        var course = new Course
        {
            Id = Guid.CreateVersion7(),
            Title = command.Title,
            Description = command.Description,
            InstructorId = instructorId,
            Status = CourseStatus.Draft
        };

        await courseRepository.AddAsync(course, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new CreateCourseResponse(
            course.Id, course.Title, course.Description, course.InstructorId, course.Status));
    }
}
