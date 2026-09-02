namespace EduFlow.Application.Features.PointsFeature;

public sealed record PointsRuleResponse(
    Guid Id,
    string Title,
    string? Description,
    int PointsCost,
    bool IsActive);
