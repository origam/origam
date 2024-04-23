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
using Origam.Schema;
using Origam.Excel;
using Origam.Rule.Xslt;

namespace Origam.BI.Excel;

public class ExcelService : IReportService
{
    public enum CellType
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

    public ExcelService()
    {
        }

    public object GetReport(Guid reportId, IXmlContainer data, string format,
        Hashtable parameters, string dbTransaction)
    {
            if (format != DataReportExportFormatType.MSExcel.ToString())
            {
                throw new ArgumentOutOfRangeException("format", format,
                    Properties.Resources.FormatNotSupported);
            }
            var report = ReportHelper.GetReportElement<AbstractDataReport>(reportId);
            using (LanguageSwitcher langSwitcher =
                new LanguageSwitcher(ReportHelper.ResolveLanguage(data, report)))
            {
                ReportHelper.PopulateDefaultValues(report, parameters);
                ReportHelper.ComputeXsltValueParameters(report, parameters);
                OrigamSpreadsheet spreadsheetData = GetSpreadsheetData(report, data, parameters, dbTransaction);
                if (spreadsheetData.Workbook.Rows.Count != 1)
                {
                    throw new Exception("Source data expect to contain a single Workbook.");
                }
                OrigamSpreadsheet.WorkbookRow sourceWorkbook = spreadsheetData.Workbook.Rows[0]
                    as OrigamSpreadsheet.WorkbookRow;
                OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
                FileInfo fi = new FileInfo(Path.Combine(settings.ReportsFolder(), report.ReportFileName));
                if (fi.Exists)
                {
                    Stream openStream = fi.Open(FileMode.Open);
                    IWorkbook wb = ExcelTools.GetWorkbook(
                        ExcelTools.StringToExcelFormat(
                            fi.Extension.Replace(".", "")),
                        openStream);
                    foreach (OrigamSpreadsheet.SheetRow sourceSheet in
                        sourceWorkbook.GetSheetRows())
                    {
                        ProcessSheet(wb, sourceSheet);
                    }
                    SetWorkbookProperties(sourceWorkbook, wb);
                    MemoryStream ms = new MemoryStream();
                    wb.Write(ms, false);
                    return ms.ToArray();
                }
                else
                {
                    throw new Exception("Could not open " + report.ReportFileName);
                }
            }
        }

    private static void SetWorkbookProperties(OrigamSpreadsheet.WorkbookRow sourceWorkbook, IWorkbook wb)
    {
            if (!sourceWorkbook.IsActiveSheetIndexNull())
            {
                wb.SetActiveSheet(sourceWorkbook.ActiveSheetIndex);
            }
        }

    private static void ProcessSheet(IWorkbook wb, OrigamSpreadsheet.SheetRow sourceSheet)
    {
            ISheet sheet = wb.GetSheet(sourceSheet.SheetName);
            if (sheet == null)
            {
                // sheet does not exist, we create it
                sheet = wb.CreateSheet(sourceSheet.SheetName);
            }
            SetSheetProperties(sourceSheet, sheet);
            foreach (OrigamSpreadsheet.RowRow sourceRow in sourceSheet.GetRowRows())
            {
                ProcessRow(sheet, sourceRow);
            }
        }

    private static void SetSheetProperties(OrigamSpreadsheet.SheetRow sourceSheet, ISheet sheet)
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
            if (!sourceSheet.IsFreezeRowIndexNull() && !sourceSheet.IsFreezeColumnIndexNull())
            {
                sheet.CreateFreezePane(sourceSheet.FreezeColumnIndex, sourceSheet.FreezeRowIndex);
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

    private static void ProcessRow(ISheet sheet, OrigamSpreadsheet.RowRow sourceRow)
    {
            IRow row = sheet.GetRow(sourceRow.Index);
            if (row == null)
            {
                row = sheet.CreateRow(sourceRow.Index);
            }
            SetRowProperties(sourceRow, row);
            foreach (OrigamSpreadsheet.CellRow sourceCell in sourceRow.GetCellRows())
            {
                ProcessCell(row, sourceCell);
            }
        }

    private static void SetRowProperties(OrigamSpreadsheet.RowRow sourceRow, IRow row)
    {
        }

    private static void ProcessCell(IRow row, OrigamSpreadsheet.CellRow sourceCell)
    {
            ICell cell = row.GetCell(sourceCell.Index);
            if (cell == null)
            {
                cell = row.CreateCell(sourceCell.Index);
            }
            SetCellProperties(sourceCell, cell);
        }

    private static void SetCellProperties(OrigamSpreadsheet.CellRow sourceCell, ICell cell)
    {
            string value = null;
            if (!sourceCell.IsValueNull())
            {
                value = sourceCell.Value;
            }
            CellType newType = (CellType)Enum.ToObject(typeof(CellType), sourceCell.Type);
            try
            {
                switch (newType)
                {
                    case CellType.NUMERIC:
                        cell.SetCellType(NPOI.SS.UserModel.CellType.Numeric);
                        cell.SetCellValue(XmlConvert.ToDouble(value));
                        break;
                    case CellType.FORMULA:
                        cell.SetCellType(NPOI.SS.UserModel.CellType.Formula);
                        cell.SetCellFormula(value);
                        break;
                    case CellType.Unknown:
                        cell.SetCellType(NPOI.SS.UserModel.CellType.Unknown);
                        cell.SetCellValue(value);
                        break;
                    case CellType.STRING:
                        cell.SetCellType(NPOI.SS.UserModel.CellType.String);
                        cell.SetCellValue(value);
                        break;
                    case CellType.BLANK:
                        cell.SetCellType(NPOI.SS.UserModel.CellType.Blank);
                        break;
                    case CellType.BOOLEAN:
                        cell.SetCellType(NPOI.SS.UserModel.CellType.Boolean);
                        cell.SetCellValue(XmlConvert.ToBoolean(value));
                        break;
                    case CellType.ERROR:
                        cell.SetCellType(NPOI.SS.UserModel.CellType.Error);
                        cell.SetCellErrorValue(XmlConvert.ToByte(value));
                        break;
                    case CellType.DATE:
                        cell.SetCellValue(XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Local));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (NullReferenceException ex)
            {
                throw new ArgumentException($"Value \"${value}\", from sheet ${cell.Sheet.SheetName}, row index: ${cell.RowIndex}, column index: ${cell.ColumnIndex} could not be converted to ${newType}", ex);
            }
        }
        
 

    private static OrigamSpreadsheet GetSpreadsheetData(AbstractDataReport report,
        IXmlContainer data, Hashtable parameters, string dbTransaction)
    {
            IDataDocument xmlDataDoc =
                ReportHelper.LoadOrUseReportData(report, data, parameters, dbTransaction);
            // optional xslt transformation
            OrigamSpreadsheet spreadsheetData = null;
            if (report.Transformation != null)
            {
                IPersistenceService persistence = ServiceManager.Services.GetService(
                    typeof(IPersistenceService)) as IPersistenceService;
                DataStructure outputDs = persistence.SchemaProvider.RetrieveInstance(
                    typeof(DataStructure), new ModelElementKey(
                        new Guid("c131aa04-6310-455d-a7cd-4e19dd012241")))
                    as DataStructure;
                IXsltEngine transformer = AsTransform.GetXsltEngine(
                    persistence.SchemaProvider, report.TransformationId);
                IDataDocument resultDoc = transformer.Transform(xmlDataDoc,
                    report.TransformationId, parameters, null, outputDs, false)
                    as IDataDocument;
                spreadsheetData = resultDoc.DataSet as OrigamSpreadsheet;
            }
            else
            {
                if (!(xmlDataDoc.DataSet is OrigamSpreadsheet))
                {
                    throw new InvalidCastException("Data source for ExcelReport must be OrigamSpreadsheet.");
                }
                spreadsheetData = xmlDataDoc.DataSet as OrigamSpreadsheet;
            }
            return spreadsheetData;
        }

    public void PrintReport(Guid reportId, IXmlContainer data,
        string printerName, int copies, Hashtable parameters)
    {
            throw new NotSupportedException();
        }
    public void SetTraceTaskInfo(TraceTaskInfo traceTaskInfo)
    {
            // do nothing unless we want something to trace
        }

    public string PrepareExternalReportViewer(Guid reportId,
        IXmlContainer data, string format, Hashtable parameters,
        string dbTransaction)
    {
            throw new NotImplementedException();
        }
}