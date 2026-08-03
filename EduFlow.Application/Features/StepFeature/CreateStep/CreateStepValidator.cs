using EduFlow.Domain.Enums;
using FluentValidation;

namespace EduFlow.Application.Features.StepFeature.CreateStep;

public sealed class CreateStepValidator : AbstractValidator<CreateStepRequest>
{
    public CreateStepValidator()
    {
        RuleFor(c => c.CourseId)
            .NotEmpty().WithMessage("CourseId is required");

        RuleFor(c => c.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters");

        RuleFor(c => c.ContentType)
            .IsInEnum().WithMessage("ContentType is invalid");

        RuleFor(c => c.ContentUrl)
            .NotEmpty().WithMessage("ContentUrl is required for video/document steps")
            .When(c => c.ContentType is StepContentType.Video or StepContentType.Document);

        RuleFor(c => c.TextContent)
            .NotEmpty().WithMessage("TextContent is required for text steps")
            .When(c => c.ContentType == StepContentType.Text);
    }
}
