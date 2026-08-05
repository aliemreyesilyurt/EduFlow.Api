using EduFlow.Application.Options;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace EduFlow.Application.Features.StepFeature.UploadStepContent;

public sealed class UploadStepContentValidator : AbstractValidator<UploadStepContentRequest>
{
    public UploadStepContentValidator(IOptions<StorageOptions> storageOptions)
    {
        RuleFor(c => c.StepId)
            .NotEmpty().WithMessage("StepId is required");

        RuleFor(c => c.File)
            .NotNull().WithMessage("File is required");

        RuleFor(c => c.File)
            .Must(f => f.Length > 0).WithMessage("File must not be empty")
            .Must(f => f.Length <= storageOptions.Value.MaxFileSizeBytes)
                .WithMessage($"File must not exceed {storageOptions.Value.MaxFileSizeBytes} bytes")
            .Must(f => StepContentFileTypes.IsAllowed(Path.GetExtension(f.FileName)))
                .WithMessage("File extension is not allowed")
            .When(c => c.File is not null);
    }
}
