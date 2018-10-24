using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;

namespace Origam.DocGenrator
{
    class Program
    {
        //private static string schemapath = "D:\\OrigamProjects\\test";
        private static string schemapath = "D:\\OrigamProjects\\be-model";
        private static string xmlpath = "D:\\prace";
        static void Main(string[] args)
        {
            LoadArgs(args);
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
               schemapath);
            sManager.AddService(persistenceService);
            service.AddProvider(StateMachineSchema);
            FilePersistenceProvider persprovider = (FilePersistenceProvider)persistenceService.SchemaProvider;
            //var independentPackage = persprovider
            //     .RetrieveList<SchemaExtension>()
            //     .FirstOrDefault(package => package.IncludedPackages.Count == 0)
            //     ?? throw new Exception("cannot find..");
            //independentPackage.Id
            persistenceService.LoadSchema(new Guid("0fe05b88-11e4-4f28-81bc-7762afa76dc8"), false, false, "");
            var documentation =  new FileStorageDocumentationService(
                persprovider,
                persistenceService.FileEventQueue);

            XmlCreate xmlfile = new XmlCreate(xmlpath, "testovacifile.xml", documentation, persprovider);
            xmlfile.Run();
        }

        private static void LoadArgs(string[] args)
        {
            if (args.Length == 2)
            {
                schemapath = args[0];
                xmlpath = args[1];
            }
        }
    }
}
