#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.ComponentModel;
using System.Windows.Forms;
using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.Services;

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for ParameterEditor.
	/// </summary>
	public class ParameterEditor : AbstractViewContent
	{
		private System.Windows.Forms.PropertyGrid propertyGrid1;
		private System.Windows.Forms.Label lblHelp;
		private System.Windows.Forms.Label lblName;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ParameterEditor()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.lblHelp.BackColor = OrigamColorScheme.FormBackgroundColor;
			this.lblName.BackColor = OrigamColorScheme.FormBackgroundColor;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParameterEditor));
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.lblHelp = new System.Windows.Forms.Label();
            this.lblName = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.HelpVisible = false;
            this.propertyGrid1.LineColor = System.Drawing.SystemColors.ScrollBar;
            this.propertyGrid1.Location = new System.Drawing.Point(0, 102);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.PropertySort = System.Windows.Forms.PropertySort.NoSort;
            this.propertyGrid1.Size = new System.Drawing.Size(292, 164);
            this.propertyGrid1.TabIndex = 0;
            this.propertyGrid1.ToolbarVisible = false;
            this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
            // 
            // lblHelp
            // 
            this.lblHelp.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHelp.Location = new System.Drawing.Point(0, 28);
            this.lblHelp.Name = "lblHelp";
            this.lblHelp.Size = new System.Drawing.Size(292, 74);
            this.lblHelp.TabIndex = 1;
            this.lblHelp.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblName
            // 
            this.lblName.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblName.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.lblName.Location = new System.Drawing.Point(0, 0);
            this.lblName.Name = "lblName";
            this.lblName.Size = new System.Drawing.Size(292, 28);
            this.lblName.TabIndex = 2;
            this.lblName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ParameterEditor
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(292, 266);
            this.Controls.Add(this.propertyGrid1);
            this.Controls.Add(this.lblHelp);
            this.Controls.Add(this.lblName);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 7.8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ParameterEditor";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.ParameterEditor_Closing);
            this.ResumeLayout(false);

		}
		#endregion

		protected override void ViewSpecificLoad(object objectToLoad)
		{
			propertyGrid1.BrowsableAttributes =
				new AttributeCollection(new [] {new CategoryAttribute("Value")});

			DataConstant constant = objectToLoad as DataConstant;

			IParameterService svc = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
			constant.Value = svc.GetParameterValue(constant.Id);

			propertyGrid1.SelectedObject = constant;

			IDocumentationService doc = ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;

			lblHelp.Text = doc.GetDocumentation(constant.Id, DocumentationType.USER_LONG_HELP);
			lblName.Text = this.TitleName + " (" + constant.Name + ")";
		}

		public override void SaveObject()
		{
			try
			{
				DataConstant constant = propertyGrid1.SelectedObject as DataConstant;

				IParameterService svc = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;

				svc.SetCustomParameterValue(constant.Id, constant.Value, constant.GuidValue, constant.IntValue, constant.StringValue, constant.BooleanValue, constant.FloatValue, constant.CurrencyValue, constant.DateValue);
			}
			catch(Exception ex)
			{
				Origam.UI.AsMessageBox.ShowError(this, 
					ResourceUtils.GetString("ErrorParameterSaveFailed", Environment.NewLine + ex.Message), 
					ResourceUtils.GetString("ParameterTitle"), ex);
			}
		}

		private void propertyGrid1_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
		{
			this.IsDirty = true;
		}

		private void ParameterEditor_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(IsDirty)
			{
				DialogResult result = MessageBox.Show(
					ResourceUtils.GetString("DoYouWishSave", this.TitleName), 
					ResourceUtils.GetString("SaveTitle"), 
					MessageBoxButtons.YesNoCancel, 
					MessageBoxIcon.Question);
			
				switch(result)
				{
					case DialogResult.Yes:
						SaveObject();
						break;

					case DialogResult.No:
						break;
			
					case DialogResult.Cancel:
						e.Cancel = true;
						break;
				}
			}
		}
	}
}
