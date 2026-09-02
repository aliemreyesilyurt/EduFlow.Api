using FluentValidation;

namespace EduFlow.Application.Features.ProctoringFeature.GetProctoringReport;

public sealed class GetProctoringReportValidator : AbstractValidator<GetProctoringReportRequest>
{
    public GetProctoringReportValidator()
    {
        RuleFor(c => c.AttemptId)
            .NotEmpty().WithMessage("AttemptId is required");
    }
}
