#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

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

using Origam.DA.Common;
using System;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel;
[SchemaItemDescription("Relationship", "Relationships", "icon_relationship.png")]
[HelpTopic("Relationships")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("RelatedEntity")]
[ClassMetaVersion("6.0.0")]
public class EntityRelationItem : AbstractSchemaItem, IAssociation
{
	public EntityRelationItem() {}
	
	public EntityRelationItem(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public EntityRelationItem(Key primaryKey) : base(primaryKey) {}
	public const string CategoryConst = "EntityRelation";
	#region Properties
	public Guid RelatedEntityId;
	[TypeConverter(typeof(EntityConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
    [XmlReference("relatedEntity", "RelatedEntityId")]
    public IDataEntity RelatedEntity
	{
		get
		{
			var key = new ModelElementKey
			{
				Id = RelatedEntityId
			};
			return (IDataEntity)PersistenceProvider.RetrieveInstance(
				typeof(AbstractSchemaItem), key);
		}
		set
		{
			if(value == null)
			{
				RelatedEntityId = Guid.Empty;
				Name = "";
			}
			else
			{
				RelatedEntityId = (Guid)value.PrimaryKey["Id"];
				Name = RelatedEntity.Name;
			}
			// We have to delete all child items
			ChildItems.Clear();
		}
	}
	private bool _isParentChild = false;
	
    [XmlAttribute("parentChild")]
    public bool IsParentChild
	{
		get => _isParentChild;
		set => _isParentChild = value;
	}
    [SelfJoinSameBaseRule]
    [XmlAttribute("selfJoin")]
    public bool IsSelfJoin { get; set; }
    private bool _isOR = false;
    [XmlAttribute("or")]
    public bool IsOR
	{
		get => _isOR;
		set => _isOR = value;
	}
	#endregion
	#region Overriden AbstractSchemaItem Members
	
	public override bool UseFolders => false;
	public override string ItemType => CategoryConst;
	public override void GetExtraDependencies(
		System.Collections.ArrayList dependencies)
	{
		try
		{
			dependencies.Add(RelatedEntity);
		}
		catch
		{
			throw new ArgumentOutOfRangeException(
				"RelatedEntityId", RelatedEntityId, 
				ResourceUtils.GetString("ErrorRelatedEntity", Name, 
					BaseEntity.Name));
		}
		base.GetExtraDependencies (dependencies);
	}
	public override bool CanMove(UI.IBrowserNode2 newNode)
	{
		var item = newNode as ISchemaItem;
		return (item != null) 
		       && item.PrimaryKey.Equals(ParentItem.PrimaryKey);
	}
	#endregion
	#region IAssociation Members
	[Browsable(false)]
	public IDataEntity BaseEntity => ParentItem as IDataEntity;
	[Browsable(false)]
	public IDataEntity AssociatedEntity => RelatedEntity;
	#endregion
	#region ISchemaItemFactory Members
	[Browsable(false)]
	public override Type[] NewItemTypes => new[] {
		typeof(EntityRelationColumnPairItem), 
		typeof(EntityRelationFilter)
	};
	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{ 
		string itemName = null;
		if(typeof(T) == typeof(EntityRelationColumnPairItem))
		{
			itemName = Name + "Key" + (ChildItems.Count + 1);
		}
		else if(typeof(T) == typeof(EntityRelationFilter))
		{
			itemName = "NewEntityRelationFilter";
		}
		return base.NewItem<T>(schemaExtensionId, group, itemName);
	}
	#endregion
}
	