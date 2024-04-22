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

namespace Origam.DA;

/// <summary>
/// Reference to the parameter in the data structure.
/// </summary>
[XmlType("parameter")]
public class QueryParameter
{
	public QueryParameter()
	{
		}

	public QueryParameter(string _parameterName, object value)
	{
			this._parameterName = _parameterName;
			this.Value = value;
		}

	string _parameterName = "";
	/// <summary>
	/// Gets or sets name of the parameter we will be setting on the query.
	/// </summary>
	[XmlAttribute("name")]
	public string Name
	{
		get
		{
				return _parameterName;
			}
		set
		{
				_parameterName = value;
			}
	}

	object _value = null;
	/// <summary>
	/// Gets or sets value of the parameter.
	/// </summary>
	[XmlElement("value")]
	public object Value
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
}