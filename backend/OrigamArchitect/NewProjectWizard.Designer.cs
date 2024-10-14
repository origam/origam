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
            this.pageLocalDeploymentSettings = new AeroWizard.WizardPage();
            this.labelPort = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.labelPrivileges = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.txtDatabaseType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lblDatabasePassword = new System.Windows.Forms.Label();
            this.txtDatabasePassword = new System.Windows.Forms.TextBox();
            this.lblDatabaseUserName = new System.Windows.Forms.Label();
            this.txtServerName = new System.Windows.Forms.TextBox();
            this.lblServerName = new System.Windows.Forms.Label();
            this.txtDatabaseUserName = new System.Windows.Forms.TextBox();
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
            this.pageReview = new AeroWizard.WizardPage();
            this.lstTasks = new System.Windows.Forms.ListView();
            this.colText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pageSuccess = new AeroWizard.WizardPage();
            this.finalMessageLabel = new System.Windows.Forms.Label();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.projectBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.projectBindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.wizard1)).BeginInit();
            this.pageWelcome.SuspendLayout();
            this.pageDeploymentType.SuspendLayout();
            this.pageLocalDeploymentSettings.SuspendLayout();
            this.pageWebUser.SuspendLayout();
            this.pageGit.SuspendLayout();
            this.pageReview.SuspendLayout();
            this.pageSuccess.SuspendLayout();
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
            this.wizard1.Pages.Add(this.pageLocalDeploymentSettings);
            this.wizard1.Pages.Add(this.pageWebUser);
            this.wizard1.Pages.Add(this.pageGit);
            this.wizard1.Pages.Add(this.pageReview);
            this.wizard1.Pages.Add(this.pageSuccess);
            this.wizard1.Size = new System.Drawing.Size(1176, 863);
            this.wizard1.TabIndex = 0;
            this.wizard1.Title = "New Project";
            // 
            // pageWelcome
            // 
            this.pageWelcome.Controls.Add(this.label9);
            this.pageWelcome.Controls.Add(this.label8);
            this.pageWelcome.Controls.Add(this.label7);
            this.pageWelcome.Controls.Add(this.label6);
            this.pageWelcome.Controls.Add(this.lblWelcome1);
            this.pageWelcome.Name = "pageWelcome";
            this.pageWelcome.Size = new System.Drawing.Size(1129, 673);
            this.pageWelcome.TabIndex = 3;
            this.pageWelcome.Text = "Welcome To New Project Wizard";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(34, 186);
            this.label9.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(304, 25);
            this.label9.TabIndex = 6;
            this.label9.Text = "– Configure the source control folder";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(34, 149);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(181, 25);
            this.label8.TabIndex = 5;
            this.label8.Text = "– Set up a web server";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(34, 112);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(264, 25);
            this.label7.TabIndex = 4;
            this.label7.Text = "– Create and populate database";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(34, 75);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(269, 25);
            this.label6.TabIndex = 3;
            this.label6.Text = "– Add an Architect configuration";
            // 
            // lblWelcome1
            // 
            this.lblWelcome1.Location = new System.Drawing.Point(-4, 20);
            this.lblWelcome1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblWelcome1.Name = "lblWelcome1";
            this.lblWelcome1.Size = new System.Drawing.Size(861, 55);
            this.lblWelcome1.TabIndex = 1;
            this.lblWelcome1.Text = "This wizard will help you set up a new project. We will go through several steps:" +
    "";
            // 
            // pageDeploymentType
            // 
            this.pageDeploymentType.Controls.Add(this.label1);
            this.pageDeploymentType.Controls.Add(this.lblName);
            this.pageDeploymentType.Controls.Add(this.txtName);
            this.pageDeploymentType.Name = "pageDeploymentType";
            this.pageDeploymentType.Size = new System.Drawing.Size(1129, 673);
            this.pageDeploymentType.TabIndex = 4;
            this.pageDeploymentType.Text = "Project Name";
            this.pageDeploymentType.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageDeploymentType_Commit);
            // 
            // label1
            // 
            this.label1.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label1.Location = new System.Drawing.Point(242, 68);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(555, 60);
            this.label1.TabIndex = 2;
            this.label1.Text = "Enter a name for your project. It will be used to set the package, database and f" +
    "olders names. Example: MyNewProject";
            // 
            // lblName
            // 
            this.lblName.AutoSize = true;
            this.lblName.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.World);
            this.lblName.Location = new System.Drawing.Point(6, 26);
            this.lblName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
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
            this.txtName.Location = new System.Drawing.Point(242, 22);
            this.txtName.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(859, 23);
            this.txtName.TabIndex = 1;
            // 
            // pageLocalDeploymentSettings
            // 
            this.pageLocalDeploymentSettings.Controls.Add(this.labelPort);
            this.pageLocalDeploymentSettings.Controls.Add(this.txtPort);
            this.pageLocalDeploymentSettings.Controls.Add(this.labelPrivileges);
            this.pageLocalDeploymentSettings.Controls.Add(this.label16);
            this.pageLocalDeploymentSettings.Controls.Add(this.txtDatabaseType);
            this.pageLocalDeploymentSettings.Controls.Add(this.label4);
            this.pageLocalDeploymentSettings.Controls.Add(this.lblDatabasePassword);
            this.pageLocalDeploymentSettings.Controls.Add(this.txtDatabasePassword);
            this.pageLocalDeploymentSettings.Controls.Add(this.lblDatabaseUserName);
            this.pageLocalDeploymentSettings.Controls.Add(this.txtServerName);
            this.pageLocalDeploymentSettings.Controls.Add(this.lblServerName);
            this.pageLocalDeploymentSettings.Controls.Add(this.txtDatabaseUserName);
            this.pageLocalDeploymentSettings.Name = "pageLocalDeploymentSettings";
            this.pageLocalDeploymentSettings.Size = new System.Drawing.Size(1129, 673);
            this.pageLocalDeploymentSettings.TabIndex = 0;
            this.pageLocalDeploymentSettings.Text = "Database";
            this.pageLocalDeploymentSettings.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageLocalDeploymentSettings_Commit);
            this.pageLocalDeploymentSettings.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageLocalDeploymentSettings_Initialize);
            // 
            // labelPort
            // 
            this.labelPort.AutoSize = true;
            this.labelPort.Location = new System.Drawing.Point(630, 206);
            this.labelPort.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPort.Name = "labelPort";
            this.labelPort.Size = new System.Drawing.Size(44, 25);
            this.labelPort.TabIndex = 19;
            this.labelPort.Text = "Port";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(696, 202);
            this.txtPort.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(148, 31);
            this.txtPort.TabIndex = 4;
            this.txtPort.Text = "1433";
            // 
            // labelPrivileges
            // 
            this.labelPrivileges.ForeColor = System.Drawing.SystemColors.GrayText;
            this.labelPrivileges.Location = new System.Drawing.Point(236, 437);
            this.labelPrivileges.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelPrivileges.Name = "labelPrivileges";
            this.labelPrivileges.Size = new System.Drawing.Size(844, 55);
            this.labelPrivileges.TabIndex = 17;
            this.labelPrivileges.Text = "Enter database user name and password. The user must have the create database pri" +
    "vilage.";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(10, 157);
            this.label16.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(86, 25);
            this.label16.TabIndex = 16;
            this.label16.Text = "Database";
            // 
            // txtDatabaseType
            // 
            this.txtDatabaseType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.txtDatabaseType.Enabled = false;
            this.txtDatabaseType.FormattingEnabled = true;
            this.txtDatabaseType.Location = new System.Drawing.Point(240, 157);
            this.txtDatabaseType.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDatabaseType.Name = "txtDatabaseType";
            this.txtDatabaseType.Size = new System.Drawing.Size(332, 33);
            this.txtDatabaseType.TabIndex = 15;
            // 
            // label4
            // 
            this.label4.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label4.Location = new System.Drawing.Point(236, 255);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(777, 82);
            this.label4.TabIndex = 14;
            this.label4.Text = "Enter \".\" if your database is located on your machnie. If not enter an IP address" +
    ".";
            // 
            // lblDatabasePassword
            // 
            this.lblDatabasePassword.AutoSize = true;
            this.lblDatabasePassword.Location = new System.Drawing.Point(10, 392);
            this.lblDatabasePassword.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDatabasePassword.Name = "lblDatabasePassword";
            this.lblDatabasePassword.Size = new System.Drawing.Size(87, 25);
            this.lblDatabasePassword.TabIndex = 7;
            this.lblDatabasePassword.Text = "Password";
            // 
            // txtDatabasePassword
            // 
            this.txtDatabasePassword.Location = new System.Drawing.Point(240, 388);
            this.txtDatabasePassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtDatabasePassword.Name = "txtDatabasePassword";
            this.txtDatabasePassword.PasswordChar = '*';
            this.txtDatabasePassword.Size = new System.Drawing.Size(180, 31);
            this.txtDatabasePassword.TabIndex = 2;
            // 
            // lblDatabaseUserName
            // 
            this.lblDatabaseUserName.AutoSize = true;
            this.lblDatabaseUserName.Location = new System.Drawing.Point(10, 346);
            this.lblDatabaseUserName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDatabaseUserName.Name = "lblDatabaseUserName";
            this.lblDatabaseUserName.Size = new System.Drawing.Size(99, 25);
            this.lblDatabaseUserName.TabIndex = 6;
            this.lblDatabaseUserName.Text = "User Name";
            // 
            // txtServerName
            // 
            this.txtServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServerName.Location = new System.Drawing.Point(240, 202);
            this.txtServerName.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.txtServerName.Name = "txtServerName";
            this.txtServerName.Size = new System.Drawing.Size(355, 31);
            this.txtServerName.TabIndex = 3;
            // 
            // lblServerName
            // 
            this.lblServerName.AutoSize = true;
            this.lblServerName.Location = new System.Drawing.Point(10, 206);
            this.lblServerName.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblServerName.Name = "lblServerName";
            this.lblServerName.Size = new System.Drawing.Size(140, 25);
            this.lblServerName.TabIndex = 5;
            this.lblServerName.Text = "Database Server";
            // 
            // txtDatabaseUserName
            // 
            this.txtDatabaseUserName.Location = new System.Drawing.Point(240, 342);
            this.txtDatabaseUserName.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.txtDatabaseUserName.Name = "txtDatabaseUserName";
            this.txtDatabaseUserName.Size = new System.Drawing.Size(180, 31);
            this.txtDatabaseUserName.TabIndex = 1;
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
            this.pageWebUser.Size = new System.Drawing.Size(1129, 673);
            this.pageWebUser.TabIndex = 10;
            this.pageWebUser.Text = "First Web User";
            this.pageWebUser.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageWebUser_Commit);
            // 
            // label30
            // 
            this.label30.AutoSize = true;
            this.label30.Location = new System.Drawing.Point(32, 222);
            this.label30.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label30.Name = "label30";
            this.label30.Size = new System.Drawing.Size(156, 25);
            this.label30.TabIndex = 26;
            this.label30.Text = "Confirm Password";
            // 
            // txtWebUserPasswordConfirmed
            // 
            this.txtWebUserPasswordConfirmed.Location = new System.Drawing.Point(213, 217);
            this.txtWebUserPasswordConfirmed.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtWebUserPasswordConfirmed.Name = "txtWebUserPasswordConfirmed";
            this.txtWebUserPasswordConfirmed.PasswordChar = '*';
            this.txtWebUserPasswordConfirmed.Size = new System.Drawing.Size(259, 31);
            this.txtWebUserPasswordConfirmed.TabIndex = 20;
            this.txtWebUserPasswordConfirmed.UseSystemPasswordChar = true;
            // 
            // label29
            // 
            this.label29.AutoSize = true;
            this.label29.Location = new System.Drawing.Point(32, 411);
            this.label29.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label29.Name = "label29";
            this.label29.Size = new System.Drawing.Size(54, 25);
            this.label29.TabIndex = 24;
            this.label29.Text = "Email";
            // 
            // label28
            // 
            this.label28.AutoSize = true;
            this.label28.Location = new System.Drawing.Point(32, 358);
            this.label28.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label28.Name = "label28";
            this.label28.Size = new System.Drawing.Size(82, 25);
            this.label28.TabIndex = 23;
            this.label28.Text = "Surname";
            // 
            // label27
            // 
            this.label27.AutoSize = true;
            this.label27.Location = new System.Drawing.Point(32, 314);
            this.label27.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label27.Name = "label27";
            this.label27.Size = new System.Drawing.Size(97, 25);
            this.label27.TabIndex = 22;
            this.label27.Text = "First Name";
            // 
            // txtWebEmail
            // 
            this.txtWebEmail.Location = new System.Drawing.Point(213, 398);
            this.txtWebEmail.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.txtWebEmail.Name = "txtWebEmail";
            this.txtWebEmail.Size = new System.Drawing.Size(259, 31);
            this.txtWebEmail.TabIndex = 23;
            // 
            // txtWebSurname
            // 
            this.txtWebSurname.Location = new System.Drawing.Point(213, 354);
            this.txtWebSurname.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.txtWebSurname.Name = "txtWebSurname";
            this.txtWebSurname.Size = new System.Drawing.Size(259, 31);
            this.txtWebSurname.TabIndex = 22;
            // 
            // txtWebFirstname
            // 
            this.txtWebFirstname.Location = new System.Drawing.Point(213, 309);
            this.txtWebFirstname.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.txtWebFirstname.Name = "txtWebFirstname";
            this.txtWebFirstname.Size = new System.Drawing.Size(259, 31);
            this.txtWebFirstname.TabIndex = 21;
            // 
            // txtWebUserPassword
            // 
            this.txtWebUserPassword.Location = new System.Drawing.Point(213, 172);
            this.txtWebUserPassword.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.txtWebUserPassword.Name = "txtWebUserPassword";
            this.txtWebUserPassword.PasswordChar = '*';
            this.txtWebUserPassword.Size = new System.Drawing.Size(259, 31);
            this.txtWebUserPassword.TabIndex = 19;
            this.txtWebUserPassword.UseSystemPasswordChar = true;
            // 
            // txtWebUserLoginName
            // 
            this.txtWebUserLoginName.Location = new System.Drawing.Point(213, 128);
            this.txtWebUserLoginName.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.txtWebUserLoginName.Name = "txtWebUserLoginName";
            this.txtWebUserLoginName.Size = new System.Drawing.Size(259, 31);
            this.txtWebUserLoginName.TabIndex = 18;
            // 
            // label26
            // 
            this.label26.AutoSize = true;
            this.label26.Location = new System.Drawing.Point(32, 177);
            this.label26.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label26.Name = "label26";
            this.label26.Size = new System.Drawing.Size(87, 25);
            this.label26.TabIndex = 17;
            this.label26.Text = "Password";
            // 
            // label25
            // 
            this.label25.AutoSize = true;
            this.label25.Location = new System.Drawing.Point(32, 132);
            this.label25.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label25.Name = "label25";
            this.label25.Size = new System.Drawing.Size(108, 25);
            this.label25.TabIndex = 16;
            this.label25.Text = "Login Name";
            // 
            // label24
            // 
            this.label24.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label24.Location = new System.Drawing.Point(27, 48);
            this.label24.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label24.Name = "label24";
            this.label24.Size = new System.Drawing.Size(1035, 103);
            this.label24.TabIndex = 15;
            this.label24.Text = "Please fill user name and password for the first user in your client web applicat" +
    "ion. You will use this account to access the applciation.ion";
            // 
            // pageGit
            // 
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
            this.pageGit.Size = new System.Drawing.Size(1129, 673);
            this.pageGit.TabIndex = 6;
            this.pageGit.Text = "Model Location";
            this.pageGit.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageGit_Initialize);
            this.pageGit.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.PageGit_Commit);
            // 
            // lblgitemail
            // 
            this.lblgitemail.AutoSize = true;
            this.lblgitemail.Location = new System.Drawing.Point(4, 188);
            this.lblgitemail.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblgitemail.Name = "lblgitemail";
            this.lblgitemail.Size = new System.Drawing.Size(54, 25);
            this.lblgitemail.TabIndex = 17;
            this.lblgitemail.Text = "Email";
            // 
            // lblgituser
            // 
            this.lblgituser.AutoSize = true;
            this.lblgituser.Location = new System.Drawing.Point(4, 117);
            this.lblgituser.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblgituser.Name = "lblgituser";
            this.lblgituser.Size = new System.Drawing.Size(47, 25);
            this.lblgituser.TabIndex = 16;
            this.lblgituser.Text = "User";
            // 
            // txtGitEmail
            // 
            this.txtGitEmail.Location = new System.Drawing.Point(240, 183);
            this.txtGitEmail.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.txtGitEmail.Name = "txtGitEmail";
            this.txtGitEmail.Size = new System.Drawing.Size(304, 31);
            this.txtGitEmail.TabIndex = 15;
            // 
            // txtGitUser
            // 
            this.txtGitUser.Location = new System.Drawing.Point(240, 112);
            this.txtGitUser.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.txtGitUser.Name = "txtGitUser";
            this.txtGitUser.Size = new System.Drawing.Size(304, 31);
            this.txtGitUser.TabIndex = 14;
            // 
            // lblSourcesFolderDescription
            // 
            this.lblSourcesFolderDescription.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblSourcesFolderDescription.Location = new System.Drawing.Point(240, 300);
            this.lblSourcesFolderDescription.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSourcesFolderDescription.Name = "lblSourcesFolderDescription";
            this.lblSourcesFolderDescription.Size = new System.Drawing.Size(555, 60);
            this.lblSourcesFolderDescription.TabIndex = 5;
            this.lblSourcesFolderDescription.Text = "A new folder with your project\'s name will be created here. Origam will store the" +
    " model in it. ";
            // 
            // lblSourcesFolder
            // 
            this.lblSourcesFolder.AutoSize = true;
            this.lblSourcesFolder.Location = new System.Drawing.Point(4, 258);
            this.lblSourcesFolder.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblSourcesFolder.Name = "lblSourcesFolder";
            this.lblSourcesFolder.Size = new System.Drawing.Size(129, 25);
            this.lblSourcesFolder.TabIndex = 4;
            this.lblSourcesFolder.Text = "Sources Folder";
            // 
            // txtSourcesFolder
            // 
            this.txtSourcesFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSourcesFolder.Location = new System.Drawing.Point(240, 254);
            this.txtSourcesFolder.Margin = new System.Windows.Forms.Padding(6, 8, 6, 8);
            this.txtSourcesFolder.Name = "txtSourcesFolder";
            this.txtSourcesFolder.Size = new System.Drawing.Size(821, 31);
            this.txtSourcesFolder.TabIndex = 3;
            // 
            // btnSelectSourcesFolder
            // 
            this.btnSelectSourcesFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectSourcesFolder.Location = new System.Drawing.Point(1073, 254);
            this.btnSelectSourcesFolder.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.btnSelectSourcesFolder.Name = "btnSelectSourcesFolder";
            this.btnSelectSourcesFolder.Size = new System.Drawing.Size(38, 35);
            this.btnSelectSourcesFolder.TabIndex = 10;
            this.btnSelectSourcesFolder.Text = "...";
            this.btnSelectSourcesFolder.UseVisualStyleBackColor = true;
            this.btnSelectSourcesFolder.Click += new System.EventHandler(this.btnSelectSourcesFolder_Click);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(4, 51);
            this.label15.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(179, 25);
            this.label15.TabIndex = 13;
            this.label15.Text = "Create GIT repository";
            // 
            // gitrepo
            // 
            this.gitrepo.AutoSize = true;
            this.gitrepo.Checked = true;
            this.gitrepo.CheckState = System.Windows.Forms.CheckState.Checked;
            this.gitrepo.Location = new System.Drawing.Point(240, 52);
            this.gitrepo.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.gitrepo.Name = "gitrepo";
            this.gitrepo.Size = new System.Drawing.Size(22, 21);
            this.gitrepo.TabIndex = 12;
            this.gitrepo.UseVisualStyleBackColor = true;
            this.gitrepo.CheckedChanged += new System.EventHandler(this.Gitrepo_CheckedChanged);
            // 
            // pageReview
            // 
            this.pageReview.Controls.Add(this.lstTasks);
            this.pageReview.Name = "pageReview";
            this.pageReview.NextPage = this.pageSuccess;
            this.pageReview.Size = new System.Drawing.Size(1129, 673);
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
            this.lstTasks.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.lstTasks.Name = "lstTasks";
            this.lstTasks.Size = new System.Drawing.Size(1129, 673);
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
            // pageSuccess
            // 
            this.pageSuccess.AllowBack = false;
            this.pageSuccess.AllowCancel = false;
            this.pageSuccess.Controls.Add(this.finalMessageLabel);
            this.pageSuccess.IsFinishPage = true;
            this.pageSuccess.Name = "pageSuccess";
            this.pageSuccess.ShowCancel = false;
            this.pageSuccess.Size = new System.Drawing.Size(1129, 673);
            this.pageSuccess.TabIndex = 11;
            this.pageSuccess.Text = "Success";
            this.pageSuccess.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageSuccess_Initialize);;
            // 
            // finalMessageLabel
            // 
            this.finalMessageLabel.Location = new System.Drawing.Point(21, 43);
            this.finalMessageLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.finalMessageLabel.Name = "finalMessageLabel";
            this.finalMessageLabel.Size = new System.Drawing.Size(1038, 158);
            this.finalMessageLabel.TabIndex = 1;
            // 
            // NewProjectWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1176, 863);
            this.ControlBox = false;
            this.Controls.Add(this.wizard1);
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
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
            this.pageLocalDeploymentSettings.ResumeLayout(false);
            this.pageLocalDeploymentSettings.PerformLayout();
            this.pageWebUser.ResumeLayout(false);
            this.pageWebUser.PerformLayout();
            this.pageGit.ResumeLayout(false);
            this.pageGit.PerformLayout();
            this.pageReview.ResumeLayout(false);
            this.pageSuccess.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.projectBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.projectBindingSource1)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Label finalMessageLabel;

        private AeroWizard.WizardPage pageSuccess;

        #endregion

        private AeroWizard.WizardControl wizard1;
        private AeroWizard.WizardPage pageLocalDeploymentSettings;
        private System.Windows.Forms.Label lblServerName;
        private System.Windows.Forms.TextBox txtServerName;
        private System.Windows.Forms.Label lblDatabaseUserName;
        private System.Windows.Forms.Label lblDatabasePassword;
        private System.Windows.Forms.TextBox txtDatabaseUserName;
        private System.Windows.Forms.TextBox txtDatabasePassword;
        private AeroWizard.WizardPage pageReview;
        private System.Windows.Forms.ListView lstTasks;
        private System.Windows.Forms.ColumnHeader colText;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.Label lblSourcesFolderDescription;
        private System.Windows.Forms.Label lblSourcesFolder;
        private System.Windows.Forms.TextBox txtSourcesFolder;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnSelectSourcesFolder;
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
        private AeroWizard.WizardPage pageWebUser;
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
    }
}