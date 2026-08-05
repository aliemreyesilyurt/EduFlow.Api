namespace EduFlow.Application.Features.StepFeature.DeleteStep;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Abstractions.Storage;
using EduFlow.Application.Features.CourseFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using Microsoft.Extensions.Logging;

public sealed record DeleteStepRequest(Guid Id);

public sealed class DeleteStepHandler(
    IRepository<Step> stepRepository,
    IRepository<Course> courseRepository,
    IUnitOfWork unitOfWork,
    IFileStorage fileStorage,
    ITenantContext tenantContext,
    ILogger<DeleteStepHandler> logger) : IHandler<DeleteStepRequest, Result>
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

        // Best-effort: the DB delete already succeeded and is the source of truth, so a disk
        // cleanup failure here must not surface as a failed delete to the caller.
        try
        {
            await fileStorage.DeleteDirectoryAsync($"steps/{step.Id}", cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to clean up stored content for deleted step {StepId}", step.Id);
        }

        return Result.Success();
    }
}
