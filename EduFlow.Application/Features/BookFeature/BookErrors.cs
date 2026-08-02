using EduFlow.Domain.Abstractions.Errors;

namespace EduFlow.Application.Features.BookFeature;

public static class BookErrors
{
    public static Error NotFound(Guid id) =>
        Error.NotFound("Books.NotFound", $"The Book with Id '{id}' was not found");
}
