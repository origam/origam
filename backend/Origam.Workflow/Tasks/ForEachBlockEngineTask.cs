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
using System.Xml;
using System.Xml.XPath;
using System.Collections;
using System.Data;
using System.Diagnostics;
using Origam.Rule;
using Origam.Rule.Xslt;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using System.Xml.Linq;
using System.Windows.Input;

namespace Origam.Workflow.Tasks;
/// <summary>
/// Summary description for ForEachBlockEngineTask.
/// </summary>
public class ForEachBlockEngineTask : BlockEngineTask
{
	private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	XPathNodeIterator _iter;
	WorkflowEngine _call;
	bool sourceContextChanged;
	public ForEachBlockEngineTask() : base()
	{
	}
	public override void Execute()
	{
		try
		{
			MeasuredExecution();
		}
		catch(Exception ex)
		{
			OnFinished(new WorkflowEngineTaskEventArgs(ex));
		}
	}
	protected override void MeasuredExecution()
	{
		base.MeasuredExecution();
		CleanUp();		
	}
	protected override void OnExecute()
	{
		if (log.IsInfoEnabled)
		{
			log.Info("ForEach Block started.");
		}
		this.Engine.Host.WorkflowFinished += Host_WorkflowFinished;
		ForeachWorkflowBlock block = this.Step as ForeachWorkflowBlock;
		IXmlContainer xmlContainer = GetSourceContextXmlContainer(block);
		XPathNavigator navigator = xmlContainer.Xml.CreateNavigator();
		OrigamXsltContext ctx = OrigamXsltContext.Create(
			new NameTable(), Engine.TransactionId);
		XPathExpression expr = navigator.Compile(block.IteratorXPath);
		expr.SetContext(ctx);
		// code might fail and this handler doesn't get cleared
		// and will interfer with other workflow invocations
		this.Engine.Host.WorkflowMessage += Host_WorkflowMessage;
		_iter = navigator.Select(expr);
		ResumeIteration();
	}
	private void ResumeIteration()
	{
		ForeachWorkflowBlock block = this.Step as ForeachWorkflowBlock;
		_call = this.Engine.GetSubEngine(block, Engine.TransactionBehavior);
		_call.IterationTotal = _iter.Count;
		for (int currentPosition = 1; currentPosition <= _call.IterationTotal;
			currentPosition++)
		{
			if (!block.IgnoreSourceContextChanges && this.sourceContextChanged)
            {
                // reinitialize _iter to updated context store and wind up
                // to current position                    
                IXmlContainer updatedSourceContextStore = GetSourceContextXmlContainer(block);
                XPathNavigator navigator = updatedSourceContextStore.Xml.CreateNavigator();
                OrigamXsltContext ctx = OrigamXsltContext.Create(
                    new NameTable(), Engine.TransactionId);
                XPathExpression expr = navigator.Compile(block.IteratorXPath);
                expr.SetContext(ctx);
                _iter = navigator.Select(expr);
                if (!WindUpTo(currentPosition))
				{
					break;
				}
            }
            else
			{
				bool moved = _iter.MoveNext();
				if (!moved || _iter.CurrentPosition > _iter.Count)
				{
					break;
				}
			}
			// if workflow finished with an exception, we don't proceed
			if(this.Engine == null) return;
			if(log.IsInfoEnabled)
			{
				log.Info("Starting iteration no. " + _iter.CurrentPosition);
			}
			// Set workflow
			_call.ParentContexts.Clear();
			_call.IterationNumber = _iter.CurrentPosition;
			// Fill input context stores
			foreach(Key key in this.Engine.RuleEngine.ContextStoreKeys)
			{
				object context = this.Engine.RuleEngine.GetContext(key);
				if(key.Equals(block.SourceContextStore.PrimaryKey))
				{
					XmlDocument document = XmlTools.GetXmlSlice(_iter); // ((IHasXmlNode)iter.Current).GetNode();
					IDataDocument dataDocument = context as IDataDocument;
					IXmlContainer xmlDocument = context as IXmlContainer;
					if(dataDocument != null)
					{
						// we clone the dataset (no data, just the structure)
						DataSet dataset = dataDocument.DataSet.Clone();
						// we load the iteration data into the dataset
						dataset.ReadXml(new XmlNodeReader(document), XmlReadMode.IgnoreSchema);
						// we add the context into the called engine
						_call.ParentContexts.Add(key, DataDocumentFactory.New(dataset));
					}
					else if(xmlDocument != null)
					{
						_call.ParentContexts.Add(key, new XmlContainer(document));
					}
				}
				else
				{
					// all other contexts
					// pass context directly
					_call.ParentContexts.Add(key, context);
				}
			}
			Engine.Host.ExecuteWorkflow(_call);
		}
	}
    private bool WindUpTo(int currentPosition)
    {
        for (int i = currentPosition; i > 0; i--)
        {
            bool moved = _iter.MoveNext();
            if (!moved || _iter.CurrentPosition > _iter.Count)
            {
				return false;
            }
        }
		return true;
    }
    private IXmlContainer GetSourceContextXmlContainer(ForeachWorkflowBlock block)
	{
		IXmlContainer xmlContainer = this.Engine.RuleEngine.GetContext(
						block.SourceContextStore) as IXmlContainer;
		if (xmlContainer == null)
		{
			throw new ArgumentOutOfRangeException(
				"SourceContextStore",
				block.SourceContextStore,
				ResourceUtils.GetString("ErrorSourceContextNotXmlDocument"));
		}
		return xmlContainer;
	}
	private void CleanUp()
	{
		// there is no other iteration, we finish
		if (this.Engine != null
		) // only if we have not finished already e.g. with an exception
		{
			UnsubscribeEvents();
			OnFinished(new WorkflowEngineTaskEventArgs());
		}
	}
	private void Host_WorkflowFinished(object sender, WorkflowHostEventArgs e)
	{
		if(this.Engine == null) return;	// finished already
		ForeachWorkflowBlock block = this.Step as ForeachWorkflowBlock;
		if(e.Engine.WorkflowUniqueId.Equals(_call.WorkflowUniqueId))
		{
			if(e.Exception != null)
			{
				UnsubscribeEvents();
				OnFinished(new WorkflowEngineTaskEventArgs(e.Exception));
				return;
			}
			if(!block.IgnoreSourceContextChanges)
			{
				// Merge data back after success
				foreach(DictionaryEntry entry in _call.ParentContexts)
				{
					if(entry.Key.Equals(block.SourceContextStore.PrimaryKey))
					{
						bool fullMerge = (! entry.Key.Equals(block.SourceContextStore.PrimaryKey));
						sourceContextChanged = Engine.MergeContext(
							(Key)entry.Key,
							_call.RuleEngine.GetContext(entry.Key as Key), 
							block, 
							this.Engine.ContextStoreName((Key)entry.Key), 
							(fullMerge ? ServiceOutputMethod.FullMerge : ServiceOutputMethod.AppendMergeExisting));
						//					}
					}
				}
			}
			if(log.IsInfoEnabled)
			{
				log.Info("Finishing iteration no. " + _iter.CurrentPosition);
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
