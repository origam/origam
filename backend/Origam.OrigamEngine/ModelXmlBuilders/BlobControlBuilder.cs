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
using System.Xml;
using Origam.Schema.GuiModel;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for DateBoxBuilder.
/// </summary>
public class BlobControlBuilder
{
    public static void Build(XmlElement propertyElement, ControlSetItem control)
    {
        propertyElement.SetAttribute(name: "Entity", value: "String");
        propertyElement.SetAttribute(name: "Column", value: "Blob");
        XmlElement propertiesElement = propertyElement.OwnerDocument.CreateElement(
            name: "Parameters"
        );
        propertyElement.AppendChild(newChild: propertiesElement);

        foreach (
            var property in control.ChildItemsByType<PropertyValueItem>(
                itemType: PropertyValueItem.CategoryConst
            )
        )
        {
            string name = property.ControlPropertyItem.Name;
            if (
                name != "Caption"
                && name != "CaptionPosition"
                && name != "CaptionLength"
                && name != "Height"
                && name != "Width"
                && name != "Left"
                && name != "Top"
                && name != "ReadOnly"
                && name != "TabIndex"
            )
            {
                XmlElement blobPropertyElement = propertiesElement.OwnerDocument.CreateElement(
                    name: "Parameter"
                );
                propertiesElement.AppendChild(newChild: blobPropertyElement);
                string value;
                switch (property.ControlPropertyItem.PropertyType)
                {
                    case ControlPropertyValueType.Boolean:
                    {
                        value = XmlConvert.ToString(value: property.BoolValue);
                        break;
                    }

                    case ControlPropertyValueType.Integer:
                    {
                        value = XmlConvert.ToString(value: property.IntValue);
                        break;
                    }

                    case ControlPropertyValueType.String:
                    {
                        value = property.Value;
                        break;
                    }

                    case ControlPropertyValueType.UniqueIdentifier:
                    {
                        value = property.GuidValue.ToString();
                        break;
                    }

                    case ControlPropertyValueType.Xml:
                    {
                        value = property.Value;
                        break;
                    }

                    default:
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "PropertyType",
                            actualValue: property.ControlPropertyItem.PropertyType,
                            message: "Unknown property type."
                        );
                    }
                }
                blobPropertyElement.SetAttribute(
                    name: "Name",
                    value: property.ControlPropertyItem.Name
                );
                blobPropertyElement.SetAttribute(name: "Value", value: value);
            }
        }
    }
}
