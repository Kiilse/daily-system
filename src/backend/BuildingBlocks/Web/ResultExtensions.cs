using BuildingBlocks.SharedKernel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BuildingBlocks.Web;

/// <summary>Turns <see cref="Result"/> values into HTTP responses: 200 on success, RFC 9457 Problem Details on failure.</summary>
public static class ResultExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : TypedResults.Problem(result.Error.ToProblemDetails());
    }

    public static ProblemDetails ToProblemDetails(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        var (status, title, type) = Describe(error.Type);

        return new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = type,
            // An unexpected error may carry infrastructure details: never send them to the client
            Detail = error.Type == ErrorType.Unexpected ? "An unexpected error occurred." : error.Message,
            Extensions = { ["code"] = error.Code },
        };
    }

    private static (int Status, string Title, string Type) Describe(ErrorType type) => type switch
    {
        ErrorType.Validation => (StatusCodes.Status400BadRequest, "Bad Request", "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.1"),
        ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Forbidden", "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.4"),
        ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found", "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.5"),
        ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict", "https://www.rfc-editor.org/rfc/rfc9110#section-15.5.10"),
        _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "https://www.rfc-editor.org/rfc/rfc9110#section-15.6.1"),
    };
}
