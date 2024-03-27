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
using System.Collections;
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

[SchemaItemDescription("New Record Screen Binding", "icon_menu-binding.png")]
[HelpTopic("")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("MenuItem")]
[ClassMetaVersion("6.0.0")]
public class NewRecordScreenBinding 
    : AbstractSchemaItem, IAuthorizationContextContainer
{
    public const string CategoryConst = "NewRecordScreenBinding";

    public NewRecordScreenBinding() {}

    public NewRecordScreenBinding(Guid schemaExtensionId) 
        : base(schemaExtensionId) {}
    
    public NewRecordScreenBinding(Key primaryKey) : base(primaryKey) {}
    
    [Browsable(false)]
    public bool IsAvailable
    {
        get
        {
            if (MenuItem is not FormReferenceMenuItem referenceMenuItem 
                || referenceMenuItem.ReadOnlyAccess)
            {
                return false;
            }
            IParameterService parameterService = ServiceManager.Services
                .GetService<IParameterService>();
            IOrigamAuthorizationProvider authorizationProvider =
                SecurityManager.GetAuthorizationProvider();
            IPrincipal principal = SecurityManager.CurrentPrincipal;
            string authContext = SecurityManager
                .GetReadOnlyRoles(referenceMenuItem.AuthorizationContext);
            bool hasReadOnlyRole = SecurityManager
                .GetAuthorizationProvider()
                .Authorize(SecurityManager.CurrentPrincipal, authContext);
            if (hasReadOnlyRole)
            {
                return false;
            }
            return
                authorizationProvider.Authorize(principal, AuthorizationContext)
                && authorizationProvider.Authorize(principal,
                    MenuItem.AuthorizationContext)
                && parameterService.IsFeatureOn(MenuItem.Features);
        }
    }


    #region Overriden AbstractSchemaItem Members
    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(ArrayList dependencies)
    {
        dependencies.Add(MenuItem);
        AbstractSchemaItem menu = MenuItem;
        while (menu.ParentItem != null)
        {
            menu = menu.ParentItem;
            dependencies.Add(menu);
        }
        base.GetExtraDependencies (dependencies);
    }

    public override bool CanMove(UI.IBrowserNode2 newNode)
    {
        return newNode is AbstractDataLookup;
    }
    
    public override bool UseFolders => false;

    #endregion

    #region Properties
    public Guid MenuItemId;

    [Category("Menu Reference")]
    [TypeConverter(typeof(MenuItemConverter))]
    [NotNullModelElementRule]
    [NotNullMenuRecordEditMethod]
    [NoRecursiveNewRecordScreenBindingsRule]
    [XmlReference("menuItem", "MenuItemId")]
    public AbstractMenuItem MenuItem
    {
        get => PersistenceProvider.RetrieveInstance<AbstractMenuItem>(MenuItemId);
        set => MenuItemId = value == null 
            ? Guid.Empty 
            : (Guid)value.PrimaryKey["Id"];
    }

    [Category("Security")]
    [NotNullModelElementRule]
    [XmlAttribute("roles")]
    public string Roles { get; set; }
    
    [XmlAttribute("dialogWidth")]
    public int DialogWidth { get; set; }
    
    [XmlAttribute("dialogHeight")]
    public int DialogHeight { get; set; }
		
    #endregion

    #region IAuthorizationContextContainer Members

    [Browsable(false)]
    public string AuthorizationContext => Roles;

    #endregion
    
    #region ISchemaItemFactory Members

    public override Type[] NewItemTypes => new[] 
    { 
        typeof(NewRecordScreenBindingParameterMapping)
    };
    #endregion

    public NewRecordScreenBindingParameterMapping[] GetParameterMappings()
    {
        return ChildItems
            .ToGeneric()
            .OfType<NewRecordScreenBindingParameterMapping>()
            .ToArray();
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
class NoRecursiveNewRecordScreenBindingsRule : AbstractModelElementRuleAttribute 
{
    public override Exception CheckRule(object instance)
    {
        if (!(instance is NewRecordScreenBinding newRecordScreenBinding))
        {
            return new Exception(
                $"{nameof(NoRecursiveNewRecordScreenBindingsRule)} can be only applied to type {nameof(NewRecordScreenBinding)}");  
        }
        if (newRecordScreenBinding.MenuItem == null)
        {
            return null;
        }
        if (newRecordScreenBinding.MenuItem is not FormReferenceMenuItem menuItem)
        {
            return new Exception(
                $"The MenuItem in {nameof(NewRecordScreenBinding)} must be a {nameof(FormReferenceMenuItem)}"); 
        }
        IEnumerable<IDataLookup> allLookups = menuItem.Screen.ChildrenRecursive
            .OfType<ControlSetItem>()
            .SelectMany(controlSetItem =>
            {
                IEnumerable<PanelControlSet> panelControlSets = FindScreenSections(controlSetItem);
                IEnumerable<ControlSetItem> comboBoxes = panelControlSets.SelectMany(FindComboBoxes);
                IEnumerable<IDataLookup> lookups = comboBoxes.SelectMany(GetLookups);
                return lookups;
            })
            .Distinct();
        var conflictingNewRecordBindingIds = allLookups
            .Select(lookup => lookup.ChildItems.ToGeneric().OfType<NewRecordScreenBinding>().FirstOrDefault())
            .Where(binding => binding != null)
            .Select(binding => binding.Id.ToString())
            .ToList();
        if (conflictingNewRecordBindingIds.Count != 0)
        {
            return new Exception("The selected menu item references a screen with NewRecordScreenBindings. " +
                                 "This would lead to nested NewRecordScreenBindings and is therefore not allowed. " +
                                 $"The conflicting NewRecordScreenBinding ids are: [{string.Join(", ",conflictingNewRecordBindingIds)}]");
        }
        return null;
    }

    private IEnumerable<PanelControlSet> FindScreenSections(ControlSetItem controlSetItem)
    {
        var dependencies = new ArrayList();
        controlSetItem.GetExtraDependencies(dependencies);
        return dependencies.OfType<PanelControlSet>();
    }

    private IEnumerable<ControlSetItem> FindComboBoxes(PanelControlSet panelControlSet)
    {
        if (panelControlSet == null)
        {
            return Enumerable.Empty<ControlSetItem>();
        }
        return panelControlSet.ChildrenRecursive
            .OfType<ControlSetItem>()
            .Where(item => item.Name == "AsCombo");
    }

    private IEnumerable<IDataLookup> GetLookups(ControlSetItem comboBox)
    {
        var dependencies = new ArrayList();
        comboBox.GetExtraDependencies(dependencies);
        return dependencies.OfType<IDataLookup>();
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        return CheckRule(instance);
    }
}