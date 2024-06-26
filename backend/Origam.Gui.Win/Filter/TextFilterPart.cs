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

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for DropDownFilterPart.
/// </summary>
public abstract class TextBoxFilterPart : FilterPart
{
	#region Constructor
	public TextBoxFilterPart(AsTextBox filteredControl, Type dataType, string dataMember, string gridColumnName, string label, FormGenerator formGenerator) : base(filteredControl, dataType, dataMember, gridColumnName, label, formGenerator)
	{
    }
	#endregion
	#region Properties
	private AsTextBox TextBox
	{
		get
		{
			return (AsTextBox)this.FilteredControl;
		}
	}
    AsTextBox _filterTextBox;
	private AsTextBox FilterTextBox
	{
		get
		{
			return _filterTextBox;
		}
	}
    AsTextBox _filterTextBox2;
	private AsTextBox FilterTextBox2
	{
		get
		{
			return _filterTextBox2;
		}
	}
	#endregion
	#region Overriden Members
	public override void CreateFilterControls()
	{
        if (_filterTextBox == null)
        {
			_filterTextBox = new AsTextBox();
        }
        if (_filterTextBox2 == null)
        {
			_filterTextBox2 = new AsTextBox();
        }
        UnsubscribeEvents();
		this.FilterControls.Clear();
		SetControlProperties(this.TextBox, this.FilterTextBox);
		SetControlProperties(this.TextBox, this.FilterTextBox2);
		this.FilterControls.Add(this.FilterTextBox);
		this.FilterControls.Add(this.FilterTextBox2);
		this.FilterTextBox.Tag = (this.Operator != FilterOperator.IsNull & this.Operator != FilterOperator.NotIsNull);
		this.FilterTextBox2.Tag = (this.Operator == FilterOperator.Between | this.Operator == FilterOperator.NotBetween);
		SubscribeEvents();
		OnControlsChanged();
	}
	private static void SetControlProperties(AsTextBox template, AsTextBox target)
	{
		target.CaptionPosition = CaptionPosition.None;
		target.DataType = template.DataType;
        target.CustomFormat = template.CustomFormat;
		target.TextAlign = template.TextAlign;
	}
	public override void LoadValues()
	{
		this.FilterTextBox.Value = this.Value1;
		this.FilterTextBox2.Value = this.Value2;
	}
	protected override void Dispose(bool disposing)
	{
		if(disposing)
		{
			if(this.FilterTextBox != null)
			{
				UnsubscribeEvents();
				_filterTextBox = null;
			}
			if(this.FilteredControl != null)
			{
				this.FilteredControl.Dispose();
			}
		}
		base.Dispose (disposing);
	}
	#endregion
	#region EventHandlers
	private void FilterTextBox_TextChanged(object sender, EventArgs e)
	{
        //this.FilterTextBox.UpdateValueWithCurrentText();
		this.Value1 = this.FilterTextBox.Value;
	}
	private void FilterTextBox2_TextChanged(object sender, EventArgs e)
	{
        //this.FilterTextBox2.UpdateValueWithCurrentText();
        this.Value2 = this.FilterTextBox2.Value;
	}
	#endregion
	#region Private Methods
	private void SubscribeEvents()
	{
		this.FilterTextBox.TextChanged += new EventHandler(FilterTextBox_TextChanged);
        this.FilterTextBox2.TextChanged += new EventHandler(FilterTextBox2_TextChanged);
	}
	private void UnsubscribeEvents()
	{
        if (FilterTextBox != null)
        {
            this.FilterTextBox.TextChanged -= new EventHandler(FilterTextBox_TextChanged);
        }
        if (FilterTextBox2 != null)
        {
            this.FilterTextBox2.TextChanged -= new EventHandler(FilterTextBox2_TextChanged);
        }
	}
	#endregion
}
