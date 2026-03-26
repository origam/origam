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
        System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType
    );
    private const string WQ_EVENT_ONCREATE = "fe40902f-8a44-477e-96f9-d157eee16a0f";
    private readonly core.ICoreDataService dataService = core.DataService.Instance;
    private CancellationTokenSource cancellationTokenSource = new();
    private readonly WorkQueueUtils workQueueUtils;
    private readonly IWorkQueueProcessor queueProcessor;
    private readonly WorkQueueThrottle workQueueThrottle;
    private readonly Timer loadExternalWorkQueuesTimer = new(60_000);
    private readonly Timer queueAutoProcessTimer;
    private bool serviceBeingUnloaded = false;
    private bool externalQueueAdapterBusy = false;
    private bool queueAutoProcessBusy = false;
    private readonly RetryManager retryManager = new();
    private static readonly Guid DS_METHOD_WQ_GETACTIVEQUEUES = new(
        "0b45c721-65d2-4305-b34a-cd0d07387ea1"
    );
    private static readonly Guid DS_METHOD_WQ_GETACTIVEQUEUESBYPROCESSOR = new(
        "b1f1abcd-c8bc-4680-8f21-06a68e8305f0"
    );
    private static readonly Guid DS_WORKQUEUE = new("7b44a488-ac98-4fe1-a427-55de0ff9e12e");
    private static readonly Guid DS_SORTSET_WQ_SORT = new("c1ec9d9e-09a2-47ad-b5e4-b57107c4dc34");

    public WorkQueueService()
        : this(10_000) { }

    public WorkQueueService(int queueProcessIntervalMillis)
    {
        queueAutoProcessTimer = new Timer(queueProcessIntervalMillis);
        var schemaService = ServiceManager.Services.GetService<SchemaService>();
        var dataLookupService = ServiceManager.Services.GetService<IDataLookupService>();
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        workQueueThrottle = new WorkQueueThrottle(persistenceService);
        workQueueUtils = new WorkQueueUtils(dataLookupService, schemaService);
        schemaService.SchemaLoaded += schemaService_SchemaLoaded;
        schemaService.SchemaUnloaded += schemaService_SchemaUnloaded;
        schemaService.SchemaUnloading += schemaService_SchemaUnloading;

        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        queueProcessor = settings.WorkQueueProcessingMode switch
        {
            WorkQueueProcessingMode.Linear => new LinearProcessor(
                ProcessQueueItem,
                workQueueUtils,
                retryManager,
                workQueueThrottle
            ),
            WorkQueueProcessingMode.RoundRobin => new RoundRobinLinearProcessor(
                ProcessQueueItem,
                workQueueUtils,
                retryManager,
                workQueueThrottle,
                settings.RoundRobinBatchSize
            ),
            _ => throw new NotImplementedException(
                $"Option {settings.WorkQueueProcessingMode} not implemented"
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
            log.DebugFormat("Stopping WorkQueueService Timers");
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
                log.Info("Unloading service - waiting for queues to finish.");
            }
            Thread.Sleep(1000);
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
            dataStructureId: new Guid("3a23f4e1-368c-4163-a790-4eed173af83d"),
            methodId: new Guid("ed3d93ca-bd4e-4830-8d26-f7120c8fc7ff"),
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null
        );
        // filter out those current user has no access to
        var rowsToDelete = new List<DataRow>();
        foreach (DataRow row in result.Tables["WorkQueue"].Rows)
        {
            IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
            if (
                row.IsNull("Roles")
                || !auth.Authorize(SecurityManager.CurrentPrincipal, (string)row["Roles"])
            )
            {
                rowsToDelete.Add(row);
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
        return workQueueUtils.WorkQueueClass(name);
    }

    public ISchemaItem WQClass(Guid queueId)
    {
        return workQueueUtils.WorkQueueClass(queueId);
    }

    public DataSet LoadWorkQueueData(string workQueueClass, object queueId)
    {
        return workQueueUtils.LoadWorkQueueData(
            workQueueClass,
            queueId,
            pageSize: 0,
            pageNumber: 0,
            transactionId: null
        );
    }

    public Guid WorkQueueAdd(string workQueueName, IXmlContainer data, string transactionId)
    {
        Guid workQueueId = workQueueUtils.GetQueueId(workQueueName);
        string workQueueClass = workQueueUtils.WorkQueueClassName(workQueueId);
        string condition = "";
        return WorkQueueAdd(
            workQueueClass,
            workQueueName,
            workQueueId,
            condition,
            data,
            attachments: null,
            transactionId
        );
    }

    public Guid WorkQueueAdd(
        string workQueueName,
        IXmlContainer data,
        WorkQueueAttachment[] attachments,
        string transactionId
    )
    {
        Guid workQueueId = workQueueUtils.GetQueueId(workQueueName);
        string workQueueClass = workQueueUtils.WorkQueueClassName(workQueueId);
        string condition = "";
        return WorkQueueAdd(
            workQueueClass,
            workQueueName,
            workQueueId,
            condition,
            data,
            attachments,
            transactionId
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
            workQueueClassIdentifier,
            workQueueName,
            workQueueId,
            condition,
            data,
            attachments: null,
            transactionId
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
            log.Debug($"Adding Work Queue Entry for Queue: {workQueueName}");
        }
        RuleEngine ruleEngine = RuleEngine.Create(new Hashtable(), transactionId);
        UserProfile profile = SecurityManager.CurrentUserProfile();
        WorkQueueClass workQueueClass = workQueueUtils.WorkQueueClass(workQueueClassIdentifier);
        if (workQueueClass != null)
        {
            Guid rowId = Guid.NewGuid();
            DataSet dataSet = new DatasetGenerator(userDefinedParameters: true).CreateDataSet(
                workQueueClass.WorkQueueStructure
            );
            DataTable table = dataSet.Tables[0];
            DataRow row = table.NewRow();
            row["Id"] = rowId;
            row["refWorkQueueId"] = workQueueId;
            row["RecordCreated"] = DateTime.Now;
            row["RecordCreatedBy"] = profile.Id;
            WorkQueueRowFill(workQueueClass, ruleEngine, row, data);
            table.Rows.Add(row);
            if (!string.IsNullOrEmpty(condition))
            {
                if (!EvaluateWorkQueueCondition(row, condition, workQueueName, transactionId))
                {
                    return Guid.Empty;
                }
            }
            StoreQueueItems(workQueueClass, table, transactionId);
            // add attachments
            if (attachments != null)
            {
                var attachmentService = ServiceManager.Services.GetService<AttachmentService>();
                foreach (WorkQueueAttachment workQueueAttachment in attachments)
                {
                    attachmentService.AddAttachment(
                        workQueueAttachment.Name,
                        workQueueAttachment.Data,
                        rowId,
                        profile.Id,
                        transactionId
                    );
                }
            }
            // notifications - OnCreate
            ProcessNotifications(
                workQueueClass,
                workQueueId,
                eventTypeId: new Guid(WQ_EVENT_ONCREATE),
                dataSet,
                transactionId
            );
            return (Guid)row["Id"];
        }
        return Guid.Empty;
    }

    public IDataDocument WorkQueueGetMessage(Guid workQueueMessageId, string transactionId)
    {
        WorkQueueClass workQueueClass = workQueueUtils.WorkQueueClassByMessageId(
            workQueueMessageId
        );
        DataSet dataSet = FetchSingleQueueEntry(workQueueClass, workQueueMessageId, transactionId);
        return DataDocumentFactory.New(dataSet);
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
        WorkQueueData workQueueData = GetQueue(workQueueId);
        foreach (
            WorkQueueData.WorkQueueNotificationRow notification in workQueueData
                .WorkQueueNotification
                .Rows
        )
        {
            if (log.IsDebugEnabled)
            {
                log.Debug($"Testing notification {notification?.Description}");
            }
            // check if the event type is equal (OnCreate, OnEscalate, etc...)
            if (!notification.refWorkQueueNotificationEventId.Equals(eventTypeId))
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("Wrong event type. Notification will not be sent.");
                }
                continue;
            }
            // notification source
            IXmlContainer notificationSource;
            if (workQueueClass.NotificationStructure == null)
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug("Notification source is work queue item.");
                }
                notificationSource = DataDocumentFactory.New(queueItem);
            }
            else
            {
                if (log.IsDebugEnabled)
                {
                    log.Debug(
                        $"Notification source is {workQueueClass.NotificationStructure.Path}"
                    );
                }
                DataSet dataSet = dataService.LoadData(
                    dataStructureId: workQueueClass.NotificationStructureId,
                    methodId: workQueueClass.NotificationLoadMethodId,
                    defaultSetId: Guid.Empty,
                    sortSetId: Guid.Empty,
                    transactionId: transactionId,
                    paramName1: workQueueClass.NotificationFilterPkParameter,
                    paramValue1: queueItem.Tables[0].Rows[0]["refId"]
                );
                notificationSource = DataDocumentFactory.New(dataSet);
                if (log.IsDebugEnabled)
                {
                    log.Debug($"Notification source result: {notificationSource?.Xml?.OuterXml}");
                }
            }
            DataRow workQueueRow = ExtractWorkQueueRowIfNotNotificationDataStructureIsSet(
                workQueueClass,
                queueItem
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
                    || senderData.OrigamNotificationContact[0].ContactIdentification == ""
                )
                {
                    if (log.IsErrorEnabled)
                    {
                        log.Error(
                            $"Skipping notification for work queue notification sender definition {sender?.Id}, no sender returned"
                        );
                    }
                    continue;
                }
                senders[sender.refOrigamNotificationChannelTypeId] = senderData
                    .OrigamNotificationContact[0]
                    .ContactIdentification;
            }
            // recipients
            WorkQueueData.WorkQueueNotificationContact_RecipientsRow[] recipientRows =
                notification.GetWorkQueueNotificationContact_RecipientsRows();
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    $"Number of recipients rows defined for work queue notification: {recipientRows.Length}"
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
                        log.Debug("Didn't get any response when trying to get recipients.");
                    }
                    continue;
                }
                if (log.IsDebugEnabled)
                {
                    log.Debug($"Recipients: {recipients?.GetXml()}");
                }
                if (!senders.Contains(recipientRow.refOrigamNotificationChannelTypeId))
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug(
                            $"Can't find any sender for notification channel `{recipientRow.refOrigamNotificationChannelTypeId}'"
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
                        notification.refOrigamNotificationTemplateId,
                        notificationSource,
                        recipient,
                        workQueueRow,
                        transactionId
                    );
                    // processing data for mail (output from transformation)
                    if (notificationData.DataSet.Tables[0].Rows.Count != 1)
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(
                                $"Notification transformation result count: {notificationData.DataSet.Tables[0].Rows.Count}"
                            );
                        }
                        continue;
                    }
                    DataRow notificationRow = notificationData.DataSet.Tables[0].Rows[0];
                    string notificationBody = null;
                    string notificationSubject = null;
                    if (!notificationRow.IsNull("Body"))
                    {
                        notificationBody = (string)notificationRow["Body"];
                    }
                    if (!notificationRow.IsNull("Subject"))
                    {
                        notificationSubject = (string)notificationRow["Subject"];
                    }
                    if (
                        string.IsNullOrEmpty(notificationBody)
                        || string.IsNullOrEmpty(notificationSubject)
                    )
                    {
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(
                                "Notification body or subject is empty. No notification will be sent."
                            );
                        }
                        continue;
                    }
                    if (log.IsDebugEnabled)
                    {
                        log.Debug($"Notification subject: {notificationSubject}");
                        log.Debug($"Notification body: '{notificationBody}'");
                    }
                    // send the notification - start the notification workflow
                    var queryParameterCollection = new QueryParameterCollection
                    {
                        new QueryParameter(
                            "sender",
                            (string)senders[recipientRow.refOrigamNotificationChannelTypeId]
                        ),
                        new QueryParameter("recipients", recipient.ContactIdentification),
                        new QueryParameter("body", notificationBody),
                        new QueryParameter("subject", notificationSubject),
                        new QueryParameter(
                            "notificationChannelTypeId",
                            recipientRow.refOrigamNotificationChannelTypeId
                        ),
                    };
                    if (notification.SendAttachments)
                    {
                        queryParameterCollection.Add(
                            new QueryParameter(
                                "attachmentRecordId",
                                (Guid)queueItem.Tables[0].Rows[0]["Id"]
                            )
                        );
                    }
                    core.WorkflowService.ExecuteWorkflow(
                        new Guid("0fea481a-24ab-4e98-8793-617ab5bb7272"),
                        queryParameterCollection,
                        transactionId
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
            workQueueRow = queueItem.Tables[0].Rows[0];
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
                "Recipient must be type OrigamNotificationContactData.OrigamNotificationContactRow."
            );
        }
        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        using var langSwitcher = new LanguageSwitcher(
            !recipient.IsLanguageTagIETFNull() ? recipient.LanguageTagIETF : ""
        );
        // get the current localized XSLT template
        DataSet templateData = dataService.LoadData(
            dataStructureId: new Guid("92c3c8b4-68a3-482b-8a90-f7142c4b17ec"), // OrigamNotificationTemplate DS
            methodId: new Guid("3724bd2a-9466-4129-bdfa-ca8dc8621a72"), // GetId
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId,
            paramName1: "OrigamNotificationTemplate_parId",
            notificationTemplateId
        );
        string template = (string)templateData.Tables[0].Rows[0]["Template"];
        // transform
        var resultStructure = persistence.SchemaProvider.RetrieveInstance<DataStructure>(
            new Guid("2f5e1853-e885-4177-ab6d-9da52123ae82")
        );
        IXsltEngine transform = new CompiledXsltEngine(persistence.SchemaProvider);
        var parameters = new Hashtable
        {
            ["RecipientRow"] = DatasetTools.GetRowXml(recipient, DataRowVersion.Default),
        };
        if (workQueueRow != null)
        {
            parameters["WorkQueueRow"] = DatasetTools.GetRowXml(
                workQueueRow,
                DataRowVersion.Default
            );
        }
        var notificationData = (IDataDocument)
            transform.Transform(
                notificationSource,
                template,
                parameters,
                transactionId,
                resultStructure,
                validateOnly: false
            );
        return notificationData;
    }

    public string CustomScreenName(Guid queueId)
    {
        return workQueueUtils.CustomScreenName(queueId);
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
                new Guid("3535c6f5-c48d-4ae9-ba21-43852d4f66f8")
            )
        )
        {
            // manual entry
            OrigamNotificationContactData.OrigamNotificationContactRow recipient =
                result.OrigamNotificationContact.NewOrigamNotificationContactRow();
            recipient.ContactIdentification = value;
            result.OrigamNotificationContact.AddOrigamNotificationContactRow(recipient);
        }
        else
        {
            // anything else - we execute the workflow to get the addresses
            var queryParameterCollection = new QueryParameterCollection
            {
                new QueryParameter(
                    "workQueueNotificationContactTypeId",
                    workQueueNotificationContactTypeId
                ),
                new QueryParameter(
                    "OrigamNotificationChannelTypeId",
                    origamNotificationChannelTypeId
                ),
                new QueryParameter("value", value),
                new QueryParameter("context", context),
            };
            if (workQueueRow != null)
            {
                queryParameterCollection.Add(
                    new QueryParameter(
                        "WorkQueueRow",
                        DatasetTools.GetRowXml(workQueueRow, DataRowVersion.Default)
                    )
                );
            }
            if (
                core.WorkflowService.ExecuteWorkflow(
                    new Guid("1e621daf-c70d-4cc1-9a52-73427c499006"),
                    queryParameterCollection,
                    transactionId
                )
                is IDataDocument wfResult
            )
            {
                DatasetTools.MergeDataSetVerbose(result, wfResult.DataSet);
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
            if (string.IsNullOrEmpty(entityMapping.XPath))
            {
                continue;
            }
            DataColumn dataColumn = row.Table.Columns[entityMapping.Name];
            OrigamDataType dataType = (OrigamDataType)
                dataColumn.ExtendedProperties["OrigamDataType"];
            object value = ruleEngine.EvaluateContext(
                entityMapping.XPath,
                data,
                dataType,
                workQueueClass.WorkQueueStructure
            );
            if (
                value is string sValue
                && (dataColumn.MaxLength > 0 & sValue.Length > dataColumn.MaxLength)
            )
            {
                // handle string length
                row[entityMapping.Name] = sValue.Substring(0, dataColumn.MaxLength - 4) + " ...";
            }
            else
            {
                row[entityMapping.Name] = value ?? DBNull.Value;
            }
        }
        // set refId to self if it was not mapped to a source row id, so e.g. notifications can
        // load the work queue entry data
        if (row.IsNull("refId"))
        {
            row["refId"] = row["Id"];
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
                "GetById",
                DataStructureMethod.CategoryConst
            )
            is not DataStructureMethod getOneEntryMethod
        )
        {
            throw new OrigamException(
                $"Programming Error: Can't find a filterset called `GetById' in DataStructure `{workQueueClass.WorkQueueStructure.Name}'. Please add the filterset to the DataStructure."
            );
        }
        // fetch entry by Id
        DataSet queueEntryDataSet = dataService.LoadData(
            workQueueClass.WorkQueueStructureId,
            getOneEntryMethod.Id,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId,
            paramName1: "WorkQueueEntry_parId",
            queueEntryId
        );
        if (queueEntryDataSet.Tables[0].Rows.Count == 1)
        {
            return queueEntryDataSet;
        }
        throw new RuleException(
            ResourceUtils.GetString("ErrorWorkQueueEntryNotFound"),
            RuleExceptionSeverity.High,
            "ErrorWorkQueueEntryNotFound",
            "WorkQueueEntry"
        );
    }

    public void WorkQueueRemove(Guid workQueueId, object queueEntryId, string transactionId)
    {
        if (queueEntryId == null)
        {
            return;
        }

        WorkQueueData queue = GetQueue(workQueueId);
        WorkQueueData.WorkQueueRow queueRow = queue.WorkQueue[0];
        if (log.IsDebugEnabled)
        {
            log.Debug($"Removing Work Queue Entries for Queue: {queueRow.Name}");
        }
        WorkQueueClass workQueueClass = workQueueUtils.WorkQueueClass(queueRow.WorkQueueClass);
        if (workQueueClass == null)
        {
            return;
        }
        DataSet queueEntryDataSet = FetchSingleQueueEntry(
            workQueueClass,
            queueEntryId,
            transactionId
        );
        queueEntryDataSet.Tables[0].Rows[0].Delete();
        try
        {
            dataService.StoreData(
                workQueueClass.WorkQueueStructure.Id,
                queueEntryDataSet,
                loadActualValuesAfterUpdate: false,
                transactionId
            );
        }
        catch (DBConcurrencyException)
        {
            dataService.StoreData(
                new Guid("7ca0c208-9ac8-4c55-bd0e-32575b613654"),
                queueEntryDataSet,
                loadActualValuesAfterUpdate: false,
                transactionId
            );
        }
        if (log.IsDebugEnabled)
        {
            log.Debug($"Removed Work Queue Entry `{queueEntryId}'  from Queue: {queueRow.Name}");
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
                $"Removing Work Queue Entries for Queue: {workQueueName} for row Id {rowKey}"
            );
        }
        WorkQueueClass workQueueClass = workQueueUtils.WorkQueueClass(workQueueClassIdentifier);
        if (workQueueClass == null)
        {
            return;
        }
        // get queue entries for this row and queue
        if (
            workQueueClass.WorkQueueStructure.GetChildByName(
                "GetByMasterId",
                DataStructureMethod.CategoryConst
            )
            is not DataStructureMethod primaryKeyMethod
        )
        {
            throw new Exception(
                $"GetByMasterId method not found for data structure of work queue class '{workQueueClass.Name}'"
            );
        }
        DataSet dataSet = dataService.LoadData(
            workQueueClass.WorkQueueStructureId,
            primaryKeyMethod.Id,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId,
            paramName1: "WorkQueueEntry_parRefId",
            paramValue1: rowKey,
            paramName2: "WorkQueueEntry_parWorkQueueId",
            paramValue2: workQueueId
        );
        int count = dataSet.Tables[0].Rows.Count;
        if (count > 0)
        {
            foreach (DataRow rowToDelete in dataSet.Tables[0].Rows)
            {
                bool delete = true;
                if (!string.IsNullOrEmpty(condition))
                {
                    if (
                        !EvaluateWorkQueueCondition(
                            rowToDelete,
                            condition,
                            workQueueName,
                            transactionId
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
                workQueueClass.WorkQueueStructure.Id,
                dataSet,
                loadActualValuesAfterUpdate: false,
                transactionId
            );
        }
        if (log.IsDebugEnabled)
        {
            log.Debug($"Removed {count} work Queue Entries from Queue: {workQueueName}");
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

        WorkQueueClass workQueueClass = workQueueUtils.WorkQueueClass(workQueueClassIdentifier);
        RuleEngine ruleEngine = RuleEngine.Create(new Hashtable(), transactionId);
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
                nameof(relationNo),
                relationNo,
                ResourceUtils.GetString("ErrorMaxWorkQueueEntities")
            );
        }
        filterSet = (DataStructureFilterSet)
            workQueueClass.WorkQueueStructure.GetChildByName(
                filterSetName,
                DataStructureFilterSet.CategoryConst
            );
        if (filterSet == null)
        {
            throw new Exception(
                ResourceUtils.GetString(
                    "ErrorNoFilterSet",
                    workQueueClass.WorkQueueStructure.Path,
                    filterSetName
                )
            );
        }
        // load all entries in the queue related to this entity
        DataSet entries = dataService.LoadData(
            workQueueClass.WorkQueueStructureId,
            filterSet.Id,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId,
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
                $"Entity Structure Primary Key Method '{workQueueClass.EntityStructurePrimaryKeyMethod.Path}' specified in a work queue class '{workQueueClass.Path}' has no parameters. A parameter to load a record by its primary key is expected."
            );
        }
        // for-each entry
        foreach (DataRow entry in entries.Tables[0].Rows)
        {
            // load original record
            DataSet originalRecord = dataService.LoadData(
                workQueueClass.EntityStructureId,
                workQueueClass.EntityStructurePkMethodId,
                defaultSetId: Guid.Empty,
                sortSetId: Guid.Empty,
                transactionId,
                primaryKeyParamName,
                entry["refId"]
            );
            // record could have been deleted in the meantime, we test
            if (originalRecord.Tables[0].Rows.Count <= 0)
            {
                continue;
            }
            IXmlContainer data = DatasetTools.GetRowXml(
                originalRecord.Tables[0].Rows[0],
                DataRowVersion.Default
            );
            // update entry from record
            WorkQueueRowFill(workQueueClass, ruleEngine, entry, data);
            entry["RecordUpdated"] = DateTime.Now;
            entry["RecordUpdatedBy"] = profile.Id;
        }
        // save entries
        StoreQueueItems(workQueueClass, entries.Tables[0], transactionId);
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
            WorkQueueData workQueueData = GetQueue(queueId);
            WorkQueueData.WorkQueueRow queue = workQueueData.WorkQueue[0];
            HandleAction(
                queue,
                workQueueClassIdentifier,
                selectedRows,
                commandType,
                command,
                param1,
                param2,
                lockItems: true,
                errorQueueId,
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
            throw new RuleException(ResourceUtils.GetString("ErrorNoRecordsSelectedForAction"));
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
                $"Begin HandleAction() queue class: {workQueueClassIdentifier} command: {command} lockItems: {lockItems}"
            );
        }
        // set all rows to be actual values, not added (in case the calling function did not do that)
        selectedRows.AcceptChanges();
        WorkQueueClass workQueueClass = workQueueUtils.WorkQueueClass(workQueueClassIdentifier);
        try
        {
            if (lockItems)
            {
                LockQueueItems(workQueueClass, selectedRows);
            }

            var parameterService = ServiceManager.Services.GetService<IParameterService>();
            if (
                commandType
                == (Guid)parameterService.GetParameterValue("WorkQueueCommandType_StateChange")
            )
            {
                HandleStateChange(
                    workQueueClassIdentifier,
                    selectedRows,
                    param1,
                    param2,
                    transactionId
                );
            }
            else if (
                commandType
                == (Guid)parameterService.GetParameterValue("WorkQueueCommandType_Remove")
            )
            {
                HandleRemove(workQueueClassIdentifier, selectedRows, transactionId);
            }
            else if (
                commandType == (Guid)parameterService.GetParameterValue("WorkQueueCommandType_Move")
            )
            {
                HandleMove(workQueueClassIdentifier, selectedRows, param1, transactionId, true);
            }
            else if (
                commandType
                == (Guid)
                    parameterService.GetParameterValue("WorkQueueCommandType_WorkQueueClassCommand")
            )
            {
                HandleWorkflow(
                    workQueueClassIdentifier,
                    selectedRows,
                    command,
                    param1,
                    param2,
                    transactionId
                );
            }
            else if (
                commandType
                == (Guid)parameterService.GetParameterValue("WorkQueueCommandType_Archive")
            )
            {
                HandleMove(workQueueClassIdentifier, selectedRows, param1, transactionId, false);
            }
            else if (
                commandType
                == (Guid)
                    parameterService.GetParameterValue(
                        "WorkQueueCommandType_LoadFromExternalSource"
                    )
            )
            {
                LoadFromExternalSource(queue.Id);
            }
            else if (
                commandType
                == (Guid)parameterService.GetParameterValue("WorkQueueCommandType_RunNotifications")
            )
            {
                HandleRunNotifications(workQueueClassIdentifier, selectedRows, transactionId);
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    nameof(commandType),
                    commandType,
                    ResourceUtils.GetString("ErrorUnknownWorkQueueCommand")
                );
            }
            if (lockItems)
            {
                UnlockQueueItems(selectedRows);
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
                    $"Error occured while processing work queue items., Queue: {workQueueClass?.Name}, Command: {command}",
                    ex
                );
            }
            if (transactionId != null)
            {
                ResourceMonitor.Rollback(transactionId);
            }
            // rollback deletion of the row when it fails - we have to work with previous state
            bool anyRowInRetry = false;
            foreach (DataRow row in selectedRows.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                {
                    row.RejectChanges();
                }
                retryManager.SetEntryRetryData(row, queue, ex.Message);
                anyRowInRetry = anyRowInRetry || (bool)row["InRetry"];
            }
            StoreFailedEntries(workQueueClass, selectedRows);
            // other failure => move to the error queue, if available
            if (!anyRowInRetry && errorQueueId != null)
            {
                HandleMoveQueue(
                    workQueueClass,
                    selectedRows,
                    (Guid)errorQueueId,
                    ex.Message,
                    transactionId: null,
                    resetErrors: false
                );
            }
            // unlock the queue item
            if (lockItems)
            {
                UnlockQueueItems(selectedRows, rejectChangesWhenDeleted: true);
            }

            throw;
        }
        if (log.IsInfoEnabled)
        {
            log.Info(
                $"Finished HandleAction() queue class: {workQueueClassIdentifier} command: {command}"
            );
        }
    }

    public void HandleAction(string workQueueCode, string commandText, Guid queueEntryId)
    {
        Guid queueId = workQueueUtils.GetQueueId(workQueueCode);
        // get all queue data from database (no entries)
        WorkQueueData queue = GetQueue(queueId);
        // extract WorkQueueClass name and construct WorkQueueClass from name
        WorkQueueData.WorkQueueRow queueRow = queue.WorkQueue[0];
        var workQueueClass = (WorkQueueClass)WQClass(queueRow.WorkQueueClass);
        // authorize access from API
        IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
        if (
            queueRow.IsApiAccessRolesNull()
            || !auth.Authorize(SecurityManager.CurrentPrincipal, queueRow.ApiAccessRoles)
        )
        {
            throw new RuleException(
                string.Format(ResourceUtils.GetString("ErrorWorkQueueApiNotAuthorized"), queueId),
                RuleExceptionSeverity.High,
                "queueId",
                ""
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
            || !auth.Authorize(SecurityManager.CurrentPrincipal, commandRow.Roles)
        )
        {
            throw new RuleException(
                string.Format(
                    ResourceUtils.GetString("ErrorWorkQueueCommandNotAuthorized"),
                    commandText,
                    queueId
                ),
                RuleExceptionSeverity.High,
                "commandId",
                ""
            );
        }
        // fetch a single queue entry
        DataSet queueEntryDataSet = FetchSingleQueueEntry(
            workQueueClass,
            queueEntryId,
            transactionId: null
        );
        // call handle action
        HandleAction(
            queueRow,
            queueRow.WorkQueueClass,
            queueEntryDataSet.Tables[0],
            commandRow.refWorkQueueCommandTypeId,
            commandRow.IsCommandNull() ? null : commandRow.Command,
            commandRow.IsParam1Null() ? null : commandRow.Param1,
            commandRow.IsParam2Null() ? null : commandRow.Param2,
            lockItems: true,
            commandRow.IsrefErrorWorkQueueIdNull() ? null : commandRow.refErrorWorkQueueId,
            transactionId: null
        );
    }

    public DataRow GetNextItem(string workQueueName, string transactionId, bool processErrors)
    {
        Guid queueId = workQueueUtils.GetQueueId(workQueueName);
        WorkQueueData.WorkQueueRow queue = GetQueue(queueId).WorkQueue[0];
        return queueProcessor.GetNextItem(
            queue,
            transactionId,
            processErrors,
            cancellationTokenSource.Token
        );
    }

    private void LockQueueItems(WorkQueueClass workQueueClass, DataTable selectedRows)
    {
        if (!workQueueUtils.LockQueueItems(workQueueClass, selectedRows))
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
                Guid id = (Guid)row["Id"];
                if (log.IsDebugEnabled)
                {
                    log.Debug($"Unlocking work queue item id {id}");
                }
                // load the fresh queue entry from the database, because
                // it may have been e.g. deleted after state change (in that case
                // nothing will happen/nothing will be unlocked)
                DataSet data = dataService.LoadData(
                    dataStructureId: new Guid("59de7db2-e2f4-437b-b191-0fd3bc766685"),
                    methodId: new Guid("a68a8990-f476-4a64-bb9a-e45228eb9aae"),
                    defaultSetId: Guid.Empty,
                    sortSetId: Guid.Empty,
                    transactionId: null,
                    paramName1: "WorkQueueEntry_parId",
                    id
                );
                foreach (DataRow freshRow in data.Tables[0].Rows)
                {
                    freshRow["IsLocked"] = false;
                    freshRow["refLockedByBusinessPartnerId"] = DBNull.Value;
                }
                dataService.StoreData(
                    new Guid("59de7db2-e2f4-437b-b191-0fd3bc766685"),
                    data,
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
                log.RunHandled(() =>
                {
                    Guid itemId = (Guid)row["Id"];
                    log.Info(
                        $"Moving queue item {itemId} to queue id {newQueueId}{(errorMessage == null ? "" : " with error: " + errorMessage)}"
                    );
                });
            }
            row["refWorkQueueId"] = newQueueId;
            retryManager.ResetEntry(row);
            if (resetErrors || errorMessage == null)
            {
                row["ErrorText"] = DBNull.Value;
            }
            else
            {
                row["ErrorText"] = errorMessage;
            }
        }
        try
        {
            StoreQueueItems(workQueueClass, selectedRows, transactionId);
        }
        catch (DBConcurrencyException)
        {
            dataService.StoreData(
                new Guid("7ca0c208-9ac8-4c55-bd0e-32575b613654"),
                selectedRows.DataSet,
                loadActualValuesAfterUpdate: false,
                transactionId
            );
        }
        foreach (DataRow row in selectedRows.Rows)
        {
            DataSet slice = DatasetTools.CloneDataSet(selectedRows.DataSet);
            DatasetTools.GetDataSlice(slice, new List<DataRow> { row });
            if (log.IsInfoEnabled)
            {
                log.RunHandled(() =>
                {
                    Guid itemId = (Guid)row["Id"];
                    log.Info($"Running notifications for item {itemId}.");
                });
            }
            ProcessNotifications(
                workQueueClass,
                newQueueId,
                new Guid(WQ_EVENT_ONCREATE),
                slice,
                transactionId
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
        CheckSelectedRowsCountPositive(selectedRows.Rows.Count);
        var workQueueClass = (WorkQueueClass)WQClass(workQueueClassIdentifier);
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
                workQueueClass.EntityStructureId,
                workQueueClass.EntityStructurePkMethodId,
                defaultSetId: Guid.Empty,
                sortSetId: Guid.Empty,
                transactionId,
                primaryKeyParamName,
                row["refId"]
            );
            if (dataSet.Tables[0].Rows.Count == 0)
            {
                throw new Exception(ResourceUtils.GetString("ErrorNoRecords"));
            }
            if (!dataSet.Tables[0].Columns.Contains(fieldName))
            {
                throw new ArgumentOutOfRangeException(
                    "fieldName",
                    fieldName,
                    ResourceUtils.GetString("ErrorSourceFieldNotFound")
                );
            }
            Type dataType = dataSet.Tables[0].Columns[fieldName].DataType;
            object value;
            if (dataType == typeof(string))
            {
                value = newValue;
            }
            else if (dataType == typeof(Guid))
            {
                value = new Guid(newValue);
            }
            else if (dataType == typeof(int))
            {
                value = XmlConvert.ToInt32(newValue);
            }
            else
            {
                throw new Exception(
                    ResourceUtils.GetString("ErrorConvertToType", dataType.ToString())
                );
            }
            dataSet.Tables[0].Rows[0][fieldName] = value;
            dataService.StoreData(
                workQueueClass.EntityStructureId,
                dataSet,
                loadActualValuesAfterUpdate: false,
                transactionId
            );
        }
    }

    private void HandleRunNotifications(
        string workQueueClassIdentifier,
        DataTable selectedRows,
        string transactionId
    )
    {
        CheckSelectedRowsCountPositive(selectedRows.Rows.Count);
        var workQueueClass = (WorkQueueClass)WQClass(workQueueClassIdentifier);
        var parameterService = ServiceManager.Services.GetService<IParameterService>();
        foreach (DataRow row in selectedRows.Rows)
        {
            DataSet slice = DatasetTools.CloneDataSet(selectedRows.DataSet);
            DatasetTools.GetDataSlice(slice, new List<DataRow> { row });
            if (log.IsInfoEnabled)
            {
                log.RunHandled(() =>
                {
                    Guid itemId = (Guid)row["Id"];
                    log.Info($"Running notifications for item {itemId}.");
                });
            }
            ProcessNotifications(
                workQueueClass,
                workQueueId: (Guid)row["refWorkQueueId"],
                eventTypeId: (Guid)
                    parameterService.GetParameterValue("WorkQueueNotificationEvent_Command"),
                slice,
                transactionId
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
        CheckSelectedRowsCountPositive(selectedRows.Rows.Count);
        var workQueueClass = (WorkQueueClass)WQClass(workQueueClassIdentifier);
        WorkQueueWorkflowCommand cmd = workQueueClass.GetCommand(command);
        var parameters = new QueryParameterCollection();
        foreach (WorkQueueWorkflowCommandParameterMapping parameterMapping in cmd.ParameterMappings)
        {
            object val = parameterMapping.Value switch
            {
                WorkQueueCommandParameterMappingType.QueueEntries => GetDataDocumentFactory(
                    selectedRows.DataSet
                ),
                WorkQueueCommandParameterMappingType.Parameter1 => param1,
                WorkQueueCommandParameterMappingType.Parameter2 => param2,
                _ => throw new ArgumentOutOfRangeException(
                    "Value",
                    parameterMapping.Value,
                    ResourceUtils.GetString("ErrorUnknownWorkQueueCommandValue")
                ),
            };
            parameters.Add(new QueryParameter(parameterMapping.Name, val));
        }
        core.WorkflowService.ExecuteWorkflow(cmd.WorkflowId, parameters, transactionId);
    }

    private object GetDataDocumentFactory(DataSet dataSet)
    {
        var val = dataSet.ExtendedProperties["IDataDocument"] ?? DataDocumentFactory.New(dataSet);
        if (dataSet.ExtendedProperties["IDataDocument"] == null)
        {
            dataSet.ExtendedProperties.Add("IDataDocument", val);
        }
        return val;
    }

    private void HandleRemove(
        string workQueueClassIdentifier,
        DataTable selectedRows,
        string transactionId
    )
    {
        CheckSelectedRowsCountPositive(selectedRows.Rows.Count);
        if (log.IsInfoEnabled)
        {
            log.Info($"Begin HandleRemove() queue class: {workQueueClassIdentifier}");
        }
        var workQueueClass = (WorkQueueClass)WQClass(workQueueClassIdentifier);
        foreach (DataRow row in selectedRows.Rows)
        {
            row.Delete();
        }
        try
        {
            StoreQueueItems(workQueueClass, selectedRows, transactionId);
        }
        catch (DBConcurrencyException)
        {
            dataService.StoreData(
                new Guid("fb5d8abe-99b8-4ca0-871a-c8c6e3ae6b76"),
                selectedRows.DataSet,
                loadActualValuesAfterUpdate: false,
                transactionId
            );
        }
        if (log.IsInfoEnabled)
        {
            log.Info($"Finished HandleRemove() queue class: {workQueueClassIdentifier}");
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
        CheckSelectedRowsCountPositive(selectedRows.Rows.Count);
        if (log.IsInfoEnabled)
        {
            log.Info($"Begin HandleMove() queue class: {workQueueClassIdentifier}");
        }
        Guid newQueueId = workQueueUtils.GetQueueId(newQueueReferenceCode);
        var workQueueClass = (WorkQueueClass)WQClass(workQueueClassIdentifier);
        HandleMoveQueue(
            workQueueClass,
            selectedRows,
            newQueueId,
            errorMessage: null,
            transactionId,
            resetErrors
        );
        if (log.IsInfoEnabled)
        {
            log.Info($"Finished HandleMove() queue class: {workQueueClassIdentifier}");
        }
    }

    private void StoreQueueItems(
        WorkQueueClass workQueueClass,
        DataTable selectedRows,
        string transactionId
    )
    {
        dataService.StoreData(
            workQueueClass.WorkQueueStructureId,
            selectedRows.DataSet,
            loadActualValuesAfterUpdate: true,
            transactionId
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
            dataService.LoadData(
                DS_WORKQUEUE,
                filterMethodGuid,
                defaultSetId: Guid.Empty,
                DS_SORTSET_WQ_SORT,
                transactionId,
                paramName1: "WorkQueue_parQueueProcessor",
                settings.Name
            )
        );
        return queues;
    }

    private WorkQueueData GetQueue(Guid queueId)
    {
        WorkQueueData queues = new WorkQueueData();
        queues.Merge(
            dataService.LoadData(
                dataStructureId: new Guid("7b44a488-ac98-4fe1-a427-55de0ff9e12e"),
                methodId: new Guid("2543bbd3-3592-4b14-9d74-86d0e9c65d98"),
                defaultSetId: Guid.Empty,
                sortSetId: new Guid("c1ec9d9e-09a2-47ad-b5e4-b57107c4dc34"),
                transactionId: null,
                paramName1: "WorkQueue_parId",
                queueId
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
        LoadFromExternalSource(Guid.Empty);
    }

    private void LoadFromExternalSource(Guid queueId)
    {
        var schemaService = ServiceManager.Services.GetService<SchemaService>();
        if (externalQueueAdapterBusy || !schemaService.IsSchemaLoaded || serviceBeingUnloaded)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    $"Skipping external work queues load: adapterBusy: {externalQueueAdapterBusy}, schemaLoaded: {schemaService?.IsSchemaLoaded}, serviceBeingUnloaded: {serviceBeingUnloaded}"
                );
            }
            return;
        }
        SecurityManager.SetServerIdentity();
        externalQueueAdapterBusy = true;
        if (log.IsInfoEnabled)
        {
            log.Info("Starting loading external work queues.");
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
                    log.Info($"Starting loading external work queue {workQueueRow.Name}");
                }
                try
                {
                    ProcessExternalQueue(workQueueRow);
                    StoreQueues(queues);
                }
                catch (Exception ex)
                {
                    if (log.IsErrorEnabled)
                    {
                        log.LogOrigamError(
                            $"Failed loading external work queue {workQueueRow.Name}",
                            ex
                        );
                    }
                    workQueueRow.ExternalSourceLastMessage = ex.Message;
                    workQueueRow.ExternalSourceLastTime = DateTime.Now;
                    StoreQueues(queues);
                }
            }
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError("External queue load failed.", ex);
            }
        }
        finally
        {
            externalQueueAdapterBusy = false;
        }
        if (log.IsInfoEnabled)
        {
            log.Info("Finished loading external work queues.");
        }
    }

    private void StoreQueues(WorkQueueData queues)
    {
        dataService.StoreData(
            new Guid("7b44a488-ac98-4fe1-a427-55de0ff9e12e"),
            queues,
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
        log.Info($"Running ProcessQueueItem in Thread: {Thread.CurrentThread.ManagedThreadId}");

        string itemId = queueEntryRow["Id"].ToString();
        string transactionId = Guid.NewGuid().ToString();
        try
        {
            foreach (WorkQueueData.WorkQueueCommandRow cmd in queue.GetWorkQueueCommandRows())
            {
                try
                {
                    if (!IsAutoProcessed(cmd, queue, queueEntryRow, transactionId))
                    {
                        continue;
                    }
                    if (log.IsInfoEnabled)
                    {
                        log.Info(
                            $"Auto processing work queue item. Id: {itemId}, Queue: {queue.Name}, Command: {cmd?.Text}"
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
                        queue,
                        queue.WorkQueueClass,
                        queueEntryRow.Table,
                        cmd.refWorkQueueCommandTypeId,
                        command,
                        param1,
                        param2,
                        lockItems: false,
                        errorQueueId,
                        transactionId
                    );
                    if (log.IsInfoEnabled)
                    {
                        log.Info(
                            $"Finished auto processing work queue item. Id: {itemId}, Queue: {queue.Name}, Command: {cmd.Text}"
                        );
                    }
                    if (
                        cmd.refWorkQueueCommandTypeId
                        == (Guid)parameterService.GetParameterValue("WorkQueueCommandType_Remove")
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
                        ResourceMonitor.Rollback(transactionId);
                    }
                    if (queueEntryRow.RowState == DataRowState.Deleted)
                    {
                        queueEntryRow.RejectChanges();
                    }
                    // unlock the queue item
                    UnlockQueueItems(queueEntryRow.Table, rejectChangesWhenDeleted: true);
                    // do not process any other commands on this queue entry
                    throw;
                }
            }
            // commit the transaction
            ResourceMonitor.Commit(transactionId);
            // unlock the queue item
            UnlockQueueItems(queueEntryRow.Table);
        }
        // Catch the command exception. Transaction is already rolled back,
        // so we continue to another queue item.
        // RuleException is logged on the debug level, others on the error level
        catch (RuleException ex) when (log.IsDebugEnabled)
        {
            log.Debug($"Queue item processing failed. Id: {itemId}, Queue: {queue?.Name}", ex);
        }
        catch (Exception ex) when (log.IsErrorEnabled)
        {
            log.Error($"Queue item processing failed. Id: {itemId}, Queue: {queue?.Name}", ex);
        }
        finally
        {
            workQueueThrottle.ReportProcessed(queue);
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
            !(bool)dataRow["InRetry"]
            && !workQueueCommandRow.IsAutoProcessedWithErrors
            && !dataRow.IsNull("ErrorText")
            && (string)dataRow["ErrorText"] != ""
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
                dataRow,
                workQueueCommandRow.AutoProcessingConditionXPath,
                workQueueRow.Name,
                transactionId
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
                $"Checking condition for work queue item. Id: {queueRow["Id"]}, Queue: {queueName}, Condition: {condition}"
            );
        }
        try
        {
            // we have to do it from the copied dataset, because later the XmlDataDocument would be re-created, which
            // is not supported by the .net
            IDataDocument oneRowXml = DataDocumentFactory.New(queueRow.Table.DataSet.Copy());
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
                    $"Condition for work queue item. Id: {queueRow["Id"]}, Queue: {queueName}, Condition: {condition} evaluated to {evaluationResult}"
                );
            }
            try
            {
                if (XmlConvert.ToBoolean(evaluationResult))
                {
                    return true;
                }
            }
            catch
            {
                throw new Exception(
                    "Work queue condition did not return boolean value. Command will not be processed."
                );
            }
        }
        catch (Exception ex)
        {
            throw new Exception(
                "Work queue condition evaluation failed. Command will not be processed.",
                ex
            );
        }
        return false;
    }

    private void StoreFailedEntries(WorkQueueClass wqc, DataTable queueEntryTable)
    {
        try
        {
            StoreQueueItems(wqc, queueEntryTable, transactionId: null);
        }
        catch (DBConcurrencyException)
        {
            var dataStructureQuery = new DataStructureQuery
            {
                DataSourceId = new Guid("7a18149a-2faa-471b-a43e-9533d7321b44"),
                MethodId = new Guid("ea139b9a-3048-4cd5-bf9a-04a91590624a"),
                LoadActualValuesAfterUpdate = false,
            };
            dataService.StoreData(dataStructureQuery, queueEntryTable.DataSet, transactionId: null);
        }
    }

    private void ProcessExternalQueue(WorkQueueData.WorkQueueRow workQueueRow)
    {
        string transactionId = Guid.NewGuid().ToString();
        WorkQueueLoaderAdapter adapter = null;
        if (log.IsInfoEnabled)
        {
            log.Info($"Loading external work queue: {workQueueRow?.Name}");
        }
        try
        {
            adapter = WorkQueueAdapterFactory.GetAdapter(
                workQueueRow.refWorkQueueExternalSourceTypeId.ToString()
            );
            if (adapter == null)
            {
                throw new Exception(
                    $"External Source Adapter not found for queue {workQueueRow.Name}"
                );
            }
            adapter.Connect(
                service: this,
                workQueueRow.Id,
                workQueueRow.WorkQueueClass,
                connection: workQueueRow.IsExternalSourceConnectionNull()
                    ? null
                    : workQueueRow.ExternalSourceConnection,
                userName: workQueueRow.IsExternalSourceUserNameNull()
                    ? null
                    : workQueueRow.ExternalSourceUserName,
                password: workQueueRow.IsExternalSourcePasswordNull()
                    ? null
                    : workQueueRow.ExternalSourcePassword,
                transactionId
            );
            int itemCount = 0;
            string lastState = null;
            if (!workQueueRow.IsExternalSourceStateNull())
            {
                lastState = workQueueRow.ExternalSourceState;
            }
            // get first item
            WorkQueueAdapterResult queueItem = adapter.GetItem(lastState);
            while (queueItem != null)
            {
                itemCount++;
                lastState = queueItem.State;
                string creationCondition = (
                    workQueueRow.IsCreationConditionNull() ? null : workQueueRow.CreationCondition
                );
                // put it to the queue
                using (new TransactionScope(TransactionScopeOption.Suppress))
                {
                    WorkQueueAdd(
                        workQueueRow.WorkQueueClass,
                        workQueueRow.Name,
                        workQueueRow.Id,
                        creationCondition,
                        queueItem.Document,
                        queueItem.Attachments,
                        transactionId
                    );
                }
                // next
                if (!serviceBeingUnloaded)
                {
                    queueItem = adapter.GetItem(lastState);
                }
                else
                {
                    if (log.IsDebugEnabled)
                    {
                        log.Debug("Service is being unloaded. Stopping retrieval.");
                    }
                    queueItem = null;
                }
            }
            ResourceMonitor.Commit(transactionId);
            adapter.Disconnect();
            workQueueRow.ExternalSourceLastMessage = ResourceUtils.GetString(
                "OKMessage",
                itemCount.ToString()
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
            ResourceMonitor.Rollback(transactionId);
            if (adapter != null)
            {
                adapter.Disconnect();
            }
            workQueueRow.ExternalSourceLastMessage = ResourceUtils.GetString(
                "ErrorMessage",
                ex.Message
            );
            workQueueRow.ExternalSourceLastTime = DateTime.Now;
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError($"Failed to load queue {workQueueRow.Name}", ex);
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
            throw new InvalidOperationException("schemaService is not loaded");
        }
        return GetQueues(activeOnly: false)
            .WorkQueue.Rows.Cast<WorkQueueData.WorkQueueRow>()
            .Where(row => row.ReferenceCode == queueRefCode)
            .Where(HasAutoCommand)
            .FirstOrDefault();
    }

    public TaskRunner GetAutoProcessorForQueue(
        string queueRefCode,
        int parallelism,
        int forceWaitMs
    )
    {
        var queueToProcess = FindQueue(queueRefCode);

        if (queueToProcess == null)
        {
            throw new ArgumentException(
                $"Queue with code: {queueRefCode} does not exist, is empty or does not have autoprocess set."
            );
        }

        Action<CancellationToken> actionToRunInEachTask = cancellationToken =>
        {
            while (true)
            {
                try
                {
                    queueProcessor.ProcessAutoQueueCommands(
                        queueToProcess,
                        cancellationToken,
                        forceWaitMs
                    );
                }
                catch (OrigamException ex)
                {
                    if (IsDeadlock(ex))
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
                    $"Worker in thread {Thread.CurrentThread.ManagedThreadId} "
                        + $"cannot get next Item from the queue, sleeping for "
                        + $"{millisToSleep} ms before trying again..."
                );
                Thread.Sleep(millisToSleep);
            }
        };

        return new TaskRunner(actionToRunInEachTask, parallelism);
    }

    private static void HandleDeadLock()
    {
        var randomGenerator = new Random();
        int millisToSleep = randomGenerator.Next(1000, 2000);
        log.Warn(
            $"Deadlock was caught! Pausing thread {Thread.CurrentThread.ManagedThreadId} for {millisToSleep} ms"
        );
        Thread.Sleep(millisToSleep);
    }

    private static bool IsDeadlock(OrigamException ex)
    {
        return ex.InnerException is SqlException
            && ex.InnerException.Message.Contains("deadlocked");
    }

    private void WorkQueueAutoProcessTimerElapsed(object sender, ElapsedEventArgs e)
    {
        var schemaService = ServiceManager.Services.GetService<SchemaService>();
        if (queueAutoProcessBusy || !schemaService.IsSchemaLoaded || serviceBeingUnloaded)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(
                    $"Skipping auto processing work queues: queueAutoProcessBusy: {queueAutoProcessBusy}, schemaLoaded: {schemaService.IsSchemaLoaded}, serviceBeingUnloaded: {serviceBeingUnloaded}"
                );
            }
            return;
        }
        SecurityManager.SetServerIdentity();
        queueAutoProcessBusy = true;
        if (log.IsInfoEnabled)
        {
            log.Info("Starting auto processing work queues.");
        }

        try
        {
            IEnumerable<WorkQueueData.WorkQueueRow> queues = GetQueues()
                .WorkQueue.Rows.Cast<WorkQueueData.WorkQueueRow>()
                .Where(HasAutoCommand);
            queueProcessor.Run(queues, cancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(
                    "Unexpected error occured while autoprocessing work queues.",
                    ex
                );
            }
        }
        finally
        {
            queueAutoProcessBusy = false;
            if (log.IsInfoEnabled)
            {
                log.Info("Finished auto processing work queues.");
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
                    $"LoadExternalWorkQueues Enabled. Interval: {settings.ExternalWorkQueueCheckPeriod}"
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
            log.Debug("schemaService_SchemaUnloading");
        }
        StopTasks();
    }

    private void schemaService_SchemaUnloaded(object sender, EventArgs e)
    {
        if (log.IsDebugEnabled)
        {
            log.Debug("schemaService_SchemaUnloaded");
        }
    }
}

public static class WorkQueueRetryType
{
    private const string NoRetryString = "69460BCF-81D4-4A97-94F7-5A391D16F771";
    private const string LinearRetryString = "8A5C793F-73B8-41EF-A459-618A8E6FE4FA";
    private const string ExponentialRetryString = "57AD4C10-1F43-4CCF-A48A-132E7E418D53";

    public static readonly Guid NoRetry = new(NoRetryString);
    public static readonly Guid LinearRetry = new(LinearRetryString);
    public static readonly Guid ExponentialRetry = new(ExponentialRetryString);
}
