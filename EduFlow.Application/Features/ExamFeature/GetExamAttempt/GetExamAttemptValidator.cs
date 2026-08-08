using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.GetExamAttempt;

public sealed class GetExamAttemptValidator : AbstractValidator<GetExamAttemptRequest>
{
    public GetExamAttemptValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
