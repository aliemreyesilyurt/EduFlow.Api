namespace EduFlow.Application.Features.ProctoringFeature.GiveProctoringConsent;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.ExamFeature;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GiveProctoringConsentRequest(Guid AttemptId);

public sealed record ProctoringConsentResponse(DateTime ConsentGivenOn);

public sealed class GiveProctoringConsentHandler(
    IRepository<ExamAttempt> examAttemptRepository,
    IUnitOfWork unitOfWork,
    ITenantContext tenantContext) : IHandler<GiveProctoringConsentRequest, Result<ProctoringConsentResponse>>
{
    public async Task<Result<ProctoringConsentResponse>> HandleAsync(
        GiveProctoringConsentRequest command, CancellationToken cancellationToken)
    {
        var attempt = await examAttemptRepository.FindAsync(a => a.Id == command.AttemptId, cancellationToken);

        if (attempt is null || attempt.StudentId != tenantContext.UserId)
        {
            return ExamErrors.AttemptNotFound(command.AttemptId);
        }

        if (attempt.SubmittedOn is not null)
        {
            return ExamErrors.AttemptAlreadySubmitted;
        }

        if (attempt.ProctoringConsentOn is null)
        {
            attempt.ProctoringConsentOn = DateTime.UtcNow;

            await examAttemptRepository.UpdateAsync(attempt, cancellationToken);
            await unitOfWork.CommitAsync(cancellationToken);
        }

        return Result.Success(new ProctoringConsentResponse(attempt.ProctoringConsentOn.Value));
    }
}
