using BuildingBlocks.SharedKernel;

namespace BuildingBlocks.UnitTests.SharedKernel;

public class ResultTests
{
    private static readonly Error NotFound = Error.NotFound("dish.not_found", "Dish 42 does not exist.");

    [Fact]
    public void Success_IsSuccess_WithoutError()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.IsFailure.ShouldBeFalse();
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Failure_IsFailure_WithTheError()
    {
        var result = Result.Failure(NotFound);

        result.IsFailure.ShouldBeTrue();
        result.Error.ShouldBe(NotFound);
    }

    [Fact]
    public void GenericSuccess_ExposesTheValue()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void GenericFailure_ReadingValue_Throws()
    {
        var result = Result<int>.Failure(NotFound);

        var exception = Should.Throw<InvalidOperationException>(() => result.Value);
        exception.Message.ShouldContain("dish.not_found");
    }

    [Fact]
    public void ImplicitConversion_FromValue_IsSuccess()
    {
        Result<string> result = "menu";

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe("menu");
    }

    [Fact]
    public void ImplicitConversion_FromError_IsFailure()
    {
        Result<string> generic = NotFound;
        Result plain = NotFound;

        generic.Error.ShouldBe(NotFound);
        plain.Error.ShouldBe(NotFound);
    }

    [Theory]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    [InlineData(ErrorType.Forbidden)]
    [InlineData(ErrorType.Unexpected)]
    public void ErrorFactories_SetTheType(ErrorType type)
    {
        Func<string, string, Error> factory = type switch
        {
            ErrorType.Validation => Error.Validation,
            ErrorType.NotFound => Error.NotFound,
            ErrorType.Conflict => Error.Conflict,
            ErrorType.Forbidden => Error.Forbidden,
            _ => Error.Unexpected,
        };

        var error = factory("code", "message");

        error.ShouldBe(new Error("code", "message", type));
    }
}
