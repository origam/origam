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

using System.Xml;
using Origam.Schema;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for TextBoxBuilder.
/// </summary>
public class TextBoxBuilder
{
    public static void Build(XmlElement propertyElement, TextBoxBuildDefinition buildDefinition)
    {
        if (
            (buildDefinition.Type == OrigamDataType.Integer)
            || (buildDefinition.Type == OrigamDataType.Long)
        )
        {
            propertyElement.SetAttribute(
                name: "IsInteger",
                value: XmlConvert.ToString(value: true)
            );
        }
        if (
            (buildDefinition.Type == OrigamDataType.Integer)
            || (buildDefinition.Type == OrigamDataType.Long)
            || (buildDefinition.Type == OrigamDataType.Float)
            || (buildDefinition.Type == OrigamDataType.Currency)
        )
        {
            propertyElement.SetAttribute(name: "Entity", value: buildDefinition.Type.ToString());
            propertyElement.SetAttribute(name: "Column", value: "Number");
            propertyElement.SetAttribute(
                name: "CustomNumericFormat",
                value: buildDefinition.CustomNumberFormat
            );
        }
        else
        {
            propertyElement.SetAttribute(name: "Entity", value: "String");
            propertyElement.SetAttribute(name: "Column", value: "Text");
            propertyElement.SetAttribute(name: "Dock", value: buildDefinition.Dock);
            propertyElement.SetAttribute(
                name: "Multiline",
                value: XmlConvert.ToString(value: buildDefinition.Multiline)
            );
            propertyElement.SetAttribute(
                name: "IsPassword",
                value: XmlConvert.ToString(value: buildDefinition.IsPassword)
            );
            propertyElement.SetAttribute(
                name: "IsRichText",
                value: XmlConvert.ToString(value: buildDefinition.IsRichText)
            );
            propertyElement.SetAttribute(
                name: "AllowTab",
                value: XmlConvert.ToString(value: buildDefinition.AllowTab)
            );
            propertyElement.SetAttribute(
                name: "MaxLength",
                value: XmlConvert.ToString(value: buildDefinition.MaxLength)
            );
        }
    }
}

public class TextBoxBuildDefinition
{
    private OrigamDataType _Type;
    public OrigamDataType Type
    {
        get { return _Type; }
    }
    private string _Dock = "None";
    public string Dock
    {
        get { return _Dock; }
        set { _Dock = value; }
    }
    private bool _Multiline = false;
    public bool Multiline
    {
        get { return _Multiline; }
        set { _Multiline = value; }
    }
    private bool _IsPassword = false;
    public bool IsPassword
    {
        get { return _IsPassword; }
        set { _IsPassword = value; }
    }
    private bool _IsRichText = false;
    public bool IsRichText
    {
        get { return _IsRichText; }
        set { _IsRichText = value; }
    }
    private bool _AllowTab;
    public bool AllowTab
    {
        get { return _AllowTab; }
        set { _AllowTab = value; }
    }
    private int _MaxLength = 0;
    public int MaxLength
    {
        get { return _MaxLength; }
        set { _MaxLength = value; }
    }

    public TextBoxBuildDefinition(OrigamDataType type)
    {
        _Type = type;
    }

    private string _CustomNumberFormat;
    public string CustomNumberFormat
    {
        get { return _CustomNumberFormat; }
        set { _CustomNumberFormat = value; }
    }
}
