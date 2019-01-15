namespace OrigamArchitect
{
    partial class RuleWindow
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
            this.okNo = new System.Windows.Forms.Button();
            this.showOk = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // okNo
            // 
            this.okNo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okNo.Location = new System.Drawing.Point(144, 62);
            this.okNo.Name = "okNo";
            this.okNo.Size = new System.Drawing.Size(75, 23);
            this.okNo.TabIndex = 1;
            this.okNo.Text = "No";
            this.okNo.UseVisualStyleBackColor = true;
            this.okNo.Click += new System.EventHandler(this.No_Click);
            // 
            // showOk
            // 
            this.showOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.showOk.Location = new System.Drawing.Point(38, 62);
            this.showOk.Name = "showOk";
            this.showOk.Size = new System.Drawing.Size(75, 23);
            this.showOk.TabIndex = 2;
            this.showOk.Text = "Ok";
            this.showOk.UseVisualStyleBackColor = true;
            this.showOk.Click += new System.EventHandler(this.okButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(35, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "label1";
            // 
            // RuleWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(276, 97);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.showOk);
            this.Controls.Add(this.okNo);
            this.Name = "RuleWindow";
            this.Text = "RuleViolated";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button okNo;
        private System.Windows.Forms.Button showOk;
        private System.Windows.Forms.Label label1;
    }
}