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
using System.Linq;
using System.Text;
using NPOI.HSSF.UserModel;
using NPOI.HSSF.Util;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Origam.DA;
using Origam.Excel;
using Origam.Workbench.Services;
using Origam.Extensions;

namespace Origam.Server
{
    public class ExcelEntityExporter
    {
        private static readonly int characterCellLimit = 32767;
        private ICellStyle dateCellStyle;
        readonly IDataLookupService lookupService = ServiceManager.Services.GetService(
            typeof(IDataLookupService)) as IDataLookupService;

        private readonly IDictionary<string, IDictionary<object, object>> lookupCache
            = new Dictionary<string, IDictionary<object, object>>();

        readonly OrigamSettings settings
            = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;

        readonly bool isExportUnlimited = SecurityManager.GetAuthorizationProvider()
            .Authorize(SecurityManager.CurrentPrincipal,
                "SYS_ExcelExport_Unlimited");

        public ExcelFormat ExportFormat => settings.GUIExcelExportFormat == "XLSX"
            ? ExcelFormat.XLSX
            : ExcelFormat.XLS;

        public IWorkbook FillWorkBook(EntityExportInfo info, List<string> columns, IEnumerable<IEnumerable<object>> rows)
        {
            ISheet sheet = InitSheet(info);
            int rowIndex = 0;
            foreach (var row in rows)
            {
                if (RowLimitReached(rowIndex))
                {
                    FillExportLimitExceeded(sheet, rowIndex);
                    break;
                }
                rowIndex++;
                AddRowToSheet(info, sheet, rowIndex, columns, row.ToList());
            }
            return sheet.Workbook;
        }

        private ISheet InitSheet(EntityExportInfo info)
        {
            IWorkbook workbook = CreateWorkbook();
            SetupDateCellStyle(workbook);
            ISheet sheet = workbook.CreateSheet("Data");
            SetupSheetHeader(sheet, info, info.Grouping.GroupNames);
            return sheet;
        }

        public IWorkbook FillWorkBook(EntityExportInfo info)
        {
            ISheet sheet = InitSheet(info);
            IWorkbook workbook = sheet.Workbook;
            int rowNumber;
            for (rowNumber = 1; rowNumber <= info.RowIds.Count; rowNumber++)
            {
                if (RowLimitReached(rowNumber))
                {
                    FillExportLimitExceeded(sheet, rowNumber);
                    break;
                }

                DataRow row = GetDataRow(info, rowNumber);
                if (row != null)
                {
                    AddRowToSheet(info, sheet, rowNumber, row);
                }
            }

            AddAggregations(sheet, info, info.Aggregations, rowNumber);
            return workbook;
        }

        private int AddAggregations(ISheet sheet, EntityExportInfo info, 
            List<AggregationData> aggregations, int rowNumber)
        {
            IRow aggregationsRow = sheet.CreateRow(rowNumber);
            foreach (AggregationData aggregation in aggregations)
            {
                int columnIndex = GetColumnIndex(info, aggregation.ColumnName);
                aggregationsRow.CreateCell(columnIndex).SetCellValue(
                    $"{aggregation.AggregationType}: {aggregation.Value}");
            }

            return ++rowNumber;
        }

        private int GetColumnIndex(EntityExportInfo info, string columnName)
        {
            var index = info.Fields.FindIndex(field => field.FieldName == columnName);
            if (index == -1)
            {
                throw new Exception(
                    $"Column {columnName} not found among fields" +
                    $" [{ string.Join(", ",info.Fields.Select(x => x.FieldName))}]");
            }
            return index + info.Grouping.GroupNames.Count;
        }

        public IWorkbook FillWorkBookGrouping(EntityExportInfo info, RootGroup rootGroup)
        {
            ISheet sheet = InitSheet(info);
            int rowNumber = 1;
            foreach (var group in rootGroup.GetGroups())
            {
                rowNumber = FillWorkBookGroupingRecursive(sheet, info,
                    group, rowNumber);
            }
            return sheet.Workbook;
        }

        private int FillWorkBookGroupingRecursive(
            ISheet sheet, EntityExportInfo info, IGroup group,
            int rowNumber)
        {
            if (RowLimitReached(rowNumber))
            {
                FillExportLimitExceeded(sheet, rowNumber);
                return rowNumber;
            }
            AddGroupHeading(sheet, rowNumber, group.Level, group.ColumnValue);
            rowNumber++;
            int groupFirstRow = rowNumber;

            var childGroups = group.GetGroups();
            foreach (var childGroup in childGroups)
            {
                rowNumber = FillWorkBookGroupingRecursive(sheet, info,
                    childGroup, rowNumber);
            }

            if (childGroups.Count == 0)
            {
                rowNumber = AddRows(sheet, info, group, rowNumber);
            }
            sheet.GroupRow(groupFirstRow, rowNumber - 2);
            return rowNumber;
        }

        private int AddRows(ISheet sheet, EntityExportInfo info,
            IGroup group, int rowNumber)
        {
            var dataRow = info.Table.NewRow();
            foreach (IEnumerable<object> row in group.GetRows())
            {
                if (RowLimitReached(rowNumber))
                {
                    FillExportLimitExceeded(sheet, rowNumber);
                    break;
                }
                    
                using var enumerator = row.GetEnumerator();
                foreach (var field in info.Fields)
                {
                    enumerator.MoveNext();
                    var value = enumerator.Current;
                    var columnName = field.FieldName;
                    DataColumn column = info.Table.Columns[columnName];
                    if (value is string[] array && column.DataType == typeof(long))
                    {
                        dataRow[columnName] = array.Length;
                    }
                    else
                    {
                        dataRow[columnName] = value ?? DBNull.Value;
                    }
                }
                AddRowToSheet(
                    info, sheet, rowNumber, dataRow, columnOffset:group.Level + 1);
                rowNumber++;
            }
            rowNumber++;
            return rowNumber;
        }

        public IWorkbook FillWorkBookGrouping(EntityExportInfo info)
        {
            ISheet sheet = InitSheet(info);
            IWorkbook workbook = sheet.Workbook;
            int rowNumber = 1;
            int groupLevel = 0;
            foreach (var group in info.Grouping.Groups)
            {
               FillWorkBookGroupingRecursive(sheet, info, group, rowNumber, groupLevel);
            }

            return workbook;
        }
        
        private int FillWorkBookGroupingRecursive(
            ISheet sheet, EntityExportInfo info, GroupNode group,
            int rowNumber, int groupLevel)
        {
            if (RowLimitReached(rowNumber))
            {
                FillExportLimitExceeded(sheet, rowNumber);
                return rowNumber;
            }
            AddGroupHeading( sheet, rowNumber, groupLevel, group.Value);
            rowNumber++;
            int groupFirstRow = rowNumber;
            foreach (var childGroup in group.ChildGroups)
            {
                 rowNumber = FillWorkBookGroupingRecursive(
                    sheet, info, childGroup, rowNumber, groupLevel + 1);
                 rowNumber++;
            }
            if (group.RowIds.Length > 0)
            {
                rowNumber = AddRows(sheet, info, group, rowNumber);
            }
            sheet.GroupRow(groupFirstRow, rowNumber - 1);
            rowNumber = AddAggregations(sheet, info, group.Aggregations, rowNumber);
            
            return rowNumber;
        }

        private int AddRows(ISheet sheet, EntityExportInfo info,
            GroupNode group, int rowNumber)
        {
            foreach (var rowId in group.RowIds)
            {
                if (RowLimitReached(rowNumber))
                {
                    FillExportLimitExceeded(sheet, rowNumber);
                    return rowNumber;
                }
                DataRow row = GetDataRow(info, rowId);
                if (row != null)
                {
                    AddRowToSheet(
                        info: info, 
                        sheet: sheet, 
                        rowNumber: rowNumber, 
                        row: row, 
                        columnOffset: info.Grouping.ColumnSettings.Length);
                    rowNumber++;
                } 
            }

            return rowNumber;
        }

        private bool RowLimitReached(int rowNumber)
        {
            return !isExportUnlimited && (settings.ExportRecordsLimit > -1)
                                      && (rowNumber > settings.ExportRecordsLimit);
        }

        private IWorkbook CreateWorkbook()
        {
            if (ExportFormat == ExcelFormat.XLS)
            {
                return new HSSFWorkbook();
            }
            else
            {
                return new XSSFWorkbook();
            }
        }

        private void AddRowToSheet(
            EntityExportInfo info, ISheet sheet,
            int rowNumber, List<string> columns, List<object> row)
        {
            if (ExportFormat == ExcelFormat.XLS && rowNumber >= 65536)
            {
                throw new Exception("Cannot export more than 65536 lines into a .xls file. Try changing output format to .xlsx");
            }

            IRow excelRow = sheet.CreateRow(rowNumber);
            for (int i = 0; i < info.Fields.Count; i++)
            {
                AddCellToRow(info, sheet.Workbook, excelRow, i, columns, row);
            }
        }
        
        private void AddCellToRow(
            EntityExportInfo info, IWorkbook workbook, IRow excelRow,
            int columnIndex, List<string> columns, List<object> row)
        {
            EntityExportField field = info.Fields[columnIndex];
            ICell cell = excelRow.CreateCell(columnIndex);
            object val = GetValue(field, columns, row);
            SetCellValue(workbook, val, cell);
        }


        private object GetValue(EntityExportField field, List<string> columns, List<object> row)
        {
            int index = columns.FindIndex(column => column == field.FieldName);
            object val = row[index];
            //object val = row.First(pair => pair.Key == field.FieldName).Value;
            if (val == null)
            {
                return null;
            }
            if (!string.IsNullOrEmpty(field.LookupId))
            {
                if (val is string[] valArray)
                {
                    return valArray.Select(value =>
                        GetLookupValue(value, field.LookupId)).ToArray();
                }
                return GetLookupValue(val, field.LookupId);
            }
            return val;
        }

        private void AddRowToSheet(
            EntityExportInfo info, ISheet sheet,
            int rowNumber, DataRow row, int columnOffset = 0)
        {
            IRow excelRow = sheet.CreateRow(rowNumber);
            for (int i = 0; i < info.Fields.Count; i++)
            {
                AddCellToRow(
                    info: info,
                    workbook: sheet.Workbook, 
                    excelRow: excelRow, 
                    dataColumnIndex: i, 
                    excelColumnIndex: columnOffset + i, 
                    row: row);
            }
        }

        private void AddGroupHeading (ISheet sheet,
            int rowNumber, int groupLevel, object groupName)
        {
            IRow excelRow = sheet.CreateRow(rowNumber);
            ICell cell = excelRow.CreateCell(groupLevel);
            SetTextBold(sheet.Workbook, cell);
            SetCellValue(sheet.Workbook, groupName, cell);
        }

        private static void SetTextBold(IWorkbook workbook, ICell cell)
        {
            var font = workbook.CreateFont();
            font.FontHeightInPoints = 10;
            font.FontName = "Arial";
            font.IsBold = true; 
            cell.CellStyle = workbook.CreateCellStyle();
            cell.CellStyle.SetFont(font);
        }

        private void AddCellToRow(
            EntityExportInfo info, IWorkbook workbook, IRow excelRow,
            int dataColumnIndex, int excelColumnIndex, DataRow row)
        {
            EntityExportField field = info.Fields[dataColumnIndex];
            ICell cell = excelRow.CreateCell(excelColumnIndex);
            object val;
            if (SessionStore.IsColumnArray(info.Table.Columns[field.FieldName]))
            {
                val = GetArrayColumnValue(info, field, row);
            }
            else
            {
                val = GetNonArrayColumnValue(field, row);
            }

            SetCellValue(workbook, val, cell);
        }

        private object GetNonArrayColumnValue(EntityExportField field, DataRow row)
        {
            // normal (non-array) column
            if ((field.LookupId != null) && (field.LookupId != ""))
            {
                return GetLookupValue(row[field.FieldName], field.LookupId);
            }
            else if (field.PolymorphRules != null)
            {
                var controlFieldValue = row[
                    field.PolymorphRules.ControlField];
                if ((controlFieldValue == null)
                    || !field.PolymorphRules.Rules.Contains(
                        controlFieldValue.ToString()))
                {
                    return null;
                }
                else
                {
                    return row[field.PolymorphRules.Rules[
                            controlFieldValue.ToString()]
                        .ToString()];
                }
            }
            else
            {
                return row[field.FieldName];
            }
        }

        private object GetArrayColumnValue(
            EntityExportInfo info, EntityExportField field, DataRow row)
        {
            // returns list of array elements
            ArrayList arrayElements =
                SessionStore.GetRowColumnArrayValue(row,
                    info.Table.Columns[field.FieldName]);
            // try to use default lookup
            if ((field.LookupId == null) || (field.LookupId == ""))
            {
                field.LookupId = info.Table.Columns[field.FieldName]
                    .ExtendedProperties[Const.DefaultLookupIdAttribute].ToString();
            }

            if ((field.LookupId != null) && (field.LookupId != ""))
            {
                // lookup array elements
                ArrayList lookupedArrayElements = new ArrayList(arrayElements.Count);
                foreach (object arrayElement in arrayElements)
                {
                    // get lookup value
                    lookupedArrayElements.Add(
                        GetLookupValue(arrayElement, field.LookupId));
                }

                // store lookuped array elements
                return lookupedArrayElements;
            }
            else
            {
                // store array elements
                return arrayElements;
            }
        }

        private void FillExportLimitExceeded(ISheet sheet, int rowNumber)
        {
            IRow excelRow = sheet.CreateRow(rowNumber);
            ICell cell = excelRow.CreateCell(0);
            SetCellValue(sheet.Workbook, Resources.ExportLimitExceeded, cell);
        }

        private void SetupSheetHeader(ISheet sheet, EntityExportInfo info, List<string> groupNames)
        {
            IRow headerRow = sheet.CreateRow(0);
            for (int i = 0; i < groupNames.Count; i++)
            {
                ICell cell = headerRow.CreateCell(i);
                cell.SetCellValue(groupNames[i]);
                SetTextBold(sheet.Workbook, cell);
            }
            for (int i = 0; i < info.Fields.Count; i++)
            {
                EntityExportField field = info.Fields[i];
                headerRow.CreateCell( groupNames.Count + i)
                    .SetCellValue(field.Caption);
            }
        }

        private void SetupDateCellStyle(IWorkbook workbook)
        {
            dateCellStyle = workbook.CreateCellStyle();
            dateCellStyle.DataFormat
                = workbook.CreateDataFormat().GetFormat("m/d/yy h:mm");
        }

        private DataRow GetDataRow(
            EntityExportInfo info, int rowNumber)
        {
            object rowId = info.RowIds[rowNumber - 1];
            return GetDataRow(info, rowId);
        }
        private DataRow GetDataRow(
            EntityExportInfo info, object rowId)
        {
            bool isPkGuid = info.Table.PrimaryKey[0].DataType == typeof(Guid);
            if (isPkGuid && (rowId is string stringRowId))
            {
                rowId = new Guid(stringRowId);
            }

            DataRow row = info.Table.Rows.Find(rowId);
            // make sure lazy loaded list gets filled
            if (info.Store.IsLazyLoadedRow(row))
            {
                info.Store.LazyLoadListRowData(rowId, row);
            }

            return row;
        }

        private Object GetLookupValue(object key, string lookupId)
        {
            if (!lookupCache.ContainsKey(lookupId))
            {
                lookupCache.Add(lookupId, new Dictionary<object, object>());
            }

            IDictionary<object, object> cache = lookupCache[lookupId];
            if (!cache.ContainsKey(key))
            {
                cache.Add(key, lookupService.GetDisplayText(
                    new Guid(lookupId), key, false, false, null));
            }

            return cache[key];
        }

        private void SetCellValue(IWorkbook workbook, object val, ICell cell)
        {
            if (val is IEnumerable enumerable && !(val is string))
            {
                String delimiter = ",";
                String escapeDelimiter = "\\,";
                StringBuilder sb = new StringBuilder();
                foreach (object arrayItem in enumerable)
                {
                    // add array item to stream
                    if (sb.Length > 0) sb.Append(delimiter);
                    string inc = arrayItem.ToString();
                    // escape quote chars
                    sb.Append(inc.Replace(delimiter, escapeDelimiter));
                }

                cell.SetCellValue(sb.ToString());
            }
            else
            {
                SetScalarCellValue(workbook, val, cell);
            }
        }

        private void SetScalarCellValue(
            IWorkbook workbook, object val, ICell cell)
        {
            if (val == null)
            {
                return;
            }

            if (val is DateTime)
            {
                cell.SetCellValue((DateTime) val);
                cell.CellStyle = dateCellStyle;
            }
            else if ((val is int) || (val is double)
                                  || (val is float) || (val is decimal))
            {
                cell.SetCellValue(Convert.ToDouble(val));
            }
            else
            {
                string fieldValue = val.ToString();
                if (fieldValue.Contains("\r"))
                {
                    fieldValue = fieldValue.Replace("\n", "");
                    fieldValue = fieldValue.Replace("\r", Environment.NewLine);
                    fieldValue = fieldValue.Replace("\t", " ");
                    cell.SetCellValue(fieldValue.Truncate(characterCellLimit));
                }
                else
                {
                    cell.SetCellValue(fieldValue.Truncate(characterCellLimit));
                }
            }
        }
    }

    public interface IGroup
    {
        EntityExportInfo ExportInfo { get; }
        int Level { get; }
        object ColumnValue { get; }
        string ColumnId { get; }
        IGroup Parent { get; }
        public string ChildFilter { get; }
        List<IGroup> GetGroups();
        IEnumerable<IEnumerable<object>> GetRows();
    }

    public class RootGroup : IGroup
    {
        private readonly Func<IGroup, List<IGroup>> childGroupGetter;
        public EntityExportInfo ExportInfo { get; }
        public int Level { get; } = -1;
        public object ColumnValue { get; }
        public string ColumnId { get; }
        public IGroup Parent { get; }
        public string ChildFilter { get; }

        public RootGroup(EntityExportInfo exportInfo, Func<IGroup, List<IGroup>> childGroupGetter)
        {
            this.childGroupGetter = childGroupGetter;
            ExportInfo = exportInfo;
            ChildFilter = ExportInfo.LazyLoadedEntityInput.Filter;
        }
        
        public List<IGroup> GetGroups()
        {
            return childGroupGetter(this);
        }

        public IEnumerable<IEnumerable<object>>  GetRows()
        {
            yield break;
        }
    }

    class Group : IGroup
    {
        private readonly Func<IGroup, List<IGroup>> childGroupGetter;
        private readonly Func<IGroup, IEnumerable<IEnumerable<object>>> childRowGetter;
        private readonly int childLevel;

        public EntityExportInfo ExportInfo { get; }
        public int Level { get; }
        public object ColumnValue { get; }
        public string ColumnId { get; }
        public IGroup Parent { get;} 
        
        public string ChildFilter => 
            SetParameterValues(ExportInfo.Grouping.ColumnSettings[Level].Filter);

        public Group(EntityExportInfo entityExportInfo, int level,
            object columnValue, Func<IGroup, List<IGroup>> childGroupGetter, 
            Func<IGroup, IEnumerable<IEnumerable<object>>> childRowGetter, IGroup parent)
        {
            ExportInfo = entityExportInfo;
            Level = level;
            childLevel = level + 1;
            ColumnId = entityExportInfo.Grouping.ColumnSettings[level].Id;
            ColumnValue = columnValue;
            this.childGroupGetter = childGroupGetter;
            this.childRowGetter = childRowGetter;
            Parent = parent;
        }

        public List<IGroup> GetGroups()
        {
            return childLevel >= ExportInfo.Grouping.ColumnSettings.Length
                ? new List<IGroup>()
                : childGroupGetter(this);
        }

        public IEnumerable<IEnumerable<object>> GetRows()
        {
            return childRowGetter(this);
        }

        private List<IGroup> GetParentsIncludingThis()
        {
            List<IGroup> parents = new List<IGroup>{this};
            IGroup parent = Parent;
            while (parent != null)
            {
                parents.Add(parent);
                parent = parent.Parent;
            }
            return parents;
        }
        
        private string SetParameterValues(string filterWithPaceHolders)
        {
            foreach (var group in GetParentsIncludingThis().OfType<Group>())
            {
                filterWithPaceHolders = filterWithPaceHolders.Replace(
                    $"{group.ColumnId}_placeHolder", group.ColumnValue.ToString());
            }

            return filterWithPaceHolders;
        }
    }
}