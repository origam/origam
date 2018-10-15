using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using Origam.Query;
using Origam.Query.Data;
using Origam.Schema;

namespace Origam.Workbench
{
	/// <summary>
	/// Summary description for OpenSchema.
	/// </summary>
	internal class OpenSchema : System.Windows.Forms.Form
	{
		private System.Windows.Forms.TreeView tvwSchemas;
		private System.Windows.Forms.Label lblSchemas;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Button btnOpen;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public OpenSchema()
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(OpenSchema));
			this.tvwSchemas = new System.Windows.Forms.TreeView();
			this.lblSchemas = new System.Windows.Forms.Label();
			this.btnCancel = new System.Windows.Forms.Button();
			this.btnOpen = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// tvwSchemas
			// 
			this.tvwSchemas.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.tvwSchemas.ImageIndex = -1;
			this.tvwSchemas.Location = new System.Drawing.Point(8, 24);
			this.tvwSchemas.Name = "tvwSchemas";
			this.tvwSchemas.SelectedImageIndex = -1;
			this.tvwSchemas.Size = new System.Drawing.Size(272, 232);
			this.tvwSchemas.TabIndex = 0;
			// 
			// lblSchemas
			// 
			this.lblSchemas.AutoSize = true;
			this.lblSchemas.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.lblSchemas.Location = new System.Drawing.Point(8, 8);
			this.lblSchemas.Name = "lblSchemas";
			this.lblSchemas.Size = new System.Drawing.Size(102, 16);
			this.lblSchemas.TabIndex = 1;
			this.lblSchemas.Text = "Available schemas:";
			// 
			// btnCancel
			// 
			this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnCancel.Location = new System.Drawing.Point(200, 264);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(80, 24);
			this.btnCancel.TabIndex = 2;
			this.btnCancel.Text = "Cancel";
			// 
			// btnOpen
			// 
			this.btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.btnOpen.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.btnOpen.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnOpen.Location = new System.Drawing.Point(112, 264);
			this.btnOpen.Name = "btnOpen";
			this.btnOpen.Size = new System.Drawing.Size(80, 24);
			this.btnOpen.TabIndex = 3;
			this.btnOpen.Text = "Open";
			this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
			// 
			// OpenSchema
			// 
			this.AcceptButton = this.btnOpen;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(288, 293);
			this.Controls.Add(this.btnOpen);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.lblSchemas);
			this.Controls.Add(this.tvwSchemas);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "OpenSchema";
			this.ShowInTaskbar = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
			this.Text = "Open Schema";
			this.ResumeLayout(false);

		}
		#endregion

		private void btnOpen_Click(object sender, System.EventArgs e)
		{
			this.Close();
		}

		dsExpressionDesigner mSchemaDataset = null;
		public dsExpressionDesigner SchemaDataset
		{
			get
			{
				return mSchemaDataset;
			}
			set
			{
				mSchemaDataset = value;
				LoadSchemas();
			}
		}

		private void LoadSchemas()
		{
			TreeNode nodSchema;
			TreeNode nodVersion;
			foreach(dsExpressionDesigner.SchemaRow schema in mSchemaDataset.Schema.Rows)
			{
				nodSchema = new TreeNode(schema.Name);
				tvwSchemas.Nodes.Add(nodSchema);
				
				DataRow[] colRows = mSchemaDataset.SchemaVersion.Select("SchemaGuid='" + schema.SchemaGuid.ToString() + "'");

				foreach(dsExpressionDesigner.SchemaVersionRow version in colRows)
				{
					nodVersion = new TreeNode(version.Name);
					
					GetExtensionNodes(version.SchemaVersionGuid, nodVersion, Guid.NewGuid(), true);

					nodSchema.Nodes.Add(nodVersion);
				}
			
				tvwSchemas.ExpandAll();
			}
		}

		private void GetExtensionNodes(Guid SchemaVersionGuid, TreeNode ParentNode, Guid ParentGuid, bool TopLevel)
		{
			TreeNode nodExtension;
			DataRow[] colRows;

			if(TopLevel)
				colRows = mSchemaDataset.SchemaExtension.Select("SchemaVersionGuid = '" + SchemaVersionGuid.ToString() + "' AND ParentSchemaExtensionGuid IS NULL");
			else
				colRows = mSchemaDataset.SchemaExtension.Select("SchemaVersionGuid = '" + SchemaVersionGuid.ToString() + "' AND ParentSchemaExtensionGuid = '" + ParentGuid.ToString() + "'");
			
			foreach(dsExpressionDesigner.SchemaExtensionRow extension in colRows)
			{
				nodExtension = new TreeNode(extension.Name);
				nodExtension.Tag = new SchemaExtensionOld(SchemaVersionGuid, extension.SchemaExtensionGuid, extension.Name);
					
				GetExtensionNodes(SchemaVersionGuid, nodExtension, extension.SchemaExtensionGuid, false);

				ParentNode.Nodes.Add(nodExtension);
			}
		}

		public SchemaExtensionOld SelectedExtension
		{
			get
			{
				return (SchemaExtensionOld)tvwSchemas.SelectedNode.Tag;
			}
		}

		public string SelectedSchemaPath
		{
			get
			{
				tvwSchemas.PathSeparator = ".";
				return tvwSchemas.SelectedNode.FullPath;
			}
		}
	}
}
