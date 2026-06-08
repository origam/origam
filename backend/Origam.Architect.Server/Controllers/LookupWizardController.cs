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

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models.Requests;
using Origam.Architect.Server.Services;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.LookupModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class LookupWizardController(
    IPersistenceService persistenceService,
    GitNodeStatusService gitNodeStatusService,
    SearchService searchService
) : ControllerBase
{
    private readonly IPersistenceProvider persistenceProvider = persistenceService.SchemaProvider;

    [HttpGet("GetEntityData")]
    public IActionResult GetEntityData([FromQuery] [Required] Guid entityId)
    {
        var entity = persistenceProvider.RetrieveInstance<IDataEntity>(entityId);
        if (entity == null)
        {
            return NotFound($"Entity {entityId} not found");
        }

        var primaryKey = entity.EntityColumns.FirstOrDefault(c =>
            c.IsPrimaryKey && !c.ExcludeFromAllFields
        );
        if (primaryKey == null)
        {
            return BadRequest("Entity has no primary key defined. Cannot create lookup.");
        }

        var columns = entity
            .EntityColumns.Where(c => !string.IsNullOrEmpty(c.ToString()))
            .OrderBy(c => c.Name)
            .Select(c => new IdName { Id = c.Id, Name = c.Name })
            .ToList();

        var filters = entity
            .EntityFilters.OrderBy(f => f.Name)
            .Select(f => new IdName { Id = f.Id, Name = f.Name })
            .ToList();

        var defaultDisplay = entity.EntityColumns.FirstOrDefault(c => c.Name == "Name");

        return Ok(
            new EntityWizardData
            {
                EntityName = entity.Name,
                PrimaryKeyId = primaryKey.Id,
                PrimaryKeyName = primaryKey.Name,
                DefaultDisplayFieldId = defaultDisplay?.Id ?? primaryKey.Id,
                Columns = columns,
                Filters = filters,
            }
        );
    }

    [HttpPost("CreateLookup")]
    public IActionResult CreateLookup([Required] [FromBody] CreateLookupModel input)
    {
        if (string.IsNullOrWhiteSpace(input.Name))
        {
            return BadRequest("Lookup name is required.");
        }

        var entity = persistenceProvider.RetrieveInstance<IDataEntity>(input.EntityId);
        if (entity == null)
        {
            return NotFound($"Entity {input.EntityId} not found");
        }

        var idColumn = entity.EntityColumns.FirstOrDefault(c =>
            c.IsPrimaryKey && !c.ExcludeFromAllFields
        );
        if (idColumn == null)
        {
            return BadRequest("Entity has no primary key defined.");
        }

        var displayColumn = entity.EntityColumns.FirstOrDefault(c => c.Id == input.DisplayFieldId);
        if (displayColumn == null)
        {
            return BadRequest("Display field not found on entity.");
        }

        var idFilter = entity.EntityFilters.FirstOrDefault(f => f.Id == input.IdFilterId);
        if (idFilter == null)
        {
            return BadRequest("Id filter not found on entity.");
        }

        EntityFilter listFilter = null;
        if (input.ListFilterId.HasValue && input.ListFilterId.Value != Guid.Empty)
        {
            listFilter = entity.EntityFilters.FirstOrDefault(f => f.Id == input.ListFilterId.Value);
            if (listFilter == null)
            {
                return BadRequest("List filter not found on entity.");
            }
        }

        try
        {
            persistenceProvider.BeginTransaction();
            var lookup = LookupHelper.CreateDataServiceLookup(
                input.Name,
                entity,
                idColumn,
                displayColumn,
                codeField: null,
                idFilter,
                listFilter,
                listDisplayMember: null
            );
            persistenceProvider.EndTransaction();
            gitNodeStatusService.ClearCache();
            return Ok(
                new CreateLookupResult
                {
                    LookupId = lookup.Id,
                    LookupName = lookup.Name,
                    DataStructureId = lookup.ListDataStructure.Id,
                    DataStructureName = lookup.ListDataStructure.Name,
                    SearchResults = searchService.BuildResults(
                        new ISchemaItem[] { lookup, lookup.ListDataStructure }
                    ),
                }
            );
        }
        catch (Exception ex)
        {
            persistenceProvider.EndTransactionDontSave();
            return StatusCode(statusCode: 500, ex.Message);
        }
    }
}

public class IdName
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

public class EntityWizardData
{
    public string EntityName { get; set; }
    public Guid PrimaryKeyId { get; set; }
    public string PrimaryKeyName { get; set; }
    public Guid DefaultDisplayFieldId { get; set; }
    public List<IdName> Columns { get; set; } = new();
    public List<IdName> Filters { get; set; } = new();
}

public class CreateLookupModel
{
    [Required]
    public Guid EntityId { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public Guid DisplayFieldId { get; set; }

    [Required]
    public Guid IdFilterId { get; set; }

    public Guid? ListFilterId { get; set; }
}

public class CreateLookupResult
{
    public Guid LookupId { get; set; }
    public string LookupName { get; set; }
    public Guid DataStructureId { get; set; }
    public string DataStructureName { get; set; }
    public List<SearchResult> SearchResults { get; set; } = new();
}
