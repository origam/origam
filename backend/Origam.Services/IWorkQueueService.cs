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
using Origam.Schema;
using Origam.Service.Core;

namespace Origam.Workbench.Services;

public interface IWorkQueueService : IWorkbenchService
{
    DataSet UserQueueList();
    ISchemaItem WQClass(string name);
    ISchemaItem WQClass(Guid queueId);
    DataSet LoadWorkQueueData(string workQueueClass, object queueId);
    Guid WorkQueueAdd(
        string workQueueClass,
        string workQueueName,
        Guid workQueueId,
        string condition,
        IXmlContainer data,
        WorkQueueAttachment[] attachments,
        string transactionId
    );
    Guid WorkQueueAdd(
        string workQueueClass,
        string workQueueName,
        Guid workQueueId,
        string condition,
        IXmlContainer data,
        string transactionId
    );
    Guid WorkQueueAdd(string workQueueName, IXmlContainer data, string transactionId);
    Guid WorkQueueAdd(
        string workQueueName,
        IXmlContainer data,
        WorkQueueAttachment[] attachments,
        string transactionId
    );
    DataRow GetNextItem(string workQueueName, string transactionId, bool processErrors);
    void WorkQueueRemove(
        string workQueueClass,
        string workQueueName,
        Guid workQueueId,
        string condition,
        object rowKey,
        string transactionId
    );
    void WorkQueueRemove(Guid workQueueId, object rowKey, string transactionId);
    IDataDocument WorkQueueGetMessage(Guid workQueueMessageId, string transactionId);
    void WorkQueueUpdate(
        string workQueueClass,
        int relationNo,
        Guid workQueueId,
        object rowKey,
        string transactionId
    );
    void HandleAction(
        Guid queueId,
        string queueClass,
        DataTable selectedRows,
        Guid commandType,
        string command,
        string param1,
        string param2,
        object errorQueueId
    );
    void HandleAction(string workQueueCode, string commandText, Guid queueEntryId);
    IDataDocument GenerateNotificationMessage(
        Guid notificationTemplateId,
        IXmlContainer notificationSource,
        DataRow recipient,
        DataRow workQueueRow,
        string transactionId
    );
    string CustomScreenName(Guid queueId);
}
