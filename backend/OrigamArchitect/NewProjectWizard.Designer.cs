using System;
using AeroWizard;

namespace OrigamArchitect
{
    partial class NewProjectWizard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewProjectWizard));
            this.wizard1 = new AeroWizard.WizardControl();
            this.pageWelcome = new AeroWizard.WizardPage();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblWelcome1 = new System.Windows.Forms.Label();
            this.pageDeploymentType = new AeroWizard.WizardPage();
            this.label1 = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.pagePaths = new AeroWizard.WizardPage();
            this.btnSelectTemplateFolder = new System.Windows.Forms.Button();
            this.btnSelectBinFolderRoot = new System.Windows.Forms.Button();
            this.lblTemplateFolderDescription = new System.Windows.Forms.Label();
            this.lblTemplateFolder = new System.Windows.Forms.Label();
            this.defaultModelPath = new System.Windows.Forms.TextBox();
            this.lblBinFolderRootDescription = new System.Windows.Forms.Label();
            this.lblBinFolderRoot = new System.Windows.Forms.Label();
            this.txtBinFolderRoot = new System.Windows.Forms.TextBox();
            this.pageWebUser = new AeroWizard.WizardPage();
            this.label30 = new System.Windows.Forms.Label();
            this.txtWebUserPasswordConfirmed = new System.Windows.Forms.TextBox();
            this.label29 = new System.Windows.Forms.Label();
            this.label28 = new System.Windows.Forms.Label();
            this.label27 = new System.Windows.Forms.Label();
            this.txtWebEmail = new System.Windows.Forms.TextBox();
            this.txtWebSurname = new System.Windows.Forms.TextBox();
            this.txtWebFirstname = new System.Windows.Forms.TextBox();
            this.txtWebUserPassword = new System.Windows.Forms.TextBox();
            this.txtWebUserLoginName = new System.Windows.Forms.TextBox();
            this.label26 = new System.Windows.Forms.Label();
            this.label25 = new System.Windows.Forms.Label();
            this.label24 = new System.Windows.Forms.Label();
            this.pageGit = new AeroWizard.WizardPage();
            this.dockerSourceFolderLabel = new System.Windows.Forms.Label();
            this.txtdosourcefolder = new System.Windows.Forms.TextBox();
            this.lblgitemail = new System.Windows.Forms.Label();
            this.lblgituser = new System.Windows.Forms.Label();
            this.txtGitEmail = new System.Windows.Forms.TextBox();
            this.txtGitUser = new System.Windows.Forms.TextBox();
            this.lblSourcesFolderDescription = new System.Windows.Forms.Label();
            this.lblSourcesFolder = new System.Windows.Forms.Label();
            this.txtSourcesFolder = new System.Windows.Forms.TextBox();
            this.btnSelectSourcesFolder = new System.Windows.Forms.Button();
            this.label15 = new System.Windows.Forms.Label();
            this.gitrepo = new System.Windows.Forms.CheckBox();
            this.pageDocker = new AeroWizard.WizardPage();
            this.label23 = new System.Windows.Forms.Label();
            this.label22 = new System.Windows.Forms.Label();
            this.dockerPortLabel = new System.Windows.Forms.Label();
            this.txtDockerPort = new System.Windows.Forms.TextBox();
            this.label21 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.pageReview = new AeroWizard.WizardPage();
            this.lstTasks = new System.Windows.Forms.ListView();
            this.colText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.wizOpenRepository = new AeroWizard.WizardPage();
            this.rdNone = new System.Windows.Forms.RadioButton();
            this.rdCopy = new System.Windows.Forms.RadioButton();
            this.rdClone = new System.Windows.Forms.RadioButton();
            this.label18 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbRepPassword = new System.Windows.Forms.TextBox();
            this.tbRepUsername = new System.Windows.Forms.TextBox();
            this.tbRepositoryLink = new System.Windows.Forms.TextBox();
            this.pageLocalDeploymentSettings = new AeroWizard.WizardPage();
            this.labelPort = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.labelPrivileges = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.txtDatabaseType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.cboWebRoot = new System.Windows.Forms.ComboBox();
            this.lblDatabasePassword = new System.Windows.Forms.Label();
            this.txtDatabasePassword = new System.Windows.Forms.TextBox();
            this.lblWebRoot = new System.Windows.Forms.Label();
            this.lblDatabaseUserName = new System.Windows.Forms.Label();
            this.txtServerName = new System.Windows.Forms.TextBox();
            this.lblServerName = new System.Windows.Forms.Label();
            this.txtDatabaseUserName = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.projectBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.projectBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.wizard1)).BeginInit();
            this.pageWelcome.SuspendLayout();
            this.pageDeploymentType.SuspendLayout();
            this.pagePaths.SuspendLayout();
            this.pageWebUser.SuspendLayout();
            this.pageGit.SuspendLayout();
            this.pageDocker.SuspendLayout();
            this.pageReview.SuspendLayout();
            this.wizOpenRepository.SuspendLayout();
            this.pageLocalDeploymentSettings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.projectBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.projectBindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // wizard1
            // 
            this.wizard1.Location = new System.Drawing.Point(0, 0);
            this.wizard1.Name = "wizard1";
            this.wizard1.Pages.Add(this.pageWelcome);
            this.wizard1.Pages.Add(this.pageDeploymentType);
            this.wizard1.Pages.Add(this.wizOpenRepository);
            this.wizard1.Pages.Add(this.pageLocalDeploymentSettings);
            this.wizard1.Pages.Add(this.pagePaths);
            this.wizard1.Pages.Add(this.pageWebUser);
            this.wizard1.Pages.Add(this.pageGit);
            this.wizard1.Pages.Add(this.pageDocker);
            this.wizard1.Pages.Add(this.pageReview);
            this.wizard1.Size = new System.Drawing.Size(784, 561);
            this.wizard1.TabIndex = 0;
            this.wizard1.Title = "New Project";
            this.wizard1.TitleIcon = ((System.Drawing.Icon)(resources.GetObject("wizard1.TitleIcon")));
            // 
            // pageWelcome
            // 
            this.pageWelcome.Controls.Add(this.label9);
            this.pageWelcome.Controls.Add(this.label8);
            this.pageWelcome.Controls.Add(this.label7);
            this.pageWelcome.Controls.Add(this.label6);
            this.pageWelcome.Controls.Add(this.lblWelcome1);
            this.pageWelcome.Name = "pageWelcome";
            this.pageWelcome.Size = new System.Drawing.Size(737, 407);
            this.pageWelcome.TabIndex = 3;
            this.pageWelcome.Text = "Welcome To New Project Wizard";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(23, 121);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(202, 15);
            this.label9.TabIndex = 6;
            this.label9.Text = "– Configure the source control folder";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(23, 97);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(117, 15);
            this.label8.TabIndex = 5;
            this.label8.Text = "– Set up a web server";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(23, 73);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(173, 15);
            this.label7.TabIndex = 4;
            this.label7.Text = "– Create and populate database";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(23, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(180, 15);
            this.label6.TabIndex = 3;
            this.label6.Text = "– Add an Architect configuration";
            // 
            // lblWelcome1
            // 
            this.lblWelcome1.Location = new System.Drawing.Point(-3, 13);
            this.lblWelcome1.Name = "lblWelcome1";
            this.lblWelcome1.Size = new System.Drawing.Size(574, 36);
            this.lblWelcome1.TabIndex = 1;
            this.lblWelcome1.Text = "This wizard will help you with multiple steps that are required in order to go th" +
    "rough the following tasks:";
            // 
            // pageDeploymentType
            // 
            this.pageDeploymentType.Controls.Add(this.label1);
            this.pageDeploymentType.Controls.Add(this.lblName);
            this.pageDeploymentType.Controls.Add(this.txtName);
            this.pageDeploymentType.Name = "pageDeploymentType";
            this.pageDeploymentType.Size = new System.Drawing.Size(737, 407);
            this.pageDeploymentType.TabIndex = 4;
            this.pageDeploymentType.Text = "Deployment Type";
            this.pageDeploymentType.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageDeploymentType_Commit);
            // 
            // label1
            // 
            this.label1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label1.Location = new System.Drawing.Point(161, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(370, 39);
            this.label1.TabIndex = 2;
            this.label1.Text = "Enter the name of your project. It will be used to set a name of the package, dat" +
    "abase, URL and folders. Example: MyNewProject";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.World);
            this.lblName.Location = new System.Drawing.Point(4, 17);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(40, 15);
            this.lblName.TabIndex = 14;
            this.lblName.Text = "Name";
            // 
            // txtName
            // 
            this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.World);
            this.txtName.Location = new System.Drawing.Point(161, 14);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(559, 23);
            this.txtName.TabIndex = 1;
            // 
            // pagePaths
            // 
            this.pagePaths.Controls.Add(this.btnSelectTemplateFolder);
            this.pagePaths.Controls.Add(this.btnSelectBinFolderRoot);
            this.pagePaths.Controls.Add(this.lblTemplateFolderDescription);
            this.pagePaths.Controls.Add(this.lblTemplateFolder);
            this.pagePaths.Controls.Add(this.defaultModelPath);
            this.pagePaths.Controls.Add(this.lblBinFolderRootDescription);
            this.pagePaths.Controls.Add(this.lblBinFolderRoot);
            this.pagePaths.Controls.Add(this.txtBinFolderRoot);
            this.pagePaths.Name = "pagePaths";
            this.pagePaths.NextPage = this.pageWebUser;
            this.pagePaths.Size = new System.Drawing.Size(737, 407);
            this.pagePaths.TabIndex = 2;
            this.pagePaths.Text = "Paths";
            this.pagePaths.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pagePaths_Commit);
            this.pagePaths.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pagePaths_Initialize);
            // 
            // btnSelectTemplateFolder
            // 
            this.btnSelectTemplateFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectTemplateFolder.Location = new System.Drawing.Point(701, 110);
            this.btnSelectTemplateFolder.Name = "btnSelectTemplateFolder";
            this.btnSelectTemplateFolder.Size = new System.Drawing.Size(25, 23);
            this.btnSelectTemplateFolder.TabIndex = 11;
            this.btnSelectTemplateFolder.Text = "...";
            this.btnSelectTemplateFolder.UseVisualStyleBackColor = true;
            this.btnSelectTemplateFolder.Click += new System.EventHandler(this.btnSelectTemplateFolder_Click);
            // 
            // btnSelectBinFolderRoot
            // 
            this.btnSelectBinFolderRoot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectBinFolderRoot.Location = new System.Drawing.Point(701, 23);
            this.btnSelectBinFolderRoot.Name = "btnSelectBinFolderRoot";
            this.btnSelectBinFolderRoot.Size = new System.Drawing.Size(25, 23);
            this.btnSelectBinFolderRoot.TabIndex = 9;
            this.btnSelectBinFolderRoot.Text = "...";
            this.btnSelectBinFolderRoot.UseVisualStyleBackColor = true;
            this.btnSelectBinFolderRoot.Click += new System.EventHandler(this.btnSelectBinFolderRoot_Click);
            // 
            // lblTemplateFolderDescription
            // 
            this.lblTemplateFolderDescription.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblTemplateFolderDescription.Location = new System.Drawing.Point(161, 140);
            this.lblTemplateFolderDescription.Name = "lblTemplateFolderDescription";
            this.lblTemplateFolderDescription.Size = new System.Drawing.Size(370, 41);
            this.lblTemplateFolderDescription.TabIndex = 8;
            this.lblTemplateFolderDescription.Text = "Enter a path to DefaultModel.zip file which contains the initial model files.";
            // 
            // lblTemplateFolder
            // 
            this.lblTemplateFolder.AutoSize = true;
            this.lblTemplateFolder.Location = new System.Drawing.Point(4, 114);
            this.lblTemplateFolder.Name = "lblTemplateFolder";
            this.lblTemplateFolder.Size = new System.Drawing.Size(82, 15);
            this.lblTemplateFolder.TabIndex = 7;
            this.lblTemplateFolder.Text = "Default Model";
            // 
            // defaultModelPath
            // 
            this.defaultModelPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.defaultModelPath.Location = new System.Drawing.Point(161, 110);
            this.defaultModelPath.Name = "defaultModelPath";
            this.defaultModelPath.Size = new System.Drawing.Size(533, 23);
            this.defaultModelPath.TabIndex = 6;
            // 
            // lblBinFolderRootDescription
            // 
            this.lblBinFolderRootDescription.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblBinFolderRootDescription.Location = new System.Drawing.Point(161, 53);
            this.lblBinFolderRootDescription.Name = "lblBinFolderRootDescription";
            this.lblBinFolderRootDescription.Size = new System.Drawing.Size(370, 39);
            this.lblBinFolderRootDescription.TabIndex = 2;
            this.lblBinFolderRootDescription.Text = "The web application\'s binary files will be copied here. A new folder will be crea" +
    "ted for your application.";
            // 
            // lblBinFolderRoot
            // 
            this.lblBinFolderRoot.AutoSize = true;
            this.lblBinFolderRoot.Location = new System.Drawing.Point(4, 26);
            this.lblBinFolderRoot.Name = "lblBinFolderRoot";
            this.lblBinFolderRoot.Size = new System.Drawing.Size(131, 15);
            this.lblBinFolderRoot.TabIndex = 1;
            this.lblBinFolderRoot.Text = "Web Application Folder";
            // 
            // txtBinFolderRoot
            // 
            this.txtBinFolderRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBinFolderRoot.Location = new System.Drawing.Point(161, 23);
            this.txtBinFolderRoot.Name = "txtBinFolderRoot";
            this.txtBinFolderRoot.Size = new System.Drawing.Size(533, 23);
            this.txtBinFolderRoot.TabIndex = 0;
            // 
            // pageWebUser
            // 
            this.pageWebUser.Controls.Add(this.label30);
            this.pageWebUser.Controls.Add(this.txtWebUserPasswordConfirmed);
            this.pageWebUser.Controls.Add(this.label29);
            this.pageWebUser.Controls.Add(this.label28);
            this.pageWebUser.Controls.Add(this.label27);
            this.pageWebUser.Controls.Add(this.txtWebEmail);
            this.pageWebUser.Controls.Add(this.txtWebSurname);
            this.pageWebUser.Controls.Add(this.txtWebFirstname);
            this.pageWebUser.Controls.Add(this.txtWebUserPassword);
            this.pageWebUser.Controls.Add(this.txtWebUserLoginName);
            this.pageWebUser.Controls.Add(this.label26);
            this.pageWebUser.Controls.Add(this.label25);
            this.pageWebUser.Controls.Add(this.label24);
            this.pageWebUser.Name = "pageWebUser";
            this.pageWebUser.NextPage = this.pageGit;
            this.pageWebUser.Size = new System.Drawing.Size(737, 407);
            this.pageWebUser.TabIndex = 10;
            this.pageWebUser.Text = "Create New Web User";
            this.pageWebUser.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageWebUser_Commit);
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(21, 144);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(104, 15);
            this.label30.TabIndex = 26;
            this.label30.Text = "Confirm Password";
            // 
            // txtWebUserPasswordConfirmed
            // 
            this.txtWebUserPasswordConfirmed.Location = new System.Drawing.Point(142, 141);
            this.txtWebUserPasswordConfirmed.Name = "txtWebUserPasswordConfirmed";
            this.txtWebUserPasswordConfirmed.PasswordChar = '*';
            this.txtWebUserPasswordConfirmed.Size = new System.Drawing.Size(174, 23);
            this.txtWebUserPasswordConfirmed.TabIndex = 20;
            this.txtWebUserPasswordConfirmed.UseSystemPasswordChar = true;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(21, 267);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(36, 15);
            this.label29.TabIndex = 24;
            this.label29.Text = "Email";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(21, 233);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(54, 15);
            this.label28.TabIndex = 23;
            this.label28.Text = "Surname";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(21, 204);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(64, 15);
            this.label27.TabIndex = 22;
            this.label27.Text = "First Name";
            // 
            // txtWebEmail
            // 
            this.txtWebEmail.Location = new System.Drawing.Point(142, 259);
            this.txtWebEmail.Name = "txtWebEmail";
            this.txtWebEmail.Size = new System.Drawing.Size(174, 23);
            this.txtWebEmail.TabIndex = 23;
            // 
            // txtWebSurname
            // 
            this.txtWebSurname.Location = new System.Drawing.Point(142, 230);
            this.txtWebSurname.Name = "txtWebSurname";
            this.txtWebSurname.Size = new System.Drawing.Size(174, 23);
            this.txtWebSurname.TabIndex = 22;
            // 
            // txtWebFirstname
            // 
            this.txtWebFirstname.Location = new System.Drawing.Point(142, 201);
            this.txtWebFirstname.Name = "txtWebFirstname";
            this.txtWebFirstname.Size = new System.Drawing.Size(174, 23);
            this.txtWebFirstname.TabIndex = 21;
            // 
            // txtWebUserPassword
            // 
            this.txtWebUserPassword.Location = new System.Drawing.Point(142, 112);
            this.txtWebUserPassword.Name = "txtWebUserPassword";
            this.txtWebUserPassword.PasswordChar = '*';
            this.txtWebUserPassword.Size = new System.Drawing.Size(174, 23);
            this.txtWebUserPassword.TabIndex = 19;
            this.txtWebUserPassword.UseSystemPasswordChar = true;
            // 
            // txtWebUserLoginName
            // 
            this.txtWebUserLoginName.Location = new System.Drawing.Point(142, 83);
            this.txtWebUserLoginName.Name = "txtWebUserLoginName";
            this.txtWebUserLoginName.Size = new System.Drawing.Size(174, 23);
            this.txtWebUserLoginName.TabIndex = 18;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(21, 115);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(57, 15);
            this.label26.TabIndex = 17;
            this.label26.Text = "Password";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(21, 86);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(72, 15);
            this.label25.TabIndex = 16;
            this.label25.Text = "Login Name";
            // 
            // label24
            // 
            this.label24.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label24.Location = new System.Drawing.Point(18, 31);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(321, 21);
            this.label24.TabIndex = 15;
            this.label24.Text = "Please fill Username and password for new First User";
            // 
            // pageGit
            // 
            this.pageGit.Controls.Add(this.dockerSourceFolderLabel);
            this.pageGit.Controls.Add(this.txtdosourcefolder);
            this.pageGit.Controls.Add(this.lblgitemail);
            this.pageGit.Controls.Add(this.lblgituser);
            this.pageGit.Controls.Add(this.txtGitEmail);
            this.pageGit.Controls.Add(this.txtGitUser);
            this.pageGit.Controls.Add(this.lblSourcesFolderDescription);
            this.pageGit.Controls.Add(this.lblSourcesFolder);
            this.pageGit.Controls.Add(this.txtSourcesFolder);
            this.pageGit.Controls.Add(this.btnSelectSourcesFolder);
            this.pageGit.Controls.Add(this.label15);
            this.pageGit.Controls.Add(this.gitrepo);
            this.pageGit.Name = "pageGit";
            this.pageGit.NextPage = this.pageDocker;
            this.pageGit.Size = new System.Drawing.Size(737, 407);
            this.pageGit.TabIndex = 6;
            this.pageGit.Text = "Configure Source Control";
            this.pageGit.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.PageGit_Commit);
            // 
            // dockerSourceFolderLabel
            // 
            this.dockerSourceFolderLabel.AutoSize = true;
            this.dockerSourceFolderLabel.Location = new System.Drawing.Point(3, 280);
            this.dockerSourceFolderLabel.Name = "dockerSourceFolderLabel";
            this.dockerSourceFolderLabel.Size = new System.Drawing.Size(117, 15);
            this.dockerSourceFolderLabel.TabIndex = 1;
            this.dockerSourceFolderLabel.Text = "Docker Model Folder";
            // 
            // txtdosourcefolder
            // 
            this.txtdosourcefolder.Location = new System.Drawing.Point(160, 277);
            this.txtdosourcefolder.Name = "txtdosourcefolder";
            this.txtdosourcefolder.Size = new System.Drawing.Size(530, 23);
            this.txtdosourcefolder.TabIndex = 18;
            // 
            // lblgitemail
            // 
            this.lblgitemail.AutoSize = true;
            this.lblgitemail.Location = new System.Drawing.Point(3, 122);
            this.lblgitemail.Name = "lblgitemail";
            this.lblgitemail.Size = new System.Drawing.Size(36, 15);
            this.lblgitemail.TabIndex = 17;
            this.lblgitemail.Text = "Email";
            // 
            // lblgituser
            // 
            this.lblgituser.AutoSize = true;
            this.lblgituser.Location = new System.Drawing.Point(3, 76);
            this.lblgituser.Name = "lblgituser";
            this.lblgituser.Size = new System.Drawing.Size(30, 15);
            this.lblgituser.TabIndex = 16;
            this.lblgituser.Text = "User";
            // 
            // txtGitEmail
            // 
            this.txtGitEmail.Location = new System.Drawing.Point(160, 119);
            this.txtGitEmail.Name = "txtGitEmail";
            this.txtGitEmail.Size = new System.Drawing.Size(204, 23);
            this.txtGitEmail.TabIndex = 15;
            // 
            // txtGitUser
            // 
            this.txtGitUser.Location = new System.Drawing.Point(160, 73);
            this.txtGitUser.Name = "txtGitUser";
            this.txtGitUser.Size = new System.Drawing.Size(204, 23);
            this.txtGitUser.TabIndex = 14;
            // 
            // lblSourcesFolderDescription
            // 
            this.lblSourcesFolderDescription.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblSourcesFolderDescription.Location = new System.Drawing.Point(160, 195);
            this.lblSourcesFolderDescription.Name = "lblSourcesFolderDescription";
            this.lblSourcesFolderDescription.Size = new System.Drawing.Size(370, 39);
            this.lblSourcesFolderDescription.TabIndex = 5;
            this.lblSourcesFolderDescription.Text = "Model XML files will be stored here. You can use it as a GIT repository. A new fo" +
    "lder will be created for your application.";
            // 
            // lblSourcesFolder
            // 
            this.lblSourcesFolder.AutoSize = true;
            this.lblSourcesFolder.Location = new System.Drawing.Point(3, 168);
            this.lblSourcesFolder.Name = "lblSourcesFolder";
            this.lblSourcesFolder.Size = new System.Drawing.Size(84, 15);
            this.lblSourcesFolder.TabIndex = 4;
            this.lblSourcesFolder.Text = "Sources Folder";
            // 
            // txtSourcesFolder
            // 
            this.txtSourcesFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSourcesFolder.Location = new System.Drawing.Point(160, 165);
            this.txtSourcesFolder.Name = "txtSourcesFolder";
            this.txtSourcesFolder.Size = new System.Drawing.Size(533, 23);
            this.txtSourcesFolder.TabIndex = 3;
            // 
            // btnSelectSourcesFolder
            // 
            this.btnSelectSourcesFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectSourcesFolder.Location = new System.Drawing.Point(700, 165);
            this.btnSelectSourcesFolder.Name = "btnSelectSourcesFolder";
            this.btnSelectSourcesFolder.Size = new System.Drawing.Size(25, 23);
            this.btnSelectSourcesFolder.TabIndex = 10;
            this.btnSelectSourcesFolder.Text = "...";
            this.btnSelectSourcesFolder.UseVisualStyleBackColor = true;
            this.btnSelectSourcesFolder.Click += new System.EventHandler(this.btnSelectSourcesFolder_Click);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(3, 33);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(117, 15);
            this.label15.TabIndex = 13;
            this.label15.Text = "Create GIT repository";
            // 
            // gitrepo
            // 
            this.gitrepo.AutoSize = true;
            this.gitrepo.Checked = true;
            this.gitrepo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.gitrepo.Location = new System.Drawing.Point(160, 34);
            this.gitrepo.Name = "gitrepo";
            this.gitrepo.Size = new System.Drawing.Size(15, 14);
            this.gitrepo.TabIndex = 12;
            this.gitrepo.UseVisualStyleBackColor = true;
            this.gitrepo.CheckedChanged += new System.EventHandler(this.Gitrepo_CheckedChanged);
            // 
            // pageDocker
            // 
            this.pageDocker.Controls.Add(this.label23);
            this.pageDocker.Controls.Add(this.label22);
            this.pageDocker.Controls.Add(this.dockerPortLabel);
            this.pageDocker.Controls.Add(this.txtDockerPort);
            this.pageDocker.Controls.Add(this.label21);
            this.pageDocker.Controls.Add(this.label19);
            this.pageDocker.Name = "pageDocker";
            this.pageDocker.NextPage = this.pageReview;
            this.pageDocker.Size = new System.Drawing.Size(737, 407);
            this.pageDocker.TabIndex = 7;
            this.pageDocker.Text = "Docker start script";
            this.pageDocker.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageDocker_Commit);
            this.pageDocker.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageDocker_Initialize);
            // 
            // label23
            // 
            this.label23.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label23.Location = new System.Drawing.Point(109, 151);
            this.label23.Name = "label23";
            this.label23.Size = new System.Drawing.Size(253, 49);
            this.label23.TabIndex = 20;
            this.label23.Text = "If you have already created docker for other project on your computer please fill" +
    " different port.";
            // 
            // label22
            // 
            this.label22.AutoSize = true;
            this.label22.Location = new System.Drawing.Point(109, 151);
            this.label22.Name = "label22";
            this.label22.Size = new System.Drawing.Size(0, 15);
            this.label22.TabIndex = 19;
            // 
            // dockerPortLabel
            // 
            this.dockerPortLabel.AutoSize = true;
            this.dockerPortLabel.Location = new System.Drawing.Point(16, 115);
            this.dockerPortLabel.Name = "dockerPortLabel";
            this.dockerPortLabel.Size = new System.Drawing.Size(69, 15);
            this.dockerPortLabel.TabIndex = 18;
            this.dockerPortLabel.Text = "Docker Port";
            // 
            // txtDockerPort
            // 
            this.txtDockerPort.Location = new System.Drawing.Point(112, 112);
            this.txtDockerPort.Name = "txtDockerPort";
            this.txtDockerPort.Size = new System.Drawing.Size(100, 23);
            this.txtDockerPort.TabIndex = 17;
            this.txtDockerPort.Text = "8080";
            // 
            // label21
            // 
            this.label21.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label21.Location = new System.Drawing.Point(16, 49);
            this.label21.Name = "label21";
            this.label21.Size = new System.Drawing.Size(662, 19);
            this.label21.TabIndex = 16;
            // 
            // label19
            // 
            this.label19.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label19.Location = new System.Drawing.Point(16, 17);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(705, 17);
            this.label19.TabIndex = 14;
            this.label19.Text = "This wizard create start script for start docker with new project.All files are i" +
    "n OrigamModel subdirectory scripts.";
            // 
            // pageReview
            // 
            this.pageReview.Controls.Add(this.lstTasks);
            this.pageReview.IsFinishPage = true;
            this.pageReview.Name = "pageReview";
            this.pageReview.NextPage = this.pageReview;
            this.pageReview.Size = new System.Drawing.Size(737, 407);
            this.pageReview.TabIndex = 1;
            this.pageReview.Text = "Progress";
            this.pageReview.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.PageReview_Commit);
            this.pageReview.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageReview_Initialize);
            // 
            // lstTasks
            // 
            this.lstTasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colText,
            this.colStatus});
            this.lstTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstTasks.FullRowSelect = true;
            this.lstTasks.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.lstTasks.HideSelection = false;
            this.lstTasks.Location = new System.Drawing.Point(0, 0);
            this.lstTasks.Name = "lstTasks";
            this.lstTasks.Size = new System.Drawing.Size(737, 407);
            this.lstTasks.TabIndex = 0;
            this.lstTasks.UseCompatibleStateImageBehavior = false;
            this.lstTasks.View = System.Windows.Forms.View.Details;
            // 
            // colText
            // 
            this.colText.Text = "Task";
            this.colText.Width = 439;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.Width = 81;
            // 
            // wizOpenRepository
            // 
            this.wizOpenRepository.Controls.Add(this.rdNone);
            this.wizOpenRepository.Controls.Add(this.rdCopy);
            this.wizOpenRepository.Controls.Add(this.rdClone);
            this.wizOpenRepository.Controls.Add(this.label18);
            this.wizOpenRepository.Controls.Add(this.label17);
            this.wizOpenRepository.Controls.Add(this.label3);
            this.wizOpenRepository.Controls.Add(this.tbRepPassword);
            this.wizOpenRepository.Controls.Add(this.tbRepUsername);
            this.wizOpenRepository.Controls.Add(this.tbRepositoryLink);
            this.wizOpenRepository.Name = "wizOpenRepository";
            this.wizOpenRepository.Size = new System.Drawing.Size(737, 407);
            this.wizOpenRepository.TabIndex = 9;
            this.wizOpenRepository.Text = "Open Git Repository";
            this.wizOpenRepository.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.WizOpenRepository_Commit);
            this.wizOpenRepository.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.WizOpenRepository_Initialize);
            // 
            // rdNone
            // 
            this.rdNone.AutoSize = true;
            this.rdNone.Location = new System.Drawing.Point(235, 89);
            this.rdNone.Name = "rdNone";
            this.rdNone.Size = new System.Drawing.Size(54, 19);
            this.rdNone.TabIndex = 8;
            this.rdNone.TabStop = true;
            this.rdNone.Text = "None";
            this.rdNone.UseVisualStyleBackColor = true;
            // 
            // rdCopy
            // 
            this.rdCopy.AutoSize = true;
            this.rdCopy.Location = new System.Drawing.Point(113, 88);
            this.rdCopy.Name = "rdCopy";
            this.rdCopy.Size = new System.Drawing.Size(53, 19);
            this.rdCopy.TabIndex = 7;
            this.rdCopy.TabStop = true;
            this.rdCopy.Text = "Copy";
            this.rdCopy.UseVisualStyleBackColor = true;
            // 
            // rdClone
            // 
            this.rdClone.AutoSize = true;
            this.rdClone.Location = new System.Drawing.Point(172, 88);
            this.rdClone.Name = "rdClone";
            this.rdClone.Size = new System.Drawing.Size(56, 19);
            this.rdClone.TabIndex = 6;
            this.rdClone.TabStop = true;
            this.rdClone.Text = "Clone";
            this.rdClone.UseVisualStyleBackColor = true;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(0, 168);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(57, 15);
            this.label18.TabIndex = 5;
            this.label18.Text = "Password";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(0, 139);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(56, 15);
            this.label17.TabIndex = 4;
            this.label17.Text = "Usename";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(0, 62);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "Repository Link";
            // 
            // tbRepPassword
            // 
            this.tbRepPassword.Location = new System.Drawing.Point(113, 165);
            this.tbRepPassword.Name = "tbRepPassword";
            this.tbRepPassword.PasswordChar = '*';
            this.tbRepPassword.Size = new System.Drawing.Size(191, 23);
            this.tbRepPassword.TabIndex = 2;
            // 
            // tbRepUsername
            // 
            this.tbRepUsername.Location = new System.Drawing.Point(113, 136);
            this.tbRepUsername.Name = "tbRepUsername";
            this.tbRepUsername.Size = new System.Drawing.Size(191, 23);
            this.tbRepUsername.TabIndex = 1;
            // 
            // tbRepositoryLink
            // 
            this.tbRepositoryLink.Location = new System.Drawing.Point(113, 59);
            this.tbRepositoryLink.Name = "tbRepositoryLink";
            this.tbRepositoryLink.Size = new System.Drawing.Size(380, 23);
            this.tbRepositoryLink.TabIndex = 0;
            // 
            // pageLocalDeploymentSettings
            // 
            this.pageLocalDeploymentSettings.Controls.Add(this.labelPort);
            this.pageLocalDeploymentSettings.Controls.Add(this.txtPort);
            this.pageLocalDeploymentSettings.Controls.Add(this.labelPrivileges);
            this.pageLocalDeploymentSettings.Controls.Add(this.label16);
            this.pageLocalDeploymentSettings.Controls.Add(this.txtDatabaseType);
            this.pageLocalDeploymentSettings.Controls.Add(this.label4);
            this.pageLocalDeploymentSettings.Controls.Add(this.label2);
            this.pageLocalDeploymentSettings.Controls.Add(this.cboWebRoot);
            this.pageLocalDeploymentSettings.Controls.Add(this.lblDatabasePassword);
            this.pageLocalDeploymentSettings.Controls.Add(this.txtDatabasePassword);
            this.pageLocalDeploymentSettings.Controls.Add(this.lblWebRoot);
            this.pageLocalDeploymentSettings.Controls.Add(this.lblDatabaseUserName);
            this.pageLocalDeploymentSettings.Controls.Add(this.txtServerName);
            this.pageLocalDeploymentSettings.Controls.Add(this.lblServerName);
            this.pageLocalDeploymentSettings.Controls.Add(this.txtDatabaseUserName);
            this.pageLocalDeploymentSettings.Name = "pageLocalDeploymentSettings";
            this.pageLocalDeploymentSettings.NextPage = this.pagePaths;
            this.pageLocalDeploymentSettings.Size = new System.Drawing.Size(737, 407);
            this.pageLocalDeploymentSettings.TabIndex = 0;
            this.pageLocalDeploymentSettings.Text = "Local Deployment Settings";
            this.pageLocalDeploymentSettings.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageLocalDeploymentSettings_Commit);
            this.pageLocalDeploymentSettings.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageLocalDeploymentSettings_Initialize);
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(420, 134);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(29, 15);
            this.labelPort.TabIndex = 19;
            this.labelPort.Text = "Port";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(464, 131);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(100, 23);
            this.txtPort.TabIndex = 18;
            this.txtPort.Text = "1433";
            // 
            // labelPrivileges
            // 
            this.labelPrivileges.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelPrivileges.Location = new System.Drawing.Point(157, 284);
            this.labelPrivileges.Name = "labelPrivileges";
            this.labelPrivileges.Size = new System.Drawing.Size(563, 36);
            this.labelPrivileges.TabIndex = 17;
            this.labelPrivileges.Text = "Enter User Name and Password witch has privileges to create Database and user.";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(7, 86);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(55, 15);
            this.label16.TabIndex = 16;
            this.label16.Text = "Database";
            // 
            // txtDatabaseType
            // 
            this.txtDatabaseType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.txtDatabaseType.FormattingEnabled = true;
            this.txtDatabaseType.Location = new System.Drawing.Point(160, 86);
            this.txtDatabaseType.Name = "txtDatabaseType";
            this.txtDatabaseType.Size = new System.Drawing.Size(223, 23);
            this.txtDatabaseType.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label4.Location = new System.Drawing.Point(157, 166);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(518, 53);
            this.label4.TabIndex = 14;
            this.label4.Text = "Enter the connection information to your database server. A new database will be " +
    "created for storing the data. Example: .\\SQLEXPRESS or localhost";
            // 
            // label2
            // 
            this.label2.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label2.Location = new System.Drawing.Point(157, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(370, 39);
            this.label2.TabIndex = 13;
            this.label2.Text = "A new virtual directory/application will be created under the selected web site. " +
    "Example: Default Web Site";
            // 
            // cboWebRoot
            // 
            this.cboWebRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboWebRoot.FormattingEnabled = true;
            this.cboWebRoot.Location = new System.Drawing.Point(161, 13);
            this.cboWebRoot.Name = "cboWebRoot";
            this.cboWebRoot.Size = new System.Drawing.Size(559, 23);
            this.cboWebRoot.TabIndex = 1;
            // 
            // lblDatabasePassword
            // 
            this.lblDatabasePassword.AutoSize = true;
            this.lblDatabasePassword.Location = new System.Drawing.Point(7, 255);
            this.lblDatabasePassword.Name = "lblDatabasePassword";
            this.lblDatabasePassword.Size = new System.Drawing.Size(57, 15);
            this.lblDatabasePassword.TabIndex = 7;
            this.lblDatabasePassword.Text = "Password";
            // 
            // txtDatabasePassword
            // 
            this.txtDatabasePassword.Location = new System.Drawing.Point(160, 252);
            this.txtDatabasePassword.Name = "txtDatabasePassword";
            this.txtDatabasePassword.PasswordChar = '*';
            this.txtDatabasePassword.Size = new System.Drawing.Size(121, 23);
            this.txtDatabasePassword.TabIndex = 9;
            // 
            // lblWebRoot
            // 
            this.lblWebRoot.AutoSize = true;
            this.lblWebRoot.Location = new System.Drawing.Point(7, 16);
            this.lblWebRoot.Name = "lblWebRoot";
            this.lblWebRoot.Size = new System.Drawing.Size(90, 15);
            this.lblWebRoot.TabIndex = 4;
            this.lblWebRoot.Text = "Parent Web Site";
            // 
            // lblDatabaseUserName
            // 
            this.lblDatabaseUserName.AutoSize = true;
            this.lblDatabaseUserName.Location = new System.Drawing.Point(7, 225);
            this.lblDatabaseUserName.Name = "lblDatabaseUserName";
            this.lblDatabaseUserName.Size = new System.Drawing.Size(65, 15);
            this.lblDatabaseUserName.TabIndex = 6;
            this.lblDatabaseUserName.Text = "User Name";
            // 
            // txtServerName
            // 
            this.txtServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServerName.Location = new System.Drawing.Point(160, 131);
            this.txtServerName.Name = "txtServerName";
            this.txtServerName.Size = new System.Drawing.Size(223, 23);
            this.txtServerName.TabIndex = 3;
            // 
            // lblServerName
            // 
            this.lblServerName.AutoSize = true;
            this.lblServerName.Location = new System.Drawing.Point(7, 134);
            this.lblServerName.Name = "lblServerName";
            this.lblServerName.Size = new System.Drawing.Size(90, 15);
            this.lblServerName.TabIndex = 5;
            this.lblServerName.Text = "Database Server";
            // 
            // txtDatabaseUserName
            // 
            this.txtDatabaseUserName.Location = new System.Drawing.Point(160, 222);
            this.txtDatabaseUserName.Name = "txtDatabaseUserName";
            this.txtDatabaseUserName.Size = new System.Drawing.Size(121, 23);
            this.txtDatabaseUserName.TabIndex = 8;
            // 
            // projectBindingSource
            // 
            this.projectBindingSource.DataSource = typeof(Origam.ProjectAutomation.Project);
            // 
            // projectBindingSource1
            // 
            this.projectBindingSource1.DataSource = typeof(Origam.ProjectAutomation.Project);
            // 
            // NewProjectWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 561);
            this.ControlBox = false;
            this.Controls.Add(this.wizard1);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NewProjectWizard";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "NewProjectWizard";
            ((System.ComponentModel.ISupportInitialize)(this.wizard1)).EndInit();
            this.pageWelcome.ResumeLayout(false);
            this.pageWelcome.PerformLayout();
            this.pageDeploymentType.ResumeLayout(false);
            this.pageDeploymentType.PerformLayout();
            this.pagePaths.ResumeLayout(false);
            this.pagePaths.PerformLayout();
            this.pageWebUser.ResumeLayout(false);
            this.pageWebUser.PerformLayout();
            this.pageGit.ResumeLayout(false);
            this.pageGit.PerformLayout();
            this.pageDocker.ResumeLayout(false);
            this.pageDocker.PerformLayout();
            this.pageReview.ResumeLayout(false);
            this.wizOpenRepository.ResumeLayout(false);
            this.wizOpenRepository.PerformLayout();
            this.pageLocalDeploymentSettings.ResumeLayout(false);
            this.pageLocalDeploymentSettings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.projectBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.projectBindingSource1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private AeroWizard.WizardControl wizard1;
        private AeroWizard.WizardPage pageLocalDeploymentSettings;
        private System.Windows.Forms.Label lblServerName;
        private System.Windows.Forms.Label lblWebRoot;
        private System.Windows.Forms.TextBox txtServerName;
        private System.Windows.Forms.Label lblDatabaseUserName;
        private System.Windows.Forms.Label lblDatabasePassword;
        private System.Windows.Forms.TextBox txtDatabaseUserName;
        private System.Windows.Forms.TextBox txtDatabasePassword;
        private System.Windows.Forms.ComboBox cboWebRoot;
        private AeroWizard.WizardPage pageReview;
        private System.Windows.Forms.ListView lstTasks;
        private System.Windows.Forms.ColumnHeader colText;
        private System.Windows.Forms.ColumnHeader colStatus;
        private AeroWizard.WizardPage pagePaths;
        private System.Windows.Forms.Label lblBinFolderRoot;
        private System.Windows.Forms.TextBox txtBinFolderRoot;
        private System.Windows.Forms.Label lblBinFolderRootDescription;
        private System.Windows.Forms.Label lblTemplateFolderDescription;
        private System.Windows.Forms.Label lblTemplateFolder;
        private System.Windows.Forms.TextBox defaultModelPath;
        private System.Windows.Forms.Label lblSourcesFolderDescription;
        private System.Windows.Forms.Label lblSourcesFolder;
        private System.Windows.Forms.TextBox txtSourcesFolder;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnSelectTemplateFolder;
        private System.Windows.Forms.Button btnSelectSourcesFolder;
        private System.Windows.Forms.Button btnSelectBinFolderRoot;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private AeroWizard.WizardPage pageWelcome;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblWelcome1;
        private AeroWizard.WizardPage pageDeploymentType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.CheckBox gitrepo;
        private AeroWizard.WizardPage pageGit;
        private System.Windows.Forms.Label lblgitemail;
        private System.Windows.Forms.Label lblgituser;
        private System.Windows.Forms.TextBox txtGitEmail;
        private System.Windows.Forms.TextBox txtGitUser;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.ComboBox txtDatabaseType;
        private System.Windows.Forms.BindingSource projectBindingSource;
        private System.Windows.Forms.BindingSource projectBindingSource1;
        private System.Windows.Forms.Label labelPrivileges;
        private System.Windows.Forms.Label labelPort;
        private System.Windows.Forms.TextBox txtPort;
        private WizardPage wizOpenRepository;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbRepPassword;
        private System.Windows.Forms.TextBox tbRepUsername;
        private System.Windows.Forms.TextBox tbRepositoryLink;
        private System.Windows.Forms.RadioButton rdCopy;
        private System.Windows.Forms.RadioButton rdClone;
        private System.Windows.Forms.RadioButton rdNone;
        private WizardPage pageDocker;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.Label label21;
        private System.Windows.Forms.Label label23;
        private System.Windows.Forms.Label label22;
        private System.Windows.Forms.Label dockerPortLabel;
        private System.Windows.Forms.TextBox txtDockerPort;
        private WizardPage pageWebUser;
        private System.Windows.Forms.TextBox txtWebUserPassword;
        private System.Windows.Forms.TextBox txtWebUserLoginName;
        private System.Windows.Forms.Label label26;
        private System.Windows.Forms.Label label25;
        private System.Windows.Forms.Label label24;
        private System.Windows.Forms.Label label29;
        private System.Windows.Forms.Label label28;
        private System.Windows.Forms.Label label27;
        private System.Windows.Forms.TextBox txtWebEmail;
        private System.Windows.Forms.TextBox txtWebSurname;
        private System.Windows.Forms.TextBox txtWebFirstname;
        private System.Windows.Forms.Label label30;
        private System.Windows.Forms.TextBox txtWebUserPasswordConfirmed;
        private System.Windows.Forms.Label dockerSourceFolderLabel;
        private System.Windows.Forms.TextBox txtdosourcefolder;
    }
}