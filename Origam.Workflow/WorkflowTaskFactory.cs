#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

namespace Origam.Workflow
{
	/// <summary>
	/// Summary description for WorkflowTaskFactory.
	/// </summary>
	public class WorkflowTaskFactory
	{
		public static IWorkflowEngineTask GetTask(IWorkflowStep step)
		{
			if(step is CheckRuleStep)
			{
				return new Tasks.CheckRuleEngineTask();
			}
			else if(step is ServiceMethodCallTask)
			{
				return new Tasks.ServiceMethodCallEngineTask();
			}
			else if(step is WorkflowCallTask)
			{
				return new Tasks.WorkflowCallEngineTask();
			}
			else if(step is ForeachWorkflowBlock)
			{
				return new Tasks.ForEachBlockEngineTask();
			}
			else if(step is TransactionWorkflowBlock)
			{
				return new Tasks.TransactionBlockEngineTask();
			}
			else if(step is UIFormTask)
			{
				return new Tasks.UIEngineTask();
			}
			else if(step is SetWorkflowPropertyTask)
			{
				return new Tasks.SetWorkflowPropertyEngineTask();
			}
			else if(step is UpdateContextTask)
			{
				return new Tasks.UpdateContextEngineTask();
			}
			else if(step is LoopWorkflowBlock)
			{
				return new Tasks.LoopBlockEngineTask();
			}
			else if(step is WaitTask)
			{
				return new Tasks.WaitEngineTask();
			}
			else
			{
				throw new ArgumentOutOfRangeException("step", step, ResourceUtils.GetString("ErrorStepNotImplemented"));
			}
		}
	}
}
