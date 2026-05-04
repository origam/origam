#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using System.Reflection;
using MoreLinq.Extensions;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Extensions;
using Origam.OrigamEngine;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using Origam.Workflow;

namespace Origam.Architect.Server;

public class Workbench
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: MethodBase.GetCurrentMethod().DeclaringType
    );

    private readonly CancellationTokenSource modelCheckCancellationTokenSource = new();
    private SchemaService schema;

    public Workbench(SchemaService schema)
    {
        this.schema = schema;
    }

    public bool PopulateEmptyDatabaseOnLoad { get; set; } = true;

    public void InitializeDefaultServices()
    {
        ServiceManager.Services.AddService(service: new MetaModelUpgradeService());
        ServiceManager.Services.AddService(service: schema);
        schema.SchemaLoaded += _schema_SchemaLoaded;
    }

    private void _schema_SchemaLoaded(object sender, bool isInteractive)
    {
        OrigamEngine.OrigamEngine.InitializeSchemaItemProviders(service: schema);
        IDeploymentService deployment = ServiceManager.Services.GetService<IDeploymentService>();
        IParameterService parameterService =
            ServiceManager.Services.GetService<IParameterService>();

        bool isEmpty = deployment.IsEmptyDatabase();
        switch (isEmpty)
        {
            // data database is empty and we are not supposed to ask for running init scripts
            // that means the new project wizard is running and will take care
            case true when !PopulateEmptyDatabaseOnLoad:
            {
                return;
            }
            case true:
            {
                deployment.Deploy();
                break;
            }
        }

        RunDeploymentScripts(deployment: deployment, isInteractive: isInteractive);
        parameterService.RefreshParameters();
        // we have to initialize the new user after parameter service gets loaded
        // otherwise it would fail generating SQL statements
        if (isEmpty)
        {
            string userName = SecurityManager.CurrentPrincipal.Identity.Name;
            IOrigamProfileProvider profileProvider = SecurityManager.GetProfileProvider();
            profileProvider.AddUser(name: "Architect (" + userName + ")", userName: userName);
        }
    }

    private void RunDeploymentScripts(IDeploymentService deployment, bool isInteractive) { }

    public void Connect()
    {
        if (!LoadConfiguration())
        {
            return;
        }

        InitPersistenceService();
        InitializeConnectedServices();
        RunBackgroundInitializationTasks();
    }

    private void InitPersistenceService()
    {
        IPersistenceService persistence = OrigamEngine.OrigamEngine.CreatePersistenceService();
        ServiceManager.Services.AddService(service: persistence);
    }

    private bool LoadConfiguration()
    {
        string origamSettingsPath = Path.Combine(
            path1: AppDomain.CurrentDomain.BaseDirectory,
            path2: "OrigamSettings.config"
        );
        OrigamSettingsCollection configurations = ConfigurationManager.GetAllUserHomeConfigurations(
            origamSettingsPath: origamSettingsPath
        );
        var configuration = configurations.Cast<OrigamSettings>().FirstOrDefault();
        if (configuration != null)
        {
            ConfigurationManager.SetActiveConfiguration(configuration: configuration);
            return true;
        }

        throw new Exception(message: "Configuration not found");
    }

    private void InitializeConnectedServices()
    {
        ServiceManager.Services.AddService(
            service: new ServiceAgentFactory(
                fromExternalAgent: externalAgent => new ExternalAgentWrapper(
                    externalServiceAgent: externalAgent
                )
            )
        );
        ServiceManager.Services.AddService(service: new StateMachineService());
        ServiceManager.Services.AddService(
            service: OrigamEngine.OrigamEngine.CreateDocumentationService()
        );
        ServiceManager.Services.AddService(service: new TracingService());
        ServiceManager.Services.AddService(service: new DataLookupService());
        ServiceManager.Services.AddService(service: new DeploymentService());
        ServiceManager.Services.AddService(service: new ParameterService());
        ServiceManager.Services.AddService(
            service: new Origam.Workflow.WorkQueue.WorkQueueService()
        );
        ServiceManager.Services.AddService(service: new AttachmentService());
        ServiceManager.Services.AddService(service: new RuleEngineService());

        var settings = ConfigurationManager.GetActiveConfiguration();
        ServiceManager.Services.AddService(service: new DatabaseProfileService(settings: settings));
    }

    public void RunBackgroundInitializationTasks()
    {
        var currentPersistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        if (!(currentPersistenceService is FilePersistenceService))
        {
            return;
        }
        var cancellationToken = modelCheckCancellationTokenSource.Token;
        Task.Factory.StartNew(
                action: () =>
                {
                    using (
                        FilePersistenceService independentPersistenceService =
                            new FilePersistenceBuilder().CreateNoBinFilePersistenceService()
                    )
                    {
                        IndexReferences(
                            independentPersistenceService: independentPersistenceService,
                            cancellationToken: cancellationToken
                        );
                        DoModelChecks(
                            independentPersistenceService: independentPersistenceService,
                            cancellationToken: cancellationToken
                        );
                    }
                },
                cancellationToken: cancellationToken
            )
            .ContinueWith(continuationAction: TaskErrorHandler);
    }

    private void TaskErrorHandler(Task previousTask)
    {
        try
        {
            previousTask.Wait();
        }
        catch (AggregateException ae)
        {
            bool actualExceptionsExist = ae.Flatten()
                .InnerExceptions.Any(predicate: x => !(x is OperationCanceledException));
            if (actualExceptionsExist)
            {
                log.LogOrigamError(ex: ae);
            }
        }
    }

    private void IndexReferences(
        FilePersistenceService independentPersistenceService,
        CancellationToken cancellationToken
    )
    {
        ReferenceIndexManager.Clear(fullClear: false);
        independentPersistenceService
            .SchemaProvider.RetrieveList<IFilePersistent>()
            .OfType<ISchemaItem>()
            .AsParallel()
            .ForEach(action: item =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                ReferenceIndexManager.Add(item: item);
            });
        ReferenceIndexManager.Initialize();
    }

    private void DoModelChecks(
        FilePersistenceService independentPersistenceService,
        CancellationToken cancellationToken
    )
    {
        List<Dictionary<ISchemaItem, string>> errorFragments = ModelRules.GetErrors(
            schemaProviders: new OrigamProviderBuilder()
                .SetSchemaProvider(schemaProvider: independentPersistenceService.SchemaProvider)
                .GetAll(),
            independentPersistenceService: independentPersistenceService,
            cancellationToken: cancellationToken
        );
        var persistenceProvider = (FilePersistenceProvider)
            independentPersistenceService.SchemaProvider;
    }
}
