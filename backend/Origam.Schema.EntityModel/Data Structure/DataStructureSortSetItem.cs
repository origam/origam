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
/// Summary description for DataStructureFilterSetFilter.
/// </summary>
[SchemaItemDescription("Sort Field", "icon_sort-field.png")]
[HelpTopic("Sort+Field")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class DataStructureSortSetItem : AbstractSchemaItem
{
	public const string CategoryConst = "DataStructureSortSetItem";

	public DataStructureSortSetItem() : base(){}
		
	public DataStructureSortSetItem(Guid schemaExtensionId) : base(schemaExtensionId) {}

	public DataStructureSortSetItem(Key primaryKey) : base(primaryKey)	{}

	#region Properties
	public Guid DataStructureEntityId;

	[TypeConverter(typeof(DataQueryEntityConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[Category("Sorting")]
	[NotNullModelElementRule()]
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

				UpdateName();
			}
	}

	private string _fieldName;
		
	[SortSetItemValidModelElementRuleAttribute()]
	[TypeConverter(typeof(DataStructureColumnStringConverter))]
	[Category("Sorting"), RefreshProperties(RefreshProperties.Repaint)]
	[XmlAttribute("fieldName")]
	public string FieldName
	{
		get
		{
				return _fieldName;
			}
		set
		{
				_fieldName = value;
				this.UpdateName();
			}
	}

	private int _sortOrder = 0;
	[Category("Sorting"), DefaultValue(0), RefreshProperties(RefreshProperties.Repaint)]
	[XmlAttribute("sortOrder")]
	public int SortOrder
	{
		get
		{
				return _sortOrder;
			}
		set
		{
				_sortOrder = value;
			}
	}

	private DataStructureColumnSortDirection _sortDirection = DataStructureColumnSortDirection.Ascending;
	[Category("Sorting"), DefaultValue(DataStructureColumnSortDirection.Ascending)]
	[XmlAttribute("sortDirection")]
	public DataStructureColumnSortDirection SortDirection
	{
		get
		{
				return _sortDirection;
			}
		set
		{
				_sortDirection = value;
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

			/* return a column used in a sort set */
			/* firstly look at columns defined on datastructure level */
			foreach (DataStructureColumn dsColumn
					in this.Entity.ChildItemsByType(DataStructureColumn.CategoryConst))
			{
				if (FieldName == dsColumn.Name)
				{
					// return data structure entity
					dependencies.Add(dsColumn);
				}
			}
			/* look at columns defined on entity level (AllFields = true) */ 
			foreach (DataStructureColumn dsColumn in this.Entity.GetColumnsFromEntity())
			{
				// check whether the name is referenced in fieldName
				if (FieldName == dsColumn.Name)
				{
					// data entity level - return data entity
					dependencies.Add(dsColumn.Field);
				}
			}

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
			string entity = this.Entity == null ? "" : this.Entity.Name;
			string field = this.FieldName == null ? "" : this.FieldName;

			this.Name = entity + "_" + this.SortOrder.ToString() + "_" + field;
		}
	#endregion

	#region IComparable Members

	public override int CompareTo(object obj)
	{
            DataStructureSortSetItem compareItem = obj as DataStructureSortSetItem;

            if (compareItem == null)
            {
                return base.CompareTo(obj);
            }

			return this.SortOrder.CompareTo(compareItem.SortOrder);
		}

	#endregion
}