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

        /// <summary>
        /// Creates a relative path from one file or folder to another.
        /// </summary>
        /// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
        /// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
        /// <param name="dontEscape">Boolean indicating whether to add uri safe escapes to the relative path</param>
        /// <returns>The relative path from the start directory to the end path.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static String MakeRelativePath(String fromPath, String toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            String relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }
    }
}
