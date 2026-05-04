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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for EntityRelationItem.
/// </summary>
[SchemaItemDescription(name: "Data Structure", iconName: "icon_data-structure.png")]
[HelpTopic(topic: "Data+Structures")]
public class DataStructure : AbstractDataStructure, ISchemaItemFactory
{
    public DataStructure()
        : base()
    {
        Init();
    }

    public DataStructure(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId)
    {
        Init();
    }

    public DataStructure(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    #region Properties
    [Browsable(browsable: false)]
    public List<DataStructureEntity> Entities
    {
        get
        {
            var entities = new List<DataStructureEntity>();
            foreach (
                var entity in ChildItemsByType<DataStructureEntity>(
                    itemType: DataStructureEntity.CategoryConst
                )
            )
            {
                entities.Add(item: entity);
                entities.AddRange(collection: GetChildEntities(entity: entity));
            }
            return entities;
        }
    }

    [Browsable(browsable: false)]
    public IList<DataStructureEntity> LocalizableEntities
    {
        get
        {
            List<DataStructureEntity> result = new List<DataStructureEntity>();
            foreach (DataStructureEntity dsEntity in Entities)
            {
                TableMappingItem table = dsEntity.Entity as TableMappingItem;
                if (table != null && table.LocalizationRelation != null)
                {
                    result.Add(item: dsEntity);
                }
            }
            return result;
        }
    }

    [Browsable(browsable: false)]
    public List<DataStructureDefaultSet> DefaultSets =>
        ChildItemsByType<DataStructureDefaultSet>(itemType: DataStructureDefaultSet.CategoryConst);

    [Browsable(browsable: false)]
    public List<DataStructureTemplateSet> TemplateSets =>
        ChildItemsByType<DataStructureTemplateSet>(
            itemType: DataStructureTemplateSet.CategoryConst
        );

    [Browsable(browsable: false)]
    public List<DataStructureMethod> Methods =>
        ChildItemsByType<DataStructureMethod>(itemType: DataStructureMethod.CategoryConst);

    [Browsable(browsable: false)]
    public List<DataStructureRuleSet> RuleSets =>
        ChildItemsByType<DataStructureRuleSet>(itemType: DataStructureRuleSet.CategoryConst);

    [Browsable(browsable: false)]
    public List<DataStructureSortSet> SortSets =>
        ChildItemsByType<DataStructureSortSet>(itemType: DataStructureSortSet.CategoryConst);

    private List<DataStructureEntity> GetChildEntities(DataStructureEntity entity)
    {
        var entities = new List<DataStructureEntity>();
        foreach (
            var childEntity in entity.ChildItemsByType<DataStructureEntity>(
                itemType: DataStructureEntity.CategoryConst
            )
        )
        {
            entities.Add(item: childEntity);
            entities.AddRange(collection: GetChildEntities(entity: childEntity));
        }
        return entities;
    }

    private bool _isLocalized = false;

    [Description(
        description: "Translate data for all entities, that has realtion marked with IsMultilingual='true'. If set to true, any read-write operation will fail."
    )]
    [XmlAttribute(attributeName: "localized")]
    public bool IsLocalized
    {
        get { return _isLocalized; }
        set { _isLocalized = value; }
    }
    private string _dataSetClass;

    [Description(
        description: "A fully qualified name of a class followed by an assembly name which has a class in it. A class should correspond (should have same xsd) as a xsd of a current datastructure. A class will be used everytime a dataset is to be created from a datastructure. A class is worth defining when we need to seamlessly pass a dataset between origam and a service agent (library) code."
    )]
    [XmlAttribute(attributeName: "dataSetClass")]
    public string DataSetClass
    {
        get { return _dataSetClass; }
        set { _dataSetClass = value; }
    }
    #endregion
    #region Overriden ISchemaItem Members
    private void Init()
    {
        this.ChildItemTypes.InsertRange(
            index: 0,
            collection: new Type[]
            {
                typeof(DataStructureEntity),
                typeof(DataStructureFilterSet),
                typeof(DataStructureDefaultSet),
                typeof(DataStructureTemplateSet),
                typeof(DataStructureRuleSet),
                typeof(DataStructureSortSet),
                typeof(SchemaItemParameter),
            }
        );
    }

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        foreach (DataStructureEntity item in Entities)
        {
            item.GetParameterReferences(parentItem: item, list: list);
        }
        foreach (DataStructureDefaultSet defset in DefaultSets)
        {
            defset.GetParameterReferences(parentItem: defset, list: list);
        }
    }
    #endregion
}
