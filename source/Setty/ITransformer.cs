using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Setty
{
    public interface ITransformer
    {
        /// <summary>
        /// Transformer accept path to the config file, transform it with key/value settings
        /// and save result to the output config file.
        /// Anyone can add new transformer by implementing this interface and adding new search files 
        /// to the SettyConstants.SearchConfigsNames. Setty will automatically choose transformer by ConfigExtention
        /// </summary>
        /// <param name="inputFilePath">Path to the setty config file (xslt or cshtml)</param>
        /// <param name="outputFilePath">Path to the output file </param>
        /// <param name="settings">Key/Value settings</param>
        void Transform(String inputFilePath, String outputFilePath, KeyValueConfigurationCollection settings);

        /// <summary>
        /// Extention of transformer config file
        /// </summary>
        string ConfigExtention { get; }
    }
}
