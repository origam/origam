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
using Origam.Schema.GuiModel;
using System.Data;
using Origam.DA.Service;
using System.Collections;
using Origam.Gui.Win;
using Origam.Schema.EntityModel;

namespace Origam.Server.Doc
{
    public class DocScreen : AbstractDoc
    {
        public DocScreen(XmlWriter writer)
            : base(writer)
        {
        }

        public override string FilterName
        {
            get { return "screen"; }
        }

        public override string Name
        {
            get { return "Screens"; }
        }

        public override bool CanExtendItems
        {
            get
            {
                return false;
            }
        }

        public override ISchemaItemProvider RootProvider()
        {
            SchemaService ss = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            return ss.GetProvider(typeof(FormSchemaItemProvider));
        }

        public override List<DiagramConnection> WriteContent(string bodyElement, string elementId, XmlWriter writer, IDocumentationService documentation, IPersistenceService ps)
        {
            if (!CheckElementId(writer, elementId)) return null;

            FormControlSet form = ps.SchemaProvider.RetrieveInstance(typeof(FormControlSet), new ModelElementKey(new Guid(elementId))) as FormControlSet;
            #region body
            DocTools.WriteStartBody(bodyElement, writer, this.Title(elementId, documentation, ps), "Screen", DocTools.ImagePath(form), form.Id);
            // summary
            WriteSummary(writer, documentation, form);
            // Packages
            WritePackages(writer, form);
            // Data Source
            WriteDataSource(writer, form);
            // Screen sections
            WriteScreen(writer, documentation, ps, form);
            // end body
            writer.WriteEndElement();
            #endregion
            return null;
        }

        public static void WriteDataSource(XmlWriter writer, FormControlSet form)
        {
            DocTools.WriteSectionStart(writer, "Data Source");
            writer.WriteString("Data Structure ");
            DocTools.WriteElementLink(writer, form.DataStructure, new DocDataStructure(null).FilterName);
            writer.WriteEndElement();
        }

        public void WriteScreen(XmlWriter writer, IDocumentationService documentation, IPersistenceService ps, FormControlSet form)
        {
            DataSet dataset = new DatasetGenerator(false).CreateDataSet(form.DataStructure);
            try
            {
                LoadControlDescription(writer, documentation, ps, form.ChildItems[0] as ControlSetItem, null, dataset);
            }
            catch (Exception ex)
            {
                DocTools.WriteError(writer, ex);
            }
        }

        private void LoadControlDescription(XmlWriter writer, IDocumentationService documentation, IPersistenceService ps,
            ControlSetItem control, string dataMember, DataSet dataset)
        {
            string caption = "";
            string gridCaption = "";
            string bindingMember = "";
            string panelTitle = "";
            int tabIndex = 0;

            foreach (PropertyValueItem property in control.ChildItemsByType(PropertyValueItem.ItemTypeConst))
            {
                if (property.ControlPropertyItem.Name == "TabIndex")
                {
                    tabIndex = property.IntValue;
                }

                if (property.ControlPropertyItem.Name == "Caption")
                {
                    caption = property.Value;
                }

                if (property.ControlPropertyItem.Name == "GridColumnCaption")
                {
                    gridCaption = property.Value;
                }

                if (property.ControlPropertyItem.Name == "PanelTitle")
                {
                    panelTitle = property.Value;
                }

                if (control.ControlItem.IsComplexType && property.ControlPropertyItem.Name == "DataMember")
                {
                    dataMember = property.Value;
                }
            }

            caption = (gridCaption == "" | gridCaption == null) ? caption : gridCaption;

            foreach (PropertyBindingInfo bindItem in control.ChildItemsByType(PropertyBindingInfo.ItemTypeConst))
            {
                bindingMember = bindItem.Value;
            }

            if (bindingMember != "")
            {
                DataTable table = dataset.Tables[FormGenerator.FindTableByDataMember(dataset, dataMember)];

                if (!table.Columns.Contains(bindingMember)) throw new Exception("Field '" + bindingMember + "' not found in a data structure for the form '" + control.RootItem.Path + "'");

                if (string.IsNullOrEmpty(caption))
                {
                    caption = table.Columns[bindingMember].Caption;
                }
                Guid id = (Guid)table.Columns[bindingMember].ExtendedProperties["Id"];

                DocTools.WriteDivStart(writer, "screenField");
                    DocTools.WriteDivStart(writer, "title");
                        writer.WriteString(caption);
                    writer.WriteEndElement();
                    string doc = documentation.GetDocumentation(id, DocumentationType.USER_LONG_HELP);
                        DocTools.WriteDivStart(writer, "description");
                        writer.WriteString(doc);
                        writer.WriteEndElement();
                writer.WriteEndElement();
            }

            ArrayList sortedControls;

            if (control.ControlItem.IsComplexType)
            {
                if (panelTitle != "")
                {
                    writer.WriteElementString(DocTools.SECTION_HEADING, "Section " + panelTitle);
                }

                AbstractDataEntity entity = GetEntity(ps, dataMember, dataset);
                writer.WriteStartElement("p");
                writer.WriteString("Entity ");
                DocTools.WriteElementLink(writer, entity, new DocEntity(null).FilterName);
                writer.WriteEndElement();

                sortedControls = control.ControlItem.PanelControlSet.ChildItems[0].ChildItemsByType(ControlSetItem.ItemTypeConst);

            }
            else
            {
                sortedControls = control.ChildItemsByType(ControlSetItem.ItemTypeConst);
            }

            sortedControls.Sort(new ControlSetItemComparer());

            foreach (ControlSetItem subControl in sortedControls)
            {
                LoadControlDescription(writer, documentation, ps, subControl, dataMember, dataset);
            }
        }

        private static AbstractDataEntity GetEntity(IPersistenceService ps, string dataMember, DataSet dataset)
        {
            DataTable table = dataset.Tables[FormGenerator.FindTableByDataMember(dataset, dataMember)];
            Guid entityId = (Guid)table.ExtendedProperties["EntityId"];
            AbstractDataEntity entity = ps.SchemaProvider.RetrieveInstance(typeof(AbstractDataEntity), new ModelElementKey(entityId)) as AbstractDataEntity;
            return entity;
        }
    }
}
