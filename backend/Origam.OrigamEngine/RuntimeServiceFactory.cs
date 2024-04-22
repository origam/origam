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

using System.Collections.Generic;
using System.Linq;
using Origam.DA;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Rule;
using Origam.Workbench.Services;
using Origam.Workflow;
using Origam.Workflow.WorkQueue;

namespace Origam.OrigamEngine;

public interface IRuntimeServiceFactory
{
    void InitializeServices();
    IPersistenceService CreatePersistenceService();
    IDocumentationService CreateDocumentationService();
    void UnloadServices();
}

public class RuntimeServiceFactory : IRuntimeServiceFactory
{
    public void InitializeServices()
    {
            ServiceManager.Services.AddService(new MetaModelUpgradeService());
            ServiceManager.Services.AddService(CreatePersistenceService());
            ServiceManager.Services.AddService(new Origam.Workflow.StateMachineService());
            // Architect initializes its own version of schema service
            if (ServiceManager.Services.GetService(typeof(SchemaService)) == null)
            {
                ServiceManager.Services.AddService(new SchemaService());
            }
            ServiceManager.Services.AddService(new ServiceAgentFactory(externalAgent => new ExternalAgentWrapper(externalAgent)));
            ServiceManager.Services.AddService(CreateDocumentationService());
            ServiceManager.Services.AddService(new TracingService());
            ServiceManager.Services.AddService(new DataLookupService());
            ServiceManager.Services.AddService(CreateParameterService());
            ServiceManager.Services.AddService(new DeploymentService());
            ServiceManager.Services.AddService(CreateWorkQueueService());
            ServiceManager.Services.AddService(new AttachmentService());
            ServiceManager.Services.AddService(new RuleEngineService());
        }
    public void UnloadServices()
    {
            List<IWorkbenchService> services = new []
                {
                    typeof(IPersistenceService),
                    typeof(IStateMachineService),
                    typeof(IBusinessServicesService),
                    typeof(IDocumentationService),
                    typeof(TracingService),
                    typeof(IDataLookupService),
                    typeof(IParameterService),
                    typeof(IDeploymentService),
                    typeof(IWorkQueueService),
                    typeof(IAttachmentService),
                    typeof(IRuleEngineService),
                }
                .Select(ServiceManager.Services.GetService)
                .ToList();
            foreach (var service in services.OfType<IBackgroundService>())
            {
                service.StopTasks();
            }
            foreach (var service in services)
            {
                ServiceManager.Services.UnloadService(service);
            }
            
        }
    protected virtual IParameterService CreateParameterService()
    {
            return new ParameterService();
        }        
        
    protected virtual IWorkQueueService CreateWorkQueueService()
    {
            return new WorkQueueService();
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
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
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