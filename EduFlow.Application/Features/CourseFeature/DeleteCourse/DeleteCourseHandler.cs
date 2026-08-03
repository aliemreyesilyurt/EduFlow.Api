namespace EduFlow.Application.Features.CourseFeature.DeleteCourse;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record DeleteCourseRequest(Guid Id);

public sealed class DeleteCourseHandler(
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<DeleteCourseRequest, Result>
{
    public async Task<Result> HandleAsync(DeleteCourseRequest command, CancellationToken cancellationToken)
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

        await courseRepository.DeleteAsync(course, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
