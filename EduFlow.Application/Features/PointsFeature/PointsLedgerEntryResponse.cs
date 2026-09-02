using EduFlow.Domain.Enums;

namespace EduFlow.Application.Features.PointsFeature;

public sealed record PointsLedgerEntryResponse(
    Guid Id,
    int Amount,
    int BalanceAfter,
    PointsReason Reason,
    string? Description,
    DateTime CreatedOn);
