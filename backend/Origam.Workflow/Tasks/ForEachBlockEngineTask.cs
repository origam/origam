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
using System.Diagnostics;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Origam.Rule;
using Origam.Rule.Xslt;
using Origam.Schema;
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
        System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
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
        OrigamXsltContext ctx = OrigamXsltContext.Create(new NameTable(), Engine.TransactionId);
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
        for (int currentPosition = 1; currentPosition <= _call.IterationTotal; currentPosition++)
        {
            if (!block.IgnoreSourceContextChanges && this.sourceContextChanged)
            {
                // reinitialize _iter to updated context store and wind up
                // to current position
                IXmlContainer updatedSourceContextStore = GetSourceContextXmlContainer(block);
                XPathNavigator navigator = updatedSourceContextStore.Xml.CreateNavigator();
                OrigamXsltContext ctx = OrigamXsltContext.Create(
                    new NameTable(),
                    Engine.TransactionId
                );
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
            if (this.Engine == null)
                return;
            if (log.IsInfoEnabled)
            {
                log.Info("Starting iteration no. " + _iter.CurrentPosition);
            }
            // Set workflow
            _call.ParentContexts.Clear();
            _call.IterationNumber = _iter.CurrentPosition;
            // Fill input context stores
            foreach (Key key in this.Engine.RuleEngine.ContextStoreKeys)
            {
                object context = this.Engine.RuleEngine.GetContext(key);
                if (key.Equals(block.SourceContextStore.PrimaryKey))
                {
                    XmlDocument document = XmlTools.GetXmlSlice(_iter); // ((IHasXmlNode)iter.Current).GetNode();
                    IDataDocument dataDocument = context as IDataDocument;
                    IXmlContainer xmlDocument = context as IXmlContainer;
                    if (dataDocument != null)
                    {
                        // we clone the dataset (no data, just the structure)
                        DataSet dataset = dataDocument.DataSet.Clone();
                        // we load the iteration data into the dataset
                        dataset.ReadXml(new XmlNodeReader(document), XmlReadMode.IgnoreSchema);
                        // we add the context into the called engine
                        _call.ParentContexts.Add(key, DataDocumentFactory.New(dataset));
                    }
                    else if (xmlDocument != null)
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
        IXmlContainer xmlContainer =
            this.Engine.RuleEngine.GetContext(block.SourceContextStore) as IXmlContainer;
        if (xmlContainer == null)
        {
            throw new ArgumentOutOfRangeException(
                "SourceContextStore",
                block.SourceContextStore,
                ResourceUtils.GetString("ErrorSourceContextNotXmlDocument")
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
            OnFinished(new WorkflowEngineTaskEventArgs());
        }
    }

    private void Host_WorkflowFinished(object sender, WorkflowHostEventArgs e)
    {
        WorkflowEngine call = _call;
        ForeachWorkflowBlock block = this.Step as ForeachWorkflowBlock;
        try
        {
            if (Engine == null)
                return; // finished already

            if (e == null)
            {
                log.Error("ForEachBlockEngineTask.Host_WorkflowFinished received null event args.");
                return;
            }

            string eventWorkflowUniqueId =
                e.Engine == null ? "<null>" : e.Engine.WorkflowUniqueId.ToString();
            string callWorkflowUniqueId = call?.WorkflowUniqueId.ToString() ?? "<null>";
            string parentWorkflowUniqueId = Engine.WorkflowUniqueId.ToString() ?? "<null>";
            string logContext =
                ", EventWorkflowUniqueId="
                + eventWorkflowUniqueId
                + ", CallWorkflowUniqueId="
                + callWorkflowUniqueId
                + ", ParentWorkflowUniqueId="
                + parentWorkflowUniqueId
                + ", EventWorkflowUniqueIdIsEmpty="
                + (e.Engine?.WorkflowUniqueId == Guid.Empty)
                + ", CallWorkflowUniqueIdIsEmpty="
                + (call?.WorkflowUniqueId == Guid.Empty)
                + ", ExceptionPresent="
                + (e.Exception != null);

            if (call == null || e.Engine == null)
            {
                log.Error(
                    "ForEachBlockEngineTask.Host_WorkflowFinished encountered null reference candidate. "
                        + "CallIsNull="
                        + (call == null)
                        + ", EventEngineIsNull="
                        + (e.Engine == null)
                        + ", EventWorkflowUniqueIdIsEmpty="
                        + (e.Engine?.WorkflowUniqueId == Guid.Empty)
                        + ", CurrentStep="
                        + (this.Step?.GetType().FullName ?? "<null>")
                        + logContext
                );
                return;
            }

            if (block == null)
            {
                log.Error(
                    "ForEachBlockEngineTask.Host_WorkflowFinished Step is not ForeachWorkflowBlock. "
                        + "StepType="
                        + (this.Step?.GetType().FullName ?? "<null>")
                        + logContext
                );
                return;
            }

            if (block.SourceContextStore == null)
            {
                log.Error(
                    "ForEachBlockEngineTask.Host_WorkflowFinished block.SourceContextStore is null. "
                        + "BlockPath="
                        + block.Path
                        + logContext
                );
                return;
            }

            if (e.Engine.WorkflowUniqueId.Equals(call.WorkflowUniqueId))
            {
                if (e.Exception != null)
                {
                    UnsubscribeEvents();
                    OnFinished(new WorkflowEngineTaskEventArgs(e.Exception));
                    return;
                }
                if (!block.IgnoreSourceContextChanges)
                {
                    if (call.ParentContexts == null)
                    {
                        log.Error(
                            "ForEachBlockEngineTask.Host_WorkflowFinished _call.ParentContexts is null. "
                                + "CallWorkflowUniqueId="
                                + (call.WorkflowUniqueId.ToString() ?? "<null>")
                                + logContext
                        );
                        return;
                    }

                    if (call.RuleEngine == null)
                    {
                        log.Error(
                            "ForEachBlockEngineTask.Host_WorkflowFinished _call.RuleEngine is null. "
                                + "CallWorkflowUniqueId="
                                + (call.WorkflowUniqueId.ToString() ?? "<null>")
                                + logContext
                        );
                        return;
                    }

                    int parentContextsCountBefore =
                        call.ParentContexts is ICollection contextsCollection
                            ? contextsCollection.Count
                            : -1;
                    string parentContextsType = call.ParentContexts.GetType().FullName;

                    // Merge data back after success
                    try
                    {
                        foreach (DictionaryEntry entry in call.ParentContexts)
                        {
                            if (entry.Key == null)
                            {
                                log.Error(
                                    "ForEachBlockEngineTask.Host_WorkflowFinished _call.ParentContexts contains null key."
                                        + logContext
                                );
                                continue;
                            }

                            if (entry.Key.Equals(block.SourceContextStore.PrimaryKey))
                            {
                                if (block.SourceContextStore.PrimaryKey == null)
                                {
                                    log.Error(
                                        "ForEachBlockEngineTask.Host_WorkflowFinished SourceContextStore.PrimaryKey is null. "
                                            + "BlockPath="
                                            + block.Path
                                            + logContext
                                    );
                                    return;
                                }

                                Key entryKey = entry.Key as Key;
                                if (entryKey == null)
                                {
                                    log.Error(
                                        "ForEachBlockEngineTask.Host_WorkflowFinished entry.Key is not Key. "
                                            + "EntryKeyType="
                                            + entry.Key.GetType().FullName
                                            + logContext
                                    );
                                    continue;
                                }

                                object contextToMerge = call.RuleEngine.GetContext(entryKey);
                                if (contextToMerge == null)
                                {
                                    log.Error(
                                        "ForEachBlockEngineTask.Host_WorkflowFinished _call.RuleEngine.GetContext(entryKey) returned null. "
                                            + "EntryKey="
                                            + entryKey
                                            + logContext
                                    );
                                    continue;
                                }

                                string contextStoreName = Engine.ContextStoreName(entryKey);
                                if (contextStoreName == null)
                                {
                                    log.Error(
                                        "ForEachBlockEngineTask.Host_WorkflowFinished Engine.ContextStoreName(entryKey) returned null. "
                                            + "EntryKey="
                                            + entryKey
                                            + logContext
                                    );
                                    continue;
                                }

                                bool fullMerge = (!entry.Key.Equals(block.SourceContextStore.PrimaryKey));
                                sourceContextChanged = Engine.MergeContext(
                                    entryKey,
                                    contextToMerge,
                                    block,
                                    contextStoreName,
                                    (
                                        fullMerge
                                            ? ServiceOutputMethod.FullMerge
                                            : ServiceOutputMethod.AppendMergeExisting
                                    )
                                );
                                //					}
                            }
                        }
                    }
                    catch (InvalidOperationException ioex)
                    {
                        int parentContextsCountAfter =
                            call.ParentContexts is ICollection contextsCollectionAfter
                                ? contextsCollectionAfter.Count
                                : -1;
                        log.Error(
                            "ForEachBlockEngineTask.Host_WorkflowFinished failed while enumerating _call.ParentContexts. "
                                + "ParentContextsType="
                                + parentContextsType
                                + ", ParentContextsCountBefore="
                                + parentContextsCountBefore
                                + ", ParentContextsCountAfter="
                                + parentContextsCountAfter
                                + logContext,
                            ioex
                        );
                        throw;
                    }
                }
                if (log.IsInfoEnabled)
                {
                    log.Info("Finishing iteration no. " + _iter.CurrentPosition);
                }
            }
        }
        catch (Exception ex)
        {
            try
            {
                UnsubscribeEvents();
            }
            catch (Exception unsubscribeEx)
            {
                log.Error(
                    "ForEachBlockEngineTask.Host_WorkflowFinished failed to unsubscribe events in catch.",
                    unsubscribeEx
                );
            }
            log.Error(
                "ForEachBlockEngineTask.Host_WorkflowFinished failed. "
                    + BuildHostWorkflowFinishedStateDump(Engine, call, block, e),
                ex
            );
            throw;
        }
    }

    private string BuildHostWorkflowFinishedStateDump(
        WorkflowEngine engine,
        WorkflowEngine call,
        ForeachWorkflowBlock block,
        WorkflowHostEventArgs e
    )
    {
        string parentContextsCount = "<null>";
        if (call?.ParentContexts is ICollection parentContexts)
        {
            parentContextsCount = parentContexts.Count.ToString();
        }

        string iteratorState;
        try
        {
            iteratorState =
                _iter == null
                    ? "<null>"
                    : "CurrentPosition=" + _iter.CurrentPosition + ", Count=" + _iter.Count;
        }
        catch (Exception iteratorEx)
        {
            iteratorState = "<error:" + iteratorEx.GetType().FullName + ">";
        }

        return "EngineIsNull="
            + (engine == null)
            + ", EngineWorkflowUniqueId="
            + (engine?.WorkflowUniqueId.ToString() ?? "<null>")
            + ", CallIsNull="
            + (call == null)
            + ", CallWorkflowUniqueId="
            + (call?.WorkflowUniqueId.ToString() ?? "<null>")
            + ", EventArgsIsNull="
            + (e == null)
            + ", EventEngineIsNull="
            + (e?.Engine == null)
            + ", EventWorkflowUniqueId="
            + (e?.Engine == null ? "<null>" : e.Engine.WorkflowUniqueId.ToString())
            + ", EventWorkflowUniqueIdIsEmpty="
            + (e?.Engine?.WorkflowUniqueId == Guid.Empty)
            + ", EventExceptionIsNull="
            + (e?.Exception == null)
            + ", StepType="
            + (this.Step?.GetType().FullName ?? "<null>")
            + ", BlockIsNull="
            + (block == null)
            + ", BlockPath="
            + (block?.Path ?? "<null>")
            + ", SourceContextStoreIsNull="
            + (block?.SourceContextStore == null)
            + ", SourceContextPrimaryKey="
            + (block?.SourceContextStore?.PrimaryKey?.ToString() ?? "<null>")
            + ", CallRuleEngineIsNull="
            + (call?.RuleEngine == null)
            + ", CallParentContextsCount="
            + parentContextsCount
            + ", Iter="
            + iteratorState;
    }

    private void Host_WorkflowMessage(object sender, WorkflowHostMessageEventArgs e)
    {
        if (e.Engine.WorkflowUniqueId.Equals(_call.WorkflowUniqueId))
        {
            if (e.Exception != null)
            {
                UnsubscribeEvents();
                OnFinished(new WorkflowEngineTaskEventArgs(e.Exception));
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
