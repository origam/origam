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

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for AbstractService.
	/// </summary>
	[SchemaItemDescription("Service", "service.png")]
    [HelpTopic("Services")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class Service : AbstractSchemaItem, IService, ISchemaItemFactory
	{
		public const string CategoryConst = "Service";

		public Service() : base() {}

		public Service(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public Service(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractSchemaItem Members
		
		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}
		#endregion

		#region Properties
		private string _classPath;
		[XmlAttribute("classPath")]
		public string ClassPath
		{
			get
			{
				return _classPath;
			}
			set
			{
				_classPath = value;
			}
		}
		#endregion

		#region ISchemaItemFactory Members

		[Browsable(false)]
		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[1] {typeof(ServiceMethod)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(ServiceMethod))
			{
				item = new ServiceMethod(schemaExtensionId);
				item.Name = "NewServiceMethod";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, "This type is not supported by ServiceModel");

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
		}

		#endregion
	}
}
