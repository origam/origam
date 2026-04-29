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
using Origam.DA;

namespace Origam.Workbench.Services.CoreServices;

/// <summary>
/// Summary description for WorkflowService.
/// </summary>
public class WorkflowService
{
    public WorkflowService() { }

    public static object ExecuteWorkflow(
        Guid workflowId,
        QueryParameterCollection parameters,
        string transactionId
    )
    {
        IServiceAgent workflowServiceAgent = (
            ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
            as IBusinessServicesService
        ).GetAgent(serviceType: "WorkflowService", ruleEngine: null, workflowEngine: null);
        Hashtable ht = new Hashtable(capacity: parameters.Count);
        foreach (QueryParameter param in parameters)
        {
            ht.Add(key: param.Name, value: param.Value);
        }
        workflowServiceAgent.MethodName = "ExecuteWorkflow";
        workflowServiceAgent.Parameters.Clear();
        workflowServiceAgent.Parameters.Add(key: "Workflow", value: workflowId);
        workflowServiceAgent.Parameters.Add(key: "Parameters", value: ht);
        workflowServiceAgent.TransactionId = transactionId;
        workflowServiceAgent.TraceWorkflowId = Guid.NewGuid();
        workflowServiceAgent.Run();
        return workflowServiceAgent.Result;
    }

    public static object ExecuteWorkflow(Guid workflowId)
    {
        return ExecuteWorkflow(
            workflowId: workflowId,
            parameters: new QueryParameterCollection(),
            transactionId: null
        );
    }
}
