#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.Xml;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;

namespace Origam.OrigamEngine.ModelXmlBuilders
{
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
            if (menuProvider.ChildItems.Count > 0)
            {
                return GetXml(menuProvider.ChildItems[0] as Menu).OuterXml;
            }
            return null;
        }

        public static XmlDocument GetXml(Menu menu)
        {
            XmlDocument doc = new XmlDocument();
            RenderNode(doc, doc, menu);
            return doc;
        }

        private static void RenderNode(
            XmlDocument doc, XmlNode parentNode, AbstractSchemaItem item)
        {
            AbstractMenuItem menu = item as AbstractMenuItem;
            DynamicMenu dynamicMenu = item as DynamicMenu;
            bool process = false;
            if (menu == null)
            {
                process = true;
            }
            else
            {
                IParameterService param = ServiceManager.Services.GetService(
                    typeof(IParameterService)) as IParameterService;
                if (menu.Features == "FLASH")
                {
                    process = true;
                }
                else if (menu.Features == "!FLASH")
                {
                    process = false;
                }
                else
                {
                    process = param.IsFeatureOn(menu.Features);
                }
                if (process)
                {
                    // check if user has access to this item, if not, we don't display it
                    if (menu is IAuthorizationContextContainer)
                    {
                        IOrigamAuthorizationProvider authorizationProvider 
                            = SecurityManager.GetAuthorizationProvider();
                        if (!authorizationProvider.Authorize(
                            SecurityManager.CurrentPrincipal, 
                            (menu as IAuthorizationContextContainer)
                            .AuthorizationContext))
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
            if (item is Menu)
            {
                XmlElement el = doc.CreateElement(item.GetType().Name);
                el.SetAttribute("label", (item as Menu).DisplayName);
                el.SetAttribute("icon", "folder");
                parentNode.AppendChild(el);
                newNode = el;
            }
            else if (item is Submenu)
            {
                XmlElement el = doc.CreateElement(item.GetType().Name);
                el.SetAttribute("label", menu.DisplayName);
                el.SetAttribute("icon", ResolveMenuIcon(
                    menu.GetType().Name, menu.MenuIcon));
                if ((item as Submenu).IsHidden)
                {
                    el.SetAttribute("isHidden", XmlConvert.ToString(true));
                }
                el.SetAttribute("id", item.Id.ToString());
                parentNode.AppendChild(el);
                newNode = el;
            }
            else if (dynamicMenu != null)
            {
                string[] splittedClassPath = dynamicMenu.ClassPath.Split(',');
                IDynamicMenuProvider provider = Reflector.InvokeObject(
                    splittedClassPath[0].Trim(), splittedClassPath[1].Trim())
                    as IDynamicMenuProvider;
                provider.AddMenuItems(parentNode);
            }
            else if (item is FormReferenceMenuItem)
            {
                FormReferenceMenuItem formRef = item as FormReferenceMenuItem;
                XmlElement el = GetMenuItemElement(doc, menu);
                if (formRef.SelectionDialogPanel != null)
                {
                    el.SetAttribute("type", 
                        el.GetAttribute("type") + "_WithSelection");
                    SetSelectionDialogSize(el, formRef.SelectionDialogPanel);
                }
                parentNode.AppendChild(el);
            }
            else if (item is DataConstantReferenceMenuItem)
            {
                XmlElement el = GetMenuItemElement(doc, menu);
                parentNode.AppendChild(el);
            }
            else if (item is WorkflowReferenceMenuItem)
            {
                XmlElement el = GetMenuItemElement(doc, menu);
                parentNode.AppendChild(el);
            }
            else if (item is ReportReferenceMenuItem)
            {
                ReportReferenceMenuItem rr = item as ReportReferenceMenuItem;
                XmlElement el = GetMenuItemElement(doc, menu);
                if (rr.SelectionDialogPanel != null)
                {
                    el.SetAttribute("type", 
                        el.GetAttribute("type") + "_WithSelection");
                    SetSelectionDialogSize(el, rr.SelectionDialogPanel);
                }
                WebReport wr = rr.Report as WebReport;
                if (wr != null)
                {
                    el.SetAttribute("urlOpenMethod", wr.OpenMethod.ToString());
                }
                parentNode.AppendChild(el);
            }
            else if (item is DashboardMenuItem)
            {
                XmlElement el = GetMenuItemElement(doc, menu);
                el.SetAttribute("type", "Dashboard");
                parentNode.AppendChild(el);
            }
            if (newNode != null)
            {
                ArrayList sortedList = new ArrayList(item.ChildItems);
                sortedList.Sort(new Origam.Schema.MenuModel
                    .AbstractMenuItem.MenuItemComparer());
                foreach (AbstractSchemaItem child in sortedList)
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
                PropertyValueItem.ItemTypeConst))
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
}
