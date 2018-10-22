using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;
using System;
using System.Collections.Generic;

namespace Origam.MdExporter
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
            var settings =new OrigamSettings();
            ConfigurationManager.SetActiveConfiguration(settings);
            var persistenceService = new FilePersistenceService(DefaultFolders,
                schemapath);
           FilePersistenceProvider persprovider = (FilePersistenceProvider)persistenceService.SchemaProvider;
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
