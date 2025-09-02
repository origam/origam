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
using System.Data;
using System.Reflection;
using Origam.DA;
using Origam.Extensions;

namespace Origam.Workbench.Services;

/// <summary>
/// Summary description for TracingService.
/// </summary>
public class TracingService : ITracingService
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        MethodBase.GetCurrentMethod().DeclaringType
    );

    private SchemaService _schema;
    IServiceAgent _dataServiceAgent;
    private bool? _enabled;

    public TracingService() { }

    #region Public Methods
    public void TraceWorkflow(Guid workflowInstanceId, Guid workflowId, string workflowName)
    {
        if (!Enabled)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Trace is disabled, workflow is not traced.");
            }
            return;
        }

        DataSet loadWorkflowData = LoadWorkflowInstanceData(workflowInstanceId);
        bool alreadyInitialized =
            loadWorkflowData.Tables.Count > 0 && loadWorkflowData.Tables[0].Rows.Count > 0;
        if (alreadyInitialized)
        {
            return;
        }

        UserProfile profile = SecurityManager.CurrentUserProfile();
        // create the record
        OrigamTraceWorkflowData data = new OrigamTraceWorkflowData();
        OrigamTraceWorkflowData.OrigamTraceWorkflowRow row =
            data.OrigamTraceWorkflow.NewOrigamTraceWorkflowRow();
        row.Id = workflowInstanceId;
        row.RecordCreated = DateTime.Now;
        row.RecordCreatedBy = profile.Id;
        row.WorkflowName = workflowName;
        row.WorkflowId = workflowId;
        data.OrigamTraceWorkflow.AddOrigamTraceWorkflowRow(row);
        StoreTraceData(dataSet: data, dataStructureQueryId: "309843cc-39ec-4eca-8848-8c69c885790c");
    }

    public void TraceStep(
        Guid workflowInstanceId,
        string stepPath,
        Guid stepId,
        string category,
        string subCategory,
        string remark,
        string data1,
        string data2,
        string message
    )
    {
        if (!Enabled)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Trace is disabled, step is not traced.");
            }
            return;
        }

        try
        {
            UserProfile profile = SecurityManager.CurrentUserProfile();
            // create the record
            OrigamTraceWorkflowStepData data = new OrigamTraceWorkflowStepData();
            OrigamTraceWorkflowStepData.OrigamTraceWorkflowStepRow row =
                data.OrigamTraceWorkflowStep.NewOrigamTraceWorkflowStepRow();
            row.Id = Guid.NewGuid();
            row.RecordCreated = DateTime.Now;
            row.RecordCreatedBy = profile.Id;

            row.Category = category;
            if (data1 != null)
            {
                row.Data1 = data1;
            }

            if (data2 != null)
            {
                row.Data2 = data2;
            }

            if (message != null)
            {
                row.Message = message;
            }

            row.refOrigamTraceWorkflowId = workflowInstanceId;
            if (remark != null)
            {
                row.Remark = remark;
            }

            row.Subcategory = subCategory;
            row.WorkflowStepId = stepId;
            row.WorkflowStepPath = stepPath;
            data.OrigamTraceWorkflowStep.AddOrigamTraceWorkflowStepRow(row);

            StoreTraceData(
                dataSet: data,
                dataStructureQueryId: "4985a6b2-8bae-4c21-9a09-0c2480c4efe2"
            );
        }
        catch (Exception ex)
        {
            log.LogOrigamError(ex);
        }
    }

    public void TraceRule(Guid ruleId, string ruleName, string ruleInput, string ruleResult)
    {
        TraceRule(ruleId, ruleName, ruleInput, ruleResult, Guid.Empty);
    }

    public void TraceRule(
        Guid ruleId,
        string ruleName,
        string ruleInput,
        string ruleResult,
        Guid workflowInstanceId
    )
    {
        if (!Enabled)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug("Trace is disabled, step is not traced.");
            }
            return;
        }

        try
        {
            UserProfile profile = SecurityManager.CurrentUserProfile();
            OrigamTraceRuleData data = new OrigamTraceRuleData();
            var row = data.OrigamTraceRule.NewOrigamTraceRuleRow();
            row.Id = Guid.NewGuid();
            row.RecordCreated = DateTime.Now;
            row.RecordCreatedBy = profile.Id;
            if (workflowInstanceId != Guid.Empty)
            {
                row.refOrigamTraceWorkflowId = workflowInstanceId;
            }
            if (ruleInput != null)
            {
                row.Input = ruleInput;
            }

            if (ruleResult != null)
            {
                row.Output = ruleResult;
            }

            row.RuleId = ruleId;
            row.RuleName = ruleName;

            data.OrigamTraceRule.AddOrigamTraceRuleRow(row);

            StoreTraceData(
                dataSet: data,
                dataStructureQueryId: "ca2f9609-e6b1-4425-a673-a46f69590eb3"
            );
        }
        catch (Exception ex)
        {
            log.LogOrigamError(ex);
        }
    }

    private void StoreTraceData(DataSet dataSet, string dataStructureQueryId)
    {
        DataStructureQuery query = new DataStructureQuery(new Guid(dataStructureQueryId));
        _dataServiceAgent.MethodName = "StoreDataByQuery";
        _dataServiceAgent.Parameters.Clear();
        _dataServiceAgent.Parameters.Add("Query", query);
        _dataServiceAgent.Parameters.Add("Data", dataSet);
        _dataServiceAgent.Run();
    }

    private DataSet LoadWorkflowInstanceData(Guid workflowInstanceId)
    {
        DataStructureQuery query = new DataStructureQuery(
            new Guid("309843cc-39ec-4eca-8848-8c69c885790c"),
            new Guid("4e6594b7-0462-4c1f-bc36-8fa37016995a")
        );
        query.Parameters.Add(new QueryParameter("OrigamTraceWorkflow_parId", workflowInstanceId));
        _dataServiceAgent.MethodName = "LoadDataByQuery";
        _dataServiceAgent.Parameters.Clear();
        _dataServiceAgent.Parameters.Add("Query", query);
        _dataServiceAgent.Run();
        return _dataServiceAgent.Result as DataSet;
    }

    #endregion
    #region IService Members
    public void UnloadService()
    {
        _dataServiceAgent = null;
        _schema = null;
    }

    public bool Enabled
    {
        get
        {
            if (_enabled.HasValue)
            {
                return _enabled.Value;
            }
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            return settings.TraceEnabled;
        }
        set => _enabled = value;
    }

    public void InitializeService()
    {
        _dataServiceAgent = (
            ServiceManager.Services.GetService(typeof(IBusinessServicesService))
            as IBusinessServicesService
        ).GetAgent("DataService", null, null);
        _schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
    }
    #endregion
}
