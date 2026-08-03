namespace EduFlow.Application.Features.EnrollmentFeature.CompleteStep;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.StepFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record CompleteStepRequest(Guid StepId);

public sealed record CompleteStepResponse(
    Guid StepId,
    Guid EnrollmentId,
    DateTime CompletedOn,
    int CompletedSteps,
    int TotalSteps,
    double ProgressPercentage,
    bool CourseCompleted);

public sealed class CompleteStepHandler(
    IRepository<Step> stepRepository,
    IRepository<Course> courseRepository,
    IRepository<Enrollment> enrollmentRepository,
    IRepository<StepProgress> stepProgressRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<CompleteStepRequest, Result<CompleteStepResponse>>
{
    public async Task<Result<CompleteStepResponse>> HandleAsync(CompleteStepRequest command, CancellationToken cancellationToken)
    {
        var step = await stepRepository.FindAsync(s => s.Id == command.StepId, cancellationToken);

        if (step is null)
        {
            return StepErrors.NotFound(command.StepId);
        }

        var course = await courseRepository.FindAsync(c => c.Id == step.CourseId, cancellationToken);

        if (course is null)
        {
            return StepErrors.NotFound(command.StepId);
        }

        if (tenantContext.UserId is not { } studentId)
        {
            return EnrollmentErrors.NotEnrolled;
        }

        var enrollment = await enrollmentRepository.FindAsync(
            e => e.CourseId == step.CourseId && e.StudentId == studentId, cancellationToken);

        if (enrollment is null)
        {
            return EnrollmentErrors.NotEnrolled;
        }

        var enrollmentProgress = (await stepProgressRepository.GetAllAsync(cancellationToken))
            .Where(sp => sp.EnrollmentId == enrollment.Id)
            .ToList();

        var existing = enrollmentProgress.FirstOrDefault(sp => sp.StepId == step.Id);
        var completedSteps = enrollmentProgress.Count;
        DateTime completedOn;

        if (existing is null)
        {
            var progress = new StepProgress
            {
                Id = Guid.CreateVersion7(),
                EnrollmentId = enrollment.Id,
                StepId = step.Id,
                CompletedOn = DateTime.UtcNow
            };

            await stepProgressRepository.AddAsync(progress, cancellationToken);
            completedOn = progress.CompletedOn;
            completedSteps += 1;
        }
        else
        {
            completedOn = existing.CompletedOn;
        }

        var totalSteps = (await stepRepository.GetAllAsync(cancellationToken)).Count(s => s.CourseId == step.CourseId);
        var courseCompleted = totalSteps > 0 && completedSteps >= totalSteps;

        if (courseCompleted && enrollment.CompletedOn is null)
        {
            enrollment.CompletedOn = DateTime.UtcNow;
            await enrollmentRepository.UpdateAsync(enrollment, cancellationToken);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        var progressPercentage = totalSteps == 0 ? 0d : Math.Round(100.0 * completedSteps / totalSteps, 2);

        return Result.Success(new CompleteStepResponse(
            step.Id, enrollment.Id, completedOn, completedSteps, totalSteps, progressPercentage, courseCompleted));
    }
}
