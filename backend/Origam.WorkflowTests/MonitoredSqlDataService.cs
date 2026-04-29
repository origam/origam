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

using System.Data;
using System.Security.Principal;
using Origam.DA;
using Origam.DA.Service;

namespace Origam.WorkflowTests;

public class MonitoredMsSqlDataService : MsSqlDataService, ITraceService
{
    private readonly SqlDataServiceMonitor monitor = new();
    public List<Operation> Operations => monitor.Operations;

    public override int UpdateData(
        DataStructureQuery query,
        IPrincipal userProfile,
        DataSet dataset,
        string transactionId,
        bool forceBulkInsert
    )
    {
        monitor.TraceUpdateData(dataset: dataset);
        return base.UpdateData(
            query: query,
            userProfile: userProfile,
            dataset: dataset,
            transactionId: transactionId,
            forceBulkInsert: forceBulkInsert
        );
    }
}

public class MonitoredPgSqlDataService : PgSqlDataService, ITraceService
{
    private readonly SqlDataServiceMonitor monitor = new();
    public List<Operation> Operations => monitor.Operations;

    public override int UpdateData(
        DataStructureQuery query,
        IPrincipal userProfile,
        DataSet dataset,
        string transactionId,
        bool forceBulkInsert
    )
    {
        monitor.TraceUpdateData(dataset: dataset);
        return base.UpdateData(
            query: query,
            userProfile: userProfile,
            dataset: dataset,
            transactionId: transactionId,
            forceBulkInsert: forceBulkInsert
        );
    }
}

interface ITraceService
{
    public List<Operation> Operations { get; }
}

public class SqlDataServiceMonitor
{
    public List<Operation> Operations { get; } = new();

    private void AddOperation(Operation operation)
    {
        Operations.Add(item: operation);
    }

    public void TraceUpdateData(DataSet dataset)
    {
        var deletesWorkQueueEntry =
            dataset
                is {
                    Tables: [
                        { TableName: "WorkQueueEntry", Rows: [{ RowState: DataRowState.Deleted }] },
                    ]
                };
        if (deletesWorkQueueEntry)
        {
            dataset.Tables[index: 0].Rows[index: 0].RejectChanges();
            var deletedRowId = (Guid)dataset.Tables[index: 0].Rows[index: 0][columnName: "Id"];
            var refWorkQueueId = (Guid)
                dataset.Tables[index: 0].Rows[index: 0][columnName: "refWorkQueueId"];
            dataset.Tables[index: 0].Rows[index: 0].Delete();
            AddOperation(
                operation: new DeleteWorkQueueEntryOperation(
                    Name: "UpdateData",
                    Parameters: new Dictionary<string, object>
                    {
                        { "refWorkQueueId", refWorkQueueId },
                        { "deletedRowId", deletedRowId },
                        { "executedAt", DateTime.Now },
                    }
                )
            );
        }
    }
}

public record Operation(string Name, Dictionary<string, object> Parameters)
{
    public override string ToString()
    {
        string parameters = string.Join(
            separator: ", ",
            values: Parameters.Select(selector: x => $"[{x.Key}, {x.Value}]")
        );
        return $"Operation: {Name}, Parameters: {parameters}";
    }
}

public record DeleteWorkQueueEntryOperation(string Name, Dictionary<string, object> Parameters)
    : Operation(Name: Name, Parameters: Parameters)
{
    public Guid RowId => (Guid)Parameters[key: "deletedRowId"];
    public DateTime ExecutedAt => (DateTime)Parameters[key: "executedAt"];
}
