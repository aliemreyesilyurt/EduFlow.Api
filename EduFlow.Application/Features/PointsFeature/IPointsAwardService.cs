namespace EduFlow.Application.Features.PointsFeature;

using EduFlow.Domain.Entities;

/// <summary>
/// Shared by SubmitExamAttemptHandler and ReviewExamAttemptHandler — both can be the moment an
/// attempt first qualifies for its exam's reward points, and the award logic (find-or-create
/// wallet, ledger entry, idempotency flag) is identical either way.
/// </summary>
public interface IPointsAwardService
{
    /// <summary>
    /// Credits <paramref name="exam"/>'s RewardPoints to <paramref name="attempt"/>'s student and
    /// marks <see cref="ExamAttempt.PointsAwarded"/>. Does not call IUnitOfWork.CommitAsync — the
    /// caller's own single commit for the request persists this alongside its other changes.
    /// </summary>
    Task AwardExamPointsAsync(ExamAttempt attempt, Exam exam, CancellationToken cancellationToken);
}
