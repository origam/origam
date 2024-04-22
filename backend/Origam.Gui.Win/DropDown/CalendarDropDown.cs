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
using System.Windows.Forms;

using Origam.UI;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for CalendarDropDown.
/// </summary>
public class CalendarDropDown : System.Windows.Forms.Form, IDropDownPart
{
	private Origam.Gui.Win.AsMonthCalendar monthCalendar1;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public CalendarDropDown()
	{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			ResizeMe();

			this.BackColor = OrigamColorScheme.DateTimePickerBorderColor;
			monthCalendar1.BackColor = OrigamColorScheme.DateTimePickerBackColor;
			monthCalendar1.TitleBackColor = OrigamColorScheme.DateTimePickerTitleBackColor;
			monthCalendar1.TitleForeColor = OrigamColorScheme.DateTimePickerTitleForeColor;
			monthCalendar1.ForeColor = OrigamColorScheme.DateTimePickerForeColor;
			monthCalendar1.TrailingForeColor = OrigamColorScheme.DateTimePickerTrailingForeColor;
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
			this.monthCalendar1 = new Origam.Gui.Win.AsMonthCalendar();
			this.SuspendLayout();
			// 		// monthCalendar1
			// 		this.monthCalendar1.BackColor = System.Drawing.SystemColors.Window;
			this.monthCalendar1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.monthCalendar1.Location = new System.Drawing.Point(1, 1);
			this.monthCalendar1.MaxSelectionCount = 1;
			this.monthCalendar1.Name = "monthCalendar1";
			this.monthCalendar1.ShowWeekNumbers = true;
			this.monthCalendar1.TabIndex = 0;
			this.monthCalendar1.SizeChanged += new System.EventHandler(this.monthCalendar1_SizeChanged);
			this.monthCalendar1.DateSelected += new System.Windows.Forms.DateRangeEventHandler(this.monthCalendar1_DateSelected);
			// 		// CalendarDropDown
			// 		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.Black;
			this.ClientSize = new System.Drawing.Size(166, 159);
			this.Controls.Add(this.monthCalendar1);
			this.DockPadding.All = 1;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.KeyPreview = true;
			this.Name = "CalendarDropDown";
			this.ShowInTaskbar = false;
			this.Text = "CalendarDropDown";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CalendarDropDown_KeyDown);
			this.ResumeLayout(false);

		}
	#endregion

	#region IDropDownPart Members

	private BaseDropDownControl _dropDownControl;
	public BaseDropDownControl DropDownControl
	{
		get
		{
				return _dropDownControl;
			}
		set
		{
				_dropDownControl = value;
			}
	}

	public string SelectedText
	{
		get
		{
				return monthCalendar1.SelectionStart.ToString(this.DropDownControl.EditControl.CustomFormat);
			}
		set
		{
				throw new NotImplementedException();
			}
	}

	public object SelectedValue
	{
		get
		{
				return monthCalendar1.SelectionStart;
			}
		set
		{
				DateTime dt;

				if(value is DateTime)
				{
					dt = (DateTime)value;

					monthCalendar1.SelectionStart = dt;
					monthCalendar1.SelectionEnd = dt;
				}
			}
	}

	private bool _canceled = false;
	public bool Canceled
	{
		get
		{
				return _canceled;
			}
		set
		{
				_canceled = value;
			}
	}

	public override bool Focused
	{
		get
		{
				if(base.Focused | monthCalendar1.Focused) 
				{
					return true;
				}
				else
				{
					return false;
				}
			}
	}

	public void MoveUp()
	{
			// TODO:  Add CalendarDropDown.MoveUp implementation
		}

	public void MoveDown()
	{
			// TODO:  Add CalendarDropDown.MoveDown implementation
		}

	public void SelectItem()
	{
			this.Close();
		}

	private void CalendarDropDown_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
	{
			switch(e.KeyCode)
			{
				case Keys.Escape:
					this.Canceled = true;
					this.Close();
					break;

				case Keys.Return:
					e.Handled = true;
					SelectItem();
					break;
			}
		}
	#endregion

	private void monthCalendar1_DateSelected(object sender, System.Windows.Forms.DateRangeEventArgs e)
	{
			SelectItem();
		}

	protected override bool ProcessTabKey(bool forward)
	{
			this.SelectItem();
			return true;
		}

	private void monthCalendar1_SizeChanged(object sender, System.EventArgs e)
	{
			ResizeMe();
		}

	private void ResizeMe()
	{
			this.Width = monthCalendar1.Width + 2;
			this.Height = monthCalendar1.Height + 2;
		}
}