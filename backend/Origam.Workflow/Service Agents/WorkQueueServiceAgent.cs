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
using System.Data;
using Origam.DA;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Workflow;

/// <summary>
/// Summary description for WorkQueueServiceAgent.
/// </summary>
public class WorkQueueServiceAgent : AbstractServiceAgent
{
    public WorkQueueServiceAgent() { }

    private object _result;
    public override object Result
    {
        get
        {
            object temp = _result;
            _result = null;
            return temp;
        }
    }

    public override void Run()
    {
        IWorkQueueService wqs =
            ServiceManager.Services.GetService(serviceType: typeof(IWorkQueueService))
            as IWorkQueueService;
        switch (this.MethodName)
        {
            case "Add":
            {
                Add(wqs: wqs);
                break;
            }

            case "Remove":
            {
                Remove(wqs: wqs);
                break;
            }

            case "Get":
            {
                Get(wqs: wqs);
                break;
            }

            case "GetNextItem":
            {
                GetNextItem(wqs: wqs);
                break;
            }

            case "GenerateNotificationMessage":
            {
                GenerateNotificationMessage(wqs: wqs);
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "MethodName",
                    actualValue: MethodName,
                    message: ResourceUtils.GetString(key: "InvalidMethodName")
                );
            }
        }
    }

    private void GetNextItem(IWorkQueueService wqs)
    {
        string queueName = Parameters[key: "QueueName"] as string;
        if (queueName == null)
        {
            throw new InvalidCastException(message: "QueueName must be a string.");
        }
        DataRow row = wqs.GetNextItem(
            workQueueName: queueName,
            transactionId: TransactionId,
            processErrors: false
        );
        if (row != null)
        {
            _result = DataDocumentFactory.New(dataSet: row.Table.DataSet);
        }
    }

    private void GenerateNotificationMessage(IWorkQueueService wqs)
    {
        // check input parameters
        if (!(this.Parameters[key: "NotificationTemplateId"] is Guid))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorNotificationTemplateIdNotGuid")
            );
        }

        if (!(this.Parameters[key: "NotificationSource"] is IXmlContainer))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorNotificationSourceNotXmlDocument")
            );
        }

        WorkQueue.OrigamNotificationContactData recipientDataDS = null;
        if (this.Parameters.Contains(key: "RecipientData"))
        {
            if (!(this.Parameters[key: "RecipientData"] is IDataDocument))
            {
                throw new InvalidCastException(
                    message: ResourceUtils.GetString(key: "ErrorRecipientDataNotXmlDataDocument")
                );
            }

            recipientDataDS = new WorkQueue.OrigamNotificationContactData();
            DatasetTools.MergeDataSetVerbose(
                mergeInDS: recipientDataDS,
                mergeFromDS: (this.Parameters[key: "RecipientData"] as IDataDocument).DataSet
            );
        }
        _result = wqs.GenerateNotificationMessage(
            notificationTemplateId: (Guid)this.Parameters[key: "NotificationTemplateId"],
            notificationSource: this.Parameters[key: "NotificationSource"] as IXmlContainer,
            recipient: (recipientDataDS == null)
                ? null
                : recipientDataDS.OrigamNotificationContact[index: 0],
            workQueueRow: null,
            transactionId: this.TransactionId
        );
    }

    private void Get(IWorkQueueService wqs)
    {
        // Check input parameters
        if (this.Parameters[key: "MessageId"] == null)
        {
            throw new InvalidCastException(message: "MessageId must not be null.");
        }

        TraceLog(parameter: "MessageId");
        _result = wqs.WorkQueueGetMessage(
            workQueueMessageId: (Guid)this.Parameters[key: "MessageId"],
            transactionId: this.TransactionId
        );
    }

    private void Remove(IWorkQueueService wqs)
    {
        // Check input parameters
        if (this.Parameters[key: "MessageId"] == null)
        {
            throw new InvalidCastException(message: "MessageId must not be null.");
        }

        if (!(this.Parameters[key: "QueueId"] is Guid))
        {
            throw new InvalidCastException(message: "QueueId must be Guid.");
        }

        TraceLog(parameter: "MessageId");
        wqs.WorkQueueRemove(
            workQueueId: (Guid)this.Parameters[key: "QueueId"],
            rowKey: this.Parameters[key: "MessageId"],
            transactionId: this.TransactionId
        );
    }

    private void TraceLog(string parameter)
    {
        ITracingService trace =
            ServiceManager.Services.GetService(serviceType: typeof(ITracingService))
            as ITracingService;
        if (
            this.Trace
            && this.OutputMethod == ServiceOutputMethod.Ignore
            && ((Origam.Schema.ISchemaItem)this.OutputStructure)?.Path == "_any"
        )
        {
            trace.TraceStep(
                workflowInstanceId: this.TraceWorkflowId,
                stepPath: this.TraceStepName,
                stepId: this.TraceStepId,
                category: this.OutputMethod.ToString(),
                subCategory: "Input",
                remark: this.OutputStructure.Name,
                data1: Workflow.WorkflowEngine.ContextData(
                    context: this.Parameters[key: parameter]
                ),
                data2: "",
                message: null
            );
        }
    }

    private void Add(IWorkQueueService wqs)
    {
        // Check input parameters
        if (!(this.Parameters[key: "Data"] is IXmlContainer))
        {
            throw new InvalidCastException(
                message: ResourceUtils.GetString(key: "ErrorNotXmlDocument")
            );
        }

        if (!(this.Parameters[key: "QueueName"] is string))
        {
            throw new InvalidCastException(message: "QueueName must be a string.");
        }

        WorkQueueAttachment[] attachments = null;
        if (this.Parameters.ContainsKey(key: "Attachments"))
        {
            DataSet attachmentsDS = null;
            if (!(this.Parameters[key: "Attachments"] is IDataDocument))
            {
                throw new InvalidCastException(
                    message: ResourceUtils.GetString(
                        key: "ErrorParamNotXmlDataDocument",
                        args: "Attachments"
                    )
                );
            }
            attachmentsDS = (this.Parameters[key: "Attachments"] as IDataDocument).DataSet;
            attachments = new WorkQueueAttachment[
                attachmentsDS.Tables[name: "Attachment"].Rows.Count
            ];
            int i = 0;
            foreach (DataRow row in attachmentsDS.Tables[name: "Attachment"].Rows)
            {
                WorkQueueAttachment att = new WorkQueueAttachment();
                att.Name = (string)row[columnName: "FileName"];
                att.Data = (byte[])row[columnName: "Data"];
                attachments[i] = att;
                i++;
            }
        }
        TraceLog(parameter: "Data");
        wqs.WorkQueueAdd(
            workQueueName: this.Parameters[key: "QueueName"] as String,
            data: this.Parameters[key: "Data"] as IXmlContainer,
            attachments: attachments,
            transactionId: this.TransactionId
        );
        _result = null;
    }
}
