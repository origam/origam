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
using Origam.Workbench.Services;
using Origam.BI;
using Origam.Service.Core;

namespace Origam.Workflow;
/// <summary>
/// Summary description for ReportServiceAgent.
/// </summary>
public class ReportServiceAgent : AbstractServiceAgent
{
	private void PrintReport(
		Guid reportId, 
		IXmlContainer data, 
		string printerName, 
		int copies, 
		Hashtable parameters)
	{
        AbstractReport report = GetReport(reportId);
        IReportService service = GetService(report);
        service.SetTraceTaskInfo(TraceTaskInfo);
        service.PrintReport(reportId, data, printerName, copies, parameters);
	}
	private object GetReport(
		Guid reportId, 
		IXmlContainer data, 
		string format, 
		Hashtable parameters)
	{
        AbstractReport report = GetReport(reportId);
        IReportService service = GetService(report);
        service.SetTraceTaskInfo(TraceTaskInfo);
        return service.GetReport(
	        reportId, data, format, parameters, TransactionId);
	}
    private static AbstractReport GetReport(Guid reportId)
    {
	    IPersistenceService persistence 
		    = ServiceManager.Services.GetService<IPersistenceService>();
	    AbstractReport report 
		    = persistence.SchemaProvider.RetrieveInstance<AbstractReport>(
			    reportId);
        return report;
    }
    public static IReportService GetService(AbstractReport report)
    {
	    string serviceName = report switch
	    {
		    CrystalReport 
			    => "Origam.BI.CrystalReports.CrystalReportService,Origam.BI.CrystalReports",
		    PrintItReport 
			    => "Origam.BI.PrintIt.PrintItService,Origam.BI.PrintIt",
		    ExcelReport 
			    => "Origam.BI.Excel.ExcelService,Origam.BI.Excel",
		    SSRSReport 
			    => "Origam.BI.SSRS.SSRSService,Origam.BI.SSRS",
		    FastReport 
			    => "Origam.BI.FastReport.FastReportService,Origam.BI.FastReport",
		    _ => throw new ArgumentOutOfRangeException(
			    nameof(report), report, "Unsupported report type.")
	    };
	    string[] split = serviceName.Split(",".ToCharArray());
        return (Reflector.InvokeObject(classname: split[0], assembly: split[1]) 
	        as IReportService)!;
    }
	#region IServiceAgent Members
	private object result;
	public override object Result => result;

	public override void Run()
	{
		switch (MethodName)
		{
			case "PrintReport":
				PrintReport(
					Parameters.Get<Guid>("Report"),
					Parameters.Get<IXmlContainer>("Data"),
					Parameters.Get<string>("PrinterName"),
					Parameters.Get<int>("Copies"),
					Parameters.Get<Hashtable>("Parameters"));
				break;
			case "GetReport":
				result = GetReport(
					Parameters.Get<Guid>("Report"),
					Parameters.Get<IXmlContainer>("Data"),
					Parameters.Get<string>("Format"),
					Parameters.Get<Hashtable>("Parameters"));
				break;
			default:
				throw new ArgumentOutOfRangeException(
					nameof(MethodName), MethodName, 
					ResourceUtils.GetString("InvalidMethodName"));
		}
	}
	#endregion
}
