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
using System.Collections;
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using System.Xml.Serialization;
using Origam.DA.Common;

namespace Origam.Schema.LookupModel
{
    /// <summary>
    /// Summary description for AbstractMenuItem.
    /// </summary>
    [XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
    public abstract class AbstractDataLookup : AbstractSchemaItem, IDataLookup
	{
		public const string CategoryConst = "DataLookup";

		public AbstractDataLookup() : base() {}

		public AbstractDataLookup(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public AbstractDataLookup(Key primaryKey) : base(primaryKey)	{}

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

		public override void GetParameterReferences(AbstractSchemaItem parentItem, System.Collections.Hashtable list)
		{
			base.GetParameterReferences(this.ListMethod, list);
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.ListDataStructure);
			dependencies.Add(this.ValueDataStructure);
			if(this.ListMethod != null) dependencies.Add(this.ListMethod);
			if(this.ValueMethod != null) dependencies.Add(this.ValueMethod);
			if(this.ValueSortSet != null) dependencies.Add(this.ValueSortSet);
			if(this.ListSortSet != null) dependencies.Add(this.ListSortSet);

			base.GetExtraDependencies (dependencies);
		}
		#endregion

		#region Properties

		[Browsable(false)]
		public ArrayList MenuBindings
		{
			get
			{
				return this.ChildItemsByType(DataLookupMenuBinding.CategoryConst);
			}
		}

		[Browsable(false)]
		public bool HasTooltip
		{
			get
			{
				ArrayList list = this.ChildItemsByType(AbstractDataTooltip.CategoryConst);

				if(list.Count > 0)
				{
					return true;
				}

				return false;
			}
		}

		[Browsable(false)]
		public ArrayList Tooltips
		{
			get
			{
				return this.ChildItemsByType(AbstractDataTooltip.CategoryConst);
			}
		}

		#region List
		private string _listValueMember;
		[Category("List")]
		[NotNullModelElementRule()]
        [XmlAttribute("listValueMember")]
		public string ListValueMember
		{
			get
			{
				return _listValueMember;
			}
			set
			{
				_listValueMember = value;
			}
		}

		private string _listDisplayMember;
		[Category("List")]
		[NotNullModelElementRule()]
        [XmlAttribute("listDisplayMember")]
        public string ListDisplayMember
		{
			get
			{
				return _listDisplayMember;
			}
			set
			{
				_listDisplayMember = value;
			}
		}

		private bool _isTree;
		[Category("List")]
		[DefaultValue(false)]
        [XmlAttribute("isTree")]
        public bool IsTree
		{
			get
			{
				return _isTree;
			}
			set
			{
				_isTree = value;
			}
		}

		private string _treeParentMember = "";
		[Category("List")]
		[XmlAttribute("treeParentMember")]
        public string TreeParentMember
		{
			get
			{
				return _treeParentMember;
			}
			set
			{
				_treeParentMember = value;
			}
		}
        
		public Guid ListDataStructureId;

		[Category("List")]
		[TypeConverter(typeof(DataStructureConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[NotNullModelElementRule()]
        [XmlReference("listDataStructure", "ListDataStructureId")]
		public DataStructure ListDataStructure
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.ListDataStructureId;

				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key) as DataStructure;
			}
			set
			{
				this.ListDataStructureId = (Guid)value.PrimaryKey["Id"];

				this.ListMethod = null;
				this.ListSortSet = null;
			}
		}

		private bool _suppressEmptyColumns;
		[Category("List")]
		[DefaultValue(false)]
        [XmlAttribute("suppressEmptyColumns")]
        public bool SuppressEmptyColumns
		{
			get
			{
				return _suppressEmptyColumns;
			}
			set
			{
				_suppressEmptyColumns = value;
			}
		}

		private bool _alwaysAllowReturnToForm;
		[Category("List")]
		[DefaultValue(false)]
        [XmlAttribute("alwaysAllowReturnToForm")]
        public bool AlwaysAllowReturnToForm
		{
			get
			{
				return _alwaysAllowReturnToForm;
			}
			set
			{
				_alwaysAllowReturnToForm = value;
			}
		}

		private bool _isFilteredServerside;
		[Category("List")]
		[DefaultValue(false)]
        [XmlAttribute("isFilteredServerside")]
        public bool IsFilteredServerside
		{
			get
			{
				return _isFilteredServerside;
			}
			set
			{
				_isFilteredServerside = value;
			}
		}

        private string _serversideFilterParameter;
        [Category("List")]
        [LookupServerSideFilterModelElementRule()]
        [XmlAttribute("serversideFilterParameter")]
        public string ServersideFilterParameter
        {
            get
            {
                return _serversideFilterParameter;
            }
            set
            {
                _serversideFilterParameter = value;
            }
        }

		private bool _searchByFirstColumnOnly;
		[Category("List")]
		[DefaultValue(false)]
        [XmlAttribute("searchByFirstColumnOnly")]
        public bool SearchByFirstColumnOnly
		{
			get
			{
				return _searchByFirstColumnOnly;
			}
			set
			{
				_searchByFirstColumnOnly = value;
			}
		}
		#endregion

		#region Value
		private string _valueValueMember;
		[Category("Value")]
		[NotNullModelElementRule("ValueDataStructure")]
        [XmlAttribute("valueValueMember")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public string ValueValueMember
		{
			get
			{
				return _valueValueMember;
			}
			set
			{
				_valueValueMember = value;
			}
		}

		private string _valueDisplayMember;
		[Category("Value")]
		[NotNullModelElementRule("ValueDataStructure")]
        [XmlAttribute("valueDisplayMember")]
        [RefreshProperties(RefreshProperties.Repaint)]
        public string ValueDisplayMember
		{
			get
			{
				return _valueDisplayMember;
			}
			set
			{
				_valueDisplayMember = value;
			}
		}

		public Guid ValueDataStructureId;

		[Category("Value")]
		[TypeConverter(typeof(DataStructureConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("valueDataStructure", "ValueDataStructureId")]
        public DataStructure ValueDataStructure
		{
			get
			{
				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ValueDataStructureId)) as DataStructure;
			}
			set
			{
				this.ValueDataStructureId = (Guid)value.PrimaryKey["Id"];
				this.ValueMethod = null;
				this.ValueSortSet = null;
			}
		}

		[Browsable(false)]
		public DataStructureEntity ValueEntity
		{
			get
			{
                if (this.ValueDataStructure != null)
                {
                    return this.ValueDataStructure.Entities[0] as DataStructureEntity;
                }
                return null;
            }
		}

		public DataStructureColumn ValueColumn
		{
			get
			{
                if (ValueEntity == null)
                {
                    return null;
                }
                DataStructureColumn valueColumn = 
					this.ValueEntity.Column(ValueValueMember);

				if(valueColumn == null)
				{
                    return null;
				}

				return valueColumn;
			}
		}

		public DataStructureColumn ValueDisplayColumn
		{
			get
			{
                if (ValueEntity == null)
                {
                    return null;
                }
                DataStructureColumn valueColumn =
                    this.ValueEntity.Column(ValueDisplayMember);

				if(valueColumn == null)
				{
                    return null;
				}

				return valueColumn;
			}
		}
		#endregion

		#region Filters
		private string _roleFilterMember;
		[Category("Filter")]
		[XmlAttribute("roleFilterMember")]
        public string RoleFilterMember
		{
			get
			{
				return _roleFilterMember;
			}
			set
			{
				_roleFilterMember = value;
			}
		}

		private string _featureFilterMember;
		[Category("Filter")]
		[XmlAttribute("featureFilterMember")]
        public string FeatureFilterMember
		{
			get
			{
				return _featureFilterMember;
			}
			set
			{
				_featureFilterMember = value;
			}
		}
		#endregion
		
		public Guid ListDataStructureMethodId;

		[TypeConverter(typeof(DataServiceDataLookupListMethodConverter))]
        [LookupServerSideFilterModelElementRule()]
        [Category("List")]
        [XmlReference("listMethod", "ListDataStructureMethodId")]
        public DataStructureMethod ListMethod
		{
			get
			{
				return (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ListDataStructureMethodId));
			}
			set
			{
				this.ListDataStructureMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		public Guid ValueDataStructureMethodId;

		[TypeConverter(typeof(DataServiceDataLookupValueFilterConverter))]
		[Category("Value")]
        [XmlReference("valueMethod", "ValueDataStructureMethodId")]
        public DataStructureMethod ValueMethod
		{
			get
			{
				return (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ValueDataStructureMethodId));
			}
			set
			{
				this.ValueDataStructureMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		public Guid ValueDataStructureSortSetId;

		[TypeConverter(typeof(DataServiceDataLookupValueSortSetConverter))]
		[Category("Value")]
        [XmlReference("valueSortSet", "ValueDataStructureSortSetId")]
        public DataStructureSortSet ValueSortSet
		{
			get
			{
				return (DataStructureSortSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ValueDataStructureSortSetId));
			}
			set
			{
				this.ValueDataStructureSortSetId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		public Guid ListDataStructureSortSetId;

		[TypeConverter(typeof(DataServiceDataLookupListSortSetConverter))]
		[Category("List")]
        [XmlReference("listSortSet", "ListDataStructureSortSetId")]
        public DataStructureSortSet ListSortSet
		{
			get
			{
				return (DataStructureSortSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ListDataStructureSortSetId));
			}
			set
			{
				this.ListDataStructureSortSetId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {
										typeof(DataLookupMenuBinding),
										typeof(DataServiceDataTooltip)
									};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(DataLookupMenuBinding))
			{
				item = new DataLookupMenuBinding(schemaExtensionId);
				item.Name = "MenuBinding";
			}
			else if(type == typeof(DataServiceDataTooltip))
			{
				item = new DataServiceDataTooltip(schemaExtensionId);
				item.Name = "NewTooltip";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorDataLookupUnknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}

		#endregion

	}
}
