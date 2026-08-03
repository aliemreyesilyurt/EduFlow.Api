using EduFlow.Domain.Abstractions.Errors;

namespace EduFlow.Application.Features.StepFeature;

public static class StepErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Steps.NotFound", $"The step with Id '{id}' was not found");

    public static readonly Error Forbidden =
        Error.Forbidden("Steps.Forbidden", "You do not have permission to manage this course's steps");

    public static readonly Error ReorderMismatch =
        Error.Validation("Steps.ReorderMismatch", "The provided step ids must exactly match the course's current steps");
}
