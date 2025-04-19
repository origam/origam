using System.Net.Mime;
using System.Reflection;
using MoreLinq.Extensions;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Extensions;
using Origam.OrigamEngine;
using Origam.Rule;
using Origam.Schema;
using Origam.Workbench.Services;
using Origam.Workflow;

namespace Origam.Architect.Server;

public class Workbench
{
    private static readonly log4net.ILog log 
        = log4net.LogManager.GetLogger(
            MethodBase.GetCurrentMethod().DeclaringType);
    private readonly CancellationTokenSource modelCheckCancellationTokenSource = new ();
    private SchemaService schema;

    public Workbench(SchemaService schema)
    {
        this.schema = schema;
    }

    public bool PopulateEmptyDatabaseOnLoad { get; set; } = true;
    
    public void InitializeDefaultServices()
    {
        ServiceManager.Services.AddService(new MetaModelUpgradeService());
        ServiceManager.Services.AddService(schema);
        schema.SchemaLoaded += _schema_SchemaLoaded;
    }
    
    private void _schema_SchemaLoaded(object sender, bool isInteractive)
	{
        OrigamEngine.OrigamEngine.InitializeSchemaItemProviders(schema);
        IDeploymentService deployment 
            = ServiceManager.Services.GetService<IDeploymentService>();
		IParameterService parameterService 
			= ServiceManager.Services.GetService<IParameterService>();

        bool isEmpty = deployment.IsEmptyDatabase();
        // data database is empty and we are not supposed to ask for running init scripts
        // that means the new project wizard is running and will take care
        if (isEmpty && !PopulateEmptyDatabaseOnLoad)
        {
            return;
        }
        if (isEmpty)
        {
            deployment.Deploy();
        }
        RunDeploymentScripts(deployment, isInteractive);
		try
		{
			parameterService.RefreshParameters();
		}
		catch
		{
			// show the error but go on
			// error can occur e.g. when duplicate constant name is loaded, e.g. due to incompatible packages
			// AsMessageBox.ShowError(this, ex.Message, strings.ErrorWhileLoadingParameters_Message, ex);
		}

        // we have to initialize the new user after parameter service gets loaded
        // otherwise it would fail generating SQL statements
        if (isEmpty)
        {
            string userName = SecurityManager.CurrentPrincipal.Identity.Name;
            // if (MessageBox.Show(string.Format(strings.AddUserToUserList_Question,
            //     userName),
            //     strings.DatabaseEmptyTitle, MessageBoxButtons.YesNo, MessageBoxIcon.Question,
            //     MessageBoxDefaultButton.Button1) == DialogResult.Yes)
            // {
                IOrigamProfileProvider profileProvider = SecurityManager.GetProfileProvider();
                profileProvider.AddUser("Architect (" + userName + ")", userName);
            // }
        }
        // UpdateTitle();
	}
    
    private void RunDeploymentScripts(IDeploymentService deployment, bool isInteractive)
    {
       
    }
    
    public void Connect(string configurationName)
    {
        if (!LoadConfiguration(configurationName))
        {
            return;
        }

        try
        {
            // _statusBarService.SetStatusText(strings
            //     .ConnectingToModelRepository_StatusText);
            InitPersistenceService();
            // _schema.SchemaBrowser = _schemaBrowserPad;
            // Init services
            InitializeConnectedServices();
            // Initialize model-connected user interface
            // InitializeConnectedPads();
            // CreateMainMenuConnect();
            // IsConnected = true;
            RunBackgroundInitializationTasks();
            // UpdateTitle();
        }
        finally
        {
            // _statusBarService.SetStatusText("");
            // this.WindowState = FormWindowState.Maximized;
        }

        // ViewExtensionPad cmd = new ViewExtensionPad();
        // cmd.Run();
    }

    private void InitPersistenceService()
    {
        IPersistenceService persistence =
            OrigamEngine.OrigamEngine.CreatePersistenceService();
        ServiceManager.Services.AddService(persistence);
    }

    private bool LoadConfiguration(string configurationName)
    {
        string origamSettingsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"OrigamSettings.config");
        OrigamSettingsCollection configurations =
            ConfigurationManager.GetAllUserHomeConfigurations(origamSettingsPath);
        if (configurationName == null)
        {
            return false;
        }
        var newConfiguration = configurations
            .Cast<OrigamSettings>()
            .FirstOrDefault(x => x.Name == configurationName);
        if (newConfiguration != null)
        {
            ConfigurationManager.SetActiveConfiguration(newConfiguration);
            return true;
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(configurationName),
                configurationName,
                "Configuartion not found");
        }
    }

    private void InitializeConnectedServices()
    {
        ServiceManager.Services.AddService(
            new ServiceAgentFactory(externalAgent =>
                new ExternalAgentWrapper(externalAgent)));
        ServiceManager.Services.AddService(new StateMachineService());
        ServiceManager.Services.AddService(OrigamEngine.OrigamEngine
            .CreateDocumentationService());
        ServiceManager.Services.AddService(new TracingService());
        ServiceManager.Services.AddService(new DataLookupService());
        // ServiceManager.Services.AddService(new ControlsLookUpService());
        ServiceManager.Services.AddService(new DeploymentService());
        ServiceManager.Services.AddService(new ParameterService());
        ServiceManager.Services.AddService(
            new Origam.Workflow.WorkQueue.WorkQueueService());
        ServiceManager.Services.AddService(new AttachmentService());
        ServiceManager.Services.AddService(new RuleEngineService());
    }
    public void RunBackgroundInitializationTasks()
    {
        var currentPersistenceService =
            ServiceManager.Services.GetService<IPersistenceService>();
        if (!(currentPersistenceService is FilePersistenceService)) return;
        var cancellationToken =
            modelCheckCancellationTokenSource.Token;
        Task.Factory.StartNew(() =>
        {
            using (FilePersistenceService independentPersistenceService =
                   new FilePersistenceBuilder()
                       .CreateNoBinFilePersistenceService())
            {
                IndexReferences(
                    independentPersistenceService, 
                    cancellationToken);
                DoModelChecks(
                    independentPersistenceService,
                    cancellationToken);
            }
        }, cancellationToken).ContinueWith(TaskErrorHandler);
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
                .InnerExceptions
                .Any(x => !(x is OperationCanceledException));
            if (actualExceptionsExist)
            {
                log.LogOrigamError(ae);
                // this.RunWithInvoke(() => AsMessageBox.ShowError(
                //     this, ae.Message, strings.GenericError_Title, ae));
            }
        }
    }
    private void IndexReferences(FilePersistenceService independentPersistenceService,
        CancellationToken cancellationToken)
    {
        try
        {
            // _statusBarService.SetStatusText("Indexing references...");
            ReferenceIndexManager.Clear(false);				
            independentPersistenceService
                .SchemaProvider
                .RetrieveList<IFilePersistent>()
                .OfType<ISchemaItem>()
                .AsParallel()
                .ForEach(item =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ReferenceIndexManager.Add(item);
                });				
            ReferenceIndexManager.Initialize();
        }
        finally
        {
            // _statusBarService.SetStatusText("");
        }
    }
    private void DoModelChecks(
        FilePersistenceService independentPersistenceService,
        CancellationToken cancellationToken)
    {
        List<Dictionary<ISchemaItem, string>> errorFragments =
            ModelRules.GetErrors(
                schemaProviders: new OrigamProviderBuilder()
                    .SetSchemaProvider(independentPersistenceService.SchemaProvider)
                    .GetAll(), 
                independentPersistenceService: independentPersistenceService, 
                cancellationToken: cancellationToken); 
        var persistenceProvider = (FilePersistenceProvider)independentPersistenceService.SchemaProvider;
        var errorSections = persistenceProvider.GetFileErrors(
            ignoreDirectoryNames: new []{ ".git","l10n"},
            cancellationToken: cancellationToken);
        if (errorFragments.Count != 0)
        {
            // FindRulesPad resultsPad = WorkbenchSingleton.Workbench.GetPad(typeof(FindRulesPad)) as FindRulesPad;

            // DialogResult dialogResult = MessageBox.Show(
            //     "Some model elements do not satisfy model integrity rules. Do you want to show the rule violations?",
            //     "Model Errors",
            //     MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
            // if (dialogResult == DialogResult.Yes)
            // {
            //     resultsPad.DisplayResults(errorFragments);
            // }

        }
        // if (errorSections.Count != 0)
        // {
        //     this.RunWithInvoke(() =>
        //     {
        //         var modelCheckResultWindow = new ModelCheckResultWindow(errorSections);
        //         modelCheckResultWindow.Show(this);
        //     });
        // }
    }
}