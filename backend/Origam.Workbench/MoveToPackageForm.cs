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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Origam.Workbench;

public partial class MoveToPackageForm : Form
{
    WorkbenchSchemaService schema = ServiceManager.Services.GetService<WorkbenchSchemaService>();
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

    private Package GetActiveItemPackage() {
        if (schema.ActiveNode is AbstractSchemaItem schemaItem) { 
            return schemaItem.Package;
        }
        if (schema.ActiveNode is SchemaItemGroup group) { 
            return group.Package; 
        }
        throw new Exception("Cannot process " + schema.ActiveNode.GetType());   
    }

    private ISchemaItemProvider GetActiveItemRootProvider() {
        if (schema.ActiveNode is AbstractSchemaItem schemaItem) { 
            return schemaItem.RootProvider;
        }
        if (schema.ActiveNode is SchemaItemGroup group) { 
            return group.RootProvider; 
        }
        throw new Exception("Cannot process " + schema.ActiveNode.GetType());   
    }

    private void okButton_Click(object sender, EventArgs e)
    {
        IPersistenceProvider persistenceProvider = ServiceManager.Services
            .GetService<IPersistenceService>().SchemaProvider;
        var targetPackage = packageComboBox.SelectedItem as Package;
        var targetGroup = groupComboBox.SelectedItem as SchemaItemGroup;
        if (targetPackage == null) { 
            return;
        }

        if (schema.ActiveNode is SchemaItemGroup activeGroup)
        {
            persistenceProvider.BeginTransaction();
            activeGroup.Package = targetPackage;
            activeGroup.ParentGroup = targetGroup;
            foreach (var child in activeGroup.ChildItemsRecursive) {
                if (child is SchemaItemGroup group) {
                    group.Package = targetPackage;
                    group.Persist();
                }
            }
            foreach (var child in activeGroup.ChildItems)
            {
                if (child is AbstractSchemaItem schemaItem)
                {
                    schemaItem.Package = targetPackage;
                    schemaItem.Persist();
                }
            }
            activeGroup.Persist();
            persistenceProvider.EndTransaction();
        }
        else if (schema.ActiveNode is AbstractSchemaItem activeItem)
        {
            CheckCanBeMovedOrThrow(activeItem, targetPackage);

            activeItem.SetExtensionRecursive(targetPackage);
            activeItem.Group = targetGroup;

            persistenceProvider.BeginTransaction();
            activeItem.Persist();
            persistenceProvider.EndTransaction();
        }
        else
        {
            return;
        }

        ISchemaItemProvider rootProvider = GetActiveItemRootProvider();
        schema.SchemaBrowser.EbrSchemaBrowser.RefreshItem(rootProvider);
        Close();
    }

    private void CancelButton_Click(object sender, EventArgs e)
    {
        Close();
    }

    private void packageComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        groupComboBox.Items.Clear();
        Package package = packageComboBox.SelectedItem as Package;
        List<SchemaItemGroup> childGroups = GetActiveItemRootProvider().ChildGroups;
        childGroups
            .OrderBy(group => group.Name)
            .Where(group => group.Package.Id == package.Id)
            .ForEach(group => groupComboBox.Items.Add(group));
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
