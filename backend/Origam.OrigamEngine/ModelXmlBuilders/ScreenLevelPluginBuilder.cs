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

namespace Origam.OrigamEngine.ModelXmlBuilders;

public class ScreenLevelPluginBuilder
{
    public static void Build(
        XmlElement parentNode,
        string text,
        Hashtable dataSources,
        DataSet dataset,
        DataStructure dataStructure,
        string dataMember
    )
    {
        parentNode.SetAttribute(
            localName: "type",
            namespaceURI: "http://www.w3.org/2001/XMLSchema-instance",
            value: "UIElement"
        );
        parentNode.SetAttribute(name: "Type", value: "ScreenLevelPlugin");
        parentNode.SetAttribute(name: "Name", value: text);

        var entities = dataStructure
            .ChildItemsByTypeRecursive(itemType: DataStructureEntity.CategoryConst)
            .Cast<DataStructureEntity>();
        foreach (var entity in entities)
        {
            DataTable table = dataset.Tables[name: entity.Name];
            XmlElement dataElement = parentNode.OwnerDocument.CreateElement(name: "UIElement");
            parentNode.ParentNode.AppendChild(newChild: dataElement);
            AddDataNode(
                parentNode: dataElement,
                table: table,
                dataSources: dataSources,
                dataMember: dataMember,
                entity: entity
            );
        }
    }

    private static void AddDataNode(
        XmlElement parentNode,
        DataTable table,
        Hashtable dataSources,
        string dataMember,
        DataStructureEntity entity
    )
    {
        string modelId = Guid.NewGuid().ToString();
        parentNode.SetAttribute(
            localName: "type",
            namespaceURI: "http://www.w3.org/2001/XMLSchema-instance",
            value: "UIElement"
        );
        parentNode.SetAttribute(name: "Type", value: "ScreenLevelPluginData");
        parentNode.SetAttribute(
            name: "HasPanelConfiguration",
            value: XmlConvert.ToString(value: true)
        );
        parentNode.SetAttribute(name: "Name", value: entity.Name);
        parentNode.SetAttribute(name: "Entity", value: entity.Name);
        parentNode.SetAttribute(name: "ModelId", value: modelId);
        parentNode.SetAttribute(name: "ModelInstanceId", value: modelId);
        parentNode.SetAttribute(name: "DataMember", value: dataMember);
        FormXmlBuilder.AddDataSource(
            dataSources: dataSources,
            table: table,
            controlId: modelId,
            isIndependent: false
        );

        XmlElement propertiesElement = parentNode.OwnerDocument.CreateElement(name: "Properties");
        parentNode.AppendChild(newChild: propertiesElement);

        XmlElement propertyNamesElement = parentNode.OwnerDocument.CreateElement(
            name: "PropertyNames"
        );
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
