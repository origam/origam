using CommandLine;
using CommandLine.Text;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;

namespace Origam.DocGenerator
{
   class Program
    {
       public class Options
        {
            [Option('s', "schema", Required = true, HelpText = "Input Origam project directory.")]
            public string Schema { get; set; }

            [Option('g', "guiddoc", Required = true, HelpText = "guidid of project in Origam.")]
            public string GuidDoc { get; set; }

            [Option('x', "xslt", Required = false, HelpText = "Xslt template")]
            public string Xslt { get; set; }

            [Option('o', "output", Required = true, HelpText = "Output directory")]
            public string Dataout { get; set; }

            [ParserState]
            public IParserState LastParserState { get; set; }

            [HelpOption]
            public string GetUsage()
            {
                return HelpText.AutoBuild(this,
                  (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
            }
        }

        static void Main(string[] args)
        {
            var options = new Options();
            if (!Parser.Default.ParseArguments(args, options))
                {
                    return;
                }

            var DefaultFolders = new List<ElementName>
            {
                ElementNameFactory.Create(typeof(SchemaExtension)),
                ElementNameFactory.Create(typeof(SchemaItemGroup))
            };
            ServiceManager sManager = ServiceManager.Services;
            SchemaService service = new SchemaService();
            IParameterService parameterService = new NullParameterService();
                
            sManager.AddService(service);
            sManager.AddService(parameterService);

            var settings =new OrigamSettings();
            ConfigurationManager.SetActiveConfiguration(settings);
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("origam_server"), null);
            StateMachineSchemaItemProvider StateMachineSchema = new StateMachineSchemaItemProvider();
            var persistenceService = new FilePersistenceService(DefaultFolders,
               options.Schema);

            sManager.AddService(persistenceService);
            service.AddProvider(StateMachineSchema);

            FilePersistenceProvider persprovider = (FilePersistenceProvider)persistenceService.SchemaProvider;
            //"0fe05b88-11e4-4f28-81bc-7762afa76dc8"
            persistenceService.LoadSchema(new Guid(options.GuidDoc), false, false, "");
            var documentation =  new FileStorageDocumentationService(
                persprovider,
                persistenceService.FileEventQueue);

            if (!new DocCreate(options.Dataout, options.Xslt,  documentation, persprovider).Run())
            {
                System.Console.WriteLine("Neco je spatne");
                Console.ReadKey();
            };
        }
    }
}
