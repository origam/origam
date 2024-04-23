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

namespace Origam.Workbench.Services;

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
            IServiceAgent result;

            switch (serviceName)
            {
                case "DataService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.DataServiceAgent", "Origam.Workflow");
                    break;

                case "DataTransformationService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.TransformationAgent", "Origam.Workflow");
                    break;

                case "Simplicor.WarehouseService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.SimplicorService.WarehouseServiceAgent", "Origam.Workflow.SimplicorService");
                    break;

                case "MailService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.MailServiceAgent", "Origam.Workflow");
                    break;

                case "ReportService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.ReportServiceAgent", "Origam.Workflow");
                    break;

                case "FileSystemService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.FileSystemServiceAgent", "Origam.Workflow");
                    break;

                case "WorkflowService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.WorkflowServiceAgent", "Origam.Workflow");
                    break;

                case "OperatingSystemService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.OperatingSystemServiceAgent", "Origam.Workflow");
                    break;

                case "ExcelService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.FileService.ExcelAgent", "Origam.Workflow.FileService");
                    break;

                case "FileService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.FileService.FileServiceAgent", "Origam.Workflow.FileService");
                    break;

                case "HttpService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.HttpServiceAgent", "Origam.Workflow");
                    break;

                case "PrintService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.PrintServiceAgent", "Origam.Workflow");
                    break;

                case "XmlJsonConvertService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.XmlJsonConvertServiceAgent", "Origam.Workflow");
                    break;

                case "EDIFACT2XMLService":
                    result = (IServiceAgent)Reflector.InvokeObject("Origam.Workflow.EDIFACT2XMLServiceAgent", "Origam.Workflow");
                    break;
                default:
					SchemaService schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
					ServiceSchemaItemProvider services = schema.GetProvider(typeof(ServiceSchemaItemProvider)) as ServiceSchemaItemProvider;

                    Origam.Schema.WorkflowModel.Service service = services.GetChildByName(serviceName, Origam.Schema.WorkflowModel.Service.CategoryConst) as Origam.Schema.WorkflowModel.Service;

                    object agent = InstantiateObject(serviceName, service);
                    if(agent == null)
					{
						throw new ArgumentOutOfRangeException("serviceName", serviceName, ResourceUtils.GetString("ErrorUnknownService"));
					}

                    result = agent is IExternalServiceAgent externalAgent
                        ? fromExternalAgent(externalAgent)
                        : agent as IServiceAgent;
					break;

			}

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