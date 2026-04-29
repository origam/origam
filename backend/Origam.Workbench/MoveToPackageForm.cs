#region license
/*
Copyright 2005 - 2024 Advantage Solutions, s. r. o.

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Workbench;

public partial class MoveToPackageForm : Form
{
    private readonly WorkbenchSchemaService schema =
        ServiceManager.Services.GetService<WorkbenchSchemaService>();

    private readonly IPersistenceProvider persistenceProvider = ServiceManager
        .Services.GetService<IPersistenceService>()
        .SchemaProvider;

    public MoveToPackageForm()
    {
        InitializeComponent();
        schema
            .ActiveExtension.IncludedPackages.Concat(second: new[] { schema.ActiveExtension })
            .OrderBy(keySelector: package => package.Name)
            .ForEach(action: package => packageComboBox.Items.Add(item: package));
        packageComboBox.DisplayMember = "Name";
        groupComboBox.DisplayMember = "Name";
    }

    private ISchemaItemProvider GetActiveItemRootProvider()
    {
        return schema.ActiveNode switch
        {
            ISchemaItem schemaItem => schemaItem.RootProvider,
            SchemaItemGroup group => group.RootProvider,
            _ => throw new Exception(message: "Cannot process " + schema.ActiveNode.GetType()),
        };
    }

    private void okButton_Click(object sender, EventArgs e)
    {
        var groupContainer = groupComboBox.SelectedItem as GroupContainer;
        var targetPackage = packageComboBox.SelectedItem as Package;
        if (groupContainer == null || targetPackage == null)
        {
            return;
        }
        var targetGroup = groupContainer.Group;
        try
        {
            persistenceProvider.BeginTransaction();
            if (schema.ActiveNode is SchemaItemGroup activeGroup)
            {
                MoveGroupRecursive(
                    targetPackage: targetPackage,
                    targetGroup: targetGroup,
                    group: activeGroup
                );
            }
            else if (schema.ActiveNode is ISchemaItem activeItem)
            {
                MoveSchemaItem(
                    targetPackage: targetPackage,
                    targetGroup: targetGroup,
                    item: activeItem
                );
            }
            else
            {
                return;
            }
        }
        catch (Exception ex)
        {
            persistenceProvider.EndTransactionDontSave();
            AsMessageBox.ShowError(owner: this, text: ex.Message, caption: "Error", exception: ex);
            return;
        }

        persistenceProvider.EndTransaction();

        ISchemaItemProvider rootProvider = GetActiveItemRootProvider();
        schema.SchemaBrowser.EbrSchemaBrowser.RefreshItem(node: rootProvider);
        Close();
    }

    private void MoveSchemaItem(
        Package targetPackage,
        SchemaItemGroup targetGroup,
        ISchemaItem item
    )
    {
        item.Group = targetGroup;
        MoveSchemaItem(item: item, targetPackage: targetPackage);
    }

    private void MoveSchemaItem(ISchemaItem item, Package targetPackage)
    {
        CheckCanBeMovedOrThrow(activeItem: item, targetPackage: targetPackage);
        item.SetExtensionRecursive(extension: targetPackage);
        if (item is PanelControlSet panelControlSet)
        {
            panelControlSet.PanelControl.SetExtensionRecursive(extension: targetPackage);
            panelControlSet.PanelControl.Persist();
        }
        item.Persist();
    }

    private void MoveGroupRecursive(
        Package targetPackage,
        SchemaItemGroup targetGroup,
        SchemaItemGroup group
    )
    {
        group.ParentGroup = targetGroup;
        foreach (var childGroup in group.ChildGroupsRecursive)
        {
            MoveGroupAndTopLevelItems(group: childGroup, targetPackage: targetPackage);
        }
        MoveGroupAndTopLevelItems(group: group, targetPackage: targetPackage);
    }

    private void MoveGroupAndTopLevelItems(SchemaItemGroup group, Package targetPackage)
    {
        foreach (var child in group.ChildItems)
        {
            MoveSchemaItem(item: child, targetPackage: targetPackage);
        }
        group.Package = targetPackage;
        group.Persist();
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void packageComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        Package package = packageComboBox.SelectedItem as Package;
        if (package == null)
        {
            return;
        }
        groupComboBox.Items.Clear();
        GetActiveItemRootProvider()
            .ChildGroups.OrderBy(keySelector: group => group.Name)
            .Where(predicate: group => group.Package.Id == package.Id)
            .ForEach(action: group =>
            {
                var containers = GroupContainer.Create(group: group).ToArray<object>();
                groupComboBox.Items.AddRange(items: containers);
            });
        if (groupComboBox.Items.Count > 0)
        {
            groupComboBox.SelectedIndex = 0;
        }
    }

    private static void CheckCanBeMovedOrThrow(ISchemaItem activeItem, Package targetPackage)
    {
        List<ISchemaItem> dependenciesInPackagesNotReferencedByTargetPackage = activeItem
            .GetDependencies(ignoreErrors: true)
            .Cast<object>()
            .OfType<ISchemaItem>()
            .Where(predicate: item =>
                !targetPackage.IncludedPackages.Contains(item: item.Package)
                && item.Package != targetPackage
            )
            .ToList();
        if (dependenciesInPackagesNotReferencedByTargetPackage.Count != 0)
        {
            throw new Exception(
                message: string.Format(
                    format: Strings.ErrorDependenciesInPackagesNotReferencedByTargetPackage,
                    arg0: targetPackage.Name,
                    arg1: FormatToIdList(
                        schemaItems: dependenciesInPackagesNotReferencedByTargetPackage
                    )
                )
            );
        }

        List<ISchemaItem> usagesInPackagesWhichDontDependOnTargetPackage = activeItem
            .GetUsage()
            .Where(predicate: item =>
                !item.Package.IncludedPackages.Contains(item: targetPackage)
                && item.Package != targetPackage
            )
            .ToList();

        if (usagesInPackagesWhichDontDependOnTargetPackage.Count != 0)
        {
            throw new Exception(
                message: String.Format(
                    format: Strings.ErrorUsagesInPackagesWhichDontDependOnTargetPackage,
                    arg0: targetPackage.Name,
                    arg1: FormatToIdList(
                        schemaItems: usagesInPackagesWhichDontDependOnTargetPackage
                    )
                )
            );
        }
    }

    private static string FormatToIdList(List<ISchemaItem> schemaItems)
    {
        return "["
            + string.Join(separator: "\n", values: schemaItems.Select(selector: x => x.Id))
            + "]";
    }
}

internal class GroupContainer
{
    internal static List<GroupContainer> Create(SchemaItemGroup group)
    {
        var containers = new List<GroupContainer>();
        CreateGroupContainers(group: group, containers: containers, level: 0);
        return containers;
    }

    private static void CreateGroupContainers(
        SchemaItemGroup group,
        List<GroupContainer> containers,
        int level
    )
    {
        containers.Add(item: new GroupContainer(group: group, level: level));
        foreach (SchemaItemGroup childGroup in group.ChildGroups)
        {
            CreateGroupContainers(group: childGroup, containers: containers, level: level + 1);
        }
    }

    public string Name { get; }
    public SchemaItemGroup Group { get; }

    private GroupContainer(SchemaItemGroup group, int level)
    {
        Name = new string(c: ' ', count: level * 2) + group.Name;
        Group = group;
    }
};
