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
using System.Collections.Generic;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;

[SchemaItemDescription(name: "Rule Set", folderName: "Rule Sets", iconName: "icon_rule-set.png")]
[HelpTopic(topic: "Rule+Sets")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataStructureRuleSet : AbstractSchemaItem
{
    public const string CategoryConst = "DataStructureRuleSet";
    private object _lock = new object();

    public DataStructureRuleSet() { }

    public DataStructureRuleSet(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataStructureRuleSet(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Public Methods
    public List<DataStructureRule> Rules()
    {
        var result = ChildItemsByType<DataStructureRule>(itemType: DataStructureRule.CategoryConst);
        // add all child rule sets
        foreach (
            var childRuleSet in ChildItemsByType<DataStructureRuleSetReference>(
                itemType: DataStructureRuleSetReference.CategoryConst
            )
        )
        {
            if (childRuleSet.RuleSet != null)
            {
                result.AddRange(collection: childRuleSet.RuleSet.Rules());
            }
        }
        return result;
    }

    public void AddUniqueRuleSetIds(
        HashSet<Guid> ruleSetUniqIds,
        DataStructureRuleSetReference curRuleSetReference
    )
    {
        if (!ruleSetUniqIds.Add(item: Id))
        {
            throw new NullReferenceException(
                message: $"Ruleset `{Name}' ({Id}) found twice. Circular ruleset reference found."
            );
        }
        var addCurrent = true;
        foreach (
            var ruleSetReference in ChildItemsByType<DataStructureRuleSetReference>(
                itemType: DataStructureRuleSetReference.CategoryConst
            )
        )
        {
            if (curRuleSetReference != null && curRuleSetReference.Id == ruleSetReference.Id)
            {
                // current already processed
                addCurrent = false;
            }
            ruleSetReference.RuleSet.AddUniqueRuleSetIds(
                ruleSetUniqIds: ruleSetUniqIds,
                curRuleSetReference: curRuleSetReference
            );
        }
        // add current ruleset virtually - if we are in proper parent ruleset
        if (curRuleSetReference != null && curRuleSetReference.ParentItemId == Id && addCurrent)
        {
            curRuleSetReference.RuleSet.AddUniqueRuleSetIds(
                ruleSetUniqIds: ruleSetUniqIds,
                curRuleSetReference: curRuleSetReference
            );
        }
    }

    public List<DataStructureRule> Rules(string entityName)
    {
        var result = new List<DataStructureRule>();
        foreach (DataStructureRule rule in Rules())
        {
            if (rule.EntityName == entityName && rule.RuleDependencies.Count == 0)
            {
                result.Add(item: rule);
            }
        }
        return result;
    }

    public Hashtable RulesDepending(string entityName)
    {
        var result = new Hashtable();
        foreach (DataStructureRule rule in Rules())
        {
            if (rule.Entity.Name == entityName && rule.RuleDependencies.Count > 0)
            {
                result[key: rule.PrimaryKey] = rule;
            }
        }
        return result;
    }

#if ORIGAM_CLIENT
    private static Dictionary<string, List<DataStructureRule>> _ruleCache = new();
#endif

    public List<DataStructureRule> Rules(string entityName, Guid fieldId, bool includeOtherEntities)
    {
        List<DataStructureRule> result;
#if ORIGAM_CLIENT
        string cacheId = Id + entityName + fieldId + includeOtherEntities;
        lock (_lock)
        {
            if (_ruleCache.TryGetValue(key: cacheId, value: out var rules))
            {
                return rules;
            }
#endif
            result = new List<DataStructureRule>();
            foreach (DataStructureRule rule in Rules())
            {
                foreach (DataStructureRuleDependency dep in rule.RuleDependencies)
                {
                    if (
                        (
                            (includeOtherEntities == false && rule.Entity.Name == entityName)
                            || includeOtherEntities
                        )
                        && dep.Entity.Name == entityName
                        && dep.FieldId == fieldId
                    )
                    {
                        result.Add(item: rule);
                    }
                }
            }
#if ORIGAM_CLIENT
            _ruleCache.Add(key: cacheId, value: result);
        }
#endif
        return result;
    }
    #endregion
    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }
    public override bool UseFolders
    {
        get { return false; }
    }
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes =>
        new[] { typeof(DataStructureRule), typeof(DataStructureRuleSetReference) };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(DataStructureRule))
        {
            itemName = "NewRule";
        }
        else if (typeof(T) == typeof(DataStructureRuleSetReference))
        {
            itemName = "NewRuleSetReference";
        }
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: itemName
        );
    }
    #endregion
}
