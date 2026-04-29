#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.Server.Controller;
using Origam.Service.Core;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

public class WorkflowServiceSoap : IWorkflowServiceSoap
{
    private readonly ILogger<AbstractController> log;

    public WorkflowServiceSoap(ILogger<AbstractController> log)
    {
        this.log = log;
    }

    public Task<object> ExecuteWorkflow0Async(string workflowId)
    {
        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.Log(logLevel: LogLevel.Information, message: "ExecuteWorkflow0");
        }
        Guid guid = new Guid(g: workflowId);
        object result = CoreServices.WorkflowService.ExecuteWorkflow(workflowId: guid);
        object diffGram = ToDiffGram(result: result, rootElementName: "ExecuteWorkflow0Result");
        return Task.FromResult(result: diffGram);
    }

    public Task<object> ExecuteWorkflowAsync(string workflowId, Parameter[] parameters)
    {
        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.Log(logLevel: LogLevel.Information, message: "ExecuteWorkflow");
        }
        Guid guid = new Guid(g: workflowId);
        var parameterCollection = ParameterUtils.ToQueryParameterCollection(parameters: parameters);
        object result = CoreServices.WorkflowService.ExecuteWorkflow(
            workflowId: guid,
            parameters: parameterCollection,
            transactionId: null
        );
        object diffGram = ToDiffGram(result: result, rootElementName: "ExecuteWorkflowResult");
        return Task.FromResult(result: diffGram);
    }

    public Task<object> ExecuteWorkflow1Async(
        string workflowId,
        string paramName,
        string paramValue
    )
    {
        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.Log(logLevel: LogLevel.Information, message: "ExecuteWorkflow1");
        }
        Guid guid = new Guid(g: workflowId);
        QueryParameterCollection parameters = new QueryParameterCollection();
        parameters.Add(value: new QueryParameter(_parameterName: paramName, value: paramValue));
        object result = CoreServices.WorkflowService.ExecuteWorkflow(
            workflowId: guid,
            parameters: parameters,
            transactionId: null
        );
        object diffGram = ToDiffGram(result: result, rootElementName: "ExecuteWorkflow1Result");
        return Task.FromResult(result: diffGram);
    }

    private static object ToDiffGram(object result, string rootElementName)
    {
        string defaultNamespace = "http://asapenginewebapi.advantages.cz/";
        if (result is IDataDocument document)
        {
            StringBuilder sb = new StringBuilder();
            XmlTextWriter writer = new XmlTextWriter(w: new StringWriter(sb: sb));
            writer.WriteStartElement(localName: rootElementName, ns: defaultNamespace);
            document.DataSet.WriteXmlSchema(writer: writer);
            document.DataSet.WriteXml(writer: writer, mode: XmlWriteMode.DiffGram);
            writer.WriteEndElement();
            return sb.ToString();
        }
        return result;
    }
}
