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
using Origam.Schema.EntityModel;

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Summary description for StateMachineState.
/// </summary>
[SchemaItemDescription(name: "Transition", folderName: "Operations", iconName: "transition-2.png")]
[HelpTopic(topic: "State+Transition")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class StateMachineOperation : AbstractSchemaItem
{
    public const string CategoryConst = "StateMachineOperation";

    public StateMachineOperation()
        : base() { }

    public StateMachineOperation(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public StateMachineOperation(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden ISchemaItem Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.TargetState);
        if (this.Rule != null)
        {
            dependencies.Add(item: this.Rule);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.TargetState.PrimaryKey))
                {
                    this.TargetState = item as StateMachineState;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }
    #endregion
    #region Properties
    private string _roles = "*";

    [NotNullModelElementRule()]
    [DefaultValue(value: "*")]
    [XmlAttribute(attributeName: "roles")]
    public string Roles
    {
        get { return _roles; }
        set { _roles = value; }
    }
    private string _features;

    [XmlAttribute(attributeName: "features")]
    public string Features
    {
        get { return _features; }
        set { _features = value; }
    }

    public Guid RuleId;

    [TypeConverter(type: typeof(EntityRuleConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "rule", idField: "RuleId")]
    public IEntityRule Rule
    {
        get
        {
            ModelElementKey key = new ModelElementKey(id: this.RuleId);
            return (IEntityRule)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set { this.RuleId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]; }
    }

    public Guid TargetStateId;

    [TypeConverter(type: typeof(StateMachineStateConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "targetState", idField: "TargetStateId")]
    public StateMachineState TargetState
    {
        get
        {
            ModelElementKey key = new ModelElementKey(id: this.TargetStateId);
            return (StateMachineState)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set { this.TargetStateId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]; }
    }
    #endregion
}
