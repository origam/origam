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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewProjectWizard));
            this.wizard1 = new AeroWizard.WizardControl();
            this.pageWelcome = new AeroWizard.WizardPage();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblAdminWarning = new System.Windows.Forms.Label();
            this.lblWelcome1 = new System.Windows.Forms.Label();
            this.btnAdminElevate = new System.Windows.Forms.Button();
            this.pageDeploymentType = new AeroWizard.WizardPage();
            this.label1 = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.cboDeploymentType = new System.Windows.Forms.ComboBox();
            this.lblDeploymentType = new System.Windows.Forms.Label();
            this.pageAzureDeploymentSettings = new AeroWizard.WizardPage();
            this.txtAzureSubscriptionId = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.txtAzureRedirectUri = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.txtAzureClientId = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtAzureTenantId = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.txtAzurePassword = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.txtAzureUserName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.pagePaths = new AeroWizard.WizardPage();
            this.btnSelectTemplateFolder = new System.Windows.Forms.Button();
            this.btnSelectSourcesFolder = new System.Windows.Forms.Button();
            this.btnSelectBinFolderRoot = new System.Windows.Forms.Button();
            this.lblTemplateFolderDescription = new System.Windows.Forms.Label();
            this.lblTemplateFolder = new System.Windows.Forms.Label();
            this.txtTemplateFolder = new System.Windows.Forms.TextBox();
            this.lblSourcesFolderDescription = new System.Windows.Forms.Label();
            this.lblSourcesFolder = new System.Windows.Forms.Label();
            this.txtSourcesFolder = new System.Windows.Forms.TextBox();
            this.lblBinFolderRootDescription = new System.Windows.Forms.Label();
            this.lblBinFolderRoot = new System.Windows.Forms.Label();
            this.txtBinFolderRoot = new System.Windows.Forms.TextBox();
            this.pageLocalDeploymentSettings = new AeroWizard.WizardPage();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.chkIntegratedAuthentication = new System.Windows.Forms.CheckBox();
            this.cboWebRoot = new System.Windows.Forms.ComboBox();
            this.lblDatabasePassword = new System.Windows.Forms.Label();
            this.txtDatabasePassword = new System.Windows.Forms.TextBox();
            this.lblWebRoot = new System.Windows.Forms.Label();
            this.lblDatabaseUserName = new System.Windows.Forms.Label();
            this.txtServerName = new System.Windows.Forms.TextBox();
            this.lblServerName = new System.Windows.Forms.Label();
            this.txtDatabaseUserName = new System.Windows.Forms.TextBox();
            this.pageReview = new AeroWizard.WizardPage();
            this.lstTasks = new System.Windows.Forms.ListView();
            this.colText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            ((System.ComponentModel.ISupportInitialize)(this.wizard1)).BeginInit();
            this.pageWelcome.SuspendLayout();
            this.pageDeploymentType.SuspendLayout();
            this.pageAzureDeploymentSettings.SuspendLayout();
            this.pagePaths.SuspendLayout();
            this.pageLocalDeploymentSettings.SuspendLayout();
            this.pageReview.SuspendLayout();
            this.SuspendLayout();
            // 
            // wizard1
            // 
            this.wizard1.Location = new System.Drawing.Point(0, 0);
            this.wizard1.Name = "wizard1";
            this.wizard1.Pages.Add(this.pageWelcome);
            this.wizard1.Pages.Add(this.pageDeploymentType);
            this.wizard1.Pages.Add(this.pageAzureDeploymentSettings);
            this.wizard1.Pages.Add(this.pageLocalDeploymentSettings);
            this.wizard1.Pages.Add(this.pagePaths);
            this.wizard1.Pages.Add(this.pageReview);
            this.wizard1.Size = new System.Drawing.Size(621, 470);
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
            this.pageWelcome.Controls.Add(this.lblAdminWarning);
            this.pageWelcome.Controls.Add(this.lblWelcome1);
            this.pageWelcome.Controls.Add(this.btnAdminElevate);
            this.pageWelcome.Name = "pageWelcome";
            this.pageWelcome.Size = new System.Drawing.Size(574, 316);
            this.pageWelcome.TabIndex = 3;
            this.pageWelcome.Text = "Welcome To New Project Wizard";
            this.pageWelcome.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageWelcome_Initialize);
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
            // lblAdminWarning
            // 
            this.lblAdminWarning.AutoSize = true;
            this.lblAdminWarning.Location = new System.Drawing.Point(-3, 187);
            this.lblAdminWarning.Name = "lblAdminWarning";
            this.lblAdminWarning.Size = new System.Drawing.Size(522, 15);
            this.lblAdminWarning.TabIndex = 2;
            this.lblAdminWarning.Text = "Administrator privileges are mandatory otherwise it won\'t be possible to configur" +
    "e the web server.";
            this.lblAdminWarning.Visible = false;
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
            // btnAdminElevate
            // 
            this.btnAdminElevate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnAdminElevate.Location = new System.Drawing.Point(0, 217);
            this.btnAdminElevate.Name = "btnAdminElevate";
            this.btnAdminElevate.Size = new System.Drawing.Size(218, 30);
            this.btnAdminElevate.TabIndex = 0;
            this.btnAdminElevate.Text = "Restart As an Administrator";
            this.btnAdminElevate.UseVisualStyleBackColor = true;
            this.btnAdminElevate.Visible = false;
            this.btnAdminElevate.Click += new System.EventHandler(this.btnAdminElevate_Click);
            // 
            // pageDeploymentType
            // 
            this.pageDeploymentType.Controls.Add(this.label1);
            this.pageDeploymentType.Controls.Add(this.lblName);
            this.pageDeploymentType.Controls.Add(this.txtName);
            this.pageDeploymentType.Controls.Add(this.cboDeploymentType);
            this.pageDeploymentType.Controls.Add(this.lblDeploymentType);
            this.pageDeploymentType.Name = "pageDeploymentType";
            this.pageDeploymentType.Size = new System.Drawing.Size(574, 316);
            this.pageDeploymentType.TabIndex = 4;
            this.pageDeploymentType.Text = "Deployment Type";
            this.pageDeploymentType.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageDeploymentType_Commit);
            this.pageDeploymentType.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageDeploymentType_Initialize);
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
            this.txtName.Size = new System.Drawing.Size(396, 23);
            this.txtName.TabIndex = 1;
            // 
            // cboDeploymentType
            // 
            this.cboDeploymentType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDeploymentType.Enabled = false;
            this.cboDeploymentType.FormattingEnabled = true;
            this.cboDeploymentType.Items.AddRange(new object[] {
            "Local (IIS Server)",
            "Cloud (Microsoft Azure)"});
            this.cboDeploymentType.Location = new System.Drawing.Point(164, 97);
            this.cboDeploymentType.Name = "cboDeploymentType";
            this.cboDeploymentType.Size = new System.Drawing.Size(241, 23);
            this.cboDeploymentType.TabIndex = 4;
            // 
            // lblDeploymentType
            // 
            this.lblDeploymentType.AutoSize = true;
            this.lblDeploymentType.Enabled = false;
            this.lblDeploymentType.Location = new System.Drawing.Point(8, 100);
            this.lblDeploymentType.Name = "lblDeploymentType";
            this.lblDeploymentType.Size = new System.Drawing.Size(135, 15);
            this.lblDeploymentType.TabIndex = 3;
            this.lblDeploymentType.Text = "Select Deployment Type";
            // 
            // pageAzureDeploymentSettings
            // 
            this.pageAzureDeploymentSettings.Controls.Add(this.txtAzureSubscriptionId);
            this.pageAzureDeploymentSettings.Controls.Add(this.label14);
            this.pageAzureDeploymentSettings.Controls.Add(this.txtAzureRedirectUri);
            this.pageAzureDeploymentSettings.Controls.Add(this.label13);
            this.pageAzureDeploymentSettings.Controls.Add(this.txtAzureClientId);
            this.pageAzureDeploymentSettings.Controls.Add(this.label12);
            this.pageAzureDeploymentSettings.Controls.Add(this.txtAzureTenantId);
            this.pageAzureDeploymentSettings.Controls.Add(this.label11);
            this.pageAzureDeploymentSettings.Controls.Add(this.txtAzurePassword);
            this.pageAzureDeploymentSettings.Controls.Add(this.label10);
            this.pageAzureDeploymentSettings.Controls.Add(this.txtAzureUserName);
            this.pageAzureDeploymentSettings.Controls.Add(this.label5);
            this.pageAzureDeploymentSettings.Name = "pageAzureDeploymentSettings";
            this.pageAzureDeploymentSettings.NextPage = this.pagePaths;
            this.pageAzureDeploymentSettings.Size = new System.Drawing.Size(574, 316);
            this.pageAzureDeploymentSettings.TabIndex = 5;
            this.pageAzureDeploymentSettings.Text = "Microsoft Azure Deployment Settings";
            this.pageAzureDeploymentSettings.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageAzureDeploymentSettings_Commit);
            // 
            // txtAzureSubscriptionId
            // 
            this.txtAzureSubscriptionId.Location = new System.Drawing.Point(143, 123);
            this.txtAzureSubscriptionId.Name = "txtAzureSubscriptionId";
            this.txtAzureSubscriptionId.Size = new System.Drawing.Size(203, 23);
            this.txtAzureSubscriptionId.TabIndex = 11;
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(3, 131);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(86, 15);
            this.label14.TabIndex = 10;
            this.label14.Text = "Subscription Id";
            // 
            // txtAzureRedirectUri
            // 
            this.txtAzureRedirectUri.Location = new System.Drawing.Point(143, 152);
            this.txtAzureRedirectUri.Name = "txtAzureRedirectUri";
            this.txtAzureRedirectUri.Size = new System.Drawing.Size(203, 23);
            this.txtAzureRedirectUri.TabIndex = 9;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(3, 160);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(71, 15);
            this.label13.TabIndex = 8;
            this.label13.Text = "Redirect URI";
            // 
            // txtAzureClientId
            // 
            this.txtAzureClientId.Location = new System.Drawing.Point(143, 94);
            this.txtAzureClientId.Name = "txtAzureClientId";
            this.txtAzureClientId.Size = new System.Drawing.Size(203, 23);
            this.txtAzureClientId.TabIndex = 7;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(3, 102);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(51, 15);
            this.label12.TabIndex = 6;
            this.label12.Text = "Client Id";
            // 
            // txtAzureTenantId
            // 
            this.txtAzureTenantId.Location = new System.Drawing.Point(143, 65);
            this.txtAzureTenantId.Name = "txtAzureTenantId";
            this.txtAzureTenantId.Size = new System.Drawing.Size(203, 23);
            this.txtAzureTenantId.TabIndex = 5;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(3, 73);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(57, 15);
            this.label11.TabIndex = 4;
            this.label11.Text = "Tenant Id";
            // 
            // txtAzurePassword
            // 
            this.txtAzurePassword.Location = new System.Drawing.Point(143, 36);
            this.txtAzurePassword.Name = "txtAzurePassword";
            this.txtAzurePassword.PasswordChar = '*';
            this.txtAzurePassword.Size = new System.Drawing.Size(203, 23);
            this.txtAzurePassword.TabIndex = 3;
            this.txtAzurePassword.UseSystemPasswordChar = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(3, 44);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(57, 15);
            this.label10.TabIndex = 2;
            this.label10.Text = "Password";
            // 
            // txtAzureUserName
            // 
            this.txtAzureUserName.Location = new System.Drawing.Point(143, 7);
            this.txtAzureUserName.Name = "txtAzureUserName";
            this.txtAzureUserName.Size = new System.Drawing.Size(203, 23);
            this.txtAzureUserName.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 15);
            this.label5.TabIndex = 0;
            this.label5.Text = "User Name";
            // 
            // pagePaths
            // 
            this.pagePaths.Controls.Add(this.btnSelectTemplateFolder);
            this.pagePaths.Controls.Add(this.btnSelectSourcesFolder);
            this.pagePaths.Controls.Add(this.btnSelectBinFolderRoot);
            this.pagePaths.Controls.Add(this.lblTemplateFolderDescription);
            this.pagePaths.Controls.Add(this.lblTemplateFolder);
            this.pagePaths.Controls.Add(this.txtTemplateFolder);
            this.pagePaths.Controls.Add(this.lblSourcesFolderDescription);
            this.pagePaths.Controls.Add(this.lblSourcesFolder);
            this.pagePaths.Controls.Add(this.txtSourcesFolder);
            this.pagePaths.Controls.Add(this.lblBinFolderRootDescription);
            this.pagePaths.Controls.Add(this.lblBinFolderRoot);
            this.pagePaths.Controls.Add(this.txtBinFolderRoot);
            this.pagePaths.Name = "pagePaths";
            this.pagePaths.Size = new System.Drawing.Size(574, 316);
            this.pagePaths.TabIndex = 2;
            this.pagePaths.Text = "Paths";
            this.pagePaths.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pagePaths_Commit);
            this.pagePaths.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pagePaths_Initialize);
            // 
            // btnSelectTemplateFolder
            // 
            this.btnSelectTemplateFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectTemplateFolder.Location = new System.Drawing.Point(537, 157);
            this.btnSelectTemplateFolder.Name = "btnSelectTemplateFolder";
            this.btnSelectTemplateFolder.Size = new System.Drawing.Size(25, 23);
            this.btnSelectTemplateFolder.TabIndex = 11;
            this.btnSelectTemplateFolder.Text = "...";
            this.btnSelectTemplateFolder.UseVisualStyleBackColor = true;
            this.btnSelectTemplateFolder.Click += new System.EventHandler(this.btnSelectTemplateFolder_Click);
            // 
            // btnSelectSourcesFolder
            // 
            this.btnSelectSourcesFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectSourcesFolder.Location = new System.Drawing.Point(537, 85);
            this.btnSelectSourcesFolder.Name = "btnSelectSourcesFolder";
            this.btnSelectSourcesFolder.Size = new System.Drawing.Size(25, 23);
            this.btnSelectSourcesFolder.TabIndex = 10;
            this.btnSelectSourcesFolder.Text = "...";
            this.btnSelectSourcesFolder.UseVisualStyleBackColor = true;
            this.btnSelectSourcesFolder.Click += new System.EventHandler(this.btnSelectSourcesFolder_Click);
            // 
            // btnSelectBinFolderRoot
            // 
            this.btnSelectBinFolderRoot.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSelectBinFolderRoot.Location = new System.Drawing.Point(537, 13);
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
            this.lblTemplateFolderDescription.Location = new System.Drawing.Point(160, 187);
            this.lblTemplateFolderDescription.Name = "lblTemplateFolderDescription";
            this.lblTemplateFolderDescription.Size = new System.Drawing.Size(370, 41);
            this.lblTemplateFolderDescription.TabIndex = 8;
            this.lblTemplateFolderDescription.Text = "Enter a path where a template of your application is stored. It must contain \"Mod" +
    "el\" and \"ServerApplication\" subfolders.";
            // 
            // lblTemplateFolder
            // 
            this.lblTemplateFolder.AutoSize = true;
            this.lblTemplateFolder.Location = new System.Drawing.Point(3, 160);
            this.lblTemplateFolder.Name = "lblTemplateFolder";
            this.lblTemplateFolder.Size = new System.Drawing.Size(93, 15);
            this.lblTemplateFolder.TabIndex = 7;
            this.lblTemplateFolder.Text = "Template Folder";
            // 
            // txtTemplateFolder
            // 
            this.txtTemplateFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtTemplateFolder.Location = new System.Drawing.Point(160, 157);
            this.txtTemplateFolder.Name = "txtTemplateFolder";
            this.txtTemplateFolder.Size = new System.Drawing.Size(370, 23);
            this.txtTemplateFolder.TabIndex = 6;
            // 
            // lblSourcesFolderDescription
            // 
            this.lblSourcesFolderDescription.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblSourcesFolderDescription.Location = new System.Drawing.Point(160, 115);
            this.lblSourcesFolderDescription.Name = "lblSourcesFolderDescription";
            this.lblSourcesFolderDescription.Size = new System.Drawing.Size(370, 39);
            this.lblSourcesFolderDescription.TabIndex = 5;
            this.lblSourcesFolderDescription.Text = "Model XML files will be stored here. You can use it as a GIT repository. A new fo" +
    "lder will be created for your application.";
            // 
            // lblSourcesFolder
            // 
            this.lblSourcesFolder.AutoSize = true;
            this.lblSourcesFolder.Location = new System.Drawing.Point(3, 88);
            this.lblSourcesFolder.Name = "lblSourcesFolder";
            this.lblSourcesFolder.Size = new System.Drawing.Size(84, 15);
            this.lblSourcesFolder.TabIndex = 4;
            this.lblSourcesFolder.Text = "Sources Folder";
            // 
            // txtSourcesFolder
            // 
            this.txtSourcesFolder.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSourcesFolder.Location = new System.Drawing.Point(160, 85);
            this.txtSourcesFolder.Name = "txtSourcesFolder";
            this.txtSourcesFolder.Size = new System.Drawing.Size(370, 23);
            this.txtSourcesFolder.TabIndex = 3;
            // 
            // lblBinFolderRootDescription
            // 
            this.lblBinFolderRootDescription.ForeColor = System.Drawing.SystemColors.GrayText;
            this.lblBinFolderRootDescription.Location = new System.Drawing.Point(160, 43);
            this.lblBinFolderRootDescription.Name = "lblBinFolderRootDescription";
            this.lblBinFolderRootDescription.Size = new System.Drawing.Size(370, 39);
            this.lblBinFolderRootDescription.TabIndex = 2;
            this.lblBinFolderRootDescription.Text = "The web application\'s binary files will be copied here. A new folder will be crea" +
    "ted for your application.";
            // 
            // lblBinFolderRoot
            // 
            this.lblBinFolderRoot.AutoSize = true;
            this.lblBinFolderRoot.Location = new System.Drawing.Point(3, 16);
            this.lblBinFolderRoot.Name = "lblBinFolderRoot";
            this.lblBinFolderRoot.Size = new System.Drawing.Size(131, 15);
            this.lblBinFolderRoot.TabIndex = 1;
            this.lblBinFolderRoot.Text = "Web Application Folder";
            // 
            // txtBinFolderRoot
            // 
            this.txtBinFolderRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBinFolderRoot.Location = new System.Drawing.Point(160, 13);
            this.txtBinFolderRoot.Name = "txtBinFolderRoot";
            this.txtBinFolderRoot.Size = new System.Drawing.Size(370, 23);
            this.txtBinFolderRoot.TabIndex = 0;
            // 
            // pageLocalDeploymentSettings
            // 
            this.pageLocalDeploymentSettings.Controls.Add(this.label4);
            this.pageLocalDeploymentSettings.Controls.Add(this.label2);
            this.pageLocalDeploymentSettings.Controls.Add(this.label3);
            this.pageLocalDeploymentSettings.Controls.Add(this.chkIntegratedAuthentication);
            this.pageLocalDeploymentSettings.Controls.Add(this.cboWebRoot);
            this.pageLocalDeploymentSettings.Controls.Add(this.lblDatabasePassword);
            this.pageLocalDeploymentSettings.Controls.Add(this.txtDatabasePassword);
            this.pageLocalDeploymentSettings.Controls.Add(this.lblWebRoot);
            this.pageLocalDeploymentSettings.Controls.Add(this.lblDatabaseUserName);
            this.pageLocalDeploymentSettings.Controls.Add(this.txtServerName);
            this.pageLocalDeploymentSettings.Controls.Add(this.lblServerName);
            this.pageLocalDeploymentSettings.Controls.Add(this.txtDatabaseUserName);
            this.pageLocalDeploymentSettings.Name = "pageLocalDeploymentSettings";
            this.pageLocalDeploymentSettings.Size = new System.Drawing.Size(574, 316);
            this.pageLocalDeploymentSettings.TabIndex = 0;
            this.pageLocalDeploymentSettings.Text = "Local Deployment Settings";
            this.pageLocalDeploymentSettings.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageLocalDeploymentSettings_Commit);
            this.pageLocalDeploymentSettings.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageLocalDeploymentSettings_Initialize);
            // 
            // label4
            // 
            this.label4.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label4.Location = new System.Drawing.Point(299, 117);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(232, 78);
            this.label4.TabIndex = 14;
            this.label4.Text = "Enter the connection information to your database server. A new database will be " +
    "created for storing the data. Example: .\\SQLEXPRESS";
            // 
            // label2
            // 
            this.label2.ForeColor = System.Drawing.SystemColors.GrayText;
            this.label2.Location = new System.Drawing.Point(161, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(370, 39);
            this.label2.TabIndex = 13;
            this.label2.Text = "A new virtual directory/application will be created under the selected web site. " +
    "Example: Default Web Site";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 117);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(143, 15);
            this.label3.TabIndex = 11;
            this.label3.Text = "Integrated Authentication";
            // 
            // chkIntegratedAuthentication
            // 
            this.chkIntegratedAuthentication.AutoSize = true;
            this.chkIntegratedAuthentication.Checked = true;
            this.chkIntegratedAuthentication.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIntegratedAuthentication.Location = new System.Drawing.Point(161, 118);
            this.chkIntegratedAuthentication.Name = "chkIntegratedAuthentication";
            this.chkIntegratedAuthentication.Size = new System.Drawing.Size(15, 14);
            this.chkIntegratedAuthentication.TabIndex = 10;
            this.chkIntegratedAuthentication.UseVisualStyleBackColor = true;
            this.chkIntegratedAuthentication.CheckedChanged += new System.EventHandler(this.chkIntegratedAuthentication_CheckedChanged);
            // 
            // cboWebRoot
            // 
            this.cboWebRoot.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboWebRoot.FormattingEnabled = true;
            this.cboWebRoot.Location = new System.Drawing.Point(161, 13);
            this.cboWebRoot.Name = "cboWebRoot";
            this.cboWebRoot.Size = new System.Drawing.Size(396, 23);
            this.cboWebRoot.TabIndex = 1;
            // 
            // lblDatabasePassword
            // 
            this.lblDatabasePassword.AutoSize = true;
            this.lblDatabasePassword.Enabled = false;
            this.lblDatabasePassword.Location = new System.Drawing.Point(5, 180);
            this.lblDatabasePassword.Name = "lblDatabasePassword";
            this.lblDatabasePassword.Size = new System.Drawing.Size(57, 15);
            this.lblDatabasePassword.TabIndex = 7;
            this.lblDatabasePassword.Text = "Password";
            // 
            // txtDatabasePassword
            // 
            this.txtDatabasePassword.Enabled = false;
            this.txtDatabasePassword.Location = new System.Drawing.Point(161, 177);
            this.txtDatabasePassword.Name = "txtDatabasePassword";
            this.txtDatabasePassword.PasswordChar = '*';
            this.txtDatabasePassword.Size = new System.Drawing.Size(121, 23);
            this.txtDatabasePassword.TabIndex = 9;
            // 
            // lblWebRoot
            // 
            this.lblWebRoot.AutoSize = true;
            this.lblWebRoot.Location = new System.Drawing.Point(5, 16);
            this.lblWebRoot.Name = "lblWebRoot";
            this.lblWebRoot.Size = new System.Drawing.Size(90, 15);
            this.lblWebRoot.TabIndex = 4;
            this.lblWebRoot.Text = "Parent Web Site";
            // 
            // lblDatabaseUserName
            // 
            this.lblDatabaseUserName.AutoSize = true;
            this.lblDatabaseUserName.Enabled = false;
            this.lblDatabaseUserName.Location = new System.Drawing.Point(5, 150);
            this.lblDatabaseUserName.Name = "lblDatabaseUserName";
            this.lblDatabaseUserName.Size = new System.Drawing.Size(65, 15);
            this.lblDatabaseUserName.TabIndex = 6;
            this.lblDatabaseUserName.Text = "User Name";
            // 
            // txtServerName
            // 
            this.txtServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtServerName.Location = new System.Drawing.Point(161, 83);
            this.txtServerName.Name = "txtServerName";
            this.txtServerName.Size = new System.Drawing.Size(396, 23);
            this.txtServerName.TabIndex = 3;
            // 
            // lblServerName
            // 
            this.lblServerName.AutoSize = true;
            this.lblServerName.Location = new System.Drawing.Point(4, 86);
            this.lblServerName.Name = "lblServerName";
            this.lblServerName.Size = new System.Drawing.Size(90, 15);
            this.lblServerName.TabIndex = 5;
            this.lblServerName.Text = "Database Server";
            // 
            // txtDatabaseUserName
            // 
            this.txtDatabaseUserName.Enabled = false;
            this.txtDatabaseUserName.Location = new System.Drawing.Point(161, 147);
            this.txtDatabaseUserName.Name = "txtDatabaseUserName";
            this.txtDatabaseUserName.Size = new System.Drawing.Size(121, 23);
            this.txtDatabaseUserName.TabIndex = 8;
            // 
            // pageReview
            // 
            this.pageReview.Controls.Add(this.lstTasks);
            this.pageReview.IsFinishPage = true;
            this.pageReview.Name = "pageReview";
            this.pageReview.Size = new System.Drawing.Size(574, 316);
            this.pageReview.TabIndex = 1;
            this.pageReview.Text = "Progress";
            this.pageReview.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageReview_Commit);
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
            this.lstTasks.Location = new System.Drawing.Point(0, 0);
            this.lstTasks.Name = "lstTasks";
            this.lstTasks.Size = new System.Drawing.Size(574, 316);
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
            // NewProjectWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(621, 470);
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
            this.pageAzureDeploymentSettings.ResumeLayout(false);
            this.pageAzureDeploymentSettings.PerformLayout();
            this.pagePaths.ResumeLayout(false);
            this.pagePaths.PerformLayout();
            this.pageLocalDeploymentSettings.ResumeLayout(false);
            this.pageLocalDeploymentSettings.PerformLayout();
            this.pageReview.ResumeLayout(false);
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
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkIntegratedAuthentication;
        private System.Windows.Forms.ColumnHeader colStatus;
        private AeroWizard.WizardPage pagePaths;
        private System.Windows.Forms.Label lblBinFolderRoot;
        private System.Windows.Forms.TextBox txtBinFolderRoot;
        private System.Windows.Forms.Label lblBinFolderRootDescription;
        private System.Windows.Forms.Label lblTemplateFolderDescription;
        private System.Windows.Forms.Label lblTemplateFolder;
        private System.Windows.Forms.TextBox txtTemplateFolder;
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
        private System.Windows.Forms.Label lblAdminWarning;
        private System.Windows.Forms.Label lblWelcome1;
        private System.Windows.Forms.Button btnAdminElevate;
        private AeroWizard.WizardPage pageDeploymentType;
        private System.Windows.Forms.ComboBox cboDeploymentType;
        private System.Windows.Forms.Label lblDeploymentType;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private AeroWizard.WizardPage pageAzureDeploymentSettings;
        private System.Windows.Forms.TextBox txtAzurePassword;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox txtAzureUserName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtAzureRedirectUri;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox txtAzureClientId;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox txtAzureTenantId;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtAzureSubscriptionId;
        private System.Windows.Forms.Label label14;
    }
}