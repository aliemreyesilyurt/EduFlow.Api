using FluentValidation;

namespace EduFlow.Application.Features.ExamFeature.GetMyExamAttempts;

public sealed class GetMyExamAttemptsValidator : AbstractValidator<GetMyExamAttemptsRequest>
{
    public GetMyExamAttemptsValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");
    }
}
