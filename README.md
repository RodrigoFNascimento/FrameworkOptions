# FrameworkOptions

A lightweight configuration loading and validation library for .NET Framework. It brings modern-style `IOptions<T>` to classic apps using simple, dependency-free code.

## Features

- **Lightweight & dependency-free** — ideal for .NET Framework projects.
- **Typed configuration loading** directly from appSettings in Web.config or App.config.
- **Environment variable support** — optionally load or override values from environment variables.
- **DataAnnotations validation** — supports [Required], [Range], [EmailAddress], etc.
- **Custom validation** — add your own predicate-based rules.
- **Tracks assigned properties** — detects missing or invalid configuration keys.
- **Immutable options model** — encourages safe and testable configuration patterns.
- **Test-friendly** — easy to mock or inject IOptions<T> in unit tests.

## Usage

Add the settings to the application's configuration:

```csharp
<configuration>
  <appSettings>
    <add key="Application.Name" value="Deep Thought" />
    <add key="Answer" value="42" />
  </appSettings>
</configuration>
```

Create a class that represents the settings:

```csharp
using System.ComponentModel.DataAnnotations;

public class AppSettings
{
    [Required]
    public string ApplicationName { get; set; }

    [Range(1, 100)]
    public int Answer { get; set; }
}
```
In your Web API startup (e.g., `Global.asax`), inject the options:

```csharp
// Global.asax
using FrameworkOptions;
using SimpleInjector;
using SimpleInjector.Integration.WebApi;

public class WebApiApplication : System.Web.HttpApplication
{
    protected void Application_Start()
    {
        // ...

        var container = new Container(); // Using SimpleInjector to handle DI as an example
        container.RegisterWebApiControllers(GlobalConfiguration.Configuration);

        var appSettings = OptionsLoader.Load<AppSettings>();
        var options = new Options<AppSettings>(appSettings)
            .ValidateDataAnnotations();

        container.RegisterInstance<IOptions<AppSettings>>(options);
        container.Verify();

        GlobalConfiguration.Configuration.DependencyResolver =
            new SimpleInjectorWebApiDependencyResolver(container);
    }
}
```

If your configuration has invalid or missing values, an exception will be thrown on startup with detailed error messages.

If you don't want to use data annotations, you can implement custom validations using lambda expressions:

```csharp
var options = new Options<AppSettings>(appSettings)
    .Validate(
        s => s.Answer > 0 && s.Answer <= 100,
        "Answer must be between 1 and 100."
    );
```

### Environment Variable Support

`OptionsLoader.Load<T>()` can optionally load configuration values from **environment variables** in addition to `appSettings`.

When `preferEnvironment` is set to `true` (the default), the loader will prioritize environment variables over `appSettings` values.
If an environment variable is not found, it will fall back to the value in `appSettings`.

If `preferEnvironment` is set to `false`, the order is reversed — `appSettings` are preferred, and environment variables are used only if the key is missing there.

| preferEnvironment  | Source Order              |
| ------------------ | ------------------------- |
| `true` *(default)* | Environment > appSettings |
| `false`            | appSettings > Environment |

### Testing Config-Dependent Code

`IOptions<T>` makes your code easier to test by decoupling it from real configuration files.

For example, imagine a service that depends on your configuration:

```csharp
public class GreetingService
{
    private readonly AppSettings _settings;

    public GreetingService(IOptions<AppSettings> options)
    {
        _settings = options.Value;
    }

    public string Greet(string name)
    {
        return $"Hi, {name}! I'm {_settings.ApplicationName}.";
    }
}
```

In a unit test, you can easily provide fake configuration using NSubstitute or a real `Options<T>` instance — no `Web.config` required.

```csharp
using FrameworkOptions;
using NSubstitute;
using Xunit;

public class GreetingServiceTests
{
    [Fact]
    public void Greet_WhenBooleanSettingIsTrue_ShouldIncludeSettingValue()
    {
        // Arrange
        var name = "Arthur";
        var applicationName = "Deep Thought";
        var fakeOptions = Substitute.For<IOptions<AppSettings>>();
        fakeOptions.Value.Returns(new AppSettings
        {
            ApplicationName = applicationName,
            IntegerSetting = 42
        });

        var service = new GreetingService(fakeOptions);

        // Act
        var result = service.Greet(name);

        // Assert
        Assert.Equal($"Hi, {name}! I'm {applicationName}", result);
    }
}
```

This approach allows your services to remain pure and testable, independent of environment or configuration files.