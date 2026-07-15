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

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Origam.Architect.Server.Models;
using Origam.Architect.Server.ReturnModels;
using Origam.Architect.Server.Services;
using Origam.Schema;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class DocumentationController(
    TreeNodeFactory treeNodeFactory,
    TabService tabService,
    IDocumentationService documentationService,
    DocumentationHelperService documentationHelper
) : ControllerBase
{
    [HttpPost("OpenEditor")]
    public IActionResult OpenEditor([Required] [FromBody] OpenTabModel input)
    {
        TabData tab = tabService.OpenDocumentationTab(input.SchemaItemId);
        ISchemaItem item = tab.Item;
        TreeNode treeNode = treeNodeFactory.Create(item);

        tab.DocumentationData = documentationService.LoadDocumentation(item.Id);
        var openTabData = new OpenTabData(
            tabId: tab.Id,
            node: treeNode,
            data: documentationHelper.GetData(tab.DocumentationData, item.Name),
            isPersisted: true
        );
        return Ok(openTabData);
    }

    [HttpPost("Update")]
    public IActionResult Update([FromBody] ChangesModel changes)
    {
        TabData tab = tabService.OpenDocumentationTab(changes.SchemaItemId);
        documentationHelper.Update(changes, tab);

        return Ok(new UpdatePropertiesResult { IsDirty = tab.IsDirty });
    }

    [HttpPost("PersistChanges")]
    public IActionResult PersistChanges([FromBody] PersistModel input)
    {
        TabData tab = tabService.OpenDocumentationTab(input.SchemaItemId);
        documentationService.SaveDocumentation(tab.DocumentationData, input.SchemaItemId);
        tab.IsDirty = false;
        return Ok();
    }
}
