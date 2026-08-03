using EduFlow.Domain.Abstractions.Errors;

namespace EduFlow.Application.Features.RatingFeature;

public static class RatingErrors
{
    public static readonly Error NotFound =
        Error.NotFound("Ratings.NotFound", "You have not rated this course");
}
