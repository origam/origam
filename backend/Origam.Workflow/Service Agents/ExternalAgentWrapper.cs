using System.Collections;
using Origam.Service.Core;

namespace Origam.Workflow;
public class ExternalAgentWrapper: AbstractServiceAgent
{
    private readonly IExternalServiceAgent externalServiceAgent;
    public ExternalAgentWrapper(IExternalServiceAgent externalServiceAgent)
    {
        this.externalServiceAgent = externalServiceAgent;
    }
    public override object Result => externalServiceAgent.Result;
    public override void Run()
    {
        externalServiceAgent.Run();
    }
    public override Hashtable Parameters => externalServiceAgent.Parameters;
    public override string MethodName
    {
        get => externalServiceAgent.MethodName;
        set => externalServiceAgent.MethodName = value;
    }
    
    public override string TransactionId
    {
        get => externalServiceAgent.TransactionId;
        set => externalServiceAgent.TransactionId = value;
    }
}
