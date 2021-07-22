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
using Origam.Schema;

namespace Origam.Gui.Win
{
    /// <summary>
	/// Summary description for SplitPanel.
	/// </summary>
	public class SplitPanel : System.Windows.Forms.Panel, IOrigamMetadataConsumer, ISupportInitialize
	{
		#region Private variables
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private bool _addingSplitter = false;
		private CollapsibleSplitter _splitter = new CollapsibleSplitter();
		private bool _splitterSizeChangedByUser = false;
		private bool _saveSplitterPosition = true;

		#endregion

		#region Constructor
		public SplitPanel()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			//InitSplitter();

			//this.Dock = DockStyle.Fill;
			this.BackColor = OrigamColorScheme.FormBackgroundColor;

			_splitter.SplitterMoved += new SplitterEventHandler(_splitter_SplitterMoved);
		}
		#endregion

		#region Overriden Control methods
        /// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(_splitter != null)
				{
					_splitter.Dispose();
					_splitter = null;
				}

				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		protected override void OnControlAdded(ControlEventArgs e)
		{
			if(_addingSplitter) return;

//			if(this.Controls.Count > 2)
//			{
//				this.Controls.Remove(e.Control);
//			}
//			else
//			{
				base.OnControlAdded (e);
//			}

			e.Control.TabIndexChanged +=new EventHandler(Control_TabIndexChanged);
			RefreshDocking();
		}

		protected override void OnControlRemoved(ControlEventArgs e)
		{
			if(this.Controls.Contains(_splitter))
			{
				this.Controls.Remove(_splitter);
			}

			e.Control.TabIndexChanged -= new EventHandler(Control_TabIndexChanged);

			RefreshDocking();

			base.OnControlRemoved (e);
		}
		#endregion

		#region Event Handlers
		private void Control_TabIndexChanged(object sender, EventArgs e)
		{
			RefreshDocking();
		}

		private void _splitter_SplitterMoved(object sender, SplitterEventArgs e)
		{
			if(this.DesignMode | _settingSplitter | _saveSplitterPosition == false) return;

			try
			{
                UserProfile profile = SecurityManager.CurrentUserProfile();
                OrigamPanelColumnConfig config = OrigamPanelColumnConfigDA.LoadUserConfig(this.OrigamMetadata.Id, profile.Id);

				OrigamPanelColumnConfig.PanelColumnConfigRow configRow;
				if(config.PanelColumnConfig.Rows.Count == 0)
				{
					configRow = config.PanelColumnConfig.NewPanelColumnConfigRow();
					configRow.Id = Guid.NewGuid();
					configRow.ColumnName = "splitPanel";
					configRow.PanelId = _origamMetadata.Id;
					configRow.ProfileId = profile.Id;
					configRow.RecordCreated = DateTime.Now;
					configRow.RecordCreatedBy = profile.Id;
					configRow.ColumnWidth = 0;
					config.PanelColumnConfig.AddPanelColumnConfigRow(configRow);
				}
				else
				{
					configRow = config.PanelColumnConfig.Rows[0] as OrigamPanelColumnConfig.PanelColumnConfigRow;
					configRow.RecordUpdated = DateTime.Now;
					configRow.RecordUpdatedBy = profile.Id;
				}
				
				configRow.ColumnWidth = _splitter.SplitPosition;

				// store the new width
				OrigamPanelColumnConfigDA.PersistColumnConfig(config);

				_splitterSizeChangedByUser = true;
			}
			catch 
			{
			}
		}
		#endregion
		
		#region Properties
		private SplitPanelOrientation _orientation = SplitPanelOrientation.Horizontal;
		public SplitPanelOrientation Orientation
		{
			get
			{
				return _orientation;
			}
			set
			{
				_orientation = value;

				InitSplitter();
				RefreshDocking();
			}
		}

		private bool _fixedSize = false;
		public bool FixedSize
		{
			get
			{
				return _fixedSize;
			}
			set
			{
				_fixedSize = value;
				InitSplitter();
			}
		}
		#endregion

		#region Private Methods
		private void InitSplitter()
		{
			switch(this.Orientation)
			{
				case SplitPanelOrientation.Horizontal:
					_splitter.Dock = DockStyle.Top;
					break;
				case SplitPanelOrientation.Vertical:
					_splitter.Dock = DockStyle.Left;
					break;
			}

			if(FixedSize)
			{
				_splitter.BackColor = this.BackColor;
				_splitter.Enabled = false;
			}
			else
			{
				_splitter.BackColor = OrigamColorScheme.SplitterBackColor;
				_splitter.Enabled = true;
			}

			_splitter.VisualStyle = VisualStyles.XP;
			
			//_splitter.UseAnimations = true;
			//_splitter.Height = 5;
		}

		private void RefreshDocking()
		{
			if(DesignMode) this.DockPadding.All = 10;

			Control control1 = null;
			Control control2 = null;

			foreach(Control control in this.Controls)
			{
				if(!control.Equals(_splitter))
				{
					control.Dock = DockStyle.None;

					switch(control.TabIndex)
					{
						case 0:
							control1 = control;
							break;
						case 1:
							control2 = control;
							break;
					}
				}
			}

			
			if(control2 != null & control1 != null)
			{
				if(! this.Controls.Contains(_splitter))
				{
					_addingSplitter = true;
					this.Controls.Add(_splitter);
					_addingSplitter = false;
				}

				switch(this.Orientation)
				{
					case SplitPanelOrientation.Horizontal:
						control1.Dock = DockStyle.Top;
						control2.Dock = DockStyle.Fill;

						control1.BringToFront();
						_splitter.BringToFront();
						control2.BringToFront();
						break;
					case SplitPanelOrientation.Vertical:
						control1.Dock = DockStyle.Left;
						control2.Dock = DockStyle.Fill;
						
						control1.BringToFront();
						_splitter.BringToFront();
						control2.BringToFront();
						break;
				}

				_splitter.ControlToHide = control1;
			}
		}

		private void LoadUserConfig()
		{
			if(DesignMode | _splitterSizeChangedByUser | _saveSplitterPosition == false) return;

			_settingSplitter = true;

			try
			{
                UserProfile profile = SecurityManager.CurrentUserProfile();
                OrigamPanelColumnConfig userConfig = OrigamPanelColumnConfigDA.LoadUserConfig(this.OrigamMetadata.Id, profile.Id);

				if(userConfig.PanelColumnConfig.Rows.Count > 0)
				{
					int storedSplitterPosition = (userConfig.PanelColumnConfig.Rows[0] as OrigamPanelColumnConfig.PanelColumnConfigRow).ColumnWidth;
					int? position = StoredPositionToPixels(storedSplitterPosition);
					if (position != null)
					{
						_splitter.SplitPosition = storedSplitterPosition;
					}
				}
			}
			catch {}

			_settingSplitter = false;
		}

		private int? StoredPositionToPixels(int storedValue)
		{
			bool isScreenSizePositionRatio = storedValue > 1000; // this will work up to 8k 
			if(isScreenSizePositionRatio)
			{
				var height = storedValue * Screen.FromControl(this).Bounds.Height / 1000_000;
				return Convert.ToInt32(height);
			}
			else
			{
				return null;
			}
		}

		#endregion

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.SuspendLayout();
			// 
			// SplitPanel
			// 
			this.Name = "SplitPanel";
			this.ResumeLayout(false);

		}
		#endregion

		#region IOrigamMetadataConsumer Members

		private AbstractSchemaItem _origamMetadata;
		public AbstractSchemaItem OrigamMetadata
		{
			get
			{
				return _origamMetadata;
			}
			set
			{
				_origamMetadata = value;
			}
		}

		#endregion

		#region ISupportInitialize Members

		public void BeginInit()
		{
		}

		bool _settingSplitter = false;
		public void EndInit()
		{
			LoadUserConfig();
			this.SizeChanged += new EventHandler(SplitPanel_SizeChanged);
		}

		#endregion

		private void SplitPanel_SizeChanged(object sender, EventArgs e)
		{
			LoadUserConfig();
		}
	}
}
