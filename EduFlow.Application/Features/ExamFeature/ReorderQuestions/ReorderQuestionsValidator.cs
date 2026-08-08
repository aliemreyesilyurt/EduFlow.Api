using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.ReorderQuestions;

public sealed class ReorderQuestionsValidator : AbstractValidator<ReorderQuestionsRequest>
{
    public ReorderQuestionsValidator()
    {
        RuleFor(c => c.ExamId)
            .NotEmpty().WithMessage("ExamId is required");

        RuleFor(c => c.QuestionIds)
            .NotEmpty().WithMessage("At least one question id is required")
            .Must(ids => ids.Distinct().Count() == ids.Count)
            .WithMessage("Question ids must not contain duplicates");
    }
}
