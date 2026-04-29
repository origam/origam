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
using System.Data;
using System.Linq;
using System.Xml.XPath;
using Origam.DA;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Schema.WorkflowModel;

[SchemaItemDescription(name: "State Workflow", iconName: "state-workflow-2.png")]
[HelpTopic(topic: "State+Workflows")]
[DefaultProperty(name: "Entity")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class StateMachine : AbstractSchemaItem
{
    public const string CategoryConst = "WorkflowStateMachine";

    public StateMachine() { }

    public StateMachine(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public StateMachine(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Public Methods
    public StateMachineState GetState(object value)
    {
        return AllStates()
            .FirstOrDefault(predicate: state =>
                (state.Type != StateMachineStateType.Group) && state.Value.Equals(obj: value)
            );
    }

    public List<StateMachineState> AllStates()
    {
        var result = new List<StateMachineState>();
        foreach (ISchemaItem item in ChildItemsRecursive)
        {
            if (item is StateMachineState state)
            {
                result.Add(item: state);
            }
        }
        return result;
    }
    #endregion
    #region Overriden ISchemaItem Members

    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: Entity);
        if (Field != null)
        {
            dependencies.Add(item: Field);
        }
        if (DynamicOperationsLookup != null)
        {
            dependencies.Add(item: DynamicOperationsLookup);
        }
        if (DynamicStatesLookup != null)
        {
            dependencies.Add(item: DynamicStatesLookup);
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
    #region Properties
    [Browsable(browsable: false)]
    public List<StateMachineEvent> Events =>
        ChildItemsByType<StateMachineEvent>(itemType: StateMachineEvent.CategoryConst);

    [Browsable(browsable: false)]
    public List<StateMachineDynamicLookupParameterMapping> ParameterMappings =>
        ChildItemsByType<StateMachineDynamicLookupParameterMapping>(
            itemType: StateMachineDynamicLookupParameterMapping.CategoryConst
        );

    public object[] DynamicOperations(IXmlContainer data)
    {
        var dataView = GetDynamicList(lookupId: DynamicOperationsLookupId, data: data);
        var result = new object[dataView.Count];
        for (var i = 0; i < dataView.Count; i++)
        {
            result[i] = dataView[recordIndex: i][property: DynamicOperationsLookup.ListValueMember];
        }
        return result;
    }

    public object[] InitialStateValues(IXmlContainer data)
    {
        var list = new List<object>();
        if (DynamicStatesLookup == null)
        {
            // states defined in the model
            foreach (StateMachineState state in AllStates())
            {
                if (state.Type == StateMachineStateType.Initial)
                {
                    list.Add(item: state.Value);
                }
            }
        }
        else
        {
            var dataView = GetDynamicList(lookupId: DynamicStatesLookupId, data: data);
            // dynamic state list from the data source
            foreach (DataRowView dataRowView in dataView)
            {
                if ((bool)dataRowView[property: "IsInitial"])
                {
                    list.Add(item: dataRowView[property: "Id"]);
                }
            }
        }
        return list.ToArray();
    }

    private DataView GetDynamicList(Guid lookupId, IXmlContainer data)
    {
        DataView view;
        var dataLookupService = ServiceManager.Services.GetService<IDataLookupService>();
        if (ParameterMappings.Count == 0)
        {
            view = dataLookupService.GetList(lookupId: lookupId, transactionId: null);
        }
        else
        {
            if (data == null)
            {
                view = new DataView(table: new OrigamDataTable());
            }
            else
            {
                var parameters = new Dictionary<string, object>();
                foreach (
                    StateMachineDynamicLookupParameterMapping parameterMapping in ParameterMappings
                )
                {
                    var xpath =
                        "/row/"
                        + (
                            parameterMapping.Field.XmlMappingType
                            == EntityColumnXmlMapping.Attribute
                                ? "@"
                                : ""
                        )
                        + parameterMapping.Field.Name;
                    var xPathNavigator = data.Xml.CreateNavigator();
                    var value = xPathNavigator.Evaluate(xpath: xpath);
                    if (value is XPathNodeIterator iterator)
                    {
                        if (iterator.Count == 0)
                        {
                            value = null;
                        }
                        else
                        {
                            iterator.MoveNext();
                            value = iterator.Current.Value;
                        }
                    }
                    else
                    {
                        value = null;
                    }
                    parameters.Add(key: parameterMapping.Name, value: value);
                }
                view = dataLookupService.GetList(
                    lookupId: lookupId,
                    parameters: parameters,
                    transactionId: null
                );
            }
        }
        return view;
    }

    public Guid EntityId;

    [NotNullModelElementRule]
    [TypeConverter(type: typeof(EntityConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "entity", idField: "EntityId")]
    public IDataEntity Entity
    {
        get =>
            (IDataEntity)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: EntityId)
                );
        set
        {
            EntityId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
            SetName();
        }
    }

    public Guid FieldId;

    [TypeConverter(type: typeof(StateMachineEntityFieldConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "field", idField: "FieldId")]
    public IDataEntityColumn Field
    {
        get =>
            (IDataEntityColumn)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: FieldId)
                );
        set
        {
            FieldId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
            SetName();
        }
    }

    public Guid DynamicStatesLookupId;

    [TypeConverter(type: typeof(DataLookupConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "dynamicStatesLookup", idField: "DynamicStatesLookupId")]
    public IDataLookup DynamicStatesLookup
    {
        get =>
            (IDataLookup)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: DynamicStatesLookupId)
                );
        set =>
            DynamicStatesLookupId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid DynamicOperationsLookupId;

    [TypeConverter(type: typeof(DataLookupConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "dynamicOperationsLookup", idField: "DynamicOperationsLookupId")]
    public IDataLookup DynamicOperationsLookup
    {
        get =>
            (IDataLookup)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: DynamicOperationsLookupId)
                );
        set =>
            DynamicOperationsLookupId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid ReverseLookupId;

    [TypeConverter(type: typeof(DataLookupConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "reverseLookup", idField: "ReverseLookupId")]
    public IDataLookup ReverseLookup
    {
        get =>
            (IDataLookup)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ReverseLookupId)
                );
        set => ReverseLookupId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    #endregion
    #region Private Methods
    private void SetName()
    {
        if ((Entity != null) && (Field != null))
        {
            Name = Entity.Name + "_" + Field.Name;
        }
        else if (Entity != null)
        {
            Name = Entity.Name;
        }
        else
        {
            Name = "";
        }
    }
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes =>
        new[]
        {
            typeof(StateMachineState),
            typeof(StateMachineEvent),
            typeof(StateMachineDynamicLookupParameterMapping),
        };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(StateMachineState))
        {
            itemName = "NewState";
        }
        else if (typeof(T) == typeof(StateMachineEvent))
        {
            itemName = "NewEvent";
        }
        else if (typeof(T) == typeof(StateMachineDynamicLookupParameterMapping))
        {
            itemName = "NewParameterMapping";
        }
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: itemName
        );
    }
    #endregion
}
