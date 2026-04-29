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
using System.Collections;
using System.ComponentModel;
using System.Xml;
using Origam.Schema;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for PropertyGridBuilder.
/// </summary>
public class PropertyGridBuilder
{
    public static XmlDocument Build(ISchemaItem item)
    {
        XmlDocument doc = FormXmlBuilder.GetWindowBaseXml(
            name: item.Name,
            menuId: item.Id,
            autoRefreshInterval: 0,
            refreshOnFocus: false,
            autoSaveOnListRecordChange: false,
            requestSaveAfterUpdate: false
        );
        XmlElement windowElement = FormXmlBuilder.WindowElement(doc: doc);
        XmlElement uiRootElement = FormXmlBuilder.UIRootElement(windowElement: windowElement);
        XmlElement bindingsElement = FormXmlBuilder.ComponentBindingsElement(
            windowElement: windowElement
        );
        XmlElement dataSourcesElement = FormXmlBuilder.DataSourcesElement(
            windowElement: windowElement
        );
        CollapsibleContainerBuilder.Build(parentNode: uiRootElement);
        XmlElement children = doc.CreateElement(name: "UIChildren");
        uiRootElement.AppendChild(newChild: children);
        PropertyDescriptorCollection properties = GetProperties(component: item);
        Hashtable categories = new Hashtable();
        Hashtable categoryWidths = new Hashtable();
        foreach (PropertyDescriptor prop in properties)
        {
            XmlElement categoryChildren;
            if (categories.Contains(key: prop.Category))
            {
                categoryChildren = categories[key: prop.Category] as XmlElement;
            }
            else
            {
                XmlElement categoryElement = doc.CreateElement(name: "UIElement");
                children.AppendChild(newChild: categoryElement);
                categoryChildren = doc.CreateElement(name: "UIChildren");
                categoryElement.AppendChild(newChild: categoryChildren);
                UIElementRenderData renderData = new UIElementRenderData();
                renderData.Text = prop.Category;
                renderData.IndentLevel = 0;
                renderData.IsHeightFixed = false;
                renderData.IsOpen = true;
                CollapsiblePanelBuilder.Build(parentNode: categoryElement, renderData: renderData);
                categories.Add(key: prop.Category, value: categoryChildren);
                categoryWidths.Add(key: prop.Category, value: 0);
                XmlElement panelElement = doc.CreateElement(name: "UIElement");
                categoryChildren.AppendChild(newChild: panelElement);
                /*
                                AsPanelBuilder.Build(panelElement, prop.Category, item.Id.ToString() + prop.Category, false,
                                    false, true, false, false, false, "", "", "", "", "", "", "", "", item.Id.ToString() + prop.Category,
                                    table, dataSources, dataMember, 0, primaryKey, false, false, false, "", "", "", "", Guid.Empty,
                                    false, 0, item.Id, false, Guid.Empty, Guid.Empty, Guid.Empty, "", false, false, "", "", "", "", "", "", 0, "", "", "");
                */
            }
        }
        return doc;
    }

    private static PropertyDescriptorCollection GetProperties(object component)
    {
        Attribute[] attributes = new Attribute[] { BrowsableAttribute.Yes };
        return TypeDescriptor.GetProperties(component: component, attributes: attributes);
    }
}
