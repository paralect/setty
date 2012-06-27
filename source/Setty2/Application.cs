using System;
using Setty.Settings.Exceptions;

namespace Setty2
{
    public class Application
    {
        /// <summary>
        /// Context folder (by default the same as executable)
        /// </summary>
        private String _contextFolder;

        /// <summary>
        /// Settings folder (by default 
        /// </summary>
        private String _settingsFolder;

        /// <summary>
        /// Do not block UI
        /// </summary>
        private Boolean _silent;

        /// <summary>
        /// Context folder (by default the same as executable)
        /// </summary>
        public string ContextFolder
        {
            get { return _contextFolder; }
        }

        /// <summary>
        /// Settings folder (by default 
        /// </summary>
        public string SettingsFolder
        {
            get { return _settingsFolder; }
        }

        /// <summary>
        /// Do not block UI
        /// </summary>
        public bool Silent
        {
            get { return _silent; }
        }

        /// <summary>
        /// Entry point
        /// </summary>
        public static void Main(String[] args)
        {
            Application app = null;
            try
            {
                Console.Write("Setty 1.0");
                app = new Application(args);
                app.Launch();
            }
            catch (SettingsFolderNotFound e)
            {
                Console.WriteLine();
                Console.WriteLine("ERROR: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine();
                Console.WriteLine("ERROR: " + e.Message);
            }
            
            if (app != null && !app.Silent)
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public Application(String[] args)
        {
            ParseArguments(args);
        }

        /// <summary>
        /// Parse arguments
        /// </summary>
        private void ParseArguments(String[] args)
        {
            _contextFolder = Environment.CurrentDirectory;
            _settingsFolder = String.Empty;
            _silent = false;

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];

                if (arg.StartsWith("/context:"))
                {
                    _contextFolder = GetValue(arg);
                }

                if (arg.StartsWith("/settings:"))
                {
                    _settingsFolder = GetValue(arg);
                }

                if (arg.StartsWith("/silent"))
                {
                    _silent = true;
                }
            }            
        }

        private void Launch()
        {
            Console.WriteLine(" ({0})", _contextFolder);
            var transformer = new Transformer();
            var transformed = transformer.Transform(_contextFolder, _settingsFolder);

            Console.WriteLine();
            Console.WriteLine("{0} files tranformed{1}", 
                transformed.Count, 
                transformed.Count == 0 ? "." : ":" + Environment.NewLine);

            for (int i = 0; i < transformed.Count; i++)
            {
                var path = transformed[i];
                Console.WriteLine("{0}) {1}", i + 1, path);
            }
        }

        private String GetValue(String arg)
        {
            var column = arg.IndexOf(':');

            if (column == -1 || column + 1 >= arg.Length)
                return String.Empty;

            var value = arg.Substring(column + 1).Trim('"');

            return value;
        }
    }
}
