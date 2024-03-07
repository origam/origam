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

using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Schema;
using Origam.UI;
using Origam.Workbench.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Origam.Workbench;

public partial class MoveToPackageForm : Form
{
    WorkbenchSchemaService schema = ServiceManager.Services
        .GetService<WorkbenchSchemaService>();
    IPersistenceProvider persistenceProvider = ServiceManager.Services
        .GetService<IPersistenceService>().SchemaProvider;
    public MoveToPackageForm()
    {
        InitializeComponent();
        schema.ActiveExtension.IncludedPackages
            .Concat(new Package[] { schema.ActiveExtension })
            .OrderBy(package => package.Name)
            .ForEach(package => packageComboBox.Items.Add(package));
        packageComboBox.DisplayMember = "Name";
        groupComboBox.DisplayMember = "Name";
    }

    private ISchemaItemProvider GetActiveItemRootProvider()
    {
        if (schema.ActiveNode is AbstractSchemaItem schemaItem)
        {
            return schemaItem.RootProvider;
        }
        if (schema.ActiveNode is SchemaItemGroup group)
        {
            return group.RootProvider;
        }
        throw new Exception("Cannot process " + schema.ActiveNode.GetType());
    }

    private void okButton_Click(object sender, EventArgs e)
    {
        var targetPackage = packageComboBox.SelectedItem as Package;
        var targetGroup = (groupComboBox.SelectedItem as GroupContainer).Group;
        if (targetPackage == null)
        {
            return;
        }
        try
        {
            persistenceProvider.BeginTransaction();
            if (schema.ActiveNode is SchemaItemGroup activeGroup)
            {
                MoveGroupRecursive(targetPackage, targetGroup, activeGroup);
            }
            else if (schema.ActiveNode is AbstractSchemaItem activeItem)
            {
                MoveSchemaItem(targetPackage, targetGroup, activeItem);
            }
            else
            {
                return;
            }
        }
        catch (Exception ex)
        {
            persistenceProvider.EndTransactionDontSave();
            AsMessageBox.ShowError(this, ex.Message, "Error", ex);
            return;
        }

        persistenceProvider.EndTransaction();

        ISchemaItemProvider rootProvider = GetActiveItemRootProvider();
        schema.SchemaBrowser.EbrSchemaBrowser.RefreshItem(rootProvider);
        Close();
    }

    private void MoveSchemaItem(Package targetPackage, 
        SchemaItemGroup targetGroup, AbstractSchemaItem item)
    {
        item.Group = targetGroup;
        MoveSchemaItem(item, targetPackage);
    }

    private void MoveSchemaItem(AbstractSchemaItem item, Package targetPackage)
    {
        CheckCanBeMovedOrThrow(item, targetPackage);
        item.SetExtensionRecursive(targetPackage);
        item.Persist();
    }

    private void MoveGroupRecursive(Package targetPackage, 
        SchemaItemGroup targetGroup, SchemaItemGroup group)
    {
        group.ParentGroup = targetGroup;
        foreach (var child in group.ChildGroupsRecursive)
        {
            if (child is SchemaItemGroup childGroup)
            {
                MoveGroupAndTopLevelItems(childGroup, targetPackage);
            }
        }
        MoveGroupAndTopLevelItems(group, targetPackage);
    }

    private void MoveGroupAndTopLevelItems(SchemaItemGroup group,
        Package targetPackage)
    {
        foreach (var child in group.ChildItems)
        {
            if (child is AbstractSchemaItem schemaItem)
            {
                MoveSchemaItem(schemaItem, targetPackage);
            }
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
        groupComboBox.Items.Clear();
        Package package = packageComboBox.SelectedItem as Package;
        GetActiveItemRootProvider()
            .ChildGroups
            .OrderBy(group => group.Name)
            .Where(group => group.Package.Id == package.Id)
            .ForEach(group => {
                    GroupContainer[] containers = GroupContainer.Create(group);
                    groupComboBox.Items.AddRange(containers);
                });
        groupComboBox.SelectedIndex = 0;
    }

    private static void CheckCanBeMovedOrThrow(AbstractSchemaItem activeItem,
            Package targetPackage)
    {
        List<ISchemaItem> dependenciesInPackagesNotReferencedByTargetPackage
            = activeItem.GetDependencies(true)
                .Cast<object>()
                .OfType<ISchemaItem>()
                .Where(item =>
                    !targetPackage.IncludedPackages.Contains(item.Package)
                    && item.Package != targetPackage)
                .ToList();
        if (dependenciesInPackagesNotReferencedByTargetPackage.Count != 0)
        {
            throw new Exception(string.Format(
                Strings.ErrorDependenciesInPackagesNotReferencedByTargetPackage,
                targetPackage.Name,
                FormatToIdList(dependenciesInPackagesNotReferencedByTargetPackage)));
        }

        List<ISchemaItem> usagesInPackagesWhichDontDependOnTargetPackage
            = activeItem.GetUsage()
                .Cast<object>()
                .OfType<ISchemaItem>()
                .Where(item =>
                    !item.Package.IncludedPackages.Contains(targetPackage)
                    && item.Package != targetPackage)
                .ToList();

        if (usagesInPackagesWhichDontDependOnTargetPackage.Count != 0)
        {
            throw new Exception(String.Format(
                Strings.ErrorUsagesInPackagesWhichDontDependOnTargetPackage,
                targetPackage.Name,
                FormatToIdList(usagesInPackagesWhichDontDependOnTargetPackage)));
        }
    }

    private static string FormatToIdList(List<ISchemaItem> schemaItems)
    {
        return "[" + string.Join("\n", schemaItems.Select(x => x.Id)) + "]";
    }
}

internal class GroupContainer
{
    internal static GroupContainer[] Create(SchemaItemGroup group)
    {
        var containers = new List<GroupContainer>();
        CreateGroupContainers(group, containers, 0);
        return containers.ToArray();
    }

    private static void CreateGroupContainers(SchemaItemGroup group,
        List<GroupContainer> containers, int level)
    {
        containers.Add(new GroupContainer(group, level));
        foreach (SchemaItemGroup childGroup in group.ChildGroups)
        {
            CreateGroupContainers(childGroup, containers, level + 1);
        }
    }
    public string Name { get; }
    public SchemaItemGroup Group { get; }

    private GroupContainer(SchemaItemGroup group, int level)
    {
        Name = new string(' ', level * 2) + group.Name;
        Group = group;
    }
};
