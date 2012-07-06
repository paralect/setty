using System.Configuration;
using System.Xml;
using System.Xml.Xsl;

namespace Setty.Engines.Xslt
{
    public class XsltTransformer : ITransformer
    {
        public void Transform(string inputFilePath, string outputFilePath, KeyValueConfigurationCollection settings)
        {
            var xsltTemplateReader = new XmlTextReader(inputFilePath);
            var emptyDocumentReader = XmlReader.Create(new System.IO.StringReader("<empty />"));

            var writerSettings = new XmlWriterSettings();
            //settings.ConformanceLevel = ConformanceLevel.Fragment;
            writerSettings.Indent = true;

            using (var outputWriter = XmlWriter.Create(outputFilePath, writerSettings))
            {
                var xsltSettings = new XsltSettings();

                XsltArgumentList argumentList = new XsltArgumentList();

                var extensions = new XsltExtensionMethods(settings);

                argumentList.AddExtensionObject("http://core.com/config", extensions);
                argumentList.AddExtensionObject("http://setty.net/config", extensions);

                var transformer = new XslCompiledTransform(true);
                transformer.Load(xsltTemplateReader, xsltSettings, null);
                transformer.Transform(emptyDocumentReader, argumentList, outputWriter);
            }
        }

        public string ConfigExtention
        {
            get { return "xslt"; }
        }
    }
}
