using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
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