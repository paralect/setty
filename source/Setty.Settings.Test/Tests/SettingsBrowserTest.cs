using System.IO;
using NUnit.Framework;

namespace Setty.Settings.Test.Tests
{
    [TestFixture]
    public class SettingsBrowserTest
    {
        public SettingsBrowserTest()
        {
            Helper.PrepareCoreConfigFiles();
        }

        [Test]
        public void SimpleTest()
        {
            var config1 = Path.Combine(Helper.GetDataPath(), "Folder");
            var config2 = Path.Combine(Helper.GetDataPath(), "Folder\\SubFolder");
            var config3 = Path.Combine(Helper.GetDataPath(), "Folder\\SubFolder\\SubSubFolder");

            SettingsBrowser browser = new SettingsBrowser();

            var res1 = browser.GetSettings(config1);
            var res2 = browser.GetSettings(config2);
            var res3 = browser.GetSettings(config3);

            Assert.AreEqual(res1["Acropolis.SolutionFolder"].Value, "d:\\Projects\\Ajeva Project");
            Assert.AreEqual(res2["Acropolis.SolutionFolder"].Value, "d:\\Projects\\Ajeva Project");
            Assert.AreEqual(res3["Acropolis.SolutionFolder"].Value, "c:\\Server\\Web\\ajeva.com\\Web");
        }
    }
}
