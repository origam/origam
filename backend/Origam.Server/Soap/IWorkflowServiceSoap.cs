namespace Origam.Server
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Tools.ServiceModel.Svcutil", "2.0.2")]
    [System.ServiceModel.ServiceContractAttribute(Namespace="http://asapenginewebapi.advantages.cz/", ConfigurationName="WorkflowServiceSoap")]
    public interface IWorkflowServiceSoap
    {
        [System.ServiceModel.OperationContractAttribute(
            Action = "http://asapenginewebapi.advantages.cz/ExecuteWorkflow0",
            ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.Threading.Tasks.Task<object> ExecuteWorkflow0Async(string workflowId); 

        [System.ServiceModel.OperationContractAttribute(
            Action = "http://asapenginewebapi.advantages.cz/ExecuteWorkflow",
            ReplyAction = "*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults = true)]
        System.Threading.Tasks.Task<object> ExecuteWorkflowAsync(string workflowId, Parameter[] parameters); 
        
        [System.ServiceModel.OperationContractAttribute(Action="http://asapenginewebapi.advantages.cz/ExecuteWorkflow1", ReplyAction="*")]
        [System.ServiceModel.XmlSerializerFormatAttribute(SupportFaults=true)]
        System.Threading.Tasks.Task<object> ExecuteWorkflow1Async(string workflowId, string paramName, string paramValue);
    }
}