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
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for DataStructureDefaultSetDefault.
	/// </summary>
	[SchemaItemDescription("Default", "icon_default.png")]
    [HelpTopic("Default+Sets")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class DataStructureDefaultSetDefault : AbstractSchemaItem
	{
		public const string CategoryConst = "DataStructureDefaultSetDefault";

		public DataStructureDefaultSetDefault() : base(){}
		
		public DataStructureDefaultSetDefault(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public DataStructureDefaultSetDefault(Key primaryKey) : base(primaryKey)	{}

		#region Properties
		public Guid DataConstantId;

		[TypeConverter(typeof(DataConstantConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("default", "DataConstantId")]
        public DataConstant Default
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.DataConstantId;

				return (DataConstant)this.PersistenceProvider.RetrieveInstance(typeof(DataConstant), key);
			}
			set
			{
				if(value == null)
				{
					this.DataConstantId = Guid.Empty;
				}
				else
				{
					this.DataConstantId = (Guid)value.PrimaryKey["Id"];
				}

				UpdateName();
			}
		}
        
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
				this.DataStructureEntityId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
				this.Field = null;

				UpdateName();
			}
		}

		public Guid EntityFieldId;

		[TypeConverter(typeof(DataStructureEntityFieldConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("field", "EntityFieldId")]
        public IDataEntityColumn Field
		{
			get
			{
				return (IDataEntityColumn)this.PersistenceProvider.RetrieveInstance(typeof(DataStructureEntity), new ModelElementKey(this.EntityFieldId));
			}
			set
			{
				this.EntityFieldId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];

				UpdateName();
			}
		}
        
		public Guid ParameterId;

		[TypeConverter(typeof(ParameterReferenceConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("parameter", "ParameterId")]
        public SchemaItemParameter Parameter
		{
			get
			{
				return (SchemaItemParameter)this.PersistenceProvider.RetrieveInstance(typeof(SchemaItemParameter), new ModelElementKey(this.ParameterId)) as SchemaItemParameter;
			}
			set
			{
				this.ParameterId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
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
			dependencies.Add(this.Default);
			dependencies.Add(this.Field);

			if(this.Parameter != null)
			{
				dependencies.Add(this.Parameter);
			}

			base.GetExtraDependencies (dependencies);
		}

		public override void GetParameterReferences(AbstractSchemaItem parentItem, Hashtable list)
		{
			if(this.Parameter != null)
			{
				if(!list.ContainsKey(this.Parameter.Name))
				{
					ParameterReference pr = new ParameterReference();
					pr.PersistenceProvider = this.PersistenceProvider;
					pr.Parameter = this.Parameter;
					pr.Name = this.Parameter.Name;
					list.Add(this.Parameter.Name, pr);
				}
			}
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

		public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
		{
			ISchemaItem item = newNode as ISchemaItem;

			return item != null && item.PrimaryKey.Equals(this.ParentItem.PrimaryKey);
		}
		#endregion

		#region Private Methods
		private void UpdateName()
		{
			string entity = this.Entity == null ? "" : this.Entity.Name;
			string field = this.Field == null ? "" : this.Field.Name;
			string def = this.Default == null ? "" : this.Default.Name;

			this.Name = entity + "_" + field + "_" + def;
		}
		#endregion
	}
}
