using System;
using System.Collections.Generic;
using System.IO;

namespace Setty.Utils
{
    /// <summary>
    /// Search files with specified names and return array of absolute paths
    /// </summary>
    public class FileSearcher
    {
        /// <summary>
        /// Search files with specified names and return array of absolute paths
        /// </summary>
        public static List<String> Search(String directory, List<String> fileNames)
        {
            return TraverseDirectory(new DirectoryInfo(directory), fileNames);
        }

        /// <summary>
        /// Traversing of directory
        /// </summary>
        private static List<String> TraverseDirectory(DirectoryInfo root, List<String> fileNames)
        {
            var list = new List<String>();

            foreach (var fileName in fileNames)
            {
                var path = Path.Combine(root.FullName, fileName);

                if (File.Exists(path))
                    list.Add(path);
            }

            foreach (var subdir in root.GetDirectories())
            {
                list.AddRange(TraverseDirectory(subdir, fileNames));
            }

            return list;
        }
    }
}
