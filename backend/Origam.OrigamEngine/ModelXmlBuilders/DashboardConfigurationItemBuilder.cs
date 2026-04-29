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
using Origam.Workbench.Services;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for DashboardConfigurationItemBuilder.
/// </summary>
public class DashboardConfigurationItemBuilder
{
    public static void Build(
        XmlDocument doc,
        XmlElement children,
        Guid menuId,
        DashboardConfigurationItem item,
        XmlElement dataSourcesElement
    )
    {
        XmlElement itemElement = doc.CreateElement(name: "UIElement");
        children.AppendChild(newChild: itemElement);
        // build a chrome around the dashboard widget
        UIElementRenderData renderData = new UIElementRenderData();
        renderData.Text = item.Label;
        renderData.TopCell = item.Top;
        renderData.LeftCell = item.Left;
        renderData.HeightCells = item.RowSpan;
        renderData.WidthCells = item.ColSpan;
        GridLayoutPanelItemBuilder.Build(parentNode: itemElement, renderData: renderData);
        XmlElement itemChildren = doc.CreateElement(name: "UIChildren");
        itemElement.AppendChild(newChild: itemChildren);
        BuildComponent(
            doc: doc,
            itemChildren: itemChildren,
            menuId: menuId,
            item: item,
            dataSourcesElement: dataSourcesElement
        );
    }

    private static void BuildComponent(
        XmlDocument doc,
        XmlElement itemChildren,
        Guid menuId,
        DashboardConfigurationItem item,
        XmlElement dataSourcesElement
    )
    {
        IPersistenceService ps =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        AbstractDashboardWidget widget =
            ps.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractDashboardWidget),
                primaryKey: new ModelElementKey(id: item.ComponentId)
            ) as AbstractDashboardWidget;
        PanelDashboardWidget panelWidget = widget as PanelDashboardWidget;
        AbstractSimpleDashboardWidget simpleWidget = widget as AbstractSimpleDashboardWidget;
        HorizontalContainerDashboardWidget hboxWidget =
            widget as HorizontalContainerDashboardWidget;
        VerticalContainerDashboardWidget vboxWidget = widget as VerticalContainerDashboardWidget;
        if (panelWidget != null)
        {
            DashboardPanelBuilder.Build(
                panelWidget: panelWidget,
                menuId: menuId,
                dashboardItemId: item.Id,
                itemChildren: itemChildren,
                dataSourcesElement: dataSourcesElement
            );
        }
        else if (simpleWidget != null)
        {
            DashboardSimpleWidgetBuilder.Build(
                doc: doc,
                simpleWidget: simpleWidget,
                dashboardItemId: item.Id,
                caption: item.Label,
                itemChildren: itemChildren,
                dataSourcesElement: dataSourcesElement
            );
        }
        else if (hboxWidget != null)
        {
            XmlElement element =
                itemChildren.AppendChild(newChild: doc.CreateElement(name: "UIElement"))
                as XmlElement;
            HBoxBuilder.Build(parentNode: element);
        }
        else if (vboxWidget != null)
        {
            XmlElement element =
                itemChildren.AppendChild(newChild: doc.CreateElement(name: "UIElement"))
                as XmlElement;
            VBoxBuilder.Build(parentNode: element);
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                paramName: "widget",
                actualValue: widget,
                message: "Unsupported widget type."
            );
        }
        XmlElement panelElement = itemChildren.FirstChild as XmlElement;
        bool hasBoundParameter = false;
        if (item.Parameters != null)
        {
            XmlNode parametersElement = panelElement.AppendChild(
                newChild: doc.CreateElement(name: "Parameters")
            );
            foreach (DashboardConfigurationItemParameter param in item.Parameters)
            {
                if (!param.IsBound)
                {
                    XmlElement parameterElement =
                        parametersElement.AppendChild(
                            newChild: doc.CreateElement(name: "Parameter")
                        ) as XmlElement;
                    parameterElement.SetAttribute(name: "Name", value: param.Name);
                    parameterElement.SetAttribute(name: "Value", value: param.Value);
                }
                else
                {
                    hasBoundParameter = true;
                }
            }
        }
        if (hasBoundParameter)
        {
            panelElement.SetAttribute(name: "IsRootGrid", value: "false");
        }
        if (item.Items != null && item.Items.Length > 0)
        {
            XmlElement childChildren = doc.CreateElement(name: "UIChildren");
            panelElement.AppendChild(newChild: childChildren);
            foreach (DashboardConfigurationItem child in item.Items)
            {
                BuildComponent(
                    doc: doc,
                    itemChildren: childChildren,
                    menuId: menuId,
                    item: child,
                    dataSourcesElement: dataSourcesElement
                );
            }
        }
    }
}
