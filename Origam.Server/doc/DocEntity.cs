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
using System.Text;
using System.Xml;
using Origam.Schema;
using Origam.Workbench.Services;
using Origam.Schema.EntityModel;
using System.Collections;
using Origam.DA.Service;

namespace Origam.Server.Doc
{
    public class DocEntity : AbstractDoc
    {
        public DocEntity(XmlWriter writer)
            : base(writer)
        {
        }

        public override string FilterName
        {
            get { return "entity"; }
        }

        public override string Name
        {
            get { return "Entities"; }
        }

        public override bool UseDiagrams
        {
            get
            {
                return true;
            }
        }

        public override ISchemaItemProvider RootProvider()
        {
            SchemaService ss = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
            return ss.GetProvider(typeof(EntityModelSchemaItemProvider));
        }

        public override List<DiagramConnection> WriteContent(string bodyElement, string elementId, XmlWriter writer, IDocumentationService documentation, IPersistenceService ps)
        {
            if (!CheckElementId(writer, elementId)) return null;

            AbstractDataEntity entity = ps.SchemaProvider.RetrieveInstance(typeof(AbstractDataEntity), new ModelElementKey(new Guid(elementId))) as AbstractDataEntity;
            List<DiagramConnection> connections = new List<DiagramConnection>();
            List<DiagramElement> elements = new List<DiagramElement>();
            DiagramElements(entity, elements, connections);
            #region body
            DocTools.WriteStartBody(bodyElement, writer, this.Title(elementId, documentation, ps), "Entity", DocTools.ImagePath(entity), entity.Id);
            // summary
            WriteSummary(writer, documentation, entity);
            // Ancestors
            WriteAncestors(writer, entity);
            // Packages
            WritePackages(writer, entity);
            // Diagram
            WriteDiagram(writer, elements);
            // Fields
            WriteFields(writer, documentation, ps, entity);
            // end body
            writer.WriteEndElement();
            #endregion
            // end document
            return connections;
        }

        private void DiagramElements(IDataEntity entity, List<DiagramElement> elements, List<DiagramConnection> connections)
        {
            string diagramPrefix = Guid.NewGuid().ToString();
            string entityId = diagramPrefix + entity.PrimaryKey["Id"].ToString();
            int lastTop = 30;
            const int DIAGRAM_FK_LEFT = 400;
            const int DIAGRAM_CHILD_LEFT = 300;

            ArrayList fields = entity.EntityColumns;
            fields.Sort();
            StringBuilder fieldNames = new StringBuilder();
            fieldNames.AppendFormat("<div class=\"title\">{0}</div>", entity.Name);
            fieldNames.Append("<ul>");
            int count = 1;
            foreach (IDataEntityColumn field in fields)
            {
                IDataEntity foreignEntity = field.ForeignKeyEntity;
                if (field.ForeignKeyEntity != null)
                {
                    count++;
                }
            }

            int i = 2;
            decimal offset = (decimal)1 / count / 2;

            foreach (IDataEntityColumn field in fields)
            {
                IDataEntity foreignEntity = field.ForeignKeyEntity;
                if (foreignEntity != null)
                {
                    DiagramConnectionAnchorType sourceAnchor = DiagramConnectionAnchorType.Custom;
                    string foreignEntityId = diagramPrefix + foreignEntity.PrimaryKey["Id"].ToString();
                    // for self-join we will not add the entity again
                    if (foreignEntity.PrimaryKey.Equals(entity.PrimaryKey))
                    {
                        sourceAnchor = DiagramConnectionAnchorType.Bottom;
                    }
                    else
                    {
                        bool found = false;
                        foreach (DiagramElement el in elements)
                        {
                            // we compare the 2nd part of the guid (1st part is random, 2nd part is the entity id)
                            if (el.Id.Equals(foreignEntityId))
                            {
                                found = true;
                                break;
                            }
                        }
                        if (!found)
                        {
                            elements.Add(new DiagramElement(foreignEntityId, foreignEntity.Name, foreignEntity, this.FilterName, lastTop, DIAGRAM_FK_LEFT, "entity"));
                            lastTop += 50;
                        }
                    }
                    DiagramConnectionPosition sourceAnchorPosition = new DiagramConnectionPosition(1, ((decimal)i / count) - offset);
                    connections.Add(new DiagramConnection(
                        diagramPrefix + field.PrimaryKey["Id"].ToString(), 
                        entityId, 
                        foreignEntityId,
                        "",
                        sourceAnchor,
                        DiagramConnectionAnchorType.Left,
                        sourceAnchorPosition,
                        null, 0));
                    fieldNames.AppendFormat("<li>{0}</li>", field.Name);
                    i++;
                }
            }

            fieldNames.Append("</ul>");

            if (lastTop > 0) lastTop -= 50;

            // add the entity
            elements.Add(new DiagramElement(entityId, fieldNames.ToString(), null, null, 30 + (lastTop / 2) - (count * 10), 0, "entity"));

            lastTop += 60;

            foreach (IAssociation relation in entity.EntityRelations)
            {
                if (relation.IsParentChild && ! relation.AssociatedEntity.PrimaryKey.Equals(entity.PrimaryKey))
                {
                    StringBuilder keys = new StringBuilder();
                    keys.Append(DocTools.ImageLink(relation.AssociatedEntity, this.FilterName));
                    keys.Append("<ul>");
                    foreach (EntityRelationColumnPairItem key in relation.ChildItemsByType(EntityRelationColumnPairItem.ItemTypeConst))
                    {
                        keys.AppendFormat("<li>{0}</li>", key.RelatedEntityField.Name);
                    }
                    keys.Append("</ul>");

                    string relationId = diagramPrefix + relation.PrimaryKey["Id"].ToString();
                    elements.Add(new DiagramElement(relationId, keys.ToString(), null, null, lastTop, DIAGRAM_CHILD_LEFT, "entity"));
                    connections.Add(new DiagramConnection(
                        relationId,
                        relationId,
                        entityId,
                        null,
                        DiagramConnectionAnchorType.Left,
                        DiagramConnectionAnchorType.Bottom,
                        null, null, 0));
                    lastTop += 50;
                }
            }
        }

        private void WriteFields(XmlWriter writer, IDocumentationService documentation, 
            IPersistenceService ps, IDataEntity entity)
        {
            List<string> columnNames = new List<string>();
            columnNames.Add("");
            columnNames.Add("Title");
            columnNames.Add("Name/Description");
            columnNames.Add("Data Type");
            columnNames.Add("Mandatory");

            DocTools.WriteSectionStart(writer, "Fields");
            writer.WriteStartElement("table");
            DocTools.WriteTableHeader(writer, columnNames);
            ArrayList fields = entity.EntityColumns;
            fields.Sort();
            foreach (IDataEntityColumn field in fields)
            {
                #region tr
                writer.WriteStartElement("tr");
                #region td icon
                writer.WriteStartElement("td");
                writer.WriteAttributeString("class", "icon");
                DocTools.WriteImage(writer, DocTools.ImagePath(field));
                writer.WriteEndElement();
                #endregion
                #region td title
                writer.WriteStartElement("td");
                writer.WriteAttributeString("class", "title");
                writer.WriteString(field.Caption);
                writer.WriteEndElement();
                #endregion
                #region td fieldName
                writer.WriteStartElement("td");
                // name
                DocTools.WriteDivString(writer, "fieldName", field.Name);
                // long description p
                WriteColumnDescription(writer, documentation, ps, field, entity);
                // documentation td
                writer.WriteEndElement();
                #endregion
                #region td dataType
                writer.WriteStartElement("td");
                writer.WriteAttributeString("class", "dataType");
                WriteDataType(writer, field);
                writer.WriteEndElement();
                #endregion
                #region td mandatoryField
                writer.WriteStartElement("td");
                writer.WriteAttributeString("class", "mandatoryField");
                if (!field.AllowNulls)
                {
                    writer.WriteString("x");
                }
                writer.WriteEndElement();
                #endregion
                // tr
                writer.WriteEndElement();
                #endregion
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        private void WriteDataType(XmlWriter writer, IDataEntityColumn field)
        {
            if (field.ForeignKeyEntity != null)
            {
                DocTools.WriteElementLink(writer, field.ForeignKeyEntity, this.FilterName);
                writer.WriteElementString("br", "");
            }
            writer.WriteString(field.DataType.ToString());
            if (field.DataType == OrigamDataType.String)
            {
                writer.WriteString(" (" + field.DataLength.ToString() + ")");
            }
        }

        private void WriteColumnDescription(XmlWriter writer, IDocumentationService documentation, 
            IPersistenceService ps, IDataEntityColumn field, IDataEntity entity)
        {
            string description = DocTools.LongHelp(documentation, (Guid)field.PrimaryKey["Id"]);
            writer.WriteStartElement("p");
            writer.WriteString(description);
            if (! string.IsNullOrEmpty(description) && !description.EndsWith("."))
            {
                writer.WriteString(".");
            }
            if (field.DerivedFrom != null)
            {
                writer.WriteString(" (Inherited from ");
                DocTools.WriteElementLink(writer, field.DerivedFrom, this.FilterName);
                writer.WriteString(".)");
            }
            else if (!field.SchemaExtension.PrimaryKey.Equals(entity.SchemaExtension.PrimaryKey))
            {
                writer.WriteString(" (Extended in package ");
                writer.WriteString(field.SchemaExtension.Name);
                writer.WriteString(".)");
            }
            // long description p
            writer.WriteEndElement();

            if (field is FunctionCall)
            {
                DatasetGenerator dg = new DatasetGenerator(true);
                DataStructure ds = new DataStructure();
                ds.PersistenceProvider = ps.SchemaProvider;
                DataStructureEntity de = (DataStructureEntity)ds.NewItem(typeof(DataStructureEntity), Guid.Empty, null);
                de.Entity = (AbstractSchemaItem)entity;
                try
                {
                    string code = dg.RenderExpression(field, de);
                    DocTools.WriteCodeString(writer, code);
                }
                catch (Exception ex)
                {
                    DocTools.WriteError(writer, ex);
                }
            }
        }
    }
}
