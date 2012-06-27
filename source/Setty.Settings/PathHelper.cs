using System;
using System.IO;

namespace Setty.Settings
{
    public class PathHelper
    {
        public static string ProcessPossiblyRelativePath(string path, string contextPath)
        {
            var res = path;
            //handle relative path
            if (!String.IsNullOrEmpty(contextPath) && !Path.IsPathRooted(path))
            {
                var configFileFileDirectory = new DirectoryInfo(contextPath);
                var contextDirectory = configFileFileDirectory.Parent.FullName + @"\";
                res = Path.GetFullPath(contextDirectory + path);
            }

            return res;
        }
    }
}
