using Blog.Shared;

namespace Blog.UnitTests.Shared;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult_WithErrorNone()
    {
        // Act
        var result = Result.Success();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void Failure_ShouldCreateFailedResult_WithGivenError()
    {
        // Arrange
        var error = new Error("Test.Error", "Test message");

        // Act
        var result = Result.Failure(error);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.True(result.IsFailure);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void SuccessGeneric_ShouldStoreValueCorrectly()
    {
        // Arrange
        var value = "Test Value";

        // Act
        var result = Result.Success(value);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(value, result.Value);
    }

    [Fact]
    public void FailureGeneric_ShouldThrowOnAccessingValue()
    {
        // Arrange
        var error = new Error("Test.Error", "Test message");
        var result = Result.Failure<string>(error);

        // Act & Assert
        Assert.False(result.IsSuccess);
        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}
