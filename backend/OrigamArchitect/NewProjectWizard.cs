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
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using AeroWizard;
using Origam;
using Origam.DA.Common.DatabasePlatform;
using Origam.Git;
using Origam.ProjectAutomation;
using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.Commands;
using Origam.Workbench.Services;

namespace OrigamArchitect;

public partial class NewProjectWizard : Form
{
    private readonly ProjectBuilder _builder = new();
    readonly Project _project = new();
    readonly NewProjectWizardSettings _settings = new();

    public NewProjectWizard()
    {
        InitializeComponent();
        InitGitConfig();
        wizard1.FinishButtonText = "Close";
        _project.DefaultModelPath = Path.Combine(
            path1: Application.StartupPath,
            path2: @"Project Templates\DefaultModel.zip"
        );
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
                _ => throw new ArgumentOutOfRangeException(
                    paramName: "DatabaseType",
                    actualValue: txtDatabaseType.SelectedIndex,
                    message: strings.UnknownDatabaseType
                ),
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
            _builder.Create(project: _project);
            WorkbenchSingleton.Workbench.Disconnect();
            WorkbenchSingleton.Workbench.Connect(configurationName: _project.Name);
            WorkbenchSchemaService schema =
                ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
                as WorkbenchSchemaService;
            schema.LoadSchema(schemaExtensionId: new Guid(g: _project.NewPackageId));
            ViewSchemaBrowserPad cmdViewBrowser = new ViewSchemaBrowserPad();
            cmdViewBrowser.Run();
        }
        catch (Exception ex)
        {
            e.Cancel = true;
            AsMessageBox.ShowError(
                owner: this,
                text: ex.Message,
                caption: strings.NewProjectFailed_Message,
                exception: ex
            );
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
        _settings.DatabaseTypeText = txtDatabaseType.GetItemText(
            item: txtDatabaseType.SelectedItem
        );
        _settings.Save();
    }

    private void pageReview_Initialize(object sender, WizardPageInitEventArgs e)
    {
        _builder.CreateTasks(project: _project);
        InitTaskList();
    }

    private void pageSuccess_Initialize(object sender, WizardPageInitEventArgs e)
    {
        finalMessageLabel.Text =
            $"The new project has been generated. Review the created env file and comand file for Linux: {Environment.NewLine}"
            + $"{_project.DockerEnvPathLinux}{Environment.NewLine}"
            + $"{_project.DockerCmdPathLinux}{Environment.NewLine}{Environment.NewLine}"
            + $"And the same files for windows if you prefer windows containers:{Environment.NewLine}"
            + $"{_project.DockerEnvPathWindows}{Environment.NewLine}"
            + $"{_project.DockerCmdPathWindows}{Environment.NewLine}{Environment.NewLine}"
            + $"The command files contain info about running the container with{Environment.NewLine}"
            + $"the application for each docker platform.";
    }

    private void InitTaskList()
    {
        lstTasks.BeginUpdate();
        lstTasks.Items.Clear();
        foreach (IProjectBuilder builder in _builder.Tasks)
        {
            lstTasks.Items.Add(value: new TaskListViewItem(name: builder.Name, builder: builder));
        }
        lstTasks.EndUpdate();
    }

    private void pageLocalDeploymentSettings_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        if (txtName.Text.IndexOfAny(anyOf: Path.GetInvalidFileNameChars()) >= 0)
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.NameContainsInvalidChars_Message,
                caption: strings.NewProjectWizard_Title,
                exception: null
            );
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(value: txtServerName.Text))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.EnterDbServerName_Message,
                caption: strings.NewProjectWizard_Title,
                exception: null
            );
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(value: txtDatabaseUserName.Text))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.EnterDbUserName_Message,
                caption: strings.NewProjectWizard_Title,
                exception: null
            );
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(value: txtDatabasePassword.Text))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.EnterDbPassword_Message,
                caption: strings.NewProjectWizard_Title,
                exception: null
            );
            e.Cancel = true;
            return;
        }
        if (!int.TryParse(s: txtPort.Text, result: out int port))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.PortError,
                caption: strings.NewProjectWizard_Title,
                exception: null
            );
            e.Cancel = true;
            return;
        }
        _project.Name = txtName.Text.ToLower().Replace(oldValue: "\\s+", newValue: "_");
        _project.DatabaseServerName = txtServerName.Text;
        _project.DatabaseUserName = txtDatabaseUserName.Text;
        _project.DatabasePassword = txtDatabasePassword.Text;
        _project.DataDatabaseName = txtName.Text.ToLower().Replace(oldValue: "\\s+", newValue: "_");
        _project.ModelDatabaseName =
            txtName.Text.ToLower().Replace(oldValue: "\\s+", newValue: "_") + "_model";
        _project.Url = txtName.Text;
        _project.ArchitectUserName = SecurityManager.CurrentPrincipal.Identity.Name;
        _project.DatabaseType = DatabaseType;
        _project.DatabasePort = port;
        _project.NewPackageId = Guid.NewGuid().ToString();
    }

    private void pageLocalDeploymentSettings_Initialize(object sender, WizardPageInitEventArgs e)
    {
        txtServerName.Text = string.IsNullOrEmpty(value: txtServerName.Text)
            ? _settings.DatabaseServerName
            : txtServerName.Text;
        if (txtDatabaseType.SelectedIndex == -1)
        {
            txtDatabaseType.SelectedItem = null;
            txtDatabaseType.Items.AddRange(
                items: new object[] { "Microsoft Sql Server", "Postgre Sql Server" }
            );
            txtDatabaseType.SelectedIndex = txtDatabaseType.FindStringExact(
                s: _settings.DatabaseTypeText
            );
        }
    }

    private void pageGit_Initialize(object sender, WizardPageInitEventArgs e)
    {
        txtSourcesFolder.Text = _settings.SourcesFolder;
    }

    private void btnSelectSourcesFolder_Click(object sender, EventArgs e)
    {
        SelectFolder(targetControl: txtSourcesFolder);
    }

    private void SelectFolder(TextBox targetControl)
    {
        if (!string.IsNullOrEmpty(value: targetControl.Text))
        {
            folderBrowserDialog1.SelectedPath = targetControl.Text;
        }
        folderBrowserDialog1.ShowDialog(owner: this);
        targetControl.Text = folderBrowserDialog1.SelectedPath;
        targetControl.Focus();
    }

    private void pageDeploymentType_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        if (string.IsNullOrEmpty(value: txtName.Text))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.EnterProjectName_Message,
                caption: strings.NewProjectWizard_Title,
                exception: null
            );
            e.Cancel = true;
            return;
        }
        if (!Regex.IsMatch(input: txtName.Text, pattern: @"^[a-zA-Z0-9]+$"))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: "Only alphanumeric characters are allowed.",
                caption: strings.NewProjectWizard_Title,
                exception: null
            );
            e.Cancel = true;
        }
    }

    private void PageGit_Commit(object sender, WizardPageConfirmEventArgs e)
    {
        if (string.IsNullOrEmpty(value: txtGitUser.Text) && gitrepo.Checked)
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.EnterUser_name,
                caption: strings.NewProjectWizard_Title,
                exception: null
            );
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(value: txtGitEmail.Text) && gitrepo.Checked)
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.EnterEmail_name,
                caption: strings.NewProjectWizard_Title,
                exception: null
            );
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(value: txtSourcesFolder.Text))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.EnterSourceFolder_Message,
                caption: strings.NewProjectWizard_Title,
                exception: null
            );
            e.Cancel = true;
            return;
        }
        DirectoryInfo dir = new DirectoryInfo(path: txtSourcesFolder.Text);
        if (!dir.Exists)
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.SourceFolderNotExists_Message,
                caption: strings.NewProjectWizard_Title,
                exception: null
            );
            e.Cancel = true;
            return;
        }
        _project.SourcesFolder = Path.Combine(path1: txtSourcesFolder.Text, path2: txtName.Text);
        _project.ModelSourceFolder = Path.Combine(
            path1: txtSourcesFolder.Text,
            path2: txtName.Text,
            path3: "model"
        );
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
        if (string.IsNullOrEmpty(value: txtWebUserLoginName.Text))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.EnterWebUserName_Message,
                caption: "Template",
                exception: null
            );
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(value: txtWebUserPassword.Text))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.EnterWebPassword_Message,
                caption: "Template",
                exception: null
            );
            e.Cancel = true;
            return;
        }
        if (!txtWebUserPasswordConfirmed.Text.Equals(value: txtWebUserPassword.Text))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.WebPasswordNotMatch_Message,
                caption: "Template",
                exception: null
            );
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(value: txtWebFirstname.Text))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.EnterWebFirstName_Message,
                caption: "Template",
                exception: null
            );
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(value: txtWebSurname.Text))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.EnterWebSurname_Message,
                caption: "Template",
                exception: null
            );
            e.Cancel = true;
            return;
        }
        if (string.IsNullOrEmpty(value: txtWebEmail.Text))
        {
            AsMessageBox.ShowError(
                owner: this,
                text: strings.EnterWebEmail_Message,
                caption: "Template",
                exception: null
            );
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
