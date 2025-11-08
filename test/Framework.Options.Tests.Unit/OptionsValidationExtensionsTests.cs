using FluentAssertions;
using System.ComponentModel.DataAnnotations;

namespace FrameworkOptions.Tests.Unit;

public sealed class OptionsValidationExtensionsTests
{
    [Fact]
    public void Validate_WhenPredicateFails_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var settings = new TestSettings
        {
            RequiredInt = 1,
            RequiredBool = true,
            RequiredString = "abc"
        };
        var options = new Options<TestSettings>(settings);

        // Act
        Action act = () => options.Validate(o => o.RequiredInt > 10, "Must be greater than 10");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Must be greater than 10*");
    }

    [Fact]
    public void Validate_WhenPredicatePasses_ShouldReturnSameInstance()
    {
        // Arrange
        var settings = new TestSettings
        {
            RequiredInt = 10,
            RequiredBool = true,
            RequiredString = "abc"
        };
        var options = new Options<TestSettings>(settings);

        // Act
        var result = options.Validate(o => o.RequiredInt == 10, "Invalid");

        // Assert
        result.Should().BeSameAs(options);
    }

    [Fact]
    public void ValidateDataAnnotations_WhenRequiredFieldsMissing_ShouldThrowValidationException()
    {
        // Arrange
        var settings = new TestSettings();
        var options = new Options<TestSettings>(settings);
        OptionsLoader.Load<TestSettings>();

        // Act
        Action act = () => options.ValidateDataAnnotations();

        // Assert
        act.Should().Throw<ValidationException>()
            .WithMessage("*is required*");
    }
}