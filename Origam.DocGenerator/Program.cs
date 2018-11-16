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
using System.Threading;

namespace Origam.DocGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            Options options = new Options();
            ConfigOption config = new ConfigOption();
            if (!Parser.Default.ParseArgumentsStrict(args, options, (verb, subOptions) =>
            {
                verb = (verb ?? "").ToLowerInvariant();
                if (verb == Options.XmlCommandName)
                {
                    if (subOptions is XmlSubOptions XmlSubOptions)
                    {
                        config.SetOption(XmlSubOptions);
                    }
                }
                else if (verb == Options.XsltCommandName)
                {
                    if (subOptions is XsltSubOptions XsltSubOptions)
                    {
                        config.SetOption(XsltSubOptions);
                    }
                }
            }))
            {
                return;
            }
            Console.WriteLine(string.Format(Strings.ShortGNU, System.Reflection.Assembly.GetEntryAssembly().GetName().Name));
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
                LocalizationFolder = Path.Combine(config.Schema , "l10n")
            };
            ConfigurationManager.SetActiveConfiguration(settings);
            SecurityManager.SetServerIdentity();
            StateMachineSchemaItemProvider StateMachineSchema = new StateMachineSchemaItemProvider();
            var persistenceService = new FilePersistenceService(DefaultFolders,
               config.Schema);
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(config.Language);
            sManager.AddService(persistenceService);
            service.AddProvider(StateMachineSchema);
            FilePersistenceProvider persprovider = (FilePersistenceProvider)persistenceService.SchemaProvider;
            MenuSchemaItemProvider menuprovider = new MenuSchemaItemProvider
            {
                PersistenceProvider = persprovider
            };
            persistenceService.LoadSchema(new Guid(config.GuidPackage), false, false, "");
            var documentation = new FileStorageDocumentationService(
                persprovider,
                persistenceService.FileEventQueue);
            new DocCreate(config.Dataout, config.Xslt, config.RootFile, documentation,
                menuprovider, persistenceService, config.XmlFile).Run();
        }
    }
}
