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

using Origam.Schema.EntityModel;

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for DataGridSortItem.
/// </summary>
public class DataSortItem : IComparable
{
	public DataSortItem(string columnName, DataStructureColumnSortDirection sortDirection, int sortOrder)
	{
		this.ColumnName = columnName;
		this.SortDirection = sortDirection;
		this.SortOrder = sortOrder;
	}
	private DataStructureColumnSortDirection _sortDirection;
	public DataStructureColumnSortDirection SortDirection
	{
		get
		{
			return _sortDirection;
		}
		set
		{
			_sortDirection = value;
		}
	}
	private string _columnName;
	public string ColumnName
	{
		get
		{
			return _columnName;
		}
		set
		{
			_columnName = value;
		}
	}
	private int _sortOrder;
	public int SortOrder
	{
		get
		{
			return _sortOrder;
		}
		set
		{
			_sortOrder = value;
		}
	}
	#region IComparable Members
	public int CompareTo(object obj)
	{
		DataSortItem compareItem = obj as DataSortItem;
		if(compareItem == null) throw new InvalidCastException(ResourceUtils.GetString("ErrorCompareDataSortItem"));
		return this.SortOrder.CompareTo(compareItem.SortOrder);
	}
	#endregion
}
