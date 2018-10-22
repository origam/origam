#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Windows.Forms;
using AeroWizard;
using Origam.Workbench;
using Origam.UI;
using Origam.ProjectAutomation;
using Origam.Workbench.Services;
using Origam.Workbench.Commands;
using System.IO;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace OrigamArchitect
{
    public partial class NewProjectWizard : Form
    {
        enum DeploymentType
        {
            Local,
            Azure
        }

        [DllImport("user32")]
        public static extern UInt32 SendMessage
            (IntPtr hWnd, UInt32 msg, UInt32 wParam, UInt32 lParam);

        internal const int BCM_FIRST = 0x1600; //Normal button
        internal const int BCM_SETSHIELD = (BCM_FIRST + 0x000C); //Elevated button
        ProjectBuilder _builder = new ProjectBuilder();
        Project _project = new Project();
        NewProjectWizardSettings _settings = new NewProjectWizardSettings();

        public NewProjectWizard()
        {
            InitializeComponent();
            wizard1.FinishButtonText = "Run";
        }

        private DeploymentType Deployment
        {
            get
            {
                switch (cboDeploymentType.SelectedIndex)
                {
                    case 0:
                        return DeploymentType.Local;
                    case 1:
                        return DeploymentType.Azure;
                    default:
                        throw new ArgumentOutOfRangeException("DeploymentType", 
                            cboDeploymentType.SelectedIndex, strings.UnknownDeploymentType);
                }
            }
        }

        private void pageReview_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            InitTaskList();
            WorkbenchSingleton.Workbench.Disconnect();
            WorkbenchSingleton.Workbench.PopulateEmptyDatabaseOnLoad = false;
            Application.DoEvents();
            try
            {
                _builder.Create(_project);
                WorkbenchSchemaService schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService))
                    as WorkbenchSchemaService;
                WorkbenchSingleton.Workbench.Disconnect();
                WorkbenchSingleton.Workbench.Connect(_project.Name);
                schema.LoadSchema(new Guid(_project.NewPackageId), false, true);
                ViewSchemaBrowserPad cmdViewBrowser = new ViewSchemaBrowserPad();
                cmdViewBrowser.Run();
                SaveSettings();
            }
            catch (Exception ex)
            {
                AsMessageBox.ShowError(this, ex.Message, strings.NewProjectFailed_Message, ex);
                WorkbenchSingleton.Workbench.Disconnect();
            }
            finally
            {
                WorkbenchSingleton.Workbench.PopulateEmptyDatabaseOnLoad = true;
            }
        }

        private void SaveSettings()
        {
            _settings.SourcesFolder = txtSourcesFolder.Text;
            _settings.BinFolder = txtBinFolderRoot.Text;
            _settings.DatabaseServerName = txtServerName.Text;
            _settings.Save();
        }

        private void pageReview_Initialize(object sender, WizardPageInitEventArgs e)
        {
            InitTaskList();
        }

        private void InitTaskList()
        {
            lstTasks.BeginUpdate();
            lstTasks.Items.Clear();
            foreach (IProjectBuilder builder in _builder.Tasks)
            {
                lstTasks.Items.Add(new TaskListViewItem(builder.Name, builder));
            }
            lstTasks.EndUpdate();
        }

        private void pageLocalDeploymentSettings_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            if (txtName.Text.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                AsMessageBox.ShowError(this, strings.NameContainsInvalidChars_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            if (string.IsNullOrEmpty(cboWebRoot.Text))
            {
                AsMessageBox.ShowError(this, strings.SelectWebRoot_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            if (string.IsNullOrEmpty(txtServerName.Text))
            {
                AsMessageBox.ShowError(this, strings.EnterDbServerName_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            if (!chkIntegratedAuthentication.Checked && string.IsNullOrEmpty(txtDatabaseUserName.Text))
            {
                AsMessageBox.ShowError(this, strings.EnterDbUserName_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            if (!chkIntegratedAuthentication.Checked && string.IsNullOrEmpty(txtDatabasePassword.Text))
            {
                AsMessageBox.ShowError(this, strings.EnterDbPassword_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }

            _project.Name = txtName.Text;
            _project.DatabaseServerName = txtServerName.Text;
            _project.DatabaseUserName = txtDatabaseUserName.Text;
            _project.DatabasePassword = txtDatabasePassword.Text;
            _project.DataDatabaseName = txtName.Text;
            _project.ModelDatabaseName = txtName.Text + "_model";
            _project.DatabaseIntegratedAuthentication = chkIntegratedAuthentication.Checked;
            _project.WebRootName = cboWebRoot.Text;
            _project.Url = txtName.Text;
            _project.ArchitectUserName = System.Threading.Thread.CurrentPrincipal.Identity.Name;
            _project.NewPackageId = Guid.NewGuid().ToString();
        }

        private void chkIntegratedAuthentication_CheckedChanged(object sender, EventArgs e)
        {
            txtDatabasePassword.Enabled = !chkIntegratedAuthentication.Checked;
            txtDatabaseUserName.Enabled = !chkIntegratedAuthentication.Checked;
            lblDatabaseUserName.Enabled = !chkIntegratedAuthentication.Checked;
            lblDatabasePassword.Enabled = !chkIntegratedAuthentication.Checked;
        }

        private void pageLocalDeploymentSettings_Initialize(object sender, WizardPageInitEventArgs e)
        {
            txtServerName.Text = _settings.DatabaseServerName;
            cboWebRoot.Items.Clear();
            cboWebRoot.Items.AddRange(_builder.WebSites());
            if(cboWebRoot.Items.Count > 0)
            {
                cboWebRoot.SelectedIndex = 0;
            }
        }

        private void pagePaths_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            if (string.IsNullOrEmpty(txtTemplateFolder.Text))
            {
                AsMessageBox.ShowError(this, strings.EnterTemplateFolder_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            if (string.IsNullOrEmpty(txtBinFolderRoot.Text))
            {
                AsMessageBox.ShowError(this, strings.EnterWebAppFolder_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            if (string.IsNullOrEmpty(txtSourcesFolder.Text))
            {
                AsMessageBox.ShowError(this, strings.EnterSourceFolder_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            DirectoryInfo dir = new DirectoryInfo(txtTemplateFolder.Text);
            if (!dir.Exists)
            {
                AsMessageBox.ShowError(this, strings.TemplateFolderNotExists_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            dir = new DirectoryInfo(txtBinFolderRoot.Text);
            if (!dir.Exists)
            {
                AsMessageBox.ShowError(this, strings.WebAppFolderNotExists_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            dir = new DirectoryInfo(txtSourcesFolder.Text);
            if (!dir.Exists)
            {
                AsMessageBox.ShowError(this, strings.SourceFolderNotExists_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            _project.ServerTemplateFolder = txtTemplateFolder.Text;
            _project.BinFolder = Path.Combine(txtBinFolderRoot.Text, txtName.Text);
            _project.SourcesFolder = Path.Combine(txtSourcesFolder.Text, txtName.Text);
        }

        private void btnSelectBinFolderRoot_Click(object sender, EventArgs e)
        {
            SelectFolder(txtBinFolderRoot);
        }

        private void btnSelectSourcesFolder_Click(object sender, EventArgs e)
        {
            SelectFolder(txtSourcesFolder);
        }

        private void btnSelectTemplateFolder_Click(object sender, EventArgs e)
        {
            SelectFolder(txtTemplateFolder);
        }

        private void SelectFolder(TextBox targetControl)
        {
            if(!string.IsNullOrEmpty(targetControl.Text))
            {
                folderBrowserDialog1.SelectedPath = targetControl.Text;
            }
            folderBrowserDialog1.ShowDialog(this);
            targetControl.Text = folderBrowserDialog1.SelectedPath;
            targetControl.Focus();
        }

        private void pagePaths_Initialize(object sender, WizardPageInitEventArgs e)
        {
            txtSourcesFolder.Text = _settings.SourcesFolder;
            txtBinFolderRoot.Text = _settings.BinFolder;
            txtTemplateFolder.Text = Path.Combine(Application.StartupPath, @"Project Templates\Default");
        }

        private void pageWelcome_Initialize(object sender, WizardPageInitEventArgs e)
        {
            if (! IsAdmin())
            {
                pageWelcome.AllowNext = false;
                lblAdminWarning.Visible = true;
                btnAdminElevate.Visible = true;
                AddShieldToButton(btnAdminElevate);
            }
        }

        private static bool IsAdmin()
        {
            WindowsIdentity id = WindowsIdentity.GetCurrent();
            WindowsPrincipal p = new WindowsPrincipal(id);
            return p.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private static void RestartElevated()
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.WorkingDirectory = Environment.CurrentDirectory;
            startInfo.FileName = Application.ExecutablePath;
            startInfo.Verb = "runas";
            try
            {
                Process p = Process.Start(startInfo);
            }
            catch (System.ComponentModel.Win32Exception ex)
            {
                return;
            }

            Application.Exit();
        }

        private static void AddShieldToButton(Button b)
        {
            b.FlatStyle = FlatStyle.System;
            SendMessage(b.Handle, BCM_SETSHIELD, 0, 0xFFFFFFFF);
        }

        private void btnAdminElevate_Click(object sender, EventArgs e)
        {
            RestartElevated();
        }

        private void pageDeploymentType_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            if (string.IsNullOrEmpty(txtName.Text))
            {
                AsMessageBox.ShowError(this, strings.EnterProjectName_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            if (string.IsNullOrEmpty(cboDeploymentType.Text))
            {
                AsMessageBox.ShowError(this, strings.SelectDeploymentType_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            switch (Deployment)
            {
                case DeploymentType.Local:
                    pageDeploymentType.NextPage = pageLocalDeploymentSettings;
                    break;
                case DeploymentType.Azure:
                    pageDeploymentType.NextPage = pageAzureDeploymentSettings;
                    break;
            }
        }

        private void pageDeploymentType_Initialize(object sender, WizardPageInitEventArgs e)
        {
            if (cboDeploymentType.SelectedIndex < 0)
            {
                cboDeploymentType.SelectedIndex = 0;
            }
        }

        private void pageAzureDeploymentSettings_Commit(object sender, WizardPageConfirmEventArgs e)
        {

        }
    }
}
