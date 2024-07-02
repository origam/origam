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
using Origam;
using Origam.DA;
using Origam.Rule;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;
using Origam.Gui;
using Origam.Server;
using Origam.Service.Core;


namespace Origam.Server;
public class ServerEntityUIActionRunnerClient: IEntityUIActionRunnerClient
{
    private readonly SessionManager sessionManager;
    private readonly SessionStore sessionStore;
    public ServerEntityUIActionRunnerClient(SessionManager sessionManager,
        string sessionFormIdentifier)
    {
        this.sessionManager = sessionManager;
        this.sessionStore = 
            sessionManager.GetSession(new Guid(sessionFormIdentifier));
    }
    public ServerEntityUIActionRunnerClient(SessionManager sessionManager, SessionStore sessionStore)
    {
        this.sessionManager = sessionManager;
        this.sessionStore = sessionStore;
    }
    public ExecuteActionProcessData CreateExecuteActionProcessData(
        string sessionFormIdentifier, string requestingGrid, 
        string actionType, string entity, List<string> selectedIds, 
        string actionId, Hashtable parameterMappings, 
        Hashtable inputParameters)
    {
        ExecuteActionProcessData processData = new ExecuteActionProcessData();
        processData.SessionFormIdentifier = sessionFormIdentifier;
        processData.RequestingGrid = requestingGrid;
        processData.ActionId = actionId;
        processData.IsModalDialog = this.sessionStore.IsModalDialog;
        processData.Entity = entity;
        processData.SelectedIds = selectedIds;
        processData.Type = (PanelActionType)Enum.Parse(
            typeof(PanelActionType), actionType);
        SessionStore sessionStore 
            = sessionManager.GetSession(new Guid(sessionFormIdentifier));
        processData.DataTable = sessionStore.GetDataTable(
            entity, sessionStore.Data);
        processData.SelectedRows = sessionStore.GetRows(entity, selectedIds);
        processData.ParameterService = ServiceManager.Services.GetService(
            typeof(IParameterService)) as IParameterService;
        if ((processData.Type != PanelActionType.QueueAction) 
        && (processData.Type != PanelActionType.SelectionDialogAction))
        {
            // get action
            processData.Action = UIActionTools.GetAction(actionId);
            // retrieve parameter mappings
            List<string> originalDataParameters 
                = UIActionTools.GetOriginalParameters(processData.Action);
            processData.Parameters = DatasetTools.RetrieveParameters(
                parameterMappings, processData.SelectedRows, originalDataParameters,
                processData.DataTable.DataSet);
            // add input parameters
            foreach (DictionaryEntry inputParameter in inputParameters)
            {
                processData.Parameters.Add(inputParameter.Key, inputParameter.Value);
            }
        }
        return processData;
    }
    public void CheckActionConditions(ExecuteActionProcessData processData)
    {
        if (processData.Action is EntityWorkflowAction ewa)
        {
            if (ewa.RequestSaveBeforeWorkflow
            && sessionManager.GetSession(processData).HasChanges())
            {
                throw new RuleException(Resources.ErrorFormNotSavedBeforeAction);
            }
            if (sessionManager.GetSession(processData).Data.HasErrors)
            {
                switch (ewa.CloseType)
                {
                    case ModalDialogCloseType.None:
                    // closing is not happening, no need for any action
                    case ModalDialogCloseType.CloseAndCancel:
                    // closing is happening, but data won't be merged,
                    // no need for any action
                    case ModalDialogCloseType.CloseAndCommitWithErrors:
                    // errors are ignored in order to enable merge of
                    // incomplete data
                    {
                        break;
                    }
                    case ModalDialogCloseType.CloseAndCommit:
                    // errors are reported
                    {
                        throw new RuleException(Resources.ErrorInForm);
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException(
                            "CloseType",
                            ewa.CloseType,
                            $@"Invalid CloseType value {ewa.CloseType}");
                    }
                }
            }
        }
    }
    public void SetModalDialogSize(List<object> resultList,ExecuteActionProcessData processData)
    {
        PanelActionResult result = (PanelActionResult)resultList[0];
        if ((processData.Action != null) 
            && (result.Request != null))
        {
            result.Request.IsModalDialog = processData.Action.IsModalDialog;
            if (processData.Action.IsModalDialog)
            {
                if (processData.Action.ModalDialogHeight == 0)
                {
                    result.Request.DialogHeight = 400;
                }
                else
                {
                    result.Request.DialogHeight = processData.Action.ModalDialogHeight;
                }
                if (processData.Action.ModalDialogWidth == 0)
                {
                    result.Request.DialogWidth = 400;
                }
                else
                {
                    result.Request.DialogWidth = processData.Action.ModalDialogWidth;
                }
            }
        }
    }
    public void ProcessWorkflowResults(UserProfile profile, ExecuteActionProcessData processData,
        DataSet sourceData,DataSet targetData,EntityWorkflowAction entityWorkflowAction,
        List<ChangeInfo> changes)
    {
        try
        {
            targetData.EnforceConstraints = false;
            foreach (DataTable t in targetData.Tables)
            {
                t.BeginLoadData();
            }
            IDictionary<string, IList<KeyValuePair<object, DataMergeChange>>> changeList =
                new Dictionary<string, IList<KeyValuePair<object, DataMergeChange>>>();
            Dictionary<DataRow, List<DataRow>> deletedRowsParents = null;
            if (entityWorkflowAction.CleanDataBeforeMerge)
            {
                targetData.AcceptChanges();
                DatasetTools.Clear(targetData);
                changes.Add(ChangeInfo.CleanDataChangeInfo());
            }
            deletedRowsParents = DatasetTools.GetDeletedRows(
                sourceData, targetData);
            MergeParams mergeParams = new MergeParams(profile.Id);
            mergeParams.TrueDelete 
                = entityWorkflowAction.MergeType == ServiceOutputMethod.FullMerge;
            DatasetTools.MergeDataSet(
                targetData, sourceData, changeList, mergeParams);
            // process rules (but not after clean merge, there we expect processed data
            // we process the rules after merge so all data were merged before we start firing any
            // events... If we would process rules WHILE merging, there would be a race condition - e.g.
            // a column change would trigger a rule recalculation of another column that has not been
            // merged yet, so after successful rule execution the column would be reset to the original
            // value when being merged, thus, resulting in a not-rule-processed data.
            if (!entityWorkflowAction.CleanDataBeforeMerge && sessionStore.HasRules)
            {
                try
                {
                    sessionStore.RegisterEvents();
                    foreach (var entry in changeList)
                    {
                        string tableName = entry.Key;
                        foreach (var rowEntry in entry.Value)
                        {
                            DataRow row = targetData.Tables[tableName].Rows.Find(rowEntry.Key);
                            DataRowState changeType = rowEntry.Value.State;
                            if (changeType == DataRowState.Added)
                            {
                                sessionStore.RuleHandler.OnRowChanged(new DataRowChangeEventArgs(row, DataRowAction.Add), sessionStore.XmlData, sessionStore.RuleSet, sessionStore.RuleEngine);
                            }
                            switch (changeType)
                            {
                                case DataRowState.Added:
                                    sessionStore.RuleHandler.OnRowCopied(row, sessionStore.XmlData, sessionStore.RuleSet, sessionStore.RuleEngine);
                                    break;
                                case DataRowState.Modified:
                                    row.BeginEdit();
                                    Hashtable changedColumns = rowEntry.Value.Columns;
                                    if (changedColumns != null)
                                    {
                                        foreach (DictionaryEntry changedColumnEntry in changedColumns)
                                        {
                                            DataColumn changedColumn = (DataColumn)changedColumnEntry.Value;
                                            object newValue = row[changedColumn];
                                            sessionStore.RuleHandler.OnColumnChanged(new DataColumnChangeEventArgs(row, changedColumn, newValue), sessionStore.XmlData, sessionStore.RuleSet, sessionStore.RuleEngine);
                                        }
                                    }
                                    row.EndEdit();
                                    break;
                                case DataRowState.Deleted:
                                    // deletions later
                                    break;
                                default:
                                    throw new Exception(Resources.ErrorUnknownRowChangeState);
                            }
                        }
                    }
                    foreach (var deletedItem in deletedRowsParents)
                    {
                        sessionStore.RuleHandler.OnRowDeleted(deletedItem.Value.ToArray(), deletedItem.Key, sessionStore.XmlData, sessionStore.RuleSet, sessionStore.RuleEngine);
                    }
                }
                finally
                {
                    sessionStore.UnregisterEvents();
                }
            }
            // in any case update the list row - we do not know if it was changed directly (will be in changelist)
            // or by a rule execution
            if (sessionStore.DataList != null)
            {
                DataRow rootRow = sessionStore.GetSessionRow(sessionStore.DataListEntity, sessionStore.CurrentRecordId);
                sessionStore.UpdateListRow(rootRow);
            }
            // read all changed keys to a table so getChangesRecursive skips all those that
            // have been directly changed and so will be processed anyway
            Hashtable ignoreKeys = new Hashtable();
            foreach (var entry in changeList)
            {
                foreach (var rowEntry in entry.Value)
                {
                    // table name + row id
                    ignoreKeys[entry.Key + rowEntry.Key] = null;
                }
            }
            bool hasErrors = sessionStore.Data.HasErrors;
            bool hasChanges = sessionStore.Data.HasChanges();
            foreach (var entry in changeList)
            {
                string tableName = entry.Key;
                foreach (var rowEntry in entry.Value)
                {
                    DataRow row = targetData.Tables[tableName].Rows.Find(rowEntry.Key);
                    DataRowState changeType = rowEntry.Value.State;
                    if (entityWorkflowAction.CleanDataBeforeMerge)
                    {
                        changeType = DataRowState.Added;
                    }
                    switch (changeType)
                    {
                        case DataRowState.Added:
                            changes.AddRange(sessionStore.GetChanges(
                                entity: tableName, 
                                id: rowEntry.Key, 
                                operation: Operation.Create, 
                                ignoreKeys: ignoreKeys, 
                                includeRowStates: true, 
                                hasErrors: hasErrors, 
                                hasChanges: hasChanges));
                            break;
                        case DataRowState.Modified:
                            changes.AddRange(sessionStore.GetChanges(
                                entity: tableName, 
                                id: rowEntry.Key, 
                                operation: Operation.Update, 
                                ignoreKeys: ignoreKeys, 
                                includeRowStates: true, 
                                hasErrors: hasErrors, 
                                hasChanges: hasChanges));
                            break;
                        case DataRowState.Deleted:
                            changes.Add(new ChangeInfo
                            {
                                Entity = tableName,
                                Operation = Operation.Delete,
                                ObjectId = rowEntry.Key
                            });
                            break;
                        default:
                            throw new Exception(Resources.ErrorUnknownRowChangeState);
                    }
                }
            }
        }
        finally
        {
            foreach (DataTable t in targetData.Tables)
            {
                t.EndLoadData();
            }
            targetData.EnforceConstraints = true;
        }
    }
    
    private static void AddSavedInfo(List<ChangeInfo> changes)
    {
        int i = 0;
        while (changes.Count > i)
        {
            if (changes[i].Operation == Operation.FormSaved)
            {
                changes.RemoveAt(i);
            }
            else
            {
                i++;
            }
        }
        changes.Add(ChangeInfo.SavedChangeInfo());
    }
    public void PostProcessWorkflowAction(DataSet data,
        EntityWorkflowAction entityWorkflowAction, List<ChangeInfo> changes)
    {
        if (entityWorkflowAction.SaveAfterWorkflow)
        {
            var saveChanges = sessionStore.ExecuteAction(SessionStore.ACTION_SAVE) as List<ChangeInfo>;
            changes.AddRange(saveChanges);
            // notify the form that the content has been saved, so it can hide the dirty sign (*)
            AddSavedInfo(changes);
        }
        if (entityWorkflowAction.CommitChangesAfterMerge)
        {
            data.AcceptChanges();
            AddSavedInfo(changes);
        }
        switch (entityWorkflowAction.RefreshAfterWorkflow)
        {
            case SaveRefreshType.RefreshCompleteForm:
                changes.Add(ChangeInfo.RefreshFormChangeInfo());
                break;
            case SaveRefreshType.ReloadActualRecord:
                changes.Add(ChangeInfo.ReloadCurrentRecordChangeInfo());
                break;
        }
        if (entityWorkflowAction.RefreshPortalAfterFinish)
        {
            changes.Add(ChangeInfo.RefreshPortalInfo());
        }
    }
    public void ProcessModalDialogCloseType(ExecuteActionProcessData processData,
        EntityWorkflowAction entityWorkflowAction)
    {
        switch(entityWorkflowAction.CloseType)
        {
            case ModalDialogCloseType.CloseAndCommit:
            case ModalDialogCloseType.CloseAndCommitWithErrors:
            {
                sessionManager.GetSession(processData)
                              .IsModalDialogCommited = true;
                break;
            }
            case ModalDialogCloseType.CloseAndCancel:
            {
                sessionManager.GetSession(processData)
                              .IsModalDialogCommited = false;
                break;
            }
        }
    }
}
