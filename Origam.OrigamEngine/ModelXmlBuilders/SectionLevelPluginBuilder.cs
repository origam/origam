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
using Origam.Schema;
using Origam.Schema.EntityModel;

namespace Origam.OrigamEngine.ModelXmlBuilders
{
    public class SectionLevelPluginBuilder
    {
        public static void Build(XmlElement parentNode, string text,
            DataTable table, DataStructure dataStructure, bool isPreloaded,
            bool isIndependent, Hashtable dataSources, string modelId, string dataMember)
        {
            DataStructureEntity entity = dataStructure
                .ChildItemsByTypeRecursive(DataStructureEntity.CategoryConst)
                .Cast<DataStructureEntity>()
                .First(item => item.Name == table.TableName);
            parentNode.SetAttribute("type", "http://www.w3.org/2001/XMLSchema-instance", "UIElement");
            parentNode.SetAttribute("Type", "SectionLevelPlugin");
            parentNode.SetAttribute("Name", text);
            parentNode.SetAttribute("HasPanelConfiguration", XmlConvert.ToString (true));
            parentNode.SetAttribute("Entity", entity.Name);
            parentNode.SetAttribute("ModelId", modelId);
            parentNode.SetAttribute("DataMember", dataMember);

            FormXmlBuilder.AddDataSource(dataSources, table, modelId, isIndependent);
            
            XmlElement propertiesElement = parentNode.OwnerDocument.CreateElement("Properties");
            parentNode.AppendChild(propertiesElement);
            
            XmlElement propertyNamesElement = parentNode.OwnerDocument.CreateElement("PropertyNames");
            // parentNode.AppendChild(propertyNamesElement);

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