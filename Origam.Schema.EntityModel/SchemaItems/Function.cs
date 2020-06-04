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
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel
{
	public enum OrigamFunctionType
	{
		Standard = 0,
		Database = 1
	}

	/// <summary>
	/// Summary description for FilterExpression.
	/// </summary>
	[SchemaItemDescription("Function", "icon_10_functions.png")]
    [HelpTopic("Functions")]
	[XmlModelRoot(ItemTypeConst)]
	public class Function : AbstractSchemaItem, ISchemaItemFactory
	{
		public const string ItemTypeConst = "Function";

		public Function() : base() {}

		public Function(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public Function(Key primaryKey) : base(primaryKey)	{}
	
		#region Overriden AbstractDataEntityColumn Members
		
		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ItemTypeConst;
			}
		}

		[Browsable(false)]
		public override bool UseFolders
		{
			get
			{
				return false;
			}
		}

		public override string Icon
		{
			get
			{
				switch(this.FunctionType) 
				{
					case OrigamFunctionType.Database:
						return "55";

					default:
						return "icon_10_functions.png";
				}
			}
		}

		#endregion

		#region ISchemaItemFactory Members

		[Browsable(false)]
		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[1] {typeof(FunctionParameter)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			if(type == typeof(FunctionParameter))
			{
				FunctionParameter item = new FunctionParameter(schemaExtensionId);
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewFunctionParameter";

				item.Group = group;
				this.ChildItems.Add(item);

				return item;
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorFunctionUnknownType"));
		}

		#endregion

		#region Properties
		private OrigamDataType _dataType;
		[EntityColumn("I01")] 
        [XmlAttribute("dataType")]
		public OrigamDataType DataType
		{
			get
			{
				return _dataType;
			}
			set
			{
				_dataType = value;
			}
		}

		private OrigamFunctionType _functionType;
		[EntityColumn("I02")]
        [XmlAttribute("type")]
        public OrigamFunctionType FunctionType
		{
			get
			{
				return _functionType;
			}
			set
			{
				_functionType = value;
			}
		}
		#endregion
	}
}
