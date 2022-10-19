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

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for EntityRelationItem.
	/// </summary>
	[SchemaItemDescription("Relationship", "Relationships", "icon_relationship.png")]
    [HelpTopic("Relationships")]
	[XmlModelRoot(CategoryConst)]
	[DefaultProperty("RelatedEntity")]
    [ClassMetaVersion("6.0.0")]
    public class EntityRelationItem : AbstractSchemaItem, IAssociation, ISchemaItemFactory
	{
		public EntityRelationItem() : base(){}
		
		public EntityRelationItem(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public EntityRelationItem(Key primaryKey) : base(primaryKey)	{}

		public const string CategoryConst = "EntityRelation";

		#region Properties
		public Guid RelatedEntityId;

		[TypeConverter(typeof(EntityConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[NotNullModelElementRuleAttribute()]
        [XmlReference("relatedEntity", "RelatedEntityId")]
        public IDataEntity RelatedEntity
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.RelatedEntityId;

				return (IDataEntity)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				if(value == null)
				{
					this.RelatedEntityId = Guid.Empty;
					this.Name = "";
				}
				else
				{
					this.RelatedEntityId = (Guid)value.PrimaryKey["Id"];
					this.Name = this.RelatedEntity.Name;
				}

				// We have to delete all child items
				this.ChildItems.Clear();
			}
		}

		private bool _isParentChild = false;
		
        [XmlAttribute("parentChild")]
        public bool IsParentChild
		{
			get
			{
				return _isParentChild;
			}
			set
			{
				_isParentChild = value;
			}
		}


        [SelfJoinSameBaseRule]
        [XmlAttribute("selfJoin")]
        public bool IsSelfJoin { get; set; }

        private bool _isOR = false;

        [XmlAttribute("or")]
        public bool IsOR
		{
			get
			{
				return _isOR;
			}
			set
			{
				_isOR = value;
			}
		}
		#endregion

		#region Overriden AbstractSchemaItem Members
		public override bool UseFolders
		{
			get
			{
				return false;
			}
		}
		
		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			try
			{
				dependencies.Add(this.RelatedEntity);
			}
			catch
			{
				throw new ArgumentOutOfRangeException("RelatedEntityId", this.RelatedEntityId, ResourceUtils.GetString("ErrorRelatedEntity", this.Name, this.BaseEntity.Name));
			}
			base.GetExtraDependencies (dependencies);
		}

		public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
		{
			ISchemaItem item = newNode as ISchemaItem;

			return item != null && item.PrimaryKey.Equals(this.ParentItem.PrimaryKey);
		}

		#endregion

		#region IAssociation Members

		[Browsable(false)]
		public IDataEntity BaseEntity
		{
			get
			{
				return this.ParentItem as IDataEntity;
			}
		}

		[Browsable(false)]
		public IDataEntity AssociatedEntity
		{
			get
			{
				return this.RelatedEntity;
			}
		}

		#endregion

		#region ISchemaItemFactory Members

		[Browsable(false)]
		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {typeof(EntityRelationColumnPairItem),
									  typeof(EntityRelationFilter)
								  };
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(EntityRelationColumnPairItem))
			{
				item = new EntityRelationColumnPairItem(schemaExtensionId);
				item.Name = this.Name + "Key" + (this.ChildItems.Count + 1);
			}
			else if(type == typeof(EntityRelationFilter))
			{
				item = new EntityRelationFilter(schemaExtensionId);
				item.Name = "NewEntityRelationFilter";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorTableMappingItemUnknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			item.IsAbstract = this.IsAbstract;
			this.ChildItems.Add(item);
			return item;
		}

		#endregion
	}
}
		