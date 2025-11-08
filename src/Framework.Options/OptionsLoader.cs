using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Globalization;
using System.Reflection;

namespace FrameworkOptions
{
    /// <summary>
    /// Provides helper methods to load configuration objects from XML-based
    /// appSettings sections in Web.config or App.config.
    /// </summary>
    public static class OptionsLoader
    {
        private static readonly Dictionary<object, HashSet<string>> _assignedProperties =
            new Dictionary<object, HashSet<string>>();

        /// <summary>
        /// Creates and populates a new instance of <typeparamref name="T"/> using
        /// configuration values from both <c>appSettings</c> (in Web.config or App.config)
        /// and environment variables.
        /// </summary>
        /// <typeparam name="T">The type of the configuration class to load.</typeparam>
        /// <param name="preferEnvironment">
        /// Determines which source has priority when both an environment variable and
        /// an appSettings key exist for the same property.
        /// <para>
        /// When <see langword="true"/> (default), environment variables override values from appSettings.
        /// When <see langword="false"/>, appSettings values take precedence.
        /// </para>
        /// </param>
        /// <returns>
        /// A new instance of <typeparamref name="T"/> populated with values from configuration
        /// and environment variables.
        /// </returns>
        /// <remarks>
        /// <para>
        /// The method matches each public writable property name of <typeparamref name="T"/>
        /// (case-sensitive) against both <c>appSettings</c> keys and environment variable names.
        /// </para>
        /// <para>
        /// Values are converted to the target property type using <see cref="Convert.ChangeType(object, Type)"/>.
        /// Boolean and enumeration types are handled automatically. Invalid or unconvertible values
        /// are ignored, similar to the behavior of <c>ConfigurationBinder</c> in modern .NET.
        /// </para>
        /// <para>
        /// This method also tracks which properties were successfully assigned, enabling
        /// <see cref="OptionsValidationExtensions.ValidateDataAnnotations{T}(IOptions{T})"/> to detect
        /// uninitialized <see cref="RequiredAttribute"/> fields.
        /// </para>
        /// </remarks>
        public static T Load<T>(bool preferEnvironment = true) where T : new()
        {
            var instance = new T();
            var assigned = new HashSet<string>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var prop in props)
            {
                if (!prop.CanWrite) continue;

                var environmentVariable = Environment.GetEnvironmentVariable(prop.Name);
                var appSettingsVariable = ConfigurationManager.AppSettings[prop.Name];

                var rawValue = preferEnvironment
                    ? environmentVariable ?? appSettingsVariable
                    : appSettingsVariable ?? environmentVariable;

                if (string.IsNullOrWhiteSpace(rawValue))
                    continue;

                try
                {
                    var value = ConvertValue(rawValue, prop.PropertyType);
                    prop.SetValue(instance, value);
                    assigned.Add(prop.Name);
                }
                catch
                {
                    // Ignore invalid conversions, just like ConfigurationBinder
                }
            }

            _assignedProperties[instance] = assigned;
            return instance;
        }

        /// <summary>
        /// Determines whether a property was successfully assigned a value from configuration.
        /// </summary>
        public static bool WasAssigned<T>(T instance, string propertyName) =>
            _assignedProperties.TryGetValue(instance, out var set) && set.Contains(propertyName);

        private static object ConvertValue(string raw, Type type)
        {
            if (type == typeof(bool))
            {
                if (bool.TryParse(raw, out var b)) return b;
                throw new FormatException();
            }

            if (type.IsEnum)
                return Enum.Parse(type, raw, ignoreCase: true);

            return Convert.ChangeType(raw, type, CultureInfo.InvariantCulture);
        }
    }
}
