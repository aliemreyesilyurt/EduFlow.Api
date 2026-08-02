using FluentValidation;

namespace EduFlow.Application.Features.BookFeature.DeleteBook;

public class DeleteBookValidator : AbstractValidator<DeleteBookRequest>
{
    public DeleteBookValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Book Id is required");
    }
}
