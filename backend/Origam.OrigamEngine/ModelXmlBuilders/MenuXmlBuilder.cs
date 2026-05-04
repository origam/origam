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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;

namespace Origam.OrigamEngine.ModelXmlBuilders;

/// <summary>
/// Summary description for MenuXmlBuilder.
/// </summary>
public class MenuXmlBuilder
{
    public static string GetMenu()
    {
        SchemaService schema =
            ServiceManager.Services.GetService(serviceType: typeof(SchemaService)) as SchemaService;
        MenuSchemaItemProvider menuProvider =
            schema.GetProvider(type: typeof(MenuSchemaItemProvider)) as MenuSchemaItemProvider;
        XmlDocument doc = new XmlDocument();
        RenderNode(doc: doc, parentNode: doc, item: menuProvider.MainMenu);
        foreach (var item in menuProvider.ChildItems)
        {
            if (item is ContextMenu contextMenu)
            {
                RenderNode(doc: doc, parentNode: doc.FirstChild, item: contextMenu);
            }
        }
        return doc.OuterXml;
    }

    public static XmlDocument GetXml(Menu menu)
    {
        XmlDocument doc = new XmlDocument();
        RenderNode(doc: doc, parentNode: doc, item: menu);
        return doc;
    }

    private static void RenderNode(XmlDocument doc, XmlNode parentNode, ISchemaItem item)
    {
        AbstractMenuItem menuItem = item as AbstractMenuItem;
        bool process;
        if (menuItem == null)
        {
            process = true;
        }
        else
        {
            IParameterService param =
                ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
                as IParameterService;
            if (menuItem.Features == "FLASH")
            {
                process = true;
            }
            else if (menuItem.Features == "!FLASH")
            {
                process = false;
            }
            else
            {
                process = param.IsFeatureOn(featureCode: menuItem.Features);
            }
            if (process)
            {
                // check if user has access to this item, if not, we don't display it
                if (menuItem is IAuthorizationContextContainer authContainer)
                {
                    IOrigamAuthorizationProvider authorizationProvider =
                        SecurityManager.GetAuthorizationProvider();
                    if (
                        !authorizationProvider.Authorize(
                            principal: SecurityManager.CurrentPrincipal,
                            context: authContainer.AuthorizationContext
                        )
                    )
                    {
                        process = false;
                    }
                }
            }
        }
        if (!process)
        {
            return;
        }
        XmlNode newNode = null;
        switch (item)
        {
            case Menu menu:
            {
                XmlElement el = doc.CreateElement(name: item.GetType().Name);
                el.SetAttribute(name: "label", value: menu.DisplayName);
                el.SetAttribute(name: "icon", value: "folder");
                parentNode.AppendChild(newChild: el);
                newNode = el;
                break;
            }
            case ContextMenu _:
            {
                XmlElement el = doc.CreateElement(name: typeof(Submenu).Name);
                el.SetAttribute(name: "label", value: item.Name);
                el.SetAttribute(name: "icon", value: "folder");
                el.SetAttribute(name: "isHidden", value: XmlConvert.ToString(value: true));
                parentNode.AppendChild(newChild: el);
                newNode = el;
                break;
            }
            case Submenu submenu:
            {
                XmlElement el = doc.CreateElement(name: item.GetType().Name);
                el.SetAttribute(name: "label", value: menuItem.DisplayName);
                el.SetAttribute(
                    name: "icon",
                    value: ResolveMenuIcon(
                        type: menuItem.GetType().Name,
                        menuIcon: menuItem.MenuIcon
                    )
                );
                if (submenu.IsHidden)
                {
                    el.SetAttribute(name: "isHidden", value: XmlConvert.ToString(value: true));
                }
                el.SetAttribute(name: "id", value: item.Id.ToString());
                parentNode.AppendChild(newChild: el);
                newNode = el;
                break;
            }
            case DynamicMenu dynamicMenu:
            {
                string[] splittedClassPath = dynamicMenu.ClassPath.Split(separator: ',');
                IDynamicMenuProvider provider =
                    Reflector.InvokeObject(
                        classname: splittedClassPath[0].Trim(),
                        assembly: splittedClassPath[1].Trim()
                    ) as IDynamicMenuProvider;
                provider.AddMenuItems(parentNode: parentNode);
                break;
            }
            case FormReferenceMenuItem formRef:
            {
                XmlElement el = GetMenuItemElement(doc: doc, menu: menuItem);
                if (formRef.SelectionDialogPanel != null)
                {
                    el.SetAttribute(
                        name: "type",
                        value: el.GetAttribute(name: "type") + "_WithSelection"
                    );
                    SetSelectionDialogSize(element: el, panel: formRef.SelectionDialogPanel);
                }
                if (formRef.ListDataStructure != null)
                {
                    el.SetAttribute(name: "lazyLoading", value: "true");
                }
                parentNode.AppendChild(newChild: el);
                break;
            }
            case DataConstantReferenceMenuItem _:
            {
                XmlElement el = GetMenuItemElement(doc: doc, menu: menuItem);
                parentNode.AppendChild(newChild: el);
                break;
            }
            case WorkflowReferenceMenuItem _:
            {
                XmlElement el = GetMenuItemElement(doc: doc, menu: menuItem);
                parentNode.AppendChild(newChild: el);
                break;
            }
            case ReportReferenceMenuItem reportRef:
            {
                XmlElement el = GetMenuItemElement(doc: doc, menu: menuItem);
                if (reportRef.SelectionDialogPanel != null)
                {
                    el.SetAttribute(
                        name: "type",
                        value: el.GetAttribute(name: "type") + "_WithSelection"
                    );
                    SetSelectionDialogSize(element: el, panel: reportRef.SelectionDialogPanel);
                }
                if (reportRef.Report is WebReport wr)
                {
                    el.SetAttribute(name: "urlOpenMethod", value: wr.OpenMethod.ToString());
                }
                parentNode.AppendChild(newChild: el);
                break;
            }
            case DashboardMenuItem _:
            {
                XmlElement el = GetMenuItemElement(doc: doc, menu: menuItem);
                el.SetAttribute(name: "type", value: "Dashboard");
                parentNode.AppendChild(newChild: el);
                break;
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: $"Unknown menu type {item.GetType()}"
                );
            }
        }
        if (newNode != null)
        {
            List<ISchemaItem> sortedList = item.ChildItems.ToList();
            sortedList.Sort(comparer: new AbstractMenuItem.MenuItemComparer());
            foreach (ISchemaItem child in sortedList)
            {
                RenderNode(doc: doc, parentNode: newNode, item: child);
            }
        }
    }

    private static XmlElement GetMenuItemElement(XmlDocument doc, AbstractMenuItem menu)
    {
        XmlElement el = doc.CreateElement(name: "Command");
        el.SetAttribute(name: "type", value: menu.GetType().Name);
        el.SetAttribute(name: "id", value: menu.Id.ToString());
        el.SetAttribute(name: "label", value: menu.DisplayName);
        el.SetAttribute(
            name: "icon",
            value: ResolveMenuIcon(type: menu.GetType().Name, menuIcon: menu.MenuIcon)
        );
        el.SetAttribute(name: "showInfoPanel", value: "false");
        el.SetAttribute(
            name: "alwaysOpenNew",
            value: XmlConvert.ToString(value: menu.AlwaysOpenNew)
        );
        return el;
    }

    public static string ResolveMenuIcon(string type, Graphics menuIcon)
    {
        if (menuIcon != null)
        {
            return menuIcon.Name;
        }
        switch (type)
        {
            case "Submenu":
            {
                return "menu_folder.png";
            }
            case "FormReferenceMenuItem":
            {
                return "menu_form.png";
            }
            case "DataConstantReferenceMenuItem":
            {
                return "menu_parameter.png";
            }
            case "WorkflowReferenceMenuItem":
            {
                return "menu_workflow.png";
            }
            case "ReportReferenceMenuItem":
            {
                return "menu_report.png";
            }
            case "DashboardMenuItem":
            {
                return "menu_dashboard.png";
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "type",
                    actualValue: type,
                    message: ResourceUtils.GetString(key: "ErrorUnknownMenuType")
                );
            }
        }
    }

    public static void GetSelectionDialogSize(PanelControlSet panel, out int width, out int height)
    {
        width = 0;
        height = 0;
        foreach (
            var prop in panel
                .ChildItems[index: 0]
                .ChildItemsByType<PropertyValueItem>(itemType: PropertyValueItem.CategoryConst)
        )
        {
            switch (prop.ControlPropertyItem.Name)
            {
                case "Width":
                {
                    width = prop.IntValue;
                    break;
                }

                case "Height":
                {
                    height = prop.IntValue;
                    break;
                }
            }
        }
    }

    private static void SetSelectionDialogSize(XmlElement element, PanelControlSet panel)
    {
        int width = 0;
        int height = 0;
        GetSelectionDialogSize(panel: panel, width: out width, height: out height);
        element.SetAttribute(name: "dialogHeight", value: XmlConvert.ToString(value: height));
        element.SetAttribute(name: "dialogWidth", value: XmlConvert.ToString(value: width));
    }
}
