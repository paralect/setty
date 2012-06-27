using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace Setty.Settings
{
    /// <summary>
    /// Reads and merges settings from hierarchy of folders
    /// </summary>
    public class SettingsReader
    {
        /// <summary>
        /// Path to folder
        /// </summary>
        private readonly string _path;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SettingsReader(String path)
        {
            _path = path;
        }

        /// <summary>
        /// Get merged KeyValueConfigurationCollection
        /// </summary>
        public KeyValueConfigurationCollection Read()
        {
            var paths = GetDirectoryAscendants(_path);
            return Merge(paths);
        }

        /// <summary>
        /// Returns merged settings collection
        /// </summary>
        private KeyValueConfigurationCollection Merge(List<String> paths)
        {
            var mergedSettings = new KeyValueConfigurationCollection();

            foreach (var path in paths)
            {
                var settings = GetApplicationSettingsFromDirectory(path);

                if (settings == null)
                    continue;

                foreach (var key in settings.AllKeys)
                {
                    var value = settings[key].Value;

                    mergedSettings.Remove(key);
                    mergedSettings.Add(key, value);
                }                
            }

            return mergedSettings;
        }

        /// <summary>
        /// Load <appSettings /> collection from well formed .net configuration file
        /// </summary>
        private KeyValueConfigurationCollection GetApplicationSettingsFromFile(String configFilePath)
        {
            ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = configFilePath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);

            var settings = config.AppSettings.Settings;
            return settings;
        }

        /// <summary>
        /// Get default configuration file from directory
        /// </summary>
        /// <returns>
        /// Collection if found, null if not
        /// </returns>
        private KeyValueConfigurationCollection GetApplicationSettingsFromDirectory(String directoryPath)
        {
            var configFilePath = Path.Combine(directoryPath, "App.config");

            if (!File.Exists(configFilePath))
                return null;

            return GetApplicationSettingsFromFile(configFilePath);
        }

        /// <summary>
        /// Will return list of ascendats of folder. Something like this:
        /// C:\
        /// C:\Folder
        /// C:\Folder\SubFolder
        /// C:\Folder\SubFolder\SubSubFolder
        /// etc...
        /// </summary>
        private List<String> GetDirectoryAscendants(String folder)
        {
            var paths = new List<string>();

            DirectoryInfo current = new DirectoryInfo(folder);
            DirectoryInfo root = current.Root;

            while (String.CompareOrdinal(root.FullName, current.FullName) != 0)
            {
                paths.Insert(0, current.FullName);
                current = current.Parent;
            }

            return paths;
        }
    }
}
