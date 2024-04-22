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

using Origam.Schema.GuiModel;
using Origam.Workbench.Services;
using Origam.Schema;
using Origam.BI;
using Origam.Service.Core;

namespace Origam.Workflow;

/// <summary>
/// Summary description for ReportServiceAgent.
/// </summary>
public class ReportServiceAgent : AbstractServiceAgent
{
	public ReportServiceAgent()
	{
		}

	private void PrintReport(Guid reportId, IXmlContainer data, string printerName, int copies, Hashtable parameters)
	{
            AbstractReport report = GetReport(reportId);
            IReportService service = GetService(report);
            service.SetTraceTaskInfo(TraceTaskInfo);
            service.PrintReport(reportId, data, printerName, copies, parameters);
		}

	private object GetReport(Guid reportId, IXmlContainer data, string format, Hashtable parameters)
	{
            AbstractReport report = GetReport(reportId);
            IReportService service = GetService(report);
            service.SetTraceTaskInfo(TraceTaskInfo);
            return service.GetReport(reportId, data, format, parameters, this.TransactionId);
		}

	private static AbstractReport GetReport(Guid reportId)
	{
            IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            AbstractReport report = persistence.SchemaProvider.RetrieveInstance(
                typeof(AbstractReport), new ModelElementKey(reportId))
                as AbstractReport;
            return report;
        }

	public static IReportService GetService(AbstractReport report)
	{
            string serviceName;
            if (report is CrystalReport)
            {
                serviceName = "Origam.BI.CrystalReports.CrystalReportService,Origam.BI.CrystalReports";
            }
            else if (report is PrintItReport)
            {
                serviceName = "Origam.BI.PrintIt.PrintItService,Origam.BI.PrintIt";
            }
            else if (report is ExcelReport)
            {
                serviceName = "Origam.BI.Excel.ExcelService,Origam.BI.Excel";
            }
            else if (report is SSRSReport)
            {
                serviceName = "Origam.BI.SSRS.SSRSService,Origam.BI.SSRS";
            }
            else if (report is FastReport)
            {
                serviceName = "Origam.BI.FastReport.FastReportService,Origam.BI.FastReport";
            }
            else
            {
                throw new ArgumentOutOfRangeException("report", report, "Unsupported report type.");
            }
            string[] split = serviceName.Split(",".ToCharArray());
            return Reflector.InvokeObject(split[0], split[1]) as IReportService;
        }
	#region IServiceAgent Members
	private object _result;
	public override object Result
	{
		get
		{
				return _result;
			}
	}

	public override void Run()
	{
			switch(this.MethodName)
			{
				case "PrintReport":
					// Check input parameters
					if(! (this.Parameters["Report"] is Guid))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorReportNotGuild"));

					if(! (this.Parameters["Data"] is IXmlContainer | this.Parameters["Data"] == null))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorNotXmlDocument"));

					if(! (this.Parameters["PrinterName"] is string | this.Parameters["PrinterName"] == null))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorPrinterNameNotString"));

					if(! (this.Parameters["Copies"] is int))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorCopiesNotInt"));

					if(! (this.Parameters["Parameters"] == null || this.Parameters["Parameters"] is Hashtable))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorNotHashtable"));

					PrintReport((Guid)this.Parameters["Report"],
						this.Parameters["Data"] as IXmlContainer,
						(string)this.Parameters["PrinterName"],
						(int)this.Parameters["Copies"],
						this.Parameters["Parameters"] as Hashtable);

					break;

				case "GetReport":
					// Check input parameters
					if(! (this.Parameters["Report"] is Guid))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorReportNotGuid"));

				    IXmlContainer data = null;;
					if (this.Parameters.Contains("Data")) {
						if(! (this.Parameters["Data"] is IXmlContainer | this.Parameters["Data"] == null))
							throw new InvalidCastException(ResourceUtils.GetString("ErrorNotXmlDocument"));
						data = this.Parameters["Data"] as IXmlContainer;
					}					

					if(! (this.Parameters["Format"] is string | this.Parameters["Format"] == null))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorFormatNotString"));

					if(! (this.Parameters["Parameters"] == null || this.Parameters["Parameters"] is Hashtable))
						throw new InvalidCastException(ResourceUtils.GetString("ErrorNotHashtable"));

					_result = GetReport((Guid)this.Parameters["Report"],
						data,
						(string)this.Parameters["Format"],
						this.Parameters["Parameters"] as Hashtable);

					break;

				default:
					throw new ArgumentOutOfRangeException("MethodName", this.MethodName, ResourceUtils.GetString("InvalidMethodName"));
			}
		}

	#endregion
}