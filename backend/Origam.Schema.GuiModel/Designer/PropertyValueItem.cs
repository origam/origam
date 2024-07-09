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
using System.Collections.Generic;
using System.ComponentModel;

using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;
using System.Xml;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.GuiModel;
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class PropertyValueItem : AbstractPropertyValueItem
{
	public const string CategoryConst = "PropertyValueItem";
	public PropertyValueItem() : base(){}
	
	public PropertyValueItem(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public PropertyValueItem(Key primaryKey) : base(primaryKey)	{}
    string _xmlPersistedValue;
    [XmlAttribute("value")]
    [Localizable(true)]
    [Browsable(false)]
    public string Value
    {
        get
        {
            if (ControlPropertyItem == null)
            {
	            return null;
            }
            switch (ControlPropertyItem.PropertyType)
            {
                case ControlPropertyValueType.Integer:
                    return XmlConvert.ToString(IntValue);
                case ControlPropertyValueType.Boolean:
                    return XmlConvert.ToString(BoolValue);
                case ControlPropertyValueType.Xml:
                case ControlPropertyValueType.String:
                    return StringValue;
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
        // while building schema from XML, we don't know in what order
        // the parsed xml attributes are comming, so it's possible,
        // that attribute property type comes after the actual value
        // (from the xml element 'value').
        // "propertyId" is a property type (e.g.left, right, text)
        // and it also defines the data type of property.
        // Therefore, we have to run a fixture code when ControlProperty
        // item comes.
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
                    StringValue = _xmlPersistedValue;
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
    private string _stringValue;
    public string StringValue
    {
        get
        {
            if (_stringValue == null) return null;
            return _stringValue.Trim();
        }
        set
        {
            _stringValue = value;
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
	public override string ItemType
	{
		get
		{
			return PropertyValueItem.CategoryConst;
		}
	}
}
[ClassMetaVersion("6.0.0")]
public abstract class AbstractPropertyValueItem  : AbstractSchemaItem, IQueryLocalizable
{
	public AbstractPropertyValueItem() : base(){}
	
	public AbstractPropertyValueItem(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public AbstractPropertyValueItem(Key primaryKey) : base(primaryKey)	{}
    #region Properties
    private Guid _controlPropertyId;
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
				_property = (ControlPropertyItem)this.PersistenceProvider.RetrieveInstance(typeof(ControlPropertyItem), key, true, false);
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
	
	#region Overriden ISchemaItem Members
	public override string Icon
	{
		get
		{
			return "7";
		}
	}		
	
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		dependencies.Add(this.ControlPropertyItem);
		base.GetExtraDependencies (dependencies);
	}
	
	public override ISchemaItemCollection ChildItems
	{
		get
		{
			return SchemaItemCollection.Create();
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
