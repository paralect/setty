using System;
using System.IO;
using System.Reflection;
using Setty.Settings;

namespace Setty.Test
{
    public class Helper
    {
        public static String GetDataPath()
        {
            string path = (new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath;

            DirectoryInfo info = new DirectoryInfo(path);
            var projectPath = info.Parent.Parent.Parent.FullName;

            return Path.Combine(projectPath, "Data");
        }

        public static void PrepareCoreConfigFiles()
        {
            var config1 = Path.Combine(GetDataPath(), SettingsBrowser.ConfigFile);
            var config2 = Path.Combine(GetDataPath(), String.Format(@"Folder\SubFolder\SubSubFolder\{0}", SettingsBrowser.ConfigFile));

            var settings1 = Path.Combine(GetDataPath(), @"Settings\DmitrySchetnikovich");
            var settings2 = Path.Combine(GetDataPath(), @"Settings\Stage");

            File.WriteAllText(config1, settings1);
            File.WriteAllText(config2, settings2);
        }
    }
}
