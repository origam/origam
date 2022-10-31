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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.DA.ObjectPersistence.Attributes;

namespace Origam.Schema.EntityModel
{
	public enum RelationType
	{
		Normal,
		FilterParent,
		NotExists,
		LeftJoin,
		InnerJoin
	}

	/// <summary>
	/// Summary description for EntityRelationItem.
	/// </summary>
	[SchemaItemDescription("Entity", "Entities", "icon_entity.png")]
    [HelpTopic("Entities")]
	[XmlModelRoot(CategoryConst)]
	[DefaultProperty("Entity")]
    [ClassMetaVersion("6.0.0")]
    public class DataStructureEntity : AbstractSchemaItem, ISchemaItemFactory
	{
		public const string CategoryConst = "DataStructureEntity";

		public DataStructureEntity() : base()	{}
		
		public DataStructureEntity(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public DataStructureEntity(Key primaryKey) : base(primaryKey)	{}

		#region Properties
		public Guid EntityId = Guid.Empty;

		/// <summary>
		/// Can be IDataEntity (as a root entity of the data structure) or
		/// IAssociation (as a child entity of another entity in the data structure).
		/// </summary>
		[TypeConverter(typeof(DataStructureEntityConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [NotNullModelElementRuleAttribute()]
        [RelationshipWithKeyRuleAttribute()]
        [XmlReference("entity", "EntityId")]
        public AbstractSchemaItem Entity
		{
			get
			{
				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.EntityId));
			}
			set
			{
				//				// We have to delete all child items
				//				this.ChildItems.Clear();

				if(value == null)
				{
					this.EntityId = Guid.Empty;

					this.Name = "";
				}
				else
				{
					if(!(value is IDataEntity | value is IAssociation))
						throw new ArgumentOutOfRangeException("Entity", value, ResourceUtils.GetString("ErrorNotIDataItem"));

					this.EntityId = (Guid)value.PrimaryKey["Id"];

					this.Name = this.Entity.Name;
				}
			}
		}

		[Browsable(false)]
		public IDataEntity EntityDefinition
		{
			get
			{
				if(this.Entity is IDataEntity)
					return this.Entity as IDataEntity;
				else if(this.Entity is IAssociation)
					return (this.Entity as IAssociation).AssociatedEntity;
				else if(this.Entity != null)
					throw new ArgumentOutOfRangeException("Entity", this.Entity, ResourceUtils.GetString("ErrorNotIDataEntity"));

				return null;
			}
		}

		[Browsable(false)]
		public DataStructureEntity RootEntity
		{
			get
			{
				return this.GetRootEntity(this);
			}
		}

		private DataStructureEntity GetRootEntity(DataStructureEntity parentEntity)
		{
			if(parentEntity.ParentItem is DataStructure)
				return parentEntity;
			else
				return GetRootEntity(parentEntity.ParentItem as DataStructureEntity);
		}

		private string _caption = "";

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

		private bool _allFields = true;
		[XmlAttribute("allFields")]
        public bool AllFields
		{
			get
			{
				return _allFields;
			}
			set
			{
				if(value == _allFields) return;

				if(value)
				{
					ArrayList list = this.ChildItemsByType(DataStructureColumn.CategoryConst);

					foreach(DataStructureColumn column in list)
					{
						if(column.Entity == null && ! column.UseCopiedValue 
							&& ! column.UseLookupValue && column.Field.Name == column.Name)
						{
							column.IsDeleted = true;
						}
					}
				}

				_allFields = value;
			}
		}

		private bool _ignoreImplicitFilters = false;
		[DefaultValue(false)]
		[Description("Disables row level security filters for an entity. Row-level filters are defined under entitities.")]
        [XmlAttribute("ignoreImplicitFilters")]
        public bool IgnoreImplicitFilters
		{
			get
			{
				return _ignoreImplicitFilters;
			}
			set
			{
				_ignoreImplicitFilters = value;
			}
		}

		private DataStructureIgnoreCondition _ignoreCondition = DataStructureIgnoreCondition.None;
		[DefaultValue(DataStructureIgnoreCondition.None)]
		[Description("Specify the condition resulting in not adding the whole entity to data query. Value 'IgnoreWhenNoFilters' means that the entity is skipped when neither one filter would be constructed for that entity. Value 'IgnoreWhenNoExplicitFilters' means the same as 'IgnoreWhenNoFilters' but it doesn't count implicit filters (aka row level security filters), so only datastructure filters are examined. Note, that filters can be avoided from construction according to their ignore condition settings and provided the whole corresponding filterset is 'dynamic'")]
        [XmlAttribute("ignoreCondition")]
        public DataStructureIgnoreCondition IgnoreCondition
		{
			get
			{
				return _ignoreCondition;
			}
			set
			{
				_ignoreCondition = value;
			}
		}

        private DataStructureConcurrencyHandling _concurrencyHandling 
            = DataStructureConcurrencyHandling.Standard;
        [DefaultValue(DataStructureConcurrencyHandling.Standard)]
        [Description("Specify behaviour during cuncurrency handling. Standard - concurrency checks are performed; LastWins - no concurrency checks are performed.")]
        [XmlAttribute("concurrencyHandling")]
        public DataStructureConcurrencyHandling ConcurrencyHandling
        {
            get
            {
                return _concurrencyHandling;
            }
            set
            {
                _concurrencyHandling = value;
            }
        }

		private RelationType _relationType = RelationType.Normal;
		[RelationTypeModelElementRule()]
        [XmlAttribute("relationType")]
        public RelationType RelationType
		{
			get
			{
				return _relationType;
			}
			set
			{
				_relationType = value;
			}
		}

		public Guid ConditionEntityConstantId;

		[TypeConverter(typeof(DataConstantConverter))]
        [XmlReference("conditionEntityConstant", "ConditionEntityConstantId")]
        public DataConstant ConditionEntityConstant
		{
			get
			{
				return (DataConstant)this.PersistenceProvider.RetrieveInstance(typeof(EntityFilter), new ModelElementKey(this.ConditionEntityConstantId));
			}
			set
			{
				this.ConditionEntityConstantId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		private string _conditionEntityParameterName;
		[Description("When defined (e.g. Resource_parName) together with a value of 'ConditionEntityConstant' then a value of the parameter is tested whether equals to a value of 'ConditionalEntityConstant'. If equals then an entity is APPLIED to resulting data query, otherwise the entity is skipped. When not defined, entity is allways applied.")]
        [XmlAttribute("conditionEntityParameterName")]
        public string ConditionEntityParameterName
		{
			get
			{
				return _conditionEntityParameterName;
			}
			set
			{
				_conditionEntityParameterName = (value == "") ? null : value; 
			}
		}

		private bool _useUpsert = false;
		[Category("Update"), DefaultValue(false)]
        [XmlAttribute("useUpsert")]
        public bool UseUPSERT
		{
			get
			{
				return _useUpsert;
			}
			set
			{
				_useUpsert = value;
			}
		}

#if ORIGAM_CLIENT
		private bool _columnsPopulated = false;
		private List<DataStructureColumn> _columns = new List<DataStructureColumn>();
#endif

		[Browsable(false)]
		public List<DataStructureColumn> Columns
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
							_columns = this.GetColumns();
							_columnsPopulated = true;
						}
					}
				}
				return _columns;
#else
				return this.GetColumns();
#endif
			}
		}

        public DataStructureColumn Column(string name)
        {
            foreach (DataStructureColumn col in Columns)
            {
                if (col.Name == name)
                {
                    return col;
                }
            }
            return null;
        }

		public List<DataStructureColumn> GetColumnsFromEntity()
		{
			List<DataStructureColumn> columns = new List<DataStructureColumn>();
			if(this.AllFields & this.EntityId != Guid.Empty) {
				foreach(IDataEntityColumn column in this.EntityDefinition.EntityColumns)
				{
					if(! column.ExcludeFromAllFields)
					{
						DataStructureColumn newColumn = new DataStructureColumn(this.SchemaExtensionId);
						newColumn.IsPersistable = false;
						newColumn.PersistenceProvider = this.PersistenceProvider;
						newColumn.Field = column;
						newColumn.Name = column.Name;
						newColumn.ParentItem = this;

						columns.Add(newColumn);
					}
				}
			}
			return columns;
		}

		public bool ExistsEntityFieldAsColumn(IDataEntityColumn entityField)
		{
			foreach (DataStructureColumn column in Columns)
			{
				if (column.Field.PrimaryKey.Equals(entityField.PrimaryKey))
				{
					return true;
				}
			}
			return false;
		}

		private List<DataStructureColumn> GetColumns()
		{
			// columns from entity (AllFields=true)
			List<DataStructureColumn> columns = GetColumnsFromEntity();
			
			// add all extra columns specified
			foreach(DataStructureColumn column in this.ChildItemsByType(DataStructureColumn.CategoryConst))
			{
				columns.Add(column);
			}

			columns.Sort();
			return columns;
		}
		#endregion

		#region Overriden AbstractSchemaItem Members

		public override void GetParameterReferences(AbstractSchemaItem parentItem, Hashtable list)
		{
			// relation has parameters (i.e. there are parameters in the JOIN clause
			if(this.Entity is IAssociation)
			{
				Hashtable childList = new Hashtable();
				this.Entity.GetParameterReferences(this.Entity, childList);

				// If children had some parameter references, we rename them and add them to the final
				// collection.
				foreach(DictionaryEntry entry in childList)
				{
					// we rename it using parent data structure entity name
					string name = this.ParentItem.Name + "_" + entry.Key;
					if(!list.ContainsKey(name))
					{
						list.Add(name, entry.Value);
					}
				}
			}

			foreach(AbstractSchemaItem item in this.Columns)
			{
				if(item is ParameterReference)
				{
					string name = this.Name + "_" + (item as ParameterReference).Parameter.Name;
					
					if(!list.ContainsKey(name))
						list.Add(name, item);
				}

				Hashtable childList = new Hashtable();

				item.GetParameterReferences(item, childList);

				// If children had some parameter references, we rename them and add them to the final
				// collection.
				foreach(DictionaryEntry entry in childList)
				{
					string name = this.Name + "_" + entry.Key;
					if(!list.ContainsKey(name))
					{
						list.Add(name, entry.Value);
					}
				}
			}
		}
		
		public override string ItemType
		{
			get
			{
				return DataStructureEntity.CategoryConst;
			}
		}

//		private bool _allColumnsPopulated = false;
//		public override SchemaItemCollection ChildItems
//		{
//			get
//			{
//				if(this.AllColumns && !_allColumnsPopulated && !_isPopulating)
//				{
//					PopulateColumns();
//					_allColumnsPopulated = true;
//				}
//				
//				return base.ChildItems;
//			}
//		}
		
//		[Browsable(false)]
//		public override bool PersistChildItems
//		{
//			get
//			{
//				return false;
//			}
//		}

		public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
		{
			// can move to the root only
			if(newNode.Equals(this.RootItem))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public override void GetExtraDependencies(ArrayList dependencies)
		{
			dependencies.Add(this.Entity);
			dependencies.Add(this.ConditionEntityConstant);

			base.GetExtraDependencies (dependencies);
		}

		#endregion

		#region ISchemaItemFactory Members

		[Browsable(false)]
		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {
									   typeof(DataStructureEntity),
									   typeof(DataStructureColumn)//,
//									   typeof(DataStructureEntityFilter)
								   };
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;
			
			if(type == typeof(DataStructureEntity))
			{
				item = new DataStructureEntity(schemaExtensionId);
			}
			else if(type == typeof(DataStructureColumn))
			{
				item = new DataStructureColumn(schemaExtensionId);
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorDataStructureEntityUnknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}

		#endregion
	}
}
