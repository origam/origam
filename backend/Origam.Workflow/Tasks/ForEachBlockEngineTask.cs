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
using System.Xml;
using System.Xml.XPath;
using Origam.Rule.Xslt;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Workflow.Tasks;

/// <summary>
/// Summary description for ForEachBlockEngineTask.
/// </summary>
public class ForEachBlockEngineTask : BlockEngineTask
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    XPathNodeIterator _iter;
    WorkflowEngine _call;
    bool sourceContextChanged;

    public ForEachBlockEngineTask()
        : base() { }

    public override void Execute()
    {
        try
        {
            MeasuredExecution();
        }
        catch (Exception ex)
        {
            OnFinished(e: new WorkflowEngineTaskEventArgs(exception: ex));
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
            log.Info(message: "ForEach Block started.");
        }
        ForeachWorkflowBlock block = this.Step as ForeachWorkflowBlock;
        _call = this.Engine.GetSubEngine(
            block: block,
            transactionBehavior: Engine.TransactionBehavior
        );
        IXmlContainer xmlContainer = GetSourceContextXmlContainer(block: block);
        XPathNavigator navigator = xmlContainer.Xml.CreateNavigator();
        OrigamXsltContext ctx = OrigamXsltContext.Create(
            nameTable: new NameTable(),
            transactionId: Engine.TransactionId
        );
        XPathExpression expr = navigator.Compile(xpath: block.IteratorXPath);
        expr.SetContext(nsManager: ctx);
        // code might fail and this handler doesn't get cleared
        // and will interfer with other workflow invocations
        this.Engine.Host.WorkflowFinished += Host_WorkflowFinished;
        this.Engine.Host.WorkflowMessage += Host_WorkflowMessage;
        _iter = navigator.Select(expr: expr);
        ResumeIteration();
    }

    private void ResumeIteration()
    {
        ForeachWorkflowBlock block = this.Step as ForeachWorkflowBlock;
        _call.IterationTotal = _iter.Count;
        for (int currentPosition = 1; currentPosition <= _call.IterationTotal; currentPosition++)
        {
            if (!block.IgnoreSourceContextChanges && this.sourceContextChanged)
            {
                // reinitialize _iter to updated context store and wind up
                // to current position
                IXmlContainer updatedSourceContextStore = GetSourceContextXmlContainer(
                    block: block
                );
                XPathNavigator navigator = updatedSourceContextStore.Xml.CreateNavigator();
                OrigamXsltContext ctx = OrigamXsltContext.Create(
                    nameTable: new NameTable(),
                    transactionId: Engine.TransactionId
                );
                XPathExpression expr = navigator.Compile(xpath: block.IteratorXPath);
                expr.SetContext(nsManager: ctx);
                _iter = navigator.Select(expr: expr);
                if (!WindUpTo(currentPosition: currentPosition))
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
            if (this.Engine == null)
            {
                return;
            }

            if (log.IsInfoEnabled)
            {
                log.Info(message: "Starting iteration no. " + _iter.CurrentPosition);
            }
            // Set workflow
            _call.ParentContexts.Clear();
            _call.IterationNumber = _iter.CurrentPosition;
            // Fill input context stores
            foreach (Key key in this.Engine.RuleEngine.ContextStoreKeys)
            {
                object context = this.Engine.RuleEngine.GetContext(key: key);
                if (key.Equals(obj: block.SourceContextStore.PrimaryKey))
                {
                    XmlDocument document = XmlTools.GetXmlSlice(iter: _iter); // ((IHasXmlNode)iter.Current).GetNode();
                    IDataDocument dataDocument = context as IDataDocument;
                    IXmlContainer xmlDocument = context as IXmlContainer;
                    if (dataDocument != null)
                    {
                        // we clone the dataset (no data, just the structure)
                        DataSet dataset = dataDocument.DataSet.Clone();
                        // we load the iteration data into the dataset
                        dataset.ReadXml(
                            reader: new XmlNodeReader(node: document),
                            mode: XmlReadMode.IgnoreSchema
                        );
                        // we add the context into the called engine
                        _call.ParentContexts.Add(
                            key: key,
                            value: DataDocumentFactory.New(dataSet: dataset)
                        );
                    }
                    else if (xmlDocument != null)
                    {
                        _call.ParentContexts.Add(
                            key: key,
                            value: new XmlContainer(xmlDocument: document)
                        );
                    }
                }
                else
                {
                    // all other contexts
                    // pass context directly
                    _call.ParentContexts.Add(key: key, value: context);
                }
            }
            Engine.Host.ExecuteWorkflow(engine: _call);
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
        IXmlContainer xmlContainer =
            this.Engine.RuleEngine.GetContext(contextStore: block.SourceContextStore)
            as IXmlContainer;
        if (xmlContainer == null)
        {
            throw new ArgumentOutOfRangeException(
                paramName: "SourceContextStore",
                actualValue: block.SourceContextStore,
                message: ResourceUtils.GetString(key: "ErrorSourceContextNotXmlDocument")
            );
        }
        return xmlContainer;
    }

    private void CleanUp()
    {
        // there is no other iteration, we finish
        if (this.Engine != null) // only if we have not finished already e.g. with an exception
        {
            UnsubscribeEvents();
            OnFinished(e: new WorkflowEngineTaskEventArgs());
        }
    }

    private void Host_WorkflowFinished(object sender, WorkflowHostEventArgs e)
    {
        if (this.Engine == null)
        {
            return; // finished already
        }

        ForeachWorkflowBlock block = this.Step as ForeachWorkflowBlock;
        if (e.Engine.WorkflowUniqueId.Equals(g: _call.WorkflowUniqueId))
        {
            if (e.Exception != null)
            {
                UnsubscribeEvents();
                OnFinished(e: new WorkflowEngineTaskEventArgs(exception: e.Exception));
                return;
            }
            if (!block.IgnoreSourceContextChanges)
            {
                // Merge data back after success
                foreach (DictionaryEntry entry in _call.ParentContexts)
                {
                    if (entry.Key.Equals(obj: block.SourceContextStore.PrimaryKey))
                    {
                        bool fullMerge = (
                            !entry.Key.Equals(obj: block.SourceContextStore.PrimaryKey)
                        );
                        sourceContextChanged = Engine.MergeContext(
                            resultContextKey: (Key)entry.Key,
                            inputContext: _call.RuleEngine.GetContext(key: entry.Key as Key),
                            step: block,
                            contextName: this.Engine.ContextStoreName(key: (Key)entry.Key),
                            method: (
                                fullMerge
                                    ? ServiceOutputMethod.FullMerge
                                    : ServiceOutputMethod.AppendMergeExisting
                            )
                        );
                        //					}
                    }
                }
            }
            if (log.IsInfoEnabled)
            {
                log.Info(message: "Finishing iteration no. " + _iter.CurrentPosition);
            }
        }
    }

    private void Host_WorkflowMessage(object sender, WorkflowHostMessageEventArgs e)
    {
        if (e.Engine.WorkflowUniqueId.Equals(g: _call.WorkflowUniqueId))
        {
            if (e.Exception != null)
            {
                UnsubscribeEvents();
                OnFinished(e: new WorkflowEngineTaskEventArgs(exception: e.Exception));
            }
        }
    }

    private void UnsubscribeEvents()
    {
        if (this.Engine != null)
        {
            this.Engine.Host.WorkflowFinished -= new WorkflowHostEvent(Host_WorkflowFinished);
            this.Engine.Host.WorkflowMessage -= new WorkflowHostMessageEvent(Host_WorkflowMessage);
        }
    }
}
