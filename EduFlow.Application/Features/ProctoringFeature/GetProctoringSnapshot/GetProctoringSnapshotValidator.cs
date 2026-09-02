using FluentValidation;

namespace EduFlow.Application.Features.ProctoringFeature.GetProctoringSnapshot;

public sealed class GetProctoringSnapshotValidator : AbstractValidator<GetProctoringSnapshotRequest>
{
    public GetProctoringSnapshotValidator()
    {
        RuleFor(c => c.AttemptId)
            .NotEmpty().WithMessage("AttemptId is required");

        RuleFor(c => c.SnapshotId)
            .NotEmpty().WithMessage("SnapshotId is required");
    }
}
