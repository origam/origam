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
using Origam.Schema.WorkflowModel;
using Origam.Schema.WorkflowModel.WorkQueue;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.Workflow.WorkQueue;

public class WorkQueueUtils
{
    private static readonly log4net.ILog log = log4net.LogManager
        .GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private readonly IDataLookupService lookupService;
    private readonly SchemaService schemaService;

    public WorkQueueUtils(IDataLookupService lookupService, SchemaService schemaService)
    {
        this.lookupService = lookupService;
        this.schemaService = schemaService;
    }

    public Guid GetQueueId(string referenceCode)
    {
        object id = lookupService.GetDisplayText(new Guid("930ae1c9-0267-4c8d-b637-6988745fd44c"), referenceCode, false, false, null);

        if (id == null)
        {
            throw new ArgumentOutOfRangeException(ResourceUtils.GetString("ErrorWorkQueueNotFoundByReferenceCode", referenceCode));
        }

        return (Guid)id;
    }

    public Guid GetQueueId(Guid commandId)
    {
        object id = lookupService.GetDisplayText(new Guid("2a1596d1-96ee-402d-b935-93e5484cd48e"), commandId, false, false, null);
        if (id == null)
        {
            throw new ArgumentOutOfRangeException("commandId", commandId, ResourceUtils.GetString("ErrorWorkQueueCommandNotFound", commandId));
        }
        return (Guid)id;
    }
    
    public WorkQueueClass WorkQueueClass(string name)
    {
        foreach (WorkQueueClass queueClass in schemaService.GetProvider(typeof(WorkQueueClassSchemaItemProvider)).ChildItems)
        {
            if (queueClass.Name == name) return queueClass;
        }

#if ORIGAM_CLIENT
        throw new ArgumentOutOfRangeException("name", name, "Work Queue Class not defined. Check Work Queue setup.");
#else
            return null;
#endif
    }
    
    public string WorkQueueClassName(Guid queueId)
    {
        return (string)lookupService.GetDisplayText(
            new Guid("46976056-f906-47ae-95e7-83d8c65412a3"), queueId, false, false, null);
    }
    
    public string CustomScreeName(Guid queueId)
    {
        return (string)lookupService.GetDisplayText(
            new Guid("9da3e167-3f3f-422d-b454-3fbea660d9cf"), queueId, false, false, null);
    }

    public string WorkQueueClassNameByMessageId(Guid queueMessageId)
    {
        return (string)lookupService.GetDisplayText(
            new Guid("0ec49729-0981-49d7-a8e6-2160d949234e"), queueMessageId, false, false, null);
    }
    
    public WorkQueueClass WorkQueueClass(Guid queueId)
    {
        return WorkQueueClass(WorkQueueClassName(queueId));
    }

    public WorkQueueClass WorkQueueClassByMessageId(Guid queueMessageId)
    {
        return WorkQueueClass(WorkQueueClassNameByMessageId(queueMessageId));
    }

    public DataSet LoadWorkQueueData(string workQueueClass, object queueId,
        int pageSize, int pageNumber, string transactionId)
    {
        WorkQueueClass queueClass = WorkQueueClass(workQueueClass);
        if (queueClass == null)
        {
            throw new ArgumentOutOfRangeException("workQueueClass",
                workQueueClass,
                "Work queue class not found in the current model.");
        }
        QueryParameterCollection parameters = new QueryParameterCollection();
        parameters.Add(new QueryParameter("WorkQueueEntry_parWorkQueueId", queueId));
        if (pageSize > 0)
        {
            parameters.Add(new QueryParameter("_pageSize", pageSize));
            parameters.Add(new QueryParameter("_pageNumber", pageNumber));
        }

        DataSet dataSet = core.DataService.Instance.LoadData(queueClass.WorkQueueStructureId,
            queueClass.WorkQueueStructureUserListMethodId, Guid.Empty,
            queueClass.WorkQueueStructureSortSetId, transactionId, parameters);
        
        CheckContainsRequiredColumns(
            new []{"AttemptCount", "LastAttemptTime", "NextAttemptTime"}, 
            dataSet.Tables["WorkQueueEntry"], 
            queueClass.WorkQueueStructureId);
        return dataSet;
    }

    private void CheckContainsRequiredColumns(string[] fieldNames,
        DataTable table, Guid workQueueStructureId)
    {
        foreach (string fieldName in fieldNames)
        {
            DataColumnCollection entryColumns = table.Columns;
            if (!entryColumns.Contains(fieldName))
            {
                throw new Exception($"Work queue data structure {workQueueStructureId} does not contain the required field {fieldName}");
            }
        }
    }

    public bool LockQueueItems(WorkQueueClass queueClass,
        DataTable selectedRows)
    {
        UserProfile profile = SecurityManager.CurrentUserProfile();
        foreach (DataRow row in selectedRows.Rows)
        {
            Guid id = (Guid)row["Id"];
            if (log.IsDebugEnabled)
            {
                log.Debug("Locking work queue item id " + id);
            }
            if ((bool)row["IsLocked"])
            {
                throw new WorkQueueItemLockedException();
            }
            row["IsLocked"] = true;
            row["refLockedByBusinessPartnerId"] = profile.Id;
        }
        try
        {
            core.DataService.Instance.StoreData(queueClass.WorkQueueStructureId,
                selectedRows.DataSet, true, null);
        }
        catch
        {
            return false;
        }
        return true;
    }
}