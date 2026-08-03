using EduFlow.Domain.Abstractions;
using EduFlow.Domain.Abstractions.Errors;

namespace EduFlow.Application.Extensions;

public static class ResultExtensions
{
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess()
            : onFailure(result.Error);
    }

    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Error, TOut> onFailure)
    {
        return result.IsSuccess
            ? onSuccess(result.Value)
            : onFailure(result.Error);
    }

    public static IResult ToHttpResult(this Error error) => error.Type switch
    {
        ErrorType.NotFound => Results.NotFound(error),
        ErrorType.Conflict => Results.Conflict(error),
        ErrorType.Unauthorized => Results.Unauthorized(),
        ErrorType.Forbidden => Results.Problem(detail: error.Description, statusCode: StatusCodes.Status403Forbidden),
        ErrorType.Validation => Results.BadRequest(error),
        _ => Results.BadRequest(error)
    };
}
