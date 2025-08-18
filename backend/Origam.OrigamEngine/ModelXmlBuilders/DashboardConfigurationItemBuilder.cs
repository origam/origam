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

using Origam.Workbench.Services;
using Origam.Schema;
using Origam.Schema.GuiModel;

namespace Origam.OrigamEngine.ModelXmlBuilders;
/// <summary>
/// Summary description for DashboardConfigurationItemBuilder.
/// </summary>
public class DashboardConfigurationItemBuilder
{
	public static void Build(XmlDocument doc, XmlElement children, Guid menuId, DashboardConfigurationItem item, XmlElement dataSourcesElement)
	{
		XmlElement itemElement = doc.CreateElement("UIElement");
		children.AppendChild(itemElement);
		// build a chrome around the dashboard widget
		UIElementRenderData renderData = new UIElementRenderData();
		renderData.Text = item.Label;
		renderData.TopCell = item.Top;
		renderData.LeftCell = item.Left;
		renderData.HeightCells = item.RowSpan;
		renderData.WidthCells = item.ColSpan;
		GridLayoutPanelItemBuilder.Build(itemElement, renderData);
		XmlElement itemChildren = doc.CreateElement("UIChildren");
		itemElement.AppendChild(itemChildren);
		BuildComponent(doc, itemChildren, menuId, item, dataSourcesElement);
	}
	private static void BuildComponent(XmlDocument doc, XmlElement itemChildren, Guid menuId, DashboardConfigurationItem item, XmlElement dataSourcesElement)
	{
		IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		AbstractDashboardWidget widget = ps.SchemaProvider.RetrieveInstance(typeof(AbstractDashboardWidget), new ModelElementKey(item.ComponentId)) as AbstractDashboardWidget;
		PanelDashboardWidget panelWidget = widget as PanelDashboardWidget;
		AbstractSimpleDashboardWidget simpleWidget = widget as AbstractSimpleDashboardWidget;
		HorizontalContainerDashboardWidget hboxWidget = widget as HorizontalContainerDashboardWidget;
		VerticalContainerDashboardWidget vboxWidget = widget as VerticalContainerDashboardWidget;
		if(panelWidget != null)
		{
			DashboardPanelBuilder.Build(panelWidget, menuId, item.Id, itemChildren, dataSourcesElement);
		}
		else if(simpleWidget != null)
		{
			DashboardSimpleWidgetBuilder.Build(doc, simpleWidget, item.Id, item.Label, itemChildren, dataSourcesElement);
		}
		else if(hboxWidget != null)
		{
			XmlElement element = itemChildren.AppendChild(doc.CreateElement("UIElement")) as XmlElement;
			HBoxBuilder.Build(element);
		}
		else if(vboxWidget != null)
		{
			XmlElement element = itemChildren.AppendChild(doc.CreateElement("UIElement")) as XmlElement;
			VBoxBuilder.Build(element);
		}
		else
		{
			throw new ArgumentOutOfRangeException("widget", widget, "Unsupported widget type.");
		}
		XmlElement panelElement = itemChildren.FirstChild as XmlElement;
		bool hasBoundParameter = false;
		if(item.Parameters != null)
		{
			XmlNode parametersElement = panelElement.AppendChild(doc.CreateElement("Parameters"));
			foreach(DashboardConfigurationItemParameter param in item.Parameters)
			{
				if(! param.IsBound)
				{
					XmlElement parameterElement = parametersElement.AppendChild(doc.CreateElement("Parameter")) as XmlElement;
					parameterElement.SetAttribute("Name", param.Name);
					parameterElement.SetAttribute("Value", param.Value);
				}
				else
				{
					hasBoundParameter = true;
				}
			}
		}
		if(hasBoundParameter)
		{
			panelElement.SetAttribute("IsRootGrid", "false");
		}
		if(item.Items != null && item.Items.Length > 0)
		{
			XmlElement childChildren = doc.CreateElement("UIChildren");
			panelElement.AppendChild(childChildren);
			foreach(DashboardConfigurationItem child in item.Items)
			{
				BuildComponent(doc, childChildren, menuId, child, dataSourcesElement);
			}
		}
	}
}
