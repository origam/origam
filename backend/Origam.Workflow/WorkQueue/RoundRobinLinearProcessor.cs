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
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using Origam.Mail;
using WorkQueueRow = Origam.Workflow.WorkQueue.WorkQueueData.WorkQueueRow;

namespace Origam.Workflow.WorkQueue;

public class RoundRobinLinearProcessor : LinearProcessor
{
    private readonly int batchSize;

    public RoundRobinLinearProcessor(
        Action<WorkQueueRow, DataRow> itemProcessAction,
        WorkQueueUtils workQueueUtils,
        RetryManager retryManager,
        WorkQueueThrottle workQueueThrottle,
        int batchSize
    )
        : base(itemProcessAction, workQueueUtils, retryManager, workQueueThrottle)
    {
        this.batchSize = batchSize;
    }

    public override void Run(IEnumerable<WorkQueueRow> queues, CancellationToken cancellationToken)
    {
        List<WorkQueueRow> queueList = queues.ToList();
        int numberOfEmptyQueues = 0;
        while (numberOfEmptyQueues != queueList.Count)
        {
            numberOfEmptyQueues = 0;
            foreach (WorkQueueRow queue in queueList)
            {
                int itemsProcessed = ProcessAutoQueueCommands(
                    queue: queue,
                    cancellationToken: cancellationToken,
                    maxItemsToProcess: batchSize
                );
                if (itemsProcessed == 0)
                {
                    numberOfEmptyQueues++;
                }
            }
        }
    }
}
