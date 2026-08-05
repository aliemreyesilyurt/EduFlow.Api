namespace EduFlow.Application.Features.CourseFeature.ArchiveCourse;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed record ArchiveCourseRequest(Guid Id);

public sealed record ArchiveCourseResponse(Guid Id, CourseStatus Status);

public sealed class ArchiveCourseHandler(
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<ArchiveCourseRequest, Result<ArchiveCourseResponse>>
{
    public async Task<Result<ArchiveCourseResponse>> HandleAsync(ArchiveCourseRequest command, CancellationToken cancellationToken)
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

        if (course.Status == CourseStatus.Archived)
        {
            return CourseErrors.AlreadyArchived;
        }

        course.Status = CourseStatus.Archived;

        await courseRepository.UpdateAsync(course, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(new ArchiveCourseResponse(course.Id, course.Status));
    }
}
