#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using Origam.Git;
using static Origam.DA.Common.Enums;
using Origam;
using System.Xml;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Threading;
using System.Text.RegularExpressions;

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
        private WebGitXmlParser XmlParser = new WebGitXmlParser();
        private List<object[]> repositories = new List<object[]>();

        public NewProjectWizard()
        {
            InitializeComponent();
            InitGitConfig();
            wizard1.FinishButtonText = "Run";
        }

        private void InitGitConfig()
        {
            string[] creditials = new GitManager().GitConfig();
            if (creditials != null)
            {
                txtGitUser.Text = creditials[0];
                txtGitEmail.Text = creditials[1];
            }
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

        private DatabaseType DatabaseType
        {
            get
            {
                switch (txtDatabaseType.SelectedIndex)
                {
                    case 0:
                        return DatabaseType.MsSql;
                    case 1:
                        return DatabaseType.PgSql;
                    default:
                        throw new ArgumentOutOfRangeException("DatabaseType",
                            txtDatabaseType.SelectedIndex, strings.UnknownDatabaseType);

                }
            }
        }

        private void PageReview_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            _builder.CreateTasks(_project);
            InitTaskList();
            WorkbenchSingleton.Workbench.Disconnect();
            WorkbenchSingleton.Workbench.PopulateEmptyDatabaseOnLoad = false;
            Application.DoEvents();
            try
            {
                _builder.Create(_project);
                WorkbenchSingleton.Workbench.Disconnect();
                WorkbenchSingleton.Workbench.Connect(_project.Name);
                WorkbenchSchemaService schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService))
                   as WorkbenchSchemaService;
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
            _settings.DatabaseTypeText = txtDatabaseType.GetItemText(txtDatabaseType.SelectedItem);
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
            if (!int.TryParse(txtPort.Text, out int Port))
            {
                AsMessageBox.ShowError(this, strings.PortError, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }

            _project.Name = txtName.Text.ToLower().Replace("\\s+", "_");
            _project.DatabaseServerName = txtServerName.Text;
            _project.DatabaseUserName = txtDatabaseUserName.Text;
            _project.DatabasePassword = txtDatabasePassword.Text;
            _project.DataDatabaseName = txtName.Text.ToLower().Replace("\\s+", "_");
            _project.ModelDatabaseName = txtName.Text.ToLower().Replace("\\s+", "_") + "_model";
            _project.DatabaseIntegratedAuthentication = chkIntegratedAuthentication.Checked;
            _project.WebRootName = cboWebRoot.Text;
            _project.Url = txtName.Text;
            _project.ArchitectUserName = System.Threading.Thread.CurrentPrincipal.Identity.Name;

            _project.DatabaseType = DatabaseType;
            _project.Port = Port;
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
            txtServerName.Text = string.IsNullOrEmpty(txtServerName.Text) ? _settings.DatabaseServerName : txtServerName.Text;
            cboWebRoot.Items.Clear();
            cboWebRoot.Items.AddRange(_builder.WebSites());
            if (cboWebRoot.Items.Count > 0)
            {
                cboWebRoot.SelectedIndex = 0;
            }
            if (txtDatabaseType.SelectedIndex == -1)
            {
                txtDatabaseType.SelectedItem = null;
                txtDatabaseType.Items.AddRange(new object[] {
                    "Microsoft Sql Server",
                    "Postgre Sql Server"});
                txtDatabaseType.SelectedIndex = txtDatabaseType.FindStringExact(_settings.DatabaseTypeText);
            }
            TxtDatabaseType_SelectedIndexChanged(null, EventArgs.Empty);
        }

        private void SetPort()
        {
            if (DatabaseType == DatabaseType.MsSql)
            {
                txtPort.Text = "0";
            }
            if (DatabaseType == DatabaseType.PgSql)
            {
                txtPort.Text = "5432";
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
            _project.ServerTemplateFolder = txtTemplateFolder.Text;
            _project.BinFolder = Path.Combine(txtBinFolderRoot.Text, txtName.Text);
            _project.GitRepository = gitrepo.Checked;
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
            if (!string.IsNullOrEmpty(targetControl.Text))
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

        private void PageWelcome_Initialize(object sender, WizardPageInitEventArgs e)
        {
            if (!IsAdmin())
            {
                pageWelcome.AllowNext = false;
                lblAdminWarning.Visible = true;
                btnAdminElevate.Visible = true;
                AddShieldToButton(btnAdminElevate);
            }
            else
            {
                InitTemplates();
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
            catch
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
                    pageDeploymentType.NextPage = pageTemplateType;
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

        private void PageGit_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            if (string.IsNullOrEmpty(txtGitUser.Text) && gitrepo.Checked)
            {
                AsMessageBox.ShowError(this, strings.EnterUser_name, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }

            if (string.IsNullOrEmpty(txtGitEmail.Text) && gitrepo.Checked)
            {
                AsMessageBox.ShowError(this, strings.EnterEmail_name, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }

            if (string.IsNullOrEmpty(txtSourcesFolder.Text))
            {
                AsMessageBox.ShowError(this, strings.EnterSourceFolder_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            DirectoryInfo dir = new DirectoryInfo(txtSourcesFolder.Text);
            if (!dir.Exists)
            {
                AsMessageBox.ShowError(this, strings.SourceFolderNotExists_Message, strings.NewProjectWizard_Title, null);
                e.Cancel = true;
                return;
            }
            _project.SourcesFolder = Path.Combine(txtSourcesFolder.Text, txtName.Text);
            _project.GitRepository = gitrepo.Checked;
            _project.Gitusername = txtGitUser.Text;
            _project.Gitemail = txtGitEmail.Text;
        }

        private void Gitrepo_CheckedChanged(object sender, EventArgs e)
        {
            txtGitUser.Enabled = gitrepo.Checked;
            txtGitEmail.Enabled = gitrepo.Checked;
        }

        private void TxtDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (DatabaseType == DatabaseType.PgSql)
            {
                chkIntegratedAuthentication.Enabled = false;
                chkIntegratedAuthentication.Checked = false;
                txtServerName.Width = txtDatabaseType.Width;
                txtPort.Visible = true;
                labelPort.Visible = true;
                chkIntegratedAuthentication.Visible = false;
                IntegratedLabel.Visible = false;
            }
            else
            {
                chkIntegratedAuthentication.Enabled = true;
                txtServerName.Width = cboWebRoot.Width;
                txtPort.Visible = false;
                labelPort.Visible = false;
                chkIntegratedAuthentication.Visible = true;
                IntegratedLabel.Visible = true;
            }
            SetPort();
        }

        private void InitTemplates()
        {
            Thread bgThread = new Thread(FillTemplate)
            {
                IsBackground = true
            };
            bgThread.Start();
        }

        private void PageTemplateType_Initialize(object sender, WizardPageInitEventArgs e)
        {
            WaitForLoaded();
            ImageList imageList = new ImageList
            {
                ImageSize = new Size(64, 64)
            };
            imageList.Images.Add(Images.New);
            foreach (var repo in repositories)
            {
                Image img = (Image)repo[0];
                imageList.Images.Add(img);
            }
            listViewTemplate.LargeImageList = imageList;

            if (listViewTemplate.Items.Count == 0)
            {
                ListViewItem defaultviewItem = new ListViewItem { ImageIndex = 0, Text = "Empty Template" };
                defaultviewItem.Tag = new object[] { null, "Empty Template.", TypeTemplate.Default };
                listViewTemplate.Items.Add(defaultviewItem);
                ListViewItem CopyviewItem = new ListViewItem { ImageIndex = 0, Text = "Copy Template" };
                CopyviewItem.Tag = new object[] { null, "My Template.", TypeTemplate.Open };
                listViewTemplate.Items.Add(CopyviewItem);
            }
            int imgindex = 1;
            foreach (var repo in repositories)
            {
                string nameOfRepository = (string)repo[1];
                string linkToGit = (string)repo[2];
                string readmeText = (string)repo[3];
                ListViewItem viewItem = new ListViewItem { ImageIndex = imgindex, Text = nameOfRepository };
                viewItem.Tag = new object[] { linkToGit, readmeText, TypeTemplate.Template };
                listViewTemplate.Items.Add(viewItem);
                imgindex++;
            }
        }

        private void WaitForLoaded()
        {
            Cursor.Current = Cursors.WaitCursor;
            for (int i = 0; i < 10; i++)
            {
                if (!XmlParser.IsLoaded)
                {
                    Thread.Sleep(1500);
                }
                else
                {
                    break;
                }
            }
            Cursor.Current = Cursors.Default;
        }

        private void FillTemplate()
        {
            repositories = XmlParser.GetList();
        }

        private void PageTemplateType_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            if (listViewTemplate.SelectedItems.Count == 0)
            {
                AsMessageBox.ShowError(this, strings.TemplateNotSelect, "Template", null);
                e.Cancel = true;
                return;
            }

            ListView.SelectedListViewItemCollection selectedListView =
                    listViewTemplate.SelectedItems;
            foreach (ListViewItem item in selectedListView)
            {
                object[] tags = (object[])item.Tag;
                _project.GitRepositoryLink = (string)tags[0];
                _project.TypeTemplate = (TypeTemplate)tags[2];
            }
            switch (_project.TypeTemplate)
            {
                case TypeTemplate.Open:
                    pageTemplateType.NextPage = wizOpenRepository;
                    break;
                case TypeTemplate.Default:
                case TypeTemplate.Template:
                    pageTemplateType.NextPage = pageLocalDeploymentSettings;
                    break;
            }
        }

        private void ListViewTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            MarkdownSharp.Markdown md = new MarkdownSharp.Markdown();
            ListView.SelectedListViewItemCollection selectedListView =
                     listViewTemplate.SelectedItems;
            foreach (ListViewItem item in selectedListView)
            {
                object[] tags = (object[])item.Tag;
                wbReadmeText.DocumentText = md.Transform((string)tags[1]);
            }
        }

        private void WizOpenRepository_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            if (string.IsNullOrEmpty(tbRepositoryLink.Text))
            {
                AsMessageBox.ShowError(this, strings.EnterTemplateFolder_Message, "Template", null);
                e.Cancel = true;
                return;
            }

            if (!TestTemplate())
            {
                AsMessageBox.ShowError(this, "Cant i get data from Repository Link!", "Template", null);
                e.Cancel = true;
                return;
            }
            _project.GitRepositoryLink = CreateCredentialGitUrl(tbRepositoryLink.Text, tbRepUsername.Text, tbRepPassword.Text);
            if (rdCopy.Checked) _project.TypeDoTemplate = TypeDoTemplate.Copy;
            if (rdClone.Checked) _project.TypeDoTemplate = TypeDoTemplate.Clone;
        }

        public bool TestTemplate()
        {
            string gitPassw = tbRepPassword.Text;
            string gitUsername = tbRepUsername.Text;
            string url = CreateCredentialGitUrl(tbRepositoryLink.Text, gitUsername, gitPassw);
            GitManager gitManager = new GitManager();
            return gitManager.IsValidUrl(url);
        }

        private string CreateCredentialGitUrl(string url, string tbRepUsername, string tbRepPassword)
        {
            if (!string.IsNullOrEmpty(tbRepUsername) && !string.IsNullOrEmpty(tbRepPassword))
            {
                string credentials = string.Format("{0}:{1}", tbRepUsername, tbRepPassword);
                if (url.Contains("@"))
                {
                    url = Regex.Replace(url, "://.*@", "://" + credentials + "@");
                }
                else
                {
                    url.Replace("://", "://" + credentials);
                }
            }
            return url;
        }

        private void WizOpenRepository_Initialize(object sender, WizardPageConfirmEventArgs e)
        {
            if(!rdClone.Checked && !rdClone.Checked)
            {
                rdCopy.Checked = true;
            }
        }
    }
}
