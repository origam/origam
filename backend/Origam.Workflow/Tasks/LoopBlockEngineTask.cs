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
using Origam.Schema;
using Origam.Schema.WorkflowModel;

namespace Origam.Workflow.Tasks
{
	/// <summary>
	/// Summary description for ForEachBlockEngineTask.
	/// </summary>
	public class LoopBlockEngineTask : BlockEngineTask
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
		WorkflowEngine _call;

		public LoopBlockEngineTask() : base()
		{
		}

		public override void Execute()
		{
			Exception exception = null;

			try
			{
				MeasuredExecution();
			}
			catch(Exception ex)
			{
				exception = ex;
				OnFinished(new WorkflowEngineTaskEventArgs(exception));
			}
		}

		protected override void OnExecute()
		{
			if(log.IsInfoEnabled)
			{
				log.Info("Loop Block started.");
			}

			this.Engine.Host.WorkflowFinished += new WorkflowHostEvent(Host_WorkflowFinished);
			this.Engine.Host.WorkflowMessage += new WorkflowHostMessageEvent(Host_WorkflowMessage);

			ResumeLoop();

		}

		private void ResumeLoop()
		{
			int i = 0;
			LoopWorkflowBlock block = this.Step as LoopWorkflowBlock;

			_call = this.Engine.GetSubEngine(block, Engine.TransactionBehavior);

			do
			{
				i++;
				// if workflow finished with an exception, we don't proceed
				if(this.Engine == null) return;

				if(log.IsInfoEnabled)
				{
					log.Info("Starting loop iteration no. " + i.ToString());
				}

				// Set workflow
				_call.ParentContexts.Clear();

				// Fill input context stores
				foreach(Key key in this.Engine.RuleEngine.ContextStoreKeys)
				{
					_call.ParentContexts.Add(key, this.Engine.RuleEngine.GetContext(key));
				}
				Engine.Host.ExecuteWorkflow(_call);
			} while ((bool)this.Engine.RuleEngine.EvaluateContext(block.LoopConditionXPath, block.LoopConditionContextStore, OrigamDataType.Boolean, null));

			// there is no other iteration, we finish
			if(this.Engine != null)	// only if we have not finished already e.g. with an exception
			{
				UnsubscribeEvents();
				OnFinished(new WorkflowEngineTaskEventArgs());
			}
		}

		private void Host_WorkflowFinished(object sender, WorkflowHostEventArgs e)
		{
			if(this.Engine == null) return;	// finished already

			LoopWorkflowBlock block = this.Step as LoopWorkflowBlock;

			if(e.Engine.WorkflowUniqueId.Equals(_call.WorkflowUniqueId))
			{
				if(e.Exception != null)
				{
					UnsubscribeEvents();
					OnFinished(new WorkflowEngineTaskEventArgs(e.Exception));
					return;
				}

				if(log.IsInfoEnabled)
				{
					log.Info("Finishing loop iteration.");
				}
			}
		}

		private void Host_WorkflowMessage(object sender, WorkflowHostMessageEventArgs e)
		{
			if(e.Engine.WorkflowUniqueId.Equals(_call.WorkflowUniqueId))
			{
				if(e.Exception != null)
				{
					UnsubscribeEvents();
					OnFinished(new WorkflowEngineTaskEventArgs(e.Exception));
				}
			}
		}

		private void UnsubscribeEvents()
		{
			if(this.Engine != null)
			{
				this.Engine.Host.WorkflowFinished -= new WorkflowHostEvent(Host_WorkflowFinished);
				this.Engine.Host.WorkflowMessage -= new WorkflowHostMessageEvent(Host_WorkflowMessage);
			}
		}
	}
}
