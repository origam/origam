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
            return new XSSFWorkbook(stream);
        }

        return new HSSFWorkbook(stream);
    }

    public static void SetWorkbookSubject(IWorkbook workbook, string subject)
    {
        if (workbook is HSSFWorkbook)
        {
            SetHSSFWorkbookSubject(workbook as HSSFWorkbook, subject);
        }
        else if (workbook is XSSFWorkbook)
        {
            SetXSSFWorkbookSubject(workbook as XSSFWorkbook, subject);
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
        return Sheets(null, wb);
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
                sheets[i] = wb.GetSheetAt(i);
            }
        }
        else
        {
            // get one sheet
            ISheet sheet = wb.GetSheet(sheetName);
            if (sheet == null)
            {
                throw new ArgumentOutOfRangeException(
                    "sheetName",
                    sheetName,
                    ResourceUtils.GetString("ExcelSheetNotFound")
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
        FileInfo fi = new FileInfo(fileName);
        FileStream stream = null;
        if (fi.Exists)
        {
            stream = fi.Open(FileMode.Open);
            if (excelFormat == ExcelFormat.XLSX)
            {
                wb = new XSSFWorkbook(stream);
            }
            else
            {
                wb = new HSSFWorkbook(stream);
            }
            stream.Close();
        }
        else
        {
            throw new FileNotFoundException(ResourceUtils.GetString("ExcelFileNotFound"), fileName);
        }
        return wb;
    }

    public static void ReadValue(TextReaderOptions options, DataRow row, ICell cell, DataColumn col)
    {
        TextReaderOptionsField fieldOptions = options.GetFieldOption(col.ColumnName);
        CellType cellType =
            cell.CellType == CellType.Formula ? cell.CachedFormulaResultType : cell.CellType;
        if (cellType == CellType.Blank)
        {
            if (fieldOptions?.NullValue != null)
            {
                // we always assign a value if NullValue was specified
                row[col] = Convert.ChangeType(fieldOptions.NullValue, col.DataType);
            }
            else if (col.AllowDBNull || col.DefaultValue != null)
            {
                // the value is empty or a default was used
                return;
            }
            else
            {
                throw new NoNullAllowedException(
                    string.Format(
                        "Cell {0},{1} is empty but field {2} does not allow nulls.",
                        cell.RowIndex + 1,
                        cell.ColumnIndex + 1,
                        col.ColumnName
                    )
                );
            }
        }
        if (col.DataType == typeof(string))
        {
            ReadStringValue(row, cellType, cell, col);
        }
        else if (col.DataType == typeof(DateTime))
        {
            ReadDateValue(fieldOptions, row, cellType, cell, col);
        }
        else if (col.DataType == typeof(Guid))
        {
            ReadGuidValue(row, cell, col);
        }
        else if (col.DataType == typeof(bool))
        {
            ReadBoolValue(row, cellType, cell, col);
        }
        else if (col.DataType == typeof(int))
        {
            ReadIntValue(row, cellType, cell, col);
        }
        else if (col.DataType == typeof(float))
        {
            ReadDecimalValue(fieldOptions, row, cellType, cell, col);
        }
        else if (col.DataType == typeof(decimal))
        {
            ReadDecimalValue(fieldOptions, row, cellType, cell, col);
        }
        else if (col.DataType == typeof(long))
        {
            ReadLongValue(fieldOptions, row, cellType, cell, col);
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
            row[col] = cell.NumericCellValue;
        }
        else if (fieldOptions != null)
        {
            row[col] = long.Parse(cell.StringCellValue, fieldOptions.GetCulture());
        }
        else
        {
            row[col] = long.Parse(cell.StringCellValue);
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
            row[col] = cell.NumericCellValue;
        }
        else if (fieldOptions != null)
        {
            row[col] = decimal.Parse(cell.StringCellValue, fieldOptions.GetCulture());
        }
        else
        {
            row[col] = decimal.Parse(cell.StringCellValue);
        }
    }

    private static void ReadIntValue(DataRow row, CellType cellType, ICell cell, DataColumn col)
    {
        if (cellType == CellType.Numeric)
        {
            row[col] = cell.NumericCellValue;
        }
        else
        {
            row[col] = int.Parse(cell.StringCellValue);
        }
    }

    private static void ReadBoolValue(DataRow row, CellType cellType, ICell cell, DataColumn col)
    {
        if (cellType == CellType.Boolean)
        {
            row[col] = cell.BooleanCellValue;
        }
        else
        {
            row[col] = bool.Parse(cell.StringCellValue);
        }
    }

    private static void ReadGuidValue(DataRow row, ICell cell, DataColumn col)
    {
        if (cell.StringCellValue != "")
        {
            row[col] = new Guid(cell.StringCellValue);
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
            row[col] = cell.DateCellValue;
        }
        else if (cellType == CellType.String)
        {
            if (fieldOptions != null && fieldOptions.Format != null && cell.StringCellValue != "")
            {
                row[col] = DateTime.ParseExact(
                    cell.StringCellValue,
                    fieldOptions.Formats,
                    fieldOptions.GetCulture(),
                    System.Globalization.DateTimeStyles.None
                );
            }
        }
    }

    private static void ReadStringValue(DataRow row, CellType cellType, ICell cell, DataColumn col)
    {
        if (cellType == CellType.Numeric)
        {
            row[col] = cell.NumericCellValue.ToString();
        }
        else
        {
            row[col] = cell.StringCellValue;
        }
    }

    public static void SaveWorkbook(IWorkbook wb, FileInfo fi)
    {
        FileStream stream = null;
        if (fi.Exists)
        {
            stream = fi.Open(FileMode.Open);
        }
        else
        {
            stream = fi.Open(FileMode.Create);
        }
        // recalculate formulas on all sheets
        for (int i = 0; i < wb.NumberOfSheets; i++)
        {
            wb.GetSheetAt(i).ForceFormulaRecalculation = true;
        }
        wb.Write(stream, false);
        stream.Close();
    }

    public static ISheet CreateOrEmptySheet(string sheetName, IWorkbook wb)
    {
        ISheet sheet = wb.GetSheet(sheetName);
        if (sheet == null)
        {
            // sheet does not exist, we create it
            sheet = wb.CreateSheet(sheetName);
        }
        else
        {
            // sheet exists already, we empty it
            EmptySheet(sheet);
        }
        return sheet;
    }

    public static IWorkbook OpenOrCreateWorkbook(FileInfo fi, ExcelFormat excelFormat)
    {
        IWorkbook wb;
        if (fi.Exists)
        {
            Stream openStream = fi.Open(FileMode.Open);
            if (excelFormat == ExcelFormat.XLSX)
            {
                wb = new XSSFWorkbook(openStream);
            }
            else
            {
                wb = new HSSFWorkbook(openStream);
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
            IRow xlRow = sheet.GetRow(i);
            foreach (ICell cell in xlRow.Cells)
            {
                xlRow.RemoveCell(cell);
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
        ICell cell = excelRow.CreateCell(i);
        if (val is DateTime)
        {
            DateTime date = (DateTime)val;
            if (fieldOptions != null && fieldOptions.Format != null)
            {
                string resultDate = date.ToString(fieldOptions.Format, fieldOptions.GetCulture());
                cell.SetCellValue(resultDate);
            }
            else
            {
                cell.SetCellValue(date);
                cell.CellStyle = dateCellStyle;
            }
        }
        else if (val is int || val is double || val is float || val is decimal)
        {
            cell.SetCellValue(Convert.ToDouble(val));
        }
        else if (val is bool)
        {
            cell.SetCellValue((bool)val);
        }
        else if (val != null)
        {
            string fieldValue = val.ToString();
            if (fieldValue.IndexOf("\r") > 0)
            {
                fieldValue = fieldValue.Replace("\n", "");
                fieldValue = fieldValue.Replace("\r", Environment.NewLine);
                fieldValue = fieldValue.Replace("\t", " ");
                cell.SetCellValue(fieldValue);
            }
            else
            {
                cell.SetCellValue(fieldValue);
            }
        }
        return cell;
    }
}
