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
using System.Reflection;
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
        monitor.TraceUpdateData(dataset);
        return base.UpdateData(query, userProfile, dataset, transactionId, forceBulkInsert);
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
        monitor.TraceUpdateData(dataset);
        return base.UpdateData(query, userProfile, dataset, transactionId, forceBulkInsert);
    }
}

interface ITraceService
{
    public List<Operation> Operations { get; }
}

public class SqlDataServiceMonitor
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        MethodBase.GetCurrentMethod()?.DeclaringType);
        
    public List<Operation> Operations { get; } = new();

    private void AddOperation(Operation operation)
    {
        Operations.Add(operation);
    }

    public void TraceUpdateData(DataSet dataset)
    {
        if (dataset.Tables.Count == 0)
        {
            log.Debug("No table in dataset");
        }

        DataTable table = dataset.Tables[0];
        log.Debug("TableName: " + table.TableName + "Rows: " + string.Join(", ", table.Rows.Cast<DataRow>().Select(x => x.RowState).Distinct()));
        var deletesWorkQueueEntry =
            dataset
                is {
                    Tables: [
                        { TableName: "WorkQueueEntry", Rows: [{ RowState: DataRowState.Deleted }] },
                    ]
                };
        if (deletesWorkQueueEntry)
        {
            table.Rows[0].RejectChanges();
            var deletedRowId = (Guid)table.Rows[0]["Id"];
            var refWorkQueueId = (Guid)table.Rows[0]["refWorkQueueId"];
            table.Rows[0].Delete();
            AddOperation(
                new DeleteWorkQueueEntryOperation(
                    "UpdateData",
                    new Dictionary<string, object>
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
        string parameters = string.Join(", ", Parameters.Select(x => $"[{x.Key}, {x.Value}]"));
        return $"Operation: {Name}, Parameters: {parameters}";
    }
}

public record DeleteWorkQueueEntryOperation(string Name, Dictionary<string, object> Parameters)
    : Operation(Name, Parameters)
{
    public Guid RowId => (Guid)Parameters["deletedRowId"];
    public DateTime ExecutedAt => (DateTime)Parameters["executedAt"];
}
