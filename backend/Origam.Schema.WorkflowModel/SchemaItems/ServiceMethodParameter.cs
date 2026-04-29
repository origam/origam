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
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Parameter that can be used to parametrize any kind of schema item.
/// </summary>
[SchemaItemDescription(name: "Parameter", folderName: "Parameters", iconName: "parameter-blm.png")]
[HelpTopic(topic: "Service+Method+Parameter")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class ServiceMethodParameter : SchemaItemParameter
{
    public ServiceMethodParameter()
        : base() { }

    public ServiceMethodParameter(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public ServiceMethodParameter(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    enum CallElementsEnum
    {
        ContextReference = 1,
        DataConstantReference = 2,
        DataStructureReference = 4,
        TransformationReference = 8,
        ReportReference = 16,
        WorkflowReference = 32,
        SystemFunctionCall = 64,
    }

    private bool Resolve(CallElementsEnum element)
    {
        return (CallElements & (int)element) == (int)element;
    }

    private void Set(CallElementsEnum element, bool value)
    {
        bool current = Resolve(element: element);
        if (current == value)
        {
            return;
        }

        if (value)
        {
            CallElements += (int)element;
        }
        else
        {
            CallElements -= (int)element;
        }
    }

    #region Properties
    private int _callElements;

    [Browsable(browsable: false)]
    [XmlAttribute(attributeName: "callElements")]
    public int CallElements
    {
        get { return _callElements; }
        set { _callElements = value; }
    }

    [DefaultValue(value: false), Category(category: "Allowed Child Elements")]
    [XmlAttribute(attributeName: "allowContextReference")]
    public bool AllowContextReference
    {
        get { return Resolve(element: CallElementsEnum.ContextReference); }
        set { Set(element: CallElementsEnum.ContextReference, value: value); }
    }

    [DefaultValue(value: false), Category(category: "Allowed Child Elements")]
    [XmlAttribute(attributeName: "allowDataConstantReference")]
    public bool AllowDataConstantReference
    {
        get { return Resolve(element: CallElementsEnum.DataConstantReference); }
        set { Set(element: CallElementsEnum.DataConstantReference, value: value); }
    }

    [DefaultValue(value: false), Category(category: "Allowed Child Elements")]
    [XmlAttribute(attributeName: "allowDataStructureReference")]
    public bool AllowDataStructureReference
    {
        get { return Resolve(element: CallElementsEnum.DataStructureReference); }
        set { Set(element: CallElementsEnum.DataStructureReference, value: value); }
    }

    [DefaultValue(value: false), Category(category: "Allowed Child Elements")]
    [XmlAttribute(attributeName: "allowReportReference")]
    public bool AllowReportReference
    {
        get { return Resolve(element: CallElementsEnum.ReportReference); }
        set { Set(element: CallElementsEnum.ReportReference, value: value); }
    }

    [DefaultValue(value: false), Category(category: "Allowed Child Elements")]
    [XmlAttribute(attributeName: "allowSystemFunctionCall")]
    public bool AllowSystemFunctionCall
    {
        get { return Resolve(element: CallElementsEnum.SystemFunctionCall); }
        set { Set(element: CallElementsEnum.SystemFunctionCall, value: value); }
    }

    [DefaultValue(value: false), Category(category: "Allowed Child Elements")]
    [XmlAttribute(attributeName: "allowTransformationReference")]
    public bool AllowTransformationReference
    {
        get { return Resolve(element: CallElementsEnum.TransformationReference); }
        set { Set(element: CallElementsEnum.TransformationReference, value: value); }
    }

    [DefaultValue(value: false), Category(category: "Allowed Child Elements")]
    [XmlAttribute(attributeName: "allowWorkflowReference")]
    public bool AllowWorkflowReference
    {
        get { return Resolve(element: CallElementsEnum.WorkflowReference); }
        set { Set(element: CallElementsEnum.WorkflowReference, value: value); }
    }
    #endregion
}
