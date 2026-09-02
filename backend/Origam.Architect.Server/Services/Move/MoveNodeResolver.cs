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

using Origam.Architect.Server.Models;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Services.Move;

public class MoveNodeResolver(SchemaService schemaService, IPersistenceService persistenceService)
{
    private IPersistenceProvider PersistenceProvider => persistenceService.SchemaProvider;

    public ISchemaItemProvider GetRootProviderById(string id)
    {
        if (schemaService.ActiveExtension == null)
        {
            return null;
        }

        return schemaService
            .ActiveExtension.ChildNodes()
            .Cast<SchemaItemProviderGroup>()
            .SelectMany(group => group.ChildNodes().Cast<ISchemaItemProvider>())
            .FirstOrDefault(provider => provider.NodeId == id);
    }

    public IBrowserNode2 Resolve(NodeRefModel reference)
    {
        if (reference == null || string.IsNullOrWhiteSpace(reference.Id))
        {
            return null;
        }

        if (Guid.TryParse(reference.Id, out Guid id))
        {
            var node = PersistenceProvider.RetrieveInstance<IBrowserNode2>(
                id,
                useCache: true,
                throwNotFoundException: false
            );
            if (node == null || !reference.IsNonPersistentItem)
            {
                return node;
            }

            return new NonpersistentSchemaItemNode
            {
                NodeText = reference.NodeText,
                ParentNode = node,
            };
        }

        return GetRootProviderById(reference.Id) as IBrowserNode2;
    }
}
