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
using System.Xml;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Text;
using Origam.DA;
using Origam.DA.Service;
using Origam.Rule;
using Origam.Workbench.Services;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workflow.WorkQueue;

using Origam.Schema.WorkflowModel;
using DataService 
    = Origam.Workbench.Services.CoreServices.DataService;
using Origam.Schema.WorkflowModel.WorkQueue;
using System.Linq;
using Origam.Extensions;
using Origam.Service.Core;

namespace Origam.Workflow;
/// <summary>
/// Summary description for StateMachineService.
/// </summary>
public class StateMachineService : AbstractService, IStateMachineService
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
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
        StateMachine sm = GetMachine(entityId, fieldId, false);
        StateMachineState targetStateObj = sm.GetChildByIdRecursive(targetStateId) as StateMachineState;
        object convertedStateValue;
        if (sm.Field.DataType == OrigamDataType.UniqueIdentifier && currentStateValue.GetType() == typeof(string))
        {
            convertedStateValue = new Guid((string)currentStateValue);
        }
        else
        {
            convertedStateValue = currentStateValue;
        }
        return targetStateObj.IsState(convertedStateValue);
    }
    public object[] AllowedStateValues(Guid entityId, Guid fieldId, object currentStateValue, DataRow dataRow, string transactionId)
    {
        IXmlContainer dataDocument = DatasetTools.GetRowXml(dataRow, DataRowVersion.Default);
        return AllowedStateValues(entityId, fieldId, currentStateValue, dataDocument, transactionId);
    }
    public object[] AllowedStateValues(Guid entityId, Guid fieldId, object currentStateValue, IXmlContainer data, string transactionId)
    {
        StateMachine sm = GetMachine(entityId, fieldId, false);
        if (sm == null)
        {
            // no statemachine on this entity/field
            return null;
        }
        else
        {
            if (currentStateValue == DBNull.Value)
            {
                // original value is null, meaning that this is a new row
                // we only allow the initial state to be selected, no other
                var resultList = new List<object>();
                foreach (object val in sm.InitialStateValues(data))
                {
                    resultList.Add(val);
                }
                return resultList.ToArray();
            }
            else if (sm.DynamicOperationsLookup != null)
            {
                // state has existed already, so we list all possible dynamic states
                IXmlContainer dynData = data.Clone() as IXmlContainer;
                if (dynData is DataDocumentCore) throw new NotImplementedException("Cannot write to Xml property of DataDocumentCore");
                if (sm.Field.XmlMappingType == EntityColumnXmlMapping.Attribute)
                {
                    ((XmlElement)dynData.Xml.FirstChild).SetAttribute(sm.Field.Name, XmlTools.ConvertToString(currentStateValue));
                }
                else
                {
                    if (dynData.Xml.FirstChild.SelectSingleNode(sm.Field.Name) != null)
                    {
                        dynData.Xml.FirstChild.SelectSingleNode(sm.Field.Name).Value = XmlTools.ConvertToString(currentStateValue);
                    }
                    else
                    {
                        XmlElement el = dynData.Xml.CreateElement(sm.Field.Name);
                        el.Value = XmlTools.ConvertToString(currentStateValue);
                        dynData.Xml.FirstChild.AppendChild(el);
                    }
                }
                ArrayList a = new ArrayList();
                a.Add(currentStateValue);
                a.AddRange(sm.DynamicOperations(dynData));
                return a.ToArray();
            }
            else
            {
                // state has existed already, so we list all the possible model states
                StateMachineState state = sm.GetState(currentStateValue);
                if (state == null)
                {
                    throw new ArgumentOutOfRangeException("currentStateValue", currentStateValue, ResourceUtils.GetString("ErrorBuildStateList"));
                }
                else
                {
                    ArrayList resultList = new ArrayList();
                    resultList.Add(currentStateValue);
                    while (state.ParentItem is StateMachineState | state.ParentItem is StateMachine)
                    {
                        foreach (StateMachineOperation op in state.Operations)
                        {
                            if (IsOperationAllowed(op, data, transactionId))
                            {
                                resultList.Add(op.TargetState.Value);
                            }
                        }
                        if (state.ParentItem is StateMachine)
                        {
                            break;
                        }
                        state = state.ParentItem as StateMachineState;
                    }
                    return (object[])resultList.ToArray(typeof(object));
                }
            }
        }
    }
    public bool IsStateAllowed(Guid entityId, Guid fieldId, object currentStateValue, object newStateValue, DataRow dataRow, string transactionId)
    {
        IXmlContainer data = DatasetTools.GetRowXml(dataRow, DataRowVersion.Default);
        return IsStateAllowed(entityId, fieldId, currentStateValue, newStateValue, data, transactionId);
    }
    public bool IsStateAllowed(Guid entityId, Guid fieldId, object currentStateValue, object newStateValue, IXmlContainer data, string transactionId)
    {
        foreach (object allowedValue in this.AllowedStateValues(entityId, fieldId, currentStateValue, data, transactionId))
        {
            if (newStateValue.Equals(allowedValue))
            {
                return true;
            }
        }
        return false;
    }
    public void OnDataChanging(DataTable changedTable, string transactionId)
    {
        if (changedTable.ExtendedProperties.Contains("EntityId"))
        {
            ArrayList stateColumns = new ArrayList();
            Guid entityId = (Guid)changedTable.ExtendedProperties["EntityId"];
            foreach (DataColumn column in changedTable.Columns)
            {
                if (column.ExtendedProperties.Contains("IsState") && (bool)column.ExtendedProperties["IsState"])
                {
                    stateColumns.Add(column);
                }
            }
            foreach (DataRow row in changedTable.Rows)
            {
                if (stateColumns.Count > 0)
                {
                    foreach (DataColumn column in stateColumns)
                    {
                        Guid fieldId = (Guid)column.ExtendedProperties["Id"];
                        switch (row.RowState)
                        {
                            case DataRowState.Added:
                                ExecutePreEvents(entityId, fieldId, DBNull.Value, row[column], row, transactionId);
                                break;
                            case DataRowState.Modified:
                                object oldValue = row[column, DataRowVersion.Original];
                                object newValue = row[column, DataRowVersion.Current];
                                if (!oldValue.Equals(newValue))
                                {
                                    ExecutePreEvents(entityId, fieldId, oldValue, newValue, row, transactionId);
                                }
                                break;
                            case DataRowState.Deleted:
                                break;
                        }
                    }
                }
                if (row.RowState == DataRowState.Deleted)
                {
                    ExecuteStatelessEvents(entityId, StateMachineServiceStatelessEventType.BeforeRecordDeleted, row, transactionId);
                }
            }
        }
    }
    public void OnDataChanged(DataSet data, List<string> changedTables, string transactionId)
    {
        ArrayList rows = new ArrayList();
        foreach (string tableName in changedTables)
        {
            DataTable changedTable = data.Tables[tableName];
            if(changedTable.ExtendedProperties.Contains("EntityId"))
            {
                ArrayList stateColumns = new ArrayList();
                Guid entityId = (Guid)changedTable.ExtendedProperties["EntityId"];
                foreach(DataColumn column in changedTable.Columns)
                {
                    if(column.ExtendedProperties.Contains("IsState") && (bool)column.ExtendedProperties["IsState"])
                    {
                        stateColumns.Add(column);
                    }
                }
                if((stateColumns.Count > 0)
                || AnyWorkQueueClassBasedOnEntity(entityId)
                || AnyStateMachineBasedOnEntity(entityId))
                {
                    foreach (DataRow row in changedTable.Rows)
                    {
                        if (row.RowState != DataRowState.Unchanged && row.RowState != DataRowState.Detached)
                        {
                            rows.Add(new StateMachineQueueEntry(row, stateColumns, entityId));
                        }
                    }
                }
            }
        }
        foreach (StateMachineQueueEntry entry in rows)
        {
            ProcessRecordCreated(entry.Row, entry.EntityId, transactionId);
        }
        foreach (StateMachineQueueEntry entry in rows)
        {
            ProcessRecordStateTransition(entry.Row, entry.StateColumns, entry.EntityId, transactionId);
        }
        foreach (StateMachineQueueEntry entry in rows)
        {
            ProcessRecordModifiedDeleted(entry.Row, entry.EntityId, transactionId);
        }
    }
    private void ProcessRecordCreated(DataRow row, Guid entityId, string transactionId)
    {
        // recordCreated
        if (row.RowState == DataRowState.Added)
        {
            XmlContainer data = DatasetTools.GetRowXml(row, DataRowVersion.Default);
            ExecuteStatelessEvents(entityId, StateMachineServiceStatelessEventType.RecordCreated, row, transactionId);
        }
    }
    private void ProcessRecordStateTransition(DataRow row, ArrayList stateColumns, Guid entityId, string transactionId)
    {
        if (row.RowState != DataRowState.Deleted)
        {
            IXmlContainer data = DatasetTools.GetRowXml(row, DataRowVersion.Default);
            // state entry or state transition
            foreach (DataColumn column in stateColumns)
            {
                Guid fieldId = (Guid)column.ExtendedProperties["Id"];
                StateMachine sm = GetMachine(entityId, fieldId, true);
                switch (row.RowState)
                {
                    case DataRowState.Added:
                        ExecutePostEvents(sm, DBNull.Value, row[column], row, data, transactionId);
                        break;
                    case DataRowState.Modified:
                        object oldValue = row[column, DataRowVersion.Original];
                        object newValue = row[column, DataRowVersion.Current];
                        if (!oldValue.Equals(newValue))
                        {
                            ExecutePostEvents(sm, oldValue, newValue, row, data, transactionId);
                        }
                        break;
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
                ExecuteStatelessEvents(entityId, StateMachineServiceStatelessEventType.RecordUpdated, row, transactionId);
                break;
            case DataRowState.Deleted:
                ExecuteStatelessEvents(entityId, StateMachineServiceStatelessEventType.RecordDeleted, row, transactionId);
                break;
        }
    }
    private bool ExecutePostEvents(StateMachine sm, object currentStateValue, object newStateValue, DataRow dataRow, IXmlContainer data, string transactionId)
    {
        object rowKey = null;
        object[] keys = DatasetTools.PrimaryKey(dataRow);
        if (keys.Length == 1) rowKey = keys[0];
        // check if state transition is allowed
        if (!IsStateAllowed(sm.EntityId, sm.FieldId, currentStateValue, newStateValue, data, transactionId))
        {
            ThrowStateTransitionInvalidException(sm, currentStateValue, newStateValue, transactionId);
        }
        ExecutePostWorkQueue(sm, StateMachineServiceStatelessEventType.RecordUpdated, newStateValue, dataRow, data, transactionId);
        foreach (StateMachineEvent ev in sm.Events)
        {
            if (IsEventAllowed(ev))
            {
                if (ev.Type == StateMachineEventType.StateEntry
                    && ev.OldState == null
                    && ev.NewState.IsState(newStateValue)
                    && !ev.NewState.IsState(currentStateValue)
                    )
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Will execute event: " + ev.Type.ToString() + ", " + ev.Name);
                    }
                    ExecuteWorkflow(ev.Action, ev, dataRow, transactionId);
                }
            }
        }
        return false;
    }
    private void ThrowStateTransitionInvalidException(StateMachine sm, object currentStateValue, object newStateValue, string transactionId)
    {
        string newStateName = StateValueName(sm, newStateValue, transactionId);
        string currentStateName = StateValueName(sm, currentStateValue, transactionId);
        throw new Exception(ResourceUtils.GetString("ErrorChangeState", sm.Field.Caption,
            (sm.Entity.Caption == "" ? sm.Entity.Name : sm.Entity.Caption), newStateName,
             currentStateName));
    }
    private void ExecutePostWorkQueue(StateMachine sm, StateMachineServiceStatelessEventType eventType, object newStateValue, DataRow dataRow, IXmlContainer data, string transactionId)
    {
        object rowKey = null;
        object[] keys = DatasetTools.PrimaryKey(dataRow);
        if (keys.Length == 1) rowKey = keys[0];
        // process post events work queue
        IParameterService ps = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
        string newStateValueString = XmlTools.ConvertToString(newStateValue);
        // creation
        foreach (WorkQueueData.WorkQueueRow wq in WorkQueuesCreation(WorkQueueList(transactionId), eventType, sm.EntityId, dataRow))
        {
            if (wq.refCreationOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_StateEntry")))
            {
                if (wq.IsCreationNewValueNull())
                {
                    throw new NullReferenceException(ResourceUtils.GetString("ErrorInvalidWorkQueueDefinition0", wq.Name));
                }
                else
                {
                    if (sm.Field != null
                        && sm.Field.Name.Equals(wq.CreationFieldName)
                        && ReverseLookupWorkQueueFieldValue(sm, wq, wq.CreationNewValue, transactionId).Equals(newStateValueString))
                    {
                        // write to queue
                        WQService.WorkQueueAdd(wq.WorkQueueClass, wq.Name, wq.Id, wq.IsCreationConditionNull() ? null : wq.CreationCondition, data, transactionId);
                    }
                }
            }
        }
        // removal
        foreach (WorkQueueData.WorkQueueRow wq in WorkQueuesRemoval(WorkQueueList(transactionId), eventType, sm.EntityId, dataRow))
        {
            if (wq.refRemovalOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_StateEntry")))
            {
                if (wq.IsRemovalNewValueNull())
                {
                    throw new NullReferenceException(ResourceUtils.GetString("ErrorInvalidWorkQueueDefinition3", wq.Name));
                }
                else
                {
                    if (sm.Field != null
                        && sm.Field.Name.Equals(wq.RemovalFieldName)
                        && ReverseLookupWorkQueueFieldValue(sm, wq, wq.RemovalNewValue, transactionId).Equals(newStateValueString))
                    {
                        // remove from queue
                        WQService.WorkQueueRemove(wq.WorkQueueClass, wq.Name, wq.Id, wq.IsRemovalConditionNull() ? null : wq.RemovalCondition, rowKey, transactionId);
                    }
                }
            }
        }
    }
    private string ReverseLookupWorkQueueFieldValue(StateMachine sm, WorkQueueData.WorkQueueRow wq, string originalValue, string transactionId)
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
                throw new NullReferenceException(string.Format(ResourceUtils.GetString("ErrorReverseLookupNotSet"), wq.Name));
            }
            IDataLookupService lookupManager = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
            result = lookupManager.GetDisplayText(sm.ReverseLookupId, originalValue, false, false, transactionId).ToString();
        }
        if (result == null)
        {
            throw new NullReferenceException(ResourceUtils.GetString("ErrorInvalidWorkQueueDefinition0", wq.Name));
        }
        return result;
    }
    public bool ExecutePreEvents(Guid entityId, Guid fieldId, object currentStateValue, object newStateValue, 
        DataRow dataRow, string transactionId)
    {
        StateMachine sm = GetMachine(entityId, fieldId, true);
        IXmlContainer data = DatasetTools.GetRowXml(dataRow, DataRowVersion.Default);
        object rowKey = null;
        object[] keys = DatasetTools.PrimaryKey(dataRow);
        if (keys.Length == 1) rowKey = keys[0];
        if (!IsStateAllowed(entityId, fieldId, currentStateValue, newStateValue, data, transactionId))
        {
            ThrowStateTransitionInvalidException(sm, currentStateValue, newStateValue, transactionId);
        }
        ExecutePreWorkQueue(entityId, currentStateValue, newStateValue, dataRow, transactionId, sm, data, rowKey);
        foreach (StateMachineEvent ev in sm.Events)
        {
            if (IsEventAllowed(ev))
            {
                if (
                    (ev.Type == StateMachineEventType.StateExit 
                        && ev.NewState == null && ev.OldState.IsState(currentStateValue))
                    ||
                    (ev.Type == StateMachineEventType.StateTransition 
                        && ev.OldState.IsState(currentStateValue) && ev.NewState.IsState(newStateValue))
                    )
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Will execute event: " + ev.Type.ToString() + ", " + ev.Name);
                    }
                    ExecuteWorkflow(ev.Action, ev, dataRow, transactionId);
                }
            }
        }
        return false;
    }
    private void ExecutePreWorkQueue(Guid entityId, object currentStateValue, object newStateValue, DataRow dataRow, string transactionId, StateMachine sm, IXmlContainer data, object rowKey)
    {
        IParameterService ps = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
        string newStateValueString = XmlTools.ConvertToString(newStateValue);
        string oldStateValueString = XmlTools.ConvertToString(currentStateValue);
        // CREATION
        foreach (WorkQueueData.WorkQueueRow wq in WorkQueuesCreation(WorkQueueList(transactionId), StateMachineServiceStatelessEventType.RecordUpdated, entityId, dataRow))
        {
            // creation - state exit
            if (wq.refCreationOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_StateExit")))
            {
                if (wq.IsCreationOldValueNull())
                {
                    throw new NullReferenceException(ResourceUtils.GetString("ErrorInvalidWorkQueueDefinition1", wq.Name));
                }
                else
                {
                    if (sm.Field != null
                        && sm.Field.Name.Equals(wq.CreationFieldName)
                        && ReverseLookupWorkQueueFieldValue(sm, wq, wq.CreationOldValue, transactionId).Equals(oldStateValueString))
                    {
                        // write to queue
                        WQService.WorkQueueAdd(wq.WorkQueueClass, wq.Name, wq.Id, wq.IsCreationConditionNull() ? null : wq.CreationCondition, data, transactionId);
                    }
                }
            }
            // creation - state transition
            if (wq.refCreationOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_StateTransition")))
            {
                if (wq.IsCreationOldValueNull())
                {
                    throw new NullReferenceException(ResourceUtils.GetString("ErrorInvalidWorkQueueDefinition2", wq.Name));
                }
                else if (wq.IsCreationNewValueNull())
                {
                    throw new NullReferenceException(ResourceUtils.GetString("ErrorInvalidWorkQueueDefinition4", wq.Name));
                }
                else if (sm.Field != null
                    && sm.Field.Name.Equals(wq.CreationFieldName))
                {
                    string oldValue = ReverseLookupWorkQueueFieldValue(sm, wq, wq.CreationOldValue, transactionId);
                    string newValue = ReverseLookupWorkQueueFieldValue(sm, wq, wq.CreationNewValue, transactionId);
                    if (oldValue.Equals(oldStateValueString) && newValue.Equals(newStateValueString))
                    {
                        // write to queue
                        WQService.WorkQueueAdd(wq.WorkQueueClass, wq.Name, wq.Id, wq.IsCreationConditionNull() ? null : wq.CreationCondition, data, transactionId);
                    }
                }
            }
        }
        // REMOVAL
        foreach (WorkQueueData.WorkQueueRow wq in WorkQueuesRemoval(WorkQueueList(transactionId), StateMachineServiceStatelessEventType.RecordUpdated, entityId, dataRow))
        {
            // removal - state exit
            if (wq.refRemovalOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_StateExit")))
            {
                if (wq.IsRemovalOldValueNull())
                {
                    throw new NullReferenceException(ResourceUtils.GetString("ErrorInvalidWorkQueueDefinition5", wq.Name));
                }
                else
                {
                    if (sm.Field != null
                        && sm.Field.Name.Equals(wq.RemovalFieldName)
                        && ReverseLookupWorkQueueFieldValue(sm, wq, wq.RemovalOldValue, transactionId).Equals(oldStateValueString))
                    {
                        // remove from queue
                        WQService.WorkQueueRemove(wq.WorkQueueClass, wq.Name, wq.Id, wq.IsRemovalConditionNull() ? null : wq.RemovalCondition, rowKey, transactionId);
                    }
                }
            }
            // removal - state transition
            if (wq.refRemovalOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_StateTransition")))
            {
                if (wq.IsRemovalOldValueNull())
                {
                    throw new NullReferenceException(ResourceUtils.GetString("ErrorInvalidWorkQueueDefinition6", wq.Name));
                }
                else if (wq.IsRemovalNewValueNull())
                {
                    throw new NullReferenceException(ResourceUtils.GetString("ErrorInvalidWorkQueueDefinition7", wq.Name));
                }
                else if (sm.Field != null
                    && sm.Field.Name.Equals(wq.RemovalFieldName))
                {
                    string oldValue = ReverseLookupWorkQueueFieldValue(sm, wq, wq.RemovalOldValue, transactionId);
                    string newValue = ReverseLookupWorkQueueFieldValue(sm, wq, wq.RemovalNewValue, transactionId);
                    if (oldValue.Equals(oldStateValueString) && newValue.Equals(newStateValueString))
                    {
                        // remove from queue
                        WQService.WorkQueueRemove(wq.WorkQueueClass, wq.Name, wq.Id, wq.IsRemovalConditionNull() ? null : wq.RemovalCondition, rowKey, transactionId);
                    }
                }
            }
        }
    }
    public bool ExecuteStatelessEvents(Guid entityId, StateMachineServiceStatelessEventType eventType, DataRow dataRow, string transactionId)
    {
        ArrayList stateMachines = GetMachines(entityId, false);
        ArrayList eventsSorted = new ArrayList();
        object rowKey = null;
        object[] keys = DatasetTools.PrimaryKey(dataRow);
        if (keys.Length == 1) rowKey = keys[0];
        WorkQueueData workQueueList = WorkQueueList(transactionId);
        // first remove any work queue entries for this record since we are deleting a record
        // after that new entries may be created if they are configured to be created OnRecordDeleted
        if (eventType == StateMachineServiceStatelessEventType.RecordDeleted)
        {
            foreach (WorkQueueData.WorkQueueRow wqr in workQueueList.WorkQueue.Rows)
            {
                WorkQueueClass wqc = WQClass(wqr.WorkQueueClass);
                if (wqc != null && wqc.EntityId.Equals(entityId))
                {
                    WQService.WorkQueueRemove(wqr.WorkQueueClass, wqr.Name, wqr.Id, null, rowKey, transactionId);
                }
            }
        }
        ExecuteStatelessWorkQueue(entityId, eventType, dataRow, transactionId, rowKey, workQueueList);
        foreach (StateMachine sm in stateMachines)
        {
            eventsSorted.AddRange(sm.Events);
        }
        eventsSorted.Sort();
        foreach (StateMachineEvent ev in eventsSorted)
        {
            if (IsEventAllowed(ev))
            {
                if (
                    (ev.Type == StateMachineEventType.RecordCreated && eventType == StateMachineServiceStatelessEventType.RecordCreated)
                    || (ev.Type == StateMachineEventType.RecordUpdated && eventType == StateMachineServiceStatelessEventType.RecordUpdated)
                    || (ev.Type == StateMachineEventType.RecordCreatedUpdated && eventType == StateMachineServiceStatelessEventType.RecordCreated)
                    || (ev.Type == StateMachineEventType.RecordCreatedUpdated && eventType == StateMachineServiceStatelessEventType.RecordUpdated)
                    || (ev.Type == StateMachineEventType.RecordDeleted && eventType == StateMachineServiceStatelessEventType.RecordDeleted)
                    || (ev.Type == StateMachineEventType.BeforeRecordDeleted && eventType == StateMachineServiceStatelessEventType.BeforeRecordDeleted)
                    )
                {
                    if (FieldsChanged(ev.FieldDependencies, dataRow, eventType))
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug("Will execute event: " + ev.Type.ToString() + ", " + ev.Name);
                        }
                        ExecuteWorkflow(ev.Action, ev, dataRow, transactionId);
                    }
                }
            }
        }
        return false;
    }
    private static void ExecuteStatelessWorkQueue(Guid entityId, StateMachineServiceStatelessEventType eventType, DataRow dataRow, string transactionId, object rowKey, WorkQueueData workQueueList)
    {
        if (workQueueList.WorkQueue.Rows.Count > 0)
        {
            IParameterService ps = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
            // creation
            WorkQueueData.WorkQueueRow[] creationQueues = WorkQueuesCreation(workQueueList, eventType, entityId, dataRow);
            if (creationQueues.Length > 0)
            {
                XmlContainer data = DatasetTools.GetRowXml(dataRow,
                    eventType == StateMachineServiceStatelessEventType.RecordDeleted
                        || eventType == StateMachineServiceStatelessEventType.BeforeRecordDeleted
                    ? DataRowVersion.Original
                    : DataRowVersion.Default);
                foreach (WorkQueueData.WorkQueueRow wq in creationQueues)
                {
                    if ((wq.refCreationOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_RecordCreated"))
                            && eventType == StateMachineServiceStatelessEventType.RecordCreated)
                        || (wq.refCreationOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_RecordUpdated"))
                            && eventType == StateMachineServiceStatelessEventType.RecordUpdated)
                        || (wq.refCreationOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_RecordDeleted"))
                            && eventType == StateMachineServiceStatelessEventType.RecordDeleted)
                        || (wq.refCreationOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_ValueUpdated"))
                            && eventType == StateMachineServiceStatelessEventType.RecordUpdated)
                        || (wq.refCreationOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_ValueUpdated"))
                            && eventType == StateMachineServiceStatelessEventType.RecordCreated)
                        )
                    {
                        // write to queue
                        WQService.WorkQueueAdd(wq.WorkQueueClass, wq.Name, wq.Id, wq.IsCreationConditionNull()
                            ? null
                            : wq.CreationCondition, data, transactionId);
                    }
                }
            }
            if (eventType == StateMachineServiceStatelessEventType.RecordUpdated)
            {
                // update all the queue entries related to this entity
                foreach (WorkQueueData.WorkQueueRow wq in workQueueList.WorkQueue.Rows)
                {
                    WorkQueueClass wqc = WQClass(wq.WorkQueueClass);
                    if (wqc != null)
                    {
                        if (entityId.Equals(wqc.EntityId))
                        {
                            WQService.WorkQueueUpdate(wq.WorkQueueClass, 0, wq.Id, rowKey, transactionId);
                        }
                        if (entityId.Equals(wqc.RelatedEntity1Id))
                        {
                            WQService.WorkQueueUpdate(wq.WorkQueueClass, 1, wq.Id, rowKey, transactionId);
                        }
                        if (entityId.Equals(wqc.RelatedEntity2Id))
                        {
                            WQService.WorkQueueUpdate(wq.WorkQueueClass, 2, wq.Id, rowKey, transactionId);
                        }
                        if (entityId.Equals(wqc.RelatedEntity3Id))
                        {
                            WQService.WorkQueueUpdate(wq.WorkQueueClass, 3, wq.Id, rowKey, transactionId);
                        }
                        if (entityId.Equals(wqc.RelatedEntity4Id))
                        {
                            WQService.WorkQueueUpdate(wq.WorkQueueClass, 4, wq.Id, rowKey, transactionId);
                        }
                        if (entityId.Equals(wqc.RelatedEntity5Id))
                        {
                            WQService.WorkQueueUpdate(wq.WorkQueueClass, 5, wq.Id, rowKey, transactionId);
                        }
                        if (entityId.Equals(wqc.RelatedEntity6Id))
                        {
                            WQService.WorkQueueUpdate(wq.WorkQueueClass, 6, wq.Id, rowKey, transactionId);
                        }
                        if (entityId.Equals(wqc.RelatedEntity7Id))
                        {
                            WQService.WorkQueueUpdate(wq.WorkQueueClass, 7, wq.Id, rowKey, transactionId);
                        }
                    }
                }
            }
            // removal (as last because to evaluate eventual XPath conditions 
            // records have to be updated before deleting them
            WorkQueueData.WorkQueueRow[] removalQueues = WorkQueuesRemoval(workQueueList, eventType, entityId, dataRow);
            foreach (WorkQueueData.WorkQueueRow wq in removalQueues)
            {
                if (!wq.IsrefRemovalOrigamStateMachineEventTypeIdNull())
                {
                    if ((wq.refRemovalOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_RecordCreated"))
                            && eventType == StateMachineServiceStatelessEventType.RecordCreated)
                        || (wq.refRemovalOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_RecordUpdated"))
                            && eventType == StateMachineServiceStatelessEventType.RecordUpdated)
                        || (wq.refRemovalOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_RecordDeleted"))
                            && eventType == StateMachineServiceStatelessEventType.RecordDeleted)
                        || (wq.refRemovalOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_ValueUpdated"))
                            && eventType == StateMachineServiceStatelessEventType.RecordUpdated)
                        || (wq.refRemovalOrigamStateMachineEventTypeId.Equals(ps.GetParameterValue("OrigamStateMachineEventType_ValueUpdated"))
                            && eventType == StateMachineServiceStatelessEventType.RecordCreated)
                        )
                    {
                        // remove from queue
                        WQService.WorkQueueRemove(wq.WorkQueueClass, wq.Name, wq.Id, wq.IsRemovalConditionNull()
                            ? null
                            : wq.RemovalCondition, rowKey, transactionId);
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
    private bool FieldsChanged(List<ISchemaItem> fields, DataRow row, StateMachineServiceStatelessEventType type)
    {
        // if there are no field dependencies all fields changed
        if (fields.Count == 0) return true;
        // for delete operation all fields changed
        if (type == StateMachineServiceStatelessEventType.BeforeRecordDeleted
            && type == StateMachineServiceStatelessEventType.RecordDeleted)
        {
            return true;
        }
        foreach (StateMachineEventFieldDependency dependency in fields)
        {
            if (FieldChanged(row, type, dependency.Field.Name))
            {
                return true;
            }
        }
        return false;
    }
    private static bool FieldChanged(DataRow row, StateMachineServiceStatelessEventType eventType,
        string fieldName)
    {
        if (row.Table.Columns.Contains(fieldName))
        {
            // when new record we test if the value is not empty, then we execute the events
            if (row.RowState == DataRowState.Added
                || eventType == StateMachineServiceStatelessEventType.RecordCreated)
            {
                if (!row[fieldName, DataRowVersion.Default].Equals(DBNull.Value))
                {
                    return true;
                }
            }
            else if (row.RowState == DataRowState.Modified
                && !row[fieldName, DataRowVersion.Original].Equals(row[fieldName, DataRowVersion.Current]))
            {
                return true;
            }
        }
        else
        {
            Guid entityId = (Guid)row.Table.ExtendedProperties["Id"];
            throw new ArgumentOutOfRangeException(string.Format(
                "Data Structure Entity '{0}' ({1}) does not contain field '{2}'. Cannot evaluate state machine field conditions.",
                row.Table.TableName, entityId, fieldName, row.Table.TableName));
        }
        return false;
    }
    private StateMachine GetMachine(Guid entityId, Guid fieldId, bool throwException)
    {
        StateMachineSchemaItemProvider stateMachines = (ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService).GetProvider(typeof(StateMachineSchemaItemProvider)) as StateMachineSchemaItemProvider;
        if (stateMachines == null)
        {
            return null;
        }
        else
        {
            StateMachine sm = stateMachines.GetMachine(entityId, fieldId);
            if (sm == null && throwException) throw new ArgumentOutOfRangeException("fieldId", fieldId, ResourceUtils.GetString("ErrorStateMachineNotFound"));
            return sm;
        }
    }
    private ArrayList GetMachines(Guid entityId, bool throwException)
    {
        StateMachineSchemaItemProvider stateMachines = (ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService).GetProvider(typeof(StateMachineSchemaItemProvider)) as StateMachineSchemaItemProvider;
        if (stateMachines == null)
        {
            return null;
        }
        else
        {
            ArrayList result = stateMachines.GetMachines(entityId);
            if (result.Count == 0 && throwException) throw new ArgumentOutOfRangeException("entityId", entityId, ResourceUtils.GetString("ErrorStateMachineNotFound"));
            return result;
        }
    }
    private string StateValueName(StateMachine sm, object stateValue, string transactionId)
    {
        IDataLookupService lookupManager = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
        if (sm.Field.DefaultLookup == null) return stateValue.ToString();
        object result = lookupManager.GetDisplayText((Guid)sm.Field.DefaultLookup.PrimaryKey["Id"], stateValue, transactionId);
        return result.ToString();
    }
    private bool IsOperationAllowed(StateMachineOperation operation, IXmlContainer data, string transactionId)
    {
        // check features
        IParameterService param = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
        if (!param.IsFeatureOn(operation.Features))
        {
            return false;
        }
        // check roles
        IOrigamAuthorizationProvider authorizationProvider = SecurityManager.GetAuthorizationProvider();
        if (!authorizationProvider.Authorize(SecurityManager.CurrentPrincipal, operation.Roles))
        {
            return false;
        }
        // check business rule
        if (operation.Rule != null)
        {
            RuleEngine ruleEngine = RuleEngine.Create(new Hashtable(), transactionId);
            //				ruleEngine.TransactionId = transactionId;
            object result = ruleEngine.EvaluateRule(operation.Rule, data, null);
            if (result is bool)
            {
                return (bool)result;
            }
            else
            {
                throw new ArgumentException(ResourceUtils.GetString("ErrorResultNotBool", operation.Path));
            }
        }
        return true;
    }
    private bool IsEventAllowed(StateMachineEvent ev)
    {
        IOrigamAuthorizationProvider authorizationProvider = SecurityManager.GetAuthorizationProvider();
        if (!authorizationProvider.Authorize(SecurityManager.CurrentPrincipal, ev.Roles))
        {
            return false;
        }
        IParameterService param = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
        return param.IsFeatureOn(ev.Features);
    }
    private void ExecuteWorkflow(IWorkflow workflow, StateMachineEvent ev, DataRow dataRow, string transactionId)
    {
        if (transactionId == null)
        {
            throw new ArgumentNullException("transactionId", ResourceUtils.GetString("ErrorNoTransaction"));
        }
        if (workflow == null)
        {
            throw new ArgumentNullException("workflow", ResourceUtils.GetString("ErrorWorkflowUnspecified", ev.Path));
        }
        if (log.IsDebugEnabled)
        {
            log.Debug("State machine is starting workflow: " + workflow.Name);
        }
        WorkflowEngine engine = new WorkflowEngine(transactionId);
        engine.PersistenceProvider = _persistence.SchemaProvider;
        engine.WorkflowBlock = workflow;
        foreach (StateMachineEventParameterMapping mapping in ev.ParameterMappings)
        {
            DataRowVersion version = DatasetTools.GetRowVersion(dataRow,
                mapping.Type == WorkflowEntityParameterMappingType.Original);
            if (mapping.Field == null)
            {
                if (mapping.Type == WorkflowEntityParameterMappingType.ChangedFlag)
                {
                    throw new ArgumentOutOfRangeException("Type",
                        mapping.Type,
                        "Field not specified for state machine parameter mapping '" + mapping.Path + "'");
                }
                // no field, we pass the whole context
                engine.InputContexts.Add(mapping.ContextStore.PrimaryKey, DatasetTools.GetRowXml(dataRow, version));
            }
            else
            {
                // field specified, we pass the field value
                bool found = false;
                foreach (DataColumn column in dataRow.Table.Columns)
                {
                    if (column.ExtendedProperties.Contains("Id")
                        && !column.ExtendedProperties.Contains(Const.IsAddittionalFieldAttribute)
                        && column.ExtendedProperties["Id"].Equals(mapping.FieldId))
                    {
                        // context already contains the value
                        if (engine.InputContexts.Contains(mapping.ContextStore.PrimaryKey))
                        {
                            if (!engine.InputContexts[mapping.ContextStore.PrimaryKey].Equals(dataRow[column, version]))
                            {
                                throw new InvalidOperationException(ResourceUtils.GetString("ErrorTwoValuesOneContext0", mapping.ContextStore.PrimaryKey.ToString())
                                    + Environment.NewLine
                                    + ResourceUtils.GetString("ErrorTwoValuesOneContext1"));
                            }
                        }
                        else
                        {
                            if (mapping.Type == WorkflowEntityParameterMappingType.ChangedFlag)
                            {
                                DataRowVersion currentVersion = DatasetTools.GetRowVersion(dataRow, false);
                                DataRowVersion originalVersion = DatasetTools.GetRowVersion(dataRow, true);
                                object currentValue = dataRow[column, currentVersion];
                                object originalValue = dataRow[column, originalVersion];
                                engine.InputContexts.Add(mapping.ContextStore.PrimaryKey, !currentValue.Equals(originalValue));
                            }
                            else
                            {
                                engine.InputContexts.Add(mapping.ContextStore.PrimaryKey, dataRow[column, version]);
                            }
                        }
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    throw new ArgumentOutOfRangeException("Field", mapping.Field.Name, "Field mapped to a workflow context not found in the provided data row. Cannot execute workflow.");
                }
            }
        }
        WorkflowHost.DefaultHost.ExecuteWorkflow(engine);
        if (engine.WorkflowException != null) throw engine.WorkflowException;
        if (log.IsDebugEnabled)
        {
            log.Debug("State machine workflow call finished: " + workflow.Name);
        }
    }
    private static WorkQueueData WorkQueueList(string transactionId)
    {
        if (_WorkQueueLastRefreshed.AddMinutes(1).CompareTo(DateTime.Now) < 0)
        {
            _WorkQueueCache = (WQService as WorkQueueService).GetQueues(
                transactionId: transactionId);
            _WorkQueueLastRefreshed = DateTime.Now;
        }
        return _WorkQueueCache;
    }
    private static bool AnyWorkQueueClassBasedOnEntity(Guid entityId)
    {
        return ServiceManager.Services.GetService<SchemaService>()
            .GetProvider<WorkQueueClassSchemaItemProvider>()
            .ChildItemsByType("WorkQueueClass").Cast<WorkQueueClass>()
            .Any(workQueueClass => workQueueClass.EntityId == entityId);
    }
    private static bool AnyStateMachineBasedOnEntity(Guid entityId)
    {
        return ServiceManager.Services.GetService<SchemaService>()
            .GetProvider<StateMachineSchemaItemProvider>()
            .ChildItemsByType("WorkflowStateMachine").Cast<StateMachine>()
            .Any(stateMachine => stateMachine.EntityId == entityId);
    }
    private static WorkQueueClass WQClass(string name)
    {
        return (WorkQueueClass)WQService.WQClass(name);
    }
    private static IWorkQueueService WQService
    {
        get
        {
            return ServiceManager.Services.GetService(typeof(IWorkQueueService)) as IWorkQueueService;
        }
    }
    private static WorkQueueData.WorkQueueRow[] WorkQueuesCreation(WorkQueueData workQueueList, StateMachineServiceStatelessEventType eventType, Guid entityId, DataRow row)
    {
        return WorkQueues(workQueueList, eventType, entityId, row, false);
    }
    private static WorkQueueData.WorkQueueRow[] WorkQueuesRemoval(WorkQueueData workQueueList, StateMachineServiceStatelessEventType eventType, Guid entityId, DataRow row)
    {
        return WorkQueues(workQueueList, eventType, entityId, row, true);
    }
    private static WorkQueueData.WorkQueueRow[] WorkQueues(WorkQueueData workQueueList, StateMachineServiceStatelessEventType eventType, Guid entityId, DataRow row, bool isRemoval)
    {
        ArrayList result = new ArrayList();
        DatasetGenerator dg = new DatasetGenerator(true);
        foreach (WorkQueueData.WorkQueueRow wqr in workQueueList.WorkQueue.Rows)
        {
            WorkQueueClass wqc = WQClass(wqr.WorkQueueClass);
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
                    if (wqc.EntityId.Equals(entityId)
                        && (fieldName == null || FieldChanged(row, eventType, fieldName))
                        )
                    {
                        if (isRemoval || wqc.ConditionFilter == null)
                        {
                            // no filter, the work queue is valid
                            result.Add(wqr);
                        }
                        else
                        {
                            // evaluate the condition filter
                            if (log.IsDebugEnabled)
                            {
                                log.RunHandled(() =>
                                {
                                    XmlContainer datarow;
                                    if (row.RowState != DataRowState.Deleted)
                                    {
                                        datarow = DatasetTools.GetRowXml(row, DataRowVersion.Default);
                                    }
                                    else
                                    {
                                        datarow = DatasetTools.GetRowXml(row, DataRowVersion.Original);
                                    }
                                    log.DebugFormat("Evaluating ConditionFilter {0} of work queue class {1} for row {2}.",
                                        wqc.ConditionFilter, wqc.Path, datarow.Xml.OuterXml);
                                });
                            }
                            StringBuilder filterBuilder = new StringBuilder();
                            string filter;
                            // we create a dummy ds entity based on the row's table
                            DataStructureEntity dummyEntity = new DataStructureEntity();
                            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
                            dummyEntity.PersistenceProvider = ps.SchemaProvider;
                            dummyEntity.EntityId = entityId;
                            dummyEntity.AllFields = true;
                            dummyEntity.Name = row.Table.TableName;
                            // we get the condition filter string
                            dg.RenderFilter(filterBuilder, wqc.ConditionFilter, dummyEntity);
                            filter = filterBuilder.ToString();
                            DataRowVersion version = (row.RowState == DataRowState.Deleted 
                                ? DataRowVersion.Original : DataRowVersion.Default);
                            // we add the row's primary key to the filter
                            foreach (DataColumn col in row.Table.PrimaryKey)
                            {
                                filter += " AND " + col.ColumnName + " = " 
                                    + dg.RenderObject(row[col.ColumnName, version], 
                                    (OrigamDataType)row.Table.Columns[col.ColumnName].ExtendedProperties["OrigamDataType"]);
                            }
                            DataViewRowState state = (row.RowState == DataRowState.Deleted 
                                ? DataViewRowState.OriginalRows : DataViewRowState.CurrentRows);
                            // and we check if the row will pass through the filter
                            if (row.Table.Select(filter, "", state).Length == 1)
                            {
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug("ConditionFilter evaluated True.");
                                }
                                // it passed, so we evaluate this work queue as valid for this row
                                result.Add(wqr);
                            }
                            else
                            {
                                if (log.IsDebugEnabled)
                                {
                                    log.Debug("ConditionFilter evaluated False.");
                                }
                            }
                        }
                    }
                }
            }
        }
        return result.ToArray(typeof(WorkQueueData.WorkQueueRow)) as WorkQueueData.WorkQueueRow[];
    }
    private void StateMachineService_Initialize(object sender, EventArgs e)
    {
    }
}
