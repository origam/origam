using Origam.Gui.UI;

namespace Origam.Workbench
{
    partial class SqlViewer
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SqlViewer));
            this.toolStrip1 = new Origam.Gui.UI.LabeledToolStrip();
            this.btnExecuteSql = new Origam.Gui.UI.BigToolStripButton();
            this.editor = new Origam.Windows.Editor.SqlEditor();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.btnExecuteSql});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.MinimumSize = new System.Drawing.Size(0, 95);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(431, 95);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "SQL Console";
            // 
            // btnExecuteSql
            // 
            this.btnExecuteSql.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.btnExecuteSql.Image = ((System.Drawing.Image)(resources.GetObject("btnExecuteSql.Image")));
            this.btnExecuteSql.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.btnExecuteSql.Margin = new System.Windows.Forms.Padding(5, 5, 5, 20);
            this.btnExecuteSql.Name = "btnExecuteSql";
            this.btnExecuteSql.Size = new System.Drawing.Size(72, 70);
            this.btnExecuteSql.Text = "E&xecute SQL\r\n ";
            this.btnExecuteSql.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.btnExecuteSql.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnExecuteSql.Click += new System.EventHandler(this.btnExecuteSql_Click);
            // 
            // editor
            // 
            this.editor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.editor.IsReadOnly = false;
            this.editor.Location = new System.Drawing.Point(0, 95);
            this.editor.Name = "editor";
            this.editor.Size = new System.Drawing.Size(431, 257);
            this.editor.TabIndex = 0;
            // 
            // SqlViewer
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(431, 352);
            this.Controls.Add(this.editor);
            this.Controls.Add(this.toolStrip1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SqlViewer";
            this.Text = "SQL Console";
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Origam.Windows.Editor.SqlEditor editor;
        private LabeledToolStrip toolStrip1;
        private BigToolStripButton btnExecuteSql;
    }
}