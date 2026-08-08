using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.PublishExam;

public sealed class PublishExamValidator : AbstractValidator<PublishExamRequest>
{
    public PublishExamValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
