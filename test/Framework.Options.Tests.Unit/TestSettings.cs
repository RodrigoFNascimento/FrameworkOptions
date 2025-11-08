using System.ComponentModel.DataAnnotations;

namespace FrameworkOptions.Tests.Unit;
internal sealed class TestSettings
{
    [Required]
    public int RequiredInt { get; set; }

    [Required]
    public bool RequiredBool { get; set; }

    [Required]
    public string RequiredString { get; set; } = string.Empty;

    public TestEnum EnumSetting { get; set; }
}

internal enum TestEnum
{
    Value
}
