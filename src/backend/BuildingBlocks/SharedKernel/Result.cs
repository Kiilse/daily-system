using System.Diagnostics.CodeAnalysis;

namespace BuildingBlocks.SharedKernel;

/// <summary>Outcome of an operation that can fail for a business reason: success, or failure with an <see cref="SharedKernel.Error"/>.</summary>
public class Result
{
    protected Result(Error? error) => Error = error;

    /// <summary>Null when the operation succeeded.</summary>
    public Error? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    [MemberNotNullWhen(true, nameof(Error))]
    public bool IsFailure => !IsSuccess;

    public static Result Success() => new(null);

    public static Result Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(error);
    }

    public static implicit operator Result(Error error) => Failure(error);
}

/// <summary>Outcome of an operation that returns a <typeparamref name="T"/> when it succeeds.</summary>
[SuppressMessage("Design", "CA1000:Do not declare static members on generic types",
    Justification = "Result<T>.Success / Failure factories are the intended construction API.")]
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value)
        : base(null) => _value = value;

    private Result(Error error)
        : base(error) => _value = default;

    /// <summary>The value of a success. Reading it on a failure is a programming error and throws.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException($"Cannot read the value of a failed result ({Error.Code}).");

    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return new(error);
    }

#pragma warning disable CA2225 // Success/Failure are the named alternatives to these operators
    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
#pragma warning restore CA2225
}
