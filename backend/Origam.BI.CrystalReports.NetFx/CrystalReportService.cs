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
using System.Data;

using Origam.Schema.GuiModel;

using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
using System.Drawing.Printing;
using Origam.Service.Core;

namespace Origam.BI.CrystalReports;

/// <summary>
/// Summary description for CrystalReportService.
/// </summary>
public class CrystalReportService : IReportService
{
	CrystalReportHelper _helper = new CrystalReportHelper();

	public CrystalReportService()
	{
		}

	public void PrintReport(Guid reportId, IXmlContainer data, string printerName, int copies, Hashtable parameters)
	{
			var report = ReportHelper.GetReportElement<CrystalReport>(reportId);

             if(! (data is IDataDocument | data == null))
             {
                 throw new ArgumentOutOfRangeException("data", data, ResourceUtils.GetString("OnlyXmlDocSupported"));
             }

             DataSet dataset = null;
             if(data is IDataDocument)
             {
                 dataset = (data as IDataDocument).DataSet;
             }

             ReportDocument.EnableEventLog(CrystalDecisions.Shared.EventLogLevel.LogEngineErrors);
             System.Diagnostics.Debug.WriteLine(ReportDocument.GetConcurrentUsage(), "Crystal Reports Concurrent Usage");

            using (LanguageSwitcher langSwitcher = new LanguageSwitcher(
				ReportHelper.ResolveLanguage(data as IDataDocument, report)))
			{
				using (ReportDocument reportDoc = _helper.CreateReport(report.Id, dataset, parameters))
				{
					if (printerName == null)
					{
						System.Drawing.Printing.PrinterSettings ps = new System.Drawing.Printing.PrinterSettings();
						printerName = ps.PrinterName;
					}

					ReportHelper.LogInfo(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "Printing report '" + report.Name + "' to printer '" + printerName + "'");

                    PrintDocument pDoc = new PrintDocument();
                    PrintLayoutSettings printLayout = new PrintLayoutSettings();
                    PrinterSettings printerSettings = new PrinterSettings();
                    printerSettings.Copies = Convert.ToInt16(copies);
                    printerSettings.PrinterName = printerName;
                    reportDoc.PrintOptions.PrinterDuplex = PrinterDuplex.Simplex;
                    PageSettings pageSettings = new PageSettings(printerSettings);
                    reportDoc.PrintOptions.DissociatePageSizeAndPrinterPaperSize = true;
                    reportDoc.PrintToPrinter(printerSettings, pageSettings, false, printLayout);
                    reportDoc.Close();
				}
			}
		}

	public object GetReport(Guid reportId, IXmlContainer data, string format, Hashtable parameters, string dbTransaction)
	{
			var report = ReportHelper.GetReportElement<AbstractDataReport>(reportId);

			IDataDocument xmlDataDoc = ReportHelper.LoadOrUseReportData(report, data, parameters, dbTransaction);
			DataSet dataset = xmlDataDoc.DataSet;

			using (LanguageSwitcher langSwitcher = new LanguageSwitcher(ReportHelper.ResolveLanguage(xmlDataDoc, report)))
			{
				using (ReportDocument reportDoc = _helper.CreateReport(report.Id, dataset, parameters))
				{
					ExportFormatType type;

					switch (format)
					{
						case "PDF":
							type = ExportFormatType.PortableDocFormat;
							break;
						case "MSExcel":
							type = ExportFormatType.Excel;
							break;
						case "RTF":
							type = ExportFormatType.EditableRTF;
							break;
						case "MSWord":
							type = ExportFormatType.WordForWindows;
							break;
						case "HTML":
							type = ExportFormatType.HTML40;
							break;
						case "CSV":
							type = ExportFormatType.CharacterSeparatedValues;
							break;
						case "TEXT":
							type = ExportFormatType.Text;
							break;
						case "XML":
							type = ExportFormatType.Xml;
							break;
						default:
							throw new ArgumentOutOfRangeException("format", format, ResourceUtils.GetString("FormatNotSupported"));
					}

					ReportHelper.LogInfo(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, "Exporting report '" + report.Name + "' to " + format);

					System.IO.Stream stream = reportDoc.ExportToStream(type);
					byte[] result = new byte[stream.Length];
					stream.Read(result, 0, Convert.ToInt32(stream.Length));
					stream.Close();

					reportDoc.Close();

					return result;
				}
			}
		}
	public void SetTraceTaskInfo(TraceTaskInfo traceTaskInfo)
	{
            // do nothing unless we need to trace
        }

	public string PrepareExternalReportViewer(Guid reportId,
		IXmlContainer data, string format, Hashtable parameters,
		string dbTransaction)
	{
            throw new NotImplementedException();
        }
}