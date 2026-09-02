namespace EduFlow.Application.Features.PointsFeature.GetMyPointsLedger;

using EduFlow.Application.Abstractions;
using EduFlow.Application.Abstractions.Data;
using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Entities;

public sealed record GetMyPointsLedgerRequest;

public sealed record GetMyPointsLedgerResponse(IReadOnlyList<PointsLedgerEntryResponse> Entries);

public sealed class GetMyPointsLedgerHandler(
    IRepository<PointsLedgerEntry> ledgerRepository,
    ITenantContext tenantContext) : IHandler<GetMyPointsLedgerRequest, Result<GetMyPointsLedgerResponse>>
{
    public async Task<Result<GetMyPointsLedgerResponse>> HandleAsync(GetMyPointsLedgerRequest command, CancellationToken cancellationToken)
    {
        if (tenantContext.UserId is not { } studentId)
        {
            return Result.Success(new GetMyPointsLedgerResponse([]));
        }

        var entries = (await ledgerRepository.GetAllAsync(cancellationToken))
            .Where(e => e.StudentId == studentId)
            .OrderByDescending(e => e.CreatedOn)
            .Select(e => new PointsLedgerEntryResponse(e.Id, e.Amount, e.BalanceAfter, e.Reason, e.Description, e.CreatedOn))
            .ToList();

        return Result.Success(new GetMyPointsLedgerResponse(entries));
    }
}
