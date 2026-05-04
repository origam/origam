#region license
/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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
using System.ComponentModel;
using System.Linq;
using System.Security.Principal;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;

namespace Origam.Schema.LookupModel;

[SchemaItemDescription(name: "New Record Screen Binding", iconName: "icon_menu-binding.png")]
[HelpTopic(topic: "")]
[XmlModelRoot(category: CategoryConst)]
[DefaultProperty(name: "MenuItem")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class NewRecordScreenBinding : AbstractSchemaItem, IAuthorizationContextContainer
{
    public const string CategoryConst = "NewRecordScreenBinding";

    public NewRecordScreenBinding() { }

    public NewRecordScreenBinding(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public NewRecordScreenBinding(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    [Browsable(browsable: false)]
    public bool IsAvailable
    {
        get
        {
            if (
                MenuItem is not FormReferenceMenuItem referenceMenuItem
                || referenceMenuItem.ReadOnlyAccess
            )
            {
                return false;
            }
            IParameterService parameterService =
                ServiceManager.Services.GetService<IParameterService>();
            IOrigamAuthorizationProvider authorizationProvider =
                SecurityManager.GetAuthorizationProvider();
            IPrincipal principal = SecurityManager.CurrentPrincipal;
            string authContext = SecurityManager.GetReadOnlyRoles(
                roles: referenceMenuItem.AuthorizationContext
            );
            bool hasReadOnlyRole = SecurityManager
                .GetAuthorizationProvider()
                .Authorize(principal: SecurityManager.CurrentPrincipal, context: authContext);
            if (hasReadOnlyRole)
            {
                return false;
            }
            return authorizationProvider.Authorize(
                    principal: principal,
                    context: AuthorizationContext
                )
                && authorizationProvider.Authorize(
                    principal: principal,
                    context: MenuItem.AuthorizationContext
                )
                && parameterService.IsFeatureOn(featureCode: MenuItem.Features);
        }
    }

    #region Overriden ISchemaItem Members
    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: MenuItem);
        ISchemaItem menu = MenuItem;
        while (menu.ParentItem != null)
        {
            menu = menu.ParentItem;
            dependencies.Add(item: menu);
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override bool CanMove(UI.IBrowserNode2 newNode)
    {
        return newNode is AbstractDataLookup;
    }

    public override bool UseFolders => false;

    #endregion

    #region Properties
    public Guid MenuItemId;

    [Category(category: "Menu Reference")]
    [TypeConverter(type: typeof(MenuItemConverter))]
    [NotNullModelElementRule]
    [NotNullMenuRecordEditMethod]
    [NoRecursiveNewRecordScreenBindingsRule]
    [XmlReference(attributeName: "menuItem", idField: "MenuItemId")]
    public AbstractMenuItem MenuItem
    {
        get => PersistenceProvider.RetrieveInstance<AbstractMenuItem>(instanceId: MenuItemId);
        set => MenuItemId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    [Category(category: "Security")]
    [NotNullModelElementRule]
    [XmlAttribute(attributeName: "roles")]
    public string Roles { get; set; }

    [XmlAttribute(attributeName: "dialogWidth")]
    public int DialogWidth { get; set; }

    [XmlAttribute(attributeName: "dialogHeight")]
    public int DialogHeight { get; set; }

    #endregion

    #region IAuthorizationContextContainer Members

    [Browsable(browsable: false)]
    public string AuthorizationContext => Roles;

    #endregion

    #region ISchemaItemFactory Members

    public override Type[] NewItemTypes => new[] { typeof(NewRecordScreenBindingParameterMapping) };
    #endregion

    public NewRecordScreenBindingParameterMapping[] GetParameterMappings()
    {
        return ChildItems.OfType<NewRecordScreenBindingParameterMapping>().ToArray();
    }
}

[AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
class NoRecursiveNewRecordScreenBindingsRule : AbstractModelElementRuleAttribute
{
    public override Exception CheckRule(object instance)
    {
        if (!(instance is NewRecordScreenBinding newRecordScreenBinding))
        {
            return new Exception(
                message: $"{nameof(NoRecursiveNewRecordScreenBindingsRule)} can be only applied to type {nameof(NewRecordScreenBinding)}"
            );
        }
        if (newRecordScreenBinding.MenuItem == null)
        {
            return null;
        }
        if (newRecordScreenBinding.MenuItem is not FormReferenceMenuItem menuItem)
        {
            return new Exception(
                message: $"The MenuItem in {nameof(NewRecordScreenBinding)} must be a {nameof(FormReferenceMenuItem)}"
            );
        }
        IEnumerable<IDataLookup> allLookups = menuItem
            .Screen.ChildrenRecursive.OfType<ControlSetItem>()
            .SelectMany(selector: controlSetItem =>
            {
                IEnumerable<PanelControlSet> panelControlSets = FindScreenSections(
                    controlSetItem: controlSetItem
                );
                IEnumerable<ControlSetItem> comboBoxes = panelControlSets.SelectMany(
                    selector: FindComboBoxes
                );
                IEnumerable<IDataLookup> lookups = comboBoxes.SelectMany(selector: GetLookups);
                return lookups;
            })
            .Distinct();
        var conflictingNewRecordBindingIds = allLookups
            .Select(selector: lookup =>
                lookup.ChildItems.OfType<NewRecordScreenBinding>().FirstOrDefault()
            )
            .Where(predicate: binding => binding != null)
            .Select(selector: binding => binding.Id.ToString())
            .ToList();
        if (conflictingNewRecordBindingIds.Count != 0)
        {
            return new Exception(
                message: "The selected menu item references a screen with NewRecordScreenBindings. "
                    + "This would lead to nested NewRecordScreenBindings and is therefore not allowed. "
                    + $"The conflicting NewRecordScreenBinding ids are: [{string.Join(separator: ", ", values: conflictingNewRecordBindingIds)}]"
            );
        }
        return null;
    }

    private IEnumerable<PanelControlSet> FindScreenSections(ControlSetItem controlSetItem)
    {
        var dependencies = new List<ISchemaItem>();
        controlSetItem.GetExtraDependencies(dependencies: dependencies);
        return dependencies.OfType<PanelControlSet>();
    }

    private IEnumerable<ControlSetItem> FindComboBoxes(PanelControlSet panelControlSet)
    {
        if (panelControlSet == null)
        {
            return Enumerable.Empty<ControlSetItem>();
        }
        return panelControlSet
            .ChildrenRecursive.OfType<ControlSetItem>()
            .Where(predicate: item => item.Name == "AsCombo");
    }

    private IEnumerable<IDataLookup> GetLookups(ControlSetItem comboBox)
    {
        var dependencies = new List<ISchemaItem>();
        comboBox.GetExtraDependencies(dependencies: dependencies);
        return dependencies.OfType<IDataLookup>();
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        return CheckRule(instance: instance);
    }
}
