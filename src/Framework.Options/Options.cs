using System;

namespace FrameworkOptions
{
    /// <summary>
    /// Default implementation of <see cref="IOptions{T}"/>.
    /// Holds a single immutable instance of the configuration object.
    /// </summary>
    /// <typeparam name="T">The type of the options class.</typeparam>
    public class Options<T> : IOptions<T>
    {
        public T Value { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Options{T}"/> class.
        /// </summary>
        /// <param name="value">The configuration object to wrap.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="value"/> is null.</exception>
        public Options(T value)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            Value = value;
        }
    }
}
