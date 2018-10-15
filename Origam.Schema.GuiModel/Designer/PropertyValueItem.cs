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
using System.Xml;

namespace Origam.Schema.GuiModel
{
    /// <summary>
    /// Summary description for PropertyValueItem.
    /// </summary>
    /// 
    [XmlModelRoot(ItemTypeConst)]
    public class PropertyBindingInfo : AbstractPropertyValueItem
	{
		public const string ItemTypeConst = "PropertyBindingInfo";

		public PropertyBindingInfo() : base(){}
		
		public PropertyBindingInfo(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public PropertyBindingInfo(Key primaryKey) : base(primaryKey)	{}

        private string _value;

        [EntityColumn("LS01")]
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

		[EntityColumn("SS01")] 
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

		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return PropertyBindingInfo.ItemTypeConst;
			}
		}
	}

	[XmlModelRoot(ItemTypeConst)]
    public class PropertyValueItem : AbstractPropertyValueItem
	{

		public const string ItemTypeConst = "PropertyValueItem";

		public PropertyValueItem() : base(){}
		
		public PropertyValueItem(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public PropertyValueItem(Key primaryKey) : base(primaryKey)	{}

        string _xmlPersistedValue;

        [XmlAttribute("value")]
        [Browsable(false)]
        public string XmlPersistedValue
        {
            get
            {
                switch (ControlPropertyItem.PropertyType)
                {
                    case ControlPropertyValueType.Integer:
                        return XmlConvert.ToString(IntValue);
                    case ControlPropertyValueType.Boolean:
                        return XmlConvert.ToString(BoolValue);
                    case ControlPropertyValueType.Xml:
                    case ControlPropertyValueType.String:
                        return Value;
                    case ControlPropertyValueType.UniqueIdentifier:
                        return XmlConvert.ToString(GuidValue);
                    default:
                        throw new ArgumentOutOfRangeException("PropertyType");
                }
            }
            set
            {
                _xmlPersistedValue = value;
                UseXmlPersistedValue();
            }
        }

        public override void AfterControlPropertySet()
        {
            UseXmlPersistedValue();
        }

        private void UseXmlPersistedValue()
        {
            if (ControlPropertyItem != null && _xmlPersistedValue != null)
            {
                switch (ControlPropertyItem.PropertyType)
                {
                    case ControlPropertyValueType.Integer:
                        IntValue = XmlConvert.ToInt32(_xmlPersistedValue);
                        break;
                    case ControlPropertyValueType.Boolean:
                        BoolValue = XmlConvert.ToBoolean(_xmlPersistedValue);
                        break;
                    case ControlPropertyValueType.Xml:
                    case ControlPropertyValueType.String:
                        Value = _xmlPersistedValue;
                        break;
                    case ControlPropertyValueType.UniqueIdentifier:
                        GuidValue = XmlConvert.ToGuid(_xmlPersistedValue);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("PropertyType");
                }
            }
        }

		private int _intValue = 0;

		#region Properties
		[EntityColumn("I01")] 
		public int IntValue
		{
			get
			{
				return _intValue;
			}
			set
			{
				_intValue=value;
			}

		}

		private bool _boolValue = false;

		[EntityColumn("B01")]
        public bool BoolValue
		{
			get
			{
				return _boolValue;
			}
			set
			{
				_boolValue=value;
			}

		}

		private Guid _guidValue;

		[EntityColumn("G02")]
        public Guid GuidValue
		{
			get
			{
				return _guidValue;
			}
			set
			{
				_guidValue=value;
			}

		}

        private string _value;

        [EntityColumn("LS01")]
        [Localizable(true)]
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

        public void SetValue(object value)
		{
			if(value == null)
			{
				this.Value = null;
				this.GuidValue = Guid.Empty;
				this.IntValue = 0;
				this.BoolValue = false;
			}
			else if(value is Guid)
			{
				this.GuidValue = (Guid)value;
			}
			else if(value is int)
			{
				this.IntValue = (int)value;
			}
			else if(value is string)
			{
				this.Value = (string)value;
			}
			else if(value is bool)
			{
				this.BoolValue = (bool)value;
			}
			else
			{
				this.Value = value.ToString();
			}
		}

		#endregion

		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return PropertyValueItem.ItemTypeConst;
			}
		}
	}


    public abstract class AbstractPropertyValueItem  : AbstractSchemaItem, IQueryLocalizable
	{
		public AbstractPropertyValueItem() : base(){}
		
		public AbstractPropertyValueItem(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public AbstractPropertyValueItem(Key primaryKey) : base(primaryKey)	{}

        #region Properties

        private Guid _controlPropertyId;

		[EntityColumn("G01")]
        [XmlAttribute("propertyId")]
		public Guid ControlPropertyId
        {
            get
            {
                return _controlPropertyId;
            }
            set
            {
                _controlPropertyId = value;
                AfterControlPropertySet();
            }
        }

		private ControlPropertyItem _property = null;
        public ControlPropertyItem ControlPropertyItem
		{
			get
			{
				if(_property == null)
				{
					ModelElementKey key = new ModelElementKey();
					key.Id = this.ControlPropertyId;

					_property = (ControlPropertyItem)this.PersistenceProvider.RetrieveInstance(typeof(ControlPropertyItem), key);
				}

				return _property;
			}
			set
			{
				this.ControlPropertyId = (Guid)value.PrimaryKey["Id"];
				_property = value;
			}
		}

        public virtual void AfterControlPropertySet()
        {

        }
		#endregion
		
		#region Overriden AbstractSchemaItem Members
		public override string Icon
		{
			get
			{
				return "7";
			}
		}		
		
		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.ControlPropertyItem);

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

		#region IQueryLocalizable Members

		public bool IsLocalizable(string member)
		{
			if(member == "Value")
			{
				return this.ControlPropertyItem.IsLocalizable;
			}

			return true;
		}

		#endregion
	}
}
