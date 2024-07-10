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
/// Summary description for DataStructureRuleDependency.
/// </summary>
[SchemaItemDescription("Dependency", "Dependencies", "icon_rule-dependency.png")]
[HelpTopic("Rule+Set+Rule+Dependency")]
[XmlModelRoot(CategoryConst)]
[DefaultProperty("Entity")]
[ClassMetaVersion("6.0.0")]
public class DataStructureRuleDependency : AbstractSchemaItem
{
	public const string CategoryConst = "DataStructureRuleDependency";
	public DataStructureRuleDependency() : base(){}
	
	public DataStructureRuleDependency(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public DataStructureRuleDependency(Key primaryKey) : base(primaryKey)	{}
	#region Properties
	public Guid DataStructureEntityId;
	[TypeConverter(typeof(DataQueryEntityConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("entity", "DataStructureEntityId")]
	public DataStructureEntity Entity
	{
		get
		{
			return (DataStructureEntity)this.PersistenceProvider.RetrieveInstance(typeof(DataStructureEntity), new ModelElementKey(this.DataStructureEntityId));
		}
		set
		{
			this.DataStructureEntityId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			this.Field = null;
		}
	}
	
	public Guid FieldId;
	[TypeConverter(typeof(DataStructureEntityFieldConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("field", "FieldId")]
	public IDataEntityColumn Field
	{
		get
		{
			return (IDataEntityColumn)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.FieldId));
		}
		set
		{
			this.FieldId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			UpdateName();
		}
	}
	#endregion
	#region Overriden AbstractSchemaItem Members
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
		dependencies.Add(this.Entity);
		dependencies.Add(this.Field);
		base.GetExtraDependencies (dependencies);
	}
	public override void UpdateReferences()
	{
		foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
		{
			if(item.OldPrimaryKey != null)
			{
				if(item.OldPrimaryKey.Equals(this.Entity.PrimaryKey))
				{
					this.Entity = item as DataStructureEntity;
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
	#region Private Methods
	private void UpdateName()
	{
		if(this.Entity != null && this.Field != null)
		{
			this.Name = this.Entity.Name + "_" + this.Field.Name;
		}
	}
	#endregion
}
