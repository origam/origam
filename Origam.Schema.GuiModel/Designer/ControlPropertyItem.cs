#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

using Origam.DA.ObjectPersistence;
using System;
using System.Xml.Serialization;

namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for ControlPropertyItem.
	/// </summary>
	/// 

	public enum ControlPropertyValueType {Integer=0, Boolean, String, Xml, UniqueIdentifier}


	[SchemaItemDescription("Property", "Properties", 3)]
	[XmlModelRoot(ItemTypeConst)]
	public class ControlPropertyItem : AbstractSchemaItem 
	{
		public const string ItemTypeConst = "ControlPropertyItem";

		public ControlPropertyItem() : base(){}
		
		public ControlPropertyItem(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public ControlPropertyItem(Key primaryKey) : base(primaryKey)	{}

		#region Properties

		private ControlPropertyValueType _propertyType;

		[EntityColumn("I01")]
        [XmlAttribute("propertyType")]
		public ControlPropertyValueType PropertyType
		{
			get
			{
				return _propertyType;
			}
			set
			{
				_propertyType = value;
			}
		}

		private bool _isBindOnly;
		[EntityColumn("B01")] 
        [XmlAttribute("bindOnly")]
		public bool IsBindOnly
		{
			get
			{
				return _isBindOnly;
			}
			set
			{
				_isBindOnly = value;
			}
		}

		private bool _isLocalizable;
		[EntityColumn("B02")] 
        [XmlAttribute("localizable")]
		public bool IsLocalizable
		{
			get
			{
				return _isLocalizable;
			}
			set
			{
				_isLocalizable = value;
			}
		}
		#endregion
		
		#region Overriden AbstractSchemaItem Members
		public override string Icon
		{
			get
			{
				return "3";
			}
		}

		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ControlPropertyItem.ItemTypeConst;
			}
		}

		public override SchemaItemCollection ChildItems
		{
			get
			{
				return new SchemaItemCollection();
			}
		}
		#endregion

	}
}
