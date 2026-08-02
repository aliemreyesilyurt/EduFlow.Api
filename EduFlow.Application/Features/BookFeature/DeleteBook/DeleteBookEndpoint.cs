using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.BookFeature.DeleteBook;

internal sealed class DeleteBookEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapDelete("books/{id:guid}", async (Guid id, IHandler<DeleteBookRequest, Result<DeleteBookResponse>> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new DeleteBookRequest(id), cancellationToken);
            return result.Match(
                onSuccess: () => Results.Ok(result.Value),
                onFailure: error => Results.NotFound(error));
        })
        .WithTags(ApiTags.Books)
        .Produces<DeleteBookResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
