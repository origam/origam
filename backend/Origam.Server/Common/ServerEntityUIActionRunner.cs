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
using System.Data;
using Origam.Extensions;
using Origam.Gui;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

public class ServerEntityUIActionRunner : EntityUIActionRunner
{
    protected readonly UIManager uiManager;
    protected readonly SessionManager sessionManager;
    protected readonly IBasicUIService basicUIService;
    protected readonly IReportManager reportManager;

    public ServerEntityUIActionRunner(
        IEntityUIActionRunnerClient actionRunnerClient,
        UIManager uiManager,
        SessionManager sessionManager,
        IBasicUIService basicUIService,
        IReportManager reportManager
    )
        : base(actionRunnerClient: actionRunnerClient)
    {
        this.uiManager = uiManager;
        this.sessionManager = sessionManager;
        this.basicUIService = basicUIService;
        this.reportManager = reportManager;
    }

    protected override void PerformAppropriateAction(ExecuteActionProcessData processData)
    {
        switch (processData.Type)
        {
            case PanelActionType.QueueAction:
            {
                ExecuteQueueAction(processData: processData);
                break;
            }

            case PanelActionType.Report:
            {
                ExecuteReportAction(processData: processData);
                break;
            }

            case PanelActionType.Workflow:
            {
                ExecuteWorkflowAction(processData: processData);
                break;
            }

            case PanelActionType.ChangeUI:
            {
                ExecuteChangeUIAction(processData: processData);
                break;
            }

            case PanelActionType.OpenForm:
            {
                ExecuteOpenFormAction(processData: processData);
                break;
            }

            case PanelActionType.SelectionDialogAction:
            {
                ExecuteSelectionDialogAction(processData: processData);
                break;
            }

            default:
            {
                throw new NotImplementedException();
            }
        }
    }

    private static void CheckSelectedRowsCountPositive(int count)
    {
        if (count == 0)
        {
            throw new RuleException(message: Resources.ErrorNoRecordsSelectedForAction);
        }
    }

    private void ExecuteQueueAction(ExecuteActionProcessData processData)
    {
        WorkQueueSessionStore wqss =
            sessionManager.GetSession(processData: processData) as WorkQueueSessionStore;
        IWorkQueueService wqs =
            ServiceManager.Services.GetService(serviceType: typeof(IWorkQueueService))
            as IWorkQueueService;
        if (processData.SelectedRows.Count == 0 && processData.SelectedIds.Count > 0)
        {
            throw new Exception(message: "Rows for the selected Ids were not loaded");
        }
        DataSet copy = processData.DataTable.DataSet.Clone();
        foreach (DataRow selectedRow in processData.SelectedRows)
        {
            copy.Tables[name: processData.DataTable.TableName]
                .LoadDataRow(values: selectedRow.ItemArray, fAcceptChanges: true);
        }
        DataTable selectedRows = copy.Tables[name: processData.DataTable.TableName];
        DataSet command = DataService.Instance.LoadData(
            dataStructureId: new Guid(g: "1d33b667-ca76-4aaa-a47d-0e404ed6f8a6"),
            methodId: new Guid(g: "6eefc3cf-6b6e-4d40-81f7-5c37a81e8a01"),
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            paramName1: "WorkQueueCommand_parId",
            paramValue1: processData.ActionId
        );
        if (command.Tables[name: "WorkQueueCommand"].Rows.Count == 0)
        {
            throw new Exception(message: Resources.ErrorWorkQueueCommandNotFound);
        }
        DataRow cmdRow = command.Tables[name: "WorkQueueCommand"].Rows[index: 0];
        // work queue command
        if (
            (Guid)cmdRow[columnName: "refWorkQueueCommandTypeId"]
            == (Guid)
                processData.ParameterService.GetParameterValue(
                    parameterName: "WorkQueueCommandType_WorkQueueClassCommand"
                )
        )
        {
            if (processData.Action == null || processData.Action.Mode != PanelActionMode.Always)
            {
                CheckSelectedRowsCountPositive(count: processData.SelectedIds.Count);
            }
            WorkQueueWorkflowCommand cmd = wqss.WorkQueueClass.GetCommand(
                name: (string)cmdRow[columnName: "Command"]
            );
            // We handle the UI actions, work queue service will handle all the other background actions
            if (cmd.ActionType == PanelActionType.OpenForm)
            {
                foreach (WorkQueueWorkflowCommandParameterMapping pm in cmd.ParameterMappings)
                {
                    object val;
                    switch (pm.Value)
                    {
                        case WorkQueueCommandParameterMappingType.QueueEntries:
                        {
                            val = DataDocumentFactory.New(dataSet: selectedRows.DataSet);
                            break;
                        }

                        case WorkQueueCommandParameterMappingType.Parameter1:
                        {
                            val = cmdRow.IsNull(columnName: "Param1")
                                ? null
                                : cmdRow[columnName: "Param1"];
                            break;
                        }

                        case WorkQueueCommandParameterMappingType.Parameter2:
                        {
                            val = cmdRow.IsNull(columnName: "Param2")
                                ? null
                                : cmdRow[columnName: "Param2"];
                            break;
                        }

                        default:
                        {
                            throw new ArgumentOutOfRangeException(
                                paramName: "Value",
                                actualValue: pm.Value,
                                message: Resources.ErrorUnknownWorkQueueCommandValueType
                            );
                        }
                    }
                    processData.Parameters.Add(key: pm.Name, value: val);
                }
                // resend the execute - now with the actual action and with queue-command parameters
                ExecuteAction(
                    sessionFormIdentifier: processData.SessionFormIdentifier,
                    requestingGrid: processData.RequestingGrid,
                    entity: processData.Entity,
                    actionType: cmd.ActionType.ToString(),
                    actionId: cmd.Id.ToString(),
                    parameterMappings: new Hashtable(),
                    selectedIds: processData.SelectedIds,
                    inputParameters: processData.Parameters
                );
                return;
            }
        }
        // otherwise we ask the work queue service to process the command
        wqs.HandleAction(
            queueId: new Guid(g: wqss.Request.ObjectId),
            workQueueClassIdentifier: wqss.WorkQueueClass.Name,
            selectedRows: selectedRows,
            commandType: (Guid)cmdRow[columnName: "refWorkQueueCommandTypeId"],
            command: cmdRow.IsNull(columnName: "Command")
                ? null
                : (string)cmdRow[columnName: "Command"],
            param1: cmdRow.IsNull(columnName: "Param1")
                ? null
                : (string)cmdRow[columnName: "Param1"],
            param2: cmdRow.IsNull(columnName: "Param2")
                ? null
                : (string)cmdRow[columnName: "Param2"],
            errorQueueId: cmdRow.IsNull(columnName: "refErrorWorkQueueId")
                ? null
                : cmdRow[columnName: "refErrorWorkQueueId"]
        );
        resultList.Add(item: new PanelActionResult(type: ActionResultType.RefreshData));
    }

    private void ExecuteReportAction(ExecuteActionProcessData processData)
    {
        if (processData.Action == null || processData.Action.Mode != PanelActionMode.Always)
        {
            CheckSelectedRowsCountPositive(count: processData.SelectedIds.Count);
            if (processData.SelectedIds.Count > 1)
            {
                throw new Exception(message: Resources.ErrorChangeUIMultipleRecords);
            }
        }
        var reportAction = processData.Action as EntityReportAction;
        var result = new PanelActionResult(type: ActionResultType.OpenUrl)
        {
            Url = reportManager.GetReportStandalone(
                reportId: reportAction.ReportId.ToString(),
                parameters: processData.Parameters,
                dataReportExportFormatType: reportAction.ExportFormatType
            ),
        };
        switch (reportAction.Report)
        {
            case WebReport webReport:
            {
                result.UrlOpenMethod = webReport.OpenMethod.ToString();
                break;
            }

            case FileSystemReport _:
            {
                result.UrlOpenMethod = WebPageOpenMethod.NoUI.ToString();
                break;
            }
        }
        result.Request = new UIRequest { Caption = processData.Action.Caption };
        if (processData.Action.RefreshAfterReturn != ReturnRefreshType.None)
        {
            result.RefreshOnReturnSessionId = processData.SessionFormIdentifier;
            result.RefreshOnReturnType = processData.Action.RefreshAfterReturn.ToString();
        }
        resultList.Add(item: result);
    }

    private async void ExecuteChangeUIAction(ExecuteActionProcessData processData)
    {
        if (processData.Action == null || processData.Action.Mode != PanelActionMode.Always)
        {
            CheckSelectedRowsCountPositive(count: processData.SelectedIds.Count);
            if (processData.SelectedIds.Count > 1)
            {
                throw new Exception(message: Resources.ErrorChangeUIMultipleRecords);
            }
        }
        PanelActionResult result = new PanelActionResult(type: ActionResultType.ChangeUI);
        UIRequest uir = RequestTools.GetActionRequest(
            parameters: processData.Parameters,
            selectedItems: processData.SelectedIds,
            action: processData.Action
        );
        uir.FormSessionId = processData.SessionFormIdentifier;
        uir.RegisterSession = false;
        result.UIResult = uiManager.InitUI(
            request: uir,
            addChildSession: true,
            parentSession: sessionManager.GetSession(processData: processData),
            basicUIService: basicUIService
        );
        resultList.Add(item: result);
        await System.Threading.Tasks.Task.CompletedTask; //CS1998
    }

    protected override void ExecuteOpenFormAction(ExecuteActionProcessData processData)
    {
        if (processData.Action == null || processData.Action.Mode != PanelActionMode.Always)
        {
            CheckSelectedRowsCountPositive(count: processData.SelectedIds.Count);
        }
        PanelActionResult result = new PanelActionResult(type: ActionResultType.OpenForm);
        UIRequest uir = RequestTools.GetActionRequest(
            parameters: processData.Parameters,
            selectedItems: processData.SelectedIds,
            action: processData.Action
        );
        // Stack can't handle resending DataDocumentFx,
        // it needs to be converted to XmlDocument
        // and then converted back to DataDocumentFx
        foreach (object key in uir.Parameters.Keys.CastToList<object>())
        {
            if (uir.Parameters[key: key] is IXmlContainer)
            {
                uir.Parameters[key: key] = ((IXmlContainer)uir.Parameters[key: key]).Xml;
            }
        }
        if (processData.Action.RefreshAfterReturn == ReturnRefreshType.MergeModalDialogChanges)
        {
            uir.ParentSessionId = processData.SessionFormIdentifier;
            uir.SourceActionId = processData.Action.Id.ToString();
        }
        result.Request = uir;
        if (processData.Action.RefreshAfterReturn != ReturnRefreshType.None)
        {
            result.RefreshOnReturnSessionId = processData.SessionFormIdentifier;
            result.RefreshOnReturnType = processData.Action.RefreshAfterReturn.ToString();
        }
        resultList.Add(item: result);
        var entityWorkflowAction = processData.Action as EntityWorkflowAction;
        if (
            entityWorkflowAction != null
            && entityWorkflowAction.CloseType != ModalDialogCloseType.None
        )
        {
            resultList.Add(item: new PanelActionResult(type: ActionResultType.DestroyForm));
        }
        else if (entityWorkflowAction == null && processData.IsModalDialog)
        {
            resultList.Add(item: new PanelActionResult(type: ActionResultType.DestroyForm));
        }
        if (entityWorkflowAction != null)
        {
            AppendScriptCalls(entityWorkflowAction: entityWorkflowAction, processData: processData);
        }
    }

    protected virtual void ExecuteSelectionDialogAction(ExecuteActionProcessData processData)
    {
        if (processData.Action == null || processData.Action.Mode != PanelActionMode.Always)
        {
            CheckSelectedRowsCountPositive(count: processData.SelectedIds.Count);
        }
        resultList.Add(
            item: sessionManager
                .GetSession(processData: processData)
                .ExecuteAction(actionId: processData.ActionId)
        );
    }

    protected override void SetTransactionId(
        ExecuteActionProcessData processData,
        string transactionId
    )
    {
        sessionManager.GetSession(processData: processData).TransationId = transactionId;
    }

    protected override ActionResult MakeActionResult(ActionResultType type)
    {
        return new PanelActionResult(type: type);
    }
}
