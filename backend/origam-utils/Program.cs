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

using CommandLine;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using BrockAllen.IdentityReboot;
using MoreLinq.Extensions;
using Origam.Extensions;
using Origam.Utils.Sql;

namespace Origam.Utils;

public class Program
{
    private static QueueProcessor queueProcessor;
    private delegate bool EventHandler(CtrlType sig);
    private static EventHandler cancelHandler;
    private static readonly log4net.ILog log =
        log4net.LogManager.GetLogger(type: System.Reflection.MethodBase
            .GetCurrentMethod().DeclaringType);
    [DllImport(dllName: "Kernel32")]
    private static extern bool SetConsoleCtrlHandler(EventHandler handler,
        bool add);
    private enum CtrlType
    {
        CTRL_C_EVENT = 0,
        CTRL_BREAK_EVENT = 1,
        CTRL_CLOSE_EVENT = 2,
        CTRL_LOGOFF_EVENT = 5,
        CTRL_SHUTDOWN_EVENT = 6
    }
    [Verb(name: "create-hash-index",
        HelpText =
            "Creates hash index file on the contents of the given folder.")]
    class CreateHashIndexOptions
    {
        [Option(shortName: 'i', longName: "input", Required = true,
            HelpText = "Folder for which the index will be created.")]
        public string Input { get; set; }
        [Option(shortName: 'm', longName: "mask", Required = true,
            HelpText = "Search pattern.")]
        public string Mask { get; set; }
        [Option(shortName: 'o', longName: "output", Required = true,
            HelpText = "Path/Name of file where the index will be stored.")]
        public string Output { get; set; }
    }
    [Verb(name: "run-scripts", HelpText = "Runs update scripts.")]
    class RunUpdateScriptsOptions
    {
    }
    [Verb(name: "restart-server", HelpText = "Invokes server restart.")]
    class RestartServerOptions
    {
    }
    [Verb(name: "compare-schema",
        HelpText =
            "Compares schema with database. If no comparison switches are defined, no comparison is done. More than one switch can be enabled.")]
    class CompareSchemaOptions
    {
        [Option(shortName: 'd', longName: "missing-in-db", Default = false,
            HelpText = "Display elements missing in database.")]
        public bool MissingInDb { get; set; }
        [Option(shortName: 's', longName: "missing-in-schema", Default = false,
            HelpText = "Display elements missing in schema.")]
        public bool MissingInSchema { get; set; }
        [Option(shortName: 'x', longName: "existing-but-different", Default = false,
            HelpText = "Display elements, that exist but are different.")]
        public bool ExistingButDifferent { get; set; }
    }
    [Verb(name: "process-queue", HelpText = "Process a queue.")]
    private class ProcessQueueOptions
    {
        [Option(shortName: 'c', longName: "queueCode", Required = true,
            HelpText = "Reference code of the queue to process.")]
        public string QueueRefCode { get; set; }
        [Option(shortName: 'p', longName: "parallelism", Required = true,
            HelpText = "MaxDegreeOfParallelism.")]
        public int Parallelism { get; set; }
        [Option(shortName: 'w', longName: "forceWait", Required = false, Default = 0,
            HelpText =
                "Delay between processing of queue items in milliseconds.")]
        public int ForceWaitMs { get; set; }
        [Option(shortName: 'v', longName: "verbose", Default = true,
            HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }
    }
    [Verb(name: "process-check-rules", HelpText = "Check rules in project.")]
    class ProcessCheckRulesOptions
    {
    }
    
    [Verb(name: "get-root-version", HelpText = "Try get Root package version to test if the model has been initialized in the database. Return 0 if successful.")]
    public class GetRootVersionOptions
    {
        [Option(shortName: 'a', longName: "attempts", Required = true,
            HelpText = "Number of test attempts.")]
        public int Attempts { get; set; }
        [Option(shortName: 'd', longName: "delay", Required = true,
            HelpText = "Delay between attempts in milliseconds.")]
        public int Delay { get; set; }
    } 
    [Verb(name: "run-sql", HelpText = "Try to connect to database and run a sql command. Return 0 if successful.")]
    public class RunSqlCommandOptions
    {
        [Option(shortName: 'a', longName: "attempts", Required = true,
            HelpText = "Number of test attempts.")]
        public int Attempts { get; set; }
        [Option(shortName: 'd', longName: "delay", Required = true,
            HelpText = "Delay between attempts in milliseconds.")]
        public int Delay { get; set; }
        [Option(shortName: 'c', longName: "sql-command", Required = true,
            HelpText = "SQL command to run.")]
        public string SqlCommand { get; set; }
    } 
    [Verb(name: "run-sql-procedure", HelpText = "Try to connect to database and run a sql procedure. Return 0 if successful.")]
    public class RunSqlProcedureCommandOptions
    {
        [Option(shortName: 'a', longName: "attempts", Required = true,
            HelpText = "Number of test attempts.")]
        public int Attempts { get; set; }
        [Option(shortName: 'd', longName: "delay", Required = true,
            HelpText = "Delay between attempts in milliseconds.")]
        public int Delay { get; set; }
        [Option(shortName: 'c', longName: "sql-command", Required = true,
            HelpText = "SQL procedure to run.")]
        public string ProcedureName { get; set; }
    }
    [Verb(name: "process-doc-generator",
        HelpText = "Generate Menu into output with xslt template.")]
    class ProcessDocGeneratorOptions
    {
        [Option(shortName: 'o', longName: "output", Required = true,
            HelpText = "Output directory")]
        public string Output { get; set; }
        [Option(shortName: 'l', longName: "language", Required = true,
            HelpText = "Localization(ie. cs-CZ).")]
        public string Language { get; set; }
        [Option(shortName: 'x', longName: "xslt", Required = true, HelpText = "Xslt template")]
        public string Xslt { get; set; }
        [Option(shortName: 'r', longName: "rootfilename", Required = true,
            HelpText = "Output File")]
        public string RootFile { get; set; }
    }
    [Verb(name: "normalize-file-format",
        HelpText =
            "Formats all files in the model according to the actual formatting rules.")]
    class NormalizeFileFormatOptions
    {
    }
    [Verb(name: "generate-password-hash",
        HelpText =
            "Generate hash of supplied password. The hash can be inserted into column Password in OrigamUser table as development password reset.")]
    class GeneratePassHashOptions
    {
        [Option(shortName: 'p', longName: "password", Required = true,
            HelpText = "String to hash")]
        public string Password { get; set; }
    }
    private static bool CancelHandler(CtrlType sig)
    {
        log.Info(message: "Exiting system due to external CTRL-C," +
                          " or process kill, or shutdown, please wait...");
        queueProcessor.Cancel();
        log.Info(message: "Cleanup complete");
        Environment.Exit(exitCode: -1);
        return true;
    }
    static int Main(string[] args)
    {
        return Parser.Default.ParseArguments(args: args, types: GetVerbs())
            .MapResult(parsedFunc: Run, notParsedFunc: errors => 1);
    }
    private static int Run(object parsedOptions)
    {
        switch (parsedOptions)
        {
            case ProcessCheckRulesOptions options:
            {
                EntryAssembly();
                return ProcessRule(invokedVerbInstance: options);
            }
            case ProcessDocGeneratorOptions options:
            {
                EntryAssembly();
                return ProcessDocGenerator(config: options);
            }
            case GeneratePassHashOptions options:
            {
                EntryAssembly();
                return HashPassword(options: options);
            }
#if !NETCORE2_1
            case ProcessQueueOptions options:
            {
                EntryAssembly();
                return ProcessQueue(options: options);
            }
            case RunUpdateScriptsOptions _:
            {
                EntryAssembly();
                return RunUpdateScripts();
            }
            case RestartServerOptions _:
            {
                EntryAssembly();
                return RestartServer();
            }
            case CreateHashIndexOptions options:
            {
                EntryAssembly();
                return CreateHashIndex(options: options);
            }
            case CompareSchemaOptions options:
            {
                EntryAssembly();
                return CompareSchema(options: options);
            }
            case GetRootVersionOptions options:
            {
                return SqlRunner.Create(log: log).GetRootVersion(arguments: options);
            }
            case RunSqlCommandOptions options:
            {
                return SqlRunner.Create(log: log).RunSqlCommand(arguments: options);
            }
            case RunSqlProcedureCommandOptions options:
            {
                return SqlRunner.Create(log: log).RunSqlProcedure(arguments: options);
            }
            case NormalizeFileFormatOptions _:
            {
                EntryAssembly();
                return NormalizeFileFormat();
            }
#endif
            default:
            {
                return 1;
            }
        }
        
    }
    private static Type[] GetVerbs()
    {
        return new[] 
        {
            typeof(ProcessCheckRulesOptions),
            typeof(ProcessDocGeneratorOptions),
            typeof(GeneratePassHashOptions)
        #if !NETCORE2_1
            ,
            typeof(GetRootVersionOptions),
            typeof(RunSqlCommandOptions),
            typeof(RunSqlProcedureCommandOptions),
            typeof(ProcessQueueOptions),
            typeof(RunUpdateScriptsOptions),
            typeof(RestartServerOptions),
            typeof(CreateHashIndexOptions),
            typeof(CompareSchemaOptions),
            typeof(NormalizeFileFormatOptions)
        #endif
        };
    }
    private static void EntryAssembly()
    {
        Console.WriteLine(value: string.Format(format: Strings.ShortGnu,
                arg0: System.Reflection.Assembly.GetEntryAssembly().GetName().Name));
    }
    private static int NormalizeFileFormat()
    {
        OrigamSettingsCollection configurations;
        try
        {
            configurations = ConfigurationManager.GetAllConfigurations();
            if (configurations.Count != 1)
            {
                Console.WriteLine(
                    value: "OrigamSettings.config doesn't contain exactly one configuration.");
                return -1;
            }
        } catch
        {
            Console.WriteLine(value: "Failed to load OrigamSettings.config");
            return -1;
        }
		Directory
            .EnumerateFiles(
                path: configurations[index: 0].ModelSourceControlLocation, 
                searchPattern: "*.origam", 
                searchOption: SearchOption.AllDirectories)
            .AsParallel()
            .ForEach(action: path =>
            {
                Console.Write(value: "Processing ");
                Console.Write(value: path);
                Console.Write(value: "...");
                var source = new OrigamXmlDocument(pathToXml: path);
                var xmlToWrite = OrigamDocumentSorter
                    .CopyAndSort(doc: source)
                    .ToBeautifulString();
                File.WriteAllText(path: path, contents: xmlToWrite);
                Console.WriteLine(value: "DONE");
            });
        return 0;
    }
    private static int HashPassword(GeneratePassHashOptions options)
    {
        var hash =
            new AdaptivePasswordHasher().HashPassword(password: options.Password);
        log.Info(message: "");
        log.Info(message: "Password: " + options.Password);
        log.Info(message: "Hash: " + hash);
        return 0;
    }
    private static int ProcessDocGenerator(ProcessDocGeneratorOptions config)
    {
        Thread.CurrentThread.CurrentUICulture 
            = new CultureInfo(name: config.Language);
        var runtimeServiceFactory = new RuntimeServiceFactoryProcessor();
        OrigamEngine.OrigamEngine.ConnectRuntime(
            customServiceFactory: runtimeServiceFactory);
        var settings = ConfigurationManager.GetActiveConfiguration();
        var persistenceService 
            = ServiceManager.Services.GetService<FilePersistenceService>();
        var menuProvider = new MenuSchemaItemProvider
        {
            PersistenceProvider 
                = (FilePersistenceProvider)persistenceService.SchemaProvider
        };
        var persistenceProvider 
            = (FilePersistenceProvider)persistenceService.SchemaProvider;
        persistenceService.LoadSchema(schemaExtensionId: settings.DefaultSchemaExtensionId);
        var documentation = new FileStorageDocumentationService(
            persistenceService: persistenceProvider,
            fileEventQueue: persistenceService.FileEventQueue);
        new DocProcessor(path: config.Output, xslt: config.Xslt, rootfile: config.RootFile,
            documentation: documentation,
            menuprovider: menuProvider, persprovider: persistenceService, xmlfile: null).Run();
        return 0;
    }
    private static int ProcessRule(ProcessCheckRulesOptions invokedVerbInstance)
    {
        var rulesProcessor = new RulesProcessor();
        return rulesProcessor.Run();
    }
    private static int ProcessQueue(ProcessQueueOptions options)
    {
        log.Info(message: "------------ Input -------------");
        log.Info(message: $"queueCode: {options.QueueRefCode}");
        log.Info(message: $"parallelism: {options.Parallelism}");
        log.Info(message: $"forceWait_ms: {options.ForceWaitMs}");
        log.Info(message: $"-------------------------------");
        cancelHandler += CancelHandler;
        SetConsoleCtrlHandler(handler: cancelHandler, add: true);
        RunQueueProcessor(options: options);
        log.Info(message: "Exiting...");
        return 0;
    }
    private static void RunQueueProcessor(ProcessQueueOptions options)
    {
        try
        {
            log.Info(message: options);
            queueProcessor = new QueueProcessor(
                queueRefCode: options.QueueRefCode,
                parallelism: options.Parallelism,
                forceWait_ms: options.ForceWaitMs
            );
            queueProcessor.Run();
        }
        catch (Exception ex)
        {
            log.LogOrigamError(ex: ex);
        }
    }
    private static int RunUpdateScripts()
    {
        if (log.IsInfoEnabled)
        {
            log.Info(message: "Running update scripts...");
        }
        OrigamEngine.OrigamEngine.ConnectRuntime(
            runRestartTimer: false, loadDeploymentScripts: true);
        var deployment
            = ServiceManager.Services.GetService<IDeploymentService>();
        deployment.Deploy();
        return 0;
    }
    private static int RestartServer()
    {
        OrigamEngine.OrigamEngine.ConnectRuntime(runRestartTimer: false);
        RestartServerInternal();
        return 0;
    }
    private static void RestartServerInternal()
    {
        if (log.IsInfoEnabled)
        {
            log.Info(message: "Invoking server restart...");
        }
        OrigamEngine.OrigamEngine.SetRestart();
    }
    private static int CreateHashIndex(CreateHashIndexOptions options)
    {
        if (log.IsInfoEnabled)
        {
            log.InfoFormat(
                format: "Creating hash index file {1} on folder {0} with pattern {2}.",
                arg0: options?.Input, arg1: options?.Output, arg2: options?.Mask);
        }
        var fileNames 
            = Directory.GetFiles(path: options.Input, searchPattern: options.Mask);
        var hashIndexFile = new HashIndexFile(indexFile: options.Output);
        foreach (var filename in fileNames)
        {
            if (log.IsInfoEnabled)
            {
                log.InfoFormat(format: "Hashing {0}...", arg0: filename);
            }
            hashIndexFile.AddEntryToIndexFile(
                entry: hashIndexFile.CreateIndexFileEntry(filename: filename));
        }
        if (log.IsInfoEnabled)
        {
            log.Info(message: "Hash index file finished.");
        }
        hashIndexFile.Dispose();
        return 0;
    }
    private static int CompareSchema(CompareSchemaOptions options)
    {
        if (!options.MissingInDb 
            && !options.MissingInSchema
            && !options.ExistingButDifferent)
        {
            if (log.IsInfoEnabled)
            {
                log.Info(message: "No comparison switches enabled...");
            }
            return 0;
        }
        OrigamEngine.OrigamEngine.ConnectRuntime(runRestartTimer: false);
        if (log.IsInfoEnabled)
        {
            log.Info(
                message: $@"Comparing schema with database: missing in database({
                    options.MissingInDb}), missing in schema({
                        options.MissingInSchema}), existing but different({
                            options.ExistingButDifferent})...");
        }
        var persistenceService
            = ServiceManager.Services.GetService<IPersistenceService>();
        var settings = ConfigurationManager.GetActiveConfiguration();
        var dataService = new MsSqlDataService(
            connection: settings.DataConnectionString,
            bulkInsertThreshold: settings.DataBulkInsertThreshold,
            updateBatchSize: settings.DataUpdateBatchSize);
        dataService.PersistenceProvider = persistenceService.SchemaProvider;
        List<SchemaDbCompareResult> results = dataService.CompareSchema(
            provider: persistenceService.SchemaProvider);
        if (results.Count == 0)
        {
            if (log.IsInfoEnabled)
            {
                log.Info(message: "No differences found.");
            }
            return 0;
        }
        return DisplaySchemaComparisonResults(options: options, results: results);
    }
    private static int DisplaySchemaComparisonResults(
        CompareSchemaOptions options, List<SchemaDbCompareResult> results)
    {
        var existingButDifferent = new List<SchemaDbCompareResult>();
        var missingInDatabase = new List<SchemaDbCompareResult>();
        var missingInSchema = new List<SchemaDbCompareResult>();
        foreach (SchemaDbCompareResult result in results)
        {
            switch (result.ResultType)
            {
                case DbCompareResultType.ExistingButDifferent:
                {
                    existingButDifferent.Add(item: result);
                    break;
                }
                case DbCompareResultType.MissingInDatabase:
                {
                    missingInDatabase.Add(item: result);
                    break;
                }
                case DbCompareResultType.MissingInSchema:
                {
                    missingInSchema.Add(item: result);
                    break;
                }
            }
        }
        var displayedResultsCount = 0;
        if (options.MissingInDb)
        {
            DisplayComparisonResultGroup(
                results: missingInDatabase, header: "Missing in Database:");
            displayedResultsCount += missingInDatabase.Count;
        }
        if (options.MissingInSchema)
        {
            DisplayComparisonResultGroup(
                results: missingInSchema, header: "Missing in Schema:");
            displayedResultsCount += missingInSchema.Count;
        }
        if (options.ExistingButDifferent)
        {
            DisplayComparisonResultGroup(
                results: existingButDifferent, header: "Existing But Different:");
            displayedResultsCount += existingButDifferent.Count;
        }
        if (displayedResultsCount == 0)
        {
            if (log.IsInfoEnabled)
            {
                log.Info(message: "No differences found.");
            }
            return 0;
        }
        return 1;
    }
    private static void DisplayComparisonResultGroup(
        List<SchemaDbCompareResult> results, string header)
    {
        if ((results.Count > 0) && log.IsInfoEnabled)
        {
            log.Info(message: header);
            foreach (SchemaDbCompareResult result in results)
            {
                log.Info(
                    message: $@"{result?.SchemaItemType.SchemaItemDescription()?.Name} {
                        result?.ItemName} {result?.Remark}");
            }
        }
    }
}