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
using System.Collections.Generic;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel;
/// <summary>
/// Summary description for EntityRelationColumnPairItem.
/// </summary>
[SchemaItemDescription("Key", 3)]
[HelpTopic("Relationship+Key")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("BaseEntityField")]
[ClassMetaVersion("6.0.0")]
public class EntityRelationColumnPairItem : AbstractSchemaItem
{
	public const string CategoryConst = "EntityRelationColumnPair";
	public EntityRelationColumnPairItem() : base(){}
	
	public EntityRelationColumnPairItem(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public EntityRelationColumnPairItem(Key primaryKey) : base(primaryKey)	{}
	#region Properties
	public Guid BaseEntityColumnId;
	[TypeConverter(typeof(RelationPrimaryKeyColumnConverter))]
	[NotNullModelElementRuleAttribute()]
    [XmlReference("baseEntityField", "BaseEntityColumnId")]
    public IDataEntityColumn BaseEntityField
	{
		get
		{
			ModelElementKey key = new ModelElementKey();
			key.Id = this.BaseEntityColumnId;
			return (IDataEntityColumn)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
		}
		set
		{
			if(value == null)
			{
				this.BaseEntityColumnId = Guid.Empty;
			}
			else
			{
				this.BaseEntityColumnId = (Guid)value.PrimaryKey["Id"];
			}
		}
	}
	public Guid RelatedEntityColumnId;
	[TypeConverter(typeof(RelationForeignKeyColumnConverter))]
	[NotNullModelElementRuleAttribute()]
    [XmlReference("relatedEntityField", "RelatedEntityColumnId")]
    public IDataEntityColumn RelatedEntityField
	{
		get
		{
			ModelElementKey key = new ModelElementKey();
			key.Id = this.RelatedEntityColumnId;
			return (IDataEntityColumn)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
		}
		set
		{
			if(value == null)
			{
				this.RelatedEntityColumnId = Guid.Empty;
			}
			else
			{
				this.RelatedEntityColumnId = (Guid)value.PrimaryKey["Id"];
			}
		}
	}
	#endregion
	#region Overriden AbstractSchemaItem Members
	public override string Icon
	{
		get
		{
			return "3";
		}
	}
	public override string ItemType
	{
		get
		{
			return EntityRelationColumnPairItem.CategoryConst;
		}
	}
	public override void GetParameterReferences(AbstractSchemaItem parentItem, Dictionary<string, ParameterReference> list)
	{
		(this.BaseEntityField as AbstractSchemaItem).GetParameterReferences (this.BaseEntityField as AbstractSchemaItem, list);
		(this.RelatedEntityField as AbstractSchemaItem).GetParameterReferences (this.RelatedEntityField as AbstractSchemaItem, list);
	}
	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
		dependencies.Add(this.BaseEntityField);
		dependencies.Add(this.RelatedEntityField);
		base.GetExtraDependencies (dependencies);
	}
	public override void UpdateReferences()
	{
		foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
		{
			if(item.OldPrimaryKey != null)
			{
				if(item.OldPrimaryKey.Equals(this.BaseEntityField.PrimaryKey))
				{
					this.BaseEntityField = item as IDataEntityColumn;
					break;
				}
			}
		}
		base.UpdateReferences ();
	}
	public override SchemaItemCollection ChildItems
	{
		get
		{
			return new SchemaItemCollection();
		}
	}
	#endregion
}
