using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.DeleteQuestion;

public sealed class DeleteQuestionValidator : AbstractValidator<DeleteQuestionRequest>
{
    public DeleteQuestionValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
