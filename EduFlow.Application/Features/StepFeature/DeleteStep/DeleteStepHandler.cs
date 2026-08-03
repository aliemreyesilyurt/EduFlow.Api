namespace EduFlow.Application.Features.StepFeature.DeleteStep;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record DeleteStepRequest(Guid Id);

public sealed class DeleteStepHandler(
    IRepository<Step> stepRepository,
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<DeleteStepRequest, Result>
{
    public async Task<Result> HandleAsync(DeleteStepRequest command, CancellationToken cancellationToken)
    {
        var step = await stepRepository.FindAsync(s => s.Id == command.Id, cancellationToken);

        if (step is null)
        {
            return StepErrors.NotFound(command.Id);
        }

        var course = await courseRepository.FindAsync(c => c.Id == step.CourseId, cancellationToken);

        if (course is null || !CourseAccess.CanManage(course, tenantContext))
        {
            return StepErrors.Forbidden;
        }

        await stepRepository.DeleteAsync(step, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success();
    }
}
