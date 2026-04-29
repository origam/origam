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
using System.IO;
using System.Xml;
using NPOI.SS.UserModel;
using Origam.Excel;
using Origam.Rule.Xslt;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using Origam.Workflow.FileService;

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
        DATE = 100,
    }

    public object GetReport(
        Guid reportId,
        IXmlContainer data,
        string format,
        Hashtable parameters,
        string dbTransaction
    )
    {
        parameters ??= new Hashtable();
        if (format != DataReportExportFormatType.MSExcel.ToString())
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(format),
                actualValue: format,
                message: Properties.Resources.FormatNotSupported
            );
        }
        var report = ReportHelper.GetReportElement<AbstractDataReport>(reportId: reportId);
        using var langSwitcher = new LanguageSwitcher(
            langIetf: ReportHelper.ResolveLanguage(doc: data, reportElement: report)
        );
        ReportHelper.PopulateDefaultValues(report: report, parameters: parameters);
        ReportHelper.ComputeXsltValueParameters(report: report, parameters: parameters);
        OrigamSpreadsheet spreadsheetData = GetSpreadsheetData(
            report: report,
            data: data,
            parameters: parameters,
            dbTransaction: dbTransaction
        );
        if (spreadsheetData.Workbook.Rows.Count != 1)
        {
            throw new Exception(message: "Source data expect to contain a single Workbook.");
        }
        var sourceWorkbook =
            spreadsheetData.Workbook.Rows[index: 0] as OrigamSpreadsheet.WorkbookRow;
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        var reportFileInfo = new FileInfo(
            fileName: Path.Combine(
                path1: settings.ReportsFolder(),
                path2: ReportHelper.ExpandCurlyBracketPlaceholdersWithParameters(
                    input: report.ReportFileName,
                    parameters: parameters
                )
            )
        );
        if (!IOTools.IsSubPathOf(path: reportFileInfo.FullName, basePath: settings.ReportsFolder()))
        {
            throw new Exception(message: Strings.PathNotOnReportPath);
        }
        if (!reportFileInfo.Exists)
        {
            throw new Exception(message: $"Could not open {report.ReportFileName}");
        }
        Stream inputStream = reportFileInfo.Open(mode: FileMode.Open);
        IWorkbook workbook = ExcelTools.GetWorkbook(
            excelFormat: ExcelTools.StringToExcelFormat(
                input: reportFileInfo.Extension.Replace(oldValue: ".", newValue: "")
            ),
            stream: inputStream
        );
        if (sourceWorkbook != null)
        {
            foreach (OrigamSpreadsheet.SheetRow sourceSheet in sourceWorkbook.GetSheetRows())
            {
                ProcessSheet(workbook: workbook, sourceSheet: sourceSheet);
            }
            SetWorkbookProperties(sourceWorkbook: sourceWorkbook, workbook: workbook);
        }
        var outputStream = new MemoryStream();
        workbook.Write(stream: outputStream);
        return outputStream.ToArray();
    }

    private static void SetWorkbookProperties(
        OrigamSpreadsheet.WorkbookRow sourceWorkbook,
        IWorkbook workbook
    )
    {
        if (!sourceWorkbook.IsActiveSheetIndexNull())
        {
            workbook.SetActiveSheet(sheetIndex: sourceWorkbook.ActiveSheetIndex);
        }
    }

    private static void ProcessSheet(IWorkbook workbook, OrigamSpreadsheet.SheetRow sourceSheet)
    {
        // sheet does not exist, we create it
        ISheet sheet =
            workbook.GetSheet(name: sourceSheet.SheetName)
            ?? workbook.CreateSheet(sheetname: sourceSheet.SheetName);
        SetSheetProperties(sourceSheet: sourceSheet, sheet: sheet);
        foreach (OrigamSpreadsheet.RowRow sourceRow in sourceSheet.GetRowRows())
        {
            ProcessRow(sheet: sheet, sourceRow: sourceRow);
        }
    }

    private static void SetSheetProperties(OrigamSpreadsheet.SheetRow sourceSheet, ISheet sheet)
    {
        if (!sourceSheet.IsTabColorIndexNull())
        {
            sheet.TabColorIndex = Convert.ToInt16(value: sourceSheet.TabColorIndex);
        }
        if (!sourceSheet.IsDefaultColumnWidthNull())
        {
            sheet.DefaultColumnWidth = sourceSheet.DefaultColumnWidth;
        }
        if (!sourceSheet.IsDefaultRowHeightNull())
        {
            sheet.DefaultRowHeight = Convert.ToInt16(value: sourceSheet.DefaultRowHeight);
        }
        if (!sourceSheet.IsHiddenNull())
        {
            sheet.Workbook.SetSheetHidden(
                sheetIx: sourceSheet.Index,
                hidden: sourceSheet.Hidden ? SheetVisibility.Hidden : SheetVisibility.Visible
            );
        }
        if (!sourceSheet.IsFreezeRowIndexNull() && !sourceSheet.IsFreezeColumnIndexNull())
        {
            sheet.CreateFreezePane(
                colSplit: sourceSheet.FreezeColumnIndex,
                rowSplit: sourceSheet.FreezeRowIndex
            );
        }
        else if (!sourceSheet.IsFreezeColumnIndexNull())
        {
            sheet.CreateFreezePane(colSplit: sourceSheet.FreezeColumnIndex, rowSplit: 0);
        }
        else if (!sourceSheet.IsFreezeRowIndexNull())
        {
            sheet.CreateFreezePane(colSplit: 0, rowSplit: sourceSheet.FreezeRowIndex);
        }
    }

    private static void ProcessRow(ISheet sheet, OrigamSpreadsheet.RowRow sourceRow)
    {
        IRow row =
            sheet.GetRow(rownum: sourceRow.Index) ?? sheet.CreateRow(rownum: sourceRow.Index);
        foreach (OrigamSpreadsheet.CellRow sourceCell in sourceRow.GetCellRows())
        {
            ProcessCell(row: row, sourceCell: sourceCell);
        }
    }

    private static void ProcessCell(IRow row, OrigamSpreadsheet.CellRow sourceCell)
    {
        ICell cell =
            row.GetCell(cellnum: sourceCell.Index) ?? row.CreateCell(column: sourceCell.Index);
        SetCellProperties(sourceCell: sourceCell, cell: cell);
    }

    private static void SetCellProperties(OrigamSpreadsheet.CellRow sourceCell, ICell cell)
    {
        string value = null;
        if (!sourceCell.IsValueNull())
        {
            value = sourceCell.Value;
        }
        var newType = (CellType)Enum.ToObject(enumType: typeof(CellType), value: sourceCell.Type);
        try
        {
            switch (newType)
            {
                case CellType.NUMERIC:
                {
                    cell.SetCellType(cellType: NPOI.SS.UserModel.CellType.Numeric);
                    cell.SetCellValue(value: XmlConvert.ToDouble(s: value));
                    break;
                }
                case CellType.FORMULA:
                {
                    cell.SetCellType(cellType: NPOI.SS.UserModel.CellType.Formula);
                    cell.SetCellFormula(formula: value);
                    break;
                }
                case CellType.Unknown:
                {
                    cell.SetCellType(cellType: NPOI.SS.UserModel.CellType.Unknown);
                    cell.SetCellValue(value: value);
                    break;
                }
                case CellType.STRING:
                {
                    cell.SetCellType(cellType: NPOI.SS.UserModel.CellType.String);
                    cell.SetCellValue(value: value);
                    break;
                }
                case CellType.BLANK:
                {
                    cell.SetCellType(cellType: NPOI.SS.UserModel.CellType.Blank);
                    break;
                }
                case CellType.BOOLEAN:
                {
                    cell.SetCellType(cellType: NPOI.SS.UserModel.CellType.Boolean);
                    cell.SetCellValue(value: XmlConvert.ToBoolean(s: value));
                    break;
                }
                case CellType.ERROR:
                {
                    cell.SetCellType(cellType: NPOI.SS.UserModel.CellType.Error);
                    cell.SetCellErrorValue(value: XmlConvert.ToByte(s: value));
                    break;
                }
                case CellType.DATE:
                {
                    cell.SetCellValue(
                        value: XmlConvert.ToDateTime(
                            s: value,
                            dateTimeOption: XmlDateTimeSerializationMode.Local
                        )
                    );
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
                message: $"Value \"${value}\", from sheet ${cell.Sheet.SheetName}, row index: ${cell.RowIndex}, column index: ${cell.ColumnIndex} could not be converted to ${newType}",
                innerException: ex
            );
        }
    }

    private static OrigamSpreadsheet GetSpreadsheetData(
        AbstractDataReport report,
        IXmlContainer data,
        Hashtable parameters,
        string dbTransaction
    )
    {
        IDataDocument xmlDataDoc = ReportHelper.LoadOrUseReportData(
            report: report,
            data: data,
            parameters: parameters,
            dbTransaction: dbTransaction
        );
        // optional xslt transformation
        OrigamSpreadsheet spreadsheetData;
        if (report.Transformation != null)
        {
            var persistence = ServiceManager.Services.GetService<IPersistenceService>();
            var outputDataStructure = persistence.SchemaProvider.RetrieveInstance<DataStructure>(
                instanceId: new Guid(g: "c131aa04-6310-455d-a7cd-4e19dd012241")
            );
            IXsltEngine transformer = new CompiledXsltEngine(
                persistence: persistence.SchemaProvider
            );
            IDataDocument resultDoc =
                transformer.Transform(
                    data: xmlDataDoc,
                    transformationId: report.TransformationId,
                    parameters: parameters,
                    transactionId: null,
                    outputStructure: outputDataStructure,
                    validateOnly: false
                ) as IDataDocument;
            spreadsheetData = resultDoc?.DataSet as OrigamSpreadsheet;
        }
        else
        {
            if (xmlDataDoc.DataSet is not OrigamSpreadsheet dataSet)
            {
                throw new InvalidCastException(
                    message: "Data source for ExcelReport must be OrigamSpreadsheet."
                );
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
        Hashtable parameters
    )
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
        string dbTransaction
    )
    {
        throw new NotImplementedException();
    }
}
