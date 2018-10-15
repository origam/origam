using Origam.Rule;
using Origam.Workbench.Services;

namespace Origam.OrigamEngine
{
    public interface IRuntimeServiceFactory
    {
        void InitializeServices();
        IPersistenceService CreatePersistenceService();
        IDocumentationService CreateDocumentationService();
    }

    public class RuntimeServiceFactory : IRuntimeServiceFactory
    {
        public void InitializeServices()
        {
            ServiceManager.Services.AddService(CreatePersistenceService());
            ServiceManager.Services.AddService(new Origam.Workflow.StateMachineService());
            // Architect initialzes its own version of schema service
            if (ServiceManager.Services.GetService(typeof(SchemaService)) == null)
            {
                ServiceManager.Services.AddService(new SchemaService());
            }
            ServiceManager.Services.AddService(new Origam.Workbench.Services.ServiceAgentFactory());
            ServiceManager.Services.AddService(CreateDocumentationService());
            ServiceManager.Services.AddService(new TracingService());
            ServiceManager.Services.AddService(new DataLookupService());
            ServiceManager.Services.AddService(CreateParameterService());
            ServiceManager.Services.AddService(new DeploymentService());
            ServiceManager.Services.AddService(new Origam.Workflow.WorkQueue.WorkQueueService());
            ServiceManager.Services.AddService(new AttachmentService());
            ServiceManager.Services.AddService(new RuleEngineService());
        }

        protected virtual IParameterService CreateParameterService()
        {
            return new ParameterService();
        }

        public IPersistenceService CreatePersistenceService()
        {
            return GetPersistenceBuilder().GetPersistenceService();
        }
        
        public IDocumentationService CreateDocumentationService()
        {
            return GetPersistenceBuilder().GetDocumentationService();
        }
        
        private static IPersistenceBuilder GetPersistenceBuilder()
        {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() as OrigamSettings;
            string[] classpath = settings.ModelProvider.Split(',');
            return Reflector.InvokeObject(classpath[0], classpath[1]) as IPersistenceBuilder;
        }
    }

    public class TestRuntimeServiceFactory : RuntimeServiceFactory
    {
        protected override IParameterService CreateParameterService()
        {
            return new  NullParameterService();
        }
    }
}