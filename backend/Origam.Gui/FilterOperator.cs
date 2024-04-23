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

namespace Origam.Gui;

/// <summary>
/// Summary description for FilterOperator.
/// </summary>
public enum FilterOperator
{
	None = 0,
	Equals = 1,
	Between = 2, 
	BeginsWith = 3,
	EndsWith = 4,
	Contains = 5,
	GreaterThan = 6,
	LessThan = 7,
	GreaterOrEqualThan = 8,
	LessOrEqualThan = 9,
	NotEquals = 10,
	NotBetween = 11,
	NotBeginsWith = 12,
	NotEndsWith = 13,
	NotContains = 14,
	IsNull = 15, 
	NotIsNull = 16
}