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

using Origam.Architect.Server.Exceptions;
using Origam.Schema;

namespace Origam.Architect.Server.Services;

public static class SchemaItemTreePath
{
    public static ISchemaItem GetRoot(ISchemaItem item)
    {
        try
        {
            ISchemaItem root = item;
            for (ISchemaItem parent = item.ParentItem; parent != null; parent = parent.ParentItem)
            {
                root = parent;
            }
            return root;
        }
        catch (Exception ex)
        {
            throw new OrphanedSchemaReferenceException(item.Id, ex);
        }
    }

    public static List<string> GetParentNodeIds(ISchemaItem item, ISchemaItem root)
    {
        try
        {
            if (root.RootProvider is not AbstractSchemaItemProvider provider)
            {
                return [];
            }

            var ids = new List<string>();
            AddFolderNameIfAny(ids, item);

            for (ISchemaItem parent = item.ParentItem; parent != null; parent = parent.ParentItem)
            {
                ids.Add(parent.Id.ToString());
                AddFolderNameIfAny(ids, parent);
            }

            for (SchemaItemGroup group = root.Group; group != null; group = group.ParentGroup)
            {
                ids.Add(group.Id.ToString());
            }

            ids.Add(provider.NodeId);
            ids.Add(provider.Group);
            ids.Reverse();

            return ids;
        }
        catch (Exception ex)
        {
            throw new OrphanedSchemaReferenceException(item.Id, ex);
        }

        static void AddFolderNameIfAny(List<string> target, ISchemaItem schemaItem)
        {
            var folderName = schemaItem?.GetType().SchemaItemDescription()?.FolderName;
            if (!string.IsNullOrWhiteSpace(folderName))
            {
                target.Add(folderName);
            }
        }
    }
}
