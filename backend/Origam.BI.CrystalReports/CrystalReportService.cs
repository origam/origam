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
using Origam.Schema.GuiModel;
using Origam.Service.Core;

namespace Origam.BI.CrystalReports;

public class CrystalReportService : IReportService
{
    private readonly CrystalReportHelper crystalReportHelper = new();

    #region IReportService Members
    public void PrintReport(
        Guid reportId,
        IXmlContainer data,
        string printerName,
        int copies,
        Hashtable parameters
    )
    {
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId: reportId);
        var xmlDataDoc = ReportHelper.LoadOrUseReportData(
            report: report,
            data: data,
            parameters: parameters,
            dbTransaction: null
        );
        using var langSwitcher = new LanguageSwitcher(
            langIetf: ReportHelper.ResolveLanguage(doc: xmlDataDoc, reportElement: report)
        );
        ReportHelper.LogInfo(
            type: System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType,
            message: $"Printing report '{report.Name}' to {printerName}"
        );
        crystalReportHelper.PrintReport(
            reportId: report.Id,
            data: xmlDataDoc.DataSet,
            parameters: parameters,
            printerName: printerName,
            copies: copies
        );
    }

    public object GetReport(
        Guid reportId,
        IXmlContainer data,
        string format,
        Hashtable parameters,
        string dbTransaction
    )
    {
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId: reportId);
        var xmlDataDoc = ReportHelper.LoadOrUseReportData(
            report: report,
            data: data,
            parameters: parameters,
            dbTransaction: dbTransaction
        );
        using var langSwitcher = new LanguageSwitcher(
            langIetf: ReportHelper.ResolveLanguage(doc: xmlDataDoc, reportElement: report)
        );
        ReportHelper.LogInfo(
            type: System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType,
            message: $"Exporting report '{report.Name}' to {format}"
        );
        return crystalReportHelper.CreateReport(
            reportId: report.Id,
            data: xmlDataDoc.DataSet,
            parameters: parameters,
            format: format
        );
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
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId: reportId);
        var xmlDataDoc = ReportHelper.LoadOrUseReportData(
            report: report,
            data: data,
            parameters: parameters,
            dbTransaction: dbTransaction
        );
        using var langSwitcher = new LanguageSwitcher(
            langIetf: ReportHelper.ResolveLanguage(doc: xmlDataDoc, reportElement: report)
        );
        ReportHelper.LogInfo(
            type: System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType,
            message: $"Exporting report '{report.Name}' to {format}"
        );
        return crystalReportHelper.PrepareReport(
            report: report,
            data: xmlDataDoc.DataSet,
            parameters: parameters,
            format: DataReportExportFormatType.RPT.ToString()
        );
    }
    #endregion
}
