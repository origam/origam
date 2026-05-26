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
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Architect.Server.Controllers;

[ApiController]
[Route("[controller]")]
public class TabController(
    PropertyEditorService propertyService,
    IPersistenceService persistenceService,
    DesignerEditorService sectionService,
    TreeNodeFactory treeNodeFactory,
    TabService tabService,
    DocumentationHelperService documentationHelper
) : ControllerBase
{
    [HttpPost("CreateNode")]
    public OpenTabData CreateNode([Required] [FromBody] NewItemModel input)
    {
        var tab = tabService.OpenTabWithNewItem(input.NodeId, input.NewTypeName);

        TreeNode treeNode = treeNodeFactory.Create(tab.Item);
        return new OpenTabData(
            tabId: tab.Id,
            node: treeNode,
            data: GetData(treeNode, tab.Item),
            isPersisted: false,
            parentNodeId: null,
            isDirty: true
        );
    }

    [HttpGet("GetOpen")]
    public IActionResult GetOpen()
    {
        var items = tabService
            .GetOpenTabs()
            .Select(tab =>
            {
                var item = tab.Item;
                TreeNode treeNode = treeNodeFactory.Create(item);

                return tab.Id.Type switch
                {
                    TabType.Default => new OpenTabData(
                        tabId: tab.Id,
                        node: treeNode,
                        data: GetData(treeNode, item),
                        isPersisted: item.IsPersisted,
                        parentNodeId: TreeNode.ToTreeNodeId(item.ParentItem),
                        isDirty: tab.IsDirty
                    ),
                    TabType.DocumentationEditor => new OpenTabData(
                        tabId: tab.Id,
                        node: treeNode,
                        data: documentationHelper.GetData(tab.DocumentationData, item.Name),
                        isPersisted: item.IsPersisted,
                        isDirty: tab.IsDirty
                    ),
                    _ => throw new Exception("Unknown tab type: " + tab.Id.Type),
                };
            })
            .ToList();
        return Ok(items);
    }

    [HttpPost("Open")]
    public IActionResult Open([Required] [FromBody] OpenTabModel input)
    {
        TabData tab = tabService.OpenDefaultTab(input.SchemaItemId);
        ISchemaItem item = tab.Item;
        TreeNode treeNode = treeNodeFactory.Create(item);

        var openTabData = new OpenTabData(
            tabId: tab.Id,
            node: treeNode,
            data: GetData(treeNode, item),
            isPersisted: true
        );
        return Ok(openTabData);
    }

    private object GetData(TreeNode treeNode, ISchemaItem item)
    {
        object data = treeNode.DefaultEditor switch
        {
            EditorSubType.GridEditor => propertyService.GetEditorPropertiesWithErrors(item),
            EditorSubType.DeploymentScriptsEditor => propertyService.GetEditorPropertiesWithErrors(
                item
            ),
            EditorSubType.XsltEditor => propertyService.GetEditorPropertiesWithErrors(item),
            EditorSubType.ScreenSectionEditor => sectionService.GetSectionEditorData(item),
            EditorSubType.ScreenEditor => sectionService.GetScreenEditorData(item),
            _ => null,
        };
        return data;
    }

    [HttpPost("Close")]
    public IActionResult Close([Required] [FromBody] CloseTabModel input)
    {
        tabService.CloseTab(input.GetTypedTabId());
        return Ok();
    }

    [HttpPost("PersistChanges")]
    public IActionResult PersistChanges([FromBody] PersistModel input)
    {
        TabData tabData = tabService.OpenDefaultTab(input.SchemaItemId);
        ISchemaItem item = tabData.Item;
        if (item is AbstractControlSet controlSet && controlSet.DataSourceId == Guid.Empty)
        {
            return BadRequest("No Datasource selected can't save");
        }

        try
        {
            persistenceService.SchemaProvider.BeginTransaction();
            item.Persist();
            tabData.IsDirty = false;
            return Ok();
        }
        finally
        {
            persistenceService.SchemaProvider.EndTransaction();
        }
    }
}
