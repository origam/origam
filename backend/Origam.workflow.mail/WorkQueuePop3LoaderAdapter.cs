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
using MailKit.Net.Pop3;
using Origam.Mail;
using Origam.Workbench.Services;
using Origam.Workflow.WorkQueue;

namespace Origam.workflow.mail;

public class WorkQueuePop3LoaderAdapter : WorkQueueLoaderAdapter
{
    string _transactionId;
    Pop3Client _popClient;

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
        _transactionId = transactionId;
        string server = null;
        int port = 0;
        bool ssl = false;
        string[] cnParts = connection.Split(";".ToCharArray());
        foreach (string part in cnParts)
        {
            string[] pair = part.Split("=".ToCharArray());
            if (pair.Length == 2)
            {
                switch (pair[0].Trim())
                {
                    case "server":
                        server = pair[1];
                        break;
                    case "port":
                        port = int.Parse(pair[1]);
                        break;
                    case "ssl":
                        ssl = bool.Parse(pair[1]);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(
                            "connectionParameterName",
                            pair[0],
                            ResourceUtils.GetString("ErrorInvalidConnectionString")
                        );
                }
            }
        }
        if (server == null)
            throw new Exception(ResourceUtils.GetString("ErrorNoServer"));
        if (port == 0)
            throw new Exception(ResourceUtils.GetString("ErrorNoString"));
        _popClient = AbstractMailService.GetPopClient(
            mailServer: server,
            port: port,
            userName: userName,
            password: password,
            transactionId: transactionId,
            useSsl: ssl
        );
    }

    public override void Disconnect()
    {
        _popClient = null;
    }

    public override WorkQueueAdapterResult GetItem(string lastState)
    {
        MailData mailData = new MailData();
        mailData.DataSetName = "ROOT";
        int messageCount = _popClient.GetMessageUids().Count;
        if (messageCount == 0)
            return null;
        AbstractMailService.RetrieveMailNext(mailData, _popClient, true);
        WorkQueueAdapterResult result = new WorkQueueAdapterResult(
            DataDocumentFactory.New(mailData)
        );
        result.Attachments = new WorkQueueAttachment[mailData.MailAttachment.Rows.Count];
        result.State = mailData.Mail[0].MessageId;
        for (int i = 0; i < mailData.MailAttachment.Rows.Count; i++)
        {
            WorkQueueAttachment att = new WorkQueueAttachment();
            att.Data = mailData.MailAttachment[i].Data;
            att.Name = mailData.MailAttachment[i].FileName;
            result.Attachments[i] = att;
        }
        return result;
    }
}
