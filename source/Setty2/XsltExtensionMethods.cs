using System;
using System.Configuration;
using Setty2.Utils;

namespace Setty2
{
    public class XsltExtensionMethods
    {
        /// <summary>
        /// Key 
        /// </summary>
        private readonly KeyValueConfigurationCollection _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public XsltExtensionMethods(KeyValueConfigurationCollection settings)
        {
            _settings = settings;
        }

        /// <summary>
        /// Returns value of application settings property or empty string if not found
        /// </summary>
        public String Value(String key)
        {
            var element = _settings[key];

            if (element == null)
                return String.Empty;

            return element.Value;
        }

        /// <summary>
        /// Returns environment variable of empty string if not found
        /// </summary>
        public String Environment(String key)
        {
            return System.Environment.GetEnvironmentVariable(key) ?? String.Empty;
        }

        /// <summary>
        /// Returns 
        /// </summary>
        public String ApplicationSettings()
        {
            return SettingsHelper.GetApplicationSettingsXmlString(_settings);
        }
    }
}