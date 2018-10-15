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
using Origam.Schema.WorkflowModel;
using Origam.Schema.WorkflowModel.WorkQueue;
using System.Collections;

namespace Origam.Server.Doc
{
    public class DocWorkQueueClass : AbstractDoc
    {
        public DocWorkQueueClass(XmlWriter writer)
            : base(writer)
        {
        }

        public override string FilterName
        {
            get { return "workQueueClass"; }
        }

        public override string Name
        {
            get { return "Work Queue Classes"; }
        }

        public override bool IsTocRecursive
        {
            get
            {
                return true;
            }
        }

        public override ISchemaItemProvider RootProvider()
        {
            SchemaService ss = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            return ss.GetProvider(typeof(WorkQueueClassSchemaItemProvider));
        }

        public override List<DiagramConnection> WriteContent(string bodyElement, string elementId, XmlWriter writer, IDocumentationService documentation, IPersistenceService ps)
        {
            if (!CheckElementId(writer, elementId)) return null;

            WorkQueueClass wqc = ps.SchemaProvider.RetrieveInstance(typeof(WorkQueueClass), new ModelElementKey(new Guid(elementId))) as WorkQueueClass;
            #region body
            DocTools.WriteStartBody(bodyElement, writer, this.Title(elementId, documentation, ps), "Work Queue Class", DocTools.ImagePath(wqc), wqc.Id);
            // summary
            WriteSummary(writer, documentation, wqc);
            // Packages
            WritePackages(writer, wqc);
            // content
            WriteWorkQueueClass(writer, documentation, ps, wqc);
            // end body
            writer.WriteEndElement();
            #endregion
            return null;
        }

        private void WriteWorkQueueClass(XmlWriter writer, IDocumentationService documentation,
            IPersistenceService ps, WorkQueueClass wqc)
        {
            DocTools.WriteSectionStart(writer, "Data");
            writer.WriteString("Data Structure ");
            DocTools.WriteElementLink(writer, wqc.WorkQueueStructure, new DocDataStructure(null).FilterName);
            writer.WriteEndElement();

            DocTools.WriteSectionStart(writer, "Queue Commands");
            ArrayList commands = wqc.ChildItemsByType(WorkQueueWorkflowCommand.ItemTypeConst);
            if (commands.Count > 0)
            {
                writer.WriteStartElement("table");
                writer.WriteStartElement("tr");
                writer.WriteElementString("th", "Name");
                writer.WriteElementString("th", "Description");
                writer.WriteEndElement();
                foreach (WorkQueueWorkflowCommand cmd in commands)
                {
                    writer.WriteStartElement("tr");
                    writer.WriteElementString("td", cmd.Name);
                    writer.WriteStartElement("td");
                    writer.WriteStartElement("p");
                    writer.WriteString("Executes ");
                    DocTools.WriteElementLink(writer, cmd.Workflow, new DocSequentialWorkflow(null).FilterName);
                    // end paragraph
                    writer.WriteEndElement();
                    writer.WriteElementString("p", "Parameter mappings:");
                    writer.WriteStartElement("ul");
                    foreach (WorkQueueWorkflowCommandParameterMapping mapping in cmd.ParameterMappings)
                    {
                        writer.WriteElementString("li", string.Format("{0} -> {1}",
                            mapping.Value.ToString(), mapping.Name));
                    }
                    // end parameter mappings
                    writer.WriteEndElement();
                    // end description
                    writer.WriteEndElement();
                    // end command
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
            else
            {
                writer.WriteElementString("p", "This work queue class has no specific commands defined. You can use data events to add and remove entries.");
            }
            writer.WriteEndElement();
        }
    }
}
