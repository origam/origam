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
using Origam.Architect.Server.ReturnModels;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class PackageController : ControllerBase
{
    private readonly SchemaService schemaService;

    public PackageController(SchemaService schemaService)
    {
        this.schemaService = schemaService;
    }

    [HttpGet("GetAll")]
    public PackagesInfo GetAll()
    {
        var packages = schemaService.AllPackages
            .OrderBy(x => x.Name)
            .Select(x => new PackageModel(x.Id, x.NodeText));

        return new PackagesInfo
        {
            Packages = packages,
            ActivePackageId = schemaService.ActiveSchemaExtensionId == Guid.Empty
                ? null 
                : schemaService.ActiveSchemaExtensionId
        };
    }
    
    [HttpPost("SetActive")]
    public ActionResult SetActive([FromBody]PackageIdentifier package)
    {
        if (schemaService.ActiveSchemaExtensionId == package.Id)
        {
            return Ok();
        }
        SecurityManager.SetServerIdentity();
        schemaService.LoadSchema(package.Id);
        return Ok();
    }
}

public record PackageIdentifier(Guid Id);
