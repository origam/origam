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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Transactions;
using System.Xml;
using System.Xml.XPath;
using Microsoft.Data.SqlClient;
using Origam.DA;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Rule;
using Origam.Rule.Xslt;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;
using Timer = System.Timers.Timer;

namespace Origam.Workflow.WorkQueue;

public class WorkQueueService : IWorkQueueService, IBackgroundService
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType
    );
    private const string WQ_EVENT_ONCREATE = "fe40902f-8a44-477e-96f9-d157eee16a0f";
    private readonly core.ICoreDataService dataService = core.DataService.Instance;
    private CancellationTokenSource cancellationTokenSource = new();
    private readonly WorkQueueUtils workQueueUtils;
    private readonly IWorkQueueProcessor queueProcessor;
    private readonly WorkQueueThrottle workQueueThrottle;
    private readonly Timer loadExternalWorkQueuesTimer = new(interval: 60_000);
    private readonly Timer queueAutoProcessTimer;
    private bool serviceBeingUnloaded = false;
    private bool externalQueueAdapterBusy = false;
    private bool queueAutoProcessBusy = false;
    private readonly RetryManager retryManager = new();
    private static readonly Guid DS_METHOD_WQ_GETACTIVEQUEUES = new(
        g: "0b45c721-65d2-4305-b34a-cd0d07387ea1"
    );
    private static readonly Guid DS_METHOD_WQ_GETACTIVEQUEUESBYPROCESSOR = new(
        g: "b1f1abcd-c8bc-4680-8f21-06a68e8305f0"
    );
    private static readonly Guid DS_WORKQUEUE = new(g: "7b44a488-ac98-4fe1-a427-55de0ff9e12e");
    private static readonly Guid DS_SORTSET_WQ_SORT = new(
        g: "c1ec9d9e-09a2-47ad-b5e4-b57107c4dc34"
    );

    public WorkQueueService()
        : this(queueProcessIntervalMillis: 10_000) { }

    public WorkQueueService(int queueProcessIntervalMillis)
    {
        queueAutoProcessTimer = new Timer(interval: queueProcessIntervalMillis);
        var schemaService = ServiceManager.Services.GetService<SchemaService>();
        var dataLookupService = ServiceManager.Services.GetService<IDataLookupService>();
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        workQueueThrottle = new WorkQueueThrottle(persistenceService: persistenceService);
        workQueueUtils = new WorkQueueUtils(
            lookupService: dataLookupService,
            schemaService: schemaService
        );
        schemaService.SchemaLoaded += schemaService_SchemaLoaded;
        schemaService.SchemaUnloaded += schemaService_SchemaUnloaded;
        schemaService.SchemaUnloading += schemaService_SchemaUnloading;

        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        queueProcessor = settings.WorkQueueProcessingMode switch
        {
            WorkQueueProcessingMode.Linear => new LinearProcessor(
                itemProcessAction: ProcessQueueItem,
                workQueueUtils: workQueueUtils,
                retryManager: retryManager,
                workQueueThrottle: workQueueThrottle
            ),
            WorkQueueProcessingMode.RoundRobin => new RoundRobinLinearProcessor(
                itemProcessAction: ProcessQueueItem,
                workQueueUtils: workQueueUtils,
                retryManager: retryManager,
                workQueueThrottle: workQueueThrottle,
                batchSize: settings.RoundRobinBatchSize
            ),
            _ => throw new NotImplementedException(
                message: $"Option {settings.WorkQueueProcessingMode} not implemented"
            ),
        };
    }

    #region IWorkbenchService Members
    public void UnloadService()
    {
        var schemaService = ServiceManager.Services.GetService<SchemaService>();
        schemaService.SchemaLoaded -= schemaService_SchemaLoaded;
        schemaService.SchemaUnloaded -= schemaService_SchemaUnloaded;
        schemaService.SchemaUnloading -= schemaService_SchemaUnloading;
        StopTasks();
    }

    public void StopTasks()
    {
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(format: "Stopping WorkQueueService Timers");
        }
        serviceBeingUnloaded = true;
        cancellationTokenSource.Cancel();
        // unsubscribe from 'Elapsed' events
        loadExternalWorkQueuesTimer.Elapsed -= LoadExternalWorkQueuesElapsed;
        queueAutoProcessTimer.Elapsed -= WorkQueueAutoProcessTimerElapsed;
        // stop timers
        queueAutoProcessTimer.Stop();
        loadExternalWorkQueuesTimer.Stop();
        while (queueAutoProcessBusy || externalQueueAdapterBusy)
        {
            if (log.IsInfoEnabled)
            {
                log.Info(message: "Unloading service - waiting for queues to finish.");
            }
            Thread.Sleep(millisecondsTimeout: 1000);
        }
        serviceBeingUnloaded = false;
        cancellationTokenSource = new CancellationTokenSource();
    }

    public void InitializeService()
    {
        serviceBeingUnloaded = false;
    }
    #endregion
    #region IWorkQueueService Members
    public DataSet UserQueueList()
    {
        // Load all active work queues
        DataSet result = dataService.LoadData(
            dataStructureId: new Guid(g: "3a23f4e1-368c-4163-a790-4eed173af83d"),
            methodId: new Guid(g: "ed3d93ca-bd4e-4830-8d26-f7120c8fc7ff"),
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null
        );
        // filter out those current user has no access to
        var rowsToDelete = new List<DataRow>();
        foreach (DataRow row in result.Tables[name: "WorkQueue"].Rows)
        {
            IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
            if (
                row.IsNull(columnName: "Roles")
                || !auth.Authorize(
                    principal: SecurityManager.CurrentPrincipal,
                    context: (string)row[columnName: "Roles"]
                )
            )
            {
                rowsToDelete.Add(item: row);
            }
        }
        foreach (DataRow row in rowsToDelete)
        {
            row.Delete();
        }
        result.AcceptChanges();
        return result;
    }

    public ISchemaItem WQClass(string name)
    {
        return workQueueUtils.WorkQueueClass(name: name);
    }

    public ISchemaItem WQClass(Guid queueId)
    {
        return workQueueUtils.WorkQueueClass(queueId: queueId);
    }

    public DataSet LoadWorkQueueData(string workQueueClass, object queueId)
    {
        return workQueueUtils.LoadWorkQueueData(
            workQueueClass: workQueueClass,
            queueId: queueId,
            pageSize: 0,
            pageNumber: 0,
            transactionId: null
        );
    }

    public Guid WorkQueueAdd(string workQueueName, IXmlContainer data, string transactionId)
    {
        Guid workQueueId = workQueueUtils.GetQueueId(referenceCode: workQueueName);
        string workQueueClass = workQueueUtils.WorkQueueClassName(queueId: workQueueId);
        string condition = "";
        return WorkQueueAdd(
            workQueueClassIdentifier: workQueueClass,
            workQueueName: workQueueName,
            workQueueId: workQueueId,
            condition: condition,
            data: data,
            attachments: null,
            transactionId: transactionId
        );
    }

    public Guid WorkQueueAdd(
        string workQueueName,
        IXmlContainer data,
        WorkQueueAttachment[] attachments,
        string transactionId
    )
    {
        Guid workQueueId = workQueueUtils.GetQueueId(referenceCode: workQueueName);
        string workQueueClass = workQueueUtils.WorkQueueClassName(queueId: workQueueId);
        string condition = "";
        return WorkQueueAdd(
            workQueueClassIdentifier: workQueueClass,
            workQueueName: workQueueName,
            workQueueId: workQueueId,
            condition: condition,
            data: data,
            attachments: attachments,
            transactionId: transactionId
        );
    }

    public Guid WorkQueueAdd(
        string workQueueClassIdentifier,
        string workQueueName,
        Guid workQueueId,
        string condition,
        IXmlContainer data,
        string transactionId
    )
    {
        return WorkQueueAdd(
            workQueueClassIdentifier: workQueueClassIdentifier,
            workQueueName: workQueueName,
            workQueueId: workQueueId,
            condition: condition,
            data: data,
            attachments: null,
            transactionId: transactionId
        );
    }

    public Guid WorkQueueAdd(
        string workQueueClassIdentifier,
        string workQueueName,
        Guid workQueueId,
        string condition,
        IXmlContainer data,
        WorkQueueAttachment[] attachments,
        string transactionId
    )
    {
        if (log.IsDebugEnabled)
        {
            log.Debug(message: $"Adding Work Queue Entry for Queue: {workQueueName}");
        }
        RuleEngine ruleEngine = RuleEngine.Create(
            contextStores: new Hashtable(),
            transactionId: transactionId
        );
        UserProfile profile = SecurityManager.CurrentUserProfile();
        WorkQueueClass workQueueClass = workQueueUtils.WorkQueueClass(
            name: workQueueClassIdentifier
        );
        if (workQueueClass != null)
        {
            Guid rowId = Guid.NewGuid();
            DataSet dataSet = new DatasetGenerator(userDefinedParameters: true).CreateDataSet(
                ds: workQueueClass.WorkQueueStructure
            );
            DataTable table = dataSet.Tables[index: 0];
            DataRow row = table.NewRow();
            row[columnName: "Id"] = rowId;
            row[columnName: "refWorkQueueId"] = workQueueId;
            row[columnName: "RecordCreated"] = DateTime.Now;
            row[columnName: "RecordCreatedBy"] = profile.Id;
            WorkQueueRowFill(
                workQueueClass: workQueueClass,
                ruleEngine: ruleEngine,
                row: row,
                data: data
            );
            table.Rows.Add(row: row);
            if (!string.IsNullOrEmpty(value: condition))
            {
                if (
                    !EvaluateWorkQueueCondition(
                        queueRow: row,
                        condition: condition,
                        queueName: workQueueName,
                        transactionId: transactionId
                    )
                )
                {
                    return Guid.Empty;
                }
            }
            StoreQueueItems(
                workQueueClass: workQueueClass,
                selectedRows: table,
                transactionId: transactionId
            );
            // add attachments
            if (attachments != null)
            {
                var attachmentService = ServiceManager.Services.GetService<AttachmentService>();
                foreach (WorkQueueAttachment workQueueAttachment in attachments)
                {
                    attachmentService.AddAttachment(
                        fileName: workQueueAttachment.Name,
                        attachment: workQueueAttachment.Data,
                        recordId: rowId,
                        profileId: profile.Id,
                        transactionId: transactionId
                    );
                }
            }
            // notifications - OnCreate
            ProcessNotifications(
                workQueueClass: workQueueClass,
                workQueueId: workQueueId,
                eventTypeId: new Guid(g: WQ_EVENT_ONCREATE),
                queueItem: dataSet,
                transactionId: transactionId
            );
            return (Guid)row[columnName: "Id"];
        }
        return Guid.Empty;
    }

    public IDataDocument WorkQueueGetMessage(Guid workQueueMessageId, string transactionId)
    {
        WorkQueueClass workQueueClass = workQueueUtils.WorkQueueClassByMessageId(
            queueMessageId: workQueueMessageId
        );
        DataSet dataSet = FetchSingleQueueEntry(
            workQueueClass: workQueueClass,
            queueEntryId: workQueueMessageId,
            transactionId: transactionId
        );
        return DataDocumentFactory.New(dataSet: dataSet);
    }

    private void ProcessNotifications(
        WorkQueueClass workQueueClass,
        Guid workQueueId,
        Guid eventTypeId,
        DataSet queueItem,
        string transactionId
    )
    {
        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        WorkQueueData workQueueData = GetQueue(queueId: workQueueId);
        foreach (
            WorkQueueData.WorkQueueNotificationRow notification in workQueueData
                .WorkQueueNotification
                .Rows
        )
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(message: $"Testing notification {notification?.Description}");
            }
            // check if the event type is equal (OnCreate, OnEscalate, etc...)
            if (!notification.refWorkQueueNotificationEventId.Equals(g: eventTypeId))
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(message: "Wrong event type. Notification will not be sent.");
                }
                continue;
            }
            // notification source
            IXmlContainer notificationSource;
            if (workQueueClass.NotificationStructure == null)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(message: "Notification source is work queue item.");
                }
                notificationSource = DataDocumentFactory.New(dataSet: queueItem);
            }
            else
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(
                        message: $"Notification source is {workQueueClass.NotificationStructure.Path}"
                    );
                }
                DataSet dataSet = dataService.LoadData(
                    dataStructureId: workQueueClass.NotificationStructureId,
                    methodId: workQueueClass.NotificationLoadMethodId,
                    defaultSetId: Guid.Empty,
                    sortSetId: Guid.Empty,
                    transactionId: transactionId,
                    paramName1: workQueueClass.NotificationFilterPkParameter,
                    paramValue1: queueItem.Tables[index: 0].Rows[index: 0][columnName: "refId"]
                );
                notificationSource = DataDocumentFactory.New(dataSet: dataSet);
                if (log.IsDebugEnabled)
                {
                    log.Debug(
                        message: $"Notification source result: {notificationSource?.Xml?.OuterXml}"
                    );
                }
            }
            DataRow workQueueRow = ExtractWorkQueueRowIfNotNotificationDataStructureIsSet(
                workQueueClass: workQueueClass,
                queueItem: queueItem
            );
            // senders
            var senders = new Hashtable();
            // evaluate senders - get one for each channel (e-mail, sms...) and then assign them by each recipient's notification channel
            foreach (
                WorkQueueData.WorkQueueNotificationContact_SendersRow sender in notification.GetWorkQueueNotificationContact_SendersRows()
            )
            {
                OrigamNotificationContactData senderData = GetNotificationContacts(
                    workQueueNotificationContactTypeId: sender.refWorkQueueNotificationContactTypeId,
                    origamNotificationChannelTypeId: sender.refOrigamNotificationChannelTypeId,
                    value: sender.Value,
                    context: notificationSource,
                    workQueueRow: workQueueRow,
                    transactionId: transactionId
                );
                if (
                    senderData.OrigamNotificationContact.Count == 0
                    || senderData.OrigamNotificationContact[index: 0].ContactIdentification == ""
                )
                {
                    if (log.IsErrorEnabled)
                    {
                        log.Error(
                            message: $"Skipping notification for work queue notification sender definition {sender?.Id}, no sender returned"
                        );
                    }
                    continue;
                }
                senders[key: sender.refOrigamNotificationChannelTypeId] = senderData
                    .OrigamNotificationContact[index: 0]
                    .ContactIdentification;
            }
            // recipients
            WorkQueueData.WorkQueueNotificationContact_RecipientsRow[] recipientRows =
                notification.GetWorkQueueNotificationContact_RecipientsRows();
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    message: $"Number of recipients rows defined for work queue notification: {recipientRows.Length}"
                );
            }
            // evaluate recipient rows and send notifications. For each row there can be found out more than one recipient.
            foreach (
                WorkQueueData.WorkQueueNotificationContact_RecipientsRow recipientRow in recipientRows
            )
            {
                string value = (recipientRow.IsValueNull() ? null : recipientRow.Value);
                OrigamNotificationContactData recipients = GetNotificationContacts(
                    workQueueNotificationContactTypeId: recipientRow.refWorkQueueNotificationContactTypeId,
                    origamNotificationChannelTypeId: recipientRow.refOrigamNotificationChannelTypeId,
                    value: value,
                    context: notificationSource,
                    workQueueRow: workQueueRow,
                    transactionId: transactionId
                );
                if (recipients == null || recipients.OrigamNotificationContact.Count == 0)
                {
                    // continue to process next recipient definition row
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                            message: "Didn't get any response when trying to get recipients."
                        );
                    }
                    continue;
                }
                if (log.IsDebugEnabled)
                {
                    log.Debug(message: $"Recipients: {recipients?.GetXml()}");
                }
                if (!senders.Contains(key: recipientRow.refOrigamNotificationChannelTypeId))
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                            message: $"Can't find any sender for notification channel '{recipientRow.refOrigamNotificationChannelTypeId}'"
                        );
                    }
                    // continue to process the next recipient definition row
                    continue;
                }
                // for each recipient generate and send the mail
                foreach (
                    OrigamNotificationContactData.OrigamNotificationContactRow recipient in recipients
                        .OrigamNotificationContact
                        .Rows
                )
                {
                    // generate data for mail
                    IDataDocument notificationData = GenerateNotificationMessage(
                        notificationTemplateId: notification.refOrigamNotificationTemplateId,
                        notificationSource: notificationSource,
                        recipientRow: recipient,
                        workQueueRow: workQueueRow,
                        transactionId: transactionId
                    );
                    // processing data for mail (output from transformation)
                    if (notificationData.DataSet.Tables[index: 0].Rows.Count != 1)
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(
                                message: $"Notification transformation result count: {notificationData.DataSet.Tables[index: 0].Rows.Count}"
                            );
                        }
                        continue;
                    }
                    DataRow notificationRow = notificationData.DataSet.Tables[index: 0].Rows[
                        index: 0
                    ];
                    string notificationBody = null;
                    string notificationSubject = null;
                    if (!notificationRow.IsNull(columnName: "Body"))
                    {
                        notificationBody = (string)notificationRow[columnName: "Body"];
                    }
                    if (!notificationRow.IsNull(columnName: "Subject"))
                    {
                        notificationSubject = (string)notificationRow[columnName: "Subject"];
                    }
                    if (
                        string.IsNullOrEmpty(value: notificationBody)
                        || string.IsNullOrEmpty(value: notificationSubject)
                    )
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(
                                message: "Notification body or subject is empty. No notification will be sent."
                            );
                        }
                        continue;
                    }
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(message: $"Notification subject: {notificationSubject}");
                        log.Debug(message: $"Notification body: '{notificationBody}'");
                    }
                    // send the notification - start the notification workflow
                    var queryParameterCollection = new QueryParameterCollection
                    {
                        new QueryParameter(
                            _parameterName: "sender",
                            value: (string)
                                senders[key: recipientRow.refOrigamNotificationChannelTypeId]
                        ),
                        new QueryParameter(
                            _parameterName: "recipients",
                            value: recipient.ContactIdentification
                        ),
                        new QueryParameter(_parameterName: "body", value: notificationBody),
                        new QueryParameter(_parameterName: "subject", value: notificationSubject),
                        new QueryParameter(
                            _parameterName: "notificationChannelTypeId",
                            value: recipientRow.refOrigamNotificationChannelTypeId
                        ),
                    };
                    if (notification.SendAttachments)
                    {
                        queryParameterCollection.Add(
                            value: new QueryParameter(
                                _parameterName: "attachmentRecordId",
                                value: (Guid)
                                    queueItem.Tables[index: 0].Rows[index: 0][columnName: "Id"]
                            )
                        );
                    }
                    core.WorkflowService.ExecuteWorkflow(
                        workflowId: new Guid(g: "0fea481a-24ab-4e98-8793-617ab5bb7272"),
                        parameters: queryParameterCollection,
                        transactionId: transactionId
                    );
                }
            }
        }
    }

    private static DataRow ExtractWorkQueueRowIfNotNotificationDataStructureIsSet(
        WorkQueueClass workQueueClass,
        DataSet queueItem
    )
    {
        // store WorkQueueRow if notification data structure is set
        // we send the work queue data anyway as a parameter (to all workflows)
        DataRow workQueueRow = null;
        if (workQueueClass.NotificationStructure != null)
        {
            workQueueRow = queueItem.Tables[index: 0].Rows[index: 0];
        }
        return workQueueRow;
    }

    /// <summary>
    /// Prepare a data to be sent as a notification.
    /// </summary>
    /// <param name="notificationTemplateId">Id of message template</param>
    /// <param name="notificationSource">notificationSource Input context for transformation - data taken either from WorkQueue Entry or fetched by refId </param>
    /// <param name="recipientRow">Recipient info</param>
    /// <param name="transactionId">current db transaction</param>
    /// <returns>Xml containing in the first row of the first table non-empty fields named "Body" and "Subject"</returns>
    public IDataDocument GenerateNotificationMessage(
        Guid notificationTemplateId,
        IXmlContainer notificationSource,
        DataRow recipientRow,
        DataRow workQueueRow,
        string transactionId
    )
    {
        if (
            recipientRow is not OrigamNotificationContactData.OrigamNotificationContactRow recipient
        )
        {
            throw new Exception(
                message: "Recipient must be type OrigamNotificationContactData.OrigamNotificationContactRow."
            );
        }
        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        using var langSwitcher = new LanguageSwitcher(
            langIetf: !recipient.IsLanguageTagIETFNull() ? recipient.LanguageTagIETF : ""
        );
        // get the current localized XSLT template
        DataSet templateData = dataService.LoadData(
            dataStructureId: new Guid(g: "92c3c8b4-68a3-482b-8a90-f7142c4b17ec"), // OrigamNotificationTemplate DS
            methodId: new Guid(g: "3724bd2a-9466-4129-bdfa-ca8dc8621a72"), // GetId
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: transactionId,
            paramName1: "OrigamNotificationTemplate_parId",
            paramValue1: notificationTemplateId
        );
        string template = (string)
            templateData.Tables[index: 0].Rows[index: 0][columnName: "Template"];
        // transform
        var resultStructure = persistence.SchemaProvider.RetrieveInstance<DataStructure>(
            instanceId: new Guid(g: "2f5e1853-e885-4177-ab6d-9da52123ae82")
        );
        IXsltEngine transform = new CompiledXsltEngine(persistence: persistence.SchemaProvider);
        var parameters = new Hashtable
        {
            [key: "RecipientRow"] = DatasetTools.GetRowXml(
                row: recipient,
                version: DataRowVersion.Default
            ),
        };
        if (workQueueRow != null)
        {
            parameters[key: "WorkQueueRow"] = DatasetTools.GetRowXml(
                row: workQueueRow,
                version: DataRowVersion.Default
            );
        }
        var notificationData = (IDataDocument)
            transform.Transform(
                data: notificationSource,
                xsl: template,
                parameters: parameters,
                transactionId: transactionId,
                outputStructure: resultStructure,
                validateOnly: false
            );
        return notificationData;
    }

    public string CustomScreenName(Guid queueId)
    {
        return workQueueUtils.CustomScreenName(queueId: queueId);
    }

    private OrigamNotificationContactData GetNotificationContacts(
        Guid workQueueNotificationContactTypeId,
        Guid origamNotificationChannelTypeId,
        string value,
        IXmlContainer context,
        DataRow workQueueRow,
        string transactionId
    )
    {
        OrigamNotificationContactData result = new OrigamNotificationContactData();
        if (
            workQueueNotificationContactTypeId.Equals(
                g: new Guid(g: "3535c6f5-c48d-4ae9-ba21-43852d4f66f8")
            )
        )
        {
            // manual entry
            OrigamNotificationContactData.OrigamNotificationContactRow recipient =
                result.OrigamNotificationContact.NewOrigamNotificationContactRow();
            recipient.ContactIdentification = value;
            result.OrigamNotificationContact.AddOrigamNotificationContactRow(row: recipient);
        }
        else
        {
            // anything else - we execute the workflow to get the addresses
            var queryParameterCollection = new QueryParameterCollection
            {
                new QueryParameter(
                    _parameterName: "workQueueNotificationContactTypeId",
                    value: workQueueNotificationContactTypeId
                ),
                new QueryParameter(
                    _parameterName: "OrigamNotificationChannelTypeId",
                    value: origamNotificationChannelTypeId
                ),
                new QueryParameter(_parameterName: "value", value: value),
                new QueryParameter(_parameterName: "context", value: context),
            };
            if (workQueueRow != null)
            {
                queryParameterCollection.Add(
                    value: new QueryParameter(
                        _parameterName: "WorkQueueRow",
                        value: DatasetTools.GetRowXml(
                            row: workQueueRow,
                            version: DataRowVersion.Default
                        )
                    )
                );
            }
            if (
                core.WorkflowService.ExecuteWorkflow(
                    workflowId: new Guid(g: "1e621daf-c70d-4cc1-9a52-73427c499006"),
                    parameters: queryParameterCollection,
                    transactionId: transactionId
                )
                is IDataDocument wfResult
            )
            {
                DatasetTools.MergeDataSetVerbose(mergeInDS: result, mergeFromDS: wfResult.DataSet);
            }
        }
        return result;
    }

    private void WorkQueueRowFill(
        WorkQueueClass workQueueClass,
        RuleEngine ruleEngine,
        DataRow row,
        IXmlContainer data
    )
    {
        foreach (WorkQueueClassEntityMapping entityMapping in workQueueClass.EntityMappings)
        {
            if (string.IsNullOrEmpty(value: entityMapping.XPath))
            {
                continue;
            }
            DataColumn dataColumn = row.Table.Columns[name: entityMapping.Name];
            OrigamDataType dataType = (OrigamDataType)
                dataColumn.ExtendedProperties[key: "OrigamDataType"];
            object value = ruleEngine.EvaluateContext(
                xpath: entityMapping.XPath,
                context: data,
                dataType: dataType,
                targetStructure: workQueueClass.WorkQueueStructure
            );
            if (
                value is string sValue
                && (dataColumn.MaxLength > 0 & sValue.Length > dataColumn.MaxLength)
            )
            {
                // handle string length
                row[columnName: entityMapping.Name] =
                    sValue.Substring(startIndex: 0, length: dataColumn.MaxLength - 4) + " ...";
            }
            else
            {
                row[columnName: entityMapping.Name] = value ?? DBNull.Value;
            }
        }
        // set refId to self if it was not mapped to a source row id, so e.g. notifications can
        // load the work queue entry data
        if (row.IsNull(columnName: "refId"))
        {
            row[columnName: "refId"] = row[columnName: "Id"];
        }
    }

    private DataSet FetchSingleQueueEntry(
        WorkQueueClass workQueueClass,
        object queueEntryId,
        string transactionId
    )
    {
        if (
            workQueueClass.WorkQueueStructure.GetChildByName(
                name: "GetById",
                itemType: DataStructureMethod.CategoryConst
            )
            is not DataStructureMethod getOneEntryMethod
        )
        {
            throw new OrigamException(
                message: $"Programming Error: Can't find a filterset called 'GetById' in DataStructure '{workQueueClass.WorkQueueStructure.Name}'. Please add the filterset to the DataStructure."
            );
        }
        // fetch entry by Id
        DataSet queueEntryDataSet = dataService.LoadData(
            dataStructureId: workQueueClass.WorkQueueStructureId,
            methodId: getOneEntryMethod.Id,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: transactionId,
            paramName1: "WorkQueueEntry_parId",
            paramValue1: queueEntryId
        );
        if (queueEntryDataSet.Tables[index: 0].Rows.Count == 1)
        {
            return queueEntryDataSet;
        }
        throw new RuleException(
            message: ResourceUtils.GetString(key: "ErrorWorkQueueEntryNotFound"),
            severity: RuleExceptionSeverity.High,
            fieldName: "ErrorWorkQueueEntryNotFound",
            entityName: "WorkQueueEntry"
        );
    }

    public void WorkQueueRemove(Guid workQueueId, object queueEntryId, string transactionId)
    {
        if (queueEntryId == null)
        {
            return;
        }

        WorkQueueData queue = GetQueue(queueId: workQueueId);
        WorkQueueData.WorkQueueRow queueRow = queue.WorkQueue[index: 0];
        if (log.IsDebugEnabled)
        {
            log.Debug(message: $"Removing Work Queue Entries for Queue: {queueRow.Name}");
        }
        WorkQueueClass workQueueClass = workQueueUtils.WorkQueueClass(
            name: queueRow.WorkQueueClass
        );
        if (workQueueClass == null)
        {
            return;
        }
        DataSet queueEntryDataSet = FetchSingleQueueEntry(
            workQueueClass: workQueueClass,
            queueEntryId: queueEntryId,
            transactionId: transactionId
        );
        queueEntryDataSet.Tables[index: 0].Rows[index: 0].Delete();
        try
        {
            dataService.StoreData(
                dataStructureId: workQueueClass.WorkQueueStructure.Id,
                data: queueEntryDataSet,
                loadActualValuesAfterUpdate: false,
                transactionId: transactionId
            );
        }
        catch (DBConcurrencyException)
        {
            dataService.StoreData(
                dataStructureId: new Guid(g: "7ca0c208-9ac8-4c55-bd0e-32575b613654"),
                data: queueEntryDataSet,
                loadActualValuesAfterUpdate: false,
                transactionId: transactionId
            );
        }
        if (log.IsDebugEnabled)
        {
            log.Debug(
                message: $"Removed Work Queue Entry '{queueEntryId}'  from Queue: {queueRow.Name}"
            );
        }
    }

    public void WorkQueueRemove(
        string workQueueClassIdentifier,
        string workQueueName,
        Guid workQueueId,
        string condition,
        object rowKey,
        string transactionId
    )
    {
        if (rowKey == null)
        {
            return;
        }

        if (log.IsDebugEnabled)
        {
            log.Debug(
                message: $"Removing Work Queue Entries for Queue: {workQueueName} for row Id {rowKey}"
            );
        }
        WorkQueueClass workQueueClass = workQueueUtils.WorkQueueClass(
            name: workQueueClassIdentifier
        );
        if (workQueueClass == null)
        {
            return;
        }
        // get queue entries for this row and queue
        if (
            workQueueClass.WorkQueueStructure.GetChildByName(
                name: "GetByMasterId",
                itemType: DataStructureMethod.CategoryConst
            )
            is not DataStructureMethod primaryKeyMethod
        )
        {
            throw new Exception(
                message: $"GetByMasterId method not found for data structure of work queue class '{workQueueClass.Name}'"
            );
        }
        DataSet dataSet = dataService.LoadData(
            dataStructureId: workQueueClass.WorkQueueStructureId,
            methodId: primaryKeyMethod.Id,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: transactionId,
            paramName1: "WorkQueueEntry_parRefId",
            paramValue1: rowKey,
            paramName2: "WorkQueueEntry_parWorkQueueId",
            paramValue2: workQueueId
        );
        int count = dataSet.Tables[index: 0].Rows.Count;
        if (count > 0)
        {
            foreach (DataRow rowToDelete in dataSet.Tables[index: 0].Rows)
            {
                bool delete = true;
                if (!string.IsNullOrEmpty(value: condition))
                {
                    if (
                        !EvaluateWorkQueueCondition(
                            queueRow: rowToDelete,
                            condition: condition,
                            queueName: workQueueName,
                            transactionId: transactionId
                        )
                    )
                    {
                        delete = false;
                        count--;
                    }
                }
                if (delete)
                {
                    rowToDelete.Delete();
                }
            }
            dataService.StoreData(
                dataStructureId: workQueueClass.WorkQueueStructure.Id,
                data: dataSet,
                loadActualValuesAfterUpdate: false,
                transactionId: transactionId
            );
        }
        if (log.IsDebugEnabled)
        {
            log.Debug(message: $"Removed {count} work Queue Entries from Queue: {workQueueName}");
        }
    }

    public void WorkQueueUpdate(
        string workQueueClassIdentifier,
        int relationNo,
        Guid workQueueId,
        object rowKey,
        string transactionId
    )
    {
        if (rowKey == null)
        {
            return;
        }

        WorkQueueClass workQueueClass = workQueueUtils.WorkQueueClass(
            name: workQueueClassIdentifier
        );
        RuleEngine ruleEngine = RuleEngine.Create(
            contextStores: new Hashtable(),
            transactionId: transactionId
        );
        UserProfile profile = SecurityManager.CurrentUserProfile();
        // get filterset for this relation no (1-7)
        string filterSetName = null;
        DataStructureFilterSet filterSet = null;
        if (relationNo == 0)
        {
            filterSetName = "GetByMasterId";
        }
        else if (relationNo > 0 & relationNo < 8)
        {
            filterSetName = $"GetByRel{relationNo}";
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(relationNo),
                actualValue: relationNo,
                message: ResourceUtils.GetString(key: "ErrorMaxWorkQueueEntities")
            );
        }
        filterSet = (DataStructureFilterSet)
            workQueueClass.WorkQueueStructure.GetChildByName(
                name: filterSetName,
                itemType: DataStructureFilterSet.CategoryConst
            );
        if (filterSet == null)
        {
            throw new Exception(
                message: ResourceUtils.GetString(
                    key: "ErrorNoFilterSet",
                    args: new object[] { workQueueClass.WorkQueueStructure.Path, filterSetName }
                )
            );
        }
        // load all entries in the queue related to this entity
        DataSet entries = dataService.LoadData(
            dataStructureId: workQueueClass.WorkQueueStructureId,
            methodId: filterSet.Id,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: transactionId,
            paramName1: "WorkQueueEntry_parRefId",
            paramValue1: rowKey,
            paramName2: "WorkQueueEntry_parWorkQueueId",
            paramValue2: workQueueId
        );
        string primaryKeyParamName = null;
        foreach (
            string parameterName in workQueueClass
                .EntityStructurePrimaryKeyMethod
                .ParameterReferences
                .Keys
        )
        {
            primaryKeyParamName = parameterName;
        }
        if (primaryKeyParamName == null)
        {
            throw new OrigamException(
                message: $"Entity Structure Primary Key Method '{workQueueClass.EntityStructurePrimaryKeyMethod.Path}' specified in a work queue class '{workQueueClass.Path}' has no parameters. A parameter to load a record by its primary key is expected."
            );
        }
        // for-each entry
        foreach (DataRow entry in entries.Tables[index: 0].Rows)
        {
            // load original record
            DataSet originalRecord = dataService.LoadData(
                dataStructureId: workQueueClass.EntityStructureId,
                methodId: workQueueClass.EntityStructurePkMethodId,
                defaultSetId: Guid.Empty,
                sortSetId: Guid.Empty,
                transactionId: transactionId,
                paramName1: primaryKeyParamName,
                paramValue1: entry[columnName: "refId"]
            );
            // record could have been deleted in the meantime, we test
            if (originalRecord.Tables[index: 0].Rows.Count <= 0)
            {
                continue;
            }
            IXmlContainer data = DatasetTools.GetRowXml(
                row: originalRecord.Tables[index: 0].Rows[index: 0],
                version: DataRowVersion.Default
            );
            // update entry from record
            WorkQueueRowFill(
                workQueueClass: workQueueClass,
                ruleEngine: ruleEngine,
                row: entry,
                data: data
            );
            entry[columnName: "RecordUpdated"] = DateTime.Now;
            entry[columnName: "RecordUpdatedBy"] = profile.Id;
        }
        // save entries
        StoreQueueItems(
            workQueueClass: workQueueClass,
            selectedRows: entries.Tables[index: 0],
            transactionId: transactionId
        );
    }
    #endregion
    /// <summary>
    /// Handles work queue actions from the UI. WILL NOT CREATE TRANSACTIONS!
    /// </summary>
    /// <param name="workQueueClassIdentifier"></param>
    /// <param name="selectedRows"></param>
    /// <param name="commandType"></param>
    /// <param name="command"></param>
    /// <param name="param1"></param>
    /// <param name="param2"></param>
    /// <param name="errorQueueId"></param>
    public void HandleAction(
        Guid queueId,
        string workQueueClassIdentifier,
        DataTable selectedRows,
        Guid commandType,
        string command,
        string param1,
        string param2,
        object errorQueueId
    )
    {
        try
        {
            WorkQueueData workQueueData = GetQueue(queueId: queueId);
            WorkQueueData.WorkQueueRow queue = workQueueData.WorkQueue[index: 0];
            HandleAction(
                queue: queue,
                workQueueClassIdentifier: workQueueClassIdentifier,
                selectedRows: selectedRows,
                commandType: commandType,
                command: command,
                param1: param1,
                param2: param2,
                lockItems: true,
                errorQueueId: errorQueueId,
                transactionId: null
            );
        }
        catch
        {
            // quit silently if work queue is defined because it was moved to error queue already
            if (errorQueueId == null)
            {
                throw;
            }
        }
    }

    private static void CheckSelectedRowsCountPositive(int count)
    {
        if (count == 0)
        {
            throw new RuleException(
                message: ResourceUtils.GetString(key: "ErrorNoRecordsSelectedForAction")
            );
        }
    }

    private void HandleAction(
        WorkQueueData.WorkQueueRow queue,
        string workQueueClassIdentifier,
        DataTable selectedRows,
        Guid commandType,
        string command,
        string param1,
        string param2,
        bool lockItems,
        object errorQueueId,
        string transactionId
    )
    {
        if (log.IsInfoEnabled)
        {
            log.Info(
                message: $"Begin HandleAction() queue class: {workQueueClassIdentifier} command: {command} lockItems: {lockItems}"
            );
        }
        // set all rows to be actual values, not added (in case the calling function did not do that)
        selectedRows.AcceptChanges();
        WorkQueueClass workQueueClass = workQueueUtils.WorkQueueClass(
            name: workQueueClassIdentifier
        );
        try
        {
            if (lockItems)
            {
                LockQueueItems(workQueueClass: workQueueClass, selectedRows: selectedRows);
            }

            var parameterService = ServiceManager.Services.GetService<IParameterService>();
            if (
                commandType
                == (Guid)
                    parameterService.GetParameterValue(
                        parameterName: "WorkQueueCommandType_StateChange"
                    )
            )
            {
                HandleStateChange(
                    workQueueClassIdentifier: workQueueClassIdentifier,
                    selectedRows: selectedRows,
                    fieldName: param1,
                    newValue: param2,
                    transactionId: transactionId
                );
            }
            else if (
                commandType
                == (Guid)
                    parameterService.GetParameterValue(parameterName: "WorkQueueCommandType_Remove")
            )
            {
                HandleRemove(
                    workQueueClassIdentifier: workQueueClassIdentifier,
                    selectedRows: selectedRows,
                    transactionId: transactionId
                );
            }
            else if (
                commandType
                == (Guid)
                    parameterService.GetParameterValue(parameterName: "WorkQueueCommandType_Move")
            )
            {
                HandleMove(
                    workQueueClassIdentifier: workQueueClassIdentifier,
                    selectedRows: selectedRows,
                    newQueueReferenceCode: param1,
                    transactionId: transactionId,
                    resetErrors: true
                );
            }
            else if (
                commandType
                == (Guid)
                    parameterService.GetParameterValue(
                        parameterName: "WorkQueueCommandType_WorkQueueClassCommand"
                    )
            )
            {
                HandleWorkflow(
                    workQueueClassIdentifier: workQueueClassIdentifier,
                    selectedRows: selectedRows,
                    command: command,
                    param1: param1,
                    param2: param2,
                    transactionId: transactionId
                );
            }
            else if (
                commandType
                == (Guid)
                    parameterService.GetParameterValue(
                        parameterName: "WorkQueueCommandType_Archive"
                    )
            )
            {
                HandleMove(
                    workQueueClassIdentifier: workQueueClassIdentifier,
                    selectedRows: selectedRows,
                    newQueueReferenceCode: param1,
                    transactionId: transactionId,
                    resetErrors: false
                );
            }
            else if (
                commandType
                == (Guid)
                    parameterService.GetParameterValue(
                        parameterName: "WorkQueueCommandType_LoadFromExternalSource"
                    )
            )
            {
                LoadFromExternalSource(queueId: queue.Id);
            }
            else if (
                commandType
                == (Guid)
                    parameterService.GetParameterValue(
                        parameterName: "WorkQueueCommandType_RunNotifications"
                    )
            )
            {
                HandleRunNotifications(
                    workQueueClassIdentifier: workQueueClassIdentifier,
                    selectedRows: selectedRows,
                    transactionId: transactionId
                );
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(commandType),
                    actualValue: commandType,
                    message: ResourceUtils.GetString(key: "ErrorUnknownWorkQueueCommand")
                );
            }
            if (lockItems)
            {
                UnlockQueueItems(selectedRows: selectedRows);
            }
        }
        catch (WorkQueueItemLockedException)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(
                    message: $"Error occured while processing work queue items., Queue: {workQueueClass?.Name}, Command: {command}",
                    ex: ex
                );
            }
            if (transactionId != null)
            {
                ResourceMonitor.Rollback(transactionId: transactionId);
            }
            // rollback deletion of the row when it fails - we have to work with previous state
            bool anyRowInRetry = false;
            foreach (DataRow row in selectedRows.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    row.RejectChanges();
                }
                retryManager.SetEntryRetryData(
                    queueEntryRow: row,
                    queue: queue,
                    errorMessage: ex.Message
                );
                anyRowInRetry = anyRowInRetry || (bool)row[columnName: "InRetry"];
            }
            StoreFailedEntries(wqc: workQueueClass, queueEntryTable: selectedRows);
            // other failure => move to the error queue, if available
            if (!anyRowInRetry && errorQueueId != null)
            {
                HandleMoveQueue(
                    workQueueClass: workQueueClass,
                    selectedRows: selectedRows,
                    newQueueId: (Guid)errorQueueId,
                    errorMessage: ex.Message,
                    transactionId: null,
                    resetErrors: false
                );
            }
            // unlock the queue item
            if (lockItems)
            {
                UnlockQueueItems(selectedRows: selectedRows, rejectChangesWhenDeleted: true);
            }

            throw;
        }
        if (log.IsInfoEnabled)
        {
            log.Info(
                message: $"Finished HandleAction() queue class: {workQueueClassIdentifier} command: {command}"
            );
        }
    }

    public void HandleAction(string workQueueCode, string commandText, Guid queueEntryId)
    {
        Guid queueId = workQueueUtils.GetQueueId(referenceCode: workQueueCode);
        // get all queue data from database (no entries)
        WorkQueueData queue = GetQueue(queueId: queueId);
        // extract WorkQueueClass name and construct WorkQueueClass from name
        WorkQueueData.WorkQueueRow queueRow = queue.WorkQueue[index: 0];
        var workQueueClass = (WorkQueueClass)WQClass(name: queueRow.WorkQueueClass);
        // authorize access from API
        IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
        if (
            queueRow.IsApiAccessRolesNull()
            || !auth.Authorize(
                principal: SecurityManager.CurrentPrincipal,
                context: queueRow.ApiAccessRoles
            )
        )
        {
            throw new RuleException(
                message: string.Format(
                    format: ResourceUtils.GetString(key: "ErrorWorkQueueApiNotAuthorized"),
                    arg0: queueId
                ),
                severity: RuleExceptionSeverity.High,
                fieldName: "queueId",
                entityName: ""
            );
        }
        // find command in the queue data
        WorkQueueData.WorkQueueCommandRow commandRow = null;
        foreach (
            WorkQueueData.WorkQueueCommandRow workQueueCommandRow in queue.WorkQueueCommand.Rows
        )
        {
            if (workQueueCommandRow.Text == commandText)
            {
                commandRow = workQueueCommandRow;
            }
        }
        if (
            commandRow.IsRolesNull()
            || !auth.Authorize(
                principal: SecurityManager.CurrentPrincipal,
                context: commandRow.Roles
            )
        )
        {
            throw new RuleException(
                message: string.Format(
                    format: ResourceUtils.GetString(key: "ErrorWorkQueueCommandNotAuthorized"),
                    arg0: commandText,
                    arg1: queueId
                ),
                severity: RuleExceptionSeverity.High,
                fieldName: "commandId",
                entityName: ""
            );
        }
        // fetch a single queue entry
        DataSet queueEntryDataSet = FetchSingleQueueEntry(
            workQueueClass: workQueueClass,
            queueEntryId: queueEntryId,
            transactionId: null
        );
        // call handle action
        HandleAction(
            queue: queueRow,
            workQueueClassIdentifier: queueRow.WorkQueueClass,
            selectedRows: queueEntryDataSet.Tables[index: 0],
            commandType: commandRow.refWorkQueueCommandTypeId,
            command: commandRow.IsCommandNull() ? null : commandRow.Command,
            param1: commandRow.IsParam1Null() ? null : commandRow.Param1,
            param2: commandRow.IsParam2Null() ? null : commandRow.Param2,
            lockItems: true,
            errorQueueId: commandRow.IsrefErrorWorkQueueIdNull()
                ? null
                : commandRow.refErrorWorkQueueId,
            transactionId: null
        );
    }

    public DataRow GetNextItem(string workQueueName, string transactionId, bool processErrors)
    {
        Guid queueId = workQueueUtils.GetQueueId(referenceCode: workQueueName);
        WorkQueueData.WorkQueueRow queue = GetQueue(queueId: queueId).WorkQueue[index: 0];
        return queueProcessor.GetNextItem(
            queue: queue,
            transactionId: transactionId,
            processErrors: processErrors,
            cancellationToken: cancellationTokenSource.Token
        );
    }

    private void LockQueueItems(WorkQueueClass workQueueClass, DataTable selectedRows)
    {
        if (!workQueueUtils.LockQueueItems(queueClass: workQueueClass, selectedRows: selectedRows))
        {
            throw new WorkQueueItemLockedException();
        }
    }

    /// <summary>
    /// Unlocks work queue entries in a separate (new) database transaction
    /// </summary>
    /// <param name="selectedRows">Work queue entries to unlock </param>
    /// <param name="rejectChangesWhenDeleted">This should be set if you call the
    /// function from within catch handler. In that case the original transaction
    /// has been rollbacked and even already removed entries has been recreated.
    /// We need to reflect that state in the dataset (reject changes)
    /// and that way unlock deleted rows.
    /// </param>
    private void UnlockQueueItems(DataTable selectedRows, bool rejectChangesWhenDeleted = false)
    {
        foreach (DataRow row in selectedRows.Rows)
        {
            if (rejectChangesWhenDeleted && row.RowState == DataRowState.Deleted)
            {
                row.RejectChanges();
            }
            if (row.RowState != DataRowState.Deleted)
            {
                Guid id = (Guid)row[columnName: "Id"];
                if (log.IsDebugEnabled)
                {
                    log.Debug(message: $"Unlocking work queue item id {id}");
                }
                // load the fresh queue entry from the database, because
                // it may have been e.g. deleted after state change (in that case
                // nothing will happen/nothing will be unlocked)
                DataSet data = dataService.LoadData(
                    dataStructureId: new Guid(g: "59de7db2-e2f4-437b-b191-0fd3bc766685"),
                    methodId: new Guid(g: "a68a8990-f476-4a64-bb9a-e45228eb9aae"),
                    defaultSetId: Guid.Empty,
                    sortSetId: Guid.Empty,
                    transactionId: null,
                    paramName1: "WorkQueueEntry_parId",
                    paramValue1: id
                );
                foreach (DataRow freshRow in data.Tables[index: 0].Rows)
                {
                    freshRow[columnName: "IsLocked"] = false;
                    freshRow[columnName: "refLockedByBusinessPartnerId"] = DBNull.Value;
                }
                dataService.StoreData(
                    dataStructureId: new Guid(g: "59de7db2-e2f4-437b-b191-0fd3bc766685"),
                    data: data,
                    loadActualValuesAfterUpdate: false,
                    transactionId: null
                );
            }
        }
    }

    private void HandleMoveQueue(
        WorkQueueClass workQueueClass,
        DataTable selectedRows,
        Guid newQueueId,
        string errorMessage,
        string transactionId,
        bool resetErrors
    )
    {
        foreach (DataRow row in selectedRows.Rows)
        {
            if (log.IsInfoEnabled)
            {
                log.RunHandled(loggingAction: () =>
                {
                    Guid itemId = (Guid)row[columnName: "Id"];
                    log.Info(
                        message: $"Moving queue item {itemId} to queue id {newQueueId}{(errorMessage == null ? "" : " with error: " + errorMessage)}"
                    );
                });
            }
            row[columnName: "refWorkQueueId"] = newQueueId;
            retryManager.ResetEntry(queueEntryRow: row);
            if (resetErrors || errorMessage == null)
            {
                row[columnName: "ErrorText"] = DBNull.Value;
            }
            else
            {
                row[columnName: "ErrorText"] = errorMessage;
            }
        }
        try
        {
            StoreQueueItems(
                workQueueClass: workQueueClass,
                selectedRows: selectedRows,
                transactionId: transactionId
            );
        }
        catch (DBConcurrencyException)
        {
            dataService.StoreData(
                dataStructureId: new Guid(g: "7ca0c208-9ac8-4c55-bd0e-32575b613654"),
                data: selectedRows.DataSet,
                loadActualValuesAfterUpdate: false,
                transactionId: transactionId
            );
        }
        foreach (DataRow row in selectedRows.Rows)
        {
            DataSet slice = DatasetTools.CloneDataSet(dataset: selectedRows.DataSet);
            DatasetTools.GetDataSlice(target: slice, rows: new List<DataRow> { row });
            if (log.IsInfoEnabled)
            {
                log.RunHandled(loggingAction: () =>
                {
                    Guid itemId = (Guid)row[columnName: "Id"];
                    log.Info(message: $"Running notifications for item {itemId}.");
                });
            }
            ProcessNotifications(
                workQueueClass: workQueueClass,
                workQueueId: newQueueId,
                eventTypeId: new Guid(g: WQ_EVENT_ONCREATE),
                queueItem: slice,
                transactionId: transactionId
            );
        }
    }

    private void HandleStateChange(
        string workQueueClassIdentifier,
        DataTable selectedRows,
        string fieldName,
        string newValue,
        string transactionId
    )
    {
        CheckSelectedRowsCountPositive(count: selectedRows.Rows.Count);
        var workQueueClass = (WorkQueueClass)WQClass(name: workQueueClassIdentifier);
        foreach (DataRow row in selectedRows.Rows)
        {
            // get the original record by refId
            string primaryKeyParamName = null;
            foreach (
                string parameterName in workQueueClass
                    .EntityStructurePrimaryKeyMethod
                    .ParameterReferences
                    .Keys
            )
            {
                primaryKeyParamName = parameterName;
            }
            DataSet dataSet = dataService.LoadData(
                dataStructureId: workQueueClass.EntityStructureId,
                methodId: workQueueClass.EntityStructurePkMethodId,
                defaultSetId: Guid.Empty,
                sortSetId: Guid.Empty,
                transactionId: transactionId,
                paramName1: primaryKeyParamName,
                paramValue1: row[columnName: "refId"]
            );
            if (dataSet.Tables[index: 0].Rows.Count == 0)
            {
                throw new Exception(message: ResourceUtils.GetString(key: "ErrorNoRecords"));
            }
            if (!dataSet.Tables[index: 0].Columns.Contains(name: fieldName))
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "fieldName",
                    actualValue: fieldName,
                    message: ResourceUtils.GetString(key: "ErrorSourceFieldNotFound")
                );
            }
            Type dataType = dataSet.Tables[index: 0].Columns[name: fieldName].DataType;
            object value;
            if (dataType == typeof(string))
            {
                value = newValue;
            }
            else if (dataType == typeof(Guid))
            {
                value = new Guid(g: newValue);
            }
            else if (dataType == typeof(int))
            {
                value = XmlConvert.ToInt32(s: newValue);
            }
            else
            {
                throw new Exception(
                    message: ResourceUtils.GetString(
                        key: "ErrorConvertToType",
                        args: dataType.ToString()
                    )
                );
            }
            dataSet.Tables[index: 0].Rows[index: 0][columnName: fieldName] = value;
            dataService.StoreData(
                dataStructureId: workQueueClass.EntityStructureId,
                data: dataSet,
                loadActualValuesAfterUpdate: false,
                transactionId: transactionId
            );
        }
    }

    private void HandleRunNotifications(
        string workQueueClassIdentifier,
        DataTable selectedRows,
        string transactionId
    )
    {
        CheckSelectedRowsCountPositive(count: selectedRows.Rows.Count);
        var workQueueClass = (WorkQueueClass)WQClass(name: workQueueClassIdentifier);
        var parameterService = ServiceManager.Services.GetService<IParameterService>();
        foreach (DataRow row in selectedRows.Rows)
        {
            DataSet slice = DatasetTools.CloneDataSet(dataset: selectedRows.DataSet);
            DatasetTools.GetDataSlice(target: slice, rows: new List<DataRow> { row });
            if (log.IsInfoEnabled)
            {
                log.RunHandled(loggingAction: () =>
                {
                    Guid itemId = (Guid)row[columnName: "Id"];
                    log.Info(message: $"Running notifications for item {itemId}.");
                });
            }
            ProcessNotifications(
                workQueueClass: workQueueClass,
                workQueueId: (Guid)row[columnName: "refWorkQueueId"],
                eventTypeId: (Guid)
                    parameterService.GetParameterValue(
                        parameterName: "WorkQueueNotificationEvent_Command"
                    ),
                queueItem: slice,
                transactionId: transactionId
            );
        }
    }

    private void HandleWorkflow(
        string workQueueClassIdentifier,
        DataTable selectedRows,
        string command,
        string param1,
        string param2,
        string transactionId
    )
    {
        CheckSelectedRowsCountPositive(count: selectedRows.Rows.Count);
        var workQueueClass = (WorkQueueClass)WQClass(name: workQueueClassIdentifier);
        WorkQueueWorkflowCommand cmd = workQueueClass.GetCommand(name: command);
        var parameters = new QueryParameterCollection();
        foreach (WorkQueueWorkflowCommandParameterMapping parameterMapping in cmd.ParameterMappings)
        {
            object val = parameterMapping.Value switch
            {
                WorkQueueCommandParameterMappingType.QueueEntries => GetDataDocumentFactory(
                    dataSet: selectedRows.DataSet
                ),
                WorkQueueCommandParameterMappingType.Parameter1 => param1,
                WorkQueueCommandParameterMappingType.Parameter2 => param2,
                _ => throw new ArgumentOutOfRangeException(
                    paramName: "Value",
                    actualValue: parameterMapping.Value,
                    message: ResourceUtils.GetString(key: "ErrorUnknownWorkQueueCommandValue")
                ),
            };
            parameters.Add(
                value: new QueryParameter(_parameterName: parameterMapping.Name, value: val)
            );
        }
        core.WorkflowService.ExecuteWorkflow(
            workflowId: cmd.WorkflowId,
            parameters: parameters,
            transactionId: transactionId
        );
    }

    private object GetDataDocumentFactory(DataSet dataSet)
    {
        var val =
            dataSet.ExtendedProperties[key: "IDataDocument"]
            ?? DataDocumentFactory.New(dataSet: dataSet);
        if (dataSet.ExtendedProperties[key: "IDataDocument"] == null)
        {
            dataSet.ExtendedProperties.Add(key: "IDataDocument", value: val);
        }
        return val;
    }

    private void HandleRemove(
        string workQueueClassIdentifier,
        DataTable selectedRows,
        string transactionId
    )
    {
        CheckSelectedRowsCountPositive(count: selectedRows.Rows.Count);
        if (log.IsInfoEnabled)
        {
            log.Info(message: $"Begin HandleRemove() queue class: {workQueueClassIdentifier}");
        }
        var workQueueClass = (WorkQueueClass)WQClass(name: workQueueClassIdentifier);
        foreach (DataRow row in selectedRows.Rows)
        {
            row.Delete();
        }
        try
        {
            StoreQueueItems(
                workQueueClass: workQueueClass,
                selectedRows: selectedRows,
                transactionId: transactionId
            );
        }
        catch (DBConcurrencyException)
        {
            dataService.StoreData(
                dataStructureId: new Guid(g: "fb5d8abe-99b8-4ca0-871a-c8c6e3ae6b76"),
                data: selectedRows.DataSet,
                loadActualValuesAfterUpdate: false,
                transactionId: transactionId
            );
        }
        if (log.IsInfoEnabled)
        {
            log.Info(message: $"Finished HandleRemove() queue class: {workQueueClassIdentifier}");
        }
    }

    private void HandleMove(
        string workQueueClassIdentifier,
        DataTable selectedRows,
        string newQueueReferenceCode,
        string transactionId,
        bool resetErrors
    )
    {
        CheckSelectedRowsCountPositive(count: selectedRows.Rows.Count);
        if (log.IsInfoEnabled)
        {
            log.Info(message: $"Begin HandleMove() queue class: {workQueueClassIdentifier}");
        }
        Guid newQueueId = workQueueUtils.GetQueueId(referenceCode: newQueueReferenceCode);
        var workQueueClass = (WorkQueueClass)WQClass(name: workQueueClassIdentifier);
        HandleMoveQueue(
            workQueueClass: workQueueClass,
            selectedRows: selectedRows,
            newQueueId: newQueueId,
            errorMessage: null,
            transactionId: transactionId,
            resetErrors: resetErrors
        );
        if (log.IsInfoEnabled)
        {
            log.Info(message: $"Finished HandleMove() queue class: {workQueueClassIdentifier}");
        }
    }

    private void StoreQueueItems(
        WorkQueueClass workQueueClass,
        DataTable selectedRows,
        string transactionId
    )
    {
        dataService.StoreData(
            dataStructureId: workQueueClass.WorkQueueStructureId,
            data: selectedRows.DataSet,
            loadActualValuesAfterUpdate: true,
            transactionId: transactionId
        );
    }

    public WorkQueueData GetQueues(
        bool activeOnly = true,
        bool ignoreQueueProcessors = false,
        string transactionId = null
    )
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        WorkQueueData queues = new WorkQueueData();

        Guid filterMethodGuid = activeOnly
            ? (
                ignoreQueueProcessors
                    ? DS_METHOD_WQ_GETACTIVEQUEUES
                    : DS_METHOD_WQ_GETACTIVEQUEUESBYPROCESSOR
            )
            : Guid.Empty;

        queues.Merge(
            dataSet: dataService.LoadData(
                dataStructureId: DS_WORKQUEUE,
                methodId: filterMethodGuid,
                defaultSetId: Guid.Empty,
                sortSetId: DS_SORTSET_WQ_SORT,
                transactionId: transactionId,
                paramName1: "WorkQueue_parQueueProcessor",
                paramValue1: settings.Name
            )
        );
        return queues;
    }

    private WorkQueueData GetQueue(Guid queueId)
    {
        WorkQueueData queues = new WorkQueueData();
        queues.Merge(
            dataSet: dataService.LoadData(
                dataStructureId: new Guid(g: "7b44a488-ac98-4fe1-a427-55de0ff9e12e"),
                methodId: new Guid(g: "2543bbd3-3592-4b14-9d74-86d0e9c65d98"),
                defaultSetId: Guid.Empty,
                sortSetId: new Guid(g: "c1ec9d9e-09a2-47ad-b5e4-b57107c4dc34"),
                transactionId: null,
                paramName1: "WorkQueue_parId",
                paramValue1: queueId
            )
        );
        return queues;
    }

    private void LoadExternalWorkQueuesElapsed(object sender, ElapsedEventArgs e)
    {
        LoadFromExternalSource();
    }

    private void LoadFromExternalSource()
    {
        LoadFromExternalSource(queueId: Guid.Empty);
    }

    private void LoadFromExternalSource(Guid queueId)
    {
        var schemaService = ServiceManager.Services.GetService<SchemaService>();
        if (externalQueueAdapterBusy || !schemaService.IsSchemaLoaded || serviceBeingUnloaded)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    message: $"Skipping external work queues load: adapterBusy: {externalQueueAdapterBusy}, schemaLoaded: {schemaService?.IsSchemaLoaded}, serviceBeingUnloaded: {serviceBeingUnloaded}"
                );
            }
            return;
        }
        SecurityManager.SetServerIdentity();
        externalQueueAdapterBusy = true;
        if (log.IsInfoEnabled)
        {
            log.Info(message: "Starting loading external work queues.");
        }
        try
        {
            WorkQueueData queues = GetQueues();
            foreach (WorkQueueData.WorkQueueRow workQueueRow in queues.WorkQueue.Rows)
            {
                if (queueId != Guid.Empty && workQueueRow.Id != queueId)
                {
                    continue;
                }
                if (workQueueRow.IsrefWorkQueueExternalSourceTypeIdNull())
                {
                    continue;
                }
                if (log.IsInfoEnabled)
                {
                    log.Info(message: $"Starting loading external work queue {workQueueRow.Name}");
                }
                try
                {
                    ProcessExternalQueue(workQueueRow: workQueueRow);
                    StoreQueues(queues: queues);
                }
                catch (Exception ex)
                {
                    if (log.IsErrorEnabled)
                    {
                        log.LogOrigamError(
                            message: $"Failed loading external work queue {workQueueRow.Name}",
                            ex: ex
                        );
                    }
                    workQueueRow.ExternalSourceLastMessage = ex.Message;
                    workQueueRow.ExternalSourceLastTime = DateTime.Now;
                    StoreQueues(queues: queues);
                }
            }
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(message: "External queue load failed.", ex: ex);
            }
        }
        finally
        {
            externalQueueAdapterBusy = false;
        }
        if (log.IsInfoEnabled)
        {
            log.Info(message: "Finished loading external work queues.");
        }
    }

    private void StoreQueues(WorkQueueData queues)
    {
        dataService.StoreData(
            dataStructureId: new Guid(g: "7b44a488-ac98-4fe1-a427-55de0ff9e12e"),
            data: queues,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
    }

    private bool HasAutoCommand(WorkQueueData.WorkQueueRow queue)
    {
        foreach (
            WorkQueueData.WorkQueueCommandRow workQueueCommandRow in queue.GetWorkQueueCommandRows()
        )
        {
            if (workQueueCommandRow.IsAutoProcessed)
            {
                return true;
            }
        }
        return false;
    }

    private void ProcessQueueItem(WorkQueueData.WorkQueueRow queue, DataRow queueEntryRow)
    {
        var parameterService = ServiceManager.Services.GetService<IParameterService>();
        log.Info(
            message: $"Running ProcessQueueItem in Thread: {Thread.CurrentThread.ManagedThreadId}"
        );

        string itemId = queueEntryRow[columnName: "Id"].ToString();
        string transactionId = Guid.NewGuid().ToString();
        try
        {
            foreach (WorkQueueData.WorkQueueCommandRow cmd in queue.GetWorkQueueCommandRows())
            {
                try
                {
                    if (
                        !IsAutoProcessed(
                            workQueueCommandRow: cmd,
                            workQueueRow: queue,
                            dataRow: queueEntryRow,
                            transactionId: transactionId
                        )
                    )
                    {
                        continue;
                    }
                    if (log.IsInfoEnabled)
                    {
                        log.Info(
                            message: $"Auto processing work queue item. Id: {itemId}, Queue: {queue.Name}, Command: {cmd?.Text}"
                        );
                    }
                    string param1 = null;
                    string param2 = null;
                    string command = null;
                    object errorQueueId = null;
                    if (!cmd.IsParam1Null())
                    {
                        param1 = cmd.Param1;
                    }

                    if (!cmd.IsParam2Null())
                    {
                        param2 = cmd.Param2;
                    }

                    if (!cmd.IsCommandNull())
                    {
                        command = cmd.Command;
                    }

                    if (!cmd.IsrefErrorWorkQueueIdNull())
                    {
                        errorQueueId = cmd.refErrorWorkQueueId;
                    }
                    // actual processing
                    HandleAction(
                        queue: queue,
                        workQueueClassIdentifier: queue.WorkQueueClass,
                        selectedRows: queueEntryRow.Table,
                        commandType: cmd.refWorkQueueCommandTypeId,
                        command: command,
                        param1: param1,
                        param2: param2,
                        lockItems: false,
                        errorQueueId: errorQueueId,
                        transactionId: transactionId
                    );
                    if (log.IsInfoEnabled)
                    {
                        log.Info(
                            message: $"Finished auto processing work queue item. Id: {itemId}, Queue: {queue.Name}, Command: {cmd.Text}"
                        );
                    }
                    if (
                        cmd.refWorkQueueCommandTypeId
                        == (Guid)
                            parameterService.GetParameterValue(
                                parameterName: "WorkQueueCommandType_Remove"
                            )
                    )
                    {
                        break;
                    }
                }
                catch (Exception)
                {
                    // rollback the transaction
                    if (transactionId != null)
                    {
                        ResourceMonitor.Rollback(transactionId: transactionId);
                    }
                    if (queueEntryRow.RowState == DataRowState.Deleted)
                    {
                        queueEntryRow.RejectChanges();
                    }
                    // unlock the queue item
                    UnlockQueueItems(
                        selectedRows: queueEntryRow.Table,
                        rejectChangesWhenDeleted: true
                    );
                    // do not process any other commands on this queue entry
                    throw;
                }
            }
            // commit the transaction
            ResourceMonitor.Commit(transactionId: transactionId);
            // unlock the queue item
            UnlockQueueItems(selectedRows: queueEntryRow.Table);
        }
        // Catch the command exception. Transaction is already rolled back,
        // so we continue to another queue item.
        catch (RuleException ex)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    message: $"Queue item processing failed. Id: {itemId}, Queue: {queue?.Name}",
                    exception: ex
                );
            }
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.Error(
                    message: $"Queue item processing failed. Id: {itemId}, Queue: {queue?.Name}",
                    exception: ex
                );
            }
        }
        finally
        {
            workQueueThrottle.ReportProcessed(queue: queue);
        }
    }

    private bool IsAutoProcessed(
        WorkQueueData.WorkQueueCommandRow workQueueCommandRow,
        WorkQueueData.WorkQueueRow workQueueRow,
        DataRow dataRow,
        string transactionId
    )
    {
        if (!workQueueCommandRow.IsAutoProcessed)
        {
            return false;
        }
        if (
            !(bool)dataRow[columnName: "InRetry"]
            && !workQueueCommandRow.IsAutoProcessedWithErrors
            && !dataRow.IsNull(columnName: "ErrorText")
            && (string)dataRow[columnName: "ErrorText"] != ""
        )
        {
            return false;
        }
        bool result = false;
        if (
            workQueueCommandRow.IsAutoProcessingConditionXPathNull()
            || workQueueCommandRow.AutoProcessingConditionXPath == string.Empty
        )
        {
            // no condition, we always process
            return true;
        }

        if (
            !workQueueCommandRow.IsAutoProcessingConditionXPathNull()
            && workQueueCommandRow.AutoProcessingConditionXPath != string.Empty
        )
        {
            result = EvaluateWorkQueueCondition(
                queueRow: dataRow,
                condition: workQueueCommandRow.AutoProcessingConditionXPath,
                queueName: workQueueRow.Name,
                transactionId: transactionId
            );
        }
        return result;
    }

    private bool EvaluateWorkQueueCondition(
        DataRow queueRow,
        string condition,
        string queueName,
        string transactionId
    )
    {
        // condition, we evaluate the condition
        if (log.IsDebugEnabled)
        {
            log.Debug(
                message: $"Checking condition for work queue item. Id: {queueRow[columnName: "Id"]}, Queue: {queueName}, Condition: {condition}"
            );
        }
        try
        {
            // we have to do it from the copied dataset, because later the XmlDataDocument would be re-created, which
            // is not supported by the .net
            IDataDocument oneRowXml = DataDocumentFactory.New(
                dataSet: queueRow.Table.DataSet.Copy()
            );
            XPathNavigator navigator = oneRowXml.Xml.CreateNavigator();
            navigator.MoveToFirstChild(); // /ROOT/
            navigator.MoveToFirstChild(); // WorkQueueEntry/
            var evaluationResult = (string)
                XpathEvaluator.Instance.Evaluate(
                    xpath: condition,
                    isPathRelative: false,
                    returnDataType: OrigamDataType.String,
                    nav: navigator,
                    contextPosition: null,
                    transactionId: transactionId
                );
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    message: $"Condition for work queue item. Id: {queueRow[columnName: "Id"]}, Queue: {queueName}, Condition: {condition} evaluated to {evaluationResult}"
                );
            }
            try
            {
                if (XmlConvert.ToBoolean(s: evaluationResult))
                {
                    return true;
                }
            }
            catch
            {
                throw new Exception(
                    message: "Work queue condition did not return boolean value. Command will not be processed."
                );
            }
        }
        catch (Exception ex)
        {
            throw new Exception(
                message: "Work queue condition evaluation failed. Command will not be processed.",
                innerException: ex
            );
        }
        return false;
    }

    private void StoreFailedEntries(WorkQueueClass wqc, DataTable queueEntryTable)
    {
        try
        {
            StoreQueueItems(
                workQueueClass: wqc,
                selectedRows: queueEntryTable,
                transactionId: null
            );
        }
        catch (DBConcurrencyException)
        {
            var dataStructureQuery = new DataStructureQuery
            {
                DataSourceId = new Guid(g: "7a18149a-2faa-471b-a43e-9533d7321b44"),
                MethodId = new Guid(g: "ea139b9a-3048-4cd5-bf9a-04a91590624a"),
                LoadActualValuesAfterUpdate = false,
            };
            dataService.StoreData(
                dataStructureQuery: dataStructureQuery,
                data: queueEntryTable.DataSet,
                transactionId: null
            );
        }
    }

    private void ProcessExternalQueue(WorkQueueData.WorkQueueRow workQueueRow)
    {
        string transactionId = Guid.NewGuid().ToString();
        WorkQueueLoaderAdapter adapter = null;
        if (log.IsInfoEnabled)
        {
            log.Info(message: $"Loading external work queue: {workQueueRow?.Name}");
        }
        try
        {
            adapter = WorkQueueAdapterFactory.GetAdapter(
                adapterId: workQueueRow.refWorkQueueExternalSourceTypeId.ToString()
            );
            if (adapter == null)
            {
                throw new Exception(
                    message: $"External Source Adapter not found for queue {workQueueRow.Name}"
                );
            }
            adapter.Connect(
                service: this,
                queueId: workQueueRow.Id,
                workQueueClass: workQueueRow.WorkQueueClass,
                connection: workQueueRow.IsExternalSourceConnectionNull()
                    ? null
                    : workQueueRow.ExternalSourceConnection,
                userName: workQueueRow.IsExternalSourceUserNameNull()
                    ? null
                    : workQueueRow.ExternalSourceUserName,
                password: workQueueRow.IsExternalSourcePasswordNull()
                    ? null
                    : workQueueRow.ExternalSourcePassword,
                transactionId: transactionId
            );
            int itemCount = 0;
            string lastState = null;
            if (!workQueueRow.IsExternalSourceStateNull())
            {
                lastState = workQueueRow.ExternalSourceState;
            }
            // get first item
            WorkQueueAdapterResult queueItem = adapter.GetItem(lastState: lastState);
            while (queueItem != null)
            {
                itemCount++;
                lastState = queueItem.State;
                string creationCondition = (
                    workQueueRow.IsCreationConditionNull() ? null : workQueueRow.CreationCondition
                );
                // put it to the queue
                using (new TransactionScope(scopeOption: TransactionScopeOption.Suppress))
                {
                    WorkQueueAdd(
                        workQueueClassIdentifier: workQueueRow.WorkQueueClass,
                        workQueueName: workQueueRow.Name,
                        workQueueId: workQueueRow.Id,
                        condition: creationCondition,
                        data: queueItem.Document,
                        attachments: queueItem.Attachments,
                        transactionId: transactionId
                    );
                }
                // next
                if (!serviceBeingUnloaded)
                {
                    queueItem = adapter.GetItem(lastState: lastState);
                }
                else
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(message: "Service is being unloaded. Stopping retrieval.");
                    }
                    queueItem = null;
                }
            }
            ResourceMonitor.Commit(transactionId: transactionId);
            adapter.Disconnect();
            workQueueRow.ExternalSourceLastMessage = ResourceUtils.GetString(
                key: "OKMessage",
                args: itemCount.ToString()
            );
            if (itemCount > 0)
            {
                if (lastState == null)
                {
                    workQueueRow.SetExternalSourceStateNull();
                }
                else
                {
                    workQueueRow.ExternalSourceState = lastState;
                }
            }
            workQueueRow.ExternalSourceLastTime = DateTime.Now;
        }
        catch (Exception ex)
        {
            ResourceMonitor.Rollback(transactionId: transactionId);
            if (adapter != null)
            {
                adapter.Disconnect();
            }
            workQueueRow.ExternalSourceLastMessage = ResourceUtils.GetString(
                key: "ErrorMessage",
                args: ex.Message
            );
            workQueueRow.ExternalSourceLastTime = DateTime.Now;
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(message: $"Failed to load queue {workQueueRow.Name}", ex: ex);
            }
        }
    }

    private void LoadExternalWorkQueuesDisposed(object sender, EventArgs e)
    {
        loadExternalWorkQueuesTimer.Elapsed -= LoadExternalWorkQueuesElapsed;
    }

    public WorkQueueData.WorkQueueRow FindQueue(string queueRefCode)
    {
        var schemaService = ServiceManager.Services.GetService<SchemaService>();
        if (!schemaService.IsSchemaLoaded)
        {
            throw new InvalidOperationException(message: "schemaService is not loaded");
        }
        return GetQueues(activeOnly: false)
            .WorkQueue.Rows.Cast<WorkQueueData.WorkQueueRow>()
            .Where(predicate: row => row.ReferenceCode == queueRefCode)
            .Where(predicate: HasAutoCommand)
            .FirstOrDefault();
    }

    public TaskRunner GetAutoProcessorForQueue(
        string queueRefCode,
        int parallelism,
        int forceWaitMs
    )
    {
        var queueToProcess = FindQueue(queueRefCode: queueRefCode);

        if (queueToProcess == null)
        {
            throw new ArgumentException(
                message: $"Queue with code: {queueRefCode} does not exist, is empty or does not have autoprocess set."
            );
        }

        Action<CancellationToken> actionToRunInEachTask = cancellationToken =>
        {
            while (true)
            {
                try
                {
                    queueProcessor.ProcessAutoQueueCommands(
                        queue: queueToProcess,
                        cancellationToken: cancellationToken,
                        maxItemsToProcess: forceWaitMs
                    );
                }
                catch (OrigamException ex)
                {
                    if (IsDeadlock(ex: ex))
                    {
                        HandleDeadLock();
                    }
                    else
                    {
                        throw;
                    }
                }

                const int millisToSleep = 1000;
                log.Info(
                    message: $"Worker in thread {Thread.CurrentThread.ManagedThreadId} "
                        + $"cannot get next Item from the queue, sleeping for "
                        + $"{millisToSleep} ms before trying again..."
                );
                Thread.Sleep(millisecondsTimeout: millisToSleep);
            }
        };

        return new TaskRunner(funcToRun: actionToRunInEachTask, workerCount: parallelism);
    }

    private static void HandleDeadLock()
    {
        var randomGenerator = new Random();
        int millisToSleep = randomGenerator.Next(minValue: 1000, maxValue: 2000);
        log.Warn(
            message: $"Deadlock was caught! Pausing thread {Thread.CurrentThread.ManagedThreadId} for {millisToSleep} ms"
        );
        Thread.Sleep(millisecondsTimeout: millisToSleep);
    }

    private static bool IsDeadlock(OrigamException ex)
    {
        return ex.InnerException is SqlException
            && ex.InnerException.Message.Contains(value: "deadlocked");
    }

    private void WorkQueueAutoProcessTimerElapsed(object sender, ElapsedEventArgs e)
    {
        var schemaService = ServiceManager.Services.GetService<SchemaService>();
        if (queueAutoProcessBusy || !schemaService.IsSchemaLoaded || serviceBeingUnloaded)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    message: $"Skipping auto processing work queues: queueAutoProcessBusy: {queueAutoProcessBusy}, schemaLoaded: {schemaService.IsSchemaLoaded}, serviceBeingUnloaded: {serviceBeingUnloaded}"
                );
            }
            return;
        }
        SecurityManager.SetServerIdentity();
        queueAutoProcessBusy = true;
        if (log.IsInfoEnabled)
        {
            log.Info(message: "Starting auto processing work queues.");
        }

        try
        {
            IEnumerable<WorkQueueData.WorkQueueRow> queues = GetQueues()
                .WorkQueue.Rows.Cast<WorkQueueData.WorkQueueRow>()
                .Where(predicate: HasAutoCommand);
            queueProcessor.Run(queues: queues, cancellationToken: cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(
                    message: "Unexpected error occured while autoprocessing work queues.",
                    ex: ex
                );
            }
        }
        finally
        {
            queueAutoProcessBusy = false;
            if (log.IsInfoEnabled)
            {
                log.Info(message: "Finished auto processing work queues.");
            }
        }
    }

    private void WorkQueueAutoProcessTimerDisposed(object sender, EventArgs e)
    {
        queueAutoProcessTimer.Elapsed -= WorkQueueAutoProcessTimerElapsed;
    }

    private void schemaService_SchemaLoaded(object sender, bool isInteractive)
    {
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        if (settings.LoadExternalWorkQueues)
        {
            if (log.IsInfoEnabled)
            {
                log.Info(
                    message: $"LoadExternalWorkQueues Enabled. Interval: {settings.ExternalWorkQueueCheckPeriod}"
                );
            }

            loadExternalWorkQueuesTimer.Interval = settings.ExternalWorkQueueCheckPeriod * 1000;
            loadExternalWorkQueuesTimer.Elapsed += LoadExternalWorkQueuesElapsed;
            loadExternalWorkQueuesTimer.Disposed += LoadExternalWorkQueuesDisposed;
            loadExternalWorkQueuesTimer.Start();
        }
        else
        {
            loadExternalWorkQueuesTimer.Stop();
        }

        if (settings.AutoProcessWorkQueues)
        {
            queueAutoProcessTimer.Elapsed += WorkQueueAutoProcessTimerElapsed;
            queueAutoProcessTimer.Disposed += WorkQueueAutoProcessTimerDisposed;
            queueAutoProcessTimer.Start();
        }
        else
        {
            queueAutoProcessTimer.Stop();
        }
    }

    private void schemaService_SchemaUnloading(object sender, CancelEventArgs e)
    {
        // work queue shouldn't start new processing here
        // since schemaService.IsSchemaLoaded has been set to false
        // as the first think when the unload started.
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "schemaService_SchemaUnloading");
        }
        StopTasks();
    }

    private void schemaService_SchemaUnloaded(object sender, EventArgs e)
    {
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "schemaService_SchemaUnloaded");
        }
    }
}

public static class WorkQueueRetryType
{
    private const string NoRetryString = "69460BCF-81D4-4A97-94F7-5A391D16F771";
    private const string LinearRetryString = "8A5C793F-73B8-41EF-A459-618A8E6FE4FA";
    private const string ExponentialRetryString = "57AD4C10-1F43-4CCF-A48A-132E7E418D53";

    public static readonly Guid NoRetry = new(g: NoRetryString);
    public static readonly Guid LinearRetry = new(g: LinearRetryString);
    public static readonly Guid ExponentialRetry = new(g: ExponentialRetryString);
}
