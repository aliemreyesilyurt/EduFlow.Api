using EduFlow.Application.Abstractions;
using EduFlow.Application.Constants;
using EduFlow.Application.Extensions;
using EduFlow.Domain.Abstractions;

namespace EduFlow.Application.Features.BookFeature.GetBookById;

internal sealed class GetBookByIdEndpoint : IApiEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("books/{id:guid}", async (Guid id, IHandler<GetBookByIdRequest, Result<GetBookByIdResponse>> handler, CancellationToken cancellationToken) =>
        {
            var result = await handler.HandleAsync(new GetBookByIdRequest(id), cancellationToken);
            return result.Match(
                onSuccess: () => Results.Ok(result.Value),
                onFailure: error => Results.NotFound(error));
        })
        .WithTags(ApiTags.Books)
        .Produces<GetBookByIdResponse>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status404NotFound);
    }
}
