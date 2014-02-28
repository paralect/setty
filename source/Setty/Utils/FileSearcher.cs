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
        public static List<String> Search(String directory, List<String> fileNames, bool deep = true)
        {
            return TraverseDirectory(new DirectoryInfo(directory), fileNames, deep);
        }

        /// <summary>
        /// Traversing of directory
        /// </summary>
        private static List<String> TraverseDirectory(DirectoryInfo root, List<String> fileNames, bool deep = true)
        {
            var list = new List<String>();

            foreach (var fileName in fileNames)
            {
                var files = root.GetFiles(fileName);
                foreach (var file in files)
                {
                    list.Add(file.FullName);
                }
            }

            if (deep)
            {
                foreach (var subdir in root.GetDirectories())
                {
                    list.AddRange(TraverseDirectory(subdir, fileNames, deep));
                }
            }

            return list;
        }
    }
}
