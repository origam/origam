using System;
using System.Xml;
using System.Collections;
using System.Data;

using Origam.Schema.GuiModel;

using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;
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
			AbstractDataReport report = ReportHelper.GetReportElement(reportId);

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
        #endregion
    }
}
