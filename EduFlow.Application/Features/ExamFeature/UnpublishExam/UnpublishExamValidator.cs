using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.UnpublishExam;

public sealed class UnpublishExamValidator : AbstractValidator<UnpublishExamRequest>
{
    public UnpublishExamValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
