using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.MenuModel;

namespace Origam.Schema.LookupModel;

[SchemaItemDescription("New Record Screen Binding", "icon_menu-binding.png")]
[HelpTopic("")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("MenuItem")]
[ClassMetaVersion("6.0.0")]
public class NewRecordScreenBinding : AbstractSchemaItem, IAuthorizationContextContainer, IComparable
{
    public const string CategoryConst = "NewRecordScreenBinding";

    public NewRecordScreenBinding() { }

    public NewRecordScreenBinding(Guid schemaExtensionId) : base(schemaExtensionId) { }
    
    public NewRecordScreenBinding(Key primaryKey) : base(primaryKey)	{}
    
    #region Overriden AbstractSchemaItem Members
    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
    {
        dependencies.Add(MenuItem);
		
        AbstractSchemaItem menu = MenuItem;
        while(menu.ParentItem != null)
        {
            menu = menu.ParentItem;
            dependencies.Add(menu);
        }
        base.GetExtraDependencies (dependencies);
    }

    public override SchemaItemCollection ChildItems => new();

    public override bool CanMove(UI.IBrowserNode2 newNode)
    {
        return newNode is AbstractDataLookup;
    }

    #endregion

    #region Properties
    public Guid MenuItemId;

    [Category("Menu Reference")]
    [TypeConverter(typeof(MenuItemConverter))]
    [NotNullModelElementRule]
    [NotNullMenuRecordEditMethod]
    [NoRecursiveNewRecordScreenBindings]
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
}



[AttributeUsage(AttributeTargets.Property, AllowMultiple=false, Inherited=true)]
class NoRecursiveNewRecordScreenBindings : AbstractModelElementRuleAttribute 
{
    public override Exception CheckRule(object instance)
    {
        if (!(instance is NewRecordScreenBinding newRecordScreenBinding))
        {
            return new Exception(
                $"{nameof(NoRecursiveNewRecordScreenBindings)} can be only applied to type {nameof(NewRecordScreenBinding)}");  
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

        var allEntities = menuItem.Screen.DataStructure.Entities
            .Cast<DataStructureEntity>()
            .SelectMany(entity => entity.ChildrenRecursive.OfType<DataStructureEntity>().Concat(new []{entity}));

        IEnumerable<IDataLookup> allLookups = allEntities
            .SelectMany(entity =>
            {
                var dataStructureColumnLookups = entity.ChildItems.ToGeneric()
                    .OfType<DataStructureColumn>()
                    .Where(x => x.UseLookupValue)
                    .Select(x => x.FinalLookup);
                var entityLookups = entity.EntityDefinition.EntityColumns
                    .OfType<LookupField>()
                    .Select(lookupField => lookupField.DefaultLookup);
                return dataStructureColumnLookups.Concat(entityLookups);
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

    public override Exception CheckRule(object instance, string memberName)
    {
        return CheckRule(instance);
    }
}