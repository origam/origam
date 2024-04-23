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
using System.Drawing;
using System.Windows.Forms;

using Origam.UI;
using Origam.Gui.UI;


namespace Origam.Gui.Win;

/// <summary>
/// Summary description for BaseDropDownControl.
/// </summary>
public class BaseDropDownControl : BaseCaptionControl, IAsGridEditor
{
	internal class NoKeyUpTextBox : EnhancedTextBox
	{
		private const int WM_KEYUP = 0x101;
		private const int WM_KEYDOWN = 0x100;
		private const int KEY_CURSOR_UP = 38;
		private const int KEY_CURSOR_DOWN = 40;

		public NoKeyUpTextBox()
		{
            }

		protected override bool ProcessKeyMessage(ref Message m)
		{
				if(! _noKeyUp) return base.ProcessKeyMessage (ref m);

				// ignore cursor keys and tab key
				if(m.Msg == WM_KEYDOWN)
				{
					if(
						(m.WParam.ToInt32() == 37 & this.DataType != typeof(DateTime))
						| (m.WParam.ToInt32() == 39 & this.DataType != typeof(DateTime))
						| (m.WParam.ToInt32() == KEY_CURSOR_DOWN & this.IgnoreCursorDown)
						| (m.WParam.ToInt32() == KEY_CURSOR_UP & this.IgnoreCursorDown)
						)
					{
						return false;
					}

					if(this.DataType == typeof(DateTime))
					{
						return this.ProcessKeyEventArgs(ref m);
					}
				}
			
				if(m.Msg == WM_KEYUP)
				{
					if(
						m.WParam.ToInt32() == 9
						| m.WParam.ToInt32() == KEY_CURSOR_UP
						| m.WParam.ToInt32() == KEY_CURSOR_DOWN
						)
					{
						if(m.WParam.ToInt32() == KEY_CURSOR_DOWN) this.OnCursorDownPressed(EventArgs.Empty);
						if(m.WParam.ToInt32() == KEY_CURSOR_UP) this.OnCursorUpPressed(EventArgs.Empty);

						return true;
					}
					else if(
						(m.WParam.ToInt32() == 37 & this.DataType != typeof(DateTime))
						| (m.WParam.ToInt32() == 39 & this.DataType != typeof(DateTime))
						| (m.WParam.ToInt32() == KEY_CURSOR_DOWN & this.IgnoreCursorDown)
						)
					{
						return false;
					}

					if(this.DataType == typeof(DateTime))
					{
						return this.ProcessKeyEventArgs(ref m);
					}
				}

				return base.ProcessKeyMessage (ref m);
			}

		protected override void OnMouseWheel(MouseEventArgs e)
		{
				BaseCaptionControl bcc = this.Parent as BaseCaptionControl;

				if(bcc != null)
				{
					bcc.OnControlMouseWheel(e);
				}
				else
				{
					base.OnMouseWheel(e);
				}
			}

		private bool _noKeyUp = false;
		public bool NoKeyUp
		{
			get
			{
					return _noKeyUp;
				}
			set
			{
					_noKeyUp = value;
				}
		}

		private bool _ignoreCursorDown = false;
		public bool IgnoreCursorDown
		{
			get
			{
					return _ignoreCursorDown;
				}
			set
			{
					_ignoreCursorDown = value;
				}
		}

		protected override void OnValidating(CancelEventArgs e)
		{
                //base.OnValidating(e);
            }

		public event System.EventHandler CursorDownPressed;
		protected virtual void OnCursorDownPressed(EventArgs e)
		{
				if (this.CursorDownPressed != null)
				{
					this.CursorDownPressed(this, e);
				}
			}

		public event System.EventHandler CursorUpPressed;
		protected virtual void OnCursorUpPressed(EventArgs e)
		{
				if (this.CursorUpPressed != null)
				{
					this.CursorUpPressed(this, e);
				}
			}
	}


	private BaseDropDownControl.NoKeyUpTextBox txtEdit;
	private System.Windows.Forms.Button btnDropDown;
	private System.Windows.Forms.Button btnOpenList;
	private System.Windows.Forms.ImageList imageList1;
	private System.ComponentModel.IContainer components;

	private static ImageListStreamer ImgListStreamer = null;
	private PopupWindowHelper _popupHelper = null;
	private bool _dropDownCanceledByButton = false;
	public event EventHandler EditorClick = delegate {};
	public event EventHandler EditorDoubleClick = delegate {};

	public BaseDropDownControl()
	{
			InitializeComponent();

			txtEdit.DoubleClick += (sender, args) =>
				EditorDoubleClick?.Invoke(null, EventArgs.Empty);
			txtEdit.MouseDown += (sender, args) => 
				EditorClick?.Invoke(null, EventArgs.Empty);
			
			this.btnDropDown.BackColor = OrigamColorScheme.ButtonBackColor;
			this.btnOpenList.BackColor = OrigamColorScheme.ButtonBackColor;

			if(ImgListStreamer == null)
			{
				System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(BaseDropDownControl));
				ImgListStreamer = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
			}

			this.imageList1.ImageStream = ImgListStreamer;

			_popupHelper = new PopupWindowHelper();
			_popupHelper.PopupCancel += new PopupCancelEventHandler(popupHelper_PopupCancel);
		}

	private void InitializeComponent()
	{
			this.components = new System.ComponentModel.Container();
			this.txtEdit = new Origam.Gui.Win.BaseDropDownControl.NoKeyUpTextBox();
			this.btnDropDown = new System.Windows.Forms.Button();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.btnOpenList = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 		// txtEdit
			// 		this.txtEdit.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtEdit.IgnoreCursorDown = false;
			this.txtEdit.Location = new System.Drawing.Point(0, 0);
			this.txtEdit.Name = "txtEdit";
			this.txtEdit.NoKeyUp = false;
			this.txtEdit.Size = new System.Drawing.Size(144, 19);
			this.txtEdit.TabIndex = 0;
			this.txtEdit.Tag = null;
			this.txtEdit.GotFocus += new System.EventHandler(this.txtEdit_GotFocus);
			// 		// btnDropDown
			// 		this.btnDropDown.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(214)), ((System.Byte)(203)), ((System.Byte)(111)));
			this.btnDropDown.Dock = System.Windows.Forms.DockStyle.Right;
			this.btnDropDown.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnDropDown.ForeColor = System.Drawing.Color.White;
			this.btnDropDown.ImageIndex = 0;
			this.btnDropDown.ImageList = this.imageList1;
			this.btnDropDown.Location = new System.Drawing.Point(160, 0);
			this.btnDropDown.Name = "btnDropDown";
			this.btnDropDown.Size = new System.Drawing.Size(16, 20);
			this.btnDropDown.TabIndex = 4;
			this.btnDropDown.TabStop = false;
			this.btnDropDown.Click += new System.EventHandler(this.btnDropDown_Click);
			// 		// imageList1
			// 		this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
			// 		// btnOpenList
			// 		this.btnOpenList.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(214)), ((System.Byte)(203)), ((System.Byte)(111)));
			this.btnOpenList.Dock = System.Windows.Forms.DockStyle.Right;
			this.btnOpenList.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.btnOpenList.ForeColor = System.Drawing.Color.White;
			this.btnOpenList.ImageIndex = 1;
			this.btnOpenList.ImageList = this.imageList1;
			this.btnOpenList.Location = new System.Drawing.Point(144, 0);
			this.btnOpenList.Name = "btnOpenList";
			this.btnOpenList.Size = new System.Drawing.Size(16, 20);
			this.btnOpenList.TabIndex = 2;
			this.btnOpenList.TabStop = false;
			// 		// BaseDropDownControl
			// 		this.Controls.Add(this.txtEdit);
			this.Controls.Add(this.btnOpenList);
			this.Controls.Add(this.btnDropDown);
			this.Name = "BaseDropDownControl";
			this.Size = new System.Drawing.Size(176, 20);
			this.VisibleChanged += new System.EventHandler(this.BaseDropDownControl_VisibleChanged);
			this.SizeChanged += new System.EventHandler(this.BaseDropDownControl_SizeChanged);
			this.ResumeLayout(false);

		}

	protected override void Dispose(bool disposing)
	{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}

				if(_popupHelper != null)
				{
					_popupHelper.PopupCancel -= new PopupCancelEventHandler(popupHelper_PopupCancel);
					_popupHelper = null;
				}

				this.VisibleChanged -= new EventHandler(BaseDropDownControl_VisibleChanged);
			}
			
			base.Dispose (disposing);
		}

	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
			if(keyData == Keys.Escape & _droppedDown)
			{
				_popup.Canceled = true;
				_popupHelper.ClosePopup();
				return true;
			}

			if((keyData & Keys.KeyCode) == Keys.Tab)
			{
				_popupHelper.ClosePopup();
			}

			return base.ProcessCmdKey (ref msg, keyData);
		}

	internal BaseDropDownControl.NoKeyUpTextBox EditControl
	{
		get
		{
				return this.txtEdit;
			}
	}

	private bool _noKeyUp = false;
	public bool NoKeyUp
	{
		get
		{
				return _noKeyUp;
			}
		set
		{
				_noKeyUp = value;
				this.txtEdit.NoKeyUp = value;
			}
	}

	private bool _setFocusToDropDown = false;
	public bool SetFocusToDropDown
	{
		get
		{
				return _setFocusToDropDown;
			}
		set
		{
				_setFocusToDropDown = value;
			}
	}

	public PopupWindowHelper PopupHelper
	{
		get
		{
				return _popupHelper;
			}
	}

	private bool _readOnly = false;
	public bool ReadOnly
	{
		get
		{
				return _readOnly;
			}
		set
		{
				_readOnly = value;

				if(_readOnly)
				{
					this.EditControl.ReadOnly = true;
					this.btnDropDown.Visible = false;
					//this.LookupShowEditButton = false;
				}
				else
				{
					this.EditControl.ReadOnly = false;
					this.btnDropDown.Visible = true;
				}
			}
	}

	private bool _showCalendarIcon = false;
	public bool ShowCalendarIcon
	{
		get
		{
				return _showCalendarIcon;
			}
		set
		{
				_showCalendarIcon = value;
				if(value)
				{
					this.btnDropDown.ImageIndex = 2;
				}
				else
				{
					this.btnDropDown.ImageIndex = 0;
				}
			}
	}

	public virtual IDropDownPart CreatePopup ()
	{
			throw new NotImplementedException();
		}

	public virtual object SelectedValue
	{
		get
		{
				throw new NotImplementedException();
			}
		set
		{
				throw new NotImplementedException();
			}
	}

	private IDropDownPart _popup;
	[Browsable(false)]
	public IDropDownPart Popup
	{
		get
		{
				return _popup;
			}
		set
		{
				_popup = value;
			}
	}

	public Button OpenListButton
	{
		get
		{
				return btnOpenList;
			}
	}

	private string _displayText;
	[Browsable(false)]
	public string DisplayText
	{
		get
		{
				return _displayText;
			}
		set
		{
				_displayText = value;
				txtEdit.Value = value;
			}
	}

	public ScreenLocation ScreenLocation
	{
		get
		{
			    Point point = this.PointToScreen(new Point(txtEdit.Left, btnDropDown.Bottom));
			    return new ScreenLocation(point.X, point.Y);
			}
	}

	private bool _droppedDown = false;
	public bool DroppedDown
	{
		get
		{
				return _droppedDown;
			}
		set
		{
				_droppedDown = value;
			}
	}

	public void DropDown()
	{
			try
			{
				if(this.ReadOnly) return;

				if(_popupHelper.Handle.ToInt32() == 0)
				{
					Form form = this.FindForm();

					if(form.Parent is WeifenLuo.WinFormsUI.Docking.DockPane) form = form.Parent.FindForm();

					_popupHelper.AssignHandle(form.Handle);
				}

				if(_droppedDown) return;

				txtEdit.Focus();

				_popup = this.CreatePopup();

				Rectangle screen = Screen.FromControl(this).WorkingArea;

				Point location = new System.Drawing.Point (this.ScreenLocation.X, this.ScreenLocation.Y);
				int screenTotalWidth = screen.X + screen.Width;
				int screenTotalHeight = screen.Y + screen.Height;

				if(location.X + _popup.Width > screenTotalWidth)
				{
					location.X -= location.X + _popup.Width - screenTotalWidth;
				}

				if(location.Y + _popup.Height > screenTotalHeight)
				{
					location.Y -= (txtEdit.Height + _popup.Height);
				}

				_popup.DropDownControl = this;

				_popupHelper.ShowPopup(this.FindForm(), _popup as Form, location);
			
				_popup.SelectedValue = this.SelectedValue;
			
			
				_droppedDown = true;
				txtEdit.IgnoreCursorDown = true;

				if(SetFocusToDropDown)
				{
					_popup.Focus();
				}
				else
				{
					txtEdit.Focus();
				}
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this.FindForm(), ex.Message, ResourceUtils.GetString("ErrorOpenList", this.Caption), ex);
			}
		}

	private void BaseDropDownControl_SizeChanged(object sender, System.EventArgs e)
	{
			this.Height = txtEdit.Height;
		}

	private void btnDropDown_Click(object sender, System.EventArgs e)
	{
			try
			{
				this.txtEdit.Focus();

				if(_dropDownCanceledByButton)
				{
					_dropDownCanceledByButton = false;
					return;
				}

				DropDown();
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this.FindForm(), ex.Message, ResourceUtils.GetString("ErrorOpenList", this.Caption), ex);
			}
		}

	private void btnDropDown_Enter(object sender, System.EventArgs e)
	{
			// never set focus on the dropdown button, when focused, focus the edit box
			this.txtEdit.Focus();
		}

	private void popupHelper_PopupCancel(object sender, PopupCancelEventArgs e)
	{
			txtEdit.Value = this.DisplayText;

			if(_droppedDown)
			{
				if(txtEdit.Focused & txtEdit.Visible
					& e.CursorLocation.X >= txtEdit.PointToScreen(new Point(0, 0)).X
					& e.CursorLocation.X <= txtEdit.PointToScreen(new Point(txtEdit.Width, txtEdit.Height)).X
					& e.CursorLocation.Y >= txtEdit.PointToScreen(new Point(0, 0)).Y
					& e.CursorLocation.Y <= txtEdit.PointToScreen(new Point(txtEdit.Width, txtEdit.Height)).Y
					)
				{
					//System.Diagnostics.Debug.WriteLine("Popup cancelled");
					e.Cancel = true;
					//		_droppedDown = false;
				}
				else
				{
					_droppedDown = false;
					txtEdit.IgnoreCursorDown = false;
					txtEdit.SelectAll();
				}

				if(e.CursorLocation.X >= btnDropDown.PointToScreen(new Point(0, 0)).X
					& e.CursorLocation.X <= btnDropDown.PointToScreen(new Point(btnDropDown.Width, btnDropDown.Height)).X
					& e.CursorLocation.Y >= btnDropDown.PointToScreen(new Point(0, 0)).Y
					& e.CursorLocation.Y <= btnDropDown.PointToScreen(new Point(btnDropDown.Width, btnDropDown.Height)).Y
					)
				{
					_dropDownCanceledByButton = true;
				}
			}

			//System.Diagnostics.Debug.WriteLine("Popup cancel Event");
		}
	
	private void BaseDropDownControl_VisibleChanged(object sender, EventArgs e)
	{
			_popupHelper.ClosePopup();
		}
	
	private void txtEdit_GotFocus(object sender, EventArgs e)
	{
			txtEdit.SelectAll();
		}

	#region Events
	public event System.EventHandler readOnlyChanged;
	protected virtual void OnReadOnlyChanged(EventArgs e)
	{
			if (this.readOnlyChanged != null)
			{
				this.readOnlyChanged(this, e);
			}
		}
	#endregion
}