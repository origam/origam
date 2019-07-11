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
            this.wizardPage1 = new AeroWizard.WizardPage();
            this.lbTitle = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.aerowizard1)).BeginInit();
            this.pageStart.SuspendLayout();
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
            this.aerowizard1.Pages.Add(this.wizardPage1);
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
            this.pageStart.Commit += new System.EventHandler<AeroWizard.WizardPageConfirmEventArgs>(this.pageStart_Commit);
            this.pageStart.Initialize += new System.EventHandler<AeroWizard.WizardPageInitEventArgs>(this.pageStart_Initialize);

            // 
            // wizardPage1
            // 
            this.wizardPage1.Name = "wizardPage1";
            this.wizardPage1.Size = new System.Drawing.Size(484, 174);
            this.wizardPage1.TabIndex = 1;
            this.wizardPage1.Text = "Page Title";
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
            this.ResumeLayout(false);

        }

        private void pageStart_Initialize(object sender, WizardPageInitEventArgs e)
        {
            GetNextPage(PagesList.startPage, pageStart);
        }
        private WizardPage getWizardPage(PagesList nextPage)
        {
            switch (nextPage)
            {
                
            }
            return null;
        }

        private void GetNextPage(PagesList actualPage, WizardPage pageStart)
        {
            bool findPage = false;
            foreach (PagesList pglist in pages)
            {
                if(findPage)
                {
                    pageStart.NextPage = getWizardPage(pglist);
                    return;
                }
                if(pglist == actualPage)
                {
                    findPage = true;
                }
            }
            pageStart.IsFinishPage = true;
        }

        private void pageStart_Commit(object sender, WizardPageConfirmEventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        #endregion
        private AeroWizard.WizardControl aerowizard1;
        private AeroWizard.WizardPage pageStart;
        private System.Windows.Forms.ListView listView1;
        private AeroWizard.WizardPage wizardPage1;
        private System.Windows.Forms.Label lbTitle;
    }
}

