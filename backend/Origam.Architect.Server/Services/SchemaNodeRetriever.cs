#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services;

public class SchemaNodeRetriever(
    SchemaService schemaService,
    IPersistenceService persistenceService
)
{
    public T Retrieve<T>(Guid id)
    {
        return WithRootProvider(persistenceService.SchemaProvider.RetrieveInstance<T>(id));
    }

    public T Retrieve<T>(Guid id, bool useCache, bool throwNotFoundException)
    {
        return WithRootProvider(
            persistenceService.SchemaProvider.RetrieveInstance<T>(
                id,
                useCache,
                throwNotFoundException
            )
        );
    }

    private T WithRootProvider<T>(T node)
    {
        if (node is SchemaItemGroup { RootProvider: null, ParentItem: null } group)
        {
            group.RootProvider = FindProvider(group.RootItemType);
        }
        else if (node is ISchemaItem { RootItem: { RootProvider: null } rootItem })
        {
            rootItem.RootProvider = FindProvider(rootItem.ItemType);
        }
        return node;
    }

    private ISchemaItemProvider FindProvider(string rootItemType)
    {
        return schemaService
            .Providers.OfType<AbstractSchemaItemProvider>()
            .FirstOrDefault(provider => provider.RootItemType == rootItemType);
    }
}
