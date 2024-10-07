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
using Origam.Schema.GuiModel;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Drawing.Printing;
using System.IO;
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
        Hashtable parameters)
    {
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId);
        if (data is not (IDataDocument or null))
        {
            throw new ArgumentOutOfRangeException(nameof(data), data,
                ResourceUtils.GetString("OnlyXmlDocSupported"));
        }
        DataSet dataset = null;
        if (data is IDataDocument dataDocument)
        {
            dataset = dataDocument.DataSet;
        }
        ReportDocument.EnableEventLog(EventLogLevel.LogEngineErrors);
        System.Diagnostics.Debug.WriteLine(ReportDocument.GetConcurrentUsage(),
            category: "Crystal Reports Concurrent Usage");
        using var languageSwitcher = new LanguageSwitcher(
            langIetf:ReportHelper.ResolveLanguage(data, report));
        using ReportDocument reportDoc = crystalReportHelper.CreateReport(
            report.Id, dataset, parameters);
        printerName ??= new PrinterSettings().PrinterName;
        ReportHelper.LogInfo(
            System.Reflection.MethodBase.GetCurrentMethod()
                ?.DeclaringType,
            $"Printing report '{report.Name}' to printer '{printerName}'");
        var printLayout = new PrintLayoutSettings();
        var printerSettings = new PrinterSettings
        {
            Copies = Convert.ToInt16(copies),
            PrinterName = printerName
        };
        reportDoc.PrintOptions.PrinterDuplex = PrinterDuplex.Simplex;
        var pageSettings = new PageSettings(printerSettings);
        reportDoc.PrintOptions.DissociatePageSizeAndPrinterPaperSize = true;
        reportDoc.PrintToPrinter(printerSettings, pageSettings, 
            reformatReportPageSettings: false, printLayout);
        reportDoc.Close();
    }

    public object GetReport(
        Guid reportId, 
        IXmlContainer data, 
        string format,
        Hashtable parameters, 
        string dbTransaction)
    {
        var report = ReportHelper.GetReportElement<AbstractDataReport>(
            reportId);
        IDataDocument xmlDataDoc = ReportHelper.LoadOrUseReportData(
            report, data, parameters, dbTransaction);
        DataSet dataset = xmlDataDoc.DataSet;
        using var languageSwitcher = new LanguageSwitcher(
                langIetf: ReportHelper.ResolveLanguage(xmlDataDoc, report));
        using ReportDocument reportDoc = crystalReportHelper.CreateReport(
            report.Id, dataset, parameters);
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
            _ => throw new ArgumentOutOfRangeException(nameof(format), format,
                ResourceUtils.GetString("FormatNotSupported"))
        };
        ReportHelper.LogInfo(
            System.Reflection.MethodBase.GetCurrentMethod()
                ?.DeclaringType,
            $"Exporting report '{report.Name}' to {format}");
        Stream stream = reportDoc.ExportToStream(type);
        var result = new byte[stream.Length];
        stream.Read(result, offset: 0, count: Convert.ToInt32(stream.Length));
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
        string dbTransaction)
    {
        throw new NotImplementedException();
    }
}