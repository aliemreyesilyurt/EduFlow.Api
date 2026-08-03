namespace EduFlow.Application.Features.RatingFeature;

public sealed record RatingResponse(Guid Id, Guid CourseId, Guid StudentId, int Value, DateTime CreatedOn, DateTime? UpdatedOn);
