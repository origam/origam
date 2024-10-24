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
using System.Xml;
using System.Collections;
using System.IO;
using Origam.Service.Core;
using Origam.Schema.GuiModel;
using NPOI.SS.UserModel;
using Origam.Workflow.FileService;
using Origam.Workbench.Services;
using Origam.Rule;
using Origam.Schema.EntityModel;
using Origam.Excel;
using Origam.Rule.Xslt;

namespace Origam.BI.Excel;
public class ExcelService : IReportService
{
    private enum CellType
    {
        Unknown = -1,
        NUMERIC = 0,
        STRING = 1,
        FORMULA = 2,
        BLANK = 3,
        BOOLEAN = 4,
        ERROR = 5,
        DATE = 100
    }
    public object GetReport(
        Guid reportId, 
        IXmlContainer data, 
        string format,
        Hashtable parameters, 
        string dbTransaction)
    {
        parameters ??= new Hashtable();
        if (format != DataReportExportFormatType.MSExcel.ToString())
        {
            throw new ArgumentOutOfRangeException(nameof(format), format,
                Properties.Resources.FormatNotSupported);
        }
        var report = ReportHelper.GetReportElement<AbstractDataReport>(reportId);
        using var langSwitcher = new LanguageSwitcher(
            langIetf: ReportHelper.ResolveLanguage(data, report));
        ReportHelper.PopulateDefaultValues(report, parameters);
        ReportHelper.ComputeXsltValueParameters(report, parameters);
        OrigamSpreadsheet spreadsheetData = GetSpreadsheetData(
            report, data, parameters, dbTransaction);
        if (spreadsheetData.Workbook.Rows.Count != 1)
        {
            throw new Exception(
                "Source data expect to contain a single Workbook.");
        }
        var sourceWorkbook = spreadsheetData.Workbook.Rows[0] 
            as OrigamSpreadsheet.WorkbookRow;
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        var reportFileInfo = new FileInfo(Path.Combine(
            settings.ReportsFolder(), 
            ReportHelper.ExpandCurlyBracketPlaceholdersWithParameters(
                report.ReportFileName,
                parameters)));
        if (!IOTools.IsSubPathOf(reportFileInfo.FullName, settings.ReportsFolder()))
        {
            throw new Exception(Strings.PathNotOnReportPath);
        }
        if (!reportFileInfo.Exists)
        {
            throw new Exception($"Could not open {report.ReportFileName}");
        }
        Stream inputStream = reportFileInfo.Open(FileMode.Open);
        IWorkbook workbook = ExcelTools.GetWorkbook(
            ExcelTools.StringToExcelFormat(
                reportFileInfo.Extension.Replace(".", "")),
            inputStream);
        if (sourceWorkbook != null)
        {
            foreach (OrigamSpreadsheet.SheetRow sourceSheet 
                     in sourceWorkbook.GetSheetRows())
            {
                ProcessSheet(workbook, sourceSheet);
            }
            SetWorkbookProperties(sourceWorkbook, workbook);
        }
        var outputStream = new MemoryStream();
        workbook.Write(outputStream);
        return outputStream.ToArray();
    }
    private static void SetWorkbookProperties(
        OrigamSpreadsheet.WorkbookRow sourceWorkbook, 
        IWorkbook workbook)
    {
        if (!sourceWorkbook.IsActiveSheetIndexNull())
        {
            workbook.SetActiveSheet(sourceWorkbook.ActiveSheetIndex);
        }
    }
    private static void ProcessSheet(
        IWorkbook workbook, 
        OrigamSpreadsheet.SheetRow sourceSheet)
    {
        // sheet does not exist, we create it
        ISheet sheet = workbook.GetSheet(sourceSheet.SheetName) 
                       ?? workbook.CreateSheet(sourceSheet.SheetName);
        SetSheetProperties(sourceSheet, sheet);
        foreach (OrigamSpreadsheet.RowRow sourceRow in sourceSheet.GetRowRows())
        {
            ProcessRow(sheet, sourceRow);
        }
    }
    private static void SetSheetProperties(
        OrigamSpreadsheet.SheetRow sourceSheet, 
        ISheet sheet)
    {
        if (!sourceSheet.IsTabColorIndexNull())
        {
            sheet.TabColorIndex = Convert.ToInt16(sourceSheet.TabColorIndex);
        }
        if (!sourceSheet.IsDefaultColumnWidthNull())
        {
            sheet.DefaultColumnWidth = sourceSheet.DefaultColumnWidth;
        }
        if (!sourceSheet.IsDefaultRowHeightNull())
        {
            sheet.DefaultRowHeight = Convert.ToInt16(sourceSheet.DefaultRowHeight);
        }
        if (!sourceSheet.IsHiddenNull())
        {
            sheet.Workbook.SetSheetHidden(
                sourceSheet.Index, 
                sourceSheet.Hidden ? SheetState.Hidden : SheetState.Visible);
        }
        if (!sourceSheet.IsFreezeRowIndexNull() 
            && !sourceSheet.IsFreezeColumnIndexNull())
        {
            sheet.CreateFreezePane(
                sourceSheet.FreezeColumnIndex, sourceSheet.FreezeRowIndex);
        }
        else if (!sourceSheet.IsFreezeColumnIndexNull())
        {
            sheet.CreateFreezePane(sourceSheet.FreezeColumnIndex, 0);
        }
        else if (!sourceSheet.IsFreezeRowIndexNull())
        {
            sheet.CreateFreezePane(0, sourceSheet.FreezeRowIndex);
        }
    }
    private static void ProcessRow(
        ISheet sheet, 
        OrigamSpreadsheet.RowRow sourceRow)
    {
        IRow row = sheet.GetRow(sourceRow.Index) 
                   ?? sheet.CreateRow(sourceRow.Index);
        foreach (OrigamSpreadsheet.CellRow sourceCell 
                 in sourceRow.GetCellRows())
        {
            ProcessCell(row, sourceCell);
        }
    }
    private static void ProcessCell(
        IRow row, 
        OrigamSpreadsheet.CellRow sourceCell)
    {
        ICell cell = row.GetCell(sourceCell.Index) 
                     ?? row.CreateCell(sourceCell.Index);
        SetCellProperties(sourceCell, cell);
    }
    private static void SetCellProperties(
        OrigamSpreadsheet.CellRow sourceCell, 
        ICell cell)
    {
        string value = null;
        if (!sourceCell.IsValueNull())
        {
            value = sourceCell.Value;
        }
        var newType = (CellType)Enum.ToObject(
            typeof(CellType), sourceCell.Type);
        try
        {
            switch (newType)
            {
                case CellType.NUMERIC:
                {
                    cell.SetCellType(NPOI.SS.UserModel.CellType.Numeric);
                    cell.SetCellValue(XmlConvert.ToDouble(value));
                    break;
                }
                case CellType.FORMULA:
                {
                    cell.SetCellType(NPOI.SS.UserModel.CellType.Formula);
                    cell.SetCellFormula(value);
                    break;
                }
                case CellType.Unknown:
                {
                    cell.SetCellType(NPOI.SS.UserModel.CellType.Unknown);
                    cell.SetCellValue(value);
                    break;
                }
                case CellType.STRING:
                {
                    cell.SetCellType(NPOI.SS.UserModel.CellType.String);
                    cell.SetCellValue(value);
                    break;
                }
                case CellType.BLANK:
                {
                    cell.SetCellType(NPOI.SS.UserModel.CellType.Blank);
                    break;
                }
                case CellType.BOOLEAN:
                {
                    cell.SetCellType(NPOI.SS.UserModel.CellType.Boolean);
                    cell.SetCellValue(XmlConvert.ToBoolean(value));
                    break;
                }
                case CellType.ERROR:
                {
                    cell.SetCellType(NPOI.SS.UserModel.CellType.Error);
                    cell.SetCellErrorValue(XmlConvert.ToByte(value));
                    break;
                }
                case CellType.DATE:
                {
                    cell.SetCellValue(XmlConvert.ToDateTime(
                        value, XmlDateTimeSerializationMode.Local));
                    break;
                }
                default:
                {
                    throw new ArgumentOutOfRangeException();
                }
            }
        }
        catch (NullReferenceException ex)
        {
            throw new ArgumentException(
                $"Value \"${value}\", from sheet ${cell.Sheet.SheetName}, row index: ${cell.RowIndex}, column index: ${cell.ColumnIndex} could not be converted to ${newType}", ex);
        }
    }
    
    private static OrigamSpreadsheet GetSpreadsheetData(
        AbstractDataReport report,
        IXmlContainer data, 
        Hashtable parameters, 
        string dbTransaction)
    {
        IDataDocument xmlDataDoc = ReportHelper.LoadOrUseReportData(
            report, data, parameters, dbTransaction);
        // optional xslt transformation
        OrigamSpreadsheet spreadsheetData;
        if (report.Transformation != null)
        {
            var persistence 
                = ServiceManager.Services.GetService<IPersistenceService>();
            var outputDataStructure = persistence.SchemaProvider
                    .RetrieveInstance<DataStructure>(
                        new Guid("c131aa04-6310-455d-a7cd-4e19dd012241"));
            IXsltEngine transformer = AsTransform.GetXsltEngine(
                persistence.SchemaProvider, report.TransformationId);
            IDataDocument resultDoc = transformer.Transform(xmlDataDoc,
                report.TransformationId, parameters, transactionId: null, 
                outputDataStructure, validateOnly: false)
                as IDataDocument;
            spreadsheetData = resultDoc?.DataSet as OrigamSpreadsheet;
        }
        else
        {
            if (xmlDataDoc.DataSet is not OrigamSpreadsheet dataSet)
            {
                throw new InvalidCastException(
                    "Data source for ExcelReport must be OrigamSpreadsheet.");
            }
            spreadsheetData = dataSet;
        }
        return spreadsheetData;
    }
    public void PrintReport(
        Guid reportId, 
        IXmlContainer data,
        string printerName, 
        int copies, 
        Hashtable parameters)
    {
        throw new NotSupportedException();
    }
    public void SetTraceTaskInfo(TraceTaskInfo traceTaskInfo)
    {
        // do nothing unless we want something to trace
    }
    public string PrepareExternalReportViewer(
        Guid reportId,
        IXmlContainer data, 
        string format, 
        Hashtable parameters,
        string dbTransaction)
    {
        throw new NotImplementedException();
    }
}
