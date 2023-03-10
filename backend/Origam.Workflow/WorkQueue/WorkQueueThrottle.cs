using System;
using System.Data;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.Workflow.WorkQueue;

public class WorkQueueThrottle
{
    private readonly IPersistenceService persistenceService;

    public WorkQueueThrottle(IPersistenceService persistenceService)
    {
        this.persistenceService = persistenceService;
    }

    private static readonly Guid WorkQueueStateDataStructureId = Guid.Empty;
    public bool CanRunNow(WorkQueueData.WorkQueueRow queue)
    {
        if (Equals(queue["EnableThrottling"], false))
        {
            return true;
        }
        DataRow stateRow = GetThrottlingState(queue.Id);
        if (stateRow == null ||
            ((DateTime)stateRow["ThrottlingIntervalStart"]).AddSeconds(
                (int)queue["ThrottlingIntervalSeconds"]) < DateTime.Now)
        {
            StoreThrottlingState(queue.Id, DateTime.Now, 0);
            return (int)queue["ThrottlingItemsPerInterval"] > 0;
        }

        return (int)stateRow["ThrottlingItemsProcessed"] < (int)queue["ThrottlingItemsPerInterval"];
    }

    public void ReportProcessed(WorkQueueData.WorkQueueRow queue)
    {
        if (Equals(queue["EnableThrottling"], false))
        {
            return;
        }

        DataRow row = GetThrottlingState(queue.Id);
        if (row == null)
        {
            StoreThrottlingState(queue.Id, DateTime.Now, 1);
        }
        else
        {
            row["ThrottlingItemsProcessed"] =
                (int)row["ThrottlingItemsProcessed"] + 1;
            StoreThrottlingState(row);
        }
    }

    private void StoreThrottlingState(Guid queueId, DateTime intervalStart,
        int itemsProcessed)
    {
        var dataStructure = persistenceService.SchemaProvider
            .RetrieveInstance<DataStructure>(WorkQueueStateDataStructureId);
        var datasetGenerator = new DatasetGenerator(false);
        var dataSet = datasetGenerator.CreateDataSet(dataStructure);
        var row = dataSet.Tables["WorkQueueState"].NewRow();
        row["refWorkQueueId"] = queueId;
        row["ThrottlingIntervalStart"] = intervalStart;
        row["ThrottlingItemsProcessed"] = itemsProcessed;
        dataSet.Tables["WorkQueueState"].Rows.Add(row);
        StoreThrottlingState(row);
    }
    
    private void StoreThrottlingState(DataRow row)
    {
        core.DataService.Instance.StoreData(WorkQueueStateDataStructureId,
            row.Table.DataSet,false,null);
    }

    private DataRow? GetThrottlingState(Guid queueId)
    {
        DataSet dataSet = new DataSet();
        var parameters = new QueryParameterCollection
        {
            new ("queueId", queueId)
        };

        core.DataService.Instance.LoadRow(WorkQueueStateDataStructureId,
            Guid.Empty, parameters, dataSet, null);
        if (dataSet.Tables.Count == 0 || 
            dataSet.Tables["WorkQueueState"] == null || 
            dataSet.Tables["WorkQueueState"].Rows.Count == 0)
        {
            return null;
        }

        return dataSet.Tables["WorkQueueState"].Rows[0];
    }
}