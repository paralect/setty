using System;
using System.Configuration;
using System.Text;

namespace Setty.Utils
{
    public static class SettingsHelper
    {
        public static String GetApplicationSettingsXmlString(KeyValueConfigurationCollection collection)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Environment.NewLine);

            foreach (var key in collection.AllKeys)
            {
                var value = collection[key].Value;

                builder.Append(String.Format(
                    "    <add key=\"{0}\" value=\"{1}\" />{2}",
                    Escape(key),
                    Escape(value),
                    Environment.NewLine));
            }

            return builder.ToString();            
        }

        public static String Escape(String value)
        {
            return value
                .Replace("&", "&amp;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("\"", "&quot;");

            //  we do not need to escape ' to &apos; for our case (because we are 
            //  using double quotes as surrounding quotes for attributes values)
        }
    }
}
