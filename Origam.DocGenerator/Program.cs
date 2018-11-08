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
using CommandLine;
using CommandLine.Text;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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

            [Option('p', "packageid", Required = true, HelpText = "guidid of package.")]
            public string GuidPackage { get; set; }

            [Option('x', "xslt", Required = true, HelpText = "Xslt template")]
            public string Xslt { get; set; }

            [Option('o', "output", Required = true, HelpText = "Output directory")]
            public string Dataout { get; set; }

            [Option('r', "rootfilename", Required = true, HelpText = "Root File")]
            public string RootFile { get; set; }

            [Option('m', "xmlfilename", Required = false, HelpText = "Xml File for export source tree.")]
            public string XmlFile { get; set; }

            [Option('l', "language", Required = true, HelpText = "Localization(ie. cs-CZ).")]
            public string Language { get; set; }

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
            Console.WriteLine(string.Format(Strings.ShortGNU, System.Reflection.Assembly.GetEntryAssembly().GetName().Name));

            Options options = new Options();

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

            var settings = new OrigamSettings
            {
                LocalizationFolder = Path.Combine(options.Schema, "l10n")
            };

            ConfigurationManager.SetActiveConfiguration(settings);
            Thread.CurrentPrincipal = new GenericPrincipal(new GenericIdentity("origam_server"), null);
            StateMachineSchemaItemProvider StateMachineSchema = new StateMachineSchemaItemProvider();
            var persistenceService = new FilePersistenceService(DefaultFolders,
               options.Schema);

             Thread.CurrentThread.CurrentUICulture = new CultureInfo(options.Language);

            sManager.AddService(persistenceService);
            service.AddProvider(StateMachineSchema);

            FilePersistenceProvider persprovider = (FilePersistenceProvider)persistenceService.SchemaProvider;
            MenuSchemaItemProvider menuprovider = new MenuSchemaItemProvider
            {
                PersistenceProvider = persprovider
            };

            persistenceService.LoadSchema(new Guid(options.GuidPackage), false, false, "");
            var documentation = new FileStorageDocumentationService(
                persprovider,
                persistenceService.FileEventQueue);

            

            new DocCreate(options.Dataout, options.Xslt, options.RootFile, documentation, menuprovider, persistenceService, options.XmlFile).Run();
        }
    }
}
