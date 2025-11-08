using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace FrameworkOptions
{
    /// <summary>
    /// Provides extensions for validating options instances,
    /// including DataAnnotations-based validation.
    /// </summary>
    public static class OptionsValidationExtensions
    {
        /// <summary>
        /// Validates the properties of the options object using
        /// <see cref="ValidationAttribute"/> attributes such as
        /// <see cref="RequiredAttribute"/>, <see cref="RangeAttribute"/>,
        /// and <see cref="EmailAddressAttribute"/>.
        /// </summary>
        /// <typeparam name="T">The type of the options being validated.</typeparam>
        /// <param name="options">The options instance to validate.</param>
        /// <returns>The validated <paramref name="options"/> instance.</returns>
        /// <exception cref="ValidationException">
        /// Thrown when one or more validation attributes fail.
        /// </exception>
        public static IOptions<T> ValidateDataAnnotations<T>(this IOptions<T> options)
        {
            var value = options.Value;
            var context = new ValidationContext(value);
            var results = new List<ValidationResult>();

            // Run built-in DataAnnotations validation first
            Validator.TryValidateObject(value, context, results, validateAllProperties: true);

            // Then check missing required assignments
            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.GetCustomAttribute<RequiredAttribute>() == null)
                    continue;

                if (!OptionsLoader.WasAssigned(value, prop.Name))
                    results.Add(new ValidationResult($"{prop.Name} is required."));
            }

            if (results.Count > 0)
            {
                var message = string.Join("; ", results.Select(r => r.ErrorMessage));
                throw new ValidationException($"Configuration validation failed for {typeof(T).Name}: {message}");
            }

            return options;
        }

        /// <summary>
        /// Validates the options instance using a custom predicate.
        /// </summary>
        /// <typeparam name="T">The type of the options being validated.</typeparam>
        /// <param name="options">The options instance to validate.</param>
        /// <param name="validator">A predicate function that returns true if valid.</param>
        /// <param name="message">The error message to include if validation fails.</param>
        /// <returns>The validated <paramref name="options"/> instance.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when <paramref name="validator"/> returns false.
        /// </exception> 
        public static IOptions<T> Validate<T>(
            this IOptions<T> options,
            Func<T, bool> validator,
            string message)
        {
            if (!validator(options.Value))
                throw new InvalidOperationException($"Options validation failed: {message}");
            return options;
        }
    }
}
