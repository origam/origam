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
using System.IO;
using NPOI;
using NPOI.HPSF;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Origam.Workflow;

namespace Origam.Excel;

public class ExcelTools
{
    public static ExcelFormat StringToExcelFormat(string input)
    {
        if (input.ToUpper() == "XLSX")
        {
            return ExcelFormat.XLSX;
        }

        return ExcelFormat.XLS;
    }

    public static IWorkbook GetWorkbook(ExcelFormat excelFormat)
    {
        if (excelFormat == ExcelFormat.XLSX)
        {
            return new XSSFWorkbook();
        }

        return new HSSFWorkbook();
    }

    public static IWorkbook GetWorkbook(ExcelFormat excelFormat, Stream stream)
    {
        if (excelFormat == ExcelFormat.XLSX)
        {
            return new XSSFWorkbook(fileStream: stream);
        }

        return new HSSFWorkbook(s: stream);
    }

    public static void SetWorkbookSubject(IWorkbook workbook, string subject)
    {
        if (workbook is HSSFWorkbook)
        {
            SetHSSFWorkbookSubject(workbook: workbook as HSSFWorkbook, subject: subject);
        }
        else if (workbook is XSSFWorkbook)
        {
            SetXSSFWorkbookSubject(workbook: workbook as XSSFWorkbook, subject: subject);
        }
    }

    private static void SetHSSFWorkbookSubject(HSSFWorkbook workbook, string subject)
    {
        SummaryInformation si = PropertySetFactory.CreateSummaryInformation();
        si.Subject = subject;
        workbook.SummaryInformation = si;
    }

    private static void SetXSSFWorkbookSubject(XSSFWorkbook workbook, string subject)
    {
        POIXMLProperties xmlProps = workbook.GetProperties();
        CoreProperties coreProps = xmlProps.CoreProperties;
        coreProps.Subject = subject;
    }

    public static ISheet[] Sheets(IWorkbook wb)
    {
        return Sheets(sheetName: null, wb: wb);
    }

    public static ISheet[] Sheets(string sheetName, IWorkbook wb)
    {
        ISheet[] sheets;
        if (sheetName == null)
        {
            sheets = new ISheet[wb.NumberOfSheets];
            // get all sheets
            for (int i = 0; i < wb.NumberOfSheets; i++)
            {
                sheets[i] = wb.GetSheetAt(index: i);
            }
        }
        else
        {
            // get one sheet
            ISheet sheet = wb.GetSheet(name: sheetName);
            if (sheet == null)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "sheetName",
                    actualValue: sheetName,
                    message: ResourceUtils.GetString(key: "ExcelSheetNotFound")
                );
            }
            sheets = new ISheet[] { sheet };
        }
        return sheets;
    }

    public static IWorkbook LoadFile(string fileName, ExcelFormat excelFormat = ExcelFormat.XLS)
    {
        IWorkbook wb;
        // read from disk
        FileInfo fi = new FileInfo(fileName: fileName);
        FileStream stream = null;
        if (fi.Exists)
        {
            stream = fi.Open(mode: FileMode.Open);
            if (excelFormat == ExcelFormat.XLSX)
            {
                wb = new XSSFWorkbook(fileStream: stream);
            }
            else
            {
                wb = new HSSFWorkbook(s: stream);
            }
            stream.Close();
        }
        else
        {
            throw new FileNotFoundException(
                message: ResourceUtils.GetString(key: "ExcelFileNotFound"),
                fileName: fileName
            );
        }
        return wb;
    }

    public static void ReadValue(TextReaderOptions options, DataRow row, ICell cell, DataColumn col)
    {
        TextReaderOptionsField fieldOptions = options.GetFieldOption(fieldName: col.ColumnName);
        CellType cellType =
            cell.CellType == CellType.Formula ? cell.CachedFormulaResultType : cell.CellType;
        if (cellType == CellType.Blank)
        {
            if (fieldOptions?.NullValue != null)
            {
                // we always assign a value if NullValue was specified
                row[column: col] = Convert.ChangeType(
                    value: fieldOptions.NullValue,
                    conversionType: col.DataType
                );
            }
            else if (col.AllowDBNull || col.DefaultValue != null)
            {
                // the value is empty or a default was used
                return;
            }
            else
            {
                throw new NoNullAllowedException(
                    s: string.Format(
                        format: "Cell {0},{1} is empty but field {2} does not allow nulls.",
                        arg0: cell.RowIndex + 1,
                        arg1: cell.ColumnIndex + 1,
                        arg2: col.ColumnName
                    )
                );
            }
        }
        if (col.DataType == typeof(string))
        {
            ReadStringValue(row: row, cellType: cellType, cell: cell, col: col);
        }
        else if (col.DataType == typeof(DateTime))
        {
            ReadDateValue(
                fieldOptions: fieldOptions,
                row: row,
                cellType: cellType,
                cell: cell,
                col: col
            );
        }
        else if (col.DataType == typeof(Guid))
        {
            ReadGuidValue(row: row, cell: cell, col: col);
        }
        else if (col.DataType == typeof(bool))
        {
            ReadBoolValue(row: row, cellType: cellType, cell: cell, col: col);
        }
        else if (col.DataType == typeof(int))
        {
            ReadIntValue(row: row, cellType: cellType, cell: cell, col: col);
        }
        else if (col.DataType == typeof(float))
        {
            ReadDecimalValue(
                fieldOptions: fieldOptions,
                row: row,
                cellType: cellType,
                cell: cell,
                col: col
            );
        }
        else if (col.DataType == typeof(decimal))
        {
            ReadDecimalValue(
                fieldOptions: fieldOptions,
                row: row,
                cellType: cellType,
                cell: cell,
                col: col
            );
        }
        else if (col.DataType == typeof(long))
        {
            ReadLongValue(
                fieldOptions: fieldOptions,
                row: row,
                cellType: cellType,
                cell: cell,
                col: col
            );
        }
    }

    private static void ReadLongValue(
        TextReaderOptionsField fieldOptions,
        DataRow row,
        CellType cellType,
        ICell cell,
        DataColumn col
    )
    {
        if (cellType == CellType.Numeric)
        {
            row[column: col] = cell.NumericCellValue;
        }
        else if (fieldOptions != null)
        {
            row[column: col] = long.Parse(
                s: cell.StringCellValue,
                provider: fieldOptions.GetCulture()
            );
        }
        else
        {
            row[column: col] = long.Parse(s: cell.StringCellValue);
        }
    }

    private static void ReadDecimalValue(
        TextReaderOptionsField fieldOptions,
        DataRow row,
        CellType cellType,
        ICell cell,
        DataColumn col
    )
    {
        if (cellType == CellType.Numeric)
        {
            row[column: col] = cell.NumericCellValue;
        }
        else if (fieldOptions != null)
        {
            row[column: col] = decimal.Parse(
                s: cell.StringCellValue,
                provider: fieldOptions.GetCulture()
            );
        }
        else
        {
            row[column: col] = decimal.Parse(s: cell.StringCellValue);
        }
    }

    private static void ReadIntValue(DataRow row, CellType cellType, ICell cell, DataColumn col)
    {
        if (cellType == CellType.Numeric)
        {
            row[column: col] = cell.NumericCellValue;
        }
        else
        {
            row[column: col] = int.Parse(s: cell.StringCellValue);
        }
    }

    private static void ReadBoolValue(DataRow row, CellType cellType, ICell cell, DataColumn col)
    {
        if (cellType == CellType.Boolean)
        {
            row[column: col] = cell.BooleanCellValue;
        }
        else
        {
            row[column: col] = bool.Parse(value: cell.StringCellValue);
        }
    }

    private static void ReadGuidValue(DataRow row, ICell cell, DataColumn col)
    {
        if (cell.StringCellValue != "")
        {
            row[column: col] = new Guid(g: cell.StringCellValue);
        }
    }

    private static void ReadDateValue(
        TextReaderOptionsField fieldOptions,
        DataRow row,
        CellType cellType,
        ICell cell,
        DataColumn col
    )
    {
        if (cellType == CellType.Numeric)
        {
            row[column: col] = cell.DateCellValue;
        }
        else if (cellType == CellType.String)
        {
            if (fieldOptions != null && fieldOptions.Format != null && cell.StringCellValue != "")
            {
                row[column: col] = DateTime.ParseExact(
                    s: cell.StringCellValue,
                    formats: fieldOptions.Formats,
                    provider: fieldOptions.GetCulture(),
                    style: System.Globalization.DateTimeStyles.None
                );
            }
        }
    }

    private static void ReadStringValue(DataRow row, CellType cellType, ICell cell, DataColumn col)
    {
        if (cellType == CellType.Numeric)
        {
            row[column: col] = cell.NumericCellValue.ToString();
        }
        else
        {
            row[column: col] = cell.StringCellValue;
        }
    }

    public static void SaveWorkbook(IWorkbook wb, FileInfo fi)
    {
        FileStream stream = null;
        if (fi.Exists)
        {
            stream = fi.Open(mode: FileMode.Open);
        }
        else
        {
            stream = fi.Open(mode: FileMode.Create);
        }
        // recalculate formulas on all sheets
        for (int i = 0; i < wb.NumberOfSheets; i++)
        {
            wb.GetSheetAt(index: i).ForceFormulaRecalculation = true;
        }
        wb.Write(stream: stream, leaveOpen: false);
        stream.Close();
    }

    public static ISheet CreateOrEmptySheet(string sheetName, IWorkbook wb)
    {
        ISheet sheet = wb.GetSheet(name: sheetName);
        if (sheet == null)
        {
            // sheet does not exist, we create it
            sheet = wb.CreateSheet(sheetname: sheetName);
        }
        else
        {
            // sheet exists already, we empty it
            EmptySheet(sheet: sheet);
        }
        return sheet;
    }

    public static IWorkbook OpenOrCreateWorkbook(FileInfo fi, ExcelFormat excelFormat)
    {
        IWorkbook wb;
        if (fi.Exists)
        {
            Stream openStream = fi.Open(mode: FileMode.Open);
            if (excelFormat == ExcelFormat.XLSX)
            {
                wb = new XSSFWorkbook(fileStream: openStream);
            }
            else
            {
                wb = new HSSFWorkbook(s: openStream);
            }
            openStream.Close();
        }
        else
        {
            if (excelFormat == ExcelFormat.XLSX)
            {
                wb = new XSSFWorkbook();
            }
            else
            {
                wb = new HSSFWorkbook();
            }
        }
        return wb;
    }

    private static void EmptySheet(ISheet sheet)
    {
        for (int i = 0; i < sheet.PhysicalNumberOfRows; i++)
        {
            IRow xlRow = sheet.GetRow(rownum: i);
            foreach (ICell cell in xlRow.Cells)
            {
                xlRow.RemoveCell(cell: cell);
            }
        }
    }

    public static ICell SetCellValue(
        IRow excelRow,
        int i,
        TextReaderOptionsField fieldOptions,
        object val,
        ICellStyle dateCellStyle
    )
    {
        ICell cell = excelRow.CreateCell(column: i);
        if (val is DateTime)
        {
            DateTime date = (DateTime)val;
            if (fieldOptions != null && fieldOptions.Format != null)
            {
                string resultDate = date.ToString(
                    format: fieldOptions.Format,
                    provider: fieldOptions.GetCulture()
                );
                cell.SetCellValue(value: resultDate);
            }
            else
            {
                cell.SetCellValue(value: date);
                cell.CellStyle = dateCellStyle;
            }
        }
        else if (val is int || val is double || val is float || val is decimal)
        {
            cell.SetCellValue(value: Convert.ToDouble(value: val));
        }
        else if (val is bool)
        {
            cell.SetCellValue(value: (bool)val);
        }
        else if (val != null)
        {
            string fieldValue = val.ToString();
            if (fieldValue.IndexOf(value: "\r") > 0)
            {
                fieldValue = fieldValue.Replace(oldValue: "\n", newValue: "");
                fieldValue = fieldValue.Replace(oldValue: "\r", newValue: Environment.NewLine);
                fieldValue = fieldValue.Replace(oldValue: "\t", newValue: " ");
                cell.SetCellValue(value: fieldValue);
            }
            else
            {
                cell.SetCellValue(value: fieldValue);
            }
        }
        return cell;
    }
}
