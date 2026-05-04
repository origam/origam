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
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Origam.DA;
using Origam.Excel;
using Origam.Extensions;
using Origam.Server.Common;
using Origam.Workbench.Services;

namespace Origam.Server;

public class ExcelEntityExporter
{
    private static readonly int characterCellLimit = 32767;
    private ICellStyle dateCellStyle;
    readonly IDataLookupService lookupService =
        ServiceManager.Services.GetService(serviceType: typeof(IDataLookupService))
        as IDataLookupService;
    private readonly IDictionary<string, IDictionary<object, object>> lookupCache =
        new Dictionary<string, IDictionary<object, object>>();
    readonly OrigamSettings settings =
        ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
    readonly bool isExportUnlimited = SecurityManager
        .GetAuthorizationProvider()
        .Authorize(
            principal: SecurityManager.CurrentPrincipal,
            context: "SYS_ExcelExport_Unlimited"
        );
    public ExcelFormat ExportFormat =>
        settings.GUIExcelExportFormat == "XLSX" ? ExcelFormat.XLSX : ExcelFormat.XLS;

    public IWorkbook FillWorkBook(
        EntityExportInfo info,
        List<string> columns,
        IEnumerable<IEnumerable<object>> rows
    )
    {
        IWorkbook workbook = CreateWorkbook();
        SetupDateCellStyle(workbook: workbook);
        ISheet sheet = workbook.CreateSheet(sheetname: "Data");
        SetupSheetHeader(sheet: sheet, info: info);

        int rowIndex = 0;
        foreach (var row in rows)
        {
            if (
                !isExportUnlimited
                && (settings.ExportRecordsLimit > -1)
                && (rowIndex > settings.ExportRecordsLimit)
            )
            {
                FillExportLimitExceeded(workbook: workbook, sheet: sheet, rowNumber: rowIndex);
                break;
            }
            rowIndex++;
            AddRowToSheet(
                info: info,
                workbook: workbook,
                sheet: sheet,
                rowNumber: rowIndex,
                columns: columns,
                row: row.ToList()
            );
        }
        if (FeatureTools.IsFeatureOn(featureCode: OrigamEvent.ExportToExcel.FeatureCode))
        {
            OrigamEventTools.RecordExportToExcel(
                entityExportInfo: info,
                numberOfRows: rowIndex + 1
            );
        }
        return workbook;
    }

    public IWorkbook FillWorkBook(EntityExportInfo info)
    {
        IWorkbook workbook = CreateWorkbook();
        SetupDateCellStyle(workbook: workbook);
        ISheet sheet = workbook.CreateSheet(sheetname: "Data");
        SetupSheetHeader(sheet: sheet, info: info);
        bool isPkGuid = info.Table.PrimaryKey[0].DataType == typeof(Guid);
        int rowNumber = 1;
        for (; rowNumber <= info.RowIds.Count; rowNumber++)
        {
            if (
                !isExportUnlimited
                && (settings.ExportRecordsLimit > -1)
                && (rowNumber > settings.ExportRecordsLimit)
            )
            {
                FillExportLimitExceeded(workbook: workbook, sheet: sheet, rowNumber: rowNumber);
                break;
            }
            DataRow row = GetDataRow(info: info, rowNumber: rowNumber, isPkGuid: isPkGuid);
            if (row != null)
            {
                AddRowToSheet(
                    info: info,
                    workbook: workbook,
                    sheet: sheet,
                    rowNumber: rowNumber,
                    row: row
                );
            }
        }
        if (FeatureTools.IsFeatureOn(featureCode: OrigamEvent.ExportToExcel.FeatureCode))
        {
            OrigamEventTools.RecordExportToExcel(
                entityExportInfo: info,
                numberOfRows: rowNumber - 1
            );
        }
        return workbook;
    }

    private IWorkbook CreateWorkbook()
    {
        if (ExportFormat == ExcelFormat.XLS)
        {
            return new HSSFWorkbook();
        }

        return new XSSFWorkbook();
    }

    private void AddRowToSheet(
        EntityExportInfo info,
        IWorkbook workbook,
        ISheet sheet,
        int rowNumber,
        List<string> columns,
        List<object> row
    )
    {
        if (ExportFormat == ExcelFormat.XLS && rowNumber >= 65536)
        {
            throw new Exception(
                message: "Cannot export more than 65536 lines into a .xls file. Try changing output format to .xlsx"
            );
        }
        IRow excelRow = sheet.CreateRow(rownum: rowNumber);
        for (int i = 0; i < info.Fields.Count; i++)
        {
            AddCellToRow(
                info: info,
                workbook: workbook,
                excelRow: excelRow,
                columnIndex: i,
                columns: columns,
                row: row
            );
        }
    }

    private void AddCellToRow(
        EntityExportInfo info,
        IWorkbook workbook,
        IRow excelRow,
        int columnIndex,
        List<string> columns,
        List<object> row
    )
    {
        EntityExportField field = info.Fields[index: columnIndex];
        ICell cell = excelRow.CreateCell(column: columnIndex);
        object val = GetValue(field: field, columns: columns, row: row);
        SetCellValue(workbook: workbook, val: val, cell: cell);
    }

    private object GetValue(EntityExportField field, List<string> columns, List<object> row)
    {
        int index = columns.FindIndex(match: column => column == field.FieldName);
        object val = row[index: index];
        //object val = row.First(pair => pair.Key == field.FieldName).Value;
        if (val == null)
        {
            return null;
        }
        if (!string.IsNullOrEmpty(value: field.LookupId))
        {
            if (val is string[] valArray)
            {
                return valArray
                    .Select(selector: value => GetLookupValue(key: value, lookupId: field.LookupId))
                    .ToArray();
            }
            return GetLookupValue(key: val, lookupId: field.LookupId);
        }
        return val;
    }

    private void AddRowToSheet(
        EntityExportInfo info,
        IWorkbook workbook,
        ISheet sheet,
        int rowNumber,
        DataRow row
    )
    {
        IRow excelRow = sheet.CreateRow(rownum: rowNumber);
        for (int i = 0; i < info.Fields.Count; i++)
        {
            AddCellToRow(
                info: info,
                workbook: workbook,
                excelRow: excelRow,
                columnIndex: i,
                row: row
            );
        }
    }

    private void AddCellToRow(
        EntityExportInfo info,
        IWorkbook workbook,
        IRow excelRow,
        int columnIndex,
        DataRow row
    )
    {
        EntityExportField field = info.Fields[index: columnIndex];
        ICell cell = excelRow.CreateCell(column: columnIndex);
        object val;
        if (SessionStore.IsColumnArray(dataColumn: info.Table.Columns[name: field.FieldName]))
        {
            val = GetArrayColumnValue(info: info, field: field, row: row);
        }
        else
        {
            val = GetNonArrayColumnValue(field: field, row: row);
        }
        SetCellValue(workbook: workbook, val: val, cell: cell);
    }

    private object GetNonArrayColumnValue(EntityExportField field, DataRow row)
    {
        // normal (non-array) column
        if ((field.LookupId != null) && (field.LookupId != ""))
        {
            return GetLookupValue(key: row[columnName: field.FieldName], lookupId: field.LookupId);
        }

        if (field.PolymorphRules != null)
        {
            var controlFieldValue = row[columnName: field.PolymorphRules.ControlField];
            if (
                (controlFieldValue == null)
                || !field.PolymorphRules.Rules.Contains(key: controlFieldValue.ToString())
            )
            {
                return null;
            }

            return row[
                columnName: field.PolymorphRules.Rules[key: controlFieldValue.ToString()].ToString()
            ];
        }

        return row[columnName: field.FieldName];
    }

    private object GetArrayColumnValue(EntityExportInfo info, EntityExportField field, DataRow row)
    {
        // returns list of array elements
        List<object> arrayElements = SessionStore.GetRowColumnArrayValue(
            row: row,
            dataColumn: info.Table.Columns[name: field.FieldName]
        );
        // try to use default lookup
        if ((field.LookupId == null) || (field.LookupId == ""))
        {
            field.LookupId = info
                .Table.Columns[name: field.FieldName]
                .ExtendedProperties[key: Const.DefaultLookupIdAttribute]
                .ToString();
        }
        if ((field.LookupId != null) && (field.LookupId != ""))
        {
            // lookup array elements
            var lookupedArrayElements = new List<object>(capacity: arrayElements.Count);
            foreach (object arrayElement in arrayElements)
            {
                // get lookup value
                lookupedArrayElements.Add(
                    item: GetLookupValue(key: arrayElement, lookupId: field.LookupId)
                );
            }
            // store lookuped array elements
            return lookupedArrayElements;
        }
        // store array elements
        return arrayElements;
    }

    private void FillExportLimitExceeded(IWorkbook workbook, ISheet sheet, int rowNumber)
    {
        IRow excelRow = sheet.CreateRow(rownum: rowNumber);
        ICell cell = excelRow.CreateCell(column: 0);
        SetCellValue(workbook: workbook, val: Resources.ExportLimitExceeded, cell: cell);
    }

    private void SetupSheetHeader(ISheet sheet, EntityExportInfo info)
    {
        IRow headerRow = sheet.CreateRow(rownum: 0);
        for (int i = 0; i < info.Fields.Count; i++)
        {
            EntityExportField field = info.Fields[index: i];
            headerRow.CreateCell(column: i).SetCellValue(value: field.Caption);
        }
    }

    private void SetupDateCellStyle(IWorkbook workbook)
    {
        dateCellStyle = workbook.CreateCellStyle();
        dateCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat(format: "m/d/yy h:mm");
    }

    private DataRow GetDataRow(EntityExportInfo info, int rowNumber, bool isPkGuid)
    {
        object pk = info.RowIds[index: rowNumber - 1];
        if (isPkGuid && (pk is string))
        {
            pk = new Guid(g: (string)pk);
        }
        DataRow row = info.Table.Rows.Find(key: pk);
        // make sure lazy loaded list gets filled
        if (info.Store.IsLazyLoadedRow(row: row))
        {
            info.Store.LazyLoadListRowData(rowId: pk, row: row);
        }
        return row;
    }

    private Object GetLookupValue(object key, string lookupId)
    {
        if (!lookupCache.ContainsKey(key: lookupId))
        {
            lookupCache.Add(key: lookupId, value: new Dictionary<object, object>());
        }
        IDictionary<object, object> cache = lookupCache[key: lookupId];
        if (!cache.ContainsKey(key: key))
        {
            cache.Add(
                key: key,
                value: lookupService.GetDisplayText(
                    lookupId: new Guid(g: lookupId),
                    lookupValue: key,
                    useCache: false,
                    returnMessageIfNull: false,
                    transactionId: null
                )
            );
        }
        return cache[key: key];
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
                if (sb.Length > 0)
                {
                    sb.Append(value: delimiter);
                }

                string inc = arrayItem.ToString();
                // escape quote chars
                sb.Append(value: inc.Replace(oldValue: delimiter, newValue: escapeDelimiter));
            }
            cell.SetCellValue(value: sb.ToString());
        }
        else
        {
            SetScalarCellValue(workbook: workbook, val: val, cell: cell);
        }
    }

    private void SetScalarCellValue(IWorkbook workbook, object val, ICell cell)
    {
        if (val == null)
        {
            return;
        }
        if (val is DateTime)
        {
            cell.SetCellValue(value: (DateTime)val);
            cell.CellStyle = dateCellStyle;
        }
        else if ((val is int) || (val is double) || (val is float) || (val is decimal))
        {
            cell.SetCellValue(value: Convert.ToDouble(value: val));
        }
        else
        {
            string fieldValue = val.ToString();
            if (fieldValue.Contains(value: "\r"))
            {
                fieldValue = fieldValue.Replace(oldValue: "\n", newValue: "");
                fieldValue = fieldValue.Replace(oldValue: "\r", newValue: Environment.NewLine);
                fieldValue = fieldValue.Replace(oldValue: "\t", newValue: " ");
                cell.SetCellValue(value: fieldValue.Truncate(maxLength: characterCellLimit));
            }
            else
            {
                cell.SetCellValue(value: fieldValue.Truncate(maxLength: characterCellLimit));
            }
        }
    }
}
