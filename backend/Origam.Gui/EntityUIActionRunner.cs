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
using Origam.Rule;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using Origam.Workflow;

namespace Origam.Gui;

public abstract class EntityUIActionRunner
{
    protected readonly IEntityUIActionRunnerClient actionRunnerClient;
    protected readonly List<object> resultList = new();

    public EntityUIActionRunner(IEntityUIActionRunnerClient actionRunnerClient)
    {
        this.actionRunnerClient = actionRunnerClient;
    }

    public IList ExecuteAction(
        string sessionFormIdentifier,
        string requestingGrid,
        string entity,
        string actionType,
        string actionId,
        Hashtable parameterMappings,
        List<string> selectedIds,
        Hashtable inputParameters
    )
    {
        ExecuteActionProcessData processData = actionRunnerClient.CreateExecuteActionProcessData(
            sessionFormIdentifier: sessionFormIdentifier,
            requestingGrid: requestingGrid,
            actionType: actionType,
            entity: entity,
            selectedIds: selectedIds,
            actionId: actionId,
            parameterMappings: parameterMappings,
            inputParameters: inputParameters
        );
        actionRunnerClient.CheckActionConditions(processData: processData);
        PerformAppropriateAction(processData: processData);
        actionRunnerClient.SetModalDialogSize(results: resultList, processData: processData);
        return resultList;
    }

    protected virtual void PerformAppropriateAction(ExecuteActionProcessData processData)
    {
        switch (processData.Type)
        {
            case PanelActionType.Workflow:
            {
                ExecuteWorkflowAction(processData: processData);
                break;
            }

            case PanelActionType.Report:
            {
                ExecuteOpenFormAction(processData: processData);
                break;
            }

            case PanelActionType.ChangeUI:
            {
                throw new NotImplementedException();
            }
            case PanelActionType.OpenForm:
            {
                ExecuteOpenFormAction(processData: processData);
                break;
            }

            case PanelActionType.SelectionDialogAction:
            {
                throw new NotImplementedException();
            }
            default:
            {
                throw new NotImplementedException();
            }
        }
    }

    protected abstract void ExecuteOpenFormAction(ExecuteActionProcessData processData);

    protected void ExecuteWorkflowAction(ExecuteActionProcessData processData)
    {
        if ((processData.Action == null) || (processData.Action.Mode != PanelActionMode.Always))
        {
            CheckSelectedRowsCountPositive(count: processData.SelectedIds.Count);
        }
        ActionResult result = MakeActionResult(type: ActionResultType.UpdateData);
        if (!(processData.Action is EntityWorkflowAction entityWorkflowAction))
        {
            throw new ArgumentOutOfRangeException(
                paramName: "action",
                actualValue: processData.Action,
                message: Properties.Resources.ErrorWorkflowActionNotSupported
            );
        }
        if (entityWorkflowAction.Workflow == null)
        {
            throw new NullReferenceException(message: Properties.Resources.ErrorWorkflowNotSet);
        }
        Key resultContextKey = null;
        foreach (
            var mapping in entityWorkflowAction.ChildItemsByType<EntityUIActionParameterMapping>(
                itemType: EntityUIActionParameterMapping.CategoryConst
            )
        )
        {
            if (DatasetTools.IsParameterViableAsResultContext(mapping: mapping))
            {
                foreach (
                    var store in entityWorkflowAction.Workflow.ChildItemsByType<ContextStore>(
                        itemType: ContextStore.CategoryConst
                    )
                )
                {
                    if (store.Name == mapping.Name)
                    {
                        resultContextKey = store.PrimaryKey;
                        break;
                    }
                }
            }
        }
        var changes = new List<ChangeInfo>();
        var transactionId = Guid.NewGuid().ToString();
        SetTransactionId(processData: processData, transactionId: transactionId);
        try
        {
            WorkflowEngine engine = WorkflowEngine.PrepareWorkflow(
                workflow: entityWorkflowAction.Workflow,
                parameters: new Hashtable(d: processData.Parameters),
                isRepeatable: false,
                titleName: ""
            );
            engine.SetTransactionId(
                transactionId: transactionId,
                transactionBehavior: entityWorkflowAction.Workflow.TransactionBehavior
            );
            using (WorkflowHost host = new WorkflowHost())
            {
                host.SupportsUI = false;
                WorkflowCallbackHandler handler = new WorkflowCallbackHandler(
                    host: host,
                    workflowInstanceId: engine.WorkflowInstanceId
                );
                handler.Subscribe();
                host.ExecuteWorkflow(engine: engine);
                handler.Event.WaitOne();
                if (handler.Result.Exception != null)
                {
                    throw handler.Result.Exception;
                }
                actionRunnerClient.ProcessModalDialogCloseType(
                    processData: processData,
                    entityWorkflowAction: entityWorkflowAction
                );
                if (
                    (resultContextKey != null)
                    && (entityWorkflowAction.MergeType != ServiceOutputMethod.Ignore)
                )
                {
                    DataSet sourceData = (
                        engine.RuleEngine.GetContext(key: resultContextKey) as IDataDocument
                    ).DataSet;
                    actionRunnerClient.ProcessWorkflowResults(
                        profile: processData.Profile,
                        processData: processData,
                        sourceData: sourceData,
                        targetData: processData.DataTable.DataSet,
                        entityWorkflowAction: entityWorkflowAction,
                        changes: changes
                    );
                }
                actionRunnerClient.PostProcessWorkflowAction(
                    data: processData.DataTable.DataSet,
                    entityWorkflowAction: entityWorkflowAction,
                    changes: changes
                );
            }
            ResourceMonitor.Commit(transactionId: transactionId);
            result.Changes = changes;
            resultList.Add(item: result);
            AppendScriptCalls(entityWorkflowAction: entityWorkflowAction, processData: processData);
            // add a special result to close the screen
            if (entityWorkflowAction.CloseType != ModalDialogCloseType.None)
            {
                resultList.Add(item: MakeActionResult(type: ActionResultType.DestroyForm));
            }
        }
        catch
        {
            ResourceMonitor.Rollback(transactionId: transactionId);
            throw;
        }
        finally
        {
            SetTransactionId(processData: processData, transactionId: null);
        }
    }

    protected virtual ActionResult MakeActionResult(ActionResultType type)
    {
        return new ActionResult(type: type);
    }

    protected abstract void SetTransactionId(
        ExecuteActionProcessData processData,
        string transactionId
    );

    protected void AppendScriptCalls(
        EntityWorkflowAction entityWorkflowAction,
        ExecuteActionProcessData processData
    )
    {
        var scriptCalls = entityWorkflowAction.ChildItemsByType<EntityWorkflowActionScriptCall>(
            itemType: EntityWorkflowActionScriptCall.CategoryConst
        );
        if (scriptCalls.Count == 0)
        {
            return;
        }
        scriptCalls.Sort(comparer: new EntityWorkflowActionScriptCallComparer());
        foreach (EntityWorkflowActionScriptCall scriptCall in scriptCalls)
        {
            if (IsScriptAllowed(scriptCall: scriptCall, processData: processData))
            {
                ActionResult result = MakeActionResult(type: ActionResultType.Script);
                result.Script = scriptCall.Script;
                resultList.Add(item: result);
            }
        }
    }

    private static void CheckSelectedRowsCountPositive(int count)
    {
        if (count == 0)
        {
            throw new RuleException(message: Properties.Resources.ErrorNoRecordsSelectedForAction);
        }
    }

    private bool IsScriptAllowed(
        EntityWorkflowActionScriptCall scriptCall,
        ExecuteActionProcessData processData
    )
    {
        // check features
        IParameterService param =
            ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
            as IParameterService;
        if (!param.IsFeatureOn(featureCode: scriptCall.Features))
        {
            return false;
        }
        // check roles
        IOrigamAuthorizationProvider authorizationProvider =
            SecurityManager.GetAuthorizationProvider();
        if (
            !authorizationProvider.Authorize(
                principal: SecurityManager.CurrentPrincipal,
                context: scriptCall.Roles
            )
        )
        {
            return false;
        }
        // check business rule
        if ((scriptCall.Rule != null) && (processData.Action.Mode != PanelActionMode.Always))
        {
            RuleEngine ruleEngine = RuleEngine.Create(
                contextStores: new Hashtable(),
                transactionId: null
            );
            XmlContainer rowXml = DatasetTools.GetRowXml(
                row: processData.SelectedRows[index: 0],
                version: DataRowVersion.Current
            );
            object result = ruleEngine.EvaluateRule(
                rule: scriptCall.Rule,
                data: rowXml,
                contextPosition: null
            );
            if (result is bool)
            {
                return (bool)result;
            }

            throw new ArgumentException(
                message: Origam.Workflow.ResourceUtils.GetString(
                    key: "ErrorResultNotBool",
                    args: scriptCall.Path
                )
            );
        }
        return true;
    }
}
