#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for EntityColumnReference.
	/// </summary>
	[SchemaItemDescription("Data Constant Reference", "Parameters", 57)]
    [HelpTopic("Data+Constant+Reference")]
	[XmlModelRoot(ItemTypeConst)]
    [DefaultProperty("DataConstant")]
    public class DataConstantReference : AbstractSchemaItem
	{
		public const string ItemTypeConst = "DataConstantReference";

		public DataConstantReference() : base() {}

		public DataConstantReference(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public DataConstantReference(Key primaryKey) : base(primaryKey)	{}
	
		#region Overriden AbstractDataEntityColumn Members
		
		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ItemTypeConst;
			}
		}

		public override string Icon
		{
			get
			{
				return "57";
			}
		}

		public override void GetParameterReferences(AbstractSchemaItem parentItem, System.Collections.Hashtable list)
		{
			if(this.DataConstant != null)
				base.GetParameterReferences(this.DataConstant as AbstractSchemaItem, list);
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.DataConstant);

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
		[EntityColumn("G01")]  
		public Guid DataConstantId;

		[Category("Reference")]
		[TypeConverter(typeof(DataConstantConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[NotNullModelElementRule()]
        [XmlReference("constant", "DataConstantId")]

        public DataConstant DataConstant
		{
			get
			{
				try
				{
					return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataConstantId)) as DataConstant;
				}
				catch
				{
					throw new Exception(ResourceUtils.GetString("ErrorDataConstantNotFound", this.Name));
				}
			}
			set
			{
				this.DataConstantId = (Guid)value.PrimaryKey["Id"];
			}
		}
		#endregion
	}
}
