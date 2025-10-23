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
using Origam.Schema.WorkflowModel;

namespace Origam.Workflow;

public class WorkflowTaskFactory
{
    public static IWorkflowEngineTask GetTask(IWorkflowStep step)
    {
        switch (step)
        {
            case CheckRuleStep _:
            {
                return new Tasks.CheckRuleEngineTask();
            }
            case ServiceMethodCallTask _:
            {
                return new Tasks.ServiceMethodCallEngineTask();
            }
            case WorkflowCallTask _:
            {
                return new Tasks.WorkflowCallEngineTask();
            }
            case ForeachWorkflowBlock _:
            {
                return new Tasks.ForEachBlockEngineTask();
            }
            case TransactionWorkflowBlock _:
            {
                return new Tasks.TransactionBlockEngineTask();
            }
            case UIFormTask _:
            {
                return new Tasks.UIEngineTask();
            }
            case SetWorkflowPropertyTask _:
            {
                return new Tasks.SetWorkflowPropertyEngineTask();
            }
            case UpdateContextTask _:
            {
                return new Tasks.UpdateContextEngineTask();
            }
            case AcceptContextStoreChangesTask _:
            {
                return new Tasks.AcceptContextStoreChangesEngineTask();
            }
            case LoopWorkflowBlock _:
            {
                return new Tasks.LoopBlockEngineTask();
            }
            case WaitTask _:
            {
                return new Tasks.WaitEngineTask();
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    "step",
                    step,
                    ResourceUtils.GetString("ErrorStepNotImplemented")
                );
            }
        }
    }
}
