namespace Origam.Workbench
{
    partial class MoveToPackageForm
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
            this.packageComboBox = new System.Windows.Forms.ComboBox();
            this.packageLabel = new System.Windows.Forms.Label();
            this.groupLabel = new System.Windows.Forms.Label();
            this.groupComboBox = new System.Windows.Forms.ComboBox();
            this.okButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // packageComboBox
            // 
            this.packageComboBox.FormattingEnabled = true;
            this.packageComboBox.Location = new System.Drawing.Point(169, 37);
            this.packageComboBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.packageComboBox.Name = "packageComboBox";
            this.packageComboBox.Size = new System.Drawing.Size(371, 37);
            this.packageComboBox.TabIndex = 0;
            this.packageComboBox.SelectedIndexChanged += new System.EventHandler(this.packageComboBox_SelectedIndexChanged);
            // 
            // packageLabel
            // 
            this.packageLabel.AutoSize = true;
            this.packageLabel.Location = new System.Drawing.Point(49, 41);
            this.packageLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.packageLabel.Name = "packageLabel";
            this.packageLabel.Size = new System.Drawing.Size(107, 29);
            this.packageLabel.TabIndex = 1;
            this.packageLabel.Text = "Package";
            // 
            // groupLabel
            // 
            this.groupLabel.AutoSize = true;
            this.groupLabel.Location = new System.Drawing.Point(49, 106);
            this.groupLabel.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.groupLabel.Name = "groupLabel";
            this.groupLabel.Size = new System.Drawing.Size(80, 29);
            this.groupLabel.TabIndex = 3;
            this.groupLabel.Text = "Group";
            // 
            // groupComboBox
            // 
            this.groupComboBox.FormattingEnabled = true;
            this.groupComboBox.Location = new System.Drawing.Point(169, 101);
            this.groupComboBox.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.groupComboBox.Name = "groupComboBox";
            this.groupComboBox.Size = new System.Drawing.Size(371, 37);
            this.groupComboBox.TabIndex = 2;
            // 
            // okButton
            // 
            this.okButton.Location = new System.Drawing.Point(221, 175);
            this.okButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(148, 58);
            this.okButton.TabIndex = 4;
            this.okButton.Text = "Move";
            this.okButton.UseVisualStyleBackColor = true;
            this.okButton.Click += new System.EventHandler(this.okButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(392, 175);
            this.CancelButton.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(148, 58);
            this.CancelButton.TabIndex = 5;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // MoveToPackageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(14F, 29F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(612, 262);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.groupLabel);
            this.Controls.Add(this.groupComboBox);
            this.Controls.Add(this.packageLabel);
            this.Controls.Add(this.packageComboBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(5, 4, 5, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "MoveToPackageForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Move To Package";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox packageComboBox;
        private System.Windows.Forms.Label packageLabel;
        private System.Windows.Forms.Label groupLabel;
        private System.Windows.Forms.ComboBox groupComboBox;
        private System.Windows.Forms.Button okButton;
        private new System.Windows.Forms.Button CancelButton;
    }
}