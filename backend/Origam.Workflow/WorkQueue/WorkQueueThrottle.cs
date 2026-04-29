#region license
/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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

    private static readonly Guid WorkQueueStateDataStructureId = new(
        g: "954f8044-54b8-4554-8777-f5ecc8a839b0"
    );

    private static readonly Guid GetByWorkQueueId = new(g: "7ee32027-ffe9-486c-83ca-e10699475618");

    public bool CanRunNow(WorkQueueData.WorkQueueRow queue)
    {
        if (!queue.EnableThrottling)
        {
            return true;
        }
        DataRow stateRow = GetThrottlingState(queueId: queue.Id);
        if (stateRow == null)
        {
            StoreThrottlingState(queueId: queue.Id, intervalStart: DateTime.Now, itemsProcessed: 0);
            return queue.ThrottlingItemsPerInterval > 0;
        }

        DateTime endOfInterval = (
            (DateTime)stateRow[columnName: "ThrottlingIntervalStart"]
        ).AddSeconds(value: queue.ThrottlingIntervalSeconds);
        if (endOfInterval < DateTime.Now)
        {
            stateRow[columnName: "ThrottlingItemsProcessed"] = 0;
            stateRow[columnName: "ThrottlingIntervalStart"] = DateTime.Now;
            StoreThrottlingState(row: stateRow);
            return queue.ThrottlingItemsPerInterval > 0;
        }

        return (int)stateRow[columnName: "ThrottlingItemsProcessed"]
            < queue.ThrottlingItemsPerInterval;
    }

    public void ReportProcessed(WorkQueueData.WorkQueueRow queue)
    {
        if (!queue.EnableThrottling)
        {
            return;
        }

        DataRow row = GetThrottlingState(queueId: queue.Id);
        if (row == null)
        {
            StoreThrottlingState(queueId: queue.Id, intervalStart: DateTime.Now, itemsProcessed: 1);
        }
        else
        {
            row[columnName: "ThrottlingItemsProcessed"] =
                (int)row[columnName: "ThrottlingItemsProcessed"] + 1;
            StoreThrottlingState(row: row);
        }
    }

    private void StoreThrottlingState(Guid queueId, DateTime intervalStart, int itemsProcessed)
    {
        var dataStructure = persistenceService.SchemaProvider.RetrieveInstance<DataStructure>(
            instanceId: WorkQueueStateDataStructureId
        );
        var datasetGenerator = new DatasetGenerator(userDefinedParameters: false);
        var dataSet = datasetGenerator.CreateDataSet(ds: dataStructure);
        var row = dataSet.Tables[name: "WorkQueueState"].NewRow();
        row[columnName: "Id"] = Guid.NewGuid();
        row[columnName: "refWorkQueueId"] = queueId;
        row[columnName: "ThrottlingIntervalStart"] = intervalStart;
        row[columnName: "ThrottlingItemsProcessed"] = itemsProcessed;
        dataSet.Tables[name: "WorkQueueState"].Rows.Add(row: row);
        StoreThrottlingState(row: row);
    }

    private void StoreThrottlingState(DataRow row)
    {
        core.DataService.Instance.StoreData(
            dataStructureId: WorkQueueStateDataStructureId,
            data: row.Table.DataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: null
        );
    }

    private DataRow GetThrottlingState(Guid queueId)
    {
        var parameters = new QueryParameterCollection
        {
            new(_parameterName: "WorkQueueState_parWorkQueueId", value: queueId),
        };
        DataSet dataSet = core.DataService.Instance.LoadData(
            dataStructureId: WorkQueueStateDataStructureId,
            methodId: GetByWorkQueueId,
            defaultSetId: Guid.Empty,
            sortSetId: Guid.Empty,
            transactionId: null,
            parameters: parameters
        );
        if (
            dataSet.Tables.Count == 0
            || dataSet.Tables[name: "WorkQueueState"] == null
            || dataSet.Tables[name: "WorkQueueState"].Rows.Count == 0
        )
        {
            return null;
        }
        return dataSet.Tables[name: "WorkQueueState"].Rows[index: 0];
    }
}
