using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Setty
{
    public static class SettyConstants
    {
        public static List<string> SearchConfigsNames
        {
            get
            {
                return new List<string>()
                {
                    "*.config.cshtml", 
                    "*.config.xslt",
                };
            }
        }

        public static List<string> SettyConfigs
        {
            get
            {
                return new List<string>()
                {
                    ".setty", 
                    ".core.config"
                };
            }
        }
    }
}
