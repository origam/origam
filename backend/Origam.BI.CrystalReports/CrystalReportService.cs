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
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId);
        var xmlDataDoc = ReportHelper.LoadOrUseReportData(
            report,
            data,
            parameters,
            dbTransaction: null
        );
        using var langSwitcher = new LanguageSwitcher(
            langIetf: ReportHelper.ResolveLanguage(xmlDataDoc, report)
        );
        ReportHelper.LogInfo(
            System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType,
            $"Printing report '{report.Name}' to {printerName}"
        );
        crystalReportHelper.PrintReport(
            report.Id,
            xmlDataDoc.DataSet,
            parameters,
            printerName,
            copies
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
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId);
        var xmlDataDoc = ReportHelper.LoadOrUseReportData(report, data, parameters, dbTransaction);
        using var langSwitcher = new LanguageSwitcher(
            langIetf: ReportHelper.ResolveLanguage(xmlDataDoc, report)
        );
        ReportHelper.LogInfo(
            System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType,
            $"Exporting report '{report.Name}' to {format}"
        );
        return crystalReportHelper.CreateReport(report.Id, xmlDataDoc.DataSet, parameters, format);
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
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId);
        var xmlDataDoc = ReportHelper.LoadOrUseReportData(report, data, parameters, dbTransaction);
        using var langSwitcher = new LanguageSwitcher(
            langIetf: ReportHelper.ResolveLanguage(xmlDataDoc, report)
        );
        ReportHelper.LogInfo(
            System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType,
            $"Exporting report '{report.Name}' to {format}"
        );
        return crystalReportHelper.PrepareReport(
            report,
            xmlDataDoc.DataSet,
            parameters,
            DataReportExportFormatType.RPT.ToString()
        );
    }
    #endregion
}
