using System;
using System.Configuration;

namespace ProcessStarter
{
    /// <summary>
    /// Helper class for service configuration
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2211:NonConstantFieldsShouldNotBeVisible", Justification = "Public constant fields are fine here.")]
    public static class ConfigHelper
    {
        static ConfigHelper()
        {
            CommandName = ReadConfig("CommandName", "");
        }

        /// <summary>
        /// Reads parameter name from configuration
        /// </summary>
        /// <param name="parameterName">name of the parameter</param>
        /// <param name="defaultValue">default value is config value missing</param>
        /// <returns>Parameter value or missing config info</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to catch and log all exceptions")]
        public static string ReadConfig(string parameterName, string defaultValue)
        {
            string result;
            try
            {
                result = ConfigurationManager.AppSettings[parameterName] ?? defaultValue;
            }
            catch (Exception)
            {
                result = defaultValue;
            }

            return result;
        }

        public const string MissingConfigValue = "missing config value";

        /// <summary>
        /// name of the executable to run
        /// </summary>
        public static readonly string CommandName;

    }
}