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
/// Summary description for StringFilterPart.
/// </summary>
public class StringFilterPart : TextBoxFilterPart
{
	#region Constructor
	public StringFilterPart(AsTextBox filteredControl, Type dataType, string dataMember, string gridColumnName, string label, FormGenerator formGenerator) : base(filteredControl, dataType, dataMember, gridColumnName, label, formGenerator)
	{
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
												FilterOperator.BeginsWith, 
												FilterOperator.NotBeginsWith, 
												FilterOperator.EndsWith, 
												FilterOperator.NotEndsWith, 
												FilterOperator.Contains, 
												FilterOperator.NotContains, 
												FilterOperator.IsNull, 
												FilterOperator.NotIsNull};
			}
	}

	public override FilterOperator DefaultOperator
	{
		get
		{
				return FilterOperator.BeginsWith;
			}
	}
	#endregion
}