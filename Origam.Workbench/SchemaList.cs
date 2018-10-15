using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI;
using System.Data;
using Origam.Query;
using Origam.Query.Data;
using Origam.Schema;

namespace Origam.Workbench
{
	/// <summary>
	/// Summary description for SchemaList.
	/// </summary>
	internal class SchemaList : DockContent
	{
		private System.Windows.Forms.TreeView tvwSchemas;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public SchemaList()
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
			this.tvwSchemas = new System.Windows.Forms.TreeView();
			this.SuspendLayout();
			// 
			// tvwSchemas
			// 
			this.tvwSchemas.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvwSchemas.ImageIndex = -1;
			this.tvwSchemas.Location = new System.Drawing.Point(0, 0);
			this.tvwSchemas.Name = "tvwSchemas";
			this.tvwSchemas.SelectedImageIndex = -1;
			this.tvwSchemas.Size = new System.Drawing.Size(292, 271);
			this.tvwSchemas.TabIndex = 1;
			// 
			// SchemaList
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 271);
			this.Controls.Add(this.tvwSchemas);
			this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)(((((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.DockLeft) 
				| WeifenLuo.WinFormsUI.DockAreas.DockRight) 
				| WeifenLuo.WinFormsUI.DockAreas.DockTop) 
				| WeifenLuo.WinFormsUI.DockAreas.DockBottom)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.HideOnClose = true;
			this.Name = "SchemaList";
			this.ShowHint = WeifenLuo.WinFormsUI.DockState.DockLeft;
			this.Text = "SchemaList";
			this.ResumeLayout(false);

		}
		#endregion

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
				if (tvwSchemas.SelectedNode != null && tvwSchemas.SelectedNode.Tag != null)
                    return (SchemaExtensionOld)tvwSchemas.SelectedNode.Tag;
				else
					return null;
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
