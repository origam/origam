using System.Collections;

namespace Origam.ServiceCore
{
    public interface IExternalServiceAgent
    {
        object Result{get;}
        void Run();
        Hashtable Parameters { get; } 
        string MethodName{ get; set; }
        string TransactionId { get; set; }
    }
}