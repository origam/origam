using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using AeroWizard;

namespace Origam.UI.WizardForm
{
    partial class Wizard
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
            this.listView1 = new System.Windows.Forms.ListView();
            this.aerowizard1 = new AeroWizard.WizardControl();
            this.StartPage = new AeroWizard.WizardPage();
            this.txtLabel = new System.Windows.Forms.TextBox();
            this.lbTitle = new System.Windows.Forms.Label();
            this.StructureNamePage = new AeroWizard.WizardPage();
            this.label1 = new System.Windows.Forms.Label();
            this.lbName = new System.Windows.Forms.Label();
            this.tbDataStructureName = new System.Windows.Forms.TextBox();
            this.ScreenFormPage = new AeroWizard.WizardPage();
            this.label2 = new System.Windows.Forms.Label();
            this.lstFields = new System.Windows.Forms.CheckedListBox();
            this.lblRole = new System.Windows.Forms.Label();
            this.txtRole = new System.Windows.Forms.TextBox();
            this.LookupFormPage = new AeroWizard.WizardPage();
            this.lblIdFilter = new System.Windows.Forms.Label();
            this.cboIdFilter = new System.Windows.Forms.ComboBox();
            this.lblListFilter = new System.Windows.Forms.Label();
            this.cboListFilter = new System.Windows.Forms.ComboBox();
            this.lblDisplayField = new System.Windows.Forms.Label();
            this.cboDisplayField = new System.Windows.Forms.ComboBox();
            this.lblName = new System.Windows.Forms.Label();
            this.txtName = new System.Windows.Forms.TextBox();
            this.FieldLookupEntity = new AeroWizard.WizardPage();
            this.txtKeyFieldCaption = new System.Windows.Forms.TextBox();
            this.lblKeyFieldCaption = new System.Windows.Forms.Label();
            this.txtNameFieldCaption = new System.Windows.Forms.TextBox();
            this.lblNameFieldCaption = new System.Windows.Forms.Label();
            this.txtKeyFieldName = new System.Windows.Forms.TextBox();
            this.lblKeyFieldName = new System.Windows.Forms.Label();
            this.txtNameFieldName = new System.Windows.Forms.TextBox();
            this.lblNameFieldName = new System.Windows.Forms.Label();
            this.chkTwoColumn = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.grdInitialValues = new System.Windows.Forms.DataGridView();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colDefault = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.chkAllowNulls = new System.Windows.Forms.CheckBox();
            this.txtCaption = new System.Windows.Forms.TextBox();
            this.lookupname = new System.Windows.Forms.TextBox();
            this.lblCaption = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.finishPage = new AeroWizard.WizardPage();
            this.progresslistview = new System.Windows.Forms.ListView();
            this.colText = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.RelationShipEntityPage = new AeroWizard.WizardPage();
            this.checkParentChild = new System.Windows.Forms.CheckBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tableRelation = new System.Windows.Forms.ComboBox();
            this.groupBoxKey = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.RelatedEntityField = new System.Windows.Forms.ComboBox();
            this.BaseEntityField = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.txtKeyName = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtRelationName = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.childEntityPage = new AeroWizard.WizardPage();
            this.label11 = new System.Windows.Forms.Label();
            this.lblEntity2 = new System.Windows.Forms.Label();
            this.cboEntity2 = new System.Windows.Forms.ComboBox();
            this.lblEntity1 = new System.Windows.Forms.Label();
            this.cboEntity1 = new System.Windows.Forms.ComboBox();
            this.label12 = new System.Windows.Forms.Label();
            this.txtchildEntityName = new System.Windows.Forms.TextBox();
            this.foreignKeyPage = new AeroWizard.WizardPage();
            this.label13 = new System.Windows.Forms.Label();
            this.txtfkCaptionName = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.cboLookup = new System.Windows.Forms.ComboBox();
            this.checkBoxAllowNulls = new System.Windows.Forms.CheckBox();
            this.lblField = new System.Windows.Forms.Label();
            this.cboField = new System.Windows.Forms.ComboBox();
            this.lblEntity = new System.Windows.Forms.Label();
            this.cboEntity = new System.Windows.Forms.ComboBox();
            this.label15 = new System.Windows.Forms.Label();
            this.txtFkFieldName = new System.Windows.Forms.TextBox();
            this.menuFromPage = new AeroWizard.WizardPage();
            this.txtMenuCaption = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.txtMenuRole = new System.Windows.Forms.TextBox();
            this.label17 = new System.Windows.Forms.Label();
            this.SummaryPage = new AeroWizard.WizardPage();
            this.richTextBoxSummary = new System.Windows.Forms.RichTextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbProgres = new System.Windows.Forms.TextBox();
            this.colCode = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.txtScreenCaption = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.aerowizard1)).BeginInit();
            this.StartPage.SuspendLayout();
            this.StructureNamePage.SuspendLayout();
            this.ScreenFormPage.SuspendLayout();
            this.LookupFormPage.SuspendLayout();
            this.FieldLookupEntity.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdInitialValues)).BeginInit();
            this.finishPage.SuspendLayout();
            this.RelationShipEntityPage.SuspendLayout();
            this.groupBoxKey.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.childEntityPage.SuspendLayout();
            this.foreignKeyPage.SuspendLayout();
            this.menuFromPage.SuspendLayout();
            this.SummaryPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(17, 72);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(504, 153);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.List;
            // 
            // aerowizard1
            // 
            this.aerowizard1.BackColor = System.Drawing.Color.White;
            this.aerowizard1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.aerowizard1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.aerowizard1.Location = new System.Drawing.Point(0, 0);
            this.aerowizard1.Name = "aerowizard1";
            this.aerowizard1.Pages.Add(this.StartPage);
            this.aerowizard1.Pages.Add(this.StructureNamePage);
            this.aerowizard1.Pages.Add(this.ScreenFormPage);
            this.aerowizard1.Pages.Add(this.LookupFormPage);
            this.aerowizard1.Pages.Add(this.FieldLookupEntity);
            this.aerowizard1.Pages.Add(this.finishPage);
            this.aerowizard1.Pages.Add(this.RelationShipEntityPage);
            this.aerowizard1.Pages.Add(this.childEntityPage);
            this.aerowizard1.Pages.Add(this.foreignKeyPage);
            this.aerowizard1.Pages.Add(this.menuFromPage);
            this.aerowizard1.Pages.Add(this.SummaryPage);
            this.aerowizard1.Size = new System.Drawing.Size(588, 497);
            this.aerowizard1.TabIndex = 0;
            this.aerowizard1.Title = "wiztitle";
            // 
            // StartPage
            // 
            this.StartPage.Controls.Add(this.txtLabel);
            this.StartPage.Controls.Add(this.lbTitle);
            this.StartPage.Controls.Add(this.listView1);
            this.StartPage.Name = "StartPage";
            this.StartPage.Size = new System.Drawing.Size(541, 343);
            this.StartPage.TabIndex = 0;
            this.StartPage.Text = "Page Title";
            this.StartPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.PageStart_Commit);
            this.StartPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.PageStart_Initialize);
            // 
            // txtLabel
            // 
            this.txtLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLabel.Location = new System.Drawing.Point(6, 13);
            this.txtLabel.Multiline = true;
            this.txtLabel.Name = "txtLabel";
            this.txtLabel.ReadOnly = true;
            this.txtLabel.Size = new System.Drawing.Size(165, 20);
            this.txtLabel.TabIndex = 2;
            // 
            // lbTitle
            // 
            this.lbTitle.AutoSize = true;
            this.lbTitle.Location = new System.Drawing.Point(3, 46);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(67, 15);
            this.lbTitle.TabIndex = 1;
            this.lbTitle.Text = "Description";
            // 
            // StructureNamePage
            // 
            this.StructureNamePage.Controls.Add(this.label1);
            this.StructureNamePage.Controls.Add(this.lbName);
            this.StructureNamePage.Controls.Add(this.tbDataStructureName);
            this.StructureNamePage.Name = "StructureNamePage";
            this.StructureNamePage.Size = new System.Drawing.Size(541, 343);
            this.StructureNamePage.TabIndex = 2;
            this.StructureNamePage.Text = "Please Write Name of Structure";
            this.StructureNamePage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.StructureNamePage_Commit);
            this.StructureNamePage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.StructureNamePage_Initialize);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(87, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(228, 34);
            this.label1.TabIndex = 2;
            this.label1.Text = "Name of Structure already exists. Please Fill different Name.";
            // 
            // lbName
            // 
            this.lbName.AutoSize = true;
            this.lbName.Location = new System.Drawing.Point(1, 3);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(39, 15);
            this.lbName.TabIndex = 1;
            this.lbName.Text = "Name";
            // 
            // tbDataStructureName
            // 
            this.tbDataStructureName.Location = new System.Drawing.Point(87, 0);
            this.tbDataStructureName.Name = "tbDataStructureName";
            this.tbDataStructureName.Size = new System.Drawing.Size(228, 23);
            this.tbDataStructureName.TabIndex = 0;
            this.tbDataStructureName.TextChanged += new System.EventHandler(this.tbDataStructureName_TextChanged);
            // 
            // ScreenFormPage
            // 
            this.ScreenFormPage.Controls.Add(this.label18);
            this.ScreenFormPage.Controls.Add(this.txtScreenCaption);
            this.ScreenFormPage.Controls.Add(this.label2);
            this.ScreenFormPage.Controls.Add(this.lstFields);
            this.ScreenFormPage.Controls.Add(this.lblRole);
            this.ScreenFormPage.Controls.Add(this.txtRole);
            this.ScreenFormPage.Name = "ScreenFormPage";
            this.ScreenFormPage.Size = new System.Drawing.Size(541, 343);
            this.ScreenFormPage.TabIndex = 3;
            this.ScreenFormPage.Text = "Please Select Fields";
            this.ScreenFormPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.ScreenFormPage_Commit);
            this.ScreenFormPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.ScreenFormPage_Initialize);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 1);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(302, 15);
            this.label2.TabIndex = 13;
            this.label2.Text = "Select the fields that will be displayed on screen Section:";
            // 
            // lstFields
            // 
            this.lstFields.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstFields.Location = new System.Drawing.Point(3, 36);
            this.lstFields.Name = "lstFields";
            this.lstFields.Size = new System.Drawing.Size(207, 236);
            this.lstFields.Sorted = true;
            this.lstFields.TabIndex = 12;
            // 
            // lblRole
            // 
            this.lblRole.Location = new System.Drawing.Point(246, 39);
            this.lblRole.Name = "lblRole";
            this.lblRole.Size = new System.Drawing.Size(40, 16);
            this.lblRole.TabIndex = 11;
            this.lblRole.Text = "Role:";
            this.lblRole.Visible = false;
            // 
            // txtRole
            // 
            this.txtRole.Location = new System.Drawing.Point(305, 36);
            this.txtRole.Name = "txtRole";
            this.txtRole.Size = new System.Drawing.Size(188, 23);
            this.txtRole.TabIndex = 2;
            this.txtRole.Visible = false;
            // 
            // LookupFormPage
            // 
            this.LookupFormPage.Controls.Add(this.lblIdFilter);
            this.LookupFormPage.Controls.Add(this.cboIdFilter);
            this.LookupFormPage.Controls.Add(this.lblListFilter);
            this.LookupFormPage.Controls.Add(this.cboListFilter);
            this.LookupFormPage.Controls.Add(this.lblDisplayField);
            this.LookupFormPage.Controls.Add(this.cboDisplayField);
            this.LookupFormPage.Controls.Add(this.lblName);
            this.LookupFormPage.Controls.Add(this.txtName);
            this.LookupFormPage.Name = "LookupFormPage";
            this.LookupFormPage.Size = new System.Drawing.Size(541, 343);
            this.LookupFormPage.TabIndex = 4;
            this.LookupFormPage.Text = "Lookup Form";
            this.LookupFormPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.LookupFormPage_Commit);
            this.LookupFormPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.LookupFormPage_Initialize);
            // 
            // lblIdFilter
            // 
            this.lblIdFilter.Location = new System.Drawing.Point(1, 90);
            this.lblIdFilter.Name = "lblIdFilter";
            this.lblIdFilter.Size = new System.Drawing.Size(72, 16);
            this.lblIdFilter.TabIndex = 15;
            this.lblIdFilter.Text = "Id Filter:";
            // 
            // cboIdFilter
            // 
            this.cboIdFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboIdFilter.Location = new System.Drawing.Point(99, 90);
            this.cboIdFilter.Name = "cboIdFilter";
            this.cboIdFilter.Size = new System.Drawing.Size(272, 23);
            this.cboIdFilter.Sorted = true;
            this.cboIdFilter.TabIndex = 3;
            this.cboIdFilter.SelectedIndexChanged += new System.EventHandler(this.cboIdFilter_SelectedIndexChanged);
            // 
            // lblListFilter
            // 
            this.lblListFilter.Location = new System.Drawing.Point(1, 61);
            this.lblListFilter.Name = "lblListFilter";
            this.lblListFilter.Size = new System.Drawing.Size(72, 16);
            this.lblListFilter.TabIndex = 13;
            this.lblListFilter.Text = "List Filter:";
            // 
            // cboListFilter
            // 
            this.cboListFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboListFilter.Location = new System.Drawing.Point(99, 61);
            this.cboListFilter.Name = "cboListFilter";
            this.cboListFilter.Size = new System.Drawing.Size(272, 23);
            this.cboListFilter.Sorted = true;
            this.cboListFilter.TabIndex = 2;
            // 
            // lblDisplayField
            // 
            this.lblDisplayField.Location = new System.Drawing.Point(1, 32);
            this.lblDisplayField.Name = "lblDisplayField";
            this.lblDisplayField.Size = new System.Drawing.Size(92, 23);
            this.lblDisplayField.TabIndex = 11;
            this.lblDisplayField.Text = "Display Field:";
            // 
            // cboDisplayField
            // 
            this.cboDisplayField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDisplayField.Location = new System.Drawing.Point(99, 32);
            this.cboDisplayField.Name = "cboDisplayField";
            this.cboDisplayField.Size = new System.Drawing.Size(272, 23);
            this.cboDisplayField.Sorted = true;
            this.cboDisplayField.TabIndex = 1;
            this.cboDisplayField.SelectedIndexChanged += new System.EventHandler(this.CboDisplayField_SelectedIndexChanged);
            // 
            // lblName
            // 
            this.lblName.Location = new System.Drawing.Point(1, 3);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(72, 16);
            this.lblName.TabIndex = 9;
            this.lblName.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(99, 3);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(272, 23);
            this.txtName.TabIndex = 0;
            // 
            // FieldLookupEntity
            // 
            this.FieldLookupEntity.Controls.Add(this.txtKeyFieldCaption);
            this.FieldLookupEntity.Controls.Add(this.lblKeyFieldCaption);
            this.FieldLookupEntity.Controls.Add(this.txtNameFieldCaption);
            this.FieldLookupEntity.Controls.Add(this.lblNameFieldCaption);
            this.FieldLookupEntity.Controls.Add(this.txtKeyFieldName);
            this.FieldLookupEntity.Controls.Add(this.lblKeyFieldName);
            this.FieldLookupEntity.Controls.Add(this.txtNameFieldName);
            this.FieldLookupEntity.Controls.Add(this.lblNameFieldName);
            this.FieldLookupEntity.Controls.Add(this.chkTwoColumn);
            this.FieldLookupEntity.Controls.Add(this.label3);
            this.FieldLookupEntity.Controls.Add(this.grdInitialValues);
            this.FieldLookupEntity.Controls.Add(this.chkAllowNulls);
            this.FieldLookupEntity.Controls.Add(this.txtCaption);
            this.FieldLookupEntity.Controls.Add(this.lookupname);
            this.FieldLookupEntity.Controls.Add(this.lblCaption);
            this.FieldLookupEntity.Controls.Add(this.label4);
            this.FieldLookupEntity.Name = "FieldLookupEntity";
            this.FieldLookupEntity.Size = new System.Drawing.Size(541, 343);
            this.FieldLookupEntity.TabIndex = 5;
            this.FieldLookupEntity.Text = "Create Field With Lookup Entity";
            this.FieldLookupEntity.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.FieldLookupEntityPage_Commit);
            this.FieldLookupEntity.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.FieldLookupEntityPage_Initialize);
            // 
            // txtKeyFieldCaption
            // 
            this.txtKeyFieldCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKeyFieldCaption.Location = new System.Drawing.Point(270, 128);
            this.txtKeyFieldCaption.Name = "txtKeyFieldCaption";
            this.txtKeyFieldCaption.Size = new System.Drawing.Size(186, 23);
            this.txtKeyFieldCaption.TabIndex = 7;
            this.txtKeyFieldCaption.Visible = false;
            this.txtKeyFieldCaption.TextChanged += new System.EventHandler(this.TxtKeyFieldCaption_TextChanged);
            // 
            // lblKeyFieldCaption
            // 
            this.lblKeyFieldCaption.Location = new System.Drawing.Point(223, 131);
            this.lblKeyFieldCaption.Name = "lblKeyFieldCaption";
            this.lblKeyFieldCaption.Size = new System.Drawing.Size(51, 20);
            this.lblKeyFieldCaption.TabIndex = 28;
            this.lblKeyFieldCaption.Text = "Caption";
            this.lblKeyFieldCaption.Visible = false;
            // 
            // txtNameFieldCaption
            // 
            this.txtNameFieldCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtNameFieldCaption.Location = new System.Drawing.Point(270, 97);
            this.txtNameFieldCaption.Name = "txtNameFieldCaption";
            this.txtNameFieldCaption.Size = new System.Drawing.Size(186, 23);
            this.txtNameFieldCaption.TabIndex = 5;
            this.txtNameFieldCaption.TextChanged += new System.EventHandler(this.TxtNameFieldCaption_TextChanged);
            // 
            // lblNameFieldCaption
            // 
            this.lblNameFieldCaption.Location = new System.Drawing.Point(223, 100);
            this.lblNameFieldCaption.Name = "lblNameFieldCaption";
            this.lblNameFieldCaption.Size = new System.Drawing.Size(51, 20);
            this.lblNameFieldCaption.TabIndex = 24;
            this.lblNameFieldCaption.Text = "Caption";
            // 
            // txtKeyFieldName
            // 
            this.txtKeyFieldName.Location = new System.Drawing.Point(116, 128);
            this.txtKeyFieldName.Name = "txtKeyFieldName";
            this.txtKeyFieldName.Size = new System.Drawing.Size(101, 23);
            this.txtKeyFieldName.TabIndex = 6;
            this.txtKeyFieldName.Visible = false;
            // 
            // lblKeyFieldName
            // 
            this.lblKeyFieldName.Location = new System.Drawing.Point(1, 131);
            this.lblKeyFieldName.Name = "lblKeyFieldName";
            this.lblKeyFieldName.Size = new System.Drawing.Size(116, 20);
            this.lblKeyFieldName.TabIndex = 26;
            this.lblKeyFieldName.Text = "\"Key\" Field Name";
            this.lblKeyFieldName.Visible = false;
            // 
            // txtNameFieldName
            // 
            this.txtNameFieldName.Location = new System.Drawing.Point(116, 97);
            this.txtNameFieldName.Name = "txtNameFieldName";
            this.txtNameFieldName.Size = new System.Drawing.Size(101, 23);
            this.txtNameFieldName.TabIndex = 4;
            // 
            // lblNameFieldName
            // 
            this.lblNameFieldName.Location = new System.Drawing.Point(1, 100);
            this.lblNameFieldName.Name = "lblNameFieldName";
            this.lblNameFieldName.Size = new System.Drawing.Size(116, 20);
            this.lblNameFieldName.TabIndex = 22;
            this.lblNameFieldName.Text = "\"Name\" Field Name";
            // 
            // chkTwoColumn
            // 
            this.chkTwoColumn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkTwoColumn.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkTwoColumn.Location = new System.Drawing.Point(270, 67);
            this.chkTwoColumn.Name = "chkTwoColumn";
            this.chkTwoColumn.Size = new System.Drawing.Size(186, 24);
            this.chkTwoColumn.TabIndex = 3;
            this.chkTwoColumn.Text = "Two-Column (Key, Name)";
            this.chkTwoColumn.CheckedChanged += new System.EventHandler(this.ChkTwoColumn_CheckedChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1, 164);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 15);
            this.label3.TabIndex = 30;
            this.label3.Text = "Initial values:";
            // 
            // grdInitialValues
            // 
            this.grdInitialValues.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grdInitialValues.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grdInitialValues.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.colDefault});
            this.grdInitialValues.Location = new System.Drawing.Point(1, 192);
            this.grdInitialValues.Name = "grdInitialValues";
            this.grdInitialValues.Size = new System.Drawing.Size(455, 128);
            this.grdInitialValues.TabIndex = 8;
            // 
            // colName
            // 
            this.colName.DataPropertyName = "Name";
            this.colName.Name = "colName";
            // 
            // colDefault
            // 
            this.colDefault.DataPropertyName = "IsDefault";
            this.colDefault.HeaderText = "Default";
            this.colDefault.Name = "colDefault";
            // 
            // chkAllowNulls
            // 
            this.chkAllowNulls.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkAllowNulls.Location = new System.Drawing.Point(1, 67);
            this.chkAllowNulls.Name = "chkAllowNulls";
            this.chkAllowNulls.Size = new System.Drawing.Size(129, 24);
            this.chkAllowNulls.TabIndex = 2;
            this.chkAllowNulls.Text = "Allow Nulls";
            // 
            // txtCaption
            // 
            this.txtCaption.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtCaption.Location = new System.Drawing.Point(116, 32);
            this.txtCaption.Name = "txtCaption";
            this.txtCaption.Size = new System.Drawing.Size(340, 23);
            this.txtCaption.TabIndex = 1;
            // 
            // lookupname
            // 
            this.lookupname.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lookupname.Location = new System.Drawing.Point(116, 0);
            this.lookupname.Name = "lookupname";
            this.lookupname.Size = new System.Drawing.Size(340, 23);
            this.lookupname.TabIndex = 0;
            // 
            // lblCaption
            // 
            this.lblCaption.Location = new System.Drawing.Point(1, 35);
            this.lblCaption.Name = "lblCaption";
            this.lblCaption.Size = new System.Drawing.Size(116, 20);
            this.lblCaption.TabIndex = 18;
            this.lblCaption.Text = "Caption";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(1, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 20);
            this.label4.TabIndex = 16;
            this.label4.Text = "Lookup Entity Name";
            // 
            // finishPage
            // 
            this.finishPage.AllowBack = false;
            this.finishPage.Controls.Add(this.progresslistview);
            this.finishPage.Name = "finishPage";
            this.finishPage.Size = new System.Drawing.Size(541, 343);
            this.finishPage.TabIndex = 6;
            this.finishPage.Text = "Progress";
            this.finishPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.FinishPage_Commit);
            this.finishPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.FinishPage_Initialize);
            // 
            // progresslistview
            // 
            this.progresslistview.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colText,
            this.colStatus});
            this.progresslistview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.progresslistview.FullRowSelect = true;
            this.progresslistview.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.progresslistview.HideSelection = false;
            this.progresslistview.Location = new System.Drawing.Point(0, 0);
            this.progresslistview.Name = "progresslistview";
            this.progresslistview.Size = new System.Drawing.Size(541, 343);
            this.progresslistview.TabIndex = 0;
            this.progresslistview.UseCompatibleStateImageBehavior = false;
            this.progresslistview.View = System.Windows.Forms.View.Details;
            // 
            // colText
            // 
            this.colText.Text = "Found In";
            this.colText.Width = 301;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Type";
            this.colStatus.Width = 228;
            // 
            // RelationShipEntityPage
            // 
            this.RelationShipEntityPage.Controls.Add(this.checkParentChild);
            this.RelationShipEntityPage.Controls.Add(this.label6);
            this.RelationShipEntityPage.Controls.Add(this.tableRelation);
            this.RelationShipEntityPage.Controls.Add(this.groupBoxKey);
            this.RelationShipEntityPage.Controls.Add(this.txtRelationName);
            this.RelationShipEntityPage.Controls.Add(this.label10);
            this.RelationShipEntityPage.Name = "RelationShipEntityPage";
            this.RelationShipEntityPage.Size = new System.Drawing.Size(541, 343);
            this.RelationShipEntityPage.TabIndex = 7;
            this.RelationShipEntityPage.Text = "Create Field With Relationship Entity Wizard";
            this.RelationShipEntityPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.RelationShipEntityPage_Commit);
            this.RelationShipEntityPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.RelationShipEntityPage_Initialize);
            // 
            // checkParentChild
            // 
            this.checkParentChild.AutoSize = true;
            this.checkParentChild.Location = new System.Drawing.Point(132, 26);
            this.checkParentChild.Name = "checkParentChild";
            this.checkParentChild.Size = new System.Drawing.Size(96, 19);
            this.checkParentChild.TabIndex = 1;
            this.checkParentChild.Text = "isParentChild";
            this.checkParentChild.UseVisualStyleBackColor = true;
            this.checkParentChild.CheckedChanged += new System.EventHandler(this.CheckParentChild_CheckedChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(5, 61);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(34, 15);
            this.label6.TabIndex = 28;
            this.label6.Text = "Table";
            // 
            // tableRelation
            // 
            this.tableRelation.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.tableRelation.Location = new System.Drawing.Point(132, 61);
            this.tableRelation.Name = "tableRelation";
            this.tableRelation.Size = new System.Drawing.Size(298, 23);
            this.tableRelation.Sorted = true;
            this.tableRelation.TabIndex = 2;
            this.tableRelation.SelectedIndexChanged += new System.EventHandler(this.TableRelation_SelectedIndexChanged);
            // 
            // groupBoxKey
            // 
            this.groupBoxKey.AutoSize = true;
            this.groupBoxKey.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.groupBoxKey.Controls.Add(this.groupBox2);
            this.groupBoxKey.Controls.Add(this.txtKeyName);
            this.groupBoxKey.Controls.Add(this.label9);
            this.groupBoxKey.Enabled = false;
            this.groupBoxKey.Location = new System.Drawing.Point(4, 103);
            this.groupBoxKey.Name = "groupBoxKey";
            this.groupBoxKey.Size = new System.Drawing.Size(426, 179);
            this.groupBoxKey.TabIndex = 27;
            this.groupBoxKey.TabStop = false;
            this.groupBoxKey.Text = "Key";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.RelatedEntityField);
            this.groupBox2.Controls.Add(this.BaseEntityField);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Location = new System.Drawing.Point(21, 65);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(399, 92);
            this.groupBox2.TabIndex = 23;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Misc";
            // 
            // RelatedEntityField
            // 
            this.RelatedEntityField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RelatedEntityField.Location = new System.Drawing.Point(107, 43);
            this.RelatedEntityField.Name = "RelatedEntityField";
            this.RelatedEntityField.Size = new System.Drawing.Size(292, 23);
            this.RelatedEntityField.Sorted = true;
            this.RelatedEntityField.TabIndex = 5;
            this.RelatedEntityField.SelectedIndexChanged += new System.EventHandler(this.RelatedEntityField_SelectedIndexChanged);
            // 
            // BaseEntityField
            // 
            this.BaseEntityField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.BaseEntityField.Location = new System.Drawing.Point(107, 15);
            this.BaseEntityField.Name = "BaseEntityField";
            this.BaseEntityField.Size = new System.Drawing.Size(292, 23);
            this.BaseEntityField.Sorted = true;
            this.BaseEntityField.TabIndex = 4;
            this.BaseEntityField.SelectedIndexChanged += new System.EventHandler(this.BaseEntityField_SelectedIndexChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 46);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(101, 15);
            this.label7.TabIndex = 3;
            this.label7.Text = "RelatedEntityField";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 23);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(86, 15);
            this.label8.TabIndex = 2;
            this.label8.Text = "BaseEntityField";
            // 
            // txtKeyName
            // 
            this.txtKeyName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtKeyName.Location = new System.Drawing.Point(128, 28);
            this.txtKeyName.Name = "txtKeyName";
            this.txtKeyName.Size = new System.Drawing.Size(292, 23);
            this.txtKeyName.TabIndex = 3;
            // 
            // label9
            // 
            this.label9.Location = new System.Drawing.Point(25, 28);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(97, 20);
            this.label9.TabIndex = 22;
            this.label9.Text = "\"Name\"";
            // 
            // txtRelationName
            // 
            this.txtRelationName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRelationName.Location = new System.Drawing.Point(133, 0);
            this.txtRelationName.Name = "txtRelationName";
            this.txtRelationName.Size = new System.Drawing.Size(292, 23);
            this.txtRelationName.TabIndex = 0;
            // 
            // label10
            // 
            this.label10.Location = new System.Drawing.Point(1, 3);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(116, 20);
            this.label10.TabIndex = 24;
            this.label10.Text = "Relation \"Name\"";
            // 
            // childEntityPage
            // 
            this.childEntityPage.Controls.Add(this.label11);
            this.childEntityPage.Controls.Add(this.lblEntity2);
            this.childEntityPage.Controls.Add(this.cboEntity2);
            this.childEntityPage.Controls.Add(this.lblEntity1);
            this.childEntityPage.Controls.Add(this.cboEntity1);
            this.childEntityPage.Controls.Add(this.label12);
            this.childEntityPage.Controls.Add(this.txtchildEntityName);
            this.childEntityPage.Name = "childEntityPage";
            this.childEntityPage.Size = new System.Drawing.Size(541, 343);
            this.childEntityPage.TabIndex = 8;
            this.childEntityPage.Text = "Create Child Entity";
            this.childEntityPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.ChildEntityPage_Commit);
            this.childEntityPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.ChildEntityPage_Initialize);
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(1, 72);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(360, 16);
            this.label11.TabIndex = 17;
            this.label11.Text = "Optional Foreign entity. Foreign key will be created in the new entity.";
            // 
            // lblEntity2
            // 
            this.lblEntity2.Location = new System.Drawing.Point(1, 101);
            this.lblEntity2.Name = "lblEntity2";
            this.lblEntity2.Size = new System.Drawing.Size(96, 16);
            this.lblEntity2.TabIndex = 16;
            this.lblEntity2.Text = "Foreign Entity:";
            // 
            // cboEntity2
            // 
            this.cboEntity2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEntity2.Location = new System.Drawing.Point(113, 101);
            this.cboEntity2.Name = "cboEntity2";
            this.cboEntity2.Size = new System.Drawing.Size(248, 23);
            this.cboEntity2.Sorted = true;
            this.cboEntity2.TabIndex = 2;
            this.cboEntity2.SelectedIndexChanged += new System.EventHandler(this.CboEntity2_SelectedIndexChanged);
            // 
            // lblEntity1
            // 
            this.lblEntity1.Location = new System.Drawing.Point(1, 32);
            this.lblEntity1.Name = "lblEntity1";
            this.lblEntity1.Size = new System.Drawing.Size(96, 16);
            this.lblEntity1.TabIndex = 15;
            this.lblEntity1.Text = "Master Entity:";
            // 
            // cboEntity1
            // 
            this.cboEntity1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEntity1.Location = new System.Drawing.Point(113, 32);
            this.cboEntity1.Name = "cboEntity1";
            this.cboEntity1.Size = new System.Drawing.Size(248, 23);
            this.cboEntity1.Sorted = true;
            this.cboEntity1.TabIndex = 1;
            this.cboEntity1.SelectedIndexChanged += new System.EventHandler(this.CboEntity1_SelectedIndexChanged);
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(1, 3);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(104, 16);
            this.label12.TabIndex = 12;
            this.label12.Text = "Child Entity Name:";
            // 
            // txtchildEntityName
            // 
            this.txtchildEntityName.Location = new System.Drawing.Point(113, 3);
            this.txtchildEntityName.Name = "txtchildEntityName";
            this.txtchildEntityName.Size = new System.Drawing.Size(248, 23);
            this.txtchildEntityName.TabIndex = 0;
            // 
            // foreignKeyPage
            // 
            this.foreignKeyPage.Controls.Add(this.label13);
            this.foreignKeyPage.Controls.Add(this.txtfkCaptionName);
            this.foreignKeyPage.Controls.Add(this.label14);
            this.foreignKeyPage.Controls.Add(this.cboLookup);
            this.foreignKeyPage.Controls.Add(this.checkBoxAllowNulls);
            this.foreignKeyPage.Controls.Add(this.lblField);
            this.foreignKeyPage.Controls.Add(this.cboField);
            this.foreignKeyPage.Controls.Add(this.lblEntity);
            this.foreignKeyPage.Controls.Add(this.cboEntity);
            this.foreignKeyPage.Controls.Add(this.label15);
            this.foreignKeyPage.Controls.Add(this.txtFkFieldName);
            this.foreignKeyPage.Name = "foreignKeyPage";
            this.foreignKeyPage.Size = new System.Drawing.Size(541, 343);
            this.foreignKeyPage.TabIndex = 9;
            this.foreignKeyPage.Text = "Create Foreign Key Wizard";
            this.foreignKeyPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.ForeignKeyPage_Commit);
            this.foreignKeyPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.ForeignKeyPage_Initialize);
            // 
            // label13
            // 
            this.label13.Location = new System.Drawing.Point(0, 162);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(93, 23);
            this.label13.TabIndex = 20;
            this.label13.Text = "Caption:";
            // 
            // txtfkCaptionName
            // 
            this.txtfkCaptionName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtfkCaptionName.Location = new System.Drawing.Point(112, 162);
            this.txtfkCaptionName.Name = "txtfkCaptionName";
            this.txtfkCaptionName.Size = new System.Drawing.Size(399, 23);
            this.txtfkCaptionName.TabIndex = 5;
            // 
            // label14
            // 
            this.label14.Location = new System.Drawing.Point(0, 61);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(93, 22);
            this.label14.TabIndex = 15;
            this.label14.Text = "Lookup:";
            // 
            // cboLookup
            // 
            this.cboLookup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboLookup.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboLookup.Location = new System.Drawing.Point(112, 61);
            this.cboLookup.Name = "cboLookup";
            this.cboLookup.Size = new System.Drawing.Size(399, 23);
            this.cboLookup.Sorted = true;
            this.cboLookup.TabIndex = 2;
            // 
            // checkBoxAllowNulls
            // 
            this.checkBoxAllowNulls.Location = new System.Drawing.Point(0, 93);
            this.checkBoxAllowNulls.Name = "checkBoxAllowNulls";
            this.checkBoxAllowNulls.Size = new System.Drawing.Size(106, 28);
            this.checkBoxAllowNulls.TabIndex = 3;
            this.checkBoxAllowNulls.Text = "Allow Nulls";
            // 
            // lblField
            // 
            this.lblField.Location = new System.Drawing.Point(0, 32);
            this.lblField.Name = "lblField";
            this.lblField.Size = new System.Drawing.Size(93, 23);
            this.lblField.TabIndex = 13;
            this.lblField.Text = "Foreign Field:";
            // 
            // cboField
            // 
            this.cboField.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboField.Location = new System.Drawing.Point(112, 32);
            this.cboField.Name = "cboField";
            this.cboField.Size = new System.Drawing.Size(399, 23);
            this.cboField.Sorted = true;
            this.cboField.TabIndex = 1;
            this.cboField.SelectedIndexChanged += new System.EventHandler(this.CboField_SelectedIndexChanged);
            // 
            // lblEntity
            // 
            this.lblEntity.Location = new System.Drawing.Point(0, 3);
            this.lblEntity.Name = "lblEntity";
            this.lblEntity.Size = new System.Drawing.Size(106, 23);
            this.lblEntity.TabIndex = 11;
            this.lblEntity.Text = "Foreign Entity:";
            // 
            // cboEntity
            // 
            this.cboEntity.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.cboEntity.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboEntity.Location = new System.Drawing.Point(112, 3);
            this.cboEntity.Name = "cboEntity";
            this.cboEntity.Size = new System.Drawing.Size(399, 23);
            this.cboEntity.Sorted = true;
            this.cboEntity.TabIndex = 0;
            this.cboEntity.SelectedIndexChanged += new System.EventHandler(this.CboEntity_SelectedIndexChanged);
            // 
            // label15
            // 
            this.label15.Location = new System.Drawing.Point(0, 133);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(93, 23);
            this.label15.TabIndex = 18;
            this.label15.Text = "FK Field Name:";
            // 
            // txtFkFieldName
            // 
            this.txtFkFieldName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFkFieldName.Location = new System.Drawing.Point(112, 133);
            this.txtFkFieldName.Name = "txtFkFieldName";
            this.txtFkFieldName.Size = new System.Drawing.Size(399, 23);
            this.txtFkFieldName.TabIndex = 4;
            // 
            // menuFromPage
            // 
            this.menuFromPage.Controls.Add(this.txtMenuCaption);
            this.menuFromPage.Controls.Add(this.label16);
            this.menuFromPage.Controls.Add(this.txtMenuRole);
            this.menuFromPage.Controls.Add(this.label17);
            this.menuFromPage.Name = "menuFromPage";
            this.menuFromPage.Size = new System.Drawing.Size(541, 343);
            this.menuFromPage.TabIndex = 10;
            this.menuFromPage.Text = "Create Menu Form";
            this.menuFromPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.MenuFromPage_Commit);
            this.menuFromPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.MenuFromPage_Initialize);
            // 
            // txtMenuCaption
            // 
            this.txtMenuCaption.Location = new System.Drawing.Point(83, 3);
            this.txtMenuCaption.Name = "txtMenuCaption";
            this.txtMenuCaption.Size = new System.Drawing.Size(277, 23);
            this.txtMenuCaption.TabIndex = 0;
            // 
            // label16
            // 
            this.label16.Location = new System.Drawing.Point(2, 3);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(65, 23);
            this.label16.TabIndex = 16;
            this.label16.Text = "Caption:";
            // 
            // txtMenuRole
            // 
            this.txtMenuRole.Location = new System.Drawing.Point(83, 32);
            this.txtMenuRole.Name = "txtMenuRole";
            this.txtMenuRole.Size = new System.Drawing.Size(277, 23);
            this.txtMenuRole.TabIndex = 1;
            // 
            // label17
            // 
            this.label17.Location = new System.Drawing.Point(2, 32);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(65, 16);
            this.label17.TabIndex = 15;
            this.label17.Text = "Role:";
            // 
            // SummaryPage
            // 
            this.SummaryPage.Controls.Add(this.richTextBoxSummary);
            this.SummaryPage.Name = "SummaryPage";
            this.SummaryPage.Size = new System.Drawing.Size(541, 343);
            this.SummaryPage.TabIndex = 11;
            this.SummaryPage.Text = "Summary";
            this.SummaryPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.SummaryPage_Commit);
            this.SummaryPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.SummaryPage_Initialize);
            // 
            // richTextBoxSummary
            // 
            this.richTextBoxSummary.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.richTextBoxSummary.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.richTextBoxSummary.Location = new System.Drawing.Point(4, 4);
            this.richTextBoxSummary.Margin = new System.Windows.Forms.Padding(0);
            this.richTextBoxSummary.Name = "richTextBoxSummary";
            this.richTextBoxSummary.ReadOnly = true;
            this.richTextBoxSummary.Size = new System.Drawing.Size(534, 336);
            this.richTextBoxSummary.TabIndex = 0;
            this.richTextBoxSummary.Text = "";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(23, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(52, 15);
            this.label5.TabIndex = 1;
            this.label5.Text = "Progress";
            // 
            // tbProgres
            // 
            this.tbProgres.Location = new System.Drawing.Point(23, 47);
            this.tbProgres.Multiline = true;
            this.tbProgres.Name = "tbProgres";
            this.tbProgres.Size = new System.Drawing.Size(495, 260);
            this.tbProgres.TabIndex = 0;
            // 
            // colCode
            // 
            this.colCode.DataPropertyName = "Code";
            this.colCode.Name = "colCode";
            // 
            // txtScreenCaption
            // 
            this.txtScreenCaption.Location = new System.Drawing.Point(305, 81);
            this.txtScreenCaption.Name = "txtScreenCaption";
            this.txtScreenCaption.Size = new System.Drawing.Size(188, 23);
            this.txtScreenCaption.TabIndex = 14;
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(246, 84);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(52, 15);
            this.label18.TabIndex = 15;
            this.label18.Text = "Caption:";
            // 
            // Wizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(588, 497);
            this.ControlBox = false;
            this.Controls.Add(this.aerowizard1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Wizard";
            this.Text = "Create Menu Item Wizard";
            ((System.ComponentModel.ISupportInitialize)(this.aerowizard1)).EndInit();
            this.StartPage.ResumeLayout(false);
            this.StartPage.PerformLayout();
            this.StructureNamePage.ResumeLayout(false);
            this.StructureNamePage.PerformLayout();
            this.ScreenFormPage.ResumeLayout(false);
            this.ScreenFormPage.PerformLayout();
            this.LookupFormPage.ResumeLayout(false);
            this.LookupFormPage.PerformLayout();
            this.FieldLookupEntity.ResumeLayout(false);
            this.FieldLookupEntity.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grdInitialValues)).EndInit();
            this.finishPage.ResumeLayout(false);
            this.RelationShipEntityPage.ResumeLayout(false);
            this.RelationShipEntityPage.PerformLayout();
            this.groupBoxKey.ResumeLayout(false);
            this.groupBoxKey.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.childEntityPage.ResumeLayout(false);
            this.childEntityPage.PerformLayout();
            this.foreignKeyPage.ResumeLayout(false);
            this.foreignKeyPage.PerformLayout();
            this.menuFromPage.ResumeLayout(false);
            this.menuFromPage.PerformLayout();
            this.SummaryPage.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
        private AeroWizard.WizardControl aerowizard1;
        private AeroWizard.WizardPage StartPage;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label lbTitle;
        private WizardPage StructureNamePage;
        private System.Windows.Forms.Label lbName;
        private System.Windows.Forms.TextBox tbDataStructureName;
        private System.Windows.Forms.Label label1;
        private WizardPage ScreenFormPage;
        private System.Windows.Forms.TextBox txtRole;
        private System.Windows.Forms.Label lblRole;
        private System.Windows.Forms.CheckedListBox lstFields;
        private System.Windows.Forms.Label label2;
        private WizardPage LookupFormPage;
        private System.Windows.Forms.Label lblIdFilter;
        private System.Windows.Forms.ComboBox cboIdFilter;
        private System.Windows.Forms.Label lblListFilter;
        private System.Windows.Forms.ComboBox cboListFilter;
        private System.Windows.Forms.Label lblDisplayField;
        private System.Windows.Forms.ComboBox cboDisplayField;
        private System.Windows.Forms.Label lblName;
        private System.Windows.Forms.TextBox txtName;
        private WizardPage FieldLookupEntity;
        private TextBox txtKeyFieldCaption;
        private Label lblKeyFieldCaption;
        private TextBox txtNameFieldCaption;
        private Label lblNameFieldCaption;
        private TextBox txtKeyFieldName;
        private Label lblKeyFieldName;
        private TextBox txtNameFieldName;
        private Label lblNameFieldName;
        private CheckBox chkTwoColumn;
        private Label label3;
        private DataGridView grdInitialValues;
        private DataGridViewCheckBoxColumn colDefault;
        private DataGridViewTextBoxColumn colCode;
        private DataGridViewTextBoxColumn colName;
        private CheckBox chkAllowNulls;
        private TextBox txtCaption;
        private TextBox lookupname;
        private Label lblCaption;
        private Label label4;
        private WizardPage finishPage;
        public ListView progressListview;
        private ColumnHeader colText;
        private ColumnHeader colStatus;
        private Label label5;
        private TextBox tbProgres;
        private WizardPage RelationShipEntityPage;
        private CheckBox checkParentChild;
        private Label label6;
        private ComboBox tableRelation;
        private GroupBox groupBoxKey;
        private GroupBox groupBox2;
        private ComboBox RelatedEntityField;
        private ComboBox BaseEntityField;
        private Label label7;
        private Label label8;
        private TextBox txtKeyName;
        private Label label9;
        private TextBox txtRelationName;
        private Label label10;
        private WizardPage childEntityPage;
        private Label label11;
        private Label lblEntity2;
        private ComboBox cboEntity2;
        private Label lblEntity1;
        private ComboBox cboEntity1;
        private Label label12;
        private TextBox txtchildEntityName;
        private WizardPage foreignKeyPage;
        private Label label13;
        private TextBox txtfkCaptionName;
        private Label label14;
        private ComboBox cboLookup;
        private CheckBox checkBoxAllowNulls;
        private Label lblField;
        private ComboBox cboField;
        private Label lblEntity;
        private ComboBox cboEntity;
        private Label label15;
        private TextBox txtFkFieldName;
        private WizardPage menuFromPage;
        private TextBox txtMenuCaption;
        private Label label16;
        private TextBox txtMenuRole;
        private Label label17;
        private TextBox txtLabel;
        private ListView progresslistview;
        private WizardPage SummaryPage;
        private RichTextBox richTextBoxSummary;
        private Label label18;
        private TextBox txtScreenCaption;
    }
}

