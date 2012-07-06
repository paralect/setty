using System;
using System.Configuration;
using System.IO;
using System.Xml;
using NUnit.Framework;
using Setty.Engines.MvcRazor;
using Setty.Engines.Xslt;

namespace Setty.Test.Tests
{
    [TestFixture()]
    public class TransformTest : TestBase
    {
        private static KeyValueConfigurationCollection GetKeyValueCollection()
        {
            var collection = new KeyValueConfigurationCollection();
            collection.Add("Name", "Value");
            collection.Add("Email", "some@email.com");
            collection.Add("Company", "IBM");
            collection.Add("Year", "2011");

            return collection;
        }

        [Test]
        public void SimpleTransformTest()
        {
            var xslt = Path.Combine(Helper.GetDataPath(), "Sample.xslt");
            var output = Path.Combine(Helper.GetDataPath(), "Sample.xml");

            if (File.Exists(output))
                File.Delete(output);

            var transformer = new XsltTransformer();
            transformer.Transform(xslt, output, GetKeyValueCollection());

            XmlDocument doc = new XmlDocument();
            doc.Load(output);

            var nameNodes = doc.SelectNodes("configuration/appSettings/add[@key = 'Name' and @value = 'Value']");
            var emailNodes = doc.SelectNodes("configuration/appSettings/add[@key = 'Email' and @value = 'some@email.com']");
            var companyNodes = doc.SelectNodes("configuration/appSettings/add[@key = 'Company' and @value = 'IBM']");

            Assert.AreEqual(nameNodes.Count, 1);
            Assert.AreEqual(emailNodes.Count, 1);
            Assert.AreEqual(companyNodes.Count, 1);
        }

        [Test]
        public void RazorTranformTest()
        {
            var xslt = Path.Combine(Helper.GetDataPath(), "Sample.cshtml");
            var output = Path.Combine(Helper.GetDataPath(), "Sample.xml");

            if (File.Exists(output))
                File.Delete(output);

            var transformer = new RazorTransformer();
            transformer.Transform(xslt, output, GetKeyValueCollection());

            XmlDocument doc = new XmlDocument();
            doc.Load(output);

            var nameNodes = doc.SelectNodes("configuration/appSettings/add[@key = 'Name' and @value = 'Value']");
            var emailNodes = doc.SelectNodes("configuration/appSettings/add[@key = 'Email' and @value = 'some@email.com']");
            var companyNodes = doc.SelectNodes("configuration/appSettings/add[@key = 'Company' and @value = 'IBM']");

            Assert.AreEqual(nameNodes.Count, 1);
            Assert.AreEqual(emailNodes.Count, 1);
            Assert.AreEqual(companyNodes.Count, 1);
        }

        [Test]
        public void ApplicatioSettings()
        {
            var xslt = Path.Combine(Helper.GetDataPath(), "ApplicationSettings.xslt");
            var output = Path.Combine(Helper.GetDataPath(), "ApplicationSettings.xml");

            if (File.Exists(output))
                File.Delete(output);

            var expected = GetKeyValueCollection();

            var transformer = new XsltTransformer();
            transformer.Transform(xslt, output, expected);

            var actual = GetApplicationSettingsFromFile(output);

            Assert.AreEqual(expected.Count, actual.Count);


   
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

    }
}
