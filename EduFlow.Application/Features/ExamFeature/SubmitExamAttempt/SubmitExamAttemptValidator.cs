using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.SubmitExamAttempt;

public sealed class SubmitExamAttemptValidator : AbstractValidator<SubmitExamAttemptRequest>
{
    public SubmitExamAttemptValidator()
    {
        RuleFor(c => c.AttemptId)
            .NotEmpty().WithMessage("AttemptId is required");

        RuleForEach(c => c.Answers).ChildRules(answer =>
        {
            answer.RuleFor(a => a.QuestionId)
                .NotEmpty().WithMessage("QuestionId is required");
        });
    }
}
