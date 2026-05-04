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
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for DashboardSimpleWidgetBuilder.
/// </summary>
public class DashboardSimpleWidgetBuilder
{
    public static void Build(
        XmlDocument doc,
        AbstractSimpleDashboardWidget simpleWidget,
        Guid dashboardItemId,
        string caption,
        XmlElement itemChildren,
        XmlElement dataSourcesElement
    )
    {
        string modelInstanceId = dashboardItemId.ToString();
        XmlElement element =
            itemChildren.AppendChild(newChild: doc.CreateElement(name: "UIElement")) as XmlElement;
        element.SetAttribute(
            localName: "type",
            namespaceURI: "http://www.w3.org/2001/XMLSchema-instance",
            value: "UIElement"
        );
        element.SetAttribute(name: "Type", value: "SimplePanel");
        element.SetAttribute(name: "Id", value: simpleWidget.Id.ToString());
        element.SetAttribute(name: "ModelInstanceId", value: modelInstanceId);
        element.SetAttribute(name: "Entity", value: modelInstanceId);
        element.SetAttribute(name: "Height", value: "20");
        XmlElement componentElement =
            element.AppendChild(newChild: doc.CreateElement(name: "Component")) as XmlElement;
        componentElement.SetAttribute(name: "Name", value: caption);
        componentElement.SetAttribute(name: "CaptionLength", value: "100");
        componentElement.SetAttribute(name: "CaptionPosition", value: "Left");
        componentElement.SetAttribute(name: "DataMember", value: "Value");
        LookupDashboardWidget lookupWidget = simpleWidget as LookupDashboardWidget;
        CurrencyDashboardWidget currencyWidget = simpleWidget as CurrencyDashboardWidget;
        TextDashboardWidget textWidget = simpleWidget as TextDashboardWidget;
        DateDashboardWidget dateWidget = simpleWidget as DateDashboardWidget;
        CheckBoxDashboardWidget checkWidghet = simpleWidget as CheckBoxDashboardWidget;
        if (lookupWidget != null)
        {
            ComboBoxBuilder.Build(
                propertyElement: componentElement,
                lookupId: lookupWidget.LookupId,
                showUniqueValues: false,
                bindingMember: null,
                table: null
            );
        }
        else if (textWidget != null)
        {
            TextBoxBuilder.Build(
                propertyElement: componentElement,
                buildDefinition: new TextBoxBuildDefinition(type: OrigamDataType.String)
            );
        }
        else if (currencyWidget != null)
        {
            TextBoxBuilder.Build(
                propertyElement: componentElement,
                buildDefinition: new TextBoxBuildDefinition(type: OrigamDataType.Currency)
            );
        }
        else if (dateWidget != null)
        {
            DateBoxBuilder.Build(
                propertyElement: componentElement,
                format: null,
                customFormat: null
            );
        }
        else if (checkWidghet != null)
        {
            CheckBoxBuilder.Build(propertyElement: componentElement, text: caption);
        }
        XmlElement newDataSourceElement = FormXmlBuilder.AddDataSourceElement(
            dataSourcesElement: dataSourcesElement,
            entity: modelInstanceId,
            identifier: "Id",
            lookupCacheKey: null,
            dataStructureEntityId: null
        );
        newDataSourceElement.AppendChild(
            newChild: FormXmlBuilder.CreateDataSourceField(doc: doc, name: "Id", index: 0)
        );
        newDataSourceElement.AppendChild(
            newChild: FormXmlBuilder.CreateDataSourceField(doc: doc, name: "Value", index: 1)
        );
    }
}
