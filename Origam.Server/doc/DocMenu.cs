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

using System;
using System.Collections.Generic;
using System.Xml;
using Origam.Schema;
using Origam.Workbench.Services;
using Origam.Schema.MenuModel;

namespace Origam.Server.Doc
{
    public class DocMenu : AbstractDoc
    {
        public DocMenu(XmlWriter writer)
            : base(writer)
        {
        }

        public override string FilterName
        {
            get { return "menu"; }
        }

        public override string Name
        {
            get { return "Menu"; }
        }

        public override bool IsTocRecursive
        {
            get
            {
                return true;
            }
        }

        public override bool IsFolder(object obj)
        {
            if (base.IsFolder(obj))
            {
                return true;
            }
            else
            {
                return obj is Menu || obj is Submenu;
            }
        }

        public override ISchemaItemProvider RootProvider()
        {
            SchemaService ss = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            return ss.GetProvider(typeof(MenuSchemaItemProvider));
        }

        public override string Title(string elementId, IDocumentationService documentation, IPersistenceService ps)
        {
            AbstractMenuItem menuItem = ps.SchemaProvider.RetrieveInstance(typeof(AbstractMenuItem), new ModelElementKey(new Guid(elementId))) as AbstractMenuItem;
            string name = menuItem.NodeText;
            AbstractSchemaItem parent = menuItem.ParentItem;
            while (parent != null)
            {
                name = parent.NodeText + " – " + name;
                parent = parent.ParentItem;
            }

            return name;
        }

        public override List<DiagramConnection> WriteContent(string bodyElement, string elementId, XmlWriter writer, IDocumentationService documentation, IPersistenceService ps)
        {
            if (!CheckElementId(writer, elementId)) return null;

            AbstractMenuItem menuItem = ps.SchemaProvider.RetrieveInstance(typeof(AbstractMenuItem), new ModelElementKey(new Guid(elementId))) as AbstractMenuItem;
            #region body
            DocTools.WriteStartBody(bodyElement, writer, this.Title(elementId, documentation, ps), "Menu Item", DocTools.ImagePath(menuItem), menuItem.Id);
            // summary
            WriteSummary(writer, documentation, menuItem);
            // Packages
            WritePackages(writer, menuItem);
            // Roles
            WriteRoles(writer, menuItem);
            // content
            WriteMenu(writer, documentation, ps, menuItem);
            // end body
            writer.WriteEndElement();
            #endregion
            return null;
        }

        private void WriteMenu(XmlWriter writer, IDocumentationService documentation,
            IPersistenceService ps, AbstractMenuItem menuItem)
        {
            FormReferenceMenuItem formItem = menuItem as FormReferenceMenuItem;
            WorkflowReferenceMenuItem wfItem = menuItem as WorkflowReferenceMenuItem;
            ReportReferenceMenuItem reportItem = menuItem as ReportReferenceMenuItem;

            if (formItem != null)
            {
                DocScreen doc = new DocScreen(writer);
                DocScreen.WriteDataSource(writer, formItem.Screen);
                doc.WriteScreen(writer, documentation, ps, formItem.Screen);
            }
            else if (wfItem != null)
            {
                DocTools.WriteSectionStart(writer, "Sequential Workflow");
                writer.WriteString("For a complete description see ");
                DocTools.WriteElementLink(writer, wfItem.Workflow, new DocSequentialWorkflow(null).FilterName);
                writer.WriteEndElement();
                DocSequentialWorkflow doc = new DocSequentialWorkflow(writer);
                doc.WriteUITasks(writer, documentation, wfItem.Workflow);
            }
        }
    }
}
