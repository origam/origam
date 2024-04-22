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

using System.Xml.Serialization;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for ComponentBinding.
/// </summary>
public class ComponentBinding
{
	public ComponentBinding()
	{
			
		}

	private string _parentComponent = "";
	[XmlAttribute()]
	public string ParentComponent
	{
		get
		{
				return _parentComponent;
			}
		set
		{
				_parentComponent = value;
			}
	}

	private string _parentProperty = "";
	[XmlAttribute()]
	public string ParentProperty
	{
		get
		{
				return _parentProperty;
			}
		set
		{
				_parentProperty = value;
			}
	}

	private string _childComponent = "";
	[XmlAttribute()]
	public string ChildComponent
	{
		get
		{
				return _childComponent;
			}
		set
		{
				_childComponent = value;
			}
	}

	private string _childProperty = "";
	[XmlAttribute()]
	public string ChildProperty
	{
		get
		{
				return _childProperty;
			}
		set
		{
				_childProperty = value;
			}
	}
}