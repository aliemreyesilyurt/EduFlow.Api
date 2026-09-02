using EduFlow.Domain.Abstractions.Errors;

namespace EduFlow.Application.Features.PointsFeature;

public static class PointsErrors
{
    public static Error RuleNotFound(Guid id) =>
        Error.NotFound("Points.RuleNotFound", $"The points rule with Id '{id}' was not found");

    public static readonly Error Forbidden =
        Error.Forbidden("Points.Forbidden", "You do not have permission to manage points rules");

    public static readonly Error TenantRequired =
        Error.Forbidden("Points.TenantRequired", "This account is not associated with a tenant, so it cannot manage points rules");
}
