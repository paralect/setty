using System.IO;
using System.Xml;
using NUnit.Framework;

namespace Setty.Test.Tests
{
    [TestFixture]
    public class ConfigFolderTransformerTest : TestBase
    {
        [Test]
        public void Test()
        {
            var transformer = new Transformer();
            transformer.Transform(Helper.GetDataPath());

            var config1 = Path.Combine(Helper.GetDataPath(), "Folder\\Web.config");
            var config2 = Path.Combine(Helper.GetDataPath(), "Folder\\SubFolder\\App.config");
            var config3 = Path.Combine(Helper.GetDataPath(), "Folder\\SubFolder\\SubSubFolder\\App.config");

            XmlDocument doc1 = new XmlDocument(); doc1.Load(config1);
            XmlDocument doc2 = new XmlDocument(); doc2.Load(config2);
            XmlDocument doc3 = new XmlDocument(); doc3.Load(config3);

            var nodes1 = doc1.SelectNodes("configuration/appSettings/add[@key = 'Acropolis.SolutionFolder' and @value = 'd:\\Projects\\Ajeva Project']");
            var nodes2 = doc2.SelectNodes("configuration/appSettings/add[@key = 'Acropolis.SolutionFolder' and @value = 'd:\\Projects\\Ajeva Project']");
            var nodes3 = doc3.SelectNodes("configuration/appSettings/add[@key = 'Acropolis.SolutionFolder' and @value = 'c:\\Server\\Web\\ajeva.com\\Web']");

            Assert.AreEqual(nodes1.Count, 1);
            Assert.AreEqual(nodes2.Count, 1);
            Assert.AreEqual(nodes3.Count, 1);


        }
    }
}
