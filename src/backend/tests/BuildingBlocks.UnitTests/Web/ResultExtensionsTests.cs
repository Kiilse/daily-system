using BuildingBlocks.SharedKernel;
using BuildingBlocks.Web;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BuildingBlocks.UnitTests.Web;

public class ResultExtensionsTests
{
    [Theory]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Conflict, 409)]
    [InlineData(ErrorType.Forbidden, 403)]
    [InlineData(ErrorType.Unexpected, 500)]
    public void ToProblemDetails_MapsTheErrorTypeToItsStatus(ErrorType type, int status)
    {
        var problem = new Error("menu.code", "Something went wrong.", type).ToProblemDetails();

        problem.Status.ShouldBe(status);
        problem.Title.ShouldNotBeNullOrWhiteSpace();
        problem.Type.ShouldStartWith("https://");
    }

    [Fact]
    public void ToProblemDetails_CarriesMessageAndCode()
    {
        var problem = Error.Conflict("dish.duplicate", "A dish with this name already exists.").ToProblemDetails();

        problem.Detail.ShouldBe("A dish with this name already exists.");
        problem.Extensions["code"].ShouldBe("dish.duplicate");
    }

    [Fact]
    public void ToProblemDetails_UnexpectedError_HidesTheInternalMessage()
    {
        var problem = Error.Unexpected("db.timeout", "Npgsql timeout on host 10.0.0.4").ToProblemDetails();

        problem.Detail.ShouldNotBeNull().ShouldNotContain("10.0.0.4");
    }

    [Fact]
    public void ToHttpResult_Success_ReturnsOkWithTheValue()
    {
        var result = Result<string>.Success("menu").ToHttpResult();

        var ok = result.ShouldBeOfType<Ok<string>>();
        ok.Value.ShouldBe("menu");
    }

    [Fact]
    public void ToHttpResult_Failure_ReturnsProblemDetails()
    {
        var result = Result<string>.Failure(Error.NotFound("dish.not_found", "Dish 42 does not exist.")).ToHttpResult();

        var problem = result.ShouldBeOfType<ProblemHttpResult>();
        problem.StatusCode.ShouldBe(404);
    }
}
