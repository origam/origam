#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using Origam.DA.Service;
using System.Collections;
using System.Web;
using Origam.DA;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Doc
{
    public class DocDataStructure : AbstractDoc
    {
        public DocDataStructure(XmlWriter writer)
            : base(writer)
        {
        }

        public override string FilterName
        {
            get { return "dataStructure"; }
        }

        public override string Name
        {
            get { return "Data Structures"; }
        }

        public override bool UsePrettyPrint
        {
            get
            {
                return true;
            }
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
            return ss.GetProvider(typeof(DataStructureSchemaItemProvider));
        }

        public override List<DiagramConnection> WriteContent(string bodyElement, string elementId, XmlWriter writer, IDocumentationService documentation, IPersistenceService ps)
        {
            if (!CheckElementId(writer, elementId)) return null;

            AbstractDataStructure dataStructure = ps.SchemaProvider.RetrieveInstance(typeof(AbstractDataStructure), new ModelElementKey(new Guid(elementId))) as AbstractDataStructure;
            DataStructure ds = dataStructure as DataStructure;
            List<DiagramConnection> connections = new List<DiagramConnection>();
            List<DiagramElement> elements = new List<DiagramElement>();
            if (ds != null)
            {
                DiagramElements(ds, elements, connections);
            }
            #region body
            DocTools.WriteStartBody(bodyElement, writer, this.Title(elementId, documentation, ps), "Data Structure", DocTools.ImagePath(dataStructure), dataStructure.Id);
            // summary
            WriteSummary(writer, documentation, dataStructure);
            // Ancestors
            WriteAncestors(writer, dataStructure);
            // Packages
            WritePackages(writer, dataStructure);
            // Diagram
            WriteDiagram(writer, elements);
            // Fields
            WriteEntities(writer, documentation, dataStructure);
            // end body
            writer.WriteEndElement();
            #endregion
            // end document
            return connections;
        }

        private void DiagramElements(DataStructure dataStructure, List<DiagramElement> elements, List<DiagramConnection> connections)
        {
            DocEntity docEntity = new DocEntity(null);

            int lastTop = 0;
            elements.Add(new DiagramElement(dataStructure.Id.ToString(), dataStructure.Name, null, docEntity.FilterName, lastTop, 0, "dataStructure"));
            lastTop = AddEntityToDiagram(dataStructure, elements, connections, docEntity, lastTop, 100);
            return;
        }

        private static int AddEntityToDiagram(AbstractSchemaItem parent, List<DiagramElement> elements,
            List<DiagramConnection> connections, DocEntity docEntity, int lastTop, int left)
        {
            // first parent-child
            foreach (DataStructureEntity entity in parent.ChildItemsByType(DataStructureEntity.ItemTypeConst))
            {
                IAssociation assoc = entity.Entity as IAssociation;
                if (assoc == null || assoc.IsParentChild)
                {
                    lastTop = AddEntity(parent, elements, connections, docEntity, lastTop, left, entity);
                }
            }
            // then the rest
            foreach (DataStructureEntity entity in parent.ChildItemsByType(DataStructureEntity.ItemTypeConst))
            {
                IAssociation assoc = entity.Entity as IAssociation;
                if (assoc != null && !assoc.IsParentChild)
                {
                    lastTop = AddEntity(parent, elements, connections, docEntity, lastTop, left, entity);
                }
            }
            return lastTop;
        }

        private static int AddEntity(AbstractSchemaItem parent, List<DiagramElement> elements, List<DiagramConnection> connections, DocEntity docEntity, int lastTop, int left, DataStructureEntity entity)
        {
            int finalLeft = left;
            decimal sourceX = (decimal)0;
            decimal sourceY = (decimal)0;
            decimal targetX = (decimal)0;
            decimal targetY = (decimal)1;

            DiagramConnectionAnchorType finalTargetAnchor = DiagramConnectionAnchorType.Left;
            DiagramConnectionAnchorType finalSourceAnchor = DiagramConnectionAnchorType.BottomLeft;
            IAssociation assoc = entity.Entity as IAssociation;
            if (assoc != null && !assoc.IsParentChild)
            {
                finalLeft = 100;
                finalSourceAnchor = DiagramConnectionAnchorType.Custom;
                finalTargetAnchor = DiagramConnectionAnchorType.Custom;
            }

            lastTop += 50;
            string cssClass = "dataStructureEntity";
            if (!entity.AllFields && entity.ChildItemsByType(DataStructureColumn.ItemTypeConst).Count == 0)
            {
                cssClass = "";
            }
            elements.Add(new DiagramElement(entity.Id.ToString(), entity.Name, entity.EntityDefinition, docEntity.FilterName, lastTop, finalLeft, cssClass));
            connections.Add(new DiagramConnection(entity.Id.ToString(),
                entity.Id.ToString(),
                parent.Id.ToString(),
                null,
                finalTargetAnchor,
                finalSourceAnchor,
                new DiagramConnectionPosition(sourceX, sourceY),
                new DiagramConnectionPosition(targetX, targetY),
                0
                )
                );
            lastTop = AddEntityToDiagram(entity, elements, connections, docEntity, lastTop, left + 100);
            return lastTop;
        }

        private void WriteEntities(XmlWriter writer, IDocumentationService documentation, AbstractDataStructure dataStructure)
        {
            DataStructure ds = dataStructure as DataStructure;
            if (ds != null)
            {
                foreach (DataStructureMethod method in dataStructure.ChildItemsByType(DataStructureMethod.ItemTypeConst))
                {
                    DocTools.WriteSectionStart(writer, method.Name + " Method");
                    DataStructureFilterSet filterSet = method as DataStructureFilterSet;
                    AbstractSqlDataService abstractSqlDataService = DataService.GetDataService() as AbstractSqlDataService;
                    AbstractSqlCommandGenerator abstractSqlCommandGenerator = (AbstractSqlCommandGenerator)abstractSqlDataService.DbDataAdapterFactory;
                    if (filterSet != null)
                    {
                        try
                        {
                            StringBuilder sql = new StringBuilder();
                            // parameter declarations
                            sql.Append(abstractSqlCommandGenerator.SelectParameterDeclarationsSql(filterSet, false, null));

                            foreach (DataStructureEntity entity in ds.Entities)
                            {
                                if (entity.Columns.Count > 0)
                                {
                                    sql.AppendLine();
                                    sql.AppendLine("-- " + entity.Name);
                                    sql.AppendLine(
                                        abstractSqlCommandGenerator.SelectSql(ds,
                                        entity,
                                        filterSet,
                                        null,
                                        DA.ColumnsInfo.Empty,
                                        new Hashtable(),
                                        null,
                                        false
                                        )
                                        );
                                }
                            }
                            DocTools.WriteCodeStart(writer, "example prettyprint language-sql");
                            writer.WriteRaw(HttpUtility.HtmlEncode(sql.ToString()));
                            writer.WriteEndElement();
                        }
                        catch (Exception ex)
                        {
                            DocTools.WriteError(writer, ex);
                        }
                        writer.WriteEndElement();
                    }
                }
            }
        }
    }
}
