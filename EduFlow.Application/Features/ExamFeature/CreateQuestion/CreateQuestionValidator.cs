using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.CreateQuestion;

public sealed class CreateQuestionValidator : AbstractValidator<CreateQuestionRequest>
{
    public CreateQuestionValidator()
    {
        RuleFor(c => c.ExamId)
            .NotEmpty().WithMessage("ExamId is required");

        RuleFor(c => c.Text)
            .NotEmpty().WithMessage("Text is required")
            .MaximumLength(2000).WithMessage("Text must not exceed 2000 characters");

        RuleFor(c => c.Points)
            .GreaterThan(0).WithMessage("Points must be positive");

        RuleFor(c => c.Options)
            .Must(o => o.Count >= 2).WithMessage("At least two options are required")
            .Must(o => o.Count(x => x.IsCorrect) == 1).WithMessage("Exactly one option must be marked correct");

        RuleForEach(c => c.Options).ChildRules(option =>
        {
            option.RuleFor(o => o.Text)
                .NotEmpty().WithMessage("Option text is required")
                .MaximumLength(500).WithMessage("Option text must not exceed 500 characters");
        });
    }
}
