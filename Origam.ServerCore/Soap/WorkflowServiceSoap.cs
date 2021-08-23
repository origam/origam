using System;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.ServerCore.Controller;
using core = Origam.Workbench.Services.CoreServices;

namespace Origam.ServerCore
{
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
            return Task.FromResult(ConvertData(core.WorkflowService.ExecuteWorkflow(guid)));
        }

        public Task<object> ExecuteWorkflowAsync(string workflowId, Parameter[] parameters)
        {
            if (log.IsEnabled(LogLevel.Information))
            {
                log.Log(LogLevel.Information, "ExecuteWorkflow");
            }

            Guid guid = new Guid(workflowId);
            var parameterCollection = ParameterUtils.ToQueryParameterCollection(parameters);
            return Task.FromResult(ConvertData(core.WorkflowService.ExecuteWorkflow(guid, parameterCollection, null)));
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
            return Task.FromResult(ConvertData(core.WorkflowService.ExecuteWorkflow(guid, parameters, null)));
        }
        
        private static object ConvertData(object result)
        {
            if (result is IDataDocument document)
            {
                StringBuilder sb = new StringBuilder();
                XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb));
                document.DataSet.WriteXml(writer, XmlWriteMode.DiffGram);
                return sb.ToString();
            }
            return result;
        }
    }
}