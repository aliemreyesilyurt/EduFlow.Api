using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.GetExamForTaking;

public sealed class GetExamForTakingValidator : AbstractValidator<GetExamForTakingRequest>
{
    public GetExamForTakingValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");
    }
}
