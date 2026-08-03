using FluentValidation;

namespace EduFlow.Application.Features.RatingFeature.GetMyRating;

public sealed class GetMyRatingValidator : AbstractValidator<GetMyRatingRequest>
{
    public GetMyRatingValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");
    }
}
