using FluentValidation;

namespace EduFlow.Application.Features.ProctoringFeature.GetExamAttemptsForReview;

public sealed class GetExamAttemptsForReviewValidator : AbstractValidator<GetExamAttemptsForReviewRequest>
{
    public GetExamAttemptsForReviewValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");
    }
}
