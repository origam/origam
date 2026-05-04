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

using System.Collections;
using System.Data;
using System.Linq;
using System.Xml;
using Origam.Schema.EntityModel;

namespace Origam.OrigamEngine.ModelXmlBuilders;

public class SectionLevelPluginBuilder
{
    public static void Build(
        XmlElement parentNode,
        string text,
        DataTable table,
        DataStructure dataStructure,
        bool isPreloaded,
        bool isIndependent,
        Hashtable dataSources,
        string modelId,
        string dataMember
    )
    {
        DataStructureEntity entity = dataStructure
            .ChildItemsByTypeRecursive(itemType: DataStructureEntity.CategoryConst)
            .Cast<DataStructureEntity>()
            .First(predicate: item => item.Name == table.TableName);
        parentNode.SetAttribute(
            localName: "type",
            namespaceURI: "http://www.w3.org/2001/XMLSchema-instance",
            value: "UIElement"
        );
        parentNode.SetAttribute(name: "Type", value: "SectionLevelPlugin");
        parentNode.SetAttribute(name: "Name", value: text);
        parentNode.SetAttribute(
            name: "HasPanelConfiguration",
            value: XmlConvert.ToString(value: true)
        );
        parentNode.SetAttribute(name: "Entity", value: entity.Name);
        parentNode.SetAttribute(name: "ModelId", value: modelId);
        parentNode.SetAttribute(name: "DataMember", value: dataMember);
        FormXmlBuilder.AddDataSource(
            dataSources: dataSources,
            table: table,
            controlId: modelId,
            isIndependent: isIndependent
        );

        XmlElement propertiesElement = parentNode.OwnerDocument.CreateElement(name: "Properties");
        parentNode.AppendChild(newChild: propertiesElement);

        XmlElement propertyNamesElement = parentNode.OwnerDocument.CreateElement(
            name: "PropertyNames"
        );
        // parentNode.AppendChild(propertyNamesElement);
        string primaryKeyColumnName = table.PrimaryKey[0].ColumnName;
        XmlElement idPropertyElement = parentNode.OwnerDocument.CreateElement(name: "Property");
        propertiesElement.AppendChild(newChild: idPropertyElement);
        idPropertyElement.SetAttribute(name: "Id", value: primaryKeyColumnName);
        idPropertyElement.SetAttribute(name: "Name", value: primaryKeyColumnName);
        idPropertyElement.SetAttribute(name: "Entity", value: "String");
        idPropertyElement.SetAttribute(name: "Column", value: "Text");
        DataStructureColumn memoColumn = null;
        int lastPos = 5;

        foreach (var column in entity.Columns)
        {
            FormXmlBuilder.AddColumn(
                entity: entity,
                columnName: column.Name,
                memoColumn: ref memoColumn,
                lastPos: ref lastPos,
                propertiesElement: propertiesElement,
                propertyNamesElement: propertyNamesElement,
                table: table,
                formatPattern: null
            );
        }
    }
}
