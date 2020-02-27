#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
using System.Collections;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Maps physical table to an entity.
	/// </summary>
	[XmlModelRoot(CategoryConst)]
	public abstract class AbstractDataEntity : AbstractSchemaItem, IDataEntity, ISchemaItemFactory
	{
		public AbstractDataEntity() : base() {Init();}

		public AbstractDataEntity(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}

		public AbstractDataEntity(Key primaryKey) : base(primaryKey)	{Init();}

		public const string  CategoryConst = "DataEntity";

		private void Init()
		{
			this.ChildItemTypes.InsertRange(0,
				new Type[] {
							   typeof(FieldMappingItem),
							   typeof(DetachedField),
							   typeof(FunctionCall),
							   typeof(AggregatedColumn),
							   typeof(LookupField),
							   typeof(EntityRelationItem),
							   typeof(DatabaseParameter),
							   typeof(EntityFilter),
							   typeof(DataEntityIndex),
							   typeof(EntitySecurityFilterReference),
							   typeof(EntitySecurityRule),
							   typeof(EntityConditionalFormatting)
						   }
				);
		}

		#region IDataEntity Members

		[Browsable(false)]
		public ArrayList EntityParameters
		{
			get
			{
				return this.ChildItemsByType(SchemaItemParameter.CategoryConst);
			}
		}

		[Browsable(false)]
		public virtual ArrayList EntityPrimaryKey
		{
			get
			{
				ArrayList list = new ArrayList();

				foreach(IDataEntityColumn column in this.EntityColumns)
				{
					if(column.IsPrimaryKey)
						list.Add(column);
				}

				return list;
			}
		}

		[EntityColumn("G01")]  
		public Guid DescribingFieldId;

		[TypeConverter(typeof(EntityColumnReferenceConverter))]
        [XmlReference("describingField", "DescribingFieldId")]
		public IDataEntityColumn DescribingField
		{
			get
			{
				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DescribingFieldId)) as IDataEntityColumn;
			}
			set => DescribingFieldId = (Guid?) value?.PrimaryKey["Id"] ?? Guid.Empty;
		}

		bool _entityIsReadOnly = false;
		[EntityColumn("B01")]
		[Category("Entity"), DefaultValue(false)]
		[Browsable(false)]
        [XmlAttribute("readOnly")]
        public bool EntityIsReadOnly
		{
			get
			{
				return _entityIsReadOnly;
			}
			set
			{
				_entityIsReadOnly = value;
			}
		}

		EntityAuditingType _auditingType = EntityAuditingType.None;
		[EntityColumn("I01")]
		[Category("Entity"), DefaultValue(EntityAuditingType.None)]
		[Description("Indicates if audit trail will be recorded for changes in this entity. If set to All, every change (create/update/delete) will be recorded in the audit log that users can browse in the UI. If set UpdatesAndDeletes only update and delete changes will be recorded.")]
        [XmlAttribute("audit")]
		public EntityAuditingType AuditingType
		{
			get
			{
				return _auditingType;
			}
			set
			{
				_auditingType = value;
			}
		}

		private string _caption = "";
		[EntityColumn("SS01")]
		[Category("Entity")]
		[Localizable(true)]
		[Description("User interface label for this entity. It is used e.g. for generic error messages about the entity ('Error occured in Invoice' instead of 'Error occured in InvDocRec').")]
        [XmlAttribute("label")]
        public string Caption
		{
			get
			{
				return _caption;
			}
			set
			{
				_caption = value;
			}
		}

#if ORIGAM_CLIENT
		private bool _columnsPopulated = false;
		private ArrayList _columns;
#endif

		[Browsable(false)]
		public ArrayList EntityColumns
		{
			get
			{
#if ORIGAM_CLIENT
				if(!_columnsPopulated)
				{
					lock(Lock)
					{
						if(!_columnsPopulated)
						{
							_columns = this.ChildItemsByType(AbstractDataEntityColumn.CategoryConst);
							_columnsPopulated = true;
						}
					}
				}
				return _columns;
#else
				return this.ChildItemsByType(AbstractDataEntityColumn.CategoryConst);
#endif
			}
		}

		[Browsable(false)]
		public ArrayList EntityRelations
		{
			get
			{
				return this.ChildItemsByType(EntityRelationItem.CategoryConst);
			}
		}

		[Browsable(false)]
		public ArrayList ChildEntities
		{
			get
			{
				ArrayList result = new ArrayList();
				foreach(EntityRelationItem relation in this.EntityRelations)
				{
					if(relation.IsParentChild) result.Add(relation.RelatedEntity);
				}

				return result;
			}
		}

		[Browsable(false)]
		public ArrayList ChildEntitiesRecursive
		{
			get
			{
				ArrayList result = new ArrayList();
				foreach(IDataEntity entity in this.ChildEntities)
				{
					result.Add(entity);
					result.AddRange(entity.ChildEntitiesRecursive);
				}

				return result;
			}
		}

		[Browsable(false)]
		public ArrayList EntityFilters
		{
			get
			{
				return this.ChildItemsByType(EntityFilter.CategoryConst);
			}
		}

		[Browsable(false)]
		public ArrayList EntityIndexes
		{
			get
			{
				return this.ChildItemsByType(DataEntityIndex.CategoryConst);
			}
		}

		[Browsable(false)]
		public ArrayList RowLevelSecurityRules
		{
			get
			{
				return this.ChildItemsByType(AbstractEntitySecurityRule.CategoryConst);
			}
		}

		[Browsable(false)]
		public ArrayList ConditionalFormattingRules
		{
			get
			{
				return this.ChildItemsByType(EntityConditionalFormatting.CategoryConst);
			}
		}

		[Browsable(false)]
		public ArrayList Constraints
		{
			get
			{
				ArrayList result = new ArrayList();
				DataEntityConstraint pk = new DataEntityConstraint(ConstraintType.PrimaryKey);
				foreach(IDataEntityColumn column in this.EntityColumns)
				{
					if(column.IsPrimaryKey)
					{
						pk.Fields.Add(column);
					}
                    DataEntityConstraint foreignKey = column.ForeignKeyConstraint;
                    if (foreignKey != null)
					{
                        result.Add(foreignKey);
						}
						}
				if(pk.Fields.Count > 0) result.Add(pk);
				return result;
			}
		}
		#endregion

		#region Overriden AbstractSchemaItem Methods
		public override void GetExtraDependencies(ArrayList dependencies)
		{
			if(this.DescribingField != null) dependencies.Add(this.DescribingField);
			base.GetExtraDependencies (dependencies);
		}

		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}
		#endregion
	}
}
