namespace EduFlow.Application.Features.CommentFeature;

public sealed record CommentResponse(
    Guid Id,
    Guid CourseId,
    Guid? StepId,
    Guid AuthorId,
    string AuthorName,
    string Content,
    bool IsHidden,
    DateTime CreatedOn);
