using System;
using System.Collections.Generic;
using System.Configuration;
using System.Dynamic;
using System.Linq;
using System.Xml;
using System.Xml.Xsl;
using RazorEngine;
using Setty.Utils;

namespace Setty.Engines.MvcRazor
{
    public class RazorTransformer : ITransformer
    {
        public void Transform(String configPath, String outputPath, KeyValueConfigurationCollection settings)
        {
            var xsltTemplateReader = new XmlTextReader(configPath);

            var writerSettings = new XmlWriterSettings();
            writerSettings.ConformanceLevel = ConformanceLevel.Fragment;
            writerSettings.Indent = true;

            using (var outputWriter = XmlWriter.Create(outputPath, writerSettings))
            {
                var template = System.IO.File.ReadAllText(configPath);
                var model = MapSettings(settings);

                string result = Razor.Parse(template, model);

                outputWriter.WriteRaw(result);
            }
        }

        public string ConfigExtention
        {
            get { return "cshtml"; }
        }

        //private dynamic MapSettings(KeyValueConfigurationCollection settings)
        //{
        //    var expandoDictionary = new ExpandoObject() as IDictionary<string, Object>;

        //    foreach (var key in settings.AllKeys)
        //        expandoDictionary.Add(key, settings[key].Value);

        //    expandoDictionary.Add("AppSettings", SettingsHelper.GetApplicationSettingsXmlString(settings));

        //    return expandoDictionary as dynamic;
        //}

        private Dictionary<string, string> MapSettings(KeyValueConfigurationCollection settings)
        {
            var res = settings.AllKeys.ToDictionary(key => key, key => settings[key].Value);

            res.Add("AppSettings", SettingsHelper.GetApplicationSettingsXmlString(settings));

            return res;
        }
    }
}
