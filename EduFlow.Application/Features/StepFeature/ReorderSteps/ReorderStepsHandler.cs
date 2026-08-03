namespace EduFlow.Application.Features.StepFeature.ReorderSteps;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record ReorderStepsRequest(Guid CourseId, IReadOnlyList<Guid> StepIds);

public sealed class ReorderStepsHandler(
    IRepository<Course> courseRepository,
    IRepository<Step> stepRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<ReorderStepsRequest, Result>
{
    public async Task<Result> HandleAsync(ReorderStepsRequest command, CancellationToken cancellationToken)
    {
        var course = await courseRepository.FindAsync(c => c.Id == command.CourseId, cancellationToken);

        if (course is null)
        {
            return CourseErrors.NotFound(command.CourseId);
        }

        if (!CourseAccess.CanManage(course, tenantContext))
        {
            return CourseErrors.Forbidden;
        }

        var allSteps = await stepRepository.GetAllAsync(cancellationToken);
        var courseSteps = allSteps.Where(s => s.CourseId == command.CourseId).ToDictionary(s => s.Id);

        if (courseSteps.Count != command.StepIds.Count || !command.StepIds.All(courseSteps.ContainsKey))
        {
            return StepErrors.ReorderMismatch;
        }

        // The (CourseId, Order) unique index would reject any assignment that transiently collides
        // with the current values, so orders are moved through a temporary negative range first.
        for (var i = 0; i < command.StepIds.Count; i++)
        {
            courseSteps[command.StepIds[i]].Order = -(i + 1);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        for (var i = 0; i < command.StepIds.Count; i++)
        {
            courseSteps[command.StepIds[i]].Order = i + 1;
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
