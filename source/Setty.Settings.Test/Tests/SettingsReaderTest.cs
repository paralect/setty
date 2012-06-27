using System;
using System.Configuration;
using System.IO;
using NUnit.Framework;

namespace Setty.Settings.Test.Tests
{
    [TestFixture()]
    public class SettingsReaderTest
    {
        public SettingsReaderTest()
        {
            Helper.PrepareCoreConfigFiles();
        }

        public static String GetSettingsPath()
        {
            return Path.Combine(Helper.GetDataPath(), "settings");
        }

        [Test]
        public void OneLevelTest()
        {
            var settingsPath = GetSettingsPath();

            var reader = new SettingsReader(settingsPath);
            var result = reader.Read();

            Assert.AreEqual(result["Acropolis.SolutionFolder"].Value, @"Please specify Acropolis.SolutionFolder in your configuration!");
        }

        [Test]
        public void TwoLevelTest()
        {
            var settingsPath = Path.Combine(GetSettingsPath(), "DmitrySchetnikovich");

            var reader = new SettingsReader(settingsPath);
            var result = reader.Read();

            Assert.AreEqual(result["Acropolis.SolutionFolder"].Value, @"d:\Projects\Ajeva Project");
        }

        [Test]
        public void ThreeLevelTest()
        {
            var settingsPath = Path.Combine(GetSettingsPath(), "Production\\Test");

            var reader = new SettingsReader(settingsPath);
            var result = reader.Read();

            Assert.AreEqual(result["Xomo.Application.RootUrl"].Value, @"http://xomo.epear.com");
        }

        [Test]
        public void Test_Relative_Path_When_Settings_In_Same_Folder()
        {
            var relative = "settings";
            var pathToConfigFile = @"d:\git\paralect\Setty\source\Setty.Settings.Test\Data\.core.config";

            var processedSettingsPath = PathHelper.ProcessPossiblyRelativePath(relative, pathToConfigFile);
            Assert.AreEqual(processedSettingsPath, @"d:\git\paralect\Setty\source\Setty.Settings.Test\Data\settings");
        }

        [Test]
        public void Test_Relative_Path_When_Settings_At_The_Parrent_Folder()
        {
            var relative = "../settings";
            var pathToConfigFile = @"d:\git\paralect\Setty\source\Setty.Settings.Test\Data\.core.config";

            var processedSettingsPath = PathHelper.ProcessPossiblyRelativePath(relative, pathToConfigFile);
            Assert.AreEqual(processedSettingsPath, @"d:\git\paralect\Setty\source\Setty.Settings.Test\settings");
        }

        [Test]
        public void Test_Relative_Path_When_Settings_Deeply()
        {
            var relative = "someSystem/settings";
            var pathToConfigFile = @"d:\git\paralect\Setty\source\Setty.Settings.Test\Data\.core.config";

            var processedSettingsPath = PathHelper.ProcessPossiblyRelativePath(relative, pathToConfigFile);
            Assert.AreEqual(processedSettingsPath, @"d:\git\paralect\Setty\source\Setty.Settings.Test\Data\someSystem\settings");
        }

        [Test]
        public void RelativePath_Which_Start_From_Slash_Will_Be_Used_As_AbsolutePath()
        {
            var relative = @"/someSystem/settings";
            var pathToConfigFile = @"d:\git\paralect\Setty\source\Setty.Settings.Test\Data\.core.config";

            var processedSettingsPath = PathHelper.ProcessPossiblyRelativePath(relative, pathToConfigFile);
            Assert.AreEqual(processedSettingsPath, relative);
        }

        [Test]
        public void Test()
        {
            KeyValueConfigurationCollection collection = new KeyValueConfigurationCollection();
            collection.Add("key", "value");
        }
    }
}
