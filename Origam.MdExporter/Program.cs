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
        private static string schemapath = "D:\\OrigamProjects\\test";
        //private static string schemapath = "D:\\gitave\\be-model";
        
        private static string xmlpath = "D:\\prace";
       
       
        static void Main(string[] args)
        {
            loadArgs(args);
            
            MenuSchemaItemProvider menuprovider = new MenuSchemaItemProvider();
           
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
           // IDocumentationService documentation = ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;

           var documentation =  new FileStorageDocumentationService(
                persprovider,
                persistenceService.FileEventQueue);

            menuprovider.PersistenceProvider = persprovider;
            var menulist = menuprovider.ChildItems.ToList();
            XmlCreate xmlfile = new XmlCreate(xmlpath, "testovacifile", documentation);
            xmlfile.WriteElement("Root");
            foreach (AbstractSchemaItem menuitem in menulist[0].ChildItems)
            {
                xmlfile.CreateXml(menuitem);
                System.Console.WriteLine(menuitem.Name);

            }
           // xmlfile.WriteEndElement();
            xmlfile.CloseXml();

            Console.ReadKey();

            //System.Console.WriteLine(consolestring);
        }

        private static void loadArgs(string[] args)
        {
            if (args.Length == 2)
            {
                schemapath = args[0];
                xmlpath = args[1];
            }
        }
    }
}
