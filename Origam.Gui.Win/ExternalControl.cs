#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for ExternalControl.
	/// </summary>
	public class ExternalControl : System.Windows.Forms.UserControl
	{
		//private AxShockwaveFlashObjects.AxShockwaveFlash flash;
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ExternalControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			// TODO: Add any initialization after the InitializeComponent call

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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ExternalControl));
//			this.flash = new AxShockwaveFlashObjects.AxShockwaveFlash();
//			((System.ComponentModel.ISupportInitialize)(this.flash)).BeginInit();
			this.SuspendLayout();
			// 
			// flash
			// 
//			this.flash.Dock = System.Windows.Forms.DockStyle.Fill;
//			this.flash.Enabled = true;
//			this.flash.Location = new System.Drawing.Point(0, 0);
//			this.flash.Name = "flash";
//			this.flash.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("flash.OcxState")));
//			this.flash.Size = new System.Drawing.Size(150, 150);
//			this.flash.TabIndex = 0;
			// 
			// ExternalControl
			// 
//			this.Controls.Add(this.flash);
			this.Name = "ExternalControl";
//			((System.ComponentModel.ISupportInitialize)(this.flash)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion
	}
}
