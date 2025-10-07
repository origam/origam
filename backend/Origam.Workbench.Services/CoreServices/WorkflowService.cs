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
            ServiceManager.Services.GetService(typeof(IBusinessServicesService))
            as IBusinessServicesService
        ).GetAgent("WorkflowService", null, null);
        Hashtable ht = new Hashtable(parameters.Count);
        foreach (QueryParameter param in parameters)
        {
            ht.Add(param.Name, param.Value);
        }
        workflowServiceAgent.MethodName = "ExecuteWorkflow";
        workflowServiceAgent.Parameters.Clear();
        workflowServiceAgent.Parameters.Add("Workflow", workflowId);
        workflowServiceAgent.Parameters.Add("Parameters", ht);
        workflowServiceAgent.TransactionId = transactionId;
        workflowServiceAgent.TraceWorkflowId = Guid.NewGuid();
        workflowServiceAgent.Run();
        return workflowServiceAgent.Result;
    }

    public static object ExecuteWorkflow(Guid workflowId)
    {
        return ExecuteWorkflow(workflowId, new QueryParameterCollection(), null);
    }
}
