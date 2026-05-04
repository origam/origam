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
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;
using Origam.DA;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.Schema.WorkflowModel.WorkQueue;
using Origam.Service.Core;
using Origam.Workbench.Services;
using Origam.Workflow.WorkQueue;

namespace Origam.Workflow;

/// <summary>
/// Summary description for StateMachineService.
/// </summary>
public class StateMachineService : AbstractService, IStateMachineService
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    IPersistenceService _persistence =
        ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
        as IPersistenceService;
    private static WorkQueueData _WorkQueueCache;
    private static DateTime _WorkQueueLastRefreshed;

    public StateMachineService()
    {
        this.Initialize += new EventHandler(StateMachineService_Initialize);
        this.Unload += new EventHandler(StateMachineService_Unload);
    }

    private void StateMachineService_Unload(object sender, EventArgs e)
    {
        _persistence = null;
    }

    #region IStateMachineService Members
    public bool IsInState(Guid entityId, Guid fieldId, object currentStateValue, Guid targetStateId)
    {
        StateMachine sm = GetMachine(entityId: entityId, fieldId: fieldId, throwException: false);
        StateMachineState targetStateObj =
            sm.GetChildByIdRecursive(id: targetStateId) as StateMachineState;
        object convertedStateValue;
        if (
            sm.Field.DataType == OrigamDataType.UniqueIdentifier
            && currentStateValue.GetType() == typeof(string)
        )
        {
            convertedStateValue = new Guid(g: (string)currentStateValue);
        }
        else
        {
            convertedStateValue = currentStateValue;
        }
        return targetStateObj.IsState(value: convertedStateValue);
    }

    public object[] AllowedStateValues(
        Guid entityId,
        Guid fieldId,
        object currentStateValue,
        DataRow dataRow,
        string transactionId
    )
    {
        IXmlContainer dataDocument = DatasetTools.GetRowXml(
            row: dataRow,
            version: DataRowVersion.Default
        );
        return AllowedStateValues(
            entityId: entityId,
            fieldId: fieldId,
            currentStateValue: currentStateValue,
            data: dataDocument,
            transactionId: transactionId
        );
    }

    public object[] AllowedStateValues(
        Guid entityId,
        Guid fieldId,
        object currentStateValue,
        IXmlContainer data,
        string transactionId
    )
    {
        StateMachine sm = GetMachine(entityId: entityId, fieldId: fieldId, throwException: false);
        if (sm == null)
        {
            // no statemachine on this entity/field
            return null;
        }

        if (currentStateValue == DBNull.Value)
        {
            // original value is null, meaning that this is a new row
            // we only allow the initial state to be selected, no other
            var resultList = new List<object>();
            foreach (object val in sm.InitialStateValues(data: data))
            {
                resultList.Add(item: val);
            }
            return resultList.ToArray();
        }

        if (sm.DynamicOperationsLookup != null)
        {
            // state has existed already, so we list all possible dynamic states
            IXmlContainer dynData = data.Clone() as IXmlContainer;
            if (dynData is DataDocumentCore)
            {
                throw new NotImplementedException(
                    message: "Cannot write to Xml property of DataDocumentCore"
                );
            }

            if (sm.Field.XmlMappingType == EntityColumnXmlMapping.Attribute)
            {
                ((XmlElement)dynData.Xml.FirstChild).SetAttribute(
                    name: sm.Field.Name,
                    value: XmlTools.ConvertToString(val: currentStateValue)
                );
            }
            else
            {
                if (dynData.Xml.FirstChild.SelectSingleNode(xpath: sm.Field.Name) != null)
                {
                    dynData.Xml.FirstChild.SelectSingleNode(xpath: sm.Field.Name).Value =
                        XmlTools.ConvertToString(val: currentStateValue);
                }
                else
                {
                    XmlElement el = dynData.Xml.CreateElement(name: sm.Field.Name);
                    el.Value = XmlTools.ConvertToString(val: currentStateValue);
                    dynData.Xml.FirstChild.AppendChild(newChild: el);
                }
            }
            var a = new List<object>();
            a.Add(item: currentStateValue);
            a.AddRange(collection: sm.DynamicOperations(data: dynData));
            return a.ToArray();
        }

        // state has existed already, so we list all the possible model states
        StateMachineState state = sm.GetState(value: currentStateValue);
        if (state == null)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "currentStateValue",
                actualValue: currentStateValue,
                message: ResourceUtils.GetString(key: "ErrorBuildStateList")
            );
        }

        {
            var resultList = new List<object>();
            resultList.Add(item: currentStateValue);
            while (state.ParentItem is StateMachineState | state.ParentItem is StateMachine)
            {
                foreach (StateMachineOperation op in state.Operations)
                {
                    if (IsOperationAllowed(operation: op, data: data, transactionId: transactionId))
                    {
                        resultList.Add(item: op.TargetState.Value);
                    }
                }
                if (state.ParentItem is StateMachine)
                {
                    break;
                }
                state = state.ParentItem as StateMachineState;
            }
            return resultList.ToArray();
        }
    }

    public bool IsStateAllowed(
        Guid entityId,
        Guid fieldId,
        object currentStateValue,
        object newStateValue,
        DataRow dataRow,
        string transactionId
    )
    {
        IXmlContainer data = DatasetTools.GetRowXml(row: dataRow, version: DataRowVersion.Default);
        return IsStateAllowed(
            entityId: entityId,
            fieldId: fieldId,
            currentStateValue: currentStateValue,
            newStateValue: newStateValue,
            data: data,
            transactionId: transactionId
        );
    }

    public bool IsStateAllowed(
        Guid entityId,
        Guid fieldId,
        object currentStateValue,
        object newStateValue,
        IXmlContainer data,
        string transactionId
    )
    {
        foreach (
            object allowedValue in this.AllowedStateValues(
                entityId: entityId,
                fieldId: fieldId,
                currentStateValue: currentStateValue,
                data: data,
                transactionId: transactionId
            )
        )
        {
            if (newStateValue.Equals(obj: allowedValue))
            {
                return true;
            }
        }
        return false;
    }

    public void OnDataChanging(DataTable changedTable, string transactionId)
    {
        if (changedTable.ExtendedProperties.Contains(key: "EntityId"))
        {
            var stateColumns = new List<DataColumn>();
            Guid entityId = (Guid)changedTable.ExtendedProperties[key: "EntityId"];
            foreach (DataColumn column in changedTable.Columns)
            {
                if (
                    column.ExtendedProperties.Contains(key: "IsState")
                    && (bool)column.ExtendedProperties[key: "IsState"]
                )
                {
                    stateColumns.Add(item: column);
                }
            }
            foreach (DataRow row in changedTable.Rows)
            {
                if (stateColumns.Count > 0)
                {
                    foreach (DataColumn column in stateColumns)
                    {
                        Guid fieldId = (Guid)column.ExtendedProperties[key: "Id"];
                        switch (row.RowState)
                        {
                            case DataRowState.Added:
                            {
                                ExecutePreEvents(
                                    entityId: entityId,
                                    fieldId: fieldId,
                                    currentStateValue: DBNull.Value,
                                    newStateValue: row[column: column],
                                    dataRow: row,
                                    transactionId: transactionId
                                );
                                break;
                            }

                            case DataRowState.Modified:
                            {
                                object oldValue = row[
                                    column: column,
                                    version: DataRowVersion.Original
                                ];
                                object newValue = row[
                                    column: column,
                                    version: DataRowVersion.Current
                                ];
                                if (!oldValue.Equals(obj: newValue))
                                {
                                    ExecutePreEvents(
                                        entityId: entityId,
                                        fieldId: fieldId,
                                        currentStateValue: oldValue,
                                        newStateValue: newValue,
                                        dataRow: row,
                                        transactionId: transactionId
                                    );
                                }
                                break;
                            }

                            case DataRowState.Deleted:
                            {
                                break;
                            }
                        }
                    }
                }
                if (row.RowState == DataRowState.Deleted)
                {
                    ExecuteStatelessEvents(
                        entityId: entityId,
                        eventType: StateMachineServiceStatelessEventType.BeforeRecordDeleted,
                        dataRow: row,
                        transactionId: transactionId
                    );
                }
            }
        }
    }

    public void OnDataChanged(DataSet data, List<string> changedTables, string transactionId)
    {
        var rows = new List<StateMachineQueueEntry>();
        foreach (string tableName in changedTables)
        {
            DataTable changedTable = data.Tables[name: tableName];
            if (changedTable.ExtendedProperties.Contains(key: "EntityId"))
            {
                var stateColumns = new List<DataColumn>();
                Guid entityId = (Guid)changedTable.ExtendedProperties[key: "EntityId"];
                foreach (DataColumn column in changedTable.Columns)
                {
                    if (
                        column.ExtendedProperties.Contains(key: "IsState")
                        && (bool)column.ExtendedProperties[key: "IsState"]
                    )
                    {
                        stateColumns.Add(item: column);
                    }
                }
                if (
                    (stateColumns.Count > 0)
                    || AnyWorkQueueClassBasedOnEntity(entityId: entityId)
                    || AnyStateMachineBasedOnEntity(entityId: entityId)
                )
                {
                    foreach (DataRow row in changedTable.Rows)
                    {
                        if (
                            row.RowState != DataRowState.Unchanged
                            && row.RowState != DataRowState.Detached
                        )
                        {
                            rows.Add(
                                item: new StateMachineQueueEntry(
                                    row: row,
                                    stateColumns: stateColumns,
                                    entityId: entityId
                                )
                            );
                        }
                    }
                }
            }
        }
        foreach (StateMachineQueueEntry entry in rows)
        {
            ProcessRecordCreated(
                row: entry.Row,
                entityId: entry.EntityId,
                transactionId: transactionId
            );
        }
        foreach (StateMachineQueueEntry entry in rows)
        {
            ProcessRecordStateTransition(
                row: entry.Row,
                stateColumns: entry.StateColumns,
                entityId: entry.EntityId,
                transactionId: transactionId
            );
        }
        foreach (StateMachineQueueEntry entry in rows)
        {
            ProcessRecordModifiedDeleted(
                row: entry.Row,
                entityId: entry.EntityId,
                transactionId: transactionId
            );
        }
    }

    private void ProcessRecordCreated(DataRow row, Guid entityId, string transactionId)
    {
        // recordCreated
        if (row.RowState == DataRowState.Added)
        {
            XmlContainer data = DatasetTools.GetRowXml(row: row, version: DataRowVersion.Default);
            ExecuteStatelessEvents(
                entityId: entityId,
                eventType: StateMachineServiceStatelessEventType.RecordCreated,
                dataRow: row,
                transactionId: transactionId
            );
        }
    }

    private void ProcessRecordStateTransition(
        DataRow row,
        List<DataColumn> stateColumns,
        Guid entityId,
        string transactionId
    )
    {
        if (row.RowState != DataRowState.Deleted)
        {
            IXmlContainer data = DatasetTools.GetRowXml(row: row, version: DataRowVersion.Default);
            // state entry or state transition
            foreach (DataColumn column in stateColumns)
            {
                Guid fieldId = (Guid)column.ExtendedProperties[key: "Id"];
                StateMachine sm = GetMachine(
                    entityId: entityId,
                    fieldId: fieldId,
                    throwException: true
                );
                switch (row.RowState)
                {
                    case DataRowState.Added:
                    {
                        ExecutePostEvents(
                            sm: sm,
                            currentStateValue: DBNull.Value,
                            newStateValue: row[column: column],
                            dataRow: row,
                            data: data,
                            transactionId: transactionId
                        );
                        break;
                    }

                    case DataRowState.Modified:
                    {
                        object oldValue = row[column: column, version: DataRowVersion.Original];
                        object newValue = row[column: column, version: DataRowVersion.Current];
                        if (!oldValue.Equals(obj: newValue))
                        {
                            ExecutePostEvents(
                                sm: sm,
                                currentStateValue: oldValue,
                                newStateValue: newValue,
                                dataRow: row,
                                data: data,
                                transactionId: transactionId
                            );
                        }
                        break;
                    }
                }
            }
        }
    }

    private void ProcessRecordModifiedDeleted(DataRow row, Guid entityId, string transactionId)
    {
        // record updated/deleted
        switch (row.RowState)
        {
            case DataRowState.Modified:
            {
                ExecuteStatelessEvents(
                    entityId: entityId,
                    eventType: StateMachineServiceStatelessEventType.RecordUpdated,
                    dataRow: row,
                    transactionId: transactionId
                );
                break;
            }

            case DataRowState.Deleted:
            {
                ExecuteStatelessEvents(
                    entityId: entityId,
                    eventType: StateMachineServiceStatelessEventType.RecordDeleted,
                    dataRow: row,
                    transactionId: transactionId
                );
                break;
            }
        }
    }

    private bool ExecutePostEvents(
        StateMachine sm,
        object currentStateValue,
        object newStateValue,
        DataRow dataRow,
        IXmlContainer data,
        string transactionId
    )
    {
        object rowKey = null;
        object[] keys = DatasetTools.PrimaryKey(row: dataRow);
        if (keys.Length == 1)
        {
            rowKey = keys[0];
        }
        // check if state transition is allowed
        if (
            !IsStateAllowed(
                entityId: sm.EntityId,
                fieldId: sm.FieldId,
                currentStateValue: currentStateValue,
                newStateValue: newStateValue,
                data: data,
                transactionId: transactionId
            )
        )
        {
            ThrowStateTransitionInvalidException(
                sm: sm,
                currentStateValue: currentStateValue,
                newStateValue: newStateValue,
                transactionId: transactionId
            );
        }
        ExecutePostWorkQueue(
            sm: sm,
            eventType: StateMachineServiceStatelessEventType.RecordUpdated,
            newStateValue: newStateValue,
            dataRow: dataRow,
            data: data,
            transactionId: transactionId
        );
        foreach (StateMachineEvent ev in sm.Events)
        {
            if (IsEventAllowed(ev: ev))
            {
                if (
                    ev.Type == StateMachineEventType.StateEntry
                    && ev.OldState == null
                    && ev.NewState.IsState(value: newStateValue)
                    && !ev.NewState.IsState(value: currentStateValue)
                )
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                            message: "Will execute event: " + ev.Type.ToString() + ", " + ev.Name
                        );
                    }
                    ExecuteWorkflow(
                        workflow: ev.Action,
                        ev: ev,
                        dataRow: dataRow,
                        transactionId: transactionId
                    );
                }
            }
        }
        return false;
    }

    private void ThrowStateTransitionInvalidException(
        StateMachine sm,
        object currentStateValue,
        object newStateValue,
        string transactionId
    )
    {
        string newStateName = StateValueName(
            sm: sm,
            stateValue: newStateValue,
            transactionId: transactionId
        );
        string currentStateName = StateValueName(
            sm: sm,
            stateValue: currentStateValue,
            transactionId: transactionId
        );
        throw new Exception(
            message: ResourceUtils.GetString(
                key: "ErrorChangeState",
                args: new object[]
                {
                    sm.Field.Caption,
                    (sm.Entity.Caption == "" ? sm.Entity.Name : sm.Entity.Caption),
                    newStateName,
                    currentStateName,
                }
            )
        );
    }

    private void ExecutePostWorkQueue(
        StateMachine sm,
        StateMachineServiceStatelessEventType eventType,
        object newStateValue,
        DataRow dataRow,
        IXmlContainer data,
        string transactionId
    )
    {
        object rowKey = null;
        object[] keys = DatasetTools.PrimaryKey(row: dataRow);
        if (keys.Length == 1)
        {
            rowKey = keys[0];
        }
        // process post events work queue
        IParameterService ps =
            ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
            as IParameterService;
        string newStateValueString = XmlTools.ConvertToString(val: newStateValue);
        // creation
        foreach (
            WorkQueueData.WorkQueueRow wq in WorkQueuesCreation(
                workQueueList: WorkQueueList(transactionId: transactionId),
                eventType: eventType,
                entityId: sm.EntityId,
                row: dataRow
            )
        )
        {
            if (
                wq.refCreationOrigamStateMachineEventTypeId.Equals(
                    o: ps.GetParameterValue(parameterName: "OrigamStateMachineEventType_StateEntry")
                )
            )
            {
                if (wq.IsCreationNewValueNull())
                {
                    throw new NullReferenceException(
                        message: ResourceUtils.GetString(
                            key: "ErrorInvalidWorkQueueDefinition0",
                            args: wq.Name
                        )
                    );
                }

                if (
                    sm.Field != null
                    && sm.Field.Name.Equals(value: wq.CreationFieldName)
                    && ReverseLookupWorkQueueFieldValue(
                            sm: sm,
                            wq: wq,
                            originalValue: wq.CreationNewValue,
                            transactionId: transactionId
                        )
                        .Equals(value: newStateValueString)
                )
                {
                    // write to queue
                    WQService.WorkQueueAdd(
                        workQueueClassIdentifier: wq.WorkQueueClass,
                        workQueueName: wq.Name,
                        workQueueId: wq.Id,
                        condition: wq.IsCreationConditionNull() ? null : wq.CreationCondition,
                        data: data,
                        transactionId: transactionId
                    );
                }
            }
        }
        // removal
        foreach (
            WorkQueueData.WorkQueueRow wq in WorkQueuesRemoval(
                workQueueList: WorkQueueList(transactionId: transactionId),
                eventType: eventType,
                entityId: sm.EntityId,
                row: dataRow
            )
        )
        {
            if (
                wq.refRemovalOrigamStateMachineEventTypeId.Equals(
                    o: ps.GetParameterValue(parameterName: "OrigamStateMachineEventType_StateEntry")
                )
            )
            {
                if (wq.IsRemovalNewValueNull())
                {
                    throw new NullReferenceException(
                        message: ResourceUtils.GetString(
                            key: "ErrorInvalidWorkQueueDefinition3",
                            args: wq.Name
                        )
                    );
                }

                if (
                    sm.Field != null
                    && sm.Field.Name.Equals(value: wq.RemovalFieldName)
                    && ReverseLookupWorkQueueFieldValue(
                            sm: sm,
                            wq: wq,
                            originalValue: wq.RemovalNewValue,
                            transactionId: transactionId
                        )
                        .Equals(value: newStateValueString)
                )
                {
                    // remove from queue
                    WQService.WorkQueueRemove(
                        workQueueClassIdentifier: wq.WorkQueueClass,
                        workQueueName: wq.Name,
                        workQueueId: wq.Id,
                        condition: wq.IsRemovalConditionNull() ? null : wq.RemovalCondition,
                        rowKey: rowKey,
                        transactionId: transactionId
                    );
                }
            }
        }
    }

    private string ReverseLookupWorkQueueFieldValue(
        StateMachine sm,
        WorkQueueData.WorkQueueRow wq,
        string originalValue,
        string transactionId
    )
    {
        // convert the user entry to lower-case in case it is a guid because
        // often the strings are upper-case as returned by an SQL Server
        if (originalValue != null && sm.Field.DataType == OrigamDataType.UniqueIdentifier)
        {
            originalValue = originalValue.ToLower();
        }
        string result = originalValue;
        if (wq.ReverseLookupFieldValues)
        {
            if (sm.ReverseLookup == null)
            {
                throw new NullReferenceException(
                    message: string.Format(
                        format: ResourceUtils.GetString(key: "ErrorReverseLookupNotSet"),
                        arg0: wq.Name
                    )
                );
            }
            IDataLookupService lookupManager =
                ServiceManager.Services.GetService(serviceType: typeof(IDataLookupService))
                as IDataLookupService;
            result = lookupManager
                .GetDisplayText(
                    lookupId: sm.ReverseLookupId,
                    lookupValue: originalValue,
                    useCache: false,
                    returnMessageIfNull: false,
                    transactionId: transactionId
                )
                .ToString();
        }
        if (result == null)
        {
            throw new NullReferenceException(
                message: ResourceUtils.GetString(
                    key: "ErrorInvalidWorkQueueDefinition0",
                    args: wq.Name
                )
            );
        }
        return result;
    }

    public bool ExecutePreEvents(
        Guid entityId,
        Guid fieldId,
        object currentStateValue,
        object newStateValue,
        DataRow dataRow,
        string transactionId
    )
    {
        StateMachine sm = GetMachine(entityId: entityId, fieldId: fieldId, throwException: true);
        IXmlContainer data = DatasetTools.GetRowXml(row: dataRow, version: DataRowVersion.Default);
        object rowKey = null;
        object[] keys = DatasetTools.PrimaryKey(row: dataRow);
        if (keys.Length == 1)
        {
            rowKey = keys[0];
        }

        if (
            !IsStateAllowed(
                entityId: entityId,
                fieldId: fieldId,
                currentStateValue: currentStateValue,
                newStateValue: newStateValue,
                data: data,
                transactionId: transactionId
            )
        )
        {
            ThrowStateTransitionInvalidException(
                sm: sm,
                currentStateValue: currentStateValue,
                newStateValue: newStateValue,
                transactionId: transactionId
            );
        }
        ExecutePreWorkQueue(
            entityId: entityId,
            currentStateValue: currentStateValue,
            newStateValue: newStateValue,
            dataRow: dataRow,
            transactionId: transactionId,
            sm: sm,
            data: data,
            rowKey: rowKey
        );
        foreach (StateMachineEvent ev in sm.Events)
        {
            if (IsEventAllowed(ev: ev))
            {
                if (
                    (
                        ev.Type == StateMachineEventType.StateExit
                        && ev.NewState == null
                        && ev.OldState.IsState(value: currentStateValue)
                    )
                    || (
                        ev.Type == StateMachineEventType.StateTransition
                        && ev.OldState.IsState(value: currentStateValue)
                        && ev.NewState.IsState(value: newStateValue)
                    )
                )
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                            message: "Will execute event: " + ev.Type.ToString() + ", " + ev.Name
                        );
                    }
                    ExecuteWorkflow(
                        workflow: ev.Action,
                        ev: ev,
                        dataRow: dataRow,
                        transactionId: transactionId
                    );
                }
            }
        }
        return false;
    }

    private void ExecutePreWorkQueue(
        Guid entityId,
        object currentStateValue,
        object newStateValue,
        DataRow dataRow,
        string transactionId,
        StateMachine sm,
        IXmlContainer data,
        object rowKey
    )
    {
        IParameterService ps =
            ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
            as IParameterService;
        string newStateValueString = XmlTools.ConvertToString(val: newStateValue);
        string oldStateValueString = XmlTools.ConvertToString(val: currentStateValue);
        // CREATION
        foreach (
            WorkQueueData.WorkQueueRow wq in WorkQueuesCreation(
                workQueueList: WorkQueueList(transactionId: transactionId),
                eventType: StateMachineServiceStatelessEventType.RecordUpdated,
                entityId: entityId,
                row: dataRow
            )
        )
        {
            // creation - state exit
            if (
                wq.refCreationOrigamStateMachineEventTypeId.Equals(
                    o: ps.GetParameterValue(parameterName: "OrigamStateMachineEventType_StateExit")
                )
            )
            {
                if (wq.IsCreationOldValueNull())
                {
                    throw new NullReferenceException(
                        message: ResourceUtils.GetString(
                            key: "ErrorInvalidWorkQueueDefinition1",
                            args: wq.Name
                        )
                    );
                }

                if (
                    sm.Field != null
                    && sm.Field.Name.Equals(value: wq.CreationFieldName)
                    && ReverseLookupWorkQueueFieldValue(
                            sm: sm,
                            wq: wq,
                            originalValue: wq.CreationOldValue,
                            transactionId: transactionId
                        )
                        .Equals(value: oldStateValueString)
                )
                {
                    // write to queue
                    WQService.WorkQueueAdd(
                        workQueueClassIdentifier: wq.WorkQueueClass,
                        workQueueName: wq.Name,
                        workQueueId: wq.Id,
                        condition: wq.IsCreationConditionNull() ? null : wq.CreationCondition,
                        data: data,
                        transactionId: transactionId
                    );
                }
            }
            // creation - state transition
            if (
                wq.refCreationOrigamStateMachineEventTypeId.Equals(
                    o: ps.GetParameterValue(
                        parameterName: "OrigamStateMachineEventType_StateTransition"
                    )
                )
            )
            {
                if (wq.IsCreationOldValueNull())
                {
                    throw new NullReferenceException(
                        message: ResourceUtils.GetString(
                            key: "ErrorInvalidWorkQueueDefinition2",
                            args: wq.Name
                        )
                    );
                }

                if (wq.IsCreationNewValueNull())
                {
                    throw new NullReferenceException(
                        message: ResourceUtils.GetString(
                            key: "ErrorInvalidWorkQueueDefinition4",
                            args: wq.Name
                        )
                    );
                }

                if (sm.Field != null && sm.Field.Name.Equals(value: wq.CreationFieldName))
                {
                    string oldValue = ReverseLookupWorkQueueFieldValue(
                        sm: sm,
                        wq: wq,
                        originalValue: wq.CreationOldValue,
                        transactionId: transactionId
                    );
                    string newValue = ReverseLookupWorkQueueFieldValue(
                        sm: sm,
                        wq: wq,
                        originalValue: wq.CreationNewValue,
                        transactionId: transactionId
                    );
                    if (
                        oldValue.Equals(value: oldStateValueString)
                        && newValue.Equals(value: newStateValueString)
                    )
                    {
                        // write to queue
                        WQService.WorkQueueAdd(
                            workQueueClassIdentifier: wq.WorkQueueClass,
                            workQueueName: wq.Name,
                            workQueueId: wq.Id,
                            condition: wq.IsCreationConditionNull() ? null : wq.CreationCondition,
                            data: data,
                            transactionId: transactionId
                        );
                    }
                }
            }
        }
        // REMOVAL
        foreach (
            WorkQueueData.WorkQueueRow wq in WorkQueuesRemoval(
                workQueueList: WorkQueueList(transactionId: transactionId),
                eventType: StateMachineServiceStatelessEventType.RecordUpdated,
                entityId: entityId,
                row: dataRow
            )
        )
        {
            // removal - state exit
            if (
                wq.refRemovalOrigamStateMachineEventTypeId.Equals(
                    o: ps.GetParameterValue(parameterName: "OrigamStateMachineEventType_StateExit")
                )
            )
            {
                if (wq.IsRemovalOldValueNull())
                {
                    throw new NullReferenceException(
                        message: ResourceUtils.GetString(
                            key: "ErrorInvalidWorkQueueDefinition5",
                            args: wq.Name
                        )
                    );
                }

                if (
                    sm.Field != null
                    && sm.Field.Name.Equals(value: wq.RemovalFieldName)
                    && ReverseLookupWorkQueueFieldValue(
                            sm: sm,
                            wq: wq,
                            originalValue: wq.RemovalOldValue,
                            transactionId: transactionId
                        )
                        .Equals(value: oldStateValueString)
                )
                {
                    // remove from queue
                    WQService.WorkQueueRemove(
                        workQueueClassIdentifier: wq.WorkQueueClass,
                        workQueueName: wq.Name,
                        workQueueId: wq.Id,
                        condition: wq.IsRemovalConditionNull() ? null : wq.RemovalCondition,
                        rowKey: rowKey,
                        transactionId: transactionId
                    );
                }
            }
            // removal - state transition
            if (
                wq.refRemovalOrigamStateMachineEventTypeId.Equals(
                    o: ps.GetParameterValue(
                        parameterName: "OrigamStateMachineEventType_StateTransition"
                    )
                )
            )
            {
                if (wq.IsRemovalOldValueNull())
                {
                    throw new NullReferenceException(
                        message: ResourceUtils.GetString(
                            key: "ErrorInvalidWorkQueueDefinition6",
                            args: wq.Name
                        )
                    );
                }

                if (wq.IsRemovalNewValueNull())
                {
                    throw new NullReferenceException(
                        message: ResourceUtils.GetString(
                            key: "ErrorInvalidWorkQueueDefinition7",
                            args: wq.Name
                        )
                    );
                }

                if (sm.Field != null && sm.Field.Name.Equals(value: wq.RemovalFieldName))
                {
                    string oldValue = ReverseLookupWorkQueueFieldValue(
                        sm: sm,
                        wq: wq,
                        originalValue: wq.RemovalOldValue,
                        transactionId: transactionId
                    );
                    string newValue = ReverseLookupWorkQueueFieldValue(
                        sm: sm,
                        wq: wq,
                        originalValue: wq.RemovalNewValue,
                        transactionId: transactionId
                    );
                    if (
                        oldValue.Equals(value: oldStateValueString)
                        && newValue.Equals(value: newStateValueString)
                    )
                    {
                        // remove from queue
                        WQService.WorkQueueRemove(
                            workQueueClassIdentifier: wq.WorkQueueClass,
                            workQueueName: wq.Name,
                            workQueueId: wq.Id,
                            condition: wq.IsRemovalConditionNull() ? null : wq.RemovalCondition,
                            rowKey: rowKey,
                            transactionId: transactionId
                        );
                    }
                }
            }
        }
    }

    public bool ExecuteStatelessEvents(
        Guid entityId,
        StateMachineServiceStatelessEventType eventType,
        DataRow dataRow,
        string transactionId
    )
    {
        List<StateMachine> stateMachines = GetMachines(entityId: entityId, throwException: false);
        var eventsSorted = new List<StateMachineEvent>();
        object rowKey = null;
        object[] keys = DatasetTools.PrimaryKey(row: dataRow);
        if (keys.Length == 1)
        {
            rowKey = keys[0];
        }

        WorkQueueData workQueueList = WorkQueueList(transactionId: transactionId);
        // first remove any work queue entries for this record since we are deleting a record
        // after that new entries may be created if they are configured to be created OnRecordDeleted
        if (eventType == StateMachineServiceStatelessEventType.RecordDeleted)
        {
            foreach (WorkQueueData.WorkQueueRow wqr in workQueueList.WorkQueue.Rows)
            {
                WorkQueueClass wqc = WQClass(name: wqr.WorkQueueClass);
                if (wqc != null && wqc.EntityId.Equals(g: entityId))
                {
                    WQService.WorkQueueRemove(
                        workQueueClassIdentifier: wqr.WorkQueueClass,
                        workQueueName: wqr.Name,
                        workQueueId: wqr.Id,
                        condition: null,
                        rowKey: rowKey,
                        transactionId: transactionId
                    );
                }
            }
        }
        ExecuteStatelessWorkQueue(
            entityId: entityId,
            eventType: eventType,
            dataRow: dataRow,
            transactionId: transactionId,
            rowKey: rowKey,
            workQueueList: workQueueList
        );
        foreach (StateMachine sm in stateMachines)
        {
            eventsSorted.AddRange(collection: sm.Events);
        }
        eventsSorted.Sort();
        foreach (StateMachineEvent ev in eventsSorted)
        {
            if (IsEventAllowed(ev: ev))
            {
                if (
                    (
                        ev.Type == StateMachineEventType.RecordCreated
                        && eventType == StateMachineServiceStatelessEventType.RecordCreated
                    )
                    || (
                        ev.Type == StateMachineEventType.RecordUpdated
                        && eventType == StateMachineServiceStatelessEventType.RecordUpdated
                    )
                    || (
                        ev.Type == StateMachineEventType.RecordCreatedUpdated
                        && eventType == StateMachineServiceStatelessEventType.RecordCreated
                    )
                    || (
                        ev.Type == StateMachineEventType.RecordCreatedUpdated
                        && eventType == StateMachineServiceStatelessEventType.RecordUpdated
                    )
                    || (
                        ev.Type == StateMachineEventType.RecordDeleted
                        && eventType == StateMachineServiceStatelessEventType.RecordDeleted
                    )
                    || (
                        ev.Type == StateMachineEventType.BeforeRecordDeleted
                        && eventType == StateMachineServiceStatelessEventType.BeforeRecordDeleted
                    )
                )
                {
                    if (FieldsChanged(fields: ev.FieldDependencies, row: dataRow, type: eventType))
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(
                                message: "Will execute event: "
                                    + ev.Type.ToString()
                                    + ", "
                                    + ev.Name
                            );
                        }
                        ExecuteWorkflow(
                            workflow: ev.Action,
                            ev: ev,
                            dataRow: dataRow,
                            transactionId: transactionId
                        );
                    }
                }
            }
        }
        return false;
    }

    private static void ExecuteStatelessWorkQueue(
        Guid entityId,
        StateMachineServiceStatelessEventType eventType,
        DataRow dataRow,
        string transactionId,
        object rowKey,
        WorkQueueData workQueueList
    )
    {
        if (workQueueList.WorkQueue.Rows.Count > 0)
        {
            IParameterService ps =
                ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
                as IParameterService;
            // creation
            WorkQueueData.WorkQueueRow[] creationQueues = WorkQueuesCreation(
                workQueueList: workQueueList,
                eventType: eventType,
                entityId: entityId,
                row: dataRow
            );
            if (creationQueues.Length > 0)
            {
                XmlContainer data = DatasetTools.GetRowXml(
                    row: dataRow,
                    version: eventType == StateMachineServiceStatelessEventType.RecordDeleted
                    || eventType == StateMachineServiceStatelessEventType.BeforeRecordDeleted
                        ? DataRowVersion.Original
                        : DataRowVersion.Default
                );
                foreach (WorkQueueData.WorkQueueRow wq in creationQueues)
                {
                    if (
                        (
                            wq.refCreationOrigamStateMachineEventTypeId.Equals(
                                o: ps.GetParameterValue(
                                    parameterName: "OrigamStateMachineEventType_RecordCreated"
                                )
                            )
                            && eventType == StateMachineServiceStatelessEventType.RecordCreated
                        )
                        || (
                            wq.refCreationOrigamStateMachineEventTypeId.Equals(
                                o: ps.GetParameterValue(
                                    parameterName: "OrigamStateMachineEventType_RecordUpdated"
                                )
                            )
                            && eventType == StateMachineServiceStatelessEventType.RecordUpdated
                        )
                        || (
                            wq.refCreationOrigamStateMachineEventTypeId.Equals(
                                o: ps.GetParameterValue(
                                    parameterName: "OrigamStateMachineEventType_RecordDeleted"
                                )
                            )
                            && eventType == StateMachineServiceStatelessEventType.RecordDeleted
                        )
                        || (
                            wq.refCreationOrigamStateMachineEventTypeId.Equals(
                                o: ps.GetParameterValue(
                                    parameterName: "OrigamStateMachineEventType_ValueUpdated"
                                )
                            )
                            && eventType == StateMachineServiceStatelessEventType.RecordUpdated
                        )
                        || (
                            wq.refCreationOrigamStateMachineEventTypeId.Equals(
                                o: ps.GetParameterValue(
                                    parameterName: "OrigamStateMachineEventType_ValueUpdated"
                                )
                            )
                            && eventType == StateMachineServiceStatelessEventType.RecordCreated
                        )
                    )
                    {
                        // write to queue
                        WQService.WorkQueueAdd(
                            workQueueClassIdentifier: wq.WorkQueueClass,
                            workQueueName: wq.Name,
                            workQueueId: wq.Id,
                            condition: wq.IsCreationConditionNull() ? null : wq.CreationCondition,
                            data: data,
                            transactionId: transactionId
                        );
                    }
                }
            }
            if (eventType == StateMachineServiceStatelessEventType.RecordUpdated)
            {
                // update all the queue entries related to this entity
                foreach (WorkQueueData.WorkQueueRow wq in workQueueList.WorkQueue.Rows)
                {
                    WorkQueueClass wqc = WQClass(name: wq.WorkQueueClass);
                    if (wqc != null)
                    {
                        if (entityId.Equals(g: wqc.EntityId))
                        {
                            WQService.WorkQueueUpdate(
                                workQueueClassIdentifier: wq.WorkQueueClass,
                                relationNo: 0,
                                workQueueId: wq.Id,
                                rowKey: rowKey,
                                transactionId: transactionId
                            );
                        }
                        if (entityId.Equals(g: wqc.RelatedEntity1Id))
                        {
                            WQService.WorkQueueUpdate(
                                workQueueClassIdentifier: wq.WorkQueueClass,
                                relationNo: 1,
                                workQueueId: wq.Id,
                                rowKey: rowKey,
                                transactionId: transactionId
                            );
                        }
                        if (entityId.Equals(g: wqc.RelatedEntity2Id))
                        {
                            WQService.WorkQueueUpdate(
                                workQueueClassIdentifier: wq.WorkQueueClass,
                                relationNo: 2,
                                workQueueId: wq.Id,
                                rowKey: rowKey,
                                transactionId: transactionId
                            );
                        }
                        if (entityId.Equals(g: wqc.RelatedEntity3Id))
                        {
                            WQService.WorkQueueUpdate(
                                workQueueClassIdentifier: wq.WorkQueueClass,
                                relationNo: 3,
                                workQueueId: wq.Id,
                                rowKey: rowKey,
                                transactionId: transactionId
                            );
                        }
                        if (entityId.Equals(g: wqc.RelatedEntity4Id))
                        {
                            WQService.WorkQueueUpdate(
                                workQueueClassIdentifier: wq.WorkQueueClass,
                                relationNo: 4,
                                workQueueId: wq.Id,
                                rowKey: rowKey,
                                transactionId: transactionId
                            );
                        }
                        if (entityId.Equals(g: wqc.RelatedEntity5Id))
                        {
                            WQService.WorkQueueUpdate(
                                workQueueClassIdentifier: wq.WorkQueueClass,
                                relationNo: 5,
                                workQueueId: wq.Id,
                                rowKey: rowKey,
                                transactionId: transactionId
                            );
                        }
                        if (entityId.Equals(g: wqc.RelatedEntity6Id))
                        {
                            WQService.WorkQueueUpdate(
                                workQueueClassIdentifier: wq.WorkQueueClass,
                                relationNo: 6,
                                workQueueId: wq.Id,
                                rowKey: rowKey,
                                transactionId: transactionId
                            );
                        }
                        if (entityId.Equals(g: wqc.RelatedEntity7Id))
                        {
                            WQService.WorkQueueUpdate(
                                workQueueClassIdentifier: wq.WorkQueueClass,
                                relationNo: 7,
                                workQueueId: wq.Id,
                                rowKey: rowKey,
                                transactionId: transactionId
                            );
                        }
                    }
                }
            }
            // removal (as last because to evaluate eventual XPath conditions
            // records have to be updated before deleting them
            WorkQueueData.WorkQueueRow[] removalQueues = WorkQueuesRemoval(
                workQueueList: workQueueList,
                eventType: eventType,
                entityId: entityId,
                row: dataRow
            );
            foreach (WorkQueueData.WorkQueueRow wq in removalQueues)
            {
                if (!wq.IsrefRemovalOrigamStateMachineEventTypeIdNull())
                {
                    if (
                        (
                            wq.refRemovalOrigamStateMachineEventTypeId.Equals(
                                o: ps.GetParameterValue(
                                    parameterName: "OrigamStateMachineEventType_RecordCreated"
                                )
                            )
                            && eventType == StateMachineServiceStatelessEventType.RecordCreated
                        )
                        || (
                            wq.refRemovalOrigamStateMachineEventTypeId.Equals(
                                o: ps.GetParameterValue(
                                    parameterName: "OrigamStateMachineEventType_RecordUpdated"
                                )
                            )
                            && eventType == StateMachineServiceStatelessEventType.RecordUpdated
                        )
                        || (
                            wq.refRemovalOrigamStateMachineEventTypeId.Equals(
                                o: ps.GetParameterValue(
                                    parameterName: "OrigamStateMachineEventType_RecordDeleted"
                                )
                            )
                            && eventType == StateMachineServiceStatelessEventType.RecordDeleted
                        )
                        || (
                            wq.refRemovalOrigamStateMachineEventTypeId.Equals(
                                o: ps.GetParameterValue(
                                    parameterName: "OrigamStateMachineEventType_ValueUpdated"
                                )
                            )
                            && eventType == StateMachineServiceStatelessEventType.RecordUpdated
                        )
                        || (
                            wq.refRemovalOrigamStateMachineEventTypeId.Equals(
                                o: ps.GetParameterValue(
                                    parameterName: "OrigamStateMachineEventType_ValueUpdated"
                                )
                            )
                            && eventType == StateMachineServiceStatelessEventType.RecordCreated
                        )
                    )
                    {
                        // remove from queue
                        WQService.WorkQueueRemove(
                            workQueueClassIdentifier: wq.WorkQueueClass,
                            workQueueName: wq.Name,
                            workQueueId: wq.Id,
                            condition: wq.IsRemovalConditionNull() ? null : wq.RemovalCondition,
                            rowKey: rowKey,
                            transactionId: transactionId
                        );
                    }
                }
            }
        }
    }
    #endregion
    /// <summary>
    /// Test if any of the field dependencies were updated in the data.
    /// </summary>
    /// <param name="fields">List of fields to test.</param>
    /// <param name="row">Data row with the data on which the test will be done.</param>
    /// <param name="type">State machine event type.</param>
    /// <returns>True if the field changed (on update) or if the field has a value (on create).
    /// For deletes this function will always return True because on delete operation basically
    /// all fields were changed, no matter if there are any dependencies or not. Also if there are
    /// no dependencies True is always returned.</returns>
    private bool FieldsChanged(
        List<StateMachineEventFieldDependency> fields,
        DataRow row,
        StateMachineServiceStatelessEventType type
    )
    {
        // if there are no field dependencies all fields changed
        if (fields.Count == 0)
        {
            return true;
        }
        // for delete operation all fields changed
        if (
            type == StateMachineServiceStatelessEventType.BeforeRecordDeleted
            && type == StateMachineServiceStatelessEventType.RecordDeleted
        )
        {
            return true;
        }
        foreach (StateMachineEventFieldDependency dependency in fields)
        {
            if (FieldChanged(row: row, eventType: type, fieldName: dependency.Field.Name))
            {
                return true;
            }
        }
        return false;
    }

    private static bool FieldChanged(
        DataRow row,
        StateMachineServiceStatelessEventType eventType,
        string fieldName
    )
    {
        if (row.Table.Columns.Contains(name: fieldName))
        {
            // when new record we test if the value is not empty, then we execute the events
            if (
                row.RowState == DataRowState.Added
                || eventType == StateMachineServiceStatelessEventType.RecordCreated
            )
            {
                if (
                    !row[columnName: fieldName, version: DataRowVersion.Default]
                        .Equals(obj: DBNull.Value)
                )
                {
                    return true;
                }
            }
            else if (
                row.RowState == DataRowState.Modified
                && !row[columnName: fieldName, version: DataRowVersion.Original]
                    .Equals(obj: row[columnName: fieldName, version: DataRowVersion.Current])
            )
            {
                return true;
            }
        }
        else
        {
            Guid entityId = (Guid)row.Table.ExtendedProperties[key: "Id"];
            throw new ArgumentOutOfRangeException(
                paramName: string.Format(
                    format: "Data Structure Entity '{0}' ({1}) does not contain field '{2}'. Cannot evaluate state machine field conditions.",
                    args: new object[]
                    {
                        row.Table.TableName,
                        entityId,
                        fieldName,
                        row.Table.TableName,
                    }
                )
            );
        }
        return false;
    }

    private StateMachine GetMachine(Guid entityId, Guid fieldId, bool throwException)
    {
        StateMachineSchemaItemProvider stateMachines =
            (
                ServiceManager.Services.GetService(serviceType: typeof(SchemaService))
                as SchemaService
            ).GetProvider(type: typeof(StateMachineSchemaItemProvider))
            as StateMachineSchemaItemProvider;
        if (stateMachines == null)
        {
            return null;
        }
        StateMachine sm = stateMachines.GetMachine(entityId: entityId, fieldId: fieldId);
        if (sm == null && throwException)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "fieldId",
                actualValue: fieldId,
                message: ResourceUtils.GetString(key: "ErrorStateMachineNotFound")
            );
        }

        return sm;
    }

    private List<StateMachine> GetMachines(Guid entityId, bool throwException)
    {
        StateMachineSchemaItemProvider stateMachines =
            (
                ServiceManager.Services.GetService(serviceType: typeof(SchemaService))
                as SchemaService
            ).GetProvider(type: typeof(StateMachineSchemaItemProvider))
            as StateMachineSchemaItemProvider;
        if (stateMachines == null)
        {
            return null;
        }
        List<StateMachine> result = stateMachines.GetMachines(entityId: entityId);
        if (result.Count == 0 && throwException)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "entityId",
                actualValue: entityId,
                message: ResourceUtils.GetString(key: "ErrorStateMachineNotFound")
            );
        }

        return result;
    }

    private string StateValueName(StateMachine sm, object stateValue, string transactionId)
    {
        IDataLookupService lookupManager =
            ServiceManager.Services.GetService(serviceType: typeof(IDataLookupService))
            as IDataLookupService;
        if (sm.Field.DefaultLookup == null)
        {
            return stateValue.ToString();
        }

        object result = lookupManager.GetDisplayText(
            lookupId: (Guid)sm.Field.DefaultLookup.PrimaryKey[key: "Id"],
            lookupValue: stateValue,
            transactionId: transactionId
        );
        return result.ToString();
    }

    private bool IsOperationAllowed(
        StateMachineOperation operation,
        IXmlContainer data,
        string transactionId
    )
    {
        // check features
        IParameterService param =
            ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
            as IParameterService;
        if (!param.IsFeatureOn(featureCode: operation.Features))
        {
            return false;
        }
        // check roles
        IOrigamAuthorizationProvider authorizationProvider =
            SecurityManager.GetAuthorizationProvider();
        if (
            !authorizationProvider.Authorize(
                principal: SecurityManager.CurrentPrincipal,
                context: operation.Roles
            )
        )
        {
            return false;
        }
        // check business rule
        if (operation.Rule != null)
        {
            RuleEngine ruleEngine = RuleEngine.Create(
                contextStores: new Hashtable(),
                transactionId: transactionId
            );
            //				ruleEngine.TransactionId = transactionId;
            object result = ruleEngine.EvaluateRule(
                rule: operation.Rule,
                data: data,
                contextPosition: null
            );
            if (result is bool)
            {
                return (bool)result;
            }

            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorResultNotBool", args: operation.Path)
            );
        }
        return true;
    }

    private bool IsEventAllowed(StateMachineEvent ev)
    {
        IOrigamAuthorizationProvider authorizationProvider =
            SecurityManager.GetAuthorizationProvider();
        if (
            !authorizationProvider.Authorize(
                principal: SecurityManager.CurrentPrincipal,
                context: ev.Roles
            )
        )
        {
            return false;
        }
        IParameterService param =
            ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
            as IParameterService;
        return param.IsFeatureOn(featureCode: ev.Features);
    }

    private void ExecuteWorkflow(
        IWorkflow workflow,
        StateMachineEvent ev,
        DataRow dataRow,
        string transactionId
    )
    {
        if (transactionId == null)
        {
            throw new ArgumentNullException(
                paramName: "transactionId",
                message: ResourceUtils.GetString(key: "ErrorNoTransaction")
            );
        }
        if (workflow == null)
        {
            throw new ArgumentNullException(
                paramName: "workflow",
                message: ResourceUtils.GetString(key: "ErrorWorkflowUnspecified", args: ev.Path)
            );
        }
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "State machine is starting workflow: " + workflow.Name);
        }
        WorkflowEngine engine = new WorkflowEngine(transactionId: transactionId);
        engine.PersistenceProvider = _persistence.SchemaProvider;
        engine.WorkflowBlock = workflow;
        foreach (StateMachineEventParameterMapping mapping in ev.ParameterMappings)
        {
            DataRowVersion version = DatasetTools.GetRowVersion(
                row: dataRow,
                originalData: mapping.Type == WorkflowEntityParameterMappingType.Original
            );
            if (mapping.Field == null)
            {
                if (mapping.Type == WorkflowEntityParameterMappingType.ChangedFlag)
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "Type",
                        actualValue: mapping.Type,
                        message: "Field not specified for state machine parameter mapping '"
                            + mapping.Path
                            + "'"
                    );
                }
                // no field, we pass the whole context
                engine.InputContexts.Add(
                    key: mapping.ContextStore.PrimaryKey,
                    value: DatasetTools.GetRowXml(row: dataRow, version: version)
                );
            }
            else
            {
                // field specified, we pass the field value
                bool found = false;
                foreach (DataColumn column in dataRow.Table.Columns)
                {
                    if (
                        column.ExtendedProperties.Contains(key: "Id")
                        && !column.ExtendedProperties.Contains(
                            key: Const.IsAddittionalFieldAttribute
                        )
                        && column.ExtendedProperties[key: "Id"].Equals(obj: mapping.FieldId)
                    )
                    {
                        // context already contains the value
                        if (engine.InputContexts.Contains(key: mapping.ContextStore.PrimaryKey))
                        {
                            if (
                                !engine
                                    .InputContexts[key: mapping.ContextStore.PrimaryKey]
                                    .Equals(obj: dataRow[column: column, version: version])
                            )
                            {
                                throw new InvalidOperationException(
                                    message: ResourceUtils.GetString(
                                        key: "ErrorTwoValuesOneContext0",
                                        args: mapping.ContextStore.PrimaryKey.ToString()
                                    )
                                        + Environment.NewLine
                                        + ResourceUtils.GetString(key: "ErrorTwoValuesOneContext1")
                                );
                            }
                        }
                        else
                        {
                            if (mapping.Type == WorkflowEntityParameterMappingType.ChangedFlag)
                            {
                                DataRowVersion currentVersion = DatasetTools.GetRowVersion(
                                    row: dataRow,
                                    originalData: false
                                );
                                DataRowVersion originalVersion = DatasetTools.GetRowVersion(
                                    row: dataRow,
                                    originalData: true
                                );
                                object currentValue = dataRow[
                                    column: column,
                                    version: currentVersion
                                ];
                                object originalValue = dataRow[
                                    column: column,
                                    version: originalVersion
                                ];
                                engine.InputContexts.Add(
                                    key: mapping.ContextStore.PrimaryKey,
                                    value: !currentValue.Equals(obj: originalValue)
                                );
                            }
                            else
                            {
                                engine.InputContexts.Add(
                                    key: mapping.ContextStore.PrimaryKey,
                                    value: dataRow[column: column, version: version]
                                );
                            }
                        }
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "Field",
                        actualValue: mapping.Field.Name,
                        message: "Field mapped to a workflow context not found in the provided data row. Cannot execute workflow."
                    );
                }
            }
        }
        WorkflowHost.DefaultHost.ExecuteWorkflow(engine: engine);
        if (engine.WorkflowException != null)
        {
            throw engine.WorkflowException;
        }

        if (log.IsDebugEnabled)
        {
            log.Debug(message: "State machine workflow call finished: " + workflow.Name);
        }
    }

    private static WorkQueueData WorkQueueList(string transactionId)
    {
        if (_WorkQueueLastRefreshed.AddMinutes(value: 1).CompareTo(value: DateTime.Now) < 0)
        {
            _WorkQueueCache = (WQService as WorkQueueService).GetQueues(
                ignoreQueueProcessors: true,
                transactionId: transactionId
            );
            _WorkQueueLastRefreshed = DateTime.Now;
        }
        return _WorkQueueCache;
    }

    private static bool AnyWorkQueueClassBasedOnEntity(Guid entityId)
    {
        return ServiceManager
            .Services.GetService<SchemaService>()
            .GetProvider<WorkQueueClassSchemaItemProvider>()
            .ChildItemsByType<WorkQueueClass>(itemType: "WorkQueueClass")
            .Any(predicate: workQueueClass => workQueueClass.EntityId == entityId);
    }

    private static bool AnyStateMachineBasedOnEntity(Guid entityId)
    {
        return ServiceManager
            .Services.GetService<SchemaService>()
            .GetProvider<StateMachineSchemaItemProvider>()
            .ChildItemsByType<StateMachine>(itemType: "WorkflowStateMachine")
            .Any(predicate: stateMachine => stateMachine.EntityId == entityId);
    }

    private static WorkQueueClass WQClass(string name)
    {
        return (WorkQueueClass)WQService.WQClass(name: name);
    }

    private static IWorkQueueService WQService
    {
        get
        {
            return ServiceManager.Services.GetService(serviceType: typeof(IWorkQueueService))
                as IWorkQueueService;
        }
    }

    private static WorkQueueData.WorkQueueRow[] WorkQueuesCreation(
        WorkQueueData workQueueList,
        StateMachineServiceStatelessEventType eventType,
        Guid entityId,
        DataRow row
    )
    {
        return WorkQueues(
            workQueueList: workQueueList,
            eventType: eventType,
            entityId: entityId,
            row: row,
            isRemoval: false
        );
    }

    private static WorkQueueData.WorkQueueRow[] WorkQueuesRemoval(
        WorkQueueData workQueueList,
        StateMachineServiceStatelessEventType eventType,
        Guid entityId,
        DataRow row
    )
    {
        return WorkQueues(
            workQueueList: workQueueList,
            eventType: eventType,
            entityId: entityId,
            row: row,
            isRemoval: true
        );
    }

    private static WorkQueueData.WorkQueueRow[] WorkQueues(
        WorkQueueData workQueueList,
        StateMachineServiceStatelessEventType eventType,
        Guid entityId,
        DataRow row,
        bool isRemoval
    )
    {
        var result = new List<WorkQueueData.WorkQueueRow>();
        DatasetGenerator dg = new DatasetGenerator(userDefinedParameters: true);
        foreach (WorkQueueData.WorkQueueRow wqr in workQueueList.WorkQueue.Rows)
        {
            WorkQueueClass wqc = WQClass(name: wqr.WorkQueueClass);
            if (wqc != null)
            {
                bool isEmptyState = false;
                if (isRemoval && wqr.IsrefRemovalOrigamStateMachineEventTypeIdNull())
                {
                    isEmptyState = true;
                }
                if (!isRemoval && wqr.IsrefCreationOrigamStateMachineEventTypeIdNull())
                {
                    isEmptyState = true;
                }
                if (!isEmptyState)
                {
                    string fieldName = null;
                    if (isRemoval && !wqr.IsRemovalFieldNameNull())
                    {
                        fieldName = wqr.RemovalFieldName;
                    }
                    else if (!wqr.IsCreationFieldNameNull())
                    {
                        fieldName = wqr.CreationFieldName;
                    }
                    // we check if the work queue's entity and field name are matching
                    if (
                        wqc.EntityId.Equals(g: entityId)
                        && (
                            fieldName == null
                            || FieldChanged(row: row, eventType: eventType, fieldName: fieldName)
                        )
                    )
                    {
                        if (isRemoval || wqc.ConditionFilter == null)
                        {
                            // no filter, the work queue is valid
                            result.Add(item: wqr);
                        }
                        else
                        {
                            // evaluate the condition filter
                            if (log.IsDebugEnabled)
                            {
                                log.RunHandled(loggingAction: () =>
                                {
                                    XmlContainer datarow;
                                    if (row.RowState != DataRowState.Deleted)
                                    {
                                        datarow = DatasetTools.GetRowXml(
                                            row: row,
                                            version: DataRowVersion.Default
                                        );
                                    }
                                    else
                                    {
                                        datarow = DatasetTools.GetRowXml(
                                            row: row,
                                            version: DataRowVersion.Original
                                        );
                                    }
                                    log.DebugFormat(
                                        format: "Evaluating ConditionFilter {0} of work queue class {1} for row {2}.",
                                        arg0: wqc.ConditionFilter,
                                        arg1: wqc.Path,
                                        arg2: datarow.Xml.OuterXml
                                    );
                                });
                            }
                            StringBuilder filterBuilder = new StringBuilder();
                            string filter;
                            // we create a dummy ds entity based on the row's table
                            DataStructureEntity dummyEntity = new DataStructureEntity();
                            IPersistenceService ps =
                                ServiceManager.Services.GetService(
                                    serviceType: typeof(IPersistenceService)
                                ) as IPersistenceService;
                            dummyEntity.PersistenceProvider = ps.SchemaProvider;
                            dummyEntity.EntityId = entityId;
                            dummyEntity.AllFields = true;
                            dummyEntity.Name = row.Table.TableName;
                            // we get the condition filter string
                            dg.RenderFilter(
                                sqlExpression: filterBuilder,
                                filter: wqc.ConditionFilter,
                                entity: dummyEntity
                            );
                            filter = filterBuilder.ToString();
                            DataRowVersion version = (
                                row.RowState == DataRowState.Deleted
                                    ? DataRowVersion.Original
                                    : DataRowVersion.Default
                            );
                            // we add the row's primary key to the filter
                            foreach (DataColumn col in row.Table.PrimaryKey)
                            {
                                filter +=
                                    " AND "
                                    + col.ColumnName
                                    + " = "
                                    + dg.RenderObject(
                                        value: row[columnName: col.ColumnName, version: version],
                                        dataType: (OrigamDataType)
                                            row.Table
                                                .Columns[name: col.ColumnName]
                                                .ExtendedProperties[key: "OrigamDataType"]
                                    );
                            }
                            DataViewRowState state = (
                                row.RowState == DataRowState.Deleted
                                    ? DataViewRowState.OriginalRows
                                    : DataViewRowState.CurrentRows
                            );
                            // and we check if the row will pass through the filter
                            if (
                                row.Table.Select(
                                    filterExpression: filter,
                                    sort: "",
                                    recordStates: state
                                ).Length == 1
                            )
                            {
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug(message: "ConditionFilter evaluated True.");
                                }
                                // it passed, so we evaluate this work queue as valid for this row
                                result.Add(item: wqr);
                            }
                            else
                            {
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug(message: "ConditionFilter evaluated False.");
                                }
                            }
                        }
                    }
                }
            }
        }
        return result.ToArray();
    }

    private void StateMachineService_Initialize(object sender, EventArgs e) { }
}
