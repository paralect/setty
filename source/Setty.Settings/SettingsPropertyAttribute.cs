using System;

namespace Setty.Settings
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SettingsPropertyAttribute : Attribute
    {
        /// <summary>
        /// Property key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Map property to the key of application settings
        /// </summary>
        public SettingsPropertyAttribute(String key)
        {
            Key = key;
        }

        /// <summary>
        /// Map property of not primitive type
        /// </summary>
        public SettingsPropertyAttribute()
        {
        }

        /// <summary>
        /// Returns true if key was specified
        /// </summary>
        public Boolean IsKeySpecified
        {
            get { return Key != null; }
        }
    }
}
