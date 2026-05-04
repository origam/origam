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
using System.Data;
using System.Xml;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for AsPanelPropertyBuilder.
/// </summary>
public static class AsPanelPropertyBuilder
{
    public static XmlElement CreateProperty(
        XmlElement propertiesElement,
        XmlElement propertyNamesElement,
        Guid modelId,
        string bindingMember,
        string caption,
        string gridCaption,
        DataTable table,
        bool readOnly,
        int left,
        int top,
        int width,
        int height,
        int captionLength,
        string captionPosition,
        string gridColumnWidth,
        UIStyle style,
        string tabIndex,
        string fieldType
    )
    {
        return CreateProperty(
            category: "Property",
            propertiesElement: propertiesElement,
            propertyNamesElement: propertyNamesElement,
            modelId: modelId,
            bindingMember: bindingMember,
            caption: caption,
            gridCaption: gridCaption,
            table: table,
            readOnly: readOnly,
            left: left,
            top: top,
            width: width,
            height: height,
            captionLength: captionLength,
            captionPosition: captionPosition,
            gridColumnWidth: gridColumnWidth,
            style: style,
            tabIndex: tabIndex,
            fieldType: fieldType
        );
    }

    public static XmlElement CreateProperty(
        string category,
        XmlElement propertiesElement,
        XmlElement propertyNamesElement,
        Guid modelId,
        string bindingMember,
        string caption,
        string gridCaption,
        DataTable table,
        bool readOnly,
        int left,
        int top,
        int width,
        int height,
        int captionLength,
        string captionPosition,
        string gridColumnWidth,
        UIStyle style,
        string tabIndex,
        string fieldType
    )
    {
        IPersistenceProvider persistenceProvider = ServiceManager
            .Services.GetService<IPersistenceService>()
            .SchemaProvider;

        IDocumentationService documentationSvc =
            ServiceManager.Services.GetService(serviceType: typeof(IDocumentationService))
            as IDocumentationService;
        XmlElement propertyElement = propertiesElement.OwnerDocument.CreateElement(name: category);
        propertiesElement.AppendChild(newChild: propertyElement);
        if (propertyNamesElement != null)
        {
            XmlElement propertyNameElement = propertyNamesElement.OwnerDocument.CreateElement(
                name: "string"
            );
            propertyNamesElement.AppendChild(newChild: propertyNameElement);
            propertyNameElement.InnerText = bindingMember;
        }
        if (string.IsNullOrEmpty(value: caption))
        {
            caption = table.Columns[name: bindingMember].Caption;
        }

        Guid id = (Guid)table.Columns[name: bindingMember].ExtendedProperties[key: "Id"];
        string propertyDocumentation = documentationSvc
            .GetDocumentation(schemaItemId: id, docType: DocumentationType.USER_LONG_HELP)
            ?.Replace(oldValue: "'", newValue: "\'")
            .Replace(oldValue: "\"", newValue: "\\\"")
            .Replace(oldValue: "\r\n", newValue: "\\r\\n")
            .Replace(oldValue: "\t", newValue: "\\t");
        if (propertyDocumentation != "" & propertyDocumentation != null)
        {
            XmlElement propertyDoc = propertyElement.OwnerDocument.CreateElement(name: "ToolTip");
            propertyElement.AppendChild(newChild: propertyDoc);
            propertyDoc.InnerText = propertyDocumentation;
        }
        propertyElement.SetAttribute(name: "Id", value: bindingMember);
        propertyElement.SetAttribute(name: "ModelInstanceId", value: modelId.ToString());
        propertyElement.SetAttribute(name: "Name", value: caption);
        if (!string.IsNullOrEmpty(value: gridCaption))
        {
            propertyElement.SetAttribute(name: "GridColumnCaption", value: gridCaption);
        }

        propertyElement.SetAttribute(name: "ReadOnly", value: XmlConvert.ToString(value: readOnly));
        propertyElement.SetAttribute(name: "X", value: XmlConvert.ToString(value: left));
        propertyElement.SetAttribute(name: "Y", value: XmlConvert.ToString(value: top));
        propertyElement.SetAttribute(name: "Width", value: XmlConvert.ToString(value: width));
        if (!string.IsNullOrEmpty(value: gridColumnWidth))
        {
            propertyElement.SetAttribute(name: "GridColumnWidth", value: gridColumnWidth);
        }

        propertyElement.SetAttribute(name: "Height", value: XmlConvert.ToString(value: height));
        propertyElement.SetAttribute(
            name: "CaptionLength",
            value: XmlConvert.ToString(value: captionLength)
        );
        propertyElement.SetAttribute(name: "CaptionPosition", value: captionPosition);
        if (!string.IsNullOrWhiteSpace(value: tabIndex))
        {
            propertyElement.SetAttribute(name: "TabIndex", value: tabIndex);
        }
        if (style != null)
        {
            propertyElement.SetAttribute(name: "Style", value: style.StyleDefinition());
        }
        if (persistenceProvider.IsOfType<AggregatedColumn>(id: id))
        {
            propertyElement.SetAttribute(name: "Aggregated", value: "true");
        }

        if (persistenceProvider.IsOfType<LookupField>(id: id))
        {
            propertyElement.SetAttribute(name: "IsLookupColumn", value: "true");
        }
        propertyElement.SetAttribute(name: "FieldType", value: fieldType);
        return propertyElement;
    }
}
