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
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace Origam.Gui.Win;
/// <summary>
/// Represents a Windows date time picker control. It enhances the .NET standard <b>DateTimePicker</b>
/// control with a ReadOnly mode as well as with the possibility to show empty values (null values).
/// </summary>
[ComVisible(false)]
[ToolboxBitmap(typeof(AsDateBox))]
public class AsDateBox : BaseDropDownControl, INotifyPropertyChanged
{
	public event EventHandler dateValueChanged;
	public event EventHandler onTextBoxValueChanged; // se AsDataViewColumn for comments
	#region Member variables
	// The format of the DateTimePicker control
	private DateTimePickerFormat format = DateTimePickerFormat.Long;
	// The custom format of the DateTimePicker control
	private string customFormat;
	#endregion
	#region Constructor
	/// <summary>
	/// Default Constructor
	/// </summary>
	public AsDateBox()
	{
		CultureInfo ci = Thread.CurrentThread.CurrentCulture;
		DateTimeFormatInfo dtf = ci.DateTimeFormat;
		this.EditControl.CustomFormat = dtf.LongDatePattern;
		this.EditControl.DataType = typeof(DateTime);
		
		this.OpenListButton.Visible = false;
		this.ShowCalendarIcon = true;
		this.SetFocusToDropDown = true;
		
		this.EditControl.ValueChanged += EditControl_ValueChanged;
		this.EditControl.KeyUp += EditControl_KeyUp;
		this.PopupHelper.PopupClosed += PopupHelper_PopupClosed;
	}
	#endregion
	#region Public properties
	private bool _enabled = true;
	public new bool Enabled
	{
		get
		{
			if(this.ReadOnly) 
			{
				return false;
			}
			else
			{
				return _enabled;
			}
		}
		set
		{
			_enabled = value;
			if(this.ReadOnly)
			{
				if(this.DesignMode)
				{
					base.Enabled = value;
				}
				else
				{
					base.Enabled = false;
				}
			}
			else
			{
				base.Enabled = value;
			}
		}
	}
	private object _selectedValue = null;
	public override object SelectedValue
	{
		get
		{
			if(_selectedValue == null)
			{
				return DBNull.Value;
			}
			else
			{
				return _selectedValue;
			}
		}
		set
		{
			if(value == null)
			{
				_selectedValue = DBNull.Value;
			}
			else
			{
				_selectedValue = value;
			}
			this.DateValue = _selectedValue;
		}
	}
	bool _settingValue = false;
	/// <summary>
	/// Gets or sets the date/time value assigned to the control.
	/// </summary>
	/// <value>The DateTime value assigned to the control
	/// </value>
	/// <remarks>
	/// <p>If the <b>Value</b> property has not been changed in code or by the user, it is set
	/// to the current date and time (<see cref="DateTime.Now"/>).</p>
	/// <p>If <b>Value</b> is <b>null</b>, the DateTimePicker shows 
	/// <see cref="NullValue"/>.</p>
	/// </remarks>
	public Object DateValue 
	{
		get => this.SelectedValue;
		set 
		{
			if(_settingValue) return;
			if(_selectedValue != value)
			{
				string converted = null;
				if(value is DateTime)
				{
					DateTime dt = (DateTime)value;
					converted = dt.ToString(this.EditControl.CustomFormat);
					//value = DateTime.Parse(converted);
				}
				if(value is string)
				{
					DateTime dt = DateTime.Parse((string)value);
					converted = dt.ToString(this.EditControl.CustomFormat);
					//value = DateTime.Parse(converted);
				}
				if((_selectedValue == null & value != DBNull.Value & value != null) || (_selectedValue != null && ! _selectedValue.Equals(value)))
				{
					_selectedValue = value;
                    if(! valueChangedByUser && 
                        !(
                        this.EditControl.Value.Equals(converted)
                        || (this.EditControl.Value == DBNull.Value && converted == null ))
                        )
                    {
						_settingValue = true;
						this.DisplayText = converted;
						_settingValue = false;
                    }
					OnDateValueChanged(EventArgs.Empty);
				}
			}
		}
	}
	/// <summary>
	/// Gets or sets the format of the date and time displayed in the control.
	/// </summary>
	/// <value>One of the <see cref="DateTimePickerFormat"/> values. The default is 
	/// <see cref="DateTimePickerFormat.Long"/>.</value>
	[Browsable(true)]
	[DefaultValue(DateTimePickerFormat.Long), TypeConverter(typeof(Enum))]
	public DateTimePickerFormat Format
	{
		get => format;
		set
		{
			if(value == 0) value = DateTimePickerFormat.Long;
			format = value;
			SetFormat();
		}
	}
	/// <summary>
	/// Gets or sets the custom date/time format string.
	/// <value>A string that represents the custom date/time format. The default is a null
	/// reference (<b>Nothing</b> in Visual Basic).</value>
	/// </summary>
	/// 
	[DefaultValue("dd.MMMM yyyy")]
	public String CustomFormat
	{
		get => customFormat;
		set 
		{ 
			customFormat = value; 
			SetFormat();
		}
	}
	#endregion
	#region Private methods/properties
	/// <summary>
	/// Sets the format according to the current DateTimePickerFormat.
	/// </summary>
	private void SetFormat()
	{
		CultureInfo ci = Thread.CurrentThread.CurrentCulture;
		DateTimeFormatInfo dtf = ci.DateTimeFormat;
		switch (format)
		{
			case DateTimePickerFormat.Long:
				this.EditControl.CustomFormat = dtf.LongDatePattern;
				break;  
			case DateTimePickerFormat.Short:
				this.EditControl.CustomFormat = dtf.ShortDatePattern;
				break;
			case DateTimePickerFormat.Time:
				this.EditControl.CustomFormat = dtf.ShortTimePattern;
				break;
			case DateTimePickerFormat.Custom:
				this.EditControl.CustomFormat = this.CustomFormat;
				break;
		}
	}
	protected override void Dispose(bool disposing)
	{
		if(disposing)
		{
			this.EditControl.KeyUp -=EditControl_KeyUp;
		}
		base.Dispose (disposing);
	}
	public override IDropDownPart CreatePopup()
	{
		return new CalendarDropDown();
	}
	#endregion
	#region OnXXXX()
	void OnDateValueChanged(EventArgs e)
	{
		dateValueChanged?.Invoke(this, e);
		PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DateValue"));
	}
	/// <summary>
	/// This member overrides <see cref="Control.OnKeyDown"/>.
	/// </summary>
	/// <param name="e"></param>
	#endregion
	#region IAsControl Members
	
	public override string DefaultBindableProperty { get; } = "DateValue";
	#endregion
	private void EditControl_KeyUp(object sender, KeyEventArgs e)
	{
        if (! this.Enabled)
        {
            return;
        }
        if (e.KeyCode == Keys.Delete)
		{
			this.DateValue = null;
		}
		// show the dropdown part on Alt+Down
		if(e.Alt & e.KeyCode == Keys.Down)
		{
			DropDown();
			return;
		}
		switch (e.KeyCode)
		{
			case Keys.Return:
				if(this.DroppedDown)
				{
					Popup.SelectItem();
				}
				return;
		}
		if(DroppedDown)
		{
			this.Popup.Canceled = true;
			this.PopupHelper.ClosePopup();
		}
	}
	bool valueChangedByUser = false;
	private void EditControl_ValueChanged(object sender, EventArgs e)
	{
        if (!this.Enabled) return;
		if (! this.DateValue.Equals(this.EditControl.Value))
	    {
	        valueChangedByUser = true;
	        if (EditControl.Value != null && EditControl.Value != DBNull.Value)
	        {
	            DateTime dt = (DateTime) EditControl.Value;
	            string converted = dt.ToString(EditControl.CustomFormat);
		        var pasrseSucessful = DateTime.TryParse(converted,out var parsedDate);
		        if (pasrseSucessful)
		        {
			        this.DateValue = parsedDate;
		        } else
		        {
			        throw new ArgumentException(String.Format(
				        ResourceUtils.GetString("WrongCustomFormatMeaasge"),
				        dt.ToString(),EditControl.CustomFormat));
		        }
		        onTextBoxValueChanged?.Invoke(this, e);
	        }
	        else
	        {
	            this.DateValue = null;
	        }
            valueChangedByUser = false;
		}
	}
	private void PopupHelper_PopupClosed(object sender, PopupClosedEventArgs e)
	{
		if(e.Popup != null)
		{
			if(!(e.Popup as IDropDownPart).Canceled)
			{
                DateTime dt = (DateTime)(e.Popup as IDropDownPart).SelectedValue;
                string converted = dt.ToString(this.EditControl.CustomFormat);
                this.DateValue = DateTime.Parse(converted);
			}
		}
		this.DroppedDown = false;
	}
    public event PropertyChangedEventHandler PropertyChanged;
}
