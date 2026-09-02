namespace EduFlow.Application.Features.PointsFeature;

using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Entities;
using EduFlow.Domain.Enums;

public sealed class PointsAwardService(
    IRepository<PointsWallet> walletRepository,
    IRepository<PointsLedgerEntry> ledgerRepository) : IPointsAwardService
{
    public async Task AwardExamPointsAsync(ExamAttempt attempt, Exam exam, CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.FindAsync(w => w.StudentId == attempt.StudentId, cancellationToken);

        if (wallet is null)
        {
            // AddAsync tracks the entity as Added; calling UpdateAsync on it afterwards would flip
            // that to Modified and EF would emit an UPDATE for a row that doesn't exist yet (0 rows
            // affected -> DbUpdateConcurrencyException). So the balance has to be set before adding.
            wallet = new PointsWallet { Id = Guid.CreateVersion7(), StudentId = attempt.StudentId, Balance = exam.RewardPoints };
            await walletRepository.AddAsync(wallet, cancellationToken);
        }
        else
        {
            wallet.Balance += exam.RewardPoints;
            await walletRepository.UpdateAsync(wallet, cancellationToken);
        }

        await ledgerRepository.AddAsync(new PointsLedgerEntry
        {
            Id = Guid.CreateVersion7(),
            StudentId = attempt.StudentId,
            Amount = exam.RewardPoints,
            BalanceAfter = wallet.Balance,
            Reason = PointsReason.ExamPassed,
            Description = $"'{exam.Title}' sınavı geçildi",
            ExamAttemptId = attempt.Id
        }, cancellationToken);

        attempt.PointsAwarded = true;
    }
}
