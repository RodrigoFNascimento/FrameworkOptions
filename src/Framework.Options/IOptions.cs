namespace FrameworkOptions
{
    /// <summary>
    /// Represents a wrapper for a configured options instance.
    /// Provides a strongly-typed way to access configuration values.
    /// </summary>
    /// <typeparam name="T">The type of the options class.</typeparam>
    public interface IOptions<out T>
    {
        T Value { get; }
    }
}
