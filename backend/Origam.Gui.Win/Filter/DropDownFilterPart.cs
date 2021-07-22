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
using Origam.Workbench.Services;

namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for DropDownFilterPart.
	/// </summary>
	public class DropDownFilterPart : FilterPart
	{
		#region Constructor
		public DropDownFilterPart(AsDropDown filteredControl, Type dataType, string dataMember, string gridColumnName, string label, FormGenerator formGenerator) : base(filteredControl, dataType, dataMember, gridColumnName, label, formGenerator)
		{
			this.FilterDropDown.CaptionPosition = CaptionPosition.None;
		}
		#endregion

		#region Properties
		private AsDropDown DropDown
		{
			get
			{
				return (AsDropDown)this.FilteredControl;
			}
		}

		AsDropDown _filterDropDown = new AsDropDown();
		private AsDropDown FilterDropDown
		{
			get
			{
				return _filterDropDown;
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
												FilterOperator.NotIsNull
											};
			}
		}

		public override FilterOperator DefaultOperator
		{
			get
			{
				return FilterOperator.Equals;
			}
		}


		public override void CreateFilterControls()
		{
			UnsubscribeEvents();
			this.FilterControls.Clear();

			if(this.DropDown == null) throw new NullReferenceException(ResourceUtils.GetString("ErrorDropDownNotSource"));

			if(this.DropDown.ParameterMappings.Count > 0)
			{
				throw new Exception(ResourceUtils.GetString("ErrorNoParams"));
			}

			this.FilterDropDown.Tag = (this.Operator != FilterOperator.IsNull & this.Operator != FilterOperator.NotIsNull);
			this.FilterDropDown.LookupId = this.DropDown.LookupId;

			this.FilterControls.Add(this.FilterDropDown);

			SubscribeEvents();

			OnControlsChanged();
		}

		public override void LoadValues()
		{
			this.FilterDropDown.LookupValue = this.Value1;
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if(this.FilterDropDown != null)
				{
					UnsubscribeEvents();
					_filterDropDown = null;
				}
			}

			base.Dispose (disposing);
		}


		#endregion

		#region EventHandlers
		private void FilterDropDown_lookupValueChanged(object sender, EventArgs e)
		{
			this.Value1 = this.FilterDropDown.LookupValue;
		}
		#endregion

		#region Private Methods
		private void SubscribeEvents()
		{
		    ServiceManager.Services.GetService<IControlsLookUpService>()
		        .AddLookupControl(this.FilterDropDown, this.FormGenerator.Form, false);
			this.FilterDropDown.lookupValueChanged += new EventHandler(FilterDropDown_lookupValueChanged);
		}

		private void UnsubscribeEvents()
		{
		    ServiceManager.Services.GetService<IControlsLookUpService>()
		        .RemoveLookupControl(this.FilterDropDown);
			this.FilterDropDown.lookupValueChanged -= new EventHandler(FilterDropDown_lookupValueChanged);
		}
		#endregion
	}
}
