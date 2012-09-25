using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace Microsoft.Setty_Vs12Addin.Code
{
    public class PackageSettings
    {
        public PackageSettings()
        {
            Solutions = new List<PackageSolutionSettings>();
        }
        
        private static string _pathToUserData;
        private static string _dataFileName = AddinSettings.AddinSettingsFile;
        private static string PathToDataFile
        {
            get { return Path.Combine(_pathToUserData, _dataFileName); }
        }

        public List<PackageSolutionSettings> Solutions { get; set; }

        public static void Serialize(string pathToUserData, PackageSettings item)
        {
            _pathToUserData = pathToUserData;
            var serializer = new XmlSerializer(typeof(PackageSettings));
            TextWriter writer = new StreamWriter(PathToDataFile);
            serializer.Serialize(writer, item);
            writer.Close();
        }

        public static PackageSettings Deserialize(string pathToUserData)
        {
            _pathToUserData = pathToUserData;
            if (!File.Exists(PathToDataFile))
            {
                Serialize(pathToUserData, new PackageSettings());
            }

            var serializer = new XmlSerializer(typeof(PackageSettings));
            using (var fs = new FileStream(PathToDataFile, FileMode.Open))
            {
                return (PackageSettings)serializer.Deserialize(fs);
            }
        }
    }

    public class PackageSolutionSettings
    {
        public PackageSolutionSettings()
        {
            Projects = new List<PackageProjectSettings>();
        }

        public List<PackageProjectSettings> Projects { get; set; }

        public string SettingsPath { get; set; }

        public string SolutionPath { get; set; }

        public SettyEngineEnum Engine { get; set; }
    }

    public class PackageProjectSettings
    {
        public string ProjectPath { get; set; }
    }
}
