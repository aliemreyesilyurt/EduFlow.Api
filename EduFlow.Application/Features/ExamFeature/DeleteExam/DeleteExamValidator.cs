using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.DeleteExam;

public sealed class DeleteExamValidator : AbstractValidator<DeleteExamRequest>
{
    public DeleteExamValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
