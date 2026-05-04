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

namespace Origam.Schema.WorkflowModel;

public enum StateMachineEventType
{
    StateEntry = 0,
    StateTransition = 1,
    StateExit = 2,
    RecordCreated = 3,
    RecordUpdated = 4,
    RecordDeleted = 5,
    RecordCreatedUpdated = 6,
    BeforeRecordDeleted = 7,
}

/// <summary>
/// Summary description for StateMachineState.
/// </summary>
[SchemaItemDescription(name: "Event", folderName: "Events", iconName: "event-4.png")]
[HelpTopic(topic: "Data+Events")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class StateMachineEvent : AbstractSchemaItem
{
    public const string CategoryConst = "StateMachineEvent";

    public StateMachineEvent()
        : base()
    {
        Init();
    }

    public StateMachineEvent(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public StateMachineEvent(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init()
    {
        this.ChildItemTypes.Add(item: typeof(StateMachineEventParameterMapping));
        this.ChildItemTypes.Add(item: typeof(StateMachineEventFieldDependency));
    }

    #region Overriden ISchemaItem Members

    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Action);
        if (this.OldState != null)
        {
            dependencies.Add(item: this.OldState);
        }

        if (this.NewState != null)
        {
            dependencies.Add(item: this.NewState);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (
                    this.NewState != null
                    && item.OldPrimaryKey.Equals(obj: this.NewState.PrimaryKey)
                )
                {
                    this.NewState = item as StateMachineState;
                    break;
                }
                if (
                    this.OldState != null
                    && item.OldPrimaryKey.Equals(obj: this.OldState.PrimaryKey)
                )
                {
                    this.OldState = item as StateMachineState;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }
    #endregion
    #region Properties
    [Browsable(browsable: false)]
    public List<StateMachineEventParameterMapping> ParameterMappings =>
        ChildItemsByType<StateMachineEventParameterMapping>(
            itemType: StateMachineEventParameterMapping.CategoryConst
        );

    [Browsable(browsable: false)]
    public List<StateMachineEventFieldDependency> FieldDependencies =>
        ChildItemsByType<StateMachineEventFieldDependency>(
            itemType: StateMachineEventFieldDependency.CategoryConst
        );

    [XmlAttribute(attributeName: "type")]
    public StateMachineEventType Type { get; set; }

    public Guid ActionId;

    [TypeConverter(type: typeof(WorkflowConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "action", idField: "ActionId")]
    public IWorkflow Action
    {
        get
        {
            ModelElementKey key = new ModelElementKey(id: this.ActionId);
            return (IWorkflow)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set => this.ActionId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid OldStateId;

    [TypeConverter(type: typeof(StateMachineStateConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "oldState", idField: "OldStateId")]
    public StateMachineState OldState
    {
        get
        {
            ModelElementKey key = new ModelElementKey(id: this.OldStateId);
            return (StateMachineState)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set => this.OldStateId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid NewStateId;

    [TypeConverter(type: typeof(StateMachineStateConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "newState", idField: "NewStateId")]
    public StateMachineState NewState
    {
        get
        {
            ModelElementKey key = new ModelElementKey(id: this.NewStateId);
            return (StateMachineState)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set => this.NewStateId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    [XmlAttribute(attributeName: "roles")]
    public string Roles { get; set; } = "*";

    [XmlAttribute(attributeName: "features")]
    public string Features { get; set; }
    #endregion
    public override int CompareTo(object obj)
    {
        StateMachineEvent compared = obj as StateMachineEvent;
        if (compared == null)
        {
            return base.CompareTo(obj: obj);
        }
        return (this.Type.CompareTo(target: compared.Type));
    }
}
