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
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;

[SchemaItemDescription(name: "Rule Set Reference", iconName: "icon_rule-set-reference.png")]
[HelpTopic(topic: "Rule+Set+Reference")]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "RuleSet")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataStructureRuleSetReference : AbstractSchemaItem
{
    public const string CategoryConst = "DataStructureRuleSetReference";

    public DataStructureRuleSetReference()
        : base() { }

    public DataStructureRuleSetReference(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataStructureRuleSetReference(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    public Guid DataStructureRuleSetId;

    [Category(category: "Ruleset reference")]
    [Description(
        description: "Choose a ruleset (from current datastructure). All rules of chosen ruleset will be applied in the current (parent) ruleset. Could be handy if you have to many rules and want to organize them or you don't want to run all rules for some reasons (e.g. optimization reasons)."
    )]
    [TypeConverter(type: typeof(DataStructureRuleSetConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [RuleSetRuleUniqnessModelElementRule()]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "ruleSet", idField: "DataStructureRuleSetId")]
    public DataStructureRuleSet RuleSet
    {
        get
        {
            return (DataStructureRuleSet)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(DataStructureRuleSet),
                    primaryKey: new ModelElementKey(id: this.DataStructureRuleSetId)
                );
        }
        set
        {
            this.DataStructureRuleSetId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
            if (this.RuleSet != null)
            {
                this.Name = this.RuleSet.Name;
            }
        }
    }
    #endregion
    #region Overriden ISchemaItem Members
    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (RuleSet != null)
        {
            dependencies.Add(item: RuleSet);
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null && this.RuleSet != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.RuleSet.PrimaryKey))
                {
                    this.DataStructureRuleSetId = (item as DataStructureRuleSet).Id;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }

    public override int CompareTo(object obj)
    {
        if ((obj as DataStructureRuleSetReference) != null)
        {
            return this.Name.CompareTo(strB: (obj as DataStructureRuleSetReference).Name);
        }
        // rulesets are always an top, so rules are lower
        return -1;
    }
}
