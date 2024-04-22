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
public class DateFilterPart : FilterPart
{
	#region Constructor
	public DateFilterPart(AsDateBox filteredControl, Type dataType, string dataMember, string gridColumnName, string label, FormGenerator formGenerator) : base(filteredControl, dataType, dataMember, gridColumnName, label, formGenerator)
	{
		}
	#endregion

	#region Properties
	private AsDateBox DateBox
	{
		get
		{
				return (AsDateBox)this.FilteredControl;
			}
	}

	AsDateBox _filterDateBox = new AsDateBox();
	private AsDateBox FilterDateBox
	{
		get
		{
				return _filterDateBox;
			}
	}

	AsDateBox _filterDateBox2 = new AsDateBox();
	private AsDateBox FilterDateBox2
	{
		get
		{
				return _filterDateBox2;
			}
	}
	#endregion

	#region Overriden Members
	public override FilterOperator[] AllowedOperators
	{
		get
		{
				return new FilterOperator[] {
												FilterOperator.Equals, 
												FilterOperator.NotEquals,
												FilterOperator.IsNull,
												FilterOperator.NotIsNull,
												FilterOperator.GreaterOrEqualThan,
												FilterOperator.LessOrEqualThan,
												FilterOperator.GreaterThan,
												FilterOperator.LessThan,
												FilterOperator.Between,
												FilterOperator.NotBetween
				};
			}
	}

	public override FilterOperator DefaultOperator
	{
		get
		{
				return FilterOperator.GreaterOrEqualThan;
			}
	}


	public override void CreateFilterControls()
	{
			UnsubscribeEvents();
			this.FilterControls.Clear();

			SetControlProperties(this.DateBox, this.FilterDateBox);
			SetControlProperties(this.DateBox, this.FilterDateBox2);

			this.FilterControls.Add(this.FilterDateBox);
			this.FilterControls.Add(this.FilterDateBox2);

			this.FilterDateBox.Tag = (this.Operator != FilterOperator.IsNull & this.Operator != FilterOperator.NotIsNull);
			this.FilterDateBox2.Tag = (this.Operator == FilterOperator.Between | this.Operator == FilterOperator.NotBetween);

			SubscribeEvents();

			OnControlsChanged();
		}

	private static void SetControlProperties(AsDateBox template, AsDateBox target)
	{
			target.CaptionPosition = CaptionPosition.None;
			target.Format = template.Format;
			target.CustomFormat = template.CustomFormat;
		}

	public override void LoadValues()
	{
			this.FilterDateBox.DateValue = this.Value1;
			this.FilterDateBox2.DateValue = this.Value2;
		}

	protected override void Dispose(bool disposing)
	{
			if(disposing)
			{
				if(this.FilterDateBox != null)
				{
					UnsubscribeEvents();
					_filterDateBox = null;
				}
			}

			base.Dispose (disposing);
		}


	#endregion

	#region EventHandlers
	private void FilterDateBox_dateValueChanged(object sender, EventArgs e)
	{
			this.Value1 = this.FilterDateBox.DateValue;
		}

	private void FilterDateBox2_dateValueChanged(object sender, EventArgs e)
	{
			if(this.FilterDateBox2.DateValue != null)
			{
				DateTime dt = (DateTime)this.FilterDateBox2.DateValue;

				if(dt.TimeOfDay.Ticks == 0)
				{
					this.Value2 = dt.AddDays(1).AddSeconds(-1);
				}
				else
				{
					this.Value2 = dt;
				}
			}
			else
			{
				this.Value2 = this.FilterDateBox2.DateValue;
			}
		}
	#endregion

	#region Private Methods
	private void SubscribeEvents()
	{
			this.FilterDateBox.dateValueChanged += new EventHandler(FilterDateBox_dateValueChanged);
			this.FilterDateBox2.dateValueChanged += new EventHandler(FilterDateBox2_dateValueChanged);
		}

	private void UnsubscribeEvents()
	{
			this.FilterDateBox.dateValueChanged -= new EventHandler(FilterDateBox_dateValueChanged);
			this.FilterDateBox2.dateValueChanged -= new EventHandler(FilterDateBox2_dateValueChanged);
		}
	#endregion
}