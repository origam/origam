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

using Origam.Schema.WorkflowModel;

namespace Origam.Workflow.Tasks;

/// <summary>
/// Summary description for TransactionBlockEngineTask.
/// </summary>
public class TransactionBlockEngineTask : BlockEngineTask
{
	private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	WorkflowEngine _call;

	public TransactionBlockEngineTask() : base()
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
			this.Engine.Host.WorkflowFinished += new WorkflowHostEvent(Host_WorkflowFinished);
			this.Engine.Host.WorkflowMessage += new WorkflowHostMessageEvent(Host_WorkflowMessage);

			TransactionWorkflowBlock block = this.Step as TransactionWorkflowBlock;

			_call = this.Engine.GetSubEngine(block, Engine.TransactionBehavior);

			// if the transaction is atomic, we start it, except if the transaction exists already
			if(block.TransactionType == TransactionTypes.Atomic & this.Engine.TransactionId == null)
			{
				_call.SetTransactionId(Guid.NewGuid().ToString(), WorkflowTransactionBehavior.InheritExisting);
				if(log.IsInfoEnabled)
				{
					log.Info("Starting new atomic transaction: " + _call.TransactionId);
				}
			}

			// Fill input context stores
			foreach(Key key in this.Engine.RuleEngine.ContextStoreKeys)
			{
				// pass directly the context, no cloning
				_call.ParentContexts.Add(key, this.Engine.RuleEngine.GetContext(key));
			}

			// Run
			Engine.ExecuteSubEngineWorkflow(_call);
		}

	private void Host_WorkflowFinished(object sender, WorkflowHostEventArgs e)
	{
			if(e.Engine.WorkflowUniqueId.Equals(_call.WorkflowUniqueId))
			{
				UnsubscribeEvents();

				if(e.Exception != null)
				{
					HandleException(e.Exception);
					return;
				}

				// commit the transaction, if we initiated the transaction
				if(_call.TransactionId != null & this.Engine.TransactionId == null)
				{
					try
					{
						if(log.IsInfoEnabled)
						{
							log.Info("Committing atomic transaction: " + _call.TransactionId);
						}
						ResourceMonitor.Commit(_call.TransactionId);
					}
					catch(Exception ex)
					{
						OnFinished(new WorkflowEngineTaskEventArgs(new Exception(ResourceUtils.GetString("ErrorWhenCommit", Environment.NewLine + ex.Message), ex)));
						return;
					}
				}

				// Copy simple data back after success (complex are references, so they
				// are left intact anyway
				foreach(DictionaryEntry entry in _call.ParentContexts)
				{
					this.Engine.RuleEngine.SetContext(entry.Key as Key, _call.RuleEngine.GetContext(entry.Key as Key));
				}

				OnFinished(new WorkflowEngineTaskEventArgs(e.Exception));
			}
		}

	private void Host_WorkflowMessage(object sender, WorkflowHostMessageEventArgs e)
	{
			if(e.Engine.WorkflowUniqueId.Equals(_call.WorkflowUniqueId))
			{
				if(e.Exception != null)
				{
					UnsubscribeEvents();
					HandleException(e.Exception);
				}
			}
		}

	private void HandleException(Exception ex)
	{
			// rollback the transaction, if we initiated the transaction
			if(_call.TransactionId != null & this.Engine.TransactionId == null)
			{
				if(log.IsInfoEnabled)
				{
					log.Info("Rolling back atomic transaction: " + _call.TransactionId);
				}

				try
				{
					ResourceMonitor.Rollback(_call.TransactionId);
				}
				catch(Exception rollbackEx)
				{
					ex = new Exception(ResourceUtils.GetString("ErrorRollBackFailed"), rollbackEx);
				}
			}

			OnFinished(new WorkflowEngineTaskEventArgs(ex));
		}

	private void UnsubscribeEvents()
	{
			this.Engine.Host.WorkflowFinished -= new WorkflowHostEvent(Host_WorkflowFinished);
			this.Engine.Host.WorkflowMessage -= new WorkflowHostMessageEvent(Host_WorkflowMessage);
		}
}