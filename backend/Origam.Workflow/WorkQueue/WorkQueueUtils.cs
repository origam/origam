using System;
using System.Collections.Generic;
using System.Data;
using Origam.DA;
using Origam.Schema;
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

    public WorkQueueUtils(IDataLookupService lookupService )
    {
        this.lookupService = lookupService;
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
    
    public WorkQueueClass WQClass(string name)
    {
        SchemaService s = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;

        foreach (WorkQueueClass c in s.GetProvider(typeof(WorkQueueClassSchemaItemProvider)).ChildItems)
        {
            if (c.Name == name) return c;
        }

#if ORIGAM_CLIENT
        throw new ArgumentOutOfRangeException("name", name, "Work Queue Class not defined. Check Work Queue setup.");
#else
            return null;
#endif
    }
    
    public string WQClassName(Guid queueId)
    {
        IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;

        return (string)ls.GetDisplayText(new Guid("46976056-f906-47ae-95e7-83d8c65412a3"), queueId, false, false, null);
    }
    
    public WorkQueueClass WQClass(Guid queueId)
    {
        return WQClass(WQClassName(queueId));
    }
    
    public DataSet LoadWorkQueueData(string workQueueClass, object queueId,
        int pageSize, int pageNumber, string transactionId)
    {
        WorkQueueClass wqc = WQClass(workQueueClass);
        if (wqc == null)
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
        return core.DataService.Instance.LoadData(wqc.WorkQueueStructureId,
            wqc.WorkQueueStructureUserListMethodId, Guid.Empty,
            wqc.WorkQueueStructureSortSetId, transactionId, parameters);
    }
    
    public bool LockQueueItems(WorkQueueClass wqc,
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
            core.DataService.Instance.StoreData(wqc.WorkQueueStructureId,
                selectedRows.DataSet, true, null);
        }
        catch
        {
            return false;
        }
        return true;
    }
}