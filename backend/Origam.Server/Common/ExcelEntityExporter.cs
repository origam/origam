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
        ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
    private readonly IDictionary<string, IDictionary<object, object>> lookupCache =
        new Dictionary<string, IDictionary<object, object>>();
    readonly OrigamSettings settings =
        ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
    readonly bool isExportUnlimited = SecurityManager
        .GetAuthorizationProvider()
        .Authorize(SecurityManager.CurrentPrincipal, "SYS_ExcelExport_Unlimited");
    public ExcelFormat ExportFormat =>
        settings.GUIExcelExportFormat == "XLSX" ? ExcelFormat.XLSX : ExcelFormat.XLS;

    public IWorkbook FillWorkBook(
        EntityExportInfo info,
        List<string> columns,
        IEnumerable<IEnumerable<object>> rows
    )
    {
        IWorkbook workbook = CreateWorkbook();
        SetupDateCellStyle(workbook);
        ISheet sheet = workbook.CreateSheet("Data");
        SetupSheetHeader(sheet, info);

        int rowIndex = 0;
        foreach (var row in rows)
        {
            if (
                !isExportUnlimited
                && (settings.ExportRecordsLimit > -1)
                && (rowIndex > settings.ExportRecordsLimit)
            )
            {
                FillExportLimitExceeded(workbook, sheet, rowIndex);
                break;
            }
            rowIndex++;
            AddRowToSheet(info, workbook, sheet, rowIndex, columns, row.ToList());
        }
        if (FeatureTools.IsFeatureOn(OrigamEvent.ExportToExcel.FeatureCode))
        {
            OrigamEventTools.RecordExportToExcel(info, rowIndex + 1);
        }
        return workbook;
    }

    public IWorkbook FillWorkBook(EntityExportInfo info)
    {
        IWorkbook workbook = CreateWorkbook();
        SetupDateCellStyle(workbook);
        ISheet sheet = workbook.CreateSheet("Data");
        SetupSheetHeader(sheet, info);
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
                FillExportLimitExceeded(workbook, sheet, rowNumber);
                break;
            }
            DataRow row = GetDataRow(info, rowNumber, isPkGuid);
            if (row != null)
            {
                AddRowToSheet(info, workbook, sheet, rowNumber, row);
            }
        }
        if (FeatureTools.IsFeatureOn(OrigamEvent.ExportToExcel.FeatureCode))
        {
            OrigamEventTools.RecordExportToExcel(info, rowNumber - 1);
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
                "Cannot export more than 65536 lines into a .xls file. Try changing output format to .xlsx"
            );
        }
        IRow excelRow = sheet.CreateRow(rowNumber);
        for (int i = 0; i < info.Fields.Count; i++)
        {
            AddCellToRow(info, workbook, excelRow, i, columns, row);
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
                return valArray.Select(value => GetLookupValue(value, field.LookupId)).ToArray();
            }
            return GetLookupValue(val, field.LookupId);
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
        IRow excelRow = sheet.CreateRow(rowNumber);
        for (int i = 0; i < info.Fields.Count; i++)
        {
            AddCellToRow(info, workbook, excelRow, i, row);
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
        EntityExportField field = info.Fields[columnIndex];
        ICell cell = excelRow.CreateCell(columnIndex);
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
            var controlFieldValue = row[field.PolymorphRules.ControlField];
            if (
                (controlFieldValue == null)
                || !field.PolymorphRules.Rules.Contains(controlFieldValue.ToString())
            )
            {
                return null;
            }

            return row[field.PolymorphRules.Rules[controlFieldValue.ToString()].ToString()];
        }
        else
        {
            return row[field.FieldName];
        }
    }

    private object GetArrayColumnValue(EntityExportInfo info, EntityExportField field, DataRow row)
    {
        // returns list of array elements
        List<object> arrayElements = SessionStore.GetRowColumnArrayValue(
            row,
            info.Table.Columns[field.FieldName]
        );
        // try to use default lookup
        if ((field.LookupId == null) || (field.LookupId == ""))
        {
            field.LookupId = info
                .Table.Columns[field.FieldName]
                .ExtendedProperties[Const.DefaultLookupIdAttribute]
                .ToString();
        }
        if ((field.LookupId != null) && (field.LookupId != ""))
        {
            // lookup array elements
            var lookupedArrayElements = new List<object>(arrayElements.Count);
            foreach (object arrayElement in arrayElements)
            {
                // get lookup value
                lookupedArrayElements.Add(GetLookupValue(arrayElement, field.LookupId));
            }
            // store lookuped array elements
            return lookupedArrayElements;
        }
        // store array elements
        return arrayElements;
    }

    private void FillExportLimitExceeded(IWorkbook workbook, ISheet sheet, int rowNumber)
    {
        IRow excelRow = sheet.CreateRow(rowNumber);
        ICell cell = excelRow.CreateCell(0);
        SetCellValue(workbook, Resources.ExportLimitExceeded, cell);
    }

    private void SetupSheetHeader(ISheet sheet, EntityExportInfo info)
    {
        IRow headerRow = sheet.CreateRow(0);
        for (int i = 0; i < info.Fields.Count; i++)
        {
            EntityExportField field = info.Fields[i];
            headerRow.CreateCell(i).SetCellValue(field.Caption);
        }
    }

    private void SetupDateCellStyle(IWorkbook workbook)
    {
        dateCellStyle = workbook.CreateCellStyle();
        dateCellStyle.DataFormat = workbook.CreateDataFormat().GetFormat("m/d/yy h:mm");
    }

    private DataRow GetDataRow(EntityExportInfo info, int rowNumber, bool isPkGuid)
    {
        object pk = info.RowIds[rowNumber - 1];
        if (isPkGuid && (pk is string))
        {
            pk = new Guid((string)pk);
        }
        DataRow row = info.Table.Rows.Find(pk);
        // make sure lazy loaded list gets filled
        if (info.Store.IsLazyLoadedRow(row))
        {
            info.Store.LazyLoadListRowData(pk, row);
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
            cache.Add(
                key,
                lookupService.GetDisplayText(new Guid(lookupId), key, false, false, null)
            );
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
                if (sb.Length > 0)
                {
                    sb.Append(delimiter);
                }

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

    private void SetScalarCellValue(IWorkbook workbook, object val, ICell cell)
    {
        if (val == null)
        {
            return;
        }
        if (val is DateTime)
        {
            cell.SetCellValue((DateTime)val);
            cell.CellStyle = dateCellStyle;
        }
        else if ((val is int) || (val is double) || (val is float) || (val is decimal))
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
