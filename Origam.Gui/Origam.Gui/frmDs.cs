using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace Origam.Gui
{
	/// <summary>
	/// Summary description for frmDs.
	/// </summary>
	public class frmDs : System.Windows.Forms.Form
	{
		private System.Windows.Forms.DataGrid grd;
		public Origam.Gui.dsOrigamGui ds;
		private System.Windows.Forms.DataGrid dataGrid1;
		private System.Windows.Forms.DataGrid dataGrid2;
		private Origam.Gui.WinForms.OrigamDataSource Karel;
		private System.Data.DataSet dataSet1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public frmDs()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.grd = new System.Windows.Forms.DataGrid();
			this.ds = new Origam.Gui.dsOrigamGui();
			this.dataGrid1 = new System.Windows.Forms.DataGrid();
			this.dataGrid2 = new System.Windows.Forms.DataGrid();
			this.Karel = new Origam.Gui.WinForms.OrigamDataSource();
			this.dataSet1 = new System.Data.DataSet();
			((System.ComponentModel.ISupportInitialize)(this.grd)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ds)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid2)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dataSet1)).BeginInit();
			this.SuspendLayout();
			// 
			// grd
			// 
			this.grd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grd.DataMember = "";
			this.grd.DataSource = this.ds.PropertyValue;
			this.grd.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grd.Location = new System.Drawing.Point(0, 0);
			this.grd.Name = "grd";
			this.grd.Size = new System.Drawing.Size(696, 144);
			this.grd.TabIndex = 0;
			// 
			// ds
			// 
			this.ds.DataSetName = "dsOrigamGui";
			this.ds.Locale = new System.Globalization.CultureInfo("en-US");
			// 
			// dataGrid1
			// 
			this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGrid1.DataMember = "";
			this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid1.Location = new System.Drawing.Point(0, 144);
			this.dataGrid1.Name = "dataGrid1";
			this.dataGrid1.Size = new System.Drawing.Size(696, 144);
			this.dataGrid1.TabIndex = 1;
			// 
			// dataGrid2
			// 
			this.dataGrid2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.dataGrid2.DataMember = "";
			this.dataGrid2.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGrid2.Location = new System.Drawing.Point(0, 288);
			this.dataGrid2.Name = "dataGrid2";
			this.dataGrid2.Size = new System.Drawing.Size(704, 144);
			this.dataGrid2.TabIndex = 2;
			// 
			// dataSet1
			// 
			this.dataSet1.DataSetName = "NewDataSet";
			this.dataSet1.Locale = new System.Globalization.CultureInfo("cs-CZ");
			// 
			// frmDs
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(696, 389);
			this.Controls.Add(this.dataGrid2);
			this.Controls.Add(this.dataGrid1);
			this.Controls.Add(this.grd);
			this.Name = "frmDs";
			this.Text = "frmDs";
			this.Load += new System.EventHandler(this.frmDs_Load);
			((System.ComponentModel.ISupportInitialize)(this.grd)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ds)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataGrid2)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dataSet1)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void frmDs_Load(object sender, System.EventArgs e)
		{
		
		}
	}
}
