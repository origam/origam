#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using System.Linq;
using System.Xml.Serialization;
using Origam.DA;
using Origam.DA.Common.Extensiosn;

namespace Origam.Server;

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[XmlType(TypeName = "parameter", Namespace = "http://asapenginewebapi.advantages.cz/")]
public partial class Parameter
{
    private object valueField;

    private string nameField;

    /// <remarks/>
    [XmlElement(Order = 0)]
    public object value
    {
        get { return this.valueField; }
        set { this.valueField = value; }
    }

    /// <remarks/>
    [XmlAttribute()]
    public string name
    {
        get { return this.nameField; }
        set { this.nameField = value; }
    }
}

public static class ParameterUtils
{
    public static QueryParameterCollection ToQueryParameterCollection(Parameter[] parameters)
    {
        return (parameters ?? new Parameter[0])
            .Select(x => new QueryParameter(x.name, x.value))
            .ToQueryParameterCollection();
    }
}
