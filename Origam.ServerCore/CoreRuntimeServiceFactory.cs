using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using Origam.DA;
using Origam.OrigamEngine;
using Origam.Schema;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;

namespace Origam.ServerCore
{
    public class CoreRuntimeServiceFactory: IRuntimeServiceFactory
    {
        public void InitializeServices()
        {
            ServiceManager.Services.AddService(CreatePersistenceService());
            //ServiceManager.Services.AddService(new Origam.Workflow.StateMachineService());
            ServiceManager.Services.AddService(new SchemaService());
            ServiceManager.Services.AddService(new ServiceAgentFactory());
//            ServiceManager.Services.AddService(CreateDocumentationService());
//            ServiceManager.Services.AddService(new TracingService());
            ServiceManager.Services.AddService(new DataLookupService());
            ServiceManager.Services.AddService(new ParameterService());
//            ServiceManager.Services.AddService(new DeploymentService());
//            ServiceManager.Services.AddService(new WorkQueueService());
//            ServiceManager.Services.AddService(new AttachmentService());
           // ServiceManager.Services.AddService(new RuleEngineService());
        }

        public IPersistenceService CreatePersistenceService()
        {
            var defaultFolders = new List<ElementName>
            {
                ElementNameFactory.Create(typeof(SchemaExtension)),
                ElementNameFactory.Create(typeof(SchemaItemGroup))
            };

            var persistenceService = new FilePersistenceService(defaultFolders);
            
            ServiceManager.Services.AddService(persistenceService);
            return persistenceService;
        }

        public IDocumentationService CreateDocumentationService()
        {
            return GetPersistenceBuilder().GetDocumentationService();
        }

        private static IPersistenceBuilder GetPersistenceBuilder()
        {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            string[] classpath = settings.ModelProvider.Split(',');
            return Reflector.InvokeObject(classpath[0], classpath[1]) as IPersistenceBuilder;
        }
    }
}
