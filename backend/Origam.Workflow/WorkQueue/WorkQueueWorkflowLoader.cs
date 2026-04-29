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
using log4net;
using Origam.Extensions;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Workflow.WorkQueue;

/// <summary>
/// Connection string:
/// name - name of the workflowLoader defined at the WorkQueueClass.
/// The workflow called must implement IWorkQueueLoader workflow defined in the OrigamRoot model.
/// anything else - input workflow parameters - constant text values as defined in the connection string
/// </summary>
public class WorkQueueWorkflowLoader : WorkQueueLoaderAdapter
{
    private static readonly ILog log = LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType
    );

    private readonly Hashtable inputParameters = new();
    private IWorkflow workflow;
    private bool executed = false;
    private string resultState;
    private IDataDocument attachmentSource;
    private WorkQueueClass workQueueClass;
    private XPathNodeIterator resultIterator;

    public override void Connect(
        IWorkQueueService service,
        Guid queueId,
        string workQueueClass,
        string connection,
        string userName,
        string password,
        string transactionId
    )
    {
        if (log.IsInfoEnabled)
        {
            log.Info(message: "Connecting " + connection);
        }
        inputParameters.Add(key: "userName", value: userName);
        inputParameters.Add(key: "password", value: password);
        string name = null;
        string[] connectionStringParts = connection.Split(separator: ";".ToCharArray());
        foreach (string connectionStringPart in connectionStringParts)
        {
            string[] pair = connectionStringPart.Split(separator: "=".ToCharArray());
            if (pair.Length == 2)
            {
                switch (pair[0])
                {
                    case "name":
                    {
                        name = pair[1];
                        break;
                    }

                    default:
                    {
                        inputParameters[key: pair[0]] = pair[1];
                        break;
                    }
                }
            }
        }
        this.workQueueClass = service.WQClass(queueId: queueId) as WorkQueueClass;
        WorkqueueLoader loader = this.workQueueClass?.GetLoader(name: name);
        workflow = loader?.Workflow;
    }

    public override void Disconnect()
    {
        inputParameters.Clear();
        workflow = null;
    }

    public override WorkQueueAdapterResult GetItem(string lastState)
    {
        if (!executed)
        {
            if (LoadData(lastState: lastState))
            {
                executed = true;
            }
            else
            {
                // no data loaded, we exit
                return null;
            }
        }
        DataTable attachmentTable = attachmentSource.DataSet.Tables[name: "Attachment"];
        if ((resultIterator.Count > 1) && (attachmentTable.Rows.Count > 0))
        {
            throw new Exception(
                message: "Workflow returned multiple results and attachments. Returning attachments is not supported when returning multiple results by a workflow to the work queue."
            );
        }
        if (!resultIterator.MoveNext())
        {
            return null;
        }
        if (resultIterator.CurrentPosition > resultIterator.Count)
        {
            return null;
        }
        IXmlContainer document = DataDocumentFactory.New(
            xmlDoc: XmlTools.GetXmlSlice(iter: resultIterator)
        );
        var result = new WorkQueueAdapterResult(document: document)
        {
            State = resultState,
            Attachments = new WorkQueueAttachment[attachmentTable.Rows.Count],
        };
        if (log.IsDebugEnabled)
        {
            log.Debug(
                message: $"Workflow loader returned {attachmentTable.Rows.Count} attachments."
            );
            log.Debug(message: document.Xml.OuterXml);
        }
        for (int i = 0; i < attachmentTable.Rows.Count; i++)
        {
            result.Attachments[i] = new WorkQueueAttachment
            {
                Name = (string)attachmentTable.Rows[index: i][columnName: "FileName"],
                Data = (byte[])attachmentTable.Rows[index: i][columnName: "Data"],
            };
        }
        return result;
    }

    private bool LoadData(string lastState)
    {
        inputParameters[key: "lastState"] = lastState;
        WorkflowHost host = WorkflowHost.DefaultHost;
        WorkflowEngine workflowEngine = WorkflowEngine.PrepareWorkflow(
            workflow: workflow,
            parameters: inputParameters,
            isRepeatable: false,
            titleName: workflow.Name
        );
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Starting workflow " + workflow.Name);
        }
        host.ExecuteWorkflow(engine: workflowEngine);
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Finishing workflow " + workflow.Name);
        }
        if (workflowEngine.WorkflowException != null)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(
                    message: workflowEngine.WorkflowException.Message,
                    ex: workflowEngine.WorkflowException
                );
            }
            throw workflowEngine.WorkflowException;
        }
        var resultData = workflowEngine.ReturnValue as IXmlContainer;
        resultState = (string)
            workflowEngine.RuleEngine.GetContext(
                key: new ModelElementKey(id: new Guid(g: "f405cef2-2fad-4d58-a71c-10df3831e966"))
            );
        attachmentSource = (IDataDocument)
            workflowEngine.RuleEngine.GetContext(
                key: new ModelElementKey(id: new Guid(g: "b0caa6ec-8a54-4524-8387-8504e34d206c"))
            );
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Workflow loader result:");
            log.Debug(message: resultData?.Xml.OuterXml);
        }
        if (resultData == null)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(message: "Workflow loader result was null.");
            }
            throw new Exception(message: "Result of work queue loader must be an IXmlContainer.");
        }
        if (resultData.Xml.DocumentElement == null)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(message: "Workflow loader result was an empty XML Document.");
            }
            return false;
        }
        string xpath = "/";
        if (workQueueClass.Entity == null)
        {
            XmlNode firstChild = resultData.Xml.FirstChild;
            XmlNode secondChild = firstChild.FirstChild;
            xpath += firstChild.Name;
            if (secondChild != null)
            {
                xpath += "/" + secondChild.Name;
            }
            else
            {
                // one empty node from the loader workflow,
                // e.g. <ROOT/> - nothing came
                return false;
            }
        }
        else
        {
            xpath = "/ROOT/" + workQueueClass.Entity.Name;
        }
        XPathNavigator navigator = resultData.Xml.CreateNavigator();
        resultIterator = navigator?.Select(xpath: xpath);
        return true;
    }
}
