namespace EduFlow.Application.Features.CourseFeature.PublishCourse;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record PublishCourseRequest(Guid Id);

public sealed record PublishCourseResponse(Guid Id, CourseStatus Status, DateTime? PublishedOn);

public sealed class PublishCourseHandler(
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<PublishCourseRequest, Result<PublishCourseResponse>>
{
    public async Task<Result<PublishCourseResponse>> HandleAsync(PublishCourseRequest command, CancellationToken cancellationToken)
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

        if (course.Status == CourseStatus.Published)
        {
            return CourseErrors.AlreadyPublished;
        }

        course.Status = CourseStatus.Published;
        course.PublishedOn = DateTime.UtcNow;

        await courseRepository.UpdateAsync(course, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new PublishCourseResponse(course.Id, course.Status, course.PublishedOn));
    }
}
