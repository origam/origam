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
using System.Data;
using System.Drawing.Printing;
using System.IO;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using Origam.Schema.GuiModel;
using Origam.Service.Core;

namespace Origam.BI.CrystalReports;

public class CrystalReportService : IReportService
{
    private readonly CrystalReportHelper crystalReportHelper = new();

    public void PrintReport(
        Guid reportId,
        IXmlContainer data,
        string printerName,
        int copies,
        Hashtable parameters
    )
    {
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId: reportId);
        if (data is not (IDataDocument or null))
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(data),
                actualValue: data,
                message: ResourceUtils.GetString(key: "OnlyXmlDocSupported")
            );
        }
        DataSet dataset = null;
        if (data is IDataDocument dataDocument)
        {
            dataset = dataDocument.DataSet;
        }
        ReportDocument.EnableEventLog(elLevel: EventLogLevel.LogEngineErrors);
        System.Diagnostics.Debug.WriteLine(
            value: ReportDocument.GetConcurrentUsage(),
            category: "Crystal Reports Concurrent Usage"
        );
        using var languageSwitcher = new LanguageSwitcher(
            langIetf: ReportHelper.ResolveLanguage(doc: data, reportElement: report)
        );
        using ReportDocument reportDoc = crystalReportHelper.CreateReport(
            reportId: report.Id,
            data: dataset,
            parameters: parameters
        );
        printerName ??= new PrinterSettings().PrinterName;
        ReportHelper.LogInfo(
            type: System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType,
            message: $"Printing report '{report.Name}' to printer '{printerName}'"
        );
        var printLayout = new PrintLayoutSettings();
        var printerSettings = new PrinterSettings
        {
            Copies = Convert.ToInt16(value: copies),
            PrinterName = printerName,
        };
        reportDoc.PrintOptions.PrinterDuplex = PrinterDuplex.Simplex;
        var pageSettings = new PageSettings(printerSettings: printerSettings);
        reportDoc.PrintOptions.DissociatePageSizeAndPrinterPaperSize = true;
        reportDoc.PrintToPrinter(
            printerSettings: printerSettings,
            pageSettings: pageSettings,
            reformatReportPageSettings: false,
            layoutSettings: printLayout
        );
        reportDoc.Close();
    }

    public object GetReport(
        Guid reportId,
        IXmlContainer data,
        string format,
        Hashtable parameters,
        string dbTransaction
    )
    {
        var report = ReportHelper.GetReportElement<AbstractDataReport>(reportId: reportId);
        IDataDocument xmlDataDoc = ReportHelper.LoadOrUseReportData(
            report: report,
            data: data,
            parameters: parameters,
            dbTransaction: dbTransaction
        );
        DataSet dataset = xmlDataDoc.DataSet;
        using var languageSwitcher = new LanguageSwitcher(
            langIetf: ReportHelper.ResolveLanguage(doc: xmlDataDoc, reportElement: report)
        );
        using ReportDocument reportDoc = crystalReportHelper.CreateReport(
            reportId: report.Id,
            data: dataset,
            parameters: parameters
        );
        ExportFormatType type = format switch
        {
            "PDF" => ExportFormatType.PortableDocFormat,
            "MSExcel" => ExportFormatType.Excel,
            "RTF" => ExportFormatType.EditableRTF,
            "MSWord" => ExportFormatType.WordForWindows,
            "HTML" => ExportFormatType.HTML40,
            "CSV" => ExportFormatType.CharacterSeparatedValues,
            "TEXT" => ExportFormatType.Text,
            "XML" => ExportFormatType.Xml,
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(format),
                actualValue: format,
                message: ResourceUtils.GetString(key: "FormatNotSupported")
            ),
        };
        ReportHelper.LogInfo(
            type: System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType,
            message: $"Exporting report '{report.Name}' to {format}"
        );
        Stream stream = reportDoc.ExportToStream(formatType: type);
        var result = new byte[stream.Length];
        stream.Read(buffer: result, offset: 0, count: Convert.ToInt32(value: stream.Length));
        stream.Close();
        reportDoc.Close();
        return result;
    }

    public void SetTraceTaskInfo(TraceTaskInfo traceTaskInfo)
    {
        // do nothing unless we need to trace
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
