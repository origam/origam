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
using System.Collections;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for DataQuery.
	/// </summary>
	[SchemaItemDescription("Template Set", "Template Sets", 
        "icon_template-set.png")]
    [HelpTopic("Template+Sets")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class DataStructureTemplateSet : AbstractSchemaItem, ISchemaItemFactory
	{
		public const string CategoryConst = "DataStructureTemplateSet";

		public DataStructureTemplateSet() : base() {}

		public DataStructureTemplateSet(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public DataStructureTemplateSet(Key primaryKey) : base(primaryKey)	{}
	
		#region Properties
		[Browsable(false)]
		public ArrayList Templates
		{
			get
			{
				return this.ChildItemsByType(DataStructureTemplate.CategoryConst);
			}
		}
		#endregion

		#region Public Methods
		public ArrayList TemplatesByDataMember(string dataMember)
		{
			ArrayList result = new ArrayList();

			foreach(DataStructureTemplate template in this.Templates)
			{
				if(template.Entity.Name == dataMember)
				{
					result.Add(template);
				}
			}

			return result;
		}
		#endregion

		#region Overriden AbstractDataEntityColumn Members
		
		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}

		public override bool UseFolders
		{
			get
			{
				return false;
			}
		}

		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {typeof(DataStructureTransformationTemplate)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(DataStructureTransformationTemplate))
			{
				item = new DataStructureTransformationTemplate(schemaExtensionId);
				item.Name = "NewTransformationTemplate";

			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorDataStructureFilterSetUnknownType"));

			item.Group = group;
			item.RootProvider = this;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);

			return item;
		}

		#endregion
	}
}
