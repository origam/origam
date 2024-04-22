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

/// <summary>
/// Summary description for EntityFilterReference.
/// </summary>
[SchemaItemDescription("Filter Reference", "icon_filter-reference.png")]
[HelpTopic("Filter+Reference")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("Filter")]
[ClassMetaVersion("6.0.0")]
public class EntityFilterReference : AbstractSchemaItem
{
	public const string CategoryConst = "EntityFilterReference";

	public EntityFilterReference() : base() {}

	public EntityFilterReference(Guid schemaExtensionId) : base(schemaExtensionId) {}

	public EntityFilterReference(Key primaryKey) : base(primaryKey)	{}
	
	#region Overriden AbstractDataEntityColumn Members

	public override string ItemType
	{
		get
		{
				return CategoryConst;
			}
	}

	public override void GetParameterReferences(AbstractSchemaItem parentItem, System.Collections.Hashtable list)
	{
			if(this.Filter != null)
				base.GetParameterReferences(this.Filter as AbstractSchemaItem, list);
		}

	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
			dependencies.Add(this.Filter);

			base.GetExtraDependencies (dependencies);
		}

	public override void UpdateReferences()
	{
			foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
			{
				if(item.OldPrimaryKey != null)
				{
					if(item.OldPrimaryKey.Equals(this.Filter.PrimaryKey))
					{
						this.Filter = item as EntityFilter;
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

	#region Properties
	public Guid FilterId;

	[TypeConverter(typeof(EntityFilterConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRuleAttribute()]
	[XmlReference("filter", "FilterId")]
	public EntityFilter Filter
	{
		get
		{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.FilterId;

				return (EntityFilter)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
		set
		{
				if(value == null)
				{
					this.FilterId = Guid.Empty;

					this.Name = "";
				}
				else
				{
					this.FilterId = (Guid)value.PrimaryKey["Id"];

					this.Name = this.Filter.Name;
				}
			}
	}

	private string _roles;
	[Category("Security")]
	[XmlAttribute("roles")]
	public string Roles
	{
		get
		{
				return _roles;
			}
		set
		{
				_roles = value;
			}
	}
	#endregion
}