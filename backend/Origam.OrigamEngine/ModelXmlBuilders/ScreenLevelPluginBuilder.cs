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
using System.Data;
using System.Linq;
using System.Xml;
using Origam.Schema.EntityModel;

namespace Origam.OrigamEngine.ModelXmlBuilders
{
    public class ScreenLevelPluginBuilder
    {
        public static void Build(XmlElement parentNode, string text,
            Hashtable dataSources,  DataSet dataset, DataStructure dataStructure,
            string dataMember)
        {
            parentNode.SetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance", "UIElement");
            parentNode.SetAttribute("Type", "ScreenLevelPlugin");
            parentNode.SetAttribute("Name", text);
            
            var entities = dataStructure
                .ChildItemsByTypeRecursive(DataStructureEntity.CategoryConst)
                .Cast<DataStructureEntity>();

            foreach (var entity in entities)
            {
                DataTable table = dataset.Tables[entity.Name];
                XmlElement dataElement = parentNode.OwnerDocument.CreateElement ("UIElement");
                parentNode.ParentNode.AppendChild(dataElement);
                AddDataNode(dataElement, table, dataSources,
                    dataMember, entity);
            }
        }

        private static void AddDataNode(XmlElement parentNode, DataTable table, 
            Hashtable dataSources, string dataMember, DataStructureEntity entity)
        {
            string modelId = Guid.NewGuid().ToString();
            parentNode.SetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance", "UIElement");
            parentNode.SetAttribute("Type", "ScreenLevelPluginData");
            parentNode.SetAttribute("HasPanelConfiguration", XmlConvert.ToString (true));
            parentNode.SetAttribute("Name", entity.Name);
            parentNode.SetAttribute("Entity", entity.Name);
            parentNode.SetAttribute("ModelId", modelId);
            parentNode.SetAttribute("ModelInstanceId", modelId);
            parentNode.SetAttribute("DataMember", dataMember);

            FormXmlBuilder.AddDataSource(dataSources, table, modelId, false);
            
            XmlElement propertiesElement = parentNode.OwnerDocument.CreateElement("Properties");
            parentNode.AppendChild(propertiesElement);
            
            XmlElement propertyNamesElement = parentNode.OwnerDocument.CreateElement("PropertyNames");

            string primaryKeyColumnName = table.PrimaryKey[0].ColumnName;
            XmlElement idPropertyElement = parentNode.OwnerDocument.CreateElement("Property");
            propertiesElement.AppendChild(idPropertyElement);
            idPropertyElement.SetAttribute("Id", primaryKeyColumnName);
            idPropertyElement.SetAttribute("Name", primaryKeyColumnName);
            idPropertyElement.SetAttribute("Entity", "String");
            idPropertyElement.SetAttribute("Column", "Text");

            DataStructureColumn memoColumn = null;
            int lastPos = 5;
            
            foreach(var column in entity.Columns)
            {
                FormXmlBuilder.AddColumn(entity, column.Name, ref memoColumn, 
                    ref lastPos, propertiesElement,	propertyNamesElement, table, null);
            }
        }
    }
}