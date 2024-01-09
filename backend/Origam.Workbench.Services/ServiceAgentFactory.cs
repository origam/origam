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
using System.Collections.Generic;
using Origam.Schema.WorkflowModel;
using Origam.Services;
using System.Reflection;
using System.Linq;
using Origam.Service.Core;
using Origam.Workflow;

namespace Origam.Workbench.Services
{
    /// <summary>
    /// Summary description for ServiceAgentFactory.
    /// </summary>
    public class ServiceAgentFactory : IBusinessServicesService
    {
        private readonly Func<IExternalServiceAgent, IServiceAgent> fromExternalAgent;

        private static readonly log4net.ILog log
            = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);   
        private IPersistenceService _persistence;
        
        public ServiceAgentFactory(Func<IExternalServiceAgent, IServiceAgent> fromExternalAgent)
        {
            this.fromExternalAgent = fromExternalAgent;
            _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
        }

        public IServiceAgent GetAgent(string serviceName, object ruleEngine, object workflowEngine)
        {
		    var schema = ServiceManager.Services.GetService<SchemaService>();
		    var services = schema.GetProvider<ServiceSchemaItemProvider>();

            Schema.WorkflowModel.Service service = 
                services.GetChildByName(serviceName, Schema.WorkflowModel.Service.CategoryConst) as Schema.WorkflowModel.Service;

            object agent = InstantiateObject(serviceName, service);
            if (agent == null)
		    {
			    throw new ArgumentOutOfRangeException("serviceName", serviceName, ResourceUtils.GetString("ErrorUnknownService"));
		    }

            IServiceAgent result = agent is IExternalServiceAgent externalAgent
                ? fromExternalAgent(externalAgent)
                : agent as IServiceAgent;
	
            result.RuleEngine = ruleEngine;
            result.WorkflowEngine = workflowEngine;
            result.PersistenceProvider = _persistence.SchemaProvider;

            return result;
        }

        private static object InstantiateObject(string serviceName,
            Origam.Schema.WorkflowModel.Service service)
        {
            if (service.ClassPath != null && service.ClassPath != string.Empty)
            {
                string[] classPath = service.ClassPath.Split(",".ToCharArray());

                string className = classPath[0];
                string assembly = String.Join(",", classPath.Skip(1));
                return Reflector.InvokeObject(className, assembly);
            }
            else
            {
                return Reflector.InvokeObject(
                    "Origam.Workflow." + serviceName
                                       + "." + serviceName + "Agent"
                    , "Origam.Workflow." + serviceName);
            }
        }

        #region IBusinessServicesService Members

        public IServiceAgent GetAgent(string serviceType, string instanceName, object ruleEngine, object workflowEngine)
        {
            // TODO:  Add ServiceAgentFactory.Origam.Workbench.Services.IBusinessServicesService.GetAgent implementation
            return null;
        }

        #endregion

        #region IService Members
        
        public void UnloadService()
        {
            _persistence = null;
        }

        public void InitializeService()
        {            
            // TODO:  Add ServiceAgentFactory.InitializeService implementation
		}

		#endregion
	}
}
