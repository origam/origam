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
using System.Collections;
using System.ComponentModel;

using Origam.Schema;

namespace Origam.OrigamEngine.ModelXmlBuilders;
/// <summary>
/// Summary description for PropertyGridBuilder.
/// </summary>
public class PropertyGridBuilder
{
	public static XmlDocument Build(AbstractSchemaItem item)
	{
		XmlDocument doc = FormXmlBuilder.GetWindowBaseXml(
			item.Name, item.Id, 0, false, false, false);
		XmlElement windowElement = FormXmlBuilder.WindowElement(doc);
		XmlElement uiRootElement = FormXmlBuilder.UIRootElement(windowElement);
		XmlElement bindingsElement = FormXmlBuilder.ComponentBindingsElement(windowElement);
		XmlElement dataSourcesElement = FormXmlBuilder.DataSourcesElement(windowElement);
		CollapsibleContainerBuilder.Build(uiRootElement);
		XmlElement children = doc.CreateElement("UIChildren");
		uiRootElement.AppendChild(children);
		PropertyDescriptorCollection properties = GetProperties(item);
		Hashtable categories = new Hashtable();
		Hashtable categoryWidths = new Hashtable();
		foreach(PropertyDescriptor prop in properties)
		{
			XmlElement categoryChildren;
			if(categories.Contains(prop.Category))
			{
				categoryChildren = categories[prop.Category] as XmlElement;
			}
			else
			{
				XmlElement categoryElement = doc.CreateElement("UIElement");
				children.AppendChild(categoryElement);
				categoryChildren = doc.CreateElement("UIChildren");
				categoryElement.AppendChild(categoryChildren);
				UIElementRenderData renderData = new UIElementRenderData();
				renderData.Text = prop.Category;
				renderData.IndentLevel = 0;
				renderData.IsHeightFixed = false;
				renderData.IsOpen = true;
				CollapsiblePanelBuilder.Build(categoryElement, renderData);
				categories.Add(prop.Category, categoryChildren);
				categoryWidths.Add(prop.Category, 0);
				XmlElement panelElement = doc.CreateElement("UIElement");
				categoryChildren.AppendChild(panelElement);
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
		return TypeDescriptor.GetProperties(component, attributes);
	}
}
