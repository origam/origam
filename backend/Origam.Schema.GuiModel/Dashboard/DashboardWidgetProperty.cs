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

namespace Origam.Schema.GuiModel;
/// <summary>
/// Summary description for DashboardWidgetProperty.
/// </summary>
public class DashboardWidgetProperty
{
	public DashboardWidgetProperty()
	{
	}
	public DashboardWidgetProperty(string name, string caption, OrigamDataType dataType)
	{
		_name = name;
		_caption = caption;
		_dataType = dataType;
	}
	private string _name;
	public string Name
	{
		get
		{
			return _name;
		}
		set
		{
			_name = value;
		}
	}
	private string _caption;
	public string Caption
	{
		get
		{
			return _caption;
		}
		set
		{
			_caption = value;
		}
	}
	private OrigamDataType _dataType;
	public OrigamDataType DataType
	{
		get
		{
			return _dataType;
		}
		set
		{
			_dataType = value;
		}
	}
}
