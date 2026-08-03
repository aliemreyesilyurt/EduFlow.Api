using EduFlow.Domain.Abstractions.Errors;

namespace EduFlow.Application.Features.CommentFeature;

public static class CommentErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Comments.NotFound", $"The comment with Id '{id}' was not found");

    public static readonly Error Forbidden =
        Error.Forbidden("Comments.Forbidden", "You do not have permission to moderate this comment");
}
