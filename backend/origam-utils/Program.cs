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
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
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
        log4net.LogManager.GetLogger(System.Reflection.MethodBase
            .GetCurrentMethod().DeclaringType);
    [DllImport("Kernel32")]
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
    [Verb("create-hash-index",
        HelpText =
            "Creates hash index file on the contents of the given folder.")]
    class CreateHashIndexOptions
    {
        [Option('i', "input", Required = true,
            HelpText = "Folder for which the index will be created.")]
        public string Input { get; set; }
        [Option('m', "mask", Required = true,
            HelpText = "Search pattern.")]
        public string Mask { get; set; }
        [Option('o', "output", Required = true,
            HelpText = "Path/Name of file where the index will be stored.")]
        public string Output { get; set; }
    }
    [Verb("run-scripts", HelpText = "Runs update scripts.")]
    class RunUpdateScriptsOptions
    {
    }
    [Verb("restart-server", HelpText = "Invokes server restart.")]
    class RestartServerOptions
    {
    }
    [Verb("compare-schema",
        HelpText =
            "Compares schema with database. If no comparison switches are defined, no comparison is done. More than one switch can be enabled.")]
    class CompareSchemaOptions
    {
        [Option('d', "missing-in-db", Default = false,
            HelpText = "Display elements missing in database.")]
        public bool MissingInDb { get; set; }
        [Option('s', "missing-in-schema", Default = false,
            HelpText = "Display elements missing in schema.")]
        public bool MissingInSchema { get; set; }
        [Option('x', "existing-but-different", Default = false,
            HelpText = "Display elements, that exist but are different.")]
        public bool ExistingButDifferent { get; set; }
    }
    [Verb("process-queue", HelpText = "Process a queue.")]
    private class ProcessQueueOptions
    {
        [Option('c', "queueCode", Required = true,
            HelpText = "Reference code of the queue to process.")]
        public string QueueRefCode { get; set; }
        [Option('p', "parallelism", Required = true,
            HelpText = "MaxDegreeOfParallelism.")]
        public int Parallelism { get; set; }
        [Option('w', "forceWait", Required = false, Default = 0,
            HelpText =
                "Delay between processing of queue items in milliseconds.")]
        public int ForceWaitMs { get; set; }
        [Option('v', "verbose", Default = true,
            HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }
    }
    [Verb("process-check-rules", HelpText = "Check rules in project.")]
    class ProcessCheckRulesOptions
    {
    }
    
    [Verb("get-root-version", HelpText = "Try get Root package version to test if the model has been initialized in the database. Return 0 if successful.")]
    public class GetRootVersionOptions
    {
        [Option('a', "attempts", Required = true,
            HelpText = "Number of test attempts.")]
        public int Attempts { get; set; }
        [Option('d', "delay", Required = true,
            HelpText = "Delay between attempts in milliseconds.")]
        public int Delay { get; set; }
    } 
    [Verb("run-sql", HelpText = "Try to connect to database and run a sql command. Return 0 if successful.")]
    public class RunSqlCommandOptions
    {
        [Option('a', "attempts", Required = true,
            HelpText = "Number of test attempts.")]
        public int Attempts { get; set; }
        [Option('d', "delay", Required = true,
            HelpText = "Delay between attempts in milliseconds.")]
        public int Delay { get; set; }
        [Option('c', "sql-command", Required = true,
            HelpText = "SQL command to run.")]
        public string SqlCommand { get; set; }
    } 
    [Verb("run-sql-procedure", HelpText = "Try to connect to database and run a sql procedure. Return 0 if successful.")]
    public class RunSqlProcedureCommandOptions
    {
        [Option('a', "attempts", Required = true,
            HelpText = "Number of test attempts.")]
        public int Attempts { get; set; }
        [Option('d', "delay", Required = true,
            HelpText = "Delay between attempts in milliseconds.")]
        public int Delay { get; set; }
        [Option('c', "sql-command", Required = true,
            HelpText = "SQL procedure to run.")]
        public string ProcedureName { get; set; }
    }
    [Verb("process-doc-generator",
        HelpText = "Generate Menu into output with xslt template.")]
    class ProcessDocGeneratorOptions
    {
        [Option('o', "output", Required = true,
            HelpText = "Output directory")]
        public string Output { get; set; }
        [Option('l', "language", Required = true,
            HelpText = "Localization(ie. cs-CZ).")]
        public string Language { get; set; }
        [Option('x', "xslt", Required = true, HelpText = "Xslt template")]
        public string Xslt { get; set; }
        [Option('r', "rootfilename", Required = true,
            HelpText = "Output File")]
        public string RootFile { get; set; }
    }
    [Verb("normalize-file-format",
        HelpText =
            "Formats all files in the model according to the actual formatting rules.")]
    class NormalizeFileFormatOptions
    {
    }
    [Verb("generate-password-hash",
        HelpText =
            "Generate hash of supplied password. The hash can be inserted into column Password in OrigamUser table as development password reset.")]
    class GeneratePassHashOptions
    {
        [Option('p', "password", Required = true,
            HelpText = "String to hash")]
        public string Password { get; set; }
    }
    private static bool CancelHandler(CtrlType sig)
    {
        log.Info("Exiting system due to external CTRL-C," +
                 " or process kill, or shutdown, please wait...");
        queueProcessor.Cancel();
        log.Info("Cleanup complete");
        Environment.Exit(-1);
        return true;
    }
    static int Main(string[] args)
    {
        return Parser.Default.ParseArguments(args, GetVerbs())
            .MapResult(Run, errors => 1);
    }
    private static int Run(object parsedOptions)
    {
        switch (parsedOptions)
        {
            case ProcessCheckRulesOptions options:
            {
                EntryAssembly();
                return ProcessRule(options);
            }
            case ProcessDocGeneratorOptions options:
            {
                EntryAssembly();
                return ProcessDocGenerator(options);
            }
            case GeneratePassHashOptions options:
            {
                EntryAssembly();
                return HashPassword(options);
            }
#if !NETCORE2_1
            case ProcessQueueOptions options:
            {
                EntryAssembly();
                return ProcessQueue(options);
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
                return CreateHashIndex(options);
            }
            case CompareSchemaOptions options:
            {
                EntryAssembly();
                return CompareSchema(options);
            }
            case GetRootVersionOptions options:
            {
                return SqlRunner.Create(log).GetRootVersion(options);
            }
            case RunSqlCommandOptions options:
            {
                return SqlRunner.Create(log).RunSqlCommand(options);
            }
            case RunSqlProcedureCommandOptions options:
            {
                return SqlRunner.Create(log).RunSqlProcedure(options);
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
        Console.WriteLine(string.Format(Strings.ShortGnu,
                System.Reflection.Assembly.GetEntryAssembly().GetName().Name));
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
                    "OrigamSettings.config doesn't contain exactly one configuration.");
                return -1;
            }
        } catch
        {
            Console.WriteLine("Failed to load OrigamSettings.config");
            return -1;
        }
		Directory
            .EnumerateFiles(
                configurations[0].ModelSourceControlLocation, 
                "*.origam", 
                SearchOption.AllDirectories)
            .AsParallel()
            .ForEach(path =>
            {
                Console.Write("Processing ");
                Console.Write(path);
                Console.Write("...");
                var source = new OrigamXmlDocument(path);
                var xmlToWrite = OrigamDocumentSorter
                    .CopyAndSort(source)
                    .ToBeautifulString();
                File.WriteAllText(path, xmlToWrite);
                Console.WriteLine("DONE");
            });
        return 0;
    }
    private static int HashPassword(GeneratePassHashOptions options)
    {
        var hash =
            new AdaptivePasswordHasher().HashPassword(options.Password);
        log.Info("");
        log.Info("Password: " + options.Password);
        log.Info("Hash: " + hash);
        return 0;
    }
    private static int ProcessDocGenerator(ProcessDocGeneratorOptions config)
    {
        Thread.CurrentThread.CurrentUICulture 
            = new CultureInfo(config.Language);
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
        persistenceService.LoadSchema(settings.DefaultSchemaExtensionId);
        var documentation = new FileStorageDocumentationService(
            persistenceProvider,
            persistenceService.FileEventQueue);
        new DocProcessor(config.Output, config.Xslt, config.RootFile,
            documentation,
            menuProvider, persistenceService, null).Run();
        return 0;
    }
    private static int ProcessRule(ProcessCheckRulesOptions invokedVerbInstance)
    {
        var rulesProcessor = new RulesProcessor();
        return rulesProcessor.Run();
    }
    private static int ProcessQueue(ProcessQueueOptions options)
    {
        log.Info("------------ Input -------------");
        log.Info($"queueCode: {options.QueueRefCode}");
        log.Info($"parallelism: {options.Parallelism}");
        log.Info($"forceWait_ms: {options.ForceWaitMs}");
        log.Info($"-------------------------------");
        cancelHandler += CancelHandler;
        SetConsoleCtrlHandler(cancelHandler, true);
        RunQueueProcessor(options);
        log.Info("Exiting...");
        return 0;
    }
    private static void RunQueueProcessor(ProcessQueueOptions options)
    {
        try
        {
            log.Info(options);
            queueProcessor = new QueueProcessor(
                options.QueueRefCode,
                options.Parallelism,
                options.ForceWaitMs
            );
            queueProcessor.Run();
        }
        catch (Exception ex)
        {
            log.LogOrigamError(ex);
        }
    }
    private static int RunUpdateScripts()
    {
        if (log.IsInfoEnabled)
        {
            log.Info("Running update scripts...");
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
            log.Info("Invoking server restart...");
        }
        OrigamEngine.OrigamEngine.SetRestart();
    }
    private static int CreateHashIndex(CreateHashIndexOptions options)
    {
        if (log.IsInfoEnabled)
        {
            log.InfoFormat(
                "Creating hash index file {1} on folder {0} with pattern {2}.",
                options?.Input, options?.Output, options?.Mask);
        }
        var fileNames 
            = Directory.GetFiles(options.Input, options.Mask);
        var hashIndexFile = new HashIndexFile(options.Output);
        foreach (var filename in fileNames)
        {
            if (log.IsInfoEnabled)
            {
                log.InfoFormat("Hashing {0}...", filename);
            }
            hashIndexFile.AddEntryToIndexFile(
                hashIndexFile.CreateIndexFileEntry(filename));
        }
        if (log.IsInfoEnabled)
        {
            log.Info("Hash index file finished.");
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
                log.Info("No comparison switches enabled...");
            }
            return 0;
        }
        OrigamEngine.OrigamEngine.ConnectRuntime(runRestartTimer: false);
        if (log.IsInfoEnabled)
        {
            log.Info(
                $@"Comparing schema with database: missing in database({
                    options.MissingInDb}), missing in schema({
                        options.MissingInSchema}), existing but different({
                            options.ExistingButDifferent})...");
        }
        var persistenceService
            = ServiceManager.Services.GetService<IPersistenceService>();
        var settings = ConfigurationManager.GetActiveConfiguration();
        var dataService = new MsSqlDataService(
            settings.DataConnectionString,
            settings.DataBulkInsertThreshold,
            settings.DataUpdateBatchSize);
        dataService.PersistenceProvider = persistenceService.SchemaProvider;
        List<SchemaDbCompareResult> results = dataService.CompareSchema(
            persistenceService.SchemaProvider);
        if (results.Count == 0)
        {
            if (log.IsInfoEnabled)
            {
                log.Info("No differences found.");
            }
            return 0;
        }
        return DisplaySchemaComparisonResults(options, results);
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
                    existingButDifferent.Add(result);
                    break;
                }
                case DbCompareResultType.MissingInDatabase:
                {
                    missingInDatabase.Add(result);
                    break;
                }
                case DbCompareResultType.MissingInSchema:
                {
                    missingInSchema.Add(result);
                    break;
                }
            }
        }
        var displayedResultsCount = 0;
        if (options.MissingInDb)
        {
            DisplayComparisonResultGroup(
                missingInDatabase, "Missing in Database:");
            displayedResultsCount += missingInDatabase.Count;
        }
        if (options.MissingInSchema)
        {
            DisplayComparisonResultGroup(
                missingInSchema, "Missing in Schema:");
            displayedResultsCount += missingInSchema.Count;
        }
        if (options.ExistingButDifferent)
        {
            DisplayComparisonResultGroup(
                existingButDifferent, "Existing But Different:");
            displayedResultsCount += existingButDifferent.Count;
        }
        if (displayedResultsCount == 0)
        {
            if (log.IsInfoEnabled)
            {
                log.Info("No differences found.");
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
            log.Info(header);
            foreach (SchemaDbCompareResult result in results)
            {
                log.Info(
                    $@"{result?.SchemaItemType.SchemaItemDescription()?.Name} {
                        result?.ItemName} {result?.Remark}");
            }
        }
    }
}