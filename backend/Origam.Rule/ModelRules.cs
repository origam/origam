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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Rule;

public class ModelRules
{
    public static List<Dictionary<ISchemaItem, string>> GetErrors(
        List<AbstractSchemaItemProvider> schemaProviders,
        FilePersistenceService independentPersistenceService,
        CancellationToken cancellationToken
    )
    {
        List<Dictionary<ISchemaItem, string>> errorFragments = independentPersistenceService
            .SchemaProvider.RetrieveList<IFilePersistent>()
            .OfType<ISchemaItem>()
            .AsParallel()
            .Select(selector: retrievedObj =>
            {
                retrievedObj.RootProvider = schemaProviders.FirstOrDefault(predicate: x =>
                    BelongsToProvider(provider: x, retrievedObj: retrievedObj)
                );
                cancellationToken.ThrowIfCancellationRequested();
                return retrievedObj;
            })
            .AsParallel()
            .Select(selector: retrievedObj =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var errorMessages = RuleTools
                    .GetExceptions(instance: retrievedObj)
                    .Select(selector: exception => exception.Message)
                    .ToList();
                if (errorMessages.Count == 0)
                {
                    return null;
                }

                return new Dictionary<ISchemaItem, string>
                {
                    { retrievedObj, string.Join(separator: "\n", values: errorMessages) },
                };
            })
            .Where(predicate: x => x != null)
            .ToList();
        return errorFragments;
    }

    private static bool BelongsToProvider(ISchemaItemProvider provider, ISchemaItem retrievedObj)
    {
        return String.Compare(
                strA: retrievedObj.ItemType,
                strB: ((AbstractSchemaItemProvider)provider).RootItemType,
                ignoreCase: true
            ) == 0;
    }
}
