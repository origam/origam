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
	public enum DataEntityIndexSortOrder
	{
		Ascending,
		Descending
	}

	/// <summary>
	/// Summary description for DataEntityIndex.
	/// </summary>
	[SchemaItemDescription("Index", "Indexes", "icon_index.png")]
    [HelpTopic("Indexes")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class DataEntityIndex : AbstractSchemaItem, ISchemaItemFactory
	{
		public DataEntityIndex() : base(){}
		
		public DataEntityIndex(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public DataEntityIndex(Key primaryKey) : base(primaryKey)	{}

		public const string CategoryConst = "DataEntityIndex";

		#region Properties
		private bool _isUnique = false;

		[DefaultValue(false)]
        [XmlAttribute("unique")]
        public bool IsUnique
		{
			get
			{
				return _isUnique;
			}
			set
			{
				_isUnique = value;
			}
		}

		private bool _generateDeploymentScript = true;
		[Category("Mapping"), DefaultValue(true)]
		[Description("Indicates if deployment script will be generated for this index. If set to false, this index will be skipped from the deployment scripts generator.")]
        [XmlAttribute("generateDeploymentScript")]
        public bool GenerateDeploymentScript
		{
			get
			{
				return _generateDeploymentScript;
			}
			set
			{
				_generateDeploymentScript = value;
			}
		}
		#endregion
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
		#endregion

		#region ISchemaItemFactory Members

		[Browsable(false)]
		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {typeof(DataEntityIndexField)};
			}
		}

		public override SchemaItemGroup NewGroup(Guid schemaExtensionId, string groupName)
		{
			return null;
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(DataEntityIndexField))
			{
				item = new DataEntityIndexField(schemaExtensionId);
				item.Name = this.Name + "Field" + (this.ChildItems.Count + 1);
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorDataEntityIndexUknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}

		#endregion
	}
}
