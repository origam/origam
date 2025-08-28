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
using System.Xml;

namespace Origam.Workbench.Services.CoreServices;

/// <summary>
/// Summary description for ReportService.
/// </summary>
public class ReportService
{
    public static byte[] GetReport(
        Guid reportId,
        XmlDocument data,
        string format,
        Hashtable parameters,
        string transactionId
    )
    {
        IServiceAgent reportServiceAgent = ServiceManager
            .Services.GetService<IBusinessServicesService>()
            .GetAgent(serviceType: "ReportService", ruleEngine: null, workflowEngine: null);
        reportServiceAgent.MethodName = "GetReport";
        reportServiceAgent.Parameters.Clear();
        reportServiceAgent.Parameters.Add("Report", reportId);
        reportServiceAgent.Parameters.Add("Data", data);
        reportServiceAgent.Parameters.Add("Format", format);
        reportServiceAgent.Parameters.Add("Parameters", parameters);
        reportServiceAgent.TransactionId = transactionId;
        reportServiceAgent.Run();
        return (byte[])reportServiceAgent.Result;
    }
}
