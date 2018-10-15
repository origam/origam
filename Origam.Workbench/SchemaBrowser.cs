using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI;

using Origam.Workbench.Services;

namespace Origam.Workbench
{
	/// <summary>
	/// Summary description for SchemaBrowser.
	/// </summary>
	public class SchemaBrowser : AbstractPadContent
	{
		internal Origam.Query.UI.Designer.ExpressionBrowser ebrSchemaBrowser;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private SchemaService _schemaService = Services.ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

		public SchemaBrowser()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			// Register for events when providers are added or removed
			_schemaService.ProviderAdded += new SchemaServiceEventHandler(_schemaService_ProviderAdded);
			_schemaService.ProviderRemoved += new SchemaServiceEventHandler(_schemaService_ProviderRemoved);
			
			// Provide us as a schema browser
			_schemaService.SchemaBrowser = this;
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
			this.ebrSchemaBrowser = new Origam.Query.UI.Designer.ExpressionBrowser();
			this.SuspendLayout();
			// 
			// ebrSchemaBrowser
			// 
			this.ebrSchemaBrowser.AllowEdit = true;
			this.ebrSchemaBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ebrSchemaBrowser.Location = new System.Drawing.Point(0, 0);
			this.ebrSchemaBrowser.Name = "ebrSchemaBrowser";
			this.ebrSchemaBrowser.ShowFilter = true;
			this.ebrSchemaBrowser.Size = new System.Drawing.Size(292, 271);
			this.ebrSchemaBrowser.TabIndex = 1;
			this.ebrSchemaBrowser.CheckSecurity = false;
			// 
			// SchemaBrowser
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 271);
			this.Controls.Add(this.ebrSchemaBrowser);
			this.DockableAreas = ((WeifenLuo.WinFormsUI.DockAreas)(((((WeifenLuo.WinFormsUI.DockAreas.Float | WeifenLuo.WinFormsUI.DockAreas.DockLeft) 
				| WeifenLuo.WinFormsUI.DockAreas.DockRight) 
				| WeifenLuo.WinFormsUI.DockAreas.DockTop) 
				| WeifenLuo.WinFormsUI.DockAreas.DockBottom)));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.HideOnClose = true;
			this.Name = "SchemaBrowser";
			this.ShowHint = WeifenLuo.WinFormsUI.DockState.DockLeft;
			this.Text = "Schema Browser";
			this.ResumeLayout(false);

		}
		#endregion

		private void _schemaService_ProviderAdded(object sender, SchemaServiceEventArgs e)
		{
			ebrSchemaBrowser.AddRootNode(e.Provider);
		}

		private void _schemaService_ProviderRemoved(object sender, SchemaServiceEventArgs e)
		{
			//TODO: Implement this ProviderRemoved event handler
		}
	}
}
