using System;
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
            this.lbTitle = new System.Windows.Forms.Label();
            this.DataStructureNamePage = new AeroWizard.WizardPage();
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
            ((System.ComponentModel.ISupportInitialize)(this.aerowizard1)).BeginInit();
            this.StartPage.SuspendLayout();
            this.DataStructureNamePage.SuspendLayout();
            this.ScreenFormPage.SuspendLayout();
            this.LookupFormPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.listView1.Location = new System.Drawing.Point(10, 33);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(455, 127);
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
            this.aerowizard1.Pages.Add(this.DataStructureNamePage);
            this.aerowizard1.Pages.Add(this.ScreenFormPage);
            this.aerowizard1.Pages.Add(this.LookupFormPage);
            this.aerowizard1.Size = new System.Drawing.Size(534, 400);
            this.aerowizard1.TabIndex = 0;
            // 
            // StartPage
            // 
            this.StartPage.Controls.Add(this.lbTitle);
            this.StartPage.Controls.Add(this.listView1);
            this.StartPage.Name = "StartPage";
            this.StartPage.Size = new System.Drawing.Size(487, 246);
            this.StartPage.TabIndex = 0;
            this.StartPage.Text = "Page Title";
            this.StartPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.PageStart_Commit);
            this.StartPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.PageStart_Initialize);
            // 
            // lbTitle
            // 
            this.lbTitle.AutoSize = true;
            this.lbTitle.Location = new System.Drawing.Point(11, 8);
            this.lbTitle.Name = "lbTitle";
            this.lbTitle.Size = new System.Drawing.Size(67, 15);
            this.lbTitle.TabIndex = 1;
            this.lbTitle.Text = "Description";
            // 
            // DataStructureNamePage
            // 
            this.DataStructureNamePage.Controls.Add(this.label1);
            this.DataStructureNamePage.Controls.Add(this.lbName);
            this.DataStructureNamePage.Controls.Add(this.tbDataStructureName);
            this.DataStructureNamePage.Name = "DataStructureNamePage";
            this.DataStructureNamePage.Size = new System.Drawing.Size(487, 246);
            this.DataStructureNamePage.TabIndex = 2;
            this.DataStructureNamePage.Text = "Please Write Name of Datastructure";
            this.DataStructureNamePage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.DataStructureNamePage_Commit);
            this.DataStructureNamePage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.DataStructureNamePage_Initialize);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(131, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(228, 34);
            this.label1.TabIndex = 2;
            this.label1.Text = "Name of DataStructure already exists. Please Fill different Name.";
            // 
            // lbName
            // 
            this.lbName.AutoSize = true;
            this.lbName.Location = new System.Drawing.Point(45, 36);
            this.lbName.Name = "lbName";
            this.lbName.Size = new System.Drawing.Size(39, 15);
            this.lbName.TabIndex = 1;
            this.lbName.Text = "Name";
            // 
            // tbDataStructureName
            // 
            this.tbDataStructureName.Location = new System.Drawing.Point(131, 33);
            this.tbDataStructureName.Name = "tbDataStructureName";
            this.tbDataStructureName.Size = new System.Drawing.Size(228, 23);
            this.tbDataStructureName.TabIndex = 0;
            // 
            // ScreenFormPage
            // 
            this.ScreenFormPage.Controls.Add(this.label2);
            this.ScreenFormPage.Controls.Add(this.lstFields);
            this.ScreenFormPage.Controls.Add(this.lblRole);
            this.ScreenFormPage.Controls.Add(this.txtRole);
            this.ScreenFormPage.Name = "ScreenFormPage";
            this.ScreenFormPage.Size = new System.Drawing.Size(487, 246);
            this.ScreenFormPage.TabIndex = 3;
            this.ScreenFormPage.Text = "Page Title";
            this.ScreenFormPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.ScreenFormPage_Commit);
            this.ScreenFormPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.ScreenFormPage_Initialize);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 17);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(302, 15);
            this.label2.TabIndex = 13;
            this.label2.Text = "Select the fields that will be displayed on screen Section:";
            // 
            // lstFields
            // 
            this.lstFields.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstFields.Location = new System.Drawing.Point(19, 52);
            this.lstFields.Name = "lstFields";
            this.lstFields.Size = new System.Drawing.Size(207, 162);
            this.lstFields.Sorted = true;
            this.lstFields.TabIndex = 12;
            // 
            // lblRole
            // 
            this.lblRole.Location = new System.Drawing.Point(285, 111);
            this.lblRole.Name = "lblRole";
            this.lblRole.Size = new System.Drawing.Size(40, 16);
            this.lblRole.TabIndex = 11;
            this.lblRole.Text = "Role:";
            this.lblRole.Visible = false;
            // 
            // txtRole
            // 
            this.txtRole.Location = new System.Drawing.Point(288, 149);
            this.txtRole.Name = "txtRole";
            this.txtRole.Size = new System.Drawing.Size(160, 23);
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
            this.LookupFormPage.Size = new System.Drawing.Size(487, 246);
            this.LookupFormPage.TabIndex = 4;
            this.LookupFormPage.Text = "Lookup Form";
            this.LookupFormPage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.LookupFormPage_Commit);
            this.LookupFormPage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.LookupFormPage_Initialize);
            // 
            // lblIdFilter
            // 
            this.lblIdFilter.Location = new System.Drawing.Point(41, 178);
            this.lblIdFilter.Name = "lblIdFilter";
            this.lblIdFilter.Size = new System.Drawing.Size(72, 16);
            this.lblIdFilter.TabIndex = 15;
            this.lblIdFilter.Text = "Id Filter:";
            // 
            // cboIdFilter
            // 
            this.cboIdFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboIdFilter.Location = new System.Drawing.Point(129, 178);
            this.cboIdFilter.Name = "cboIdFilter";
            this.cboIdFilter.Size = new System.Drawing.Size(272, 23);
            this.cboIdFilter.Sorted = true;
            this.cboIdFilter.TabIndex = 14;
            // 
            // lblListFilter
            // 
            this.lblListFilter.Location = new System.Drawing.Point(41, 128);
            this.lblListFilter.Name = "lblListFilter";
            this.lblListFilter.Size = new System.Drawing.Size(72, 16);
            this.lblListFilter.TabIndex = 13;
            this.lblListFilter.Text = "List Filter:";
            // 
            // cboListFilter
            // 
            this.cboListFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboListFilter.Location = new System.Drawing.Point(129, 128);
            this.cboListFilter.Name = "cboListFilter";
            this.cboListFilter.Size = new System.Drawing.Size(272, 23);
            this.cboListFilter.Sorted = true;
            this.cboListFilter.TabIndex = 12;
            // 
            // lblDisplayField
            // 
            this.lblDisplayField.Location = new System.Drawing.Point(41, 76);
            this.lblDisplayField.Name = "lblDisplayField";
            this.lblDisplayField.Size = new System.Drawing.Size(72, 16);
            this.lblDisplayField.TabIndex = 11;
            this.lblDisplayField.Text = "Display Field:";
            // 
            // cboDisplayField
            // 
            this.cboDisplayField.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboDisplayField.Location = new System.Drawing.Point(129, 76);
            this.cboDisplayField.Name = "cboDisplayField";
            this.cboDisplayField.Size = new System.Drawing.Size(272, 23);
            this.cboDisplayField.Sorted = true;
            this.cboDisplayField.TabIndex = 10;
            this.cboDisplayField.SelectedIndexChanged += new System.EventHandler(this.CboDisplayField_SelectedIndexChanged);
            // 
            // lblName
            // 
            this.lblName.Location = new System.Drawing.Point(41, 25);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(72, 16);
            this.lblName.TabIndex = 9;
            this.lblName.Text = "Name:";
            // 
            // txtName
            // 
            this.txtName.Location = new System.Drawing.Point(129, 25);
            this.txtName.Name = "txtName";
            this.txtName.Size = new System.Drawing.Size(272, 23);
            this.txtName.TabIndex = 8;
            // 
            // Wizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(534, 400);
            this.ControlBox = false;
            this.Controls.Add(this.aerowizard1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Wizard";
            this.Text = "Create Menu Item Wizard";
            ((System.ComponentModel.ISupportInitialize)(this.aerowizard1)).EndInit();
            this.StartPage.ResumeLayout(false);
            this.StartPage.PerformLayout();
            this.DataStructureNamePage.ResumeLayout(false);
            this.DataStructureNamePage.PerformLayout();
            this.ScreenFormPage.ResumeLayout(false);
            this.ScreenFormPage.PerformLayout();
            this.LookupFormPage.ResumeLayout(false);
            this.LookupFormPage.PerformLayout();
            this.ResumeLayout(false);

        }
        #endregion
        private AeroWizard.WizardControl aerowizard1;
        private AeroWizard.WizardPage StartPage;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label lbTitle;
        private WizardPage DataStructureNamePage;
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
        internal System.Windows.Forms.ComboBox cboIdFilter;
        private System.Windows.Forms.Label lblListFilter;
        internal System.Windows.Forms.ComboBox cboListFilter;
        private System.Windows.Forms.Label lblDisplayField;
        internal System.Windows.Forms.ComboBox cboDisplayField;
        private System.Windows.Forms.Label lblName;
        internal System.Windows.Forms.TextBox txtName;
    }
}

