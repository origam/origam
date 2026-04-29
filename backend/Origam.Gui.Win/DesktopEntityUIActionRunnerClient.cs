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
using Origam.DA;
using Origam.Rule;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Gui.Win;

public class DesktopEntityUIActionRunnerClient : IEntityUIActionRunnerClient
{
    private readonly FormGenerator generator;
    private readonly DataSet dataSource;

    public DesktopEntityUIActionRunnerClient(FormGenerator generator, DataSet dataSource)
    {
        this.generator = generator;
        this.dataSource = dataSource;
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
        string dataTableName = entity.Split(separator: ".".ToCharArray()).Last();
        var processData = new ExecuteActionProcessData();
        processData.SessionFormIdentifier = sessionFormIdentifier;
        processData.RequestingGrid = requestingGrid;
        processData.ActionId = actionId;
        processData.Entity = dataTableName;
        processData.SelectedIds = selectedIds;
        processData.Type = (PanelActionType)
            Enum.Parse(enumType: typeof(PanelActionType), value: actionType);
        processData.DataTable = dataSource.Tables[name: dataTableName];
        IList<DataRow> rows = new List<DataRow>();
        foreach (string id in selectedIds)
        {
            DataRow row = null;
            if ((dataSource != null) && dataSource.Tables.Contains(name: dataTableName))
            {
                row = dataSource.Tables[name: dataTableName].Rows.Find(key: id);
            }
            if (row == null)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "id",
                    actualValue: id,
                    message: ResourceUtils.GetString(key: "ErrorRecordNotFound")
                );
            }
            rows.Add(item: row);
        }
        processData.SelectedRows = rows;
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

    public void CheckActionConditions(ExecuteActionProcessData processData) { }

    public void SetModalDialogSize(
        List<object> resultList,
        ExecuteActionProcessData processData
    ) { }

    public void ProcessWorkflowResults(
        UserProfile profile,
        ExecuteActionProcessData processData,
        DataSet sourceData,
        DataSet targetData,
        EntityWorkflowAction entityWorkflowAction,
        List<ChangeInfo> changes
    )
    {
        EntityWorkflowAction action = processData.Action as EntityWorkflowAction;
        try
        {
            targetData.EnforceConstraints = false;
            foreach (DataTable table in targetData.Tables)
            {
                table.BeginLoadData();
            }
            IDictionary<string, IList<KeyValuePair<object, DataMergeChange>>> changeList =
                new Dictionary<string, IList<KeyValuePair<object, DataMergeChange>>>();
            Dictionary<DataRow, List<DataRow>> deletedRowsParents = null;
            if (action.CleanDataBeforeMerge)
            {
                generator.FormRuleEngine.PauseRuleProcessing();
                targetData.AcceptChanges();
                DatasetTools.Clear(data: targetData);
            }
            deletedRowsParents = DatasetTools.GetDeletedRows(
                sourceData: sourceData,
                targetData: targetData
            );
            MergeParams mergeParams = new MergeParams(ProfileId: processData.Profile.Id);
            mergeParams.TrueDelete = action.MergeType == ServiceOutputMethod.FullMerge;
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
            if (
                !action.CleanDataBeforeMerge
                && DatasetTools.HasDataSetRules(dataSet: targetData, ruleSet: generator.RuleSet)
            )
            {
                ProcessRulesForWorkflowResults(
                    changeList: changeList,
                    targetData: targetData,
                    deletedRowsParents: deletedRowsParents
                );
            }
        }
        finally
        {
            generator.FormRuleEngine.ResumeRuleProcessing();
            foreach (DataTable table in targetData.Tables)
            {
                table.EndLoadData();
            }
            targetData.EnforceConstraints = true;
        }
    }

    private void ProcessRulesForWorkflowResults(
        IDictionary<string, IList<KeyValuePair<object, DataMergeChange>>> changeList,
        DataSet targetData,
        Dictionary<DataRow, List<DataRow>> deletedRowsParents
    )
    {
        // is rule handler right way, is conversion to xml data correct?
        DatasetRuleHandler ruleHandler = new DatasetRuleHandler();
        IDataDocument xmlData = DataDocumentFactory.New(dataSet: targetData);
        foreach (var entry in changeList)
        {
            string tableName = entry.Key;
            foreach (var rowEntry in entry.Value)
            {
                DataRow row = targetData.Tables[name: tableName].Rows.Find(key: rowEntry.Key);
                DataRowState changeType = rowEntry.Value.State;
                if (changeType == DataRowState.Added)
                {
                    ruleHandler.OnRowChanged(
                        e: new DataRowChangeEventArgs(row: row, action: DataRowAction.Add),
                        data: xmlData,
                        ruleSet: generator.RuleSet,
                        ruleEngine: generator.FormRuleEngine
                    );
                }
                switch (changeType)
                {
                    case DataRowState.Added:
                    {
                        ruleHandler.OnRowCopied(
                            row: row,
                            data: xmlData,
                            ruleSet: generator.RuleSet,
                            ruleEngine: generator.FormRuleEngine
                        );
                        break;
                    }

                    case DataRowState.Modified:
                    {
                        row.BeginEdit();
                        Hashtable changedColumns = rowEntry.Value.Columns;
                        if (changedColumns != null)
                        {
                            foreach (DictionaryEntry changedColumnEntry in changedColumns)
                            {
                                DataColumn changedColumn = (DataColumn)changedColumnEntry.Value;
                                object newValue = row[column: changedColumn];
                                ruleHandler.OnColumnChanged(
                                    e: new DataColumnChangeEventArgs(
                                        row: row,
                                        column: changedColumn,
                                        value: newValue
                                    ),
                                    data: xmlData,
                                    ruleSet: generator.RuleSet,
                                    ruleEngine: generator.FormRuleEngine
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
                            message: ResourceUtils.GetString(key: "ErrorUnknownRowChangeState")
                        );
                    }
                }
            }
        }
        foreach (var deletedItem in deletedRowsParents)
        {
            ruleHandler.OnRowDeleted(
                parentRows: deletedItem.Value.ToArray(),
                deletedRow: deletedItem.Key,
                data: xmlData,
                ruleSet: generator.RuleSet,
                ruleEngine: generator.FormRuleEngine
            );
        }
    }

    public void PostProcessWorkflowAction(
        DataSet data,
        EntityWorkflowAction entityWorkflowAction,
        List<ChangeInfo> changes
    ) { }

    public void ProcessModalDialogCloseType(
        ExecuteActionProcessData processData,
        EntityWorkflowAction entityWorkflowAction
    ) { }
}
