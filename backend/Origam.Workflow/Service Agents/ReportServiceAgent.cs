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
using Origam.BI;
using Origam.Schema.GuiModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Workflow;

public class ReportServiceAgent : AbstractServiceAgent
{
    private void PrintReport(
        Guid reportId,
        IXmlContainer data,
        string printerName,
        int copies,
        Hashtable parameters
    )
    {
        AbstractReport report = GetReport(reportId: reportId);
        IReportService service = GetService(report: report);
        service.SetTraceTaskInfo(traceTaskInfo: TraceTaskInfo);
        service.PrintReport(
            reportId: reportId,
            data: data,
            printerName: printerName,
            copies: copies,
            parameters: parameters
        );
    }

    private object GetReport(Guid reportId, IXmlContainer data, string format, Hashtable parameters)
    {
        AbstractReport report = GetReport(reportId: reportId);
        IReportService service = GetService(report: report);
        service.SetTraceTaskInfo(traceTaskInfo: TraceTaskInfo);
        return service.GetReport(
            reportId: reportId,
            data: data,
            format: format,
            parameters: parameters,
            dbTransaction: TransactionId
        );
    }

    private static AbstractReport GetReport(Guid reportId)
    {
        IPersistenceService persistence = ServiceManager.Services.GetService<IPersistenceService>();
        AbstractReport report = persistence.SchemaProvider.RetrieveInstance<AbstractReport>(
            instanceId: reportId
        );
        return report;
    }

    public static IReportService GetService(AbstractReport report)
    {
        string serviceName = report switch
        {
            CrystalReport =>
                "Origam.BI.CrystalReports.CrystalReportService,Origam.BI.CrystalReports",
            PrintItReport => "Origam.BI.PrintIt.PrintItService,Origam.BI.PrintIt",
            ExcelReport => "Origam.BI.Excel.ExcelService,Origam.BI.Excel",
            SSRSReport => "Origam.BI.SSRS.SSRSService,Origam.BI.SSRS",
            FastReport => "Origam.BI.FastReport.FastReportService,Origam.BI.FastReport",
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(report),
                actualValue: report,
                message: "Unsupported report type."
            ),
        };
        string[] split = serviceName.Split(separator: ",".ToCharArray());
        return (Reflector.InvokeObject(classname: split[0], assembly: split[1]) as IReportService)!;
    }

    #region IServiceAgent Members
    private object result;
    public override object Result => result;

    public override void Run()
    {
        switch (MethodName)
        {
            case "PrintReport":
            {
                PrintReport(
                    reportId: Parameters.Get<Guid>(key: "Report"),
                    data: Parameters.TryGet<IXmlContainer>(key: "Data"),
                    printerName: Parameters.TryGet<string>(key: "PrinterName"),
                    copies: Parameters.Get<int>(key: "Copies"),
                    parameters: Parameters.TryGet<Hashtable>(key: "Parameters")
                );
                break;
            }

            case "GetReport":
            {
                result = GetReport(
                    reportId: Parameters.Get<Guid>(key: "Report"),
                    data: Parameters.TryGet<IXmlContainer>(key: "Data"),
                    format: Parameters.TryGet<string>(key: "Format"),
                    parameters: Parameters.TryGet<Hashtable>(key: "Parameters")
                );
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(MethodName),
                    actualValue: MethodName,
                    message: ResourceUtils.GetString(key: "InvalidMethodName")
                );
            }
        }
    }
    #endregion
}
