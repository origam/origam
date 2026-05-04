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
using System.IO;
using FastReport;
using FastReport.Export.PdfSimple;
using Origam.Schema.GuiModel;
using Origam.Service.Core;

namespace Origam.BI.FastReport;

public class FastReportService : IReportService
{
    public object GetReport(
        Guid reportId,
        IXmlContainer data,
        string format,
        Hashtable parameters,
        string dbTransaction
    )
    {
        var report = ReportHelper.GetReportElement<AbstractDataReport>(reportId: reportId);
        parameters ??= new Hashtable();
        ReportHelper.PopulateDefaultValues(report: report, parameters: parameters);
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
        using var reportDoc = new Report();
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        string path = Path.Combine(
            path1: settings.ReportsFolder(),
            path2: ReportHelper.ExpandCurlyBracketPlaceholdersWithParameters(
                input: report.ReportFileName,
                parameters: parameters
            )
        );
        if (!IOTools.IsSubPathOf(path: path, basePath: settings.ReportsFolder()))
        {
            throw new Exception(message: Strings.PathNotOnReportPath);
        }
        if (File.Exists(path: path))
        {
            reportDoc.Load(fileName: path);
        }
        else
        {
            throw new Exception(message: ResourceUtils.GetString(key: "PathNotFound", args: path));
        }
        foreach (DataTable table in dataset.Tables)
        {
            reportDoc.RegisterData(data: table, name: table.TableName);
        }
        reportDoc.Prepare();
        if (format != "PDF")
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(format),
                actualValue: format,
                message: ResourceUtils.GetString(key: "FormatNotSupported")
            );
        }
        ReportHelper.LogInfo(
            type: System.Reflection.MethodBase.GetCurrentMethod()?.DeclaringType,
            message: $"Exporting report '{report.Name}' to {format}"
        );
        using var stream = new MemoryStream();
        var pdf = new PDFSimpleExport();
        reportDoc.Export(export: pdf, stream: stream);
        return stream.ToArray();
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

    public void PrintReport(
        Guid reportId,
        IXmlContainer data,
        string printerName,
        int copies,
        Hashtable parameters
    )
    {
        throw new NotImplementedException();
    }

    public void SetTraceTaskInfo(TraceTaskInfo traceTaskInfo) { }
}
