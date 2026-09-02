namespace EduFlow.Application.Features.ProctoringFeature.LogProctoringEvents;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Application.Features.ExamFeature;
using EduFlow.Application.Options;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;
using Microsoft.Extensions.Options;

public sealed record ProctoringEventInput(ProctoringEventType Type, DateTime OccurredOn, string? Details);

public sealed record LogProctoringEventsRequest(Guid AttemptId, IReadOnlyList<ProctoringEventInput> Events);

public sealed record LogProctoringEventsResponse(int ViolationCount, bool RequiresReview, bool ThresholdExceeded);

public sealed class LogProctoringEventsHandler(
    IRepository<ExamAttempt> examAttemptRepository,
    IRepository<Exam> examRepository,
    IRepository<ProctoringEvent> proctoringEventRepository,
    IUnitOfWork unitOfWork,
    IOptions<ProctoringOptions> proctoringOptions,
    ITenantContext tenantContext) : IHandler<LogProctoringEventsRequest, Result<LogProctoringEventsResponse>>
{
    public async Task<Result<LogProctoringEventsResponse>> HandleAsync(
        LogProctoringEventsRequest command, CancellationToken cancellationToken)
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
            return ProctoringErrors.ConsentRequired;
        }

        var exam = await examRepository.FindAsync(e => e.Id == attempt.ExamId, cancellationToken);

        if (exam is null)
        {
            return ExamErrors.AttemptNotFound(command.AttemptId);
        }

        var remainingCapacity = Math.Max(0, proctoringOptions.Value.MaxEventsPerAttempt - attempt.ViolationCount);
        var eventsToLog = command.Events.Take(remainingCapacity).ToList();

        var newEvents = eventsToLog
            .Select(e => new ProctoringEvent
            {
                Id = Guid.CreateVersion7(),
                ExamAttemptId = attempt.Id,
                Type = e.Type,
                OccurredOn = e.OccurredOn,
                Details = e.Details
            })
            .ToList();

        attempt.ViolationCount += newEvents.Count;

        var thresholdExceeded = false;

        if (!attempt.RequiresReview
            && exam.ViolationWarningThreshold is { } threshold
            && attempt.ViolationCount >= threshold)
        {
            attempt.RequiresReview = true;
            thresholdExceeded = true;
        }

        if (newEvents.Count > 0)
        {
            await proctoringEventRepository.AddRangeAsync(newEvents, cancellationToken);
        }

        await examAttemptRepository.UpdateAsync(attempt, cancellationToken);
        await unitOfWork.CommitAsync(cancellationToken);

        return Result.Success(
            new LogProctoringEventsResponse(attempt.ViolationCount, attempt.RequiresReview, thresholdExceeded));
    }
}
