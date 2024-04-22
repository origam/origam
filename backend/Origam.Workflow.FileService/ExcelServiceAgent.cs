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
using System.IO;
using System.Xml;
using System.Data;
using System.Collections;
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

    public ExcelAgent()
    {
        }

    private IDataDocument ReadSheet(
        ExcelFormat excelFormat, string fileName, string sheetName,
        string entity, XmlDocument optionsXml, byte[] file)
    {
            IWorkbook wb;
            TextReaderOptions options = TextReaderOptions.Deserialize(optionsXml);
            if(options == null)
            {
                throw new Exception("Options not supplied.");
            }
            if(file == null)
            {
                wb = ExcelTools.LoadFile(fileName, excelFormat);
            }
            else
            {
                // read from variable
                MemoryStream stream = new MemoryStream(file);
                if(excelFormat == ExcelFormat.XLSX)
                {
                    wb = new XSSFWorkbook(stream);
                }
                else
                {
                    wb = new HSSFWorkbook(stream);
                }
            }
            ISheet[] sheets = ExcelTools.Sheets(sheetName, wb);
            DataSet data = CreateEmptyOutputData();
            DataTable table = data.Tables[entity];
            if(table == null)
            {
                throw new Exception("Entity '" + entity + "' not found in data structure " + this.OutputStructure.Name);
            }
            foreach(ISheet sheet in sheets)
            {
                if(sheet.PhysicalNumberOfRows > 0)
                {
                    // read header
                    IRow headerRow = sheet.GetRow(0);
                    Hashtable columns = new Hashtable(headerRow.PhysicalNumberOfCells);
                    foreach(ICell cell in headerRow)
                    {
                        try
                        {
                            DataColumn col = LookupColumnByCaption(table, cell.StringCellValue);
                            columns[cell.ColumnIndex] = col;
                        }
                        catch(Exception ex)
                        {
                            throw new Exception("Error occured while reading a header column "
                                + (cell.ColumnIndex + 1).ToString()
                                + ". Sheet: " + sheet.SheetName, ex);
                        }
                    }

                    foreach(IRow xlRow in sheet)
                    {
                        if(xlRow.RowNum >= (sheet.PhysicalNumberOfRows - options.IgnoreLast)) break;
                        if(xlRow.RowNum != 0 && xlRow.RowNum > options.IgnoreFirst) // skip header row
                        {
                            DataRow row = table.NewRow();
                            // handle guid primary key
                            DatasetTools.ApplyPrimaryKey(row);
                            foreach(ICell cell in xlRow)
                            {
                                try
                                {
                                    DataColumn col = (DataColumn)columns[cell.ColumnIndex];
                                    if(col != null)
                                    {
                                        ExcelTools.ReadValue(options, row, cell, col);
                                    }
                                }
                                catch(Exception ex)
                                {
                                    throw new Exception("Error occured while reading row "
                                        + (xlRow.RowNum + 1).ToString()
                                        + " column " + (cell.ColumnIndex + 1).ToString()
                                        + " sheet: " + sheet.SheetName
                                        + ". "
                                        + ex.Message,
                                        ex);
                                }
                            }
                            try
                            {
                                table.Rows.Add(row);
                            }
                            catch(Exception ex)
                            {
                                throw new Exception("Error occured while reading row "
                                    + (xlRow.RowNum + 1).ToString()
                                    + " sheet: " + sheet.SheetName
                                    + ". "
                                    + ex.Message,
                                    ex);
                            }
                        }
                    }
                }
            }
            return DataDocumentFactory.New(data);
        }

    private DataColumn LookupColumnByCaption(DataTable table, string caption)
    {
            foreach(DataColumn col in table.Columns)
            {
                if(col.Caption == caption) return col;
            }

            return null;
        }

    private void UpdateSheet(
        ExcelFormat excelFormat, string fileName, string sheetName,
        string entity, IDataDocument data, XmlDocument optionsXml)
    {
            TextReaderOptions options = TextReaderOptions.Deserialize(optionsXml);
            IWorkbook wb;
            FileInfo fi = new FileInfo(fileName);
            wb = ExcelTools.OpenOrCreateWorkbook(fi, excelFormat);
            ISheet sheet = ExcelTools.CreateOrEmptySheet(sheetName, wb);
            DataTable table = data.DataSet.Tables[entity];
            // CREATE CELL STYLES
            // So they can be reused later. There is a limit of 4000 cell styles in Excel 2003 and earlier, so
            // we have to create only as many styles as neccessary.
            _dateCellStyle = wb.CreateCellStyle();
            _dateCellStyle.DataFormat
                = wb.CreateDataFormat().GetFormat("m/d/yy h:mm");
            ArrayList columnNamesSorted = new ArrayList();
            foreach(DataColumn col in table.Columns)
            {
                columnNamesSorted.Add(col.ColumnName);
            }
            columnNamesSorted.Sort();
            // header row
            IRow headerRow = sheet.CreateRow(0);
            for(int i = 0; i < columnNamesSorted.Count; i++)
            {
                DataColumn column = table.Columns[(string)columnNamesSorted[i]];
                headerRow.CreateCell(i).SetCellValue(column.Caption);
            }
            // data rows
            for(int rowNumber = 0; rowNumber < table.Rows.Count; rowNumber++)
            {
                IRow excelRow = sheet.CreateRow(rowNumber + 1);
                DataRow row = table.Rows[rowNumber];
                for(int i = 0; i < columnNamesSorted.Count; i++)
                {
                    DataColumn column = table.Columns[(string)columnNamesSorted[i]];
                    TextReaderOptionsField fieldOptions = options.GetFieldOption(column.ColumnName);
                    object val = row[column];
                    ExcelTools.SetCellValue(excelRow, i, fieldOptions,
                        val, _dateCellStyle);
                }
            }
            ExcelTools.SaveWorkbook(wb, fi);
        }

    #region IServiceAgent Members
    private object _result;
    public override object Result
    {
        get
        {
                return _result;
            }
    }

    public override void Run()
    {
            switch(this.MethodName)
            {
                case "ReadSheet":
                    byte[] file = null;

                    // Check input parameters
                    if(this.Parameters.Contains("File") && this.Parameters["File"] is byte[])
                        file = (byte[])this.Parameters["File"];

                    if(!(this.Parameters["Entity"] is string | this.Parameters["Entity"] == null))
                        throw new InvalidCastException(ResourceUtils.GetString("ErrorViewNameNotString"));

                    _result =
                        this.ReadSheet(
                            this.Parameters["Format"] is String ?
                                ExcelTools.StringToExcelFormat(
                                    this.Parameters["Format"] as String)
                                    : ExcelFormat.XLS,
                            this.Parameters["FileName"] as String,
                            this.Parameters["SheetName"] as String,
                            this.Parameters["Entity"] as String,
                            this.Parameters["Options"] as XmlDocument,
                            file);

                    break;

                case "WriteSheet":
                    // Check input parameters
                    if(!(this.Parameters["FileName"] is string))
                        throw new InvalidCastException(ResourceUtils.GetString("ErrorListNameNotString"));

                    if(!(this.Parameters["SheetName"] is string | this.Parameters["SheetName"] == null))
                        throw new InvalidCastException(ResourceUtils.GetString("ErrorViewNameNotString"));

                    if(!(this.Parameters["Entity"] is string | this.Parameters["Entity"] == null))
                        throw new InvalidCastException(ResourceUtils.GetString("ErrorViewNameNotString"));

                    if(!(this.Parameters["Data"] is IDataDocument))
                        throw new InvalidCastException(ResourceUtils.GetString("ErrorDataNotXml"));

                    this.UpdateSheet(
                        this.Parameters["Format"] is String ?
                            ExcelTools.StringToExcelFormat(
                                this.Parameters["Format"] as String)
                                : ExcelFormat.XLS,
                        this.Parameters["FileName"] as String,
                        this.Parameters["SheetName"] as String,
                        this.Parameters["Entity"] as String,
                        this.Parameters["Data"] as IDataDocument,
                        this.Parameters["Options"] as XmlDocument);

                    _result = null;

                    break;
            }
        }
    #endregion
}