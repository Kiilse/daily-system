namespace BuildingBlocks.SharedKernel;

/// <summary>Category of a business error. BuildingBlocks.Web maps each one to an HTTP status.</summary>
public enum ErrorType
{
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Unexpected,
}
