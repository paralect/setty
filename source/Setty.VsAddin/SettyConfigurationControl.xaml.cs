using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;
using System.IO;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Setty.VsAddin.Code;
using UserControl = System.Windows.Controls.UserControl;

namespace Setty.VsAddin
{
    /// <summary>
    /// Interaction logic for SettyConfigurationControl.xaml
    /// </summary>
    public partial class SettyConfigurationControl : UserControl
    {
        public SettyConfigurationControl()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            InitWindow();
        }

        private void InitWindow()
        {
            UpdateSettings();

            cbEngine.Items.Add(new SelectListItem() { Value = ((int)SettyEngineEnum.Razor).ToString(), Display = "Razor" });
            cbEngine.Items.Add(new SelectListItem() { Value = ((int)SettyEngineEnum.Xslt).ToString(), Display = "Xslt" });
            cbEngine.DisplayMemberPath = "Display";
            cbEngine.SelectedValuePath = "Value";
            cbEngine.SelectedItem = cbEngine.Items[0];

            if (!string.IsNullOrEmpty(PackageManager.Instance.ActiveProjectPath))
            {
                cbActiveProject.Items.Add(new SelectListItem()
                                              {
                                                  Value = PackageManager.Instance.ActiveProjectPath,
                                                  Display =
                                                      Path.GetFileNameWithoutExtension(PackageManager.Instance.ActiveProjectPath)
                                              });
                cbActiveProject.SelectedItem = cbActiveProject.Items[0];

                foreach (var project in PackageManager.Instance.SolutionProjects)
                {
                    if (!string.IsNullOrEmpty(project) && project != PackageManager.Instance.ActiveProjectPath)
                    {
                        cbActiveProject.Items.Add(new SelectListItem()
                                                      {
                                                          Value = project,
                                                          Display = Path.GetFileNameWithoutExtension(project)
                                                      });
                    }
                }

                cbActiveProject.DisplayMemberPath = "Display";
                cbActiveProject.SelectedValuePath = "Value";

                var settings = PackageManager.Instance.Settings;
                foreach (var solution in settings.Solutions)
                {
                    if (solution.SolutionPath == PackageManager.Instance.ActiveSolutionPath)
                    {
                        cbEngine.SelectedItem = cbEngine.Items[(int)solution.Engine];
                        textBoxSettingsPath.Text = solution.SettingsPath;
                    }
                }

                textBoxSettingsPath.Text = new DirectoryInfo(PackageManager.Instance.ActiveSolutionPath).Parent.FullName;
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

        private void btnChangePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            dialog.SelectedPath = textBoxSettingsPath.Text;
            var result = dialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                textBoxSettingsPath.Text = dialog.SelectedPath;
            }

        }

        private void btnAddSetty_Click(object sender, RoutedEventArgs e)
        {
            if (!Directory.Exists(textBoxSettingsPath.Text))
            {
                System.Windows.MessageBox.Show("Invalid path to a settings folder.");
                return;
            }
            if (string.IsNullOrEmpty(PackageManager.Instance.ActiveProjectPath))
            {
                System.Windows.MessageBox.Show("Open any project to use setty.");
                return;
            }

            PackageManager.Instance.ActiveProjectPath = ((SelectListItem)cbActiveProject.SelectedItem).Value;
            var engine = (SelectListItem)cbEngine.SelectedItem;
            var solution = PackageManager.Instance.GetActiveSolution();
            solution.Engine = (SettyEngineEnum)int.Parse(engine.Value);
            solution.SettingsPath = textBoxSettingsPath.Text;
            PackageSettings.Serialize(PackageManager.Instance.UserDataPath, PackageManager.Instance.Settings);

            PackageManager.Instance.GenerateConfigs();
        }
    }

    public class SelectListItem
    {
        public string Value { get; set; }
        public string Display { get; set; }
    }



}
