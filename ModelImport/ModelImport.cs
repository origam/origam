using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using Origam;
using Origam.Schema;
using Origam.Workbench.Services;

[assembly: log4net.Config.XmlConfigurator(Watch = true)]
namespace Origam.ModelImport
{
    class ModelImport
    {
        public const string ROOT_DIRECTORY_PARAMETER = "r";

        private static readonly log4net.ILog log 
            = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private IPersistenceService persistenceService;

        public ModelImport()
        {
            Initialize();
        }

        static void Main(string[] args)
        {
            ModelImport modelImport = new ModelImport();
            modelImport.Import(args);
        }

        public void Import(string[] arguments)
        {
            Dictionary<string, string> commandArguments 
                = ParseArguments(arguments);
            DirectoryInfo rootDirectory;
            if(commandArguments.ContainsKey(ROOT_DIRECTORY_PARAMETER))
            {
                rootDirectory = new DirectoryInfo(
                    commandArguments[ROOT_DIRECTORY_PARAMETER]);
            }
            else
            {
                log.Info("Root directory not specified, using current directory...");
                rootDirectory = new DirectoryInfo(Environment.CurrentDirectory);
            }
            ModelFileInfo[] modelFiles = GetModelFiles(rootDirectory);
            ArrayList presentPackages = persistenceService.SchemaListProvider
                .RetrieveList(typeof(SchemaExtension), "");
            string transactionId = Guid.NewGuid().ToString();
            log.Info("Import will run under transaction " + transactionId);
            try
            {
                foreach(var modelFileInfo in modelFiles)
                {
                    ImportModelFile(modelFileInfo, presentPackages, transactionId);
                }
                ResourceMonitor.Commit(transactionId);
            }
            catch(Exception)
            {
                log.Fatal("Failed to complete import of the packages. Rolling back...");
                ResourceMonitor.Rollback(transactionId);
            }
        }

        private void ImportModelFile(
            ModelFileInfo modelFileInfo, ArrayList presentPackages, 
            string transactionId)
        {
            log.Info("Importing model " + modelFileInfo.ExtensionName + "...");
            DataSet data = null;
            try
            {
                bool isNewPackage 
                    = IsPackageNew(modelFileInfo.ExtensionId, presentPackages);
                if(! isNewPackage)
                {
                    persistenceService.LoadSchema(
                        new Guid(modelFileInfo.ExtensionId), true, true, transactionId);
                }
                else
                {
                    log.Info("Model " + modelFileInfo.ExtensionName
                        + " is not in DB. It is going to be created");
                }
                data = FillSchemaData(modelFileInfo, isNewPackage);
                persistenceService.MergePackage(
                    new Guid(modelFileInfo.ExtensionId), data, transactionId);
                log.Info("Model " + modelFileInfo.ExtensionName + " imported.");
            }
            catch(Exception ex)
            {
                log.Error(
                    "Failed to import model " + modelFileInfo.ExtensionName, ex);
                throw new Exception(
                    "Failed to import model " + modelFileInfo.ExtensionName, ex);
            }
            finally
            {
                if(data != null)
                {
                    data.Dispose();
                }
            }
        }

        private bool IsPackageNew(string extensionId, ArrayList presentPackages)
        {
            bool retVal = true;
            foreach(SchemaExtension extension in presentPackages)
            {
                if(extension.NodeId == extensionId)
                {
                    retVal = false;
                    break;
                }
            }
            return retVal;
        }

        private DataSet FillSchemaData(
            ModelFileInfo modelFileInfo, bool isNewPackage)
        {
            DataSet data;
            if(isNewPackage)
            {
                data = CreateEmptyDataSet();
            }
            else
            {
                SchemaService schemaService = ServiceManager.Services.GetService(
                    typeof(SchemaService)) as SchemaService;
                data = schemaService.EmptySchema();
            }
            data.EnforceConstraints = false;
            XmlDataDocument xmldata = new XmlDataDocument(data);
            using(XmlReader xmlReader 
                = XmlReader.Create(modelFileInfo.fileInfo.FullName))
            {
                xmldata.Load(xmlReader);
                xmlReader.Close();
            }
            return data;
        }

        private DataSet CreateEmptyDataSet()
        {
            return
                persistenceService.SchemaProvider.DataService
                .GetEmptyDataSet(new Guid(PersistenceService.SchemaDataStructureId));
        }


        private void Initialize()
        {
            log.Info("Initializing tool...");
            AppDomain.CurrentDomain.SetPrincipalPolicy(
                System.Security.Principal.PrincipalPolicy.WindowsPrincipal);
            InitializeOrigamSettings();
            InitializeServices();
            persistenceService = ServiceManager.Services.GetService(
                typeof(IPersistenceService)) as IPersistenceService;
            persistenceService.LoadSchemaList();
            log.Info("Tool initialized...");
        }

        private void InitializeOrigamSettings()
        {
            OrigamSettingsCollection configurations 
                = ConfigurationManager.GetAllConfigurations("OrigamSettings") 
                as OrigamSettingsCollection;
            if(configurations.Count != 1)
            {
                throw new Exception("Expecting just one configuration.");
            }
            ConfigurationManager.SetActiveConfiguration(configurations[0]);
        }

        private void InitializeServices()
        {
            ServiceManager.Services.AddService(new PersistenceService());
            ServiceManager.Services.AddService(new SchemaService());
        }

        private ModelFileInfo[] GetModelFiles(DirectoryInfo rootDirectory)
        {
            log.Info("Searching for model files in " 
                + rootDirectory.FullName + "...");
            FileInfo[] xmlFiles = rootDirectory.GetFiles("*.xml", 
                SearchOption.AllDirectories);
            log.Debug("Found " + xmlFiles.Length + " xml files...");
            log.Debug("Parsing xml files...");
            List<ModelFileInfo> modelFiles = new List<ModelFileInfo>();
            foreach(var fileInfo in xmlFiles)
            {
                ModelFileInfo modelFileInfo = FileInfoToModelFileInfo(fileInfo);
                if(modelFileInfo != null)
                {
                    modelFiles.Add(modelFileInfo);
                }
            }
            log.Info("Found " + modelFiles.Count + " model files...");
            return modelFiles.ToArray();
        }

        private ModelFileInfo FileInfoToModelFileInfo(FileInfo fileInfo)
        {
            using(XmlReader xmlReader = XmlReader.Create(fileInfo.FullName))
            {
                while(xmlReader.Read())
                {
                    if (xmlReader.Name == "SchemaExtension")
                    {
                        ModelFileInfo modelFileInfo = new ModelFileInfo();
                        modelFileInfo.fileInfo = fileInfo;
                        modelFileInfo.ExtensionId = xmlReader.GetAttribute("Id");
                        modelFileInfo.ExtensionName = xmlReader.GetAttribute("Name");
                        log.Info("File " + fileInfo.FullName + " contains model " 
                            + modelFileInfo.ExtensionName 
                            + " [" + modelFileInfo.ExtensionId + "]");
                        xmlReader.Close();
                        return modelFileInfo;
                    }
                }
            }
            log.Warn("File " + fileInfo.FullName 
                + " doesn't seem to contain model. Skipping...");
            return null;
        }

        private Dictionary<string, string> ParseArguments(string[] arguments)
        {
            log.Info("Parsing arguments...");
            Regex regex = new Regex(@"/(?<name>.+?):(?<val>.+)");
            Dictionary<string, string> commandArguments 
                = new Dictionary<string, string>();
            foreach(string input in arguments)
            {
                Match match = regex.Match(input);
                if(match.Success)
                {
                    commandArguments.Add(
                        match.Groups[1].Value, match.Groups[2].Value);
                }
            }
            log.Info("Arguments parsed...");
            return commandArguments;
        }
    }

    class ModelFileInfo
    {
        public FileInfo fileInfo;

        public string ExtensionId;

        public string ExtensionName;
    }
}
