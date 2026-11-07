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

namespace Origam.Architect.Server.Services.Move;

public class SchemaItemMover(ModelTransactionRunner transactionRunner)
{
    public ISchemaItem Move(ISchemaItem item, MoveDecision decision)
    {
        ISchemaItemProvider rootProvider = item.RootProvider;
        ISchemaItem oldParent = item.ParentItem;
        SchemaItemGroup oldGroup = GetGroupOrThrow(item);
        bool oldIsAbstract = item.IsAbstract;
        try
        {
            transactionRunner.Run(() =>
            {
                decision.ApplyTo(item);
                item.Persist();
            });
        }
        catch
        {
            // The transaction rolls back the files only, not the in memory item.
            item.ParentItem = oldParent;
            item.Group = oldGroup;
            item.IsAbstract = oldIsAbstract;
            decision.ClearAffectedCaches(rootProvider, oldParent);
            throw;
        }

        if (oldParent == null && decision.Kind == MoveDestination.ParentItem)
        {
            rootProvider?.ClearCache();
        }

        return item;
    }

    // The Group getter throws a plain Exception when the group is gone from the model.
    private static SchemaItemGroup GetGroupOrThrow(ISchemaItem item)
    {
        try
        {
            return item.Group;
        }
        catch (Exception exception)
        {
            throw new UserOrigamException(
                string.Format(Strings.Move_SourceGroupNotFound, item.Name),
                exception.Message,
                exception
            );
        }
    }
}
