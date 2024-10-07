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
public class WorkflowServiceSoap: IWorkflowServiceSoap
{
    private readonly ILogger<AbstractController> log;
    public WorkflowServiceSoap(ILogger<AbstractController> log)
    {
        this.log = log;
    }
    public Task<object> ExecuteWorkflow0Async(string workflowId)
    {
        if (log.IsEnabled(LogLevel.Information))
        {
            log.Log(LogLevel.Information, "ExecuteWorkflow0");
        }
        Guid guid = new Guid(workflowId);
        object result = CoreServices.WorkflowService.ExecuteWorkflow(guid);
        object diffGram = ToDiffGram(result, "ExecuteWorkflow0Result");
        return Task.FromResult(diffGram);
    }
    public Task<object> ExecuteWorkflowAsync(string workflowId, Parameter[] parameters)
    {
        if (log.IsEnabled(LogLevel.Information))
        {
            log.Log(LogLevel.Information, "ExecuteWorkflow");
        }
        Guid guid = new Guid(workflowId);
        var parameterCollection = ParameterUtils.ToQueryParameterCollection(parameters);
        object result = CoreServices.WorkflowService.ExecuteWorkflow(guid, parameterCollection, null);
        object diffGram = ToDiffGram(result, "ExecuteWorkflowResult");
        return Task.FromResult(diffGram);
    }
    public Task<object> ExecuteWorkflow1Async(string workflowId, string paramName, string paramValue)
    {
        if (log.IsEnabled(LogLevel.Information))
        {
            log.Log(LogLevel.Information, "ExecuteWorkflow1");
        }
        Guid guid = new Guid(workflowId);
        QueryParameterCollection parameters = new QueryParameterCollection();
        parameters.Add(new QueryParameter(paramName, paramValue));
        object result = CoreServices.WorkflowService.ExecuteWorkflow(guid, parameters, null);
        object diffGram = ToDiffGram(result, "ExecuteWorkflow1Result");
        return Task.FromResult(diffGram);
    }
    
    private static object ToDiffGram(object result, string rootElementName)
    {
        string defaultNamespace = "http://asapenginewebapi.advantages.cz/";
        if (result is IDataDocument document)
        {
            StringBuilder sb = new StringBuilder();
            XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb));
            writer.WriteStartElement( rootElementName,defaultNamespace);
            document.DataSet.WriteXmlSchema(writer);
            document.DataSet.WriteXml(writer, XmlWriteMode.DiffGram);
            writer.WriteEndElement();
            return sb.ToString();
        }
        return result;
    }
}
