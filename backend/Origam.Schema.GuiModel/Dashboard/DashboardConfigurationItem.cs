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
using System.Xml.Serialization;

namespace Origam.Schema.GuiModel;
/// <summary>
/// Summary description for DashboardConfigurationItem.
/// </summary>
[Serializable()]
public class DashboardConfigurationItem
{
	private string _label;
	private Guid _id;
	private Guid _componentId;
	private int _left;
	private int _top;
	private int _colSpan;
	private int _rowSpan;
	private DashboardConfigurationItemParameter[] _parameters;
	private DashboardConfigurationItem[] _items;
	public DashboardConfigurationItem()
	{
	}
	[XmlAttribute("label")]
	public string Label
	{
		get
		{
			return _label;
		}
		set
		{
			_label = value;
		}
	}
	[XmlAttribute("id")]
	public Guid Id
	{
		get
		{
			return _id;
		}
		set
		{
			_id = value;
		}
	}
	[XmlAttribute("componentId")]
	public Guid ComponentId
	{
		get
		{
			return _componentId;
		}
		set
		{
			_componentId = value;
		}
	}
	[XmlAttribute("left")]
	public int Left
	{
		get
		{
			return _left;
		}
		set
		{
			_left = value;
		}
	}
	[XmlAttribute("top")]
	public int Top
	{
		get
		{
			return _top;
		}
		set
		{
			_top = value;
		}
	}

	[XmlAttribute("colSpan")]
	public int ColSpan
	{
		get
		{
			return _colSpan;
		}
		set
		{
			_colSpan = value;
		}
	}

	[XmlAttribute("rowSpan")]
	public int RowSpan
	{
		get
		{
			return _rowSpan;
		}
		set
		{
			_rowSpan = value;
		}
	}
	[XmlElement("parameter", typeof(DashboardConfigurationItemParameter))]
	public DashboardConfigurationItemParameter[] Parameters
	{
		get
		{
			return _parameters;
		}
		set
		{
			_parameters = value;
		}
	}
	[XmlArray("children")]
	[XmlArrayItem("item", typeof(DashboardConfigurationItem))]
	public DashboardConfigurationItem[] Items
	{
		get
		{
			return _items;
		}
		set
		{
			_items = value;
		}
	}
}
