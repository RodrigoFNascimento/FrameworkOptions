using FluentAssertions;
using System.Configuration;

namespace FrameworkOptions.Tests.Unit;
public sealed class OptionsLoaderTests
{
    public OptionsLoaderTests()
    {
        ConfigurationManager.AppSettings[nameof(TestSettings.RequiredInt)] = null;
        ConfigurationManager.AppSettings[nameof(TestSettings.RequiredBool)] = null;
        ConfigurationManager.AppSettings[nameof(TestSettings.RequiredString)] = null;
        ConfigurationManager.AppSettings[nameof(TestSettings.EnumSetting)] = null;
    }

    [Fact]
    public void Load_WhenValidValuesProvided_ShouldPopulateProperties()
    {
        // Arrange
        var expectedInt = 24;
        var expectedBool = true;
        var expectedString = "test";
        var expectedEnum = TestEnum.Value;
        ConfigurationManager.AppSettings[nameof(TestSettings.RequiredInt)] = expectedInt.ToString();
        ConfigurationManager.AppSettings[nameof(TestSettings.RequiredBool)] = expectedBool.ToString();
        ConfigurationManager.AppSettings[nameof(TestSettings.RequiredString)] = expectedString;
        ConfigurationManager.AppSettings[nameof(TestSettings.EnumSetting)] = expectedEnum.ToString();

        // Act
        var result = OptionsLoader.Load<TestSettings>();

        // Assert
        result.RequiredInt.Should().Be(expectedInt);
        result.RequiredBool.Should().Be(expectedBool);
        result.RequiredString.Should().Be(expectedString);
        result.EnumSetting.Should().Be(expectedEnum);
    }
    
    [Fact]
    public void Load_WhenAppSettingsPreferred_ShouldPreferAppSettings()
    {
        // Arrange
        var expectedInt = 24;
        ConfigurationManager.AppSettings[nameof(TestSettings.RequiredInt)] = expectedInt.ToString();
        Environment.SetEnvironmentVariable(nameof(TestSettings.RequiredInt), "1");

        // Act
        var result = OptionsLoader.Load<TestSettings>(false);

        // Assert
        result.RequiredInt.Should().Be(expectedInt);
    }

    [Fact]
    public void Load_WhenInvalidConversion_ShouldSkipProperty()
    {
        // Arrange
        ConfigurationManager.AppSettings[nameof(TestSettings.RequiredInt)] = "invalid";
        ConfigurationManager.AppSettings[nameof(TestSettings.RequiredBool)] = "test";

        // Act
        var result = OptionsLoader.Load<TestSettings>();

        // Assert
        result.RequiredInt.Should().Be(default);
        result.RequiredBool.Should().Be(default);
    }

    [Fact]
    public void WasAssigned_WhenPropertyWasSet_ShouldReturnTrue()
    {
        // Arrange
        ConfigurationManager.AppSettings[nameof(TestSettings.RequiredInt)] = "42";
        var instance = OptionsLoader.Load<TestSettings>();

        // Act
        var result = OptionsLoader.WasAssigned(instance, nameof(TestSettings.RequiredInt));

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void WasAssigned_WhenPropertyWasNotSet_ShouldReturnFalse()
    {
        // Arrange
        ConfigurationManager.AppSettings[nameof(TestSettings.RequiredInt)] = null;
        var instance = OptionsLoader.Load<TestSettings>();

        // Act
        var result = OptionsLoader.WasAssigned(instance, nameof(TestSettings.RequiredInt));

        // Assert
        result.Should().BeFalse();
    }
}
