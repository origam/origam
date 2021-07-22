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

using System.Data;

namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for IDropDownPart.
	/// </summary>
	public interface ILookupDropDownPart : IDropDownPart
	{
		DataView DataSource{get; set;}
		string ValueMember{get; set;}
		string DisplayMember{get; set;}
		string ParentMember{get; set;}
	}
}
