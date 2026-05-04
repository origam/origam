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
using System.IO;
using System.Xml;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using Origam.DA;
using Origam.Excel;
using Origam.Service.Core;

namespace Origam.Workflow.FileService;

/// <summary>
/// Summary description for ExcelService.
/// </summary>
public class ExcelAgent : AbstractServiceAgent
{
    private static ICellStyle _dateCellStyle;

    public ExcelAgent() { }

    private IDataDocument ReadSheet(
        ExcelFormat excelFormat,
        string fileName,
        string sheetName,
        string entity,
        XmlDocument optionsXml,
        byte[] file
    )
    {
        IWorkbook wb;
        TextReaderOptions options = TextReaderOptions.Deserialize(doc: optionsXml);
        if (options == null)
        {
            throw new Exception(message: "Options not supplied.");
        }
        if (file == null)
        {
            wb = ExcelTools.LoadFile(fileName: fileName, excelFormat: excelFormat);
        }
        else
        {
            // read from variable
            MemoryStream stream = new MemoryStream(buffer: file);
            if (excelFormat == ExcelFormat.XLSX)
            {
                wb = new XSSFWorkbook(fileStream: stream);
            }
            else
            {
                wb = new HSSFWorkbook(s: stream);
            }
        }
        ISheet[] sheets = ExcelTools.Sheets(sheetName: sheetName, wb: wb);
        DataSet data = CreateEmptyOutputData();
        DataTable table = data.Tables[name: entity];
        if (table == null)
        {
            throw new Exception(
                message: "Entity '"
                    + entity
                    + "' not found in data structure "
                    + this.OutputStructure.Name
            );
        }
        foreach (ISheet sheet in sheets)
        {
            if (sheet.PhysicalNumberOfRows > 0)
            {
                // read header
                IRow headerRow = sheet.GetRow(rownum: 0);
                Hashtable columns = new Hashtable(capacity: headerRow.PhysicalNumberOfCells);
                foreach (ICell cell in headerRow)
                {
                    try
                    {
                        DataColumn col = LookupColumnByCaption(
                            table: table,
                            caption: cell.StringCellValue
                        );
                        columns[key: cell.ColumnIndex] = col;
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(
                            message: "Error occured while reading a header column "
                                + (cell.ColumnIndex + 1).ToString()
                                + ". Sheet: "
                                + sheet.SheetName,
                            innerException: ex
                        );
                    }
                }
                foreach (IRow xlRow in sheet)
                {
                    if (xlRow.RowNum >= (sheet.PhysicalNumberOfRows - options.IgnoreLast))
                    {
                        break;
                    }

                    if (xlRow.RowNum != 0 && xlRow.RowNum > options.IgnoreFirst) // skip header row
                    {
                        DataRow row = table.NewRow();
                        // handle guid primary key
                        DatasetTools.ApplyPrimaryKey(row: row);
                        foreach (ICell cell in xlRow)
                        {
                            try
                            {
                                DataColumn col = (DataColumn)columns[key: cell.ColumnIndex];
                                if (col != null)
                                {
                                    ExcelTools.ReadValue(
                                        options: options,
                                        row: row,
                                        cell: cell,
                                        col: col
                                    );
                                }
                            }
                            catch (Exception ex)
                            {
                                throw new Exception(
                                    message: "Error occured while reading row "
                                        + (xlRow.RowNum + 1).ToString()
                                        + " column "
                                        + (cell.ColumnIndex + 1).ToString()
                                        + " sheet: "
                                        + sheet.SheetName
                                        + ". "
                                        + ex.Message,
                                    innerException: ex
                                );
                            }
                        }
                        try
                        {
                            table.Rows.Add(row: row);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception(
                                message: "Error occured while reading row "
                                    + (xlRow.RowNum + 1).ToString()
                                    + " sheet: "
                                    + sheet.SheetName
                                    + ". "
                                    + ex.Message,
                                innerException: ex
                            );
                        }
                    }
                }
            }
        }
        return DataDocumentFactory.New(dataSet: data);
    }

    private DataColumn LookupColumnByCaption(DataTable table, string caption)
    {
        foreach (DataColumn col in table.Columns)
        {
            if (col.Caption == caption)
            {
                return col;
            }
        }
        return null;
    }

    private void UpdateSheet(
        ExcelFormat excelFormat,
        string fileName,
        string sheetName,
        string entity,
        IDataDocument data,
        XmlDocument optionsXml
    )
    {
        TextReaderOptions options = TextReaderOptions.Deserialize(doc: optionsXml);
        IWorkbook wb;
        FileInfo fi = new FileInfo(fileName: fileName);
        wb = ExcelTools.OpenOrCreateWorkbook(fi: fi, excelFormat: excelFormat);
        ISheet sheet = ExcelTools.CreateOrEmptySheet(sheetName: sheetName, wb: wb);
        DataTable table = data.DataSet.Tables[name: entity];
        // CREATE CELL STYLES
        // So they can be reused later. There is a limit of 4000 cell styles in Excel 2003 and earlier, so
        // we have to create only as many styles as neccessary.
        _dateCellStyle = wb.CreateCellStyle();
        _dateCellStyle.DataFormat = wb.CreateDataFormat().GetFormat(format: "m/d/yy h:mm");
        var columnNamesSorted = new List<string>();
        foreach (DataColumn col in table.Columns)
        {
            columnNamesSorted.Add(item: col.ColumnName);
        }
        columnNamesSorted.Sort();
        // header row
        IRow headerRow = sheet.CreateRow(rownum: 0);
        for (int i = 0; i < columnNamesSorted.Count; i++)
        {
            DataColumn column = table.Columns[name: columnNamesSorted[index: i]];
            headerRow.CreateCell(column: i).SetCellValue(value: column.Caption);
        }
        // data rows
        for (int rowNumber = 0; rowNumber < table.Rows.Count; rowNumber++)
        {
            IRow excelRow = sheet.CreateRow(rownum: rowNumber + 1);
            DataRow row = table.Rows[index: rowNumber];
            for (int i = 0; i < columnNamesSorted.Count; i++)
            {
                DataColumn column = table.Columns[name: columnNamesSorted[index: i]];
                TextReaderOptionsField fieldOptions = options.GetFieldOption(
                    fieldName: column.ColumnName
                );
                object val = row[column: column];
                ExcelTools.SetCellValue(
                    excelRow: excelRow,
                    i: i,
                    fieldOptions: fieldOptions,
                    val: val,
                    dateCellStyle: _dateCellStyle
                );
            }
        }
        ExcelTools.SaveWorkbook(wb: wb, fi: fi);
    }

    #region IServiceAgent Members
    private object _result;
    public override object Result
    {
        get { return _result; }
    }

    public override void Run()
    {
        switch (this.MethodName)
        {
            case "ReadSheet":
            {
                byte[] file = null;
                // Check input parameters
                if (this.Parameters.Contains(key: "File") && this.Parameters[key: "File"] is byte[])
                {
                    file = (byte[])this.Parameters[key: "File"];
                }

                if (
                    !(
                        this.Parameters[key: "Entity"] is string
                        | this.Parameters[key: "Entity"] == null
                    )
                )
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorViewNameNotString")
                    );
                }

                _result = this.ReadSheet(
                    excelFormat: this.Parameters[key: "Format"] is String
                        ? ExcelTools.StringToExcelFormat(
                            input: this.Parameters[key: "Format"] as String
                        )
                        : ExcelFormat.XLS,
                    fileName: this.Parameters[key: "FileName"] as String,
                    sheetName: this.Parameters[key: "SheetName"] as String,
                    entity: this.Parameters[key: "Entity"] as String,
                    optionsXml: this.Parameters[key: "Options"] as XmlDocument,
                    file: file
                );
                break;
            }

            case "WriteSheet":
            {
                // Check input parameters
                if (!(this.Parameters[key: "FileName"] is string))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorListNameNotString")
                    );
                }

                if (
                    !(
                        this.Parameters[key: "SheetName"] is string
                        | this.Parameters[key: "SheetName"] == null
                    )
                )
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorViewNameNotString")
                    );
                }

                if (
                    !(
                        this.Parameters[key: "Entity"] is string
                        | this.Parameters[key: "Entity"] == null
                    )
                )
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorViewNameNotString")
                    );
                }

                if (!(this.Parameters[key: "Data"] is IDataDocument))
                {
                    throw new InvalidCastException(
                        message: ResourceUtils.GetString(key: "ErrorDataNotXml")
                    );
                }

                this.UpdateSheet(
                    excelFormat: this.Parameters[key: "Format"] is String
                        ? ExcelTools.StringToExcelFormat(
                            input: this.Parameters[key: "Format"] as String
                        )
                        : ExcelFormat.XLS,
                    fileName: this.Parameters[key: "FileName"] as String,
                    sheetName: this.Parameters[key: "SheetName"] as String,
                    entity: this.Parameters[key: "Entity"] as String,
                    data: this.Parameters[key: "Data"] as IDataDocument,
                    optionsXml: this.Parameters[key: "Options"] as XmlDocument
                );
                _result = null;
                break;
            }
        }
    }
    #endregion
}
