using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using System.Xml;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Setty;
using Setty.Utils;

namespace Microsoft.Setty_Vs12Addin.Code
{
    public class PackageManager
    {
        private static volatile PackageManager _instance;
        private static object _syncRoot = new Object();
        private SettyXmlBuilder _settyXmlBuilder;

        private PackageManager()
        {
            _settyXmlBuilder = new SettyXmlBuilder();
        }

        #region Properties

        public static PackageManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new PackageManager();
                    }
                }

                return _instance;
            }
        }

        public PackageSettings Settings { get; set; }

        public List<string> SolutionProjects { get; set; }

        public string ActiveProjectPath { get; set; }

        public string ActiveSolutionPath { get; set; }

        public string UserDataPath { get; set; }

        #endregion

        public PackageSettings MergeSettings(string solutionPath, string projectPath)
        {
            var settings = PackageSettings.Deserialize(UserDataPath);

            bool solutionExists = false;
            bool projectExists = false;
            foreach (var solution in settings.Solutions)
            {
                if (solution.SolutionPath == solutionPath)
                    solutionExists = true;
                foreach (var project in solution.Projects)
                {
                    if (project.ProjectPath == projectPath)
                    {
                        projectExists = true;
                    }
                }
            }

            if (!solutionExists)
            {
                var solution = new PackageSolutionSettings()
                {
                    SolutionPath = solutionPath,
                    Projects = new List<PackageProjectSettings>() { new PackageProjectSettings() { ProjectPath = projectPath } }
                };
                settings.Solutions.Add(solution);
            }
            else if (!projectExists)
            {
                foreach (var solution in settings.Solutions)
                {
                    if (solution.SolutionPath == solutionPath)
                    {
                        solution.Projects.Add(new PackageProjectSettings() { ProjectPath = projectPath });
                    }
                }
            }

            PackageSettings.Serialize(UserDataPath, settings);

            Instance.Settings = settings;
            Instance.ActiveProjectPath = projectPath;
            Instance.ActiveSolutionPath = solutionPath;

            return settings;
        }

        public PackageSolutionSettings GetActiveSolution()
        {
            return Settings.Solutions.FirstOrDefault(solution => solution.SolutionPath == ActiveSolutionPath);
        }

        public void GenerateConfigs()
        {
            var solutionSettings = GetActiveSolution();
            var contextFolder = Path.GetDirectoryName(ActiveProjectPath);
            var configs = FileSearcher.Search(contextFolder, SettyConstants.SupportedConfigNames, false);
            if (configs.Count > 0)
            {
                //0. Create empty setty.config
                _settyXmlBuilder.CreateEmptySettingsFile(solutionSettings.SettingsPath);
                //1. Create .setty if not exists
                CreateSettyConfig(solutionSettings.SolutionPath, solutionSettings.SettingsPath);
                //2. Download setty if not exists
                var pathToTheSetty = DownloadSetty(solutionSettings.SolutionPath);
                //3. Modify Active Project solution file
                _settyXmlBuilder.AddSettyPostBuildPartIntoProjectFile(ActiveProjectPath, pathToTheSetty, solutionSettings.SettingsPath);
                //4. Generate configs
                GenerateConfigs(configs, solutionSettings.Engine);
            }
        }

        public void CreateSettyWithoutUi()
        {
            UpdateSettings();

            var dialog = new FolderBrowserDialog();

            var settingsPath = Path.GetDirectoryName(PackageManager.Instance.ActiveSolutionPath);
            foreach (var solution in Instance.Settings.Solutions)
            {
                if (solution.SolutionPath == Instance.ActiveSolutionPath)
                {
                    if (!String.IsNullOrEmpty(solution.SettingsPath))
                        settingsPath = solution.SettingsPath;

                    break;
                }
            }

            dialog.SelectedPath = settingsPath;
            dialog.Description = "Select path to the global settings folder";
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK)
            {
                settingsPath = dialog.SelectedPath;
                var solution = PackageManager.Instance.GetActiveSolution();
                solution.Engine = SettyEngineEnum.Razor;
                solution.SettingsPath = settingsPath;
                PackageSettings.Serialize(PackageManager.Instance.UserDataPath, PackageManager.Instance.Settings);

                PackageManager.Instance.GenerateConfigs();
            }
        }

        public static void UpdateSettings()
        {
            DTE dte = PackageManager.GetCurrentDte();
            var projects = dte.Application.ActiveSolutionProjects as Array;
            if (projects.Length > 0)
            {
                var projectName = ((dynamic)projects.GetValue(0)).FullName;
                var solutionPath = dte.Solution.FullName;

                PackageManager.Instance.MergeSettings(solutionPath, projectName);

                PackageManager.Instance.SolutionProjects = new List<string>();
                foreach (var project in dte.Solution.Projects)
                {
                    PackageManager.Instance.SolutionProjects.Add(((dynamic)project).FullName);
                }
            }
        }

        private string CreateSettyConfig(string pathToSolution, string pathToSettings)
        {
            var solutionContextFolder = Path.GetDirectoryName(pathToSolution);
            string pathToConfig = Path.Combine(solutionContextFolder, SettyConstants.SettyConfigs.First());
            var settyConfigs = FileSearcher.Search(solutionContextFolder, SettyConstants.SettyConfigs);
            if (settyConfigs.Count == 0)
            {

                using (var settyConfig = new StreamWriter(pathToConfig))
                {
                    settyConfig.Write(pathToSettings);
                }

            }

            return pathToConfig;
        }

        private string DownloadSetty(string pathToSolution)
        {
            var solutionFolder = Path.GetDirectoryName(ActiveSolutionPath);
            var localPath = Path.Combine(solutionFolder, "setty.exe");

            if (!File.Exists(localPath))
            {
                var webClient = new WebClient();
                webClient.DownloadFile(AddinSettings.SettyDownloadPath, localPath);
            }

            return localPath;
        }

        private void GenerateConfigs(List<string> originalConfigs, SettyEngineEnum engine)
        {
            foreach (var originalConfig in originalConfigs)
            {
                var originalConfigFileName = Path.GetFileName(originalConfig);
                switch (engine)
                {
                    case SettyEngineEnum.Razor:
                        {
                            var settyConfig = String.Format("{0}.cshtml", originalConfig);

                            if (!File.Exists(settyConfig))
                            {
                                var xmlDoc = new XmlDocument();
                                xmlDoc.Load(originalConfig);
                                xmlDoc.Save(settyConfig);

                                _settyXmlBuilder.IncludeConfigIntoProjFile(ActiveProjectPath, originalConfigFileName, Path.GetFileName(settyConfig));
                            }
                        }
                        break;
                    case SettyEngineEnum.Xslt:
                        {
                            var settyConfig = String.Format("{0}.xslt", originalConfig);
                            if (!File.Exists(settyConfig))
                            {
                                _settyXmlBuilder.WrapSettyConfigWithXslt(originalConfig, settyConfig);
                                _settyXmlBuilder.IncludeConfigIntoProjFile(ActiveProjectPath, originalConfigFileName, Path.GetFileName(settyConfig));
                            }
                        }
                        break;
                }
            }
        }

        public static DTE GetCurrentDte(IServiceProvider provider)
        {
            /*ENVDTE. */
            DTE vs = (DTE)provider.GetService(typeof(DTE));
            if (vs == null) throw new InvalidOperationException("DTE not found.");
            return vs;
        }

        public static DTE GetCurrentDte()
        {
            return GetCurrentDte(/* Microsoft.VisualStudio.Shell. */ServiceProvider.GlobalProvider);
        }
    }
}
