using System;
using System.Collections.Generic;
using System.IO;
using Setty.Settings;
using Setty.Utils;

namespace Setty
{
    public class Transformer
    {
        public List<String> Transform(String contextPath, String settingsPath = "")
        {
            var configs = FileSearcher.Search(contextPath, SettyConstants.SearchConfigsNames);

            var settingsBrowser = new SettingsBrowser();
            var transformerSelector = new TransformerSelector();

            foreach (var config in configs)
            {
                var info = GetDirectoryOfFile(config);
                var output = GetOutputFilePath(config);

                var settings = settingsBrowser.GetSettings(info, settingsPath);

                var transformer = transformerSelector.Select(config);
                transformer.Transform(config, output, settings);
            }

            return configs;
        }

        /// <summary>
        /// Returns absolute path to directory where file is located
        /// </summary>
        private String GetDirectoryOfFile(String absoluteFilePath)
        {
            var info = new FileInfo(absoluteFilePath);
            return info.Directory.FullName;
        }

        /// <summary>
        /// Get output file name by input file name
        /// </summary>
        private String GetOutputFilePath(String inputFilePath)
        {
            var dotPosition = inputFilePath.LastIndexOf('.');

            if (dotPosition == -1)
                return String.Format("{0}.output", inputFilePath);

            return inputFilePath.Substring(0, dotPosition);
        }
    }
}
