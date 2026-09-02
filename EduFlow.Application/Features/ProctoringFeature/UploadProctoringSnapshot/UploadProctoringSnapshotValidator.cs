using EduFlow.Application.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace EduFlow.Application.Features.ProctoringFeature.UploadProctoringSnapshot;

public sealed class UploadProctoringSnapshotValidator : AbstractValidator<UploadProctoringSnapshotRequest>
{
    public UploadProctoringSnapshotValidator(IOptions<ProctoringOptions> proctoringOptions)
    {
        RuleFor(c => c.AttemptId)
            .NotEmpty().WithMessage("AttemptId is required");

        RuleFor(c => c.File)
            .NotNull().WithMessage("File is required");

        RuleFor(c => c.File)
            .Must(f => f.Length > 0).WithMessage("File must not be empty")
            .Must(f => f.Length <= proctoringOptions.Value.MaxSnapshotSizeBytes)
                .WithMessage($"File must not exceed {proctoringOptions.Value.MaxSnapshotSizeBytes} bytes")
            .Must(f => proctoringOptions.Value.AllowedSnapshotContentTypes.Contains(f.ContentType))
                .WithMessage("File content type is not allowed")
            .When(c => c.File is not null);
    }
}
