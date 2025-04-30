#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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
using Origam.Git;
using static Origam.DA.Common.Enums;
using Origam;
using System.Text.RegularExpressions;


namespace OrigamArchitect;
public partial class NewProjectWizard : Form
{
    private readonly ProjectBuilder _builder = new ();
    readonly Project _project = new ();
    readonly NewProjectWizardSettings _settings = new ();
    public NewProjectWizard()
    {
        InitializeComponent();
        InitGitConfig();
        wizard1.FinishButtonText = "Close";
        _project.DefaultModelPath = Path.Combine(
            Application.StartupPath,
            @"Project Templates\DefaultModel.zip");
    }
    private void InitGitConfig()
    {
        string[] credentials = new GitManager().GitConfig();
        if (credentials != null)
        {
            txtGitUser.Text = credentials[0];
            txtGitEmail.Text = credentials[1];
        }
    }
    private DatabaseType DatabaseType
    {
        get
        {
            return txtDatabaseType.SelectedIndex switch
            {
                0 => DatabaseType.MsSql,
                1 => DatabaseType.PgSql,
                _ => throw new ArgumentOutOfRangeException("DatabaseType",
                    txtDatabaseType.SelectedIndex, strings.UnknownDatabaseType)
            };
        }
    }
    private void PageReview_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        pageReview.AllowNext = false;
        SaveSettings();
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
            schema.LoadSchema(new Guid(_project.NewPackageId));
            ViewSchemaBrowserPad cmdViewBrowser = new ViewSchemaBrowserPad();
            cmdViewBrowser.Run();
        }
        catch (Exception ex)
        {
            e.Cancel = true;
            AsMessageBox.ShowError(this, ex.Message, strings.NewProjectFailed_Message, ex);
            WorkbenchSingleton.Workbench.Disconnect();
            pageReview.AllowNext = true;
        }
        finally
        {
            WorkbenchSingleton.Workbench.PopulateEmptyDatabaseOnLoad = true;
        }
    }
    private void SaveSettings()
    {
        _settings.SourcesFolder = txtSourcesFolder.Text;
        _settings.DatabaseServerName = txtServerName.Text;
        _settings.DatabaseTypeText = txtDatabaseType.GetItemText(txtDatabaseType.SelectedItem);
        _settings.Save();
    }
    private void pageReview_Initialize(object sender, WizardPageInitEventArgs e)
    {
        _builder.CreateTasks(_project);
        InitTaskList();
    }    
    private void pageSuccess_Initialize(object sender, WizardPageInitEventArgs e)
    {
        finalMessageLabel.Text =
            $"The new project has been generated. Review the created env file and comand file for Linux: {Environment.NewLine}" +
            $"{_project.DockerEnvPathLinux}{Environment.NewLine}" +
            $"{_project.DockerCmdPathLinux}{Environment.NewLine}{Environment.NewLine}" +
            $"And the same files for windows if you prefer windows containers:{Environment.NewLine}" +
            $"{_project.DockerEnvPathWindows}{Environment.NewLine}" +
            $"{_project.DockerCmdPathWindows}{Environment.NewLine}{Environment.NewLine}" +
            $"The command files contain info about running the container with{Environment.NewLine}" +
            $"the application for each docker platform.";
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
        if (string.IsNullOrEmpty(txtServerName.Text))
        {
            AsMessageBox.ShowError(this, strings.EnterDbServerName_Message, strings.NewProjectWizard_Title, null);
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(txtDatabaseUserName.Text))
        {
            AsMessageBox.ShowError(this, strings.EnterDbUserName_Message, strings.NewProjectWizard_Title, null);
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(txtDatabasePassword.Text))
        {
            AsMessageBox.ShowError(this, strings.EnterDbPassword_Message, strings.NewProjectWizard_Title, null);
            e.Cancel = true;
            return;
        }
        if (!int.TryParse(txtPort.Text, out int port))
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
        _project.Url = txtName.Text;
        _project.ArchitectUserName = SecurityManager.CurrentPrincipal.Identity.Name;
        _project.DatabaseType = DatabaseType;
        _project.DatabasePort = port;
        _project.NewPackageId = Guid.NewGuid().ToString();
    }
    private void pageLocalDeploymentSettings_Initialize(object sender, WizardPageInitEventArgs e)
    {
        txtServerName.Text = string.IsNullOrEmpty(txtServerName.Text) ? _settings.DatabaseServerName : txtServerName.Text;
        if (txtDatabaseType.SelectedIndex == -1)
        {
            txtDatabaseType.SelectedItem = null;
            txtDatabaseType.Items.AddRange(new object[] {
                "Microsoft Sql Server",
                "Postgre Sql Server"});
            txtDatabaseType.SelectedIndex = txtDatabaseType.FindStringExact(_settings.DatabaseTypeText);
        }
    }
    
    private void pageGit_Initialize(object sender, WizardPageInitEventArgs e)
    {
        txtSourcesFolder.Text = _settings.SourcesFolder;
    }
    
    private void btnSelectSourcesFolder_Click(object sender, EventArgs e)
    {
        SelectFolder(txtSourcesFolder);
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

    private void pageDeploymentType_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        if (string.IsNullOrEmpty(txtName.Text))
        {
            AsMessageBox.ShowError(this, strings.EnterProjectName_Message, strings.NewProjectWizard_Title, null);
            e.Cancel = true;
            return;
        }
        if (!Regex.IsMatch(txtName.Text, @"^[a-zA-Z0-9]+$"))
        {
            AsMessageBox.ShowError(this, "Only alphanumeric characters are allowed.", strings.NewProjectWizard_Title, null);
            e.Cancel = true;
        }
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
        _project.ModelSourceFolder = Path.Combine(txtSourcesFolder.Text, txtName.Text,"model");
        _project.RootSourceFolder = txtSourcesFolder.Text;
        _project.GitRepository = gitrepo.Checked;
        _project.GitUsername = txtGitUser.Text;
        _project.GitEmail = txtGitEmail.Text;
    }
    private void Gitrepo_CheckedChanged(object sender, EventArgs e)
    {
        txtGitUser.Enabled = gitrepo.Checked;
        txtGitEmail.Enabled = gitrepo.Checked;
    }
    private void pageWebUser_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        if (string.IsNullOrEmpty(txtWebUserLoginName.Text))
        {
            AsMessageBox.ShowError(this, strings.EnterWebUserName_Message, "Template", null);
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(txtWebUserPassword.Text))
        {
            AsMessageBox.ShowError(this, strings.EnterWebPassword_Message, "Template", null);
            e.Cancel = true;
            return;
        }
        if (!txtWebUserPasswordConfirmed.Text.Equals(txtWebUserPassword.Text))
        {
            AsMessageBox.ShowError(this, strings.WebPasswordNotMatch_Message, "Template", null);
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(txtWebFirstname.Text))
        {
            AsMessageBox.ShowError(this, strings.EnterWebFirstName_Message, "Template", null);
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(txtWebSurname.Text))
        {
            AsMessageBox.ShowError(this, strings.EnterWebSurname_Message, "Template", null);
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(txtWebEmail.Text))
        {
            AsMessageBox.ShowError(this, strings.EnterWebEmail_Message, "Template", null);
            e.Cancel = true;
            return;
        }
        _project.WebUserName = txtWebUserLoginName.Text;
        _project.WebUserPassword = txtWebUserPassword.Text;
        _project.WebFirstName = txtWebFirstname.Text;
        _project.WebSurname = txtWebSurname.Text;
        _project.WebEmail = txtWebEmail.Text;
    }
}
