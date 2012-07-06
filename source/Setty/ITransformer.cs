using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace Setty
{
    public interface ITransformer
    {
        void Transform(String inputFilePath, String outputFilePath, KeyValueConfigurationCollection settings);

        string ConfigExtention { get; }
    }
}
