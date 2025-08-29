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
        "954f8044-54b8-4554-8777-f5ecc8a839b0"
    );

    private static readonly Guid GetByWorkQueueId = new("7ee32027-ffe9-486c-83ca-e10699475618");

    public bool CanRunNow(WorkQueueData.WorkQueueRow queue)
    {
        if (!queue.EnableThrottling)
        {
            return true;
        }
        DataRow stateRow = GetThrottlingState(queue.Id);
        if (stateRow == null)
        {
            StoreThrottlingState(queue.Id, DateTime.Now, 0);
            return queue.ThrottlingItemsPerInterval > 0;
        }

        DateTime endOfInterval = ((DateTime)stateRow["ThrottlingIntervalStart"]).AddSeconds(
            queue.ThrottlingIntervalSeconds
        );
        if (endOfInterval < DateTime.Now)
        {
            stateRow["ThrottlingItemsProcessed"] = 0;
            stateRow["ThrottlingIntervalStart"] = DateTime.Now;
            StoreThrottlingState(stateRow);
            return queue.ThrottlingItemsPerInterval > 0;
        }

        return (int)stateRow["ThrottlingItemsProcessed"] < queue.ThrottlingItemsPerInterval;
    }

    public void ReportProcessed(WorkQueueData.WorkQueueRow queue)
    {
        if (!queue.EnableThrottling)
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
            row["ThrottlingItemsProcessed"] = (int)row["ThrottlingItemsProcessed"] + 1;
            StoreThrottlingState(row);
        }
    }

    private void StoreThrottlingState(Guid queueId, DateTime intervalStart, int itemsProcessed)
    {
        var dataStructure = persistenceService.SchemaProvider.RetrieveInstance<DataStructure>(
            WorkQueueStateDataStructureId
        );
        var datasetGenerator = new DatasetGenerator(false);
        var dataSet = datasetGenerator.CreateDataSet(dataStructure);
        var row = dataSet.Tables["WorkQueueState"].NewRow();
        row["Id"] = Guid.NewGuid();
        row["refWorkQueueId"] = queueId;
        row["ThrottlingIntervalStart"] = intervalStart;
        row["ThrottlingItemsProcessed"] = itemsProcessed;
        dataSet.Tables["WorkQueueState"].Rows.Add(row);
        StoreThrottlingState(row);
    }

    private void StoreThrottlingState(DataRow row)
    {
        core.DataService.Instance.StoreData(
            WorkQueueStateDataStructureId,
            row.Table.DataSet,
            false,
            null
        );
    }

    private DataRow GetThrottlingState(Guid queueId)
    {
        var parameters = new QueryParameterCollection
        {
            new("WorkQueueState_parWorkQueueId", queueId),
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
            || dataSet.Tables["WorkQueueState"] == null
            || dataSet.Tables["WorkQueueState"].Rows.Count == 0
        )
        {
            return null;
        }
        return dataSet.Tables["WorkQueueState"].Rows[0];
    }
}
