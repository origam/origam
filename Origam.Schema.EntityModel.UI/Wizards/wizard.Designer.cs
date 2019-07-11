using System;
using AeroWizard;

namespace Origam.Schema.EntityModel.Wizards
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
            this.pageStart = new AeroWizard.WizardPage();
            this.lbTitle = new System.Windows.Forms.Label();
            this.DataStructureNamePage = new AeroWizard.WizardPage();
            this.lbName = new System.Windows.Forms.Label();
            this.tbDataStructureName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.aerowizard1)).BeginInit();
            this.pageStart.SuspendLayout();
            this.DataStructureNamePage.SuspendLayout();
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
            this.aerowizard1.Pages.Add(this.pageStart);
            this.aerowizard1.Pages.Add(this.DataStructureNamePage);
            this.aerowizard1.Size = new System.Drawing.Size(531, 328);
            this.aerowizard1.TabIndex = 0;
            // 
            // pageStart
            // 
            this.pageStart.Controls.Add(this.lbTitle);
            this.pageStart.Controls.Add(this.listView1);
            this.pageStart.Name = "pageStart";
            this.pageStart.Size = new System.Drawing.Size(484, 174);
            this.pageStart.TabIndex = 0;
            this.pageStart.Text = "Page Title";
            this.pageStart.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.PageStart_Commit);
            this.pageStart.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.PageStart_Initialize);
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
            this.DataStructureNamePage.Size = new System.Drawing.Size(484, 174);
            this.DataStructureNamePage.TabIndex = 2;
            this.DataStructureNamePage.Text = "Please Write Name of Datastructure";
            this.DataStructureNamePage.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.DataStructureNamePage_Commit);
            this.DataStructureNamePage.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.DataStructureNamePage_Initialize);
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
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(131, 75);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(228, 34);
            this.label1.TabIndex = 2;
            this.label1.Text = "Name of DataStructure already exists. Please Fill different Name.";
            // 
            // Wizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(531, 328);
            this.ControlBox = false;
            this.Controls.Add(this.aerowizard1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "Wizard";
            this.Text = "Create Menu Item Wizard";
            ((System.ComponentModel.ISupportInitialize)(this.aerowizard1)).EndInit();
            this.pageStart.ResumeLayout(false);
            this.pageStart.PerformLayout();
            this.DataStructureNamePage.ResumeLayout(false);
            this.DataStructureNamePage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private AeroWizard.WizardControl aerowizard1;
        private AeroWizard.WizardPage pageStart;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.Label lbTitle;
        private WizardPage DataStructureNamePage;
        private System.Windows.Forms.Label lbName;
        private System.Windows.Forms.TextBox tbDataStructureName;
        private System.Windows.Forms.Label label1;
    }
}

