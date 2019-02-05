#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using Origam.Schema;
using Origam.Workbench.Services;
using Origam.Rule;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.Schema.WorkflowModel.WorkQueue;
using Origam.Schema.GuiModel;
using Origam.Schema.RuleModel;
using Origam.Schema.MenuModel;
using Origam.Schema.LookupModel;
using Origam.Schema.DeploymentModel;
using Origam.DA;
using System.Collections.Generic;
using System.Linq;
using Origam.DA.ObjectPersistence;
using System.Security.Principal;

namespace Origam.OrigamEngine
{
	/// <summary>
	/// Summary description for OrigamEngine.
	/// </summary>
	public class OrigamEngine
	{
        #region Private Members
        private const string LAST_RESTART_REQUEST_CONST_NAME = "LastRestartRequest";
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static System.Timers.Timer RestartTimer = new System.Timers.Timer(1000);
        private static DateTime LastRestartRequestDate;
		private static readonly IRuntimeServiceFactory standardServiceFactory = new RuntimeServiceFactory();
        #endregion

        #region Constructors
        internal OrigamEngine()
		{
		}
		#endregion

		
		
		#region Public Static Methods
		public static void InitializeSchemaItemProviders(SchemaService service)
		{
			service.RemoveAllProviders();

            foreach (var schemaItemProvider in new OrigamProviders().GetAllProviders())
            {
                service.AddProvider(schemaItemProvider);
            }

//			service.AddProvider(new StringSchemaItemProvider());
//			service.AddProvider(new FeatureSchemaItemProvider());
//			service.AddProvider(new EntityModelSchemaItemProvider());
//			service.AddProvider(new DataConstantSchemaItemProvider());
//            service.AddProvider(new DatabaseDataTypeSchemaItemProvider());
//            service.AddProvider(new UserControlSchemaItemProvider()); 
//			service.AddProvider(new PanelSchemaItemProvider()); 
//			service.AddProvider(new FormSchemaItemProvider()); 
//			service.AddProvider(new DataStructureSchemaItemProvider()); 
//			service.AddProvider(new FunctionSchemaItemProvider());
//			service.AddProvider(new ServiceSchemaItemProvider());
//			service.AddProvider(new WorkflowSchemaItemProvider());
//			service.AddProvider(new TransformationSchemaItemProvider());
//			service.AddProvider(new ReportSchemaItemProvider());
//			service.AddProvider(new RuleSchemaItemProvider());
//			service.AddProvider(new MenuSchemaItemProvider());
////			service.AddProvider(new TestScenarioSchemaItemProvider());
////			service.AddProvider(new TestChecklistRuleSchemaItemProvider());
//			service.AddProvider(new WorkflowScheduleSchemaItemProvider());
//			service.AddProvider(new ScheduleTimeSchemaItemProvider());
//			service.AddProvider(new DataLookupSchemaItemProvider());
//			service.AddProvider(new DeploymentSchemaItemProvider());
//			service.AddProvider(new GraphicsSchemaItemProvider());
//			service.AddProvider(new StateMachineSchemaItemProvider());
//			service.AddProvider(new WorkQueueClassSchemaItemProvider());
//			service.AddProvider(new ChartSchemaItemProvider());
//			service.AddProvider(new PagesSchemaItemProvider());
//			service.AddProvider(new DashboardWidgetsSchemaItemProvider());
//			service.AddProvider(new StylesSchemaItemProvider());
//			service.AddProvider(new NotificationBoxSchemaItemProvider());
//			service.AddProvider(new TreeStructureSchemaItemProvider());
//			service.AddProvider(new KeyboardShortcutsSchemaItemProvider());
//            service.AddProvider(new SearchSchemaItemProvider());

            AbstractSchemaItem.ExtensionChildItemTypes.Add(new Type[] {typeof(AbstractDataEntity), typeof(EntityMenuAction)});
			AbstractSchemaItem.ExtensionChildItemTypes.Add(new Type[] {typeof(AbstractDataEntity), typeof(EntityReportAction)});
			AbstractSchemaItem.ExtensionChildItemTypes.Add(new Type[] {typeof(AbstractDataEntity), typeof(EntityWorkflowAction)});
			AbstractSchemaItem.ExtensionChildItemTypes.Add(new Type[] {typeof(AbstractDataEntity), typeof(EntityDropdownAction)});

			AbstractSchemaItem.ExtensionChildItemTypes.Add(new Type[] {typeof(AbstractDataStructure), typeof(DataStructureWorkflowMethod)});

			AbstractSchemaItem.ExtensionChildItemTypes.Add(new Type[] {typeof(EntityDropdownAction), typeof(EntityMenuAction)});
			AbstractSchemaItem.ExtensionChildItemTypes.Add(new Type[] {typeof(EntityDropdownAction), typeof(EntityReportAction)});
			AbstractSchemaItem.ExtensionChildItemTypes.Add(new Type[] {typeof(EntityDropdownAction), typeof(EntityWorkflowAction)});

			AbstractSchemaItemProvider.ExtensionChildItemTypes.Add(new Type[] {typeof(PagesSchemaItemProvider), typeof(WorkflowPage)});
		}

		public static void InitializeRuntimeServices()
		{
			standardServiceFactory.InitializeServices();
		}

        public static void UnloadConnectedServices()
        {
            IWorkbenchService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService));
            IBusinessServicesService serviceAgent = ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService;
            IWorkbenchService documentation = ServiceManager.Services.GetService(typeof(IDocumentationService));
            IWorkbenchService tracing = ServiceManager.Services.GetService(typeof(TracingService));
            IDataLookupService dataLookupService = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
            IWorkbenchService deployment = ServiceManager.Services.GetService(typeof(IDeploymentService));
            IWorkbenchService parameter = ServiceManager.Services.GetService(typeof(IParameterService));
            IWorkbenchService stateMachine = ServiceManager.Services.GetService(typeof(IStateMachineService));
            IWorkbenchService attachment = ServiceManager.Services.GetService(typeof(IAttachmentService));
            IWorkbenchService ruleEngine = ServiceManager.Services.GetService(typeof(IRuleEngineService));
            IWorkbenchService workQueue = ServiceManager.Services.GetService(typeof(IWorkQueueService));
            Origam.Workflow.DataServiceAgent dataServiceAgent = serviceAgent?.GetAgent("DataService", null, null) as Origam.Workflow.DataServiceAgent;
            dataServiceAgent?.DisconnectDataService();
            ServiceManager.Services.UnloadService(stateMachine);
            ServiceManager.Services.UnloadService(parameter);
            ServiceManager.Services.UnloadService(deployment);
            ServiceManager.Services.UnloadService(dataLookupService);
            ServiceManager.Services.UnloadService(tracing);
            ServiceManager.Services.UnloadService(documentation);
            ServiceManager.Services.UnloadService(serviceAgent);
            ServiceManager.Services.UnloadService(persistence);
            ServiceManager.Services.UnloadService(attachment);
            ServiceManager.Services.UnloadService(ruleEngine);
            ServiceManager.Services.UnloadService(workQueue);
        }

		public static void ConnectRuntime(
			string configName="", bool runRestartTimer=true,
			bool loadDeploymentScripts=false,
			IRuntimeServiceFactory customServiceFactory=null)
		{
			log.Info("Connecting ORIGAM Runtime.");
			AppDomain.CurrentDomain.SetPrincipalPolicy(
                PrincipalPolicy.NoPrincipal);
            SecurityManager.SetServerIdentity();
			SetActiveConfiguration(configName);
			OrigamSettings settings 
                = ConfigurationManager.GetActiveConfiguration() 
                as OrigamSettings;
			if (customServiceFactory == null)
			{
				standardServiceFactory.InitializeServices();
			}
			else
			{
				customServiceFactory.InitializeServices();
			}
			SchemaService schema 
                = ServiceManager.Services.GetService<SchemaService>();
            IPersistenceService persistence 
                = ServiceManager.Services.GetService<IPersistenceService>();
			persistence.LoadSchemaList();
			log.Info("Loading model " + settings.Name + ", Package ID: " + settings.DefaultSchemaExtensionId.ToString());
			schema.LoadSchema(settings.DefaultSchemaExtensionId, 
                settings.ExtraSchemaExtensionId, false, 
                settings.ExecuteUpgradeScriptsOnStart || loadDeploymentScripts);
			log.Info("Loading model finished successfully. Version loaded: " + schema.ActiveExtension.Version);
			InitializeSchemaItemProviders(schema);
			// upgrade database
			if(settings.ExecuteUpgradeScriptsOnStart)
			{
				log.Info("Checking database version.");
                IDeploymentService deployment 
                    = ServiceManager.Services.GetService<IDeploymentService>();
				deployment.Deploy();
				log.Info("Database version check finished.");
			}
            IParameterService parameterService 
                = ServiceManager.Services.GetService<IParameterService>();
			parameterService.RefreshParameters();
			RestartTimer.Elapsed += RestartTimer_Elapsed;
			LastRestartRequestDate = GetLastRestartRequestDate();
			if (runRestartTimer)
			{
				RestartTimer.Elapsed += RestartTimer_Elapsed;
				RestartTimer.Start();
			}
			log.Info("ORIGAM Runtime Connected");
		}

		private static void SetActiveConfiguration(string configName="")
		{
			OrigamSettingsCollection configurations =
				ConfigurationManager.GetAllConfigurations();
			
			OrigamSettings origamSettings = GetSettings(configName, configurations);
			ConfigurationManager.SetActiveConfiguration(origamSettings);
		}

		private static OrigamSettings GetSettings(string configName,
			OrigamSettingsCollection configurations)
		{
			if (string.IsNullOrEmpty(configName))
			{
				if (configurations.Count != 1)
				{
					throw new Exception(
						ResourceUtils.GetString("ErrorOnlyOneOrigamSettings"));
				}

				return  configurations[0];
			}
			return
				configurations 
				.Cast<OrigamSettings>()
				.FirstOrDefault(settings => settings.Name == configName)
				?? throw new ArgumentException($"Configuration {configName} not found in settings file.");
		}

		public static void SetRestart()
		{
			IParameterService parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
			parameterService.SetCustomParameterValue(LAST_RESTART_REQUEST_CONST_NAME, DateTime.Now, 
				Guid.Empty, 0, null, false, 0, 0, DateTime.Now, false);
		}

        public static IPersistenceService CreatePersistenceService()
        {
	        return standardServiceFactory.CreatePersistenceService();
        }

        public static IDocumentationService CreateDocumentationService()
        {
	        return standardServiceFactory.CreateDocumentationService();
        }

		private static DateTime GetLastRestartRequestDate()
		{
			IParameterService parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
			object value = parameterService.GetCustomParameterValue(LAST_RESTART_REQUEST_CONST_NAME);
			if(value == null)
			{
				return DateTime.MinValue;
			}
			else
			{
				return (DateTime)value;
			}
		}
		#endregion

        public static bool IsRestartPending()
        {
            SecurityManager.SetServerIdentity();
            DateTime newDate = GetLastRestartRequestDate();
            return !LastRestartRequestDate.Equals(newDate);
        }

		#region Event Handlers
		private static void RestartTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
            try
            {
#if ORIGAM_SERVER
                if (IsRestartPending())
                {
                    AppDomain.Unload(AppDomain.CurrentDomain);
                }
#endif
                RestartTimer.Interval = 1000;
            }
            catch (Exception ex)
            {
                RestartTimer.Interval *= 10;
                if(log.IsErrorEnabled)
                {
                    log.Error("Could not get restart status. Will retry in " + RestartTimer.Interval / 1000 + "seconds.", ex);
                }
            }
		}
#endregion
	}
}
