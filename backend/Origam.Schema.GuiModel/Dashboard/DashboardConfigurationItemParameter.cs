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
/// Summary description for DashboardConfigurationItemParameter.
/// </summary>
[Serializable()]
public class DashboardConfigurationItemParameter
{
	private string _name;
	private bool _isBound;
	private string _value;
	public DashboardConfigurationItemParameter()
	{
	}
	[XmlAttribute("name")]
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
	[XmlAttribute("isBound")]
	public bool IsBound
	{
		get
		{
			return _isBound;
		}
		set
		{
			_isBound = value;
		}
	}
	[XmlAttribute("value")]
	public string Value
	{
		get
		{
			return _value;
		}
		set
		{
			_value = value;
		}
	}
	public Guid BoundItemId
	{
		get
		{
			string[] paramArray = this.Value.Split(".".ToCharArray());
			
			return new Guid(paramArray[0]);
		}
	}
	public string BoundItemProperty
	{
		get
		{
			string[] paramArray = this.Value.Split(".".ToCharArray());
			
			return paramArray[1];
		}
	}
}
