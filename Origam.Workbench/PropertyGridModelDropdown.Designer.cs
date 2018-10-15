namespace Origam.Workbench
{
    partial class PropertyGridModelDropdown
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.listBox1 = new System.Windows.Forms.ListView();
            this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Dock = System.Windows.Forms.DockStyle.Top;
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(268, 20);
            this.textBox1.TabIndex = 1;
            this.textBox1.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
            this.textBox1.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.textBox1_KeyDown);
            // 
            // listBox1
            // 
            this.listBox1.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.listBox1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colName});
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.FullRowSelect = true;
            this.listBox1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.listBox1.HideSelection = false;
            this.listBox1.Location = new System.Drawing.Point(0, 20);
            this.listBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listBox1.MultiSelect = false;
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(268, 251);
            this.listBox1.TabIndex = 0;
            this.listBox1.UseCompatibleStateImageBehavior = false;
            this.listBox1.View = System.Windows.Forms.View.Details;
            this.listBox1.ItemActivate += new System.EventHandler(this.listBox1_ItemActivate);
            this.listBox1.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.listBox1_KeyPress);
            this.listBox1.PreviewKeyDown += new System.Windows.Forms.PreviewKeyDownEventHandler(this.listBox1_PreviewKeyDown);
            // 
            // colName
            // 
            this.colName.Text = "Name";
            // 
            // PropertyGridModelDropdown
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.textBox1);
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "PropertyGridModelDropdown";
            this.Size = new System.Drawing.Size(268, 271);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.ListView listBox1;
        private System.Windows.Forms.ColumnHeader colName;
    }
}
