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

using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Service.Core;

namespace Origam.Architect.Server.Services.Xslt;

public class ParameterData
{
    public string Name { get; }
    public string TextValue { get; } = "";
    public OrigamDataType Type { get; }

    public ParameterData(string name, string type)
    {
        Name = name;
        Type = StringTypeToParameterDataType(type: type);
    }

    public ParameterData(string name, string type, string textValue)
        : this(name: name, type: type)
    {
        TextValue = textValue;
    }

    private OrigamDataType StringTypeToParameterDataType(string type)
    {
        if (type == null)
        {
            return OrigamDataType.String;
        }

        return Enum.GetValues(enumType: typeof(OrigamDataType))
                .Cast<OrigamDataType?>()
                .FirstOrDefault(predicate: origamType => origamType.ToString() == type)
            ?? throw new ArgumentException(
                message: string.Format(format: Strings.WrongParameterType, arg0: type)
            );
    }

    public object Value
    {
        get
        {
            if (Type == OrigamDataType.Xml)
            {
                return new XmlContainer(xmlString: TextValue);
            }
            Type systemType = DatasetGenerator.ConvertDataType(dataType: Type);

            return DatasetTools.ConvertValue(value: TextValue, targetType: systemType);
        }
    }

    public override string ToString() => Name;
}
