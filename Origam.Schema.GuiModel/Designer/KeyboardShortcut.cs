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

using System;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for Graphics.
	/// </summary>
	[SchemaItemDescription("Keyboard Shortcut", 81)]
    [HelpTopic("Keyboard+Shortcuts")]
	[XmlModelRoot(ItemTypeConst)]
	public class KeyboardShortcut : AbstractSchemaItem
	{
		public const string ItemTypeConst = "KeyboardShortcut";

		public KeyboardShortcut() : base(){}

		public KeyboardShortcut(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public KeyboardShortcut(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractSchemaItem Members
		
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
				return "81";
			}
		}
		#endregion

		#region Properties
		private bool _isShift = false;
		[DefaultValue(false)]
		[EntityColumn("B01")]
        [XmlAttribute("shift")]
		public bool IsShift
		{
			get
			{
				return _isShift;
			}
			set
			{
				_isShift = value;
			}
		}

		private bool _isControl = false;
		[DefaultValue(false)]
		[EntityColumn("B02")]
        [XmlAttribute("control")]
		public bool IsControl
		{
			get
			{
				return _isControl;
			}
			set
			{
				_isControl = value;
			}
		}

		private bool _isAlt = false;
		[DefaultValue(false)]
		[EntityColumn("B03")]
        [XmlAttribute("alt")]
		public bool IsAlt
		{
			get
			{
				return _isAlt;
			}
			set
			{
				_isAlt = value;
			}
		}

		private int _keyCode = 0;
		[DefaultValue(false)]
		[EntityColumn("I01")]
        [XmlAttribute("keyCode")]
		public int KeyCode
		{
			get
			{
				return _keyCode;
			}
			set
			{
				_keyCode = value;
			}
		}
		#endregion
	}
}
