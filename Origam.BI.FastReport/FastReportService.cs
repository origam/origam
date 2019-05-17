#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

namespace Origam.BI.FastReport
{
    public class FastReportService : IReportService
    {
        public object GetReport(Guid reportId, IXmlContainer data, 
            string format, Hashtable parameters, string dbTransaction)
        {
            var report = ReportHelper.GetReportElement(reportId);
            IDataDocument xmlDataDoc = ReportHelper.LoadOrUseReportData(
                report, data, parameters, dbTransaction);
            DataSet dataset = xmlDataDoc.DataSet;
            using (LanguageSwitcher langSwitcher = new LanguageSwitcher(
                ReportHelper.ResolveLanguage(xmlDataDoc, report)))
            {
                using (var reportDoc = new Report())
                {
                    reportDoc.Load(report.ReportFileName);
                    reportDoc.RegisterData(dataset);
                    reportDoc.Prepare();
                    if (format != "PDF")
                    {
                        throw new ArgumentOutOfRangeException("format", 
                            format, 
                            ResourceUtils.GetString("FormatNotSupported"));
                    }
                    ReportHelper.LogInfo(
                        System.Reflection.MethodBase.GetCurrentMethod()
                        .DeclaringType, 
                        "Exporting report '" + report.Name + "' to " + format);
                    using (var stream = new MemoryStream())
                    {
                        PDFSimpleExport pdf = new PDFSimpleExport();
                        reportDoc.Export(pdf, stream);
                        return stream.ToArray();
                    }
                }
            }
        }

        public void PrintReport(Guid reportId, IXmlContainer data, string printerName, int copies, Hashtable parameters)
        {
            throw new NotImplementedException();
        }

        public void SetTraceTaskInfo(TraceTaskInfo traceTaskInfo)
        {
        }
    }
}
