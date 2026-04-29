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
using System.Net;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Threading;
using log4net;
using Origam.BI.SSRS.SSRSWebReference;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.BI.SSRS;

// ReSharper disable once InconsistentNaming
public class SSRSService : IReportService
{
    private TraceTaskInfo traceTaskInfo;

    // ReSharper disable once InconsistentNaming
    private static readonly ILog log = LogManager.GetLogger(
        type: MethodBase.GetCurrentMethod().DeclaringType
    );

    public object GetReport(
        Guid reportId,
        IXmlContainer data,
        string format,
        Hashtable parameters,
        string dbTransaction
    )
    {
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        if (
            !(
                persistenceService.SchemaProvider.RetrieveInstance(
                    type: typeof(AbstractReport),
                    primaryKey: new ModelElementKey(id: reportId)
                )
                is SSRSReport report
            )
        )
        {
            throw new ArgumentOutOfRangeException(
                paramName: "reportId",
                actualValue: reportId,
                message: Strings.DefinitionNotInModel
            );
        }
        if (parameters == null)
        {
            parameters = new Hashtable();
        }
        ReportHelper.PopulateDefaultValues(report: report, parameters: parameters);
        ReportHelper.ComputeXsltValueParameters(
            report: report,
            parameters: parameters,
            traceTaskInfo: traceTaskInfo
        );
        var settings = ConfigurationManager.GetActiveConfiguration();
        var serviceTimeout = TimeSpan.FromMilliseconds(value: settings.SQLReportServiceTimeout);
        var binding = new BasicHttpBinding(
            securityMode: BasicHttpSecurityMode.TransportCredentialOnly
        )
        {
            OpenTimeout = serviceTimeout,
            SendTimeout = serviceTimeout,
            ReceiveTimeout = serviceTimeout,
            CloseTimeout = serviceTimeout,
            MaxReceivedMessageSize = 104857600, //100MB
        };
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(
                format: "SSRSService Timeout: {0}",
                arg0: settings.SQLReportServiceTimeout
            );
        }
        var serviceClient = new ReportExecutionServiceSoapClient(
            binding: binding,
            remoteAddress: new EndpointAddress(uri: settings.SQLReportServiceUrl)
        );
        if (string.IsNullOrEmpty(value: settings.SQLReportServiceAccount))
        {
            serviceClient.ClientCredentials.Windows.ClientCredential =
                CredentialCache.DefaultNetworkCredentials;
        }
        else
        {
            binding.Security.Transport.ClientCredentialType = HttpClientCredentialType.Ntlm;
            serviceClient.ClientCredentials.Windows.ClientCredential = new NetworkCredential(
                userName: settings.SQLReportServiceAccount,
                password: settings.SQLReportServicePassword
            );
        }
        serviceClient.Endpoint.EndpointBehaviors.Add(item: new ReportingServicesEndpointBehavior());
        var reportPath = ReportHelper.ExpandCurlyBracketPlaceholdersWithParameters(
            input: report.ReportPath,
            parameters: parameters
        );
        var executionHeader = new ExecutionHeader();
        // TrustedUserHeader is used when connecting via https
        // so for we don't have such instance, so we ignore it
        var loadReportResponse = serviceClient
            .LoadReportAsync(TrustedUserHeader: null, Report: reportPath, HistoryID: null)
            .ConfigureAwait(continueOnCapturedContext: false)
            .GetAwaiter()
            .GetResult();
        if (loadReportResponse.ExecutionHeader != null)
        {
            executionHeader.ExecutionID = loadReportResponse.ExecutionHeader.ExecutionID;
        }
        if (parameters.Count > 0)
        {
            var reportParameters = new ParameterValue[parameters.Count];
            var index = 0;
            foreach (string key in parameters.Keys)
            {
                var parameterValue = new ParameterValue
                {
                    Name = key,
                    Value = parameters[key: key].ToString(),
                };
                reportParameters[index] = parameterValue;
                index++;
            }
            serviceClient
                .SetExecutionParametersAsync(
                    ExecutionHeader: executionHeader,
                    TrustedUserHeader: null,
                    Parameters: reportParameters,
                    ParameterLanguage: Thread.CurrentThread.CurrentCulture.IetfLanguageTag
                )
                .ConfigureAwait(continueOnCapturedContext: false)
                .GetAwaiter()
                .GetResult();
        }
        return serviceClient
            .RenderAsync(
                request: new RenderRequest(
                    ExecutionHeader: executionHeader,
                    TrustedUserHeader: null,
                    Format: format,
                    DeviceInfo: "<DeviceInfo><Toolbar>False</Toolbar></DeviceInfo>"
                )
            )
            .ConfigureAwait(continueOnCapturedContext: false)
            .GetAwaiter()
            .GetResult()
            .Result;
    }

    public void PrintReport(
        Guid reportId,
        IXmlContainer data,
        string printerName,
        int copies,
        Hashtable parameters
    )
    {
        throw new NotSupportedException();
    }

    public void SetTraceTaskInfo(TraceTaskInfo value)
    {
        this.traceTaskInfo = value;
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
}

internal class ReportingServicesEndpointBehavior : IEndpointBehavior
{
    public void AddBindingParameters(
        ServiceEndpoint endpoint,
        BindingParameterCollection bindingParameters
    ) { }

    public void ApplyClientBehavior(ServiceEndpoint endpoint, ClientRuntime clientRuntime)
    {
        clientRuntime.ClientMessageInspectors.Add(item: new ReportingServicesExecutionInspector());
    }

    public void ApplyDispatchBehavior(
        ServiceEndpoint endpoint,
        EndpointDispatcher endpointDispatcher
    ) { }

    public void Validate(ServiceEndpoint endpoint) { }
}

internal class ReportingServicesExecutionInspector : IClientMessageInspector
{
    private MessageHeaders headers;

    public void AfterReceiveReply(ref Message reply, object correlationState)
    {
        var index = reply.Headers.FindHeader(
            name: "ExecutionHeader",
            ns: "http://schemas.microsoft.com/sqlserver/2005/06/30/reporting/reportingservices"
        );
        if ((index >= 0) && (headers == null))
        {
            headers = new MessageHeaders(version: MessageVersion.Soap11);
            headers.CopyHeaderFrom(
                message: reply,
                headerIndex: reply.Headers.FindHeader(
                    name: "ExecutionHeader",
                    ns: "http://schemas.microsoft.com/sqlserver/2005/06/30/reporting/reportingservices"
                )
            );
        }
    }

    public object BeforeSendRequest(ref Message request, IClientChannel channel)
    {
        if (headers != null)
        {
            request.Headers.CopyHeadersFrom(collection: headers);
        }
        //https://msdn.microsoft.com/en-us/library/system.servicemodel.dispatcher.iclientmessageinspector.beforesendrequest(v=vs.110).aspx#Anchor_0
        return Guid.NewGuid();
    }
}
