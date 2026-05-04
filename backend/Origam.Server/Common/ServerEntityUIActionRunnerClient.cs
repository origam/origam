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
using Origam.DA;
using Origam.Gui;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Server;

public class ServerEntityUIActionRunnerClient : IEntityUIActionRunnerClient
{
    private readonly SessionManager sessionManager;
    private readonly SessionStore sessionStore;

    public ServerEntityUIActionRunnerClient(
        SessionManager sessionManager,
        string sessionFormIdentifier
    )
    {
        this.sessionManager = sessionManager;
        this.sessionStore = sessionManager.GetSession(
            sessionFormIdentifier: new Guid(g: sessionFormIdentifier)
        );
    }

    public ServerEntityUIActionRunnerClient(
        SessionManager sessionManager,
        SessionStore sessionStore
    )
    {
        this.sessionManager = sessionManager;
        this.sessionStore = sessionStore;
    }

    public ExecuteActionProcessData CreateExecuteActionProcessData(
        string sessionFormIdentifier,
        string requestingGrid,
        string actionType,
        string entity,
        List<string> selectedIds,
        string actionId,
        Hashtable parameterMappings,
        Hashtable inputParameters
    )
    {
        ExecuteActionProcessData processData = new ExecuteActionProcessData();
        processData.SessionFormIdentifier = sessionFormIdentifier;
        processData.RequestingGrid = requestingGrid;
        processData.ActionId = actionId;
        processData.IsModalDialog = this.sessionStore.IsModalDialog;
        processData.Entity = entity;
        processData.SelectedIds = selectedIds;
        processData.Type = (PanelActionType)
            Enum.Parse(enumType: typeof(PanelActionType), value: actionType);
        SessionStore sessionStore = sessionManager.GetSession(
            sessionFormIdentifier: new Guid(g: sessionFormIdentifier)
        );
        processData.DataTable = sessionStore.GetDataTable(entity: entity, data: sessionStore.Data);
        processData.SelectedRows = sessionStore.GetRows(entity: entity, ids: selectedIds);
        processData.ParameterService =
            ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
            as IParameterService;
        if (
            (processData.Type != PanelActionType.QueueAction)
            && (processData.Type != PanelActionType.SelectionDialogAction)
        )
        {
            // get action
            processData.Action = UIActionTools.GetAction(action: actionId);
            // retrieve parameter mappings
            List<string> originalDataParameters = UIActionTools.GetOriginalParameters(
                action: processData.Action
            );
            processData.Parameters = DatasetTools.RetrieveParameters(
                parameterMappings: parameterMappings,
                rows: processData.SelectedRows,
                originalDataParameters: originalDataParameters,
                fullData: processData.DataTable.DataSet
            );
            // add input parameters
            foreach (DictionaryEntry inputParameter in inputParameters)
            {
                processData.Parameters.Add(key: inputParameter.Key, value: inputParameter.Value);
            }
        }
        return processData;
    }

    public void CheckActionConditions(ExecuteActionProcessData processData)
    {
        if (processData.Action is EntityWorkflowAction ewa)
        {
            if (
                ewa.RequestSaveBeforeWorkflow
                && sessionManager.GetSession(processData: processData).HasChanges()
            )
            {
                throw new RuleException(message: Resources.ErrorFormNotSavedBeforeAction);
            }
            if (sessionManager.GetSession(processData: processData).Data.HasErrors)
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
                        throw new RuleException(message: Resources.ErrorInForm);
                    }
                    default:
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "CloseType",
                            actualValue: ewa.CloseType,
                            message: $@"Invalid CloseType value {ewa.CloseType}"
                        );
                    }
                }
            }
        }
    }

    public void SetModalDialogSize(List<object> resultList, ExecuteActionProcessData processData)
    {
        PanelActionResult result = (PanelActionResult)resultList[index: 0];
        if ((processData.Action != null) && (result.Request != null))
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

    public void ProcessWorkflowResults(
        UserProfile profile,
        ExecuteActionProcessData processData,
        DataSet sourceData,
        DataSet targetData,
        EntityWorkflowAction entityWorkflowAction,
        List<ChangeInfo> changes
    )
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
                DatasetTools.Clear(data: targetData);
                changes.Add(item: ChangeInfo.CleanDataChangeInfo());
            }
            deletedRowsParents = DatasetTools.GetDeletedRows(
                sourceData: sourceData,
                targetData: targetData
            );
            MergeParams mergeParams = new MergeParams(ProfileId: profile.Id);
            mergeParams.TrueDelete =
                entityWorkflowAction.MergeType == ServiceOutputMethod.FullMerge;
            DatasetTools.MergeDataSet(
                inout_dsTarget: targetData,
                in_dsSource: sourceData,
                changeList: changeList,
                mergeParams: mergeParams
            );
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
                            DataRow row = targetData
                                .Tables[name: tableName]
                                .Rows.Find(key: rowEntry.Key);
                            DataRowState changeType = rowEntry.Value.State;
                            if (changeType == DataRowState.Added)
                            {
                                sessionStore.RuleHandler.OnRowChanged(
                                    e: new DataRowChangeEventArgs(
                                        row: row,
                                        action: DataRowAction.Add
                                    ),
                                    data: sessionStore.XmlData,
                                    ruleSet: sessionStore.RuleSet,
                                    ruleEngine: sessionStore.RuleEngine
                                );
                            }
                            switch (changeType)
                            {
                                case DataRowState.Added:
                                {
                                    sessionStore.RuleHandler.OnRowCopied(
                                        row: row,
                                        data: sessionStore.XmlData,
                                        ruleSet: sessionStore.RuleSet,
                                        ruleEngine: sessionStore.RuleEngine
                                    );
                                    break;
                                }

                                case DataRowState.Modified:
                                {
                                    row.BeginEdit();
                                    Hashtable changedColumns = rowEntry.Value.Columns;
                                    if (changedColumns != null)
                                    {
                                        foreach (
                                            DictionaryEntry changedColumnEntry in changedColumns
                                        )
                                        {
                                            DataColumn changedColumn = (DataColumn)
                                                changedColumnEntry.Value;
                                            object newValue = row[column: changedColumn];
                                            sessionStore.RuleHandler.OnColumnChanged(
                                                e: new DataColumnChangeEventArgs(
                                                    row: row,
                                                    column: changedColumn,
                                                    value: newValue
                                                ),
                                                data: sessionStore.XmlData,
                                                ruleSet: sessionStore.RuleSet,
                                                ruleEngine: sessionStore.RuleEngine
                                            );
                                        }
                                    }
                                    row.EndEdit();
                                    break;
                                }

                                case DataRowState.Deleted:
                                {
                                    // deletions later
                                    break;
                                }
                                default:
                                {
                                    throw new Exception(
                                        message: Resources.ErrorUnknownRowChangeState
                                    );
                                }
                            }
                        }
                    }
                    foreach (var deletedItem in deletedRowsParents)
                    {
                        sessionStore.RuleHandler.OnRowDeleted(
                            parentRows: deletedItem.Value.ToArray(),
                            deletedRow: deletedItem.Key,
                            data: sessionStore.XmlData,
                            ruleSet: sessionStore.RuleSet,
                            ruleEngine: sessionStore.RuleEngine
                        );
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
                DataRow rootRow = sessionStore.GetSessionRow(
                    entity: sessionStore.DataListEntity,
                    id: sessionStore.CurrentRecordId
                );
                sessionStore.UpdateListRow(r: rootRow);
            }
            // read all changed keys to a table so getChangesRecursive skips all those that
            // have been directly changed and so will be processed anyway
            Hashtable ignoreKeys = new Hashtable();
            foreach (var entry in changeList)
            {
                foreach (var rowEntry in entry.Value)
                {
                    // table name + row id
                    ignoreKeys[key: entry.Key + rowEntry.Key] = null;
                }
            }
            bool hasErrors = sessionStore.Data.HasErrors;
            bool hasChanges = sessionStore.Data.HasChanges();
            foreach (var entry in changeList)
            {
                string tableName = entry.Key;
                foreach (var rowEntry in entry.Value)
                {
                    DataRow row = targetData.Tables[name: tableName].Rows.Find(key: rowEntry.Key);
                    DataRowState changeType = rowEntry.Value.State;
                    if (entityWorkflowAction.CleanDataBeforeMerge)
                    {
                        changeType = DataRowState.Added;
                    }
                    switch (changeType)
                    {
                        case DataRowState.Added:
                        {
                            changes.AddRange(
                                collection: sessionStore.GetChanges(
                                    entity: tableName,
                                    id: rowEntry.Key,
                                    operation: Operation.Create,
                                    ignoreKeys: ignoreKeys,
                                    includeRowStates: true,
                                    hasErrors: hasErrors,
                                    hasChanges: hasChanges
                                )
                            );
                            break;
                        }

                        case DataRowState.Modified:
                        {
                            changes.AddRange(
                                collection: sessionStore.GetChanges(
                                    entity: tableName,
                                    id: rowEntry.Key,
                                    operation: Operation.Update,
                                    ignoreKeys: ignoreKeys,
                                    includeRowStates: true,
                                    hasErrors: hasErrors,
                                    hasChanges: hasChanges
                                )
                            );
                            break;
                        }

                        case DataRowState.Deleted:
                        {
                            changes.Add(
                                item: new ChangeInfo
                                {
                                    Entity = tableName,
                                    Operation = Operation.Delete,
                                    ObjectId = rowEntry.Key,
                                }
                            );
                            break;
                        }

                        default:
                        {
                            throw new Exception(message: Resources.ErrorUnknownRowChangeState);
                        }
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
            if (changes[index: i].Operation == Operation.FormSaved)
            {
                changes.RemoveAt(index: i);
            }
            else
            {
                i++;
            }
        }
        changes.Add(item: ChangeInfo.SavedChangeInfo());
    }

    public void PostProcessWorkflowAction(
        DataSet data,
        EntityWorkflowAction entityWorkflowAction,
        List<ChangeInfo> changes
    )
    {
        if (entityWorkflowAction.SaveAfterWorkflow)
        {
            var saveChanges =
                sessionStore.ExecuteAction(actionId: SessionStore.ACTION_SAVE) as List<ChangeInfo>;
            changes.AddRange(collection: saveChanges);
            // notify the form that the content has been saved, so it can hide the dirty sign (*)
            AddSavedInfo(changes: changes);
        }
        if (entityWorkflowAction.CommitChangesAfterMerge)
        {
            data.AcceptChanges();
            AddSavedInfo(changes: changes);
        }
        switch (entityWorkflowAction.RefreshAfterWorkflow)
        {
            case SaveRefreshType.RefreshCompleteForm:
            {
                changes.Add(item: ChangeInfo.RefreshFormChangeInfo());
                break;
            }

            case SaveRefreshType.ReloadActualRecord:
            {
                changes.Add(item: ChangeInfo.ReloadCurrentRecordChangeInfo());
                break;
            }
        }
        if (entityWorkflowAction.RefreshPortalAfterFinish)
        {
            changes.Add(item: ChangeInfo.RefreshPortalInfo());
        }
    }

    public void ProcessModalDialogCloseType(
        ExecuteActionProcessData processData,
        EntityWorkflowAction entityWorkflowAction
    )
    {
        switch (entityWorkflowAction.CloseType)
        {
            case ModalDialogCloseType.CloseAndCommit:
            case ModalDialogCloseType.CloseAndCommitWithErrors:
            {
                sessionManager.GetSession(processData: processData).IsModalDialogCommited = true;
                break;
            }
            case ModalDialogCloseType.CloseAndCancel:
            {
                sessionManager.GetSession(processData: processData).IsModalDialogCommited = false;
                break;
            }
        }
    }
}
