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

using Origam.Architect.Server.ReturnModels;
using Origam.Schema;
using Origam.Schema.EntityModel;

namespace Origam.Architect.Server.Services;

public class TabResponseFactory(
    TreeNodeFactory treeNodeFactory,
    PropertyEditorService propertyEditorService,
    DesignerEditorService designerEditorService
)
{
    public OpenTabData CreatedTabData(TabData tab)
    {
        ISchemaItem item = tab.Item;
        TreeNode treeNode = treeNodeFactory.Create(item);
        (string parentName, string parentOrigamId) = DescribeParent(item);
        return new OpenTabData(
            tabId: tab.Id,
            node: treeNode,
            data: GetEditorData(treeNode, item),
            isPersisted: item.IsPersisted,
            parentNodeId: null,
            isDirty: !item.IsPersisted,
            parentName: parentName,
            parentOrigamId: parentOrigamId,
            primaryKeyFieldId: DescribePrimaryKeyField(item)
        );
    }

    public OpenTabData DiscardedTabData(TabData tab)
    {
        TreeNode treeNode = treeNodeFactory.Create(tab.Item);
        object data = GetEditorData(treeNode, tab.Item);
        if (data is IEnumerable<EditorProperty> properties)
        {
            data = properties.ToList();
        }

        return new OpenTabData(
            tabId: tab.Id,
            node: treeNode,
            data: data,
            isPersisted: false,
            parentNodeId: null,
            isDirty: false,
            discarded: true
        );
    }

    public object GetEditorData(TreeNode treeNode, ISchemaItem item)
    {
        return treeNode.DefaultEditor switch
        {
            EditorSubType.GridEditor => propertyEditorService.GetEditorPropertiesWithErrors(item),
            EditorSubType.DeploymentScriptsEditor =>
                propertyEditorService.GetEditorPropertiesWithErrors(item),
            EditorSubType.XsltEditor => propertyEditorService.GetEditorPropertiesWithErrors(item),
            EditorSubType.ScreenSectionEditor => designerEditorService.GetSectionEditorData(item),
            EditorSubType.ScreenEditor => designerEditorService.GetScreenEditorData(item),
            _ => null,
        };
    }

    private static string DescribePrimaryKeyField(ISchemaItem item)
    {
        if (item is not IDataEntity entity)
        {
            return null;
        }
        return entity
            .EntityColumns.FirstOrDefault(column =>
                column.IsPrimaryKey && !column.ExcludeFromAllFields && column.Name != null
            )
            ?.Id.ToString("D");
    }

    private static (string Name, string OrigamId) DescribeParent(ISchemaItem item)
    {
        if (item.Group != null)
        {
            return (item.Group.NodeText, item.Group.Id.ToString());
        }
        if (item.ParentItem != null)
        {
            return (item.ParentItem.NodeText, item.ParentItem.Id.ToString());
        }
        return item.RootProvider == null
            ? (null, null)
            : (item.RootProvider.NodeText, item.RootProvider.GetType().FullName);
    }
}
