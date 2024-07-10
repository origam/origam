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

using System;
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;
using Origam.DA.Common;

namespace Origam.Schema.EntityModel;
/// <summary>
/// Summary description for DataStructureTemplate.
/// </summary>
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public abstract class DataStructureTemplate : AbstractSchemaItem
{
	public const string CategoryConst = "DataStructureTemplate";
	public DataStructureTemplate() : base(){}
	
	public DataStructureTemplate(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public DataStructureTemplate(Key primaryKey) : base(primaryKey)	{}
	#region Properties
	public Guid DataStructureEntityId;
	[TypeConverter(typeof(DataQueryEntityConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
    [NotNullModelElementRuleAttribute()]
    [XmlReference("entity", "DataStructureEntityId")]
	public DataStructureEntity Entity
	{
		get
		{
			ModelElementKey key = new ModelElementKey();
			key.Id = this.DataStructureEntityId;
			return (DataStructureEntity)this.PersistenceProvider.RetrieveInstance(typeof(DataStructureEntity), key);
		}
		set
		{
			if(value == null)
			{
				this.DataStructureEntityId = Guid.Empty;
			}
			else
			{
				this.DataStructureEntityId = (Guid)value.PrimaryKey["Id"];
			}
		}
	}
	#endregion
	#region Overriden AbstractSchemaItem Members
	public override string Icon
	{
		get
		{
			return "5";
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
		dependencies.Add(this.Entity);
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
	#endregion
}
