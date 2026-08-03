using FluentValidation;

namespace EduFlow.Application.Features.RatingFeature.DeleteRating;

public sealed class DeleteRatingValidator : AbstractValidator<DeleteRatingRequest>
{
    public DeleteRatingValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");
    }
}
