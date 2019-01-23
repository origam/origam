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
using Origam.OrigamEngine;
using Origam.Schema;
using Origam.Workbench.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Origam.Utils
{
    class RulesProcessor
    {
        private CancellationTokenSource ruleCheckCancellationTokenSource = new CancellationTokenSource();
        private readonly string pathProject;

        public RulesProcessor(string pathProject)
        {
            this.pathProject = pathProject;
        }

        internal int Run()
        {
            OrigamEngine.OrigamEngine.ConnectRuntime();
            var DefaultFolders = new List<ElementName>
            {
                ElementNameFactory.Create(typeof(SchemaExtension)),
                ElementNameFactory.Create(typeof(SchemaItemGroup))
            };
            var persistenceService = new FilePersistenceService(DefaultFolders,
               pathProject,false,false);

            List<Dictionary<IFilePersistent, string>> errorFragments
                    = OrigamArchitect.Commands.ModelRules.GetErrors(persistenceService, ruleCheckCancellationTokenSource.Token);
            if (errorFragments.Count != 0)
            {
                StringBuilder sb = new StringBuilder("Rule violations were found  in");
                sb.Append(pathProject);
                sb.Append("\n");
                foreach (Dictionary<IFilePersistent, string> dict in errorFragments)
                {
                    var retrievedObj = dict.First().Key;
                    sb.Append("Object with Id: \"" + retrievedObj.Id +
                               "\" in file: \"" + retrievedObj.RelativeFilePath +
                               "\"\n" + string.Join("\n", dict.First().Value));
                }
                System.Console.Write(sb.ToString());
                return 1;
            }
            else
            {
                return 0;
            }
        }
    }
}
