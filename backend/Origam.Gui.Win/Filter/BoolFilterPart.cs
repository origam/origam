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
using System.Drawing;

namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for DropDownFilterPart.
	/// </summary>
	public class BoolFilterPart : FilterPart
	{
		#region Constructor
		public BoolFilterPart(AsCheckBox filteredControl, Type dataType, string dataMember, string gridColumnName, string label, FormGenerator formGenerator) : base(filteredControl, dataType, dataMember, gridColumnName, label, formGenerator)
		{
			this.FilterCheckBox.CaptionPosition = CaptionPosition.None;
		}
		#endregion

		#region Overriden Members
		public override FilterOperator[] AllowedOperators
		{
			get
			{
				return new FilterOperator[] {FilterOperator.Equals};
			}
		}

		public override FilterOperator DefaultOperator
		{
			get
			{
				return FilterOperator.Equals;
			}
		}
		#endregion

		#region Properties
		private AsCheckBox CheckBox
		{
			get
			{
				return (AsCheckBox)this.FilteredControl;
			}
		}

		AsCheckBox _filterCheckBox = new AsCheckBox();
		private AsCheckBox FilterCheckBox
		{
			get
			{
				return _filterCheckBox;
			}
		}
		#endregion

		#region Overriden Members

		public override void CreateFilterControls()
		{
			UnsubscribeEvents();
			this.FilterControls.Clear();

			this.FilterCheckBox.FlatStyle = FlatStyle.Standard;
			this.FilterCheckBox.CheckAlign = ContentAlignment.MiddleCenter;
			this.FilterCheckBox.ThreeState = true;
			this.FilterCheckBox.CheckState = CheckState.Indeterminate;
			this.FilterCheckBox.Tag = true;

			this.FilterControls.Add(this.FilterCheckBox);

			SubscribeEvents();
		}

		public override void LoadValues()
		{
			this.FilterCheckBox.Value = this.Value1;
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if(this.FilterCheckBox != null)
				{
					UnsubscribeEvents();
					_filterCheckBox = null;
				}
			}

			base.Dispose (disposing);
		}


		#endregion

		#region EventHandlers
		private void FilterCheckBox_ValueChanged(object sender, EventArgs e)
		{
			this.Value1 = this.FilterCheckBox.Value;
		}
		#endregion

		#region Private Methods
		private void SubscribeEvents()
		{
			this.FilterCheckBox.valueChanged += new EventHandler(FilterCheckBox_ValueChanged);
		}

		private void UnsubscribeEvents()
		{
			this.FilterCheckBox.valueChanged -= new EventHandler(FilterCheckBox_ValueChanged);
		}
		#endregion

	}
}
