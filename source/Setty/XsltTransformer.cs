using System;
using System.Configuration;
using System.Xml;
using System.Xml.Xsl;

namespace Setty
{
    public class XsltTransformer
    {
        private readonly string _xsltPath;
        private readonly string _outputPath;
        private readonly KeyValueConfigurationCollection _settings;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public XsltTransformer(String xsltPath, String outputPath, KeyValueConfigurationCollection settings)
        {
            _xsltPath = xsltPath;
            _outputPath = outputPath;
            _settings = settings;
        }

        public void Transform()
        {
            var xsltTemplateReader = new XmlTextReader(_xsltPath);
            var emptyDocumentReader = XmlReader.Create(new System.IO.StringReader("<empty />"));

            var settings = new XmlWriterSettings();
            //settings.ConformanceLevel = ConformanceLevel.Fragment;
            settings.Indent = true;

            using (var outputWriter = XmlWriter.Create(_outputPath, settings))
            {
                var xsltSettings = new XsltSettings();

                XsltArgumentList argumentList = new XsltArgumentList();

                var extensions = new XsltExtensionMethods(_settings);

                argumentList.AddExtensionObject("http://core.com/config", extensions);
                argumentList.AddExtensionObject("http://setty.net/config", extensions);

                var transformer = new XslCompiledTransform(true);
                transformer.Load(xsltTemplateReader, xsltSettings, null);
                transformer.Transform(emptyDocumentReader,argumentList, outputWriter);
            }
        }
    }
}
