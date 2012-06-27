using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using Setty.Settings.Exceptions;

namespace Setty.Settings
{
    /// <summary>
    /// Use this class to read settings respecting hierarchical file system 
    /// configuration (i.e. .core.settings files)
    /// </summary>
    public class SettingsBrowser
    {
        /// <summary>
        /// Name of config file
        /// </summary>
        public const String ConfigFile = ".setty.config";

        /// <summary>
        /// Legacy name of the config file
        /// </summary>
        private const String ConfigFileLegacy = ".core.config";

        /// <summary>
        /// Cached collection of settings (by settings folder)
        /// </summary>
        private Dictionary<String, KeyValueConfigurationCollection> _settingsByConfigurationPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public SettingsBrowser()
        {
            _settingsByConfigurationPath = new Dictionary<string, KeyValueConfigurationCollection>();
        }

        /// <summary>
        /// Get settings for specified folder.
        /// </summary>
        /// <param name="contextFolder">
        /// This is not a settings folder! This is a context for what you want to get settings
        /// </param>
        /// <param name="settingsFolder">
        /// Specify exact path to settings folder, if needed. By default it will browse file system to 
        /// find .core.settings config file.
        /// </param>
        public KeyValueConfigurationCollection GetSettings(String contextFolder, String settingsFolder = "")
        {
            if (!String.IsNullOrEmpty(settingsFolder) && !Path.IsPathRooted(settingsFolder))
                throw new ArgumentException("Path to settings folder can't be relative");


            string pathToConfigFile = "";
            if (String.IsNullOrEmpty(settingsFolder))
            {
                pathToConfigFile = FindPathToConfigFile(contextFolder);
                settingsFolder = ReadSettingsFolderFromConfig(pathToConfigFile);
            }

            if (_settingsByConfigurationPath.ContainsKey(settingsFolder))
                return _settingsByConfigurationPath[settingsFolder];

            SettingsReader reader = new SettingsReader(settingsFolder);
            var settings = reader.Read();

            _settingsByConfigurationPath[settingsFolder] = settings;
            return settings;
        }

        /// <summary>
        /// By given directory find path to ConfigFile
        /// </summary>
        /// <param name="directoryPath">Folder from which start search, it will go deeply and deeply until not find first .setty.config or .core.config</param>
        /// <returns>Path to ConfigFile</returns>
        private String FindPathToConfigFile(string directoryPath)
        {
            DirectoryInfo current = new DirectoryInfo(directoryPath);
            DirectoryInfo root = current.Root;

            while (String.CompareOrdinal(root.FullName, current.FullName) != 0)
            {
                var coreConfigurationFilePath = Path.Combine(current.FullName, ConfigFile);
                var legacyCoreConfigurationFilePath = Path.Combine(current.FullName, ConfigFileLegacy);

                // First read from .setty.config
                if (File.Exists(coreConfigurationFilePath))
                    return coreConfigurationFilePath;

                // Then try to find .core.config legacy configuration file
                if (File.Exists(legacyCoreConfigurationFilePath))
                    return legacyCoreConfigurationFilePath;

                current = current.Parent;
            }

            throw new SettingsFolderNotFound(String.Format("Settings folder was not found. Make sure that you have .setty.config file in the current folder (or in any ancestor folder)."));
        }

        /// <summary>
        /// Read path to Settings Folder from config (i.e. .core.settings)
        /// </summary>
        private String ReadSettingsFolderFromConfig(String filePath)
        {
            var settingsFolderPath = File.ReadAllText(filePath).Trim();
            settingsFolderPath = PathHelper.ProcessPossiblyRelativePath(settingsFolderPath, filePath);
            if (!Directory.Exists(settingsFolderPath))
                throw new SettingsFolderNotFound(String.Format("Settings folder was not found. File {0} points to settings folder that doesn't exist: {1}.", filePath, settingsFolderPath));

            return settingsFolderPath;
        }
    }
}
