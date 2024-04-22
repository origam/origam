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
using System.Xml;

using Origam.DA;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Workflow;

/// <summary>
/// Summary description for WorkQueueServiceAgent.
/// </summary>
public class WorkQueueServiceAgent : AbstractServiceAgent
{
    public WorkQueueServiceAgent()
    {
		}

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
            IWorkQueueService wqs = ServiceManager.Services.GetService(typeof(IWorkQueueService)) as IWorkQueueService;
            switch (this.MethodName)
			{
                case "Add":
                    Add(wqs);
                    break;
                case "Remove":
                    Remove(wqs);
                    break;
                case "Get":
                    Get(wqs);
                    break;
                case "GetNextItem":
                    GetNextItem(wqs);
                    break;
                case "GenerateNotificationMessage":
                    GenerateNotificationMessage(wqs);
                    break;
                default:
					throw new ArgumentOutOfRangeException("MethodName", 
                        MethodName, ResourceUtils.GetString("InvalidMethodName"));
			}
		}

    private void GetNextItem(IWorkQueueService wqs)
    {
            string queueName = Parameters["QueueName"] as string;
            if (queueName == null)
            {
                throw new InvalidCastException("QueueName must be a string.");
            }
            DataRow row = wqs.GetNextItem(queueName, TransactionId, false);
            if (row != null)
            {
                _result = DataDocumentFactory.New(row.Table.DataSet);
            }
        }

    private void GenerateNotificationMessage(IWorkQueueService wqs)
    {
            // check input parameters
            if (!(this.Parameters["NotificationTemplateId"] is Guid))
                throw new InvalidCastException(ResourceUtils.GetString("ErrorNotificationTemplateIdNotGuid"));
            if (!(this.Parameters["NotificationSource"] is IXmlContainer))
                throw new InvalidCastException(ResourceUtils.GetString("ErrorNotificationSourceNotXmlDocument"));

            WorkQueue.OrigamNotificationContactData recipientDataDS = null;
            if (this.Parameters.Contains("RecipientData"))
            {
                if (!(this.Parameters["RecipientData"] is IDataDocument))
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorRecipientDataNotXmlDataDocument"));
                recipientDataDS = new WorkQueue.OrigamNotificationContactData();
                DatasetTools.MergeDataSetVerbose(recipientDataDS, (this.Parameters["RecipientData"] as IDataDocument).DataSet);
            }
            _result = wqs.GenerateNotificationMessage(
                (Guid)this.Parameters["NotificationTemplateId"]
                , this.Parameters["NotificationSource"] as IXmlContainer
                , (recipientDataDS == null) ? null : recipientDataDS.OrigamNotificationContact[0],
                null,
                this.TransactionId);
        }

    private void Get(IWorkQueueService wqs)
    {
            // Check input parameters
            if (this.Parameters["MessageId"] == null)
                throw new InvalidCastException("MessageId must not be null.");
            TraceLog("MessageId");
            _result = wqs.WorkQueueGetMessage((Guid)this.Parameters["MessageId"], this.TransactionId);
        }

    private void Remove(IWorkQueueService wqs)
    {
            // Check input parameters
            if (this.Parameters["MessageId"] == null)
                throw new InvalidCastException("MessageId must not be null.");
            if (!(this.Parameters["QueueId"] is Guid))
                throw new InvalidCastException("QueueId must be Guid.");
            TraceLog("MessageId");
            wqs.WorkQueueRemove((Guid)this.Parameters["QueueId"], this.Parameters["MessageId"], this.TransactionId);
        }

    private void TraceLog(string parameter)
    {
            ITracingService trace = ServiceManager.Services.GetService(typeof(ITracingService)) as ITracingService;
            if (this.Trace && this.OutputMethod == ServiceOutputMethod.Ignore && ((Origam.Schema.AbstractSchemaItem)this.OutputStructure)?.Path == "_any")
            {
                trace.TraceStep(this.TraceWorkflowId, this.TraceStepName, this.TraceStepId, this.OutputMethod.ToString(), "Input", this.OutputStructure.Name,
                    Workflow.WorkflowEngine.ContextData(this.Parameters[parameter]), "", null);
            }
        }
    private void Add(IWorkQueueService wqs)
    {
            // Check input parameters
            if (!(this.Parameters["Data"] is IXmlContainer))
                throw new InvalidCastException(ResourceUtils.GetString("ErrorNotXmlDocument"));

            if (!(this.Parameters["QueueName"] is string))
                throw new InvalidCastException("QueueName must be a string.");

            WorkQueueAttachment[] attachments = null;
            if (this.Parameters.ContainsKey("Attachments"))
            {
                DataSet attachmentsDS = null;
                if (!(this.Parameters["Attachments"] is IDataDocument))
                {
                    throw new InvalidCastException(ResourceUtils.GetString("ErrorParamNotXmlDataDocument", "Attachments"));
                }
                attachmentsDS = (this.Parameters["Attachments"] as IDataDocument).DataSet;
                attachments = new WorkQueueAttachment[attachmentsDS.Tables["Attachment"].Rows.Count];
                int i = 0;
                foreach (DataRow row in attachmentsDS.Tables["Attachment"].Rows)
                {
                    WorkQueueAttachment att = new WorkQueueAttachment();
                    att.Name = (string)row["FileName"];
                    att.Data = (byte[])row["Data"];
                    attachments[i] = att;
                    i++;
                }
            }
            TraceLog("Data");
            wqs.WorkQueueAdd(this.Parameters["QueueName"] as String, this.Parameters["Data"] as IXmlContainer, attachments, this.TransactionId);
            _result = null;
        }
}