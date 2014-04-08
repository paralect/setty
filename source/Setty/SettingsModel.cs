using System.Collections.Generic;

namespace Setty
{
    public class SettingsModel
    {
        private readonly Dictionary<string, string> _settings;

        public SettingsModel(IDictionary<string, string> source)
        {
            _settings = new Dictionary<string, string>(source);
        }

        public string this[string key]   
        {
            get
            {
                if (!_settings.ContainsKey(key))
                {
                    throw new KeyNotFoundException(string.Format("Key \"{0}\" was not found in settings file", key));
                }
                return _settings[key];
            }
        }
    }
}