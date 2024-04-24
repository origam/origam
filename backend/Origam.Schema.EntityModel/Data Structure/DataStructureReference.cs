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

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for EntityColumnReference.
	/// </summary>
	[SchemaItemDescription("Data Structure Reference", "data-structure-reference.png")]
    [HelpTopic("Data+Structure+Reference")]
	[XmlModelRoot(CategoryConst)]
    [DefaultProperty("DataStructure")]
    [ClassMetaVersion("6.0.0")]
    public  class DataStructureReference : AbstractSchemaItem, IDataStructureReference
	{
		public const string CategoryConst = "DataStructureReference";

		public DataStructureReference() : base() {}

		public DataStructureReference(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public DataStructureReference(Key primaryKey) : base(primaryKey)	{}
	
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
			if(this.DataStructure != null)
				base.GetParameterReferences(this.DataStructure as AbstractSchemaItem, list);
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.DataStructure);
			if(this.Method  != null) dependencies.Add(this.Method);
			if(this.SortSet != null) dependencies.Add(this.SortSet);

			base.GetExtraDependencies (dependencies);
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
		public Guid DataStructureId;

		[Category("Reference")]
		[TypeConverter(typeof(DataStructureConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("dataStructure", "DataStructureId")]
        [NotNullModelElementRule()]
        public DataStructure DataStructure
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.DataStructureId;

				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key) as DataStructure;
			}
			set
			{
				this.DataStructureId = (Guid)value.PrimaryKey["Id"];

				this.Method = null;
				this.SortSet = null;

				this.Name = this.DataStructure.Name;
			}
		}

		public Guid DataStructureMethodId;

		[TypeConverter(typeof(DataStructureReferenceMethodConverter))]
		[Category("Reference")]
        [XmlReference("method", "DataStructureMethodId")]
        public DataStructureMethod Method
		{
			get
			{
				return (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(DataStructureMethodId));
			}
			set
			{
				this.DataStructureMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
        
		public Guid DataStructureSortSetId;

		[TypeConverter(typeof(DataStructureReferenceSortSetConverter))]
		[Category("Reference")]
        [XmlReference("sortSet", "DataStructureSortSetId")]
        public DataStructureSortSet SortSet
		{
			get
			{
				return (DataStructureSortSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureSortSetId));
			}
			set
			{
				this.DataStructureSortSetId = (value == null? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
		#endregion
	}
}
