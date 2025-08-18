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
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;
/// <summary>
/// Summary description for EntityColumnReference.
/// </summary>
[SchemaItemDescription("Field Reference", "icon_field-reference.png")]
[HelpTopic("Field+Reference")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("Field")]
[ClassMetaVersion("6.0.0")]
public class EntityColumnReference : AbstractSchemaItem
{
	public const string CategoryConst = "EntityColumnReference";
	public EntityColumnReference() : base() {}
	public EntityColumnReference(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public EntityColumnReference(Key primaryKey) : base(primaryKey)	{}

	#region Overriden AbstractDataEntityColumn Members
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	public override string Icon
	{
		get
		{
			try
			{
				return this.Field.Icon;
			}
			catch
			{
				return "icon_field-reference.png";
			}
		}
	}
	public override void GetParameterReferences(ISchemaItem parentItem, Dictionary<string, ParameterReference> list)
	{
		if(this.Field != null)
			base.GetParameterReferences(Field, list);
	}
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		dependencies.Add(this.Field);
		base.GetExtraDependencies (dependencies);
	}
	public override void UpdateReferences()
	{
		foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
		{
			if(item.OldPrimaryKey != null)
			{
				if(item.OldPrimaryKey.Equals(this.Field.PrimaryKey))
				{
					this.Field = item as IDataEntityColumn;
					break;
				}
			}
		}
		base.UpdateReferences ();
	}
	public override ISchemaItemCollection ChildItems
	{
		get
		{
			return SchemaItemCollection.Create();
		}
	}
	#endregion
	#region Properties
	public Guid FieldId;
	[Category("Reference")]
	[TypeConverter(typeof(EntityColumnReferenceConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
    [XmlReference("field", "FieldId")]
	public IDataEntityColumn Field
	{
		get
		{
			ModelElementKey key = new ModelElementKey();
			key.Id = this.FieldId;
			return (ISchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), key) as IDataEntityColumn;
		}
		set
		{
			this.FieldId = (Guid)value.PrimaryKey["Id"];
			if(this.Name == null)
			{
				this.Name = this.Field.Name;
			}
		}
	}
	#endregion
}
