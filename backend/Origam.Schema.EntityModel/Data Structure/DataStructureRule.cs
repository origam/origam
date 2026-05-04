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
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;

[SchemaItemDescription(name: "Rule", iconName: "icon_rule.png")]
[HelpTopic(topic: "Rule+Set+Rule")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataStructureRule : AbstractSchemaItem
{
    public const string CategoryConst = "DataStructureRule";

    public DataStructureRule() { }

    public DataStructureRule(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataStructureRule(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    public List<DataStructureRuleDependency> RuleDependencies =>
        ChildItemsByType<DataStructureRuleDependency>(
            itemType: DataStructureRuleDependency.CategoryConst
        );
    private int _priority = 100;

    [DefaultValue(value: 100)]
    [XmlAttribute(attributeName: "priority")]
    public int Priority
    {
        get => _priority;
        set => _priority = value;
    }

    public Guid DataStructureEntityId;
    private string entityName;
    public string EntityName => entityName ??= Entity?.Name;

    [TypeConverter(type: typeof(DataQueryEntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "entity", idField: "DataStructureEntityId")]
    [NotNullModelElementRule]
    public DataStructureEntity Entity
    {
        get =>
            (DataStructureEntity)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(DataStructureEntity),
                    primaryKey: new ModelElementKey(id: DataStructureEntityId)
                );
        set
        {
            DataStructureEntityId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
            TargetField = null;
        }
    }

    public Guid TargetFieldId;

    [TypeConverter(type: typeof(DataStructureEntityFieldConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "targetField", idField: "TargetFieldId")]
    public IDataEntityColumn TargetField
    {
        get =>
            (IDataEntityColumn)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: TargetFieldId)
                );
        set => TargetFieldId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid ValueRuleId;

    [TypeConverter(type: typeof(DataRuleConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "valueRule", idField: "ValueRuleId")]
    [NotNullModelElementRule]
    public IDataRule ValueRule
    {
        get =>
            (IDataRule)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ValueRuleId)
                );
        set => ValueRuleId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid CheckRuleId;

    [TypeConverter(type: typeof(StartRuleConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "conditionRule", idField: "CheckRuleId")]
    public IStartRule ConditionRule
    {
        get =>
            (IStartRule)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: CheckRuleId)
                );
        set => CheckRuleId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    #endregion
    #region Overriden ISchemaItem Members

    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: Entity);
        if (TargetField != null)
        {
            dependencies.Add(item: TargetField);
        }
        if (ValueRule != null)
        {
            dependencies.Add(item: ValueRule);
        }
        if (ConditionRule != null)
        {
            dependencies.Add(item: ConditionRule);
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey?.Equals(obj: Entity.PrimaryKey) == true)
            {
                var targetFieldBck = TargetField;
                // setting the Entity normally resets TargetField as well
                Entity = item as DataStructureEntity;
                TargetField = targetFieldBck;
                break;
            }
        }
        base.UpdateReferences();
    }

    public override bool CanMove(UI.IBrowserNode2 newNode)
    {
        return newNode.Equals(obj: ParentItem);
    }

    public override int CompareTo(object obj)
    {
        if ((obj as DataStructureRule) != null)
        {
            return this.Name.CompareTo(strB: (obj as DataStructureRule).Name);
        }
        // rulesets are always an top, so rules are lower
        return 1;
    }
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes => new[] { typeof(DataStructureRuleDependency) };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: typeof(T) == typeof(DataStructureRuleDependency) ? "NewDependency" : null
        );
    }
    #endregion
}
