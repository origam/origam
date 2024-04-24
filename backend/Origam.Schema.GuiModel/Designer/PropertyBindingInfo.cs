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
using System.Xml;

namespace Origam.Schema.GuiModel
{
    /// <summary>
    /// Summary description for PropertyValueItem.
    /// </summary>
    /// 
    [XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
    public class PropertyBindingInfo : AbstractPropertyValueItem
	{
		public const string CategoryConst = "PropertyBindingInfo";

		public PropertyBindingInfo() : base(){}
		
		public PropertyBindingInfo(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public PropertyBindingInfo(Key primaryKey) : base(primaryKey)	{}

        private string _value;

        [Localizable(true)]
        [XmlAttribute("value")]
        public string Value
        {
            get
            {
                if (_value == null) return null;

                return _value.Trim();
            }
            set
            {
                _value = value;
            }

        }

        private string _designDataSetPath;

        [XmlAttribute("designDataSetPath")]
		public string DesignDataSetPath
		{
			get
			{
				return _designDataSetPath;
			}
			set
			{
				_designDataSetPath=value;
			}

		}

		public override string ItemType
		{
			get
			{
				return PropertyBindingInfo.CategoryConst;
			}
		}
	}
}
