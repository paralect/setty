using System;
using System.Reflection;

namespace Setty.Settings
{
    /// <summary>
    /// Maps application settings to your custom type or to existing instance of object
    /// SettingsMapper read <appSettings /> right from your *.config file
    /// </summary>
    public static class SettingsMapper
    {
        /// <summary>
        /// Map application settings to instance of specified type
        /// </summary>
        public static TObject Map<TObject>()
            where TObject : class, new()
        {
            var instance = Activator.CreateInstance(typeof(TObject));
            Map(typeof(TObject), instance);
            return (TObject)instance;
        }

        /// <summary>
        /// Map application settings to existing object instance
        /// </summary>
        public static void Map<TObject>(TObject instance)
            where TObject : class, new()
        {
            Map(typeof(TObject), instance);
        }

        /// <summary>
        /// Map application settings to instance of specified type
        /// </summary>
        public static Object Map(Type type, string prefix = null)
        {
            var instance = Activator.CreateInstance(type);
            Map(type, instance, prefix);
            return instance;
        }

        /// <summary>
        /// Map application settings to existing object instance
        /// </summary>
        public static void Map(Object instance, string prefix = null)
        {
            Map(instance.GetType(), instance, prefix);
        }

        /// <summary>
        /// Map application settings to your custom type or to existing instance of object
        /// null i
        /// </summary>
        private static void Map(Type type, Object instance, string prefix = null)
        {
            // Create object instance if not specified
            if (instance == null)
                throw new ArgumentNullException("instance");

            // Get all the public properties of the type
            PropertyInfo[] propertyInfos = type.GetProperties();

            foreach (var propertyInfo in propertyInfos)
            {
                Object[] propertyAttributes = propertyInfo.GetCustomAttributes(typeof(SettingsPropertyAttribute), true);

                if (propertyAttributes.Length == 0)
                    continue;

                Object[] prefixAttributes = propertyInfo.GetCustomAttributes(typeof(SettingsPrefixAttribute), true);
                if (prefixAttributes.Length == 1)
                {
                    prefix = ((SettingsPrefixAttribute)prefixAttributes[0]).Prefix;
                }

                ApplySettingsProperty(instance, propertyInfo, (SettingsPropertyAttribute)propertyAttributes[0], prefix);
            }

        }

        private static void ApplySettingsProperty(Object instance, PropertyInfo propertyInfo, SettingsPropertyAttribute attribute, string prefix = null)
        {
            // If key was not specified we assuming that this is inner object
            if (!attribute.IsKeySpecified)
            {
                var innerObj = Map(propertyInfo.PropertyType, prefix);
                propertyInfo.SetValue(instance, innerObj, null);
                return;
            }

            // Reading settings from <appSettings />
            var key = String.IsNullOrEmpty(prefix) ? attribute.Key : String.Format("{0}.{1}", prefix, attribute.Key);
            var value = System.Configuration.ConfigurationManager.AppSettings[key];

            Object convertedValue = null;

            // If this is Boolean 
            if (propertyInfo.PropertyType == typeof(Boolean))
            {
                if (String.Compare(value, "true", true) == 0 || String.Compare(value, "yes", true) == 0)
                    convertedValue = true;
                else if (String.Compare(value, "false", true) == 0 || String.Compare(value, "no", true) == 0)
                    convertedValue = false;
                else
                    throw new Exception(String.Format("Cannot convert from '{0}' to Boolean. Value can be 'true' or 'false'.", value));
            }
            // Otherwise we are using System.Convert 
            else
            {
                convertedValue = Convert.ChangeType(value, propertyInfo.PropertyType);
            }

            propertyInfo.SetValue(instance, convertedValue, null);

        }
    }
}
