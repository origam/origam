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
using Origam.Workbench.Services;

namespace Origam.Rule;

public interface ICounter
{
    string GetNewCounter(string counterCode, DateTime date, string transactionId);
}

/// <summary>
/// Summary description for Counter.
/// </summary>
public class Counter : ICounter
{
    private const int RETRIES = 50;
    private const int RETRY_INTERVAL = 1000;
    public const string QUERY = "b9a9c301-d33c-4139-ad22-a10e8514e3d5";
    public const string FILTER = "01cc148c-1099-4653-8a17-95d1c9a033be";
    private DataStructureQuery _query = null;
    private CounterDataset _data = new CounterDataset();
    private readonly IServiceAgent _dataServiceAgent;

    public Counter(IBusinessServicesService businessService)
    {
        _dataServiceAgent = businessService.GetAgent("DataService", null, null);
    }

    public string GetNewCounter(string counterCode, DateTime date, string transactionId)
    {
        CounterDataset.CounterDetailRow row = null;
        int counter = 0;
        string result = "";
        for (int i = 0; i <= RETRIES; i++)
        {
            bool error = false;
            try
            {
                _query = new DataStructureQuery(new Guid(QUERY), new Guid(FILTER));
                _query.Parameters.Add(new QueryParameter("Counter_parReferenceCode", counterCode));
                row = ReadRow(_query, counterCode, date, transactionId);
                // increment the counter
                counter = row.CurrentPosition;
                counter += row.Increment;
                // save the counter state back to the database
                Update(_query, row, counter, date, transactionId);
            }
            catch (DBConcurrencyException)
            {
                error = true;
                if (i == RETRIES)
                {
                    throw;
                }
                System.Threading.Thread.Sleep(RETRY_INTERVAL);
            }
            if (!error)
                break;
        }
        string prefix = "";
        if (!row.IsPrefixNull())
        {
            prefix = row.Prefix;
        }
        int lenght = row.Length;
        string tempZeroes = "";
        for (int x = 1; x <= (lenght - counter.ToString().Length); x++)
        {
            tempZeroes += "0";
        }
        result = prefix + tempZeroes + counter.ToString();
        return result;
    }

    private CounterDataset.CounterDetailRow ReadRow(
        DataStructureQuery query,
        string counterCode,
        DateTime date,
        string transactionId
    )
    {
        _dataServiceAgent.MethodName = "LoadDataByQuery";
        _dataServiceAgent.Parameters.Clear();
        _dataServiceAgent.Parameters.Add("Query", query);
        _dataServiceAgent.TransactionId = transactionId;
        _dataServiceAgent.Run();
        DataSet result = _dataServiceAgent.Result as DataSet;
        _data.Clear();
        _data.Merge(result);
        if (_data == null || _data.Counter.Rows.Count < 1)
        {
            throw new ArgumentOutOfRangeException(
                "counterCode",
                counterCode,
                ResourceUtils.GetString("ErrorNoSequence")
            );
        }
        else if (_data.Counter[0].ManageValidityByDate)
        {
            foreach (CounterDataset.CounterDetailRow row in _data.CounterDetail)
            {
                if (
                    row.IsValidFromNull() == false
                    && row.IsValidToNull() == false
                    && (row.ValidFrom <= date & row.ValidTo >= date)
                )
                {
                    return row;
                }
            }
            throw new Exception(
                ResourceUtils.GetString("ErrorNoSequenceForDate", date, counterCode)
            );
        }
        else
        {
            CounterDataset.CounterDetailRow[] rows = _data.Counter[0].GetCounterDetailRows();
            if (rows.Length == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "counterCode",
                    counterCode,
                    "Èíselná øada existuje, ale nemá definován rozsah."
                );
            }
            return rows[0];
        }
    }

    private void Update(
        DataStructureQuery query,
        CounterDataset.CounterDetailRow row,
        int counter,
        DateTime date,
        string transactionId
    )
    {
        string name = (
            row.GetParentRow(row.Table.ParentRelations[0]) as CounterDataset.CounterRow
        ).ReferenceCode;
        if (!row.IsCounterToNull())
        {
            int maximum = (int)row.CounterTo;
            if (maximum <= counter)
            {
                throw new ArgumentException(ResourceUtils.GetString("ErrorSequenceMax", name));
            }
        }
        row.BeginEdit();
        row.CurrentPosition = counter;
        row.EndEdit();

        _dataServiceAgent.MethodName = "StoreDataByQuery";
        _dataServiceAgent.Parameters.Clear();
        _dataServiceAgent.Parameters.Add("Query", query);
        _dataServiceAgent.Parameters.Add("Data", _data);
        _dataServiceAgent.TransactionId = transactionId;
        _dataServiceAgent.Run();
    }
}
