namespace BuildingBlocks.SharedKernel;

/// <summary>
/// A business error returned instead of thrown (ADR-007).
/// <paramref name="Code"/> is stable and machine-readable ("dish.not_found"), <paramref name="Message"/> is for humans.
/// </summary>
public sealed record Error(string Code, string Message, ErrorType Type)
{
    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);

    public static Error Unexpected(string code, string message) => new(code, message, ErrorType.Unexpected);
}
