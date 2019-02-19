#region license
/*
Copyright 2005 - 2017 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using Origam.Schema.MenuModel;
using System;
using System.Data;
using System.Xml;
using core = Origam.Workbench.Services.CoreServices;
using Origam.OrigamEngine;
using Origam.Workbench.Services;
using Origam;
using Origam.ServerCommon;

namespace Origam.Server
{
    class OrigamDesignerDynamicMenuProvider : IDynamicMenuProvider,
        IDynamicSessionStoreProvider
    {
        public void AddMenuItems(XmlNode parentNode)
        {
            var profile = SecurityManager.CurrentUserProfile();
            DataSet menuData = core.DataService.LoadData(new Guid("8e8474d7-1004-43fe-8306-723eab7f42eb"),
                new Guid("520609b5-a5b9-41bd-8fed-9f415947d189"), Guid.Empty, 
                new Guid("a485b168-f7d4-47de-aaee-cd0908a02a21"), null);
            foreach (DataRow submenuItem in menuData.Tables["OrigamApplication"].Rows)
            {
                bool isMyApp = profile.Id.Equals((Guid)submenuItem["RecordCreatedBy"]);
                XmlNode parentSubmenu = GetSubmenuElement(parentNode.OwnerDocument,
                    (Guid)submenuItem["Id"], (string)submenuItem["Name"]);
                parentNode.AppendChild(parentSubmenu);
                foreach (DataRow menuItem in submenuItem.GetChildRows("OrigamEntity"))
                {
                    parentSubmenu.AppendChild(GetMenuItemElement(parentSubmenu.OwnerDocument,
                    (Guid)menuItem["Id"], (string)menuItem["Label"]));
                }
                if (isMyApp)
                {
                    parentSubmenu.AppendChild(GetAddTableMenuItemElement(parentSubmenu.OwnerDocument,
                        (Guid)submenuItem["Id"], "Add table to " + (string)submenuItem["Name"]));
                }
            }
        }

        private static XmlElement GetMenuItemElement(XmlDocument doc, Guid id, string label)
        {
            XmlElement el = doc.CreateElement("Command");
            el.SetAttribute("type", typeof(FormReferenceMenuItem).Name);
            el.SetAttribute("id", "APP_" + id.ToString() + "|" + typeof(OrigamDesignerDynamicMenuProvider).AssemblyQualifiedName);
            el.SetAttribute("label", label);
            el.SetAttribute("icon", "menu_form.png");
            el.SetAttribute("showInfoPanel", "false");
            return el;
        }

        private static XmlElement GetAddTableMenuItemElement(XmlDocument doc, Guid appId, string label)
        {
            XmlElement el = doc.CreateElement("Command");
            el.SetAttribute("type", typeof(FormReferenceMenuItem).Name);
            el.SetAttribute("id", "EDT_" + appId.ToString() + "|" + typeof(OrigamDesignerDynamicMenuProvider).AssemblyQualifiedName);
            el.SetAttribute("label", label);
            el.SetAttribute("icon", "application_manage.png");
            el.SetAttribute("showInfoPanel", "false");
            return el;
        }

        private static XmlElement GetSubmenuElement(XmlDocument doc, Guid id, string label)
        {
            XmlElement el = doc.CreateElement(typeof(Submenu).Name);
            el.SetAttribute("label", label);
            el.SetAttribute("icon", "package.png");
            el.SetAttribute("id", id.ToString());
            return el;
        }

        public SessionStore GetSessionStore(IBasicUIService service, UIRequest request)
        {
            string[] idArray = request.ObjectId.Split("|".ToCharArray());
            string[] idArray2 = idArray[0].Split("_".ToCharArray());
            string actionType = idArray2[0];
            Guid id = new Guid(idArray2[1]);
            if (actionType == "APP")
            {
                return GetAppSessionStore(service, request, id);
            }
            else if (actionType == "EDT")
            {
                return GetAddTableSessionStore(service, request, id, "Edit app");
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private static SessionStore GetAppSessionStore(IBasicUIService service, UIRequest request, Guid entityId)
        {
            DataSet menuData = core.DataService.LoadData(new Guid("dbeac0e3-2696-4687-b7e6-c551c638f602"),
                new Guid("f25b6a5b-44d4-4cd2-ac70-2dc3ce73841e"), Guid.Empty, Guid.Empty, null, "OrigamEntity_parId", entityId);
            DataRow menuRow = menuData.Tables["OrigamEntity"].Rows[0];
            IPersistenceService persistence = ServiceManager.Services.GetService(
                typeof(IPersistenceService)) as IPersistenceService;
            FormReferenceMenuItem menu = new FormReferenceMenuItem();
            menu.PersistenceProvider = persistence.SchemaProvider;
            menu.DefaultSetId = new Guid("7a773bcc-cad5-4c3d-80ca-1600eed8ad60");
            menu.DisplayName = (string)menuRow["Label"];
            menu.ListDataStructureId = new Guid("e0e16683-9a1b-4fa2-b028-a0e08165df57");
            menu.ListEntityId = new Guid("7ae7685d-fb9f-416a-855d-cdbc068e82c3");
            menu.ListMethodId = new Guid("ef66d9f4-f656-41aa-bc8a-86da26882763");
            menu.MethodId = new Guid("b8a9306e-1dca-4bdd-bb7b-77b425de79b9");
            menu.AutoSaveOnListRecordChange = true;
            menu.AutoRefreshIntervalConstantId = new Guid("fb80e964-1fb5-42f6-8b1c-9f22d27072be");
            SessionStore result = new OrigamDesignerSessionStore(service, request, menu.DisplayName, 
                menu, entityId, Analytics.Instance);
            request.Parameters.Add("EntityId", entityId);
            request.Parameters.Add("OrigamRecord_parOrigamEntityId", entityId);
            return result;
        }

        private static SessionStore GetAddTableSessionStore(IBasicUIService service, UIRequest request,
            Guid appId, string appName)
        {
            request.ObjectId = "ecfc2869-4a73-4191-b81e-a3235004ab9f";
            request.Parameters.Add("applicationId", appId);
            return new WorkflowSessionStore(service, request, 
                new Guid("ecfc2869-4a73-4191-b81e-a3235004ab9f"), appName, Analytics.Instance);
        }
    }
}
