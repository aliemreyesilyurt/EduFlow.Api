namespace EduFlow.Application.Features.CourseFeature.UpdateCourse;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record UpdateCourseRequest(Guid Id, string Title, string? Description);

public sealed record UpdateCourseResponse(
    Guid Id,
    string Title,
    string? Description,
    Guid InstructorId,
    CourseStatus Status);

public sealed class UpdateCourseHandler(
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<UpdateCourseRequest, Result<UpdateCourseResponse>>
{
    public async Task<Result<UpdateCourseResponse>> HandleAsync(UpdateCourseRequest command, CancellationToken cancellationToken)
    {
        var course = await courseRepository.FindAsync(c => c.Id == command.Id, cancellationToken);

        if (course is null)
        {
            return CourseErrors.NotFound(command.Id);
        }

        if (!CourseAccess.CanManage(course, tenantContext))
        {
            return CourseErrors.Forbidden;
        }

        course.Title = command.Title;
        course.Description = command.Description;

        await courseRepository.UpdateAsync(course, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new UpdateCourseResponse(
            course.Id, course.Title, course.Description, course.InstructorId, course.Status));
    }
}
