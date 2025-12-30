#region license

/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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

using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models.Requests;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class SearchController(IPersistenceService persistenceService) : ControllerBase
{
    [HttpGet("Text")]
    public ActionResult Text([FromQuery] string text)
    {
        var results = persistenceService.SchemaProvider.FullTextSearch<ISchemaItem>(text);
        return Ok(
            results.Select(result => new SearchResult
            {
                Name = result.Name,
                SchemaId = result.Id,
                ParentNodeIds = GetParentNodeIds(result),
            })
        );
    }

    [HttpGet("References")]
    public ActionResult References([FromQuery] Guid schemaItemId)
    {
        var item = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(schemaItemId);
        
        return Ok(
            item.GetUsage().Select(result => new SearchResult
            {
                Name = result.Name,
                SchemaId = result.Id,
                ParentNodeIds = GetParentNodeIds(result),
            })
        );
    }
    
    [HttpGet("Dependencies")]
    public ActionResult Dependencies([FromQuery] Guid schemaItemId)
    {
        var item = persistenceService.SchemaProvider.RetrieveInstance<ISchemaItem>(schemaItemId);
        
        return Ok(
            item.GetDependencies(false).Select(result => new SearchResult
            {
                Name = result.Name,
                SchemaId = result.Id,
                ParentNodeIds = GetParentNodeIds(result),
            })
        );
    }

    private static List<string> GetParentNodeIds(ISchemaItem item)
    {
        if (item?.RootItem?.RootProvider is not AbstractSchemaItemProvider provider)
        {
            return [];
        }

        var ids = new List<string>();

        AddFolderNameIfAny(ids, item);

        for (var parent = item.ParentItem; parent != null; parent = parent.ParentItem)
        {
            AddFolderNameIfAny(ids, parent);
            ids.Add(parent.Id.ToString());
        }

        for (var group = item.RootItem.Group; group != null; group = group.ParentGroup)
        {
            ids.Add(group.Id.ToString());
        }

        ids.Add(provider.NodeId);
        ids.Add(provider.Group);

        ids.Reverse();
        return ids;

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
