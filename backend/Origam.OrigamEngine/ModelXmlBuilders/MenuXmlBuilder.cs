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
        SchemaService schema = ServiceManager.Services.GetService(
            typeof(SchemaService)) as SchemaService;
        MenuSchemaItemProvider menuProvider = schema.GetProvider(
            typeof(MenuSchemaItemProvider)) as MenuSchemaItemProvider;
        XmlDocument doc = new XmlDocument();
        RenderNode(doc, doc, menuProvider.MainMenu);
        foreach (var item in menuProvider.ChildItems)
        {
            if (item is ContextMenu contextMenu)
            {
                RenderNode(doc, doc.FirstChild, contextMenu);
            }
        }
        return doc.OuterXml;
    }
    public static XmlDocument GetXml(Menu menu)
    {
        XmlDocument doc = new XmlDocument();
        RenderNode(doc, doc, menu);
        return doc;
    }
    private static void RenderNode(
        XmlDocument doc, XmlNode parentNode, ISchemaItem item)
    {
        AbstractMenuItem menuItem = item as AbstractMenuItem;
        bool process;
        if (menuItem == null)
        {
            process = true;
        }
        else
        {
            IParameterService param = ServiceManager.Services.GetService(
                typeof(IParameterService)) as IParameterService;
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
                process = param.IsFeatureOn(menuItem.Features);
            }
            if (process)
            {
                // check if user has access to this item, if not, we don't display it
                if (menuItem is IAuthorizationContextContainer authContainer)
                {
                    IOrigamAuthorizationProvider authorizationProvider 
                        = SecurityManager.GetAuthorizationProvider();
                    if (!authorizationProvider.Authorize(
                        SecurityManager.CurrentPrincipal,
                        authContainer.AuthorizationContext))
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
                    XmlElement el = doc.CreateElement(item.GetType().Name);
                    el.SetAttribute("label", menu.DisplayName);
                    el.SetAttribute("icon", "folder");
                    parentNode.AppendChild(el);
                    newNode = el;
                    break;
                }
            case ContextMenu _:
                {
                    XmlElement el = doc.CreateElement(typeof(Submenu).Name);
                    el.SetAttribute("label", item.Name);
                    el.SetAttribute("icon", "folder");
                    el.SetAttribute("isHidden", XmlConvert.ToString(true));
                    parentNode.AppendChild(el);
                    newNode = el;
                    break;
                }
            case Submenu submenu:
                {
                    XmlElement el = doc.CreateElement(item.GetType().Name);
                    el.SetAttribute("label", menuItem.DisplayName);
                    el.SetAttribute("icon", ResolveMenuIcon(
                        menuItem.GetType().Name, menuItem.MenuIcon));
                    if (submenu.IsHidden)
                    {
                        el.SetAttribute("isHidden", XmlConvert.ToString(true));
                    }
                    el.SetAttribute("id", item.Id.ToString());
                    parentNode.AppendChild(el);
                    newNode = el;
                    break;
                }
            case DynamicMenu dynamicMenu:
                {
                    string[] splittedClassPath = dynamicMenu.ClassPath.Split(',');
                    IDynamicMenuProvider provider = Reflector.InvokeObject(
                        splittedClassPath[0].Trim(), splittedClassPath[1].Trim())
                        as IDynamicMenuProvider;
                    provider.AddMenuItems(parentNode);
                    break;
                }
            case FormReferenceMenuItem formRef:
                {
                    XmlElement el = GetMenuItemElement(doc, menuItem);
                    if (formRef.SelectionDialogPanel != null)
                    {
                        el.SetAttribute("type",
                            el.GetAttribute("type") + "_WithSelection");
                        SetSelectionDialogSize(el, formRef.SelectionDialogPanel);
                    }
                    if (formRef.ListDataStructure != null)
                    {
                        el.SetAttribute("lazyLoading", "true");
                    }
                    parentNode.AppendChild(el);
                    break;
                }
            case DataConstantReferenceMenuItem _:
                {
                    XmlElement el = GetMenuItemElement(doc, menuItem);
                    parentNode.AppendChild(el);
                    break;
                }
            case WorkflowReferenceMenuItem _:
                {
                    XmlElement el = GetMenuItemElement(doc, menuItem);
                    parentNode.AppendChild(el);
                    break;
                }
            case ReportReferenceMenuItem reportRef:
                {
                    XmlElement el = GetMenuItemElement(doc, menuItem);
                    if (reportRef.SelectionDialogPanel != null)
                    {
                        el.SetAttribute("type",
                            el.GetAttribute("type") + "_WithSelection");
                        SetSelectionDialogSize(el, reportRef.SelectionDialogPanel);
                    }
                    if (reportRef.Report is WebReport wr)
                    {
                        el.SetAttribute("urlOpenMethod", wr.OpenMethod.ToString());
                    }
                    parentNode.AppendChild(el);
                    break;
                }
            case DashboardMenuItem _:
                {
                    XmlElement el = GetMenuItemElement(doc, menuItem);
                    el.SetAttribute("type", "Dashboard");
                    parentNode.AppendChild(el);
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException($"Unknown menu type {item.GetType()}");
        }
        if (newNode != null)
        {
            List<ISchemaItem> sortedList = item.ChildItems.ToList();
            sortedList.Sort(new AbstractMenuItem.MenuItemComparer());
            foreach (ISchemaItem child in sortedList)
            {
                RenderNode(doc, newNode, child);
            }
        }
    }
    private static XmlElement GetMenuItemElement(
        XmlDocument doc, AbstractMenuItem menu)
    {
        XmlElement el = doc.CreateElement("Command");
        el.SetAttribute("type", menu.GetType().Name);
        el.SetAttribute("id", menu.Id.ToString());
        el.SetAttribute("label", menu.DisplayName);
        el.SetAttribute("icon", 
            ResolveMenuIcon(menu.GetType().Name, menu.MenuIcon));
        el.SetAttribute("showInfoPanel", "false");
        el.SetAttribute("alwaysOpenNew", XmlConvert.ToString(menu.AlwaysOpenNew));
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
                return "menu_folder.png";
            case "FormReferenceMenuItem":
                return "menu_form.png";
            case "DataConstantReferenceMenuItem":
                return "menu_parameter.png";
            case "WorkflowReferenceMenuItem":
                return "menu_workflow.png";
            case "ReportReferenceMenuItem":
                return "menu_report.png";
            case "DashboardMenuItem":
                return "menu_dashboard.png";
            default:
                throw new ArgumentOutOfRangeException("type", type,
                    ResourceUtils.GetString("ErrorUnknownMenuType"));
        }
    }
    public static void GetSelectionDialogSize(
        PanelControlSet panel, out int width, out int height)
    {
        width = 0;
        height = 0;
        foreach (PropertyValueItem prop 
            in panel.ChildItems[0].ChildItemsByType(
            PropertyValueItem.CategoryConst))
        {
            switch (prop.ControlPropertyItem.Name)
            {
                case "Width":
                    width = prop.IntValue;
                    break;
                case "Height":
                    height = prop.IntValue;
                    break;
            }
        }
    }
    private static void SetSelectionDialogSize(
        XmlElement element, PanelControlSet panel)
    {
        int width = 0;
        int height = 0;
        GetSelectionDialogSize(panel, out width, out height);
        element.SetAttribute("dialogHeight", XmlConvert.ToString(height));
        element.SetAttribute("dialogWidth", XmlConvert.ToString(width));
    }
}
