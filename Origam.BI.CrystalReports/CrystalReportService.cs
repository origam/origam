#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using System.Data;
using Origam.Schema.GuiModel;
using System.Drawing.Printing;

namespace Origam.BI.CrystalReports
{
	/// <summary>
	/// Summary description for CrystalReportService.
	/// </summary>
	public class CrystalReportService : IReportService
	{
		CrystalReportHelper _helper = new CrystalReportHelper();

		public CrystalReportService()
		{
		}

		#region IReportService Members

		public void PrintReport(Guid reportId, IXmlContainer data, string printerName, int copies, Hashtable parameters)
		{
			CrystalReport report = ReportHelper.GetReportElement(reportId) as CrystalReport;

             if(! (data is IDataDocument | data == null))
             {
                 throw new ArgumentOutOfRangeException("data", data, ResourceUtils.GetString("OnlyXmlDocSupported"));
             }
             DataSet dataset = null;
             if(data is IDataDocument)
             {
                 dataset = (data as IDataDocument).DataSet;
             }
            using (LanguageSwitcher langSwitcher = new LanguageSwitcher(
				ReportHelper.ResolveLanguage(data as IDataDocument, report)))
			{
				
			}
		}

		public object GetReport(Guid reportId, IXmlContainer data, string format, Hashtable parameters, string dbTransaction)
		{
			AbstractDataReport report = ReportHelper.GetReportElement(reportId);
			IDataDocument xmlDataDoc = ReportHelper.LoadOrUseReportData(report, data, parameters, dbTransaction);
			DataSet dataset = xmlDataDoc.DataSet;
			using (LanguageSwitcher langSwitcher = new LanguageSwitcher(ReportHelper.ResolveLanguage(xmlDataDoc, report)))
			{
				ReportHelper.LogInfo(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "Exporting report '" + report.Name + "' to " + format);
				return _helper.CreateReport(report.Id, dataset, parameters, format);
			}
		}
        public void SetTraceTaskInfo(TraceTaskInfo traceTaskInfo)
        {
            // do nothing unless we need to trace
        }
        #endregion
    }
}
