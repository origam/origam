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
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Workbench.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Origam.Rule;
using Origam.Schema.WorkflowModel;

namespace Origam.Utils
{
    class RulesProcessor
    {
        private readonly string pathProject;
        private SchemaService service = new SchemaService();

        public RulesProcessor(string pathProject)
        {
            this.pathProject = pathProject;
        }

        internal int Run()
        {
            List<Dictionary<IFilePersistent, string>> errorFragments
                    = ModelRules.GetErrors(
                        service, 
                        GetPersistence(),
                        new CancellationTokenSource().Token);
            if (errorFragments.Count != 0)
            {
                StringBuilder sb = new StringBuilder("Rule violations in ");
                sb.Append(pathProject.ToUpper());
                sb.Append("\n");
                foreach (Dictionary<IFilePersistent, string> dict in errorFragments)
                {
                    IFilePersistent retrievedObj = dict.First().Key;
                    sb.Append("Object with Id: \"" + retrievedObj.Id +
                               "\" in file: \"" + retrievedObj.RelativeFilePath +
                                "\" --> " + string.Join("\n", dict.First().Value) + "\n");
                }
                System.Console.Write(sb.ToString());
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private FilePersistenceService GetPersistence()
        {
            var DefaultFolders = new List<ElementName>
            {
                ElementNameFactory.Create(typeof(SchemaExtension)),
                ElementNameFactory.Create(typeof(SchemaItemGroup))
            };
            ServiceManager sManager = ServiceManager.Services;
            IParameterService parameterService = new NullParameterService();
            sManager.AddService(service);
            sManager.AddService(parameterService);
            StateMachineSchemaItemProvider StateMachineSchema = new StateMachineSchemaItemProvider();
            var settings = new OrigamSettings();
            ConfigurationManager.SetActiveConfiguration(settings);
            SecurityManager.SetServerIdentity();
            var persistenceService = new FilePersistenceService(DefaultFolders,
               pathProject, false, false);

            sManager.AddService(persistenceService);
            service.AddProvider(StateMachineSchema);
            OrigamEngine.OrigamEngine.InitializeSchemaItemProviders(service);
            return persistenceService;
        }
    }
}
