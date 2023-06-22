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
using System.Data;
using Origam.DA;

namespace Origam.Server
{
    public static class DataTools
    {
        public static IDictionary<string, object> DatasetToHashtable(DataSet data)
        {
            return DatasetToHashtable(data, null, 0, null, null, null);
        }

        public static IDictionary<string, object> DatasetToHashtable(
            DataSet data, IList<string> columns, int firstPageRecords, object firstRecordId, 
            string dataListEntity, SessionStore ss)
        {
            if (data == null) return null;

            IDictionary<string, object> resultDataset = new Dictionary<string, object>(data.Tables.Count);

            foreach (DataTable t in data.Tables)
            {
                object recordId = null;
                if (columns != null)
                {
                    if (t.TableName == dataListEntity)
                    {
                        resultDataset.Add(t.TableName, DatatableToHashtable(
                            t, columns, firstPageRecords, firstRecordId, false, 
                            ss));
                    }
                    else
                    {
                        Hashtable emptyTable = new Hashtable();
                        emptyTable.Add("data", new ArrayList());
                        resultDataset.Add(t.TableName, emptyTable);
                    }
                }
                else
                {
                    resultDataset.Add(t.TableName, DatatableToHashtable(t, columns,
                        firstPageRecords, recordId, false, ss));
                }
            }

            return resultDataset;
        }

        public static IDictionary<string, ArrayList> DatatableToHashtable(
            DataTable t, bool includeColumnNames)
        {
            return DatatableToHashtable(t, null, 0, null, includeColumnNames, null);
        }

        public static IDictionary<string, ArrayList> DatatableToHashtable(
            DataTable t, IList<string> columns, int initialPageRecords, 
            object initialRecordId, bool includeColumnNames, SessionStore ss)
        {
            bool primaryKeysOnly = (columns != null);
            IDictionary<string, ArrayList> resultTable = new Dictionary<string, ArrayList>(2);
            string[] allColumnNames = SessionStore.GetColumnNames(t);
            bool primaryKeysOnlyFinal = primaryKeysOnly;
            if (primaryKeysOnly && t.Rows.Count <= initialPageRecords)
            {
                primaryKeysOnlyFinal = false;
            }
            if (includeColumnNames)
            {
                resultTable.Add("columnNames", new ArrayList(allColumnNames));
            }
            if (primaryKeysOnly && ! primaryKeysOnlyFinal && ss != null)
            {
                // less than initialPageRecords was loaded - we have to load up all
                // records because we will return all of them to the frontend at once
                // - no delayed loading
                for (int i = 0; i < t.Rows.Count; i++)
                {
                    DataRow r = t.Rows[i];
                    object rowId = DatasetTools.PrimaryKey(r)[0];
                    ss.LazyLoadListRowData(rowId, r);
                }
            }
            string[] columnNamesFinal;
            if (primaryKeysOnlyFinal)
            {
                // if only primary keys should be sent, we send also all the
                // other preloaded columns, e.g. initial sort columns
                columnNamesFinal = new string[columns.Count + t.PrimaryKey.Length];
                for (int i = 0; i < t.PrimaryKey.Length; i++)
                {
                    columnNamesFinal[i] = t.PrimaryKey[i].ColumnName;
                }
                columns.CopyTo(columnNamesFinal, t.PrimaryKey.Length);
                resultTable.Add("columnNames", new ArrayList(columnNamesFinal));
            }
            else
            {
                columnNamesFinal = allColumnNames;
            }
            ArrayList data = DataTableToArrayList(t, columnNamesFinal);
            resultTable.Add("data", data);
            if (primaryKeysOnlyFinal)
            {
                resultTable.Add("initialPage", 
                    DataTableToArrayList(t, initialPageRecords, initialRecordId,
                    ss));
            }
            return resultTable;
        }

        public static ArrayList DataTableToArrayList(DataTable t, string[] columnNames)
        {
            ArrayList data = new ArrayList(t.Rows.Count);
            foreach (DataRow r in t.Rows)
            {
                if (r.RowState != DataRowState.Deleted && r.RowState != DataRowState.Detached)
                {
                    data.Add(SessionStore.GetRowData(r, columnNames));
                }
            }
            return data;
        }

        public static ArrayList DataTableToArrayList(DataTable t, int pageSize, 
            object startRecordId, SessionStore ss)
        {
            ArrayList data = new ArrayList(pageSize);
            string[] columnNames = SessionStore.GetColumnNames(t);
            DataRow startRow = null;
            int startRowIndex = 0;
            if (t.Rows.Count <= pageSize)
            {
                throw new Exception("Total number of rows is less than the page size. Paged data should not be used for so small number of records.");
            }
            if (startRecordId != null)
            {
                startRow = t.Rows.Find(startRecordId);
                startRowIndex = t.Rows.IndexOf(startRow);
            }
            // if there are not enough records till the end of table, we get some
            // records BEFORE the startRecordId so we always get as many records
            // as defined in the pageSize parameter
            if (t.Rows.Count - startRowIndex < pageSize)
            {
                startRowIndex -= pageSize - (t.Rows.Count - startRowIndex);
            }
            for (int i = startRowIndex; i < startRowIndex + pageSize; i++)
            {
                DataRow r = t.Rows[i];
                object rowId = DatasetTools.PrimaryKey(r)[0];
                ss.LazyLoadListRowData(rowId, r);
                if (r.RowState != DataRowState.Deleted && r.RowState != DataRowState.Detached)
                {
                    data.Add(SessionStore.GetRowData(r, columnNames));
                }
            }
            return data;
        }
    }
}