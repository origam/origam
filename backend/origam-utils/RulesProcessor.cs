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
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Workbench.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Origam.Rule;
using Origam.OrigamEngine;

namespace Origam.Utils;
class RulesProcessor
{
    public RulesProcessor()
    {
    }
    internal int Run()
    {
        RuntimeServiceFactoryProcessor RuntimeServiceFactory = new RuntimeServiceFactoryProcessor();
        OrigamEngine.OrigamEngine.ConnectRuntime(customServiceFactory: RuntimeServiceFactory);
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        FilePersistenceService persistence = ServiceManager.Services.GetService(typeof(FilePersistenceService)) as FilePersistenceService;
        List<AbstractSchemaItemProvider> allproviders = new OrigamProviderBuilder()
            .SetSchemaProvider(persistence.SchemaProvider)
            .GetAll();
        List<Dictionary<ISchemaItem, string>> errorFragments
                = ModelRules.GetErrors(
                    allproviders,
                    persistence,
                    new CancellationTokenSource().Token);
        if (errorFragments.Count != 0)
        {
            StringBuilder sb = new StringBuilder("Rule violations in ");
            sb.Append(settings.ModelSourceControlLocation.ToUpper());
            sb.Append("\n");
            foreach (Dictionary<ISchemaItem, string> dict in errorFragments)
            {
                ISchemaItem retrievedObj = dict.First().Key;
                sb.Append("Object with Id: \"" + retrievedObj.Id +
                           "\" in file: \"" + retrievedObj.RelativeFilePath +
                            "\" --> " + string.Join("\n", dict.First().Value) + "\n");
            }
            System.Console.Write(sb.ToString());
            return 1;
        }

        return 0;
    }
}
