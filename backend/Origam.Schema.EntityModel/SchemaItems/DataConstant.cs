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

namespace Origam.Schema.EntityModel;
/// <summary>
/// Summary description for DataConstant.
/// </summary>
[SchemaItemDescription("Data Constant", "icon_data-constant.png")]
[HelpTopic("Data+Constants")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class DataConstant : AbstractSchemaItem
{
	public const string CategoryConst = "DataConstant";
	public DataConstant() : base() {}
	public DataConstant(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public DataConstant(Key primaryKey) : base(primaryKey)	{}
    #region Properties
    [Category("(Schema Item)")]
    [RefreshProperties(RefreshProperties.Repaint)]
    [StringNotEmptyModelElementRule]
    [XmlAttribute("name")]
    [Description("Name of the model element. The name is mainly used for giving the model elements a human readable name. In some cases the name is an identificator of the model element (e.g. for defining XML structures or for requesting constants from XSLT tranformations).")]
    [NoDuplicateNamesInDataConstantRuleAtribute]
    public override string Name
    {
        get => base.Name;
        set => base.Name = value;
    }
    OrigamDataType _dataType = OrigamDataType.String;
    [RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
	[Category("Data Constant")]
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
	Guid _guidValue = Guid.Empty;
	//		[RefreshProperties(RefreshProperties.Repaint)]
    [Browsable(false)]
	public Guid GuidValue
	{
		get
		{
			return _guidValue;
		}
		set
		{
			_guidValue = value;
		}
	}
	string _stringValue = "";
	[Browsable(false)]
    public string StringValue
	{
		get
		{
			return _stringValue;
		}
		set
		{
			_stringValue = value;
		}
	}
	bool _booleanValue = false;
	[Browsable(false)]
    public bool BooleanValue
	{
		get
		{
			return _booleanValue;
		}
		set
		{
			_booleanValue = value;
		}
	}
	int _intValue = 0;
	[Browsable(false)]
    public int IntValue
	{
		get
		{
			return _intValue;
		}
		set
		{
			_intValue = value;
		}
	}
	decimal _currencyValue = 0;
	[Browsable(false)]
    public decimal CurrencyValue
	{
		get
		{
			return _currencyValue;
		}
		set
		{
			_currencyValue = value;
		}
	}
	decimal _floatValue = 0;
	[Browsable(false)]
    public decimal FloatValue
	{
		get
		{
			return _floatValue;
		}
		set
		{
			_floatValue = value;
		}
	}
	object _dateValue = null;
	[Browsable(false)]
    public object DateValue
	{
		get
		{
			return _dateValue;
		}
		set
		{
			if(value == null | value is DateTime)
			{
				_dateValue = value;
			}
			else
			{
				throw new ArgumentOutOfRangeException("DateValue", value, ResourceUtils.GetString("ErrorNotDateTime"));
			}
		}
	}
    public Guid DataLookupId;
	[Category("Data Constant")]
	[TypeConverter(typeof(DataLookupConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("lookup", "DataLookupId")]
    public IDataLookup DataLookup
	{
		get
		{
			ModelElementKey key = new ModelElementKey();
			key.Id = this.DataLookupId;
			return (IDataLookup)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
		}
		set
		{
			if(value == null)
			{
				this.DataLookupId = Guid.Empty;
			}
			else
			{
				this.DataLookupId = (Guid)value.PrimaryKey["Id"];
			}
		}
	}
    [XmlAttribute("value")]
    [Browsable(false)]
    public string XmlValue
    {
        get => XmlTools.ConvertToString(Value);
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                Value = null;
            }
            else
            {
                switch (DataType)
                {
                    case OrigamDataType.Boolean:
                        Value = XmlConvert.ToBoolean(value);
                        break;
                    case OrigamDataType.Float:
                    case OrigamDataType.Currency:
                        Value = XmlConvert.ToDecimal(value);
                        break;
                    case OrigamDataType.Date:
                        Value = XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.Unspecified);
                        break;
                    case OrigamDataType.Integer:
                        Value = XmlConvert.ToInt32(value);
                        break;
                    case OrigamDataType.Memo:
                    case OrigamDataType.Xml:
                    case OrigamDataType.String:
                        Value = value;
                        break;
                    case OrigamDataType.UniqueIdentifier:
                        Value = new Guid(value);
                        break;
                    default:
                        throw new NotSupportedException(DataType.ToString());
                }
            }
        }
    }
	[Category("Value")]
	[TypeConverter(typeof(DataConstantLookupReaderConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
    public object Value
	{
		get
		{
			return ConvertValue(this.DataType, this.StringValue, this.IntValue, this.GuidValue, this.CurrencyValue, this.FloatValue, this.BooleanValue, this.DateValue);
		}
		set
		{
			switch(this.DataType)
			{
				case OrigamDataType.Xml:
				case OrigamDataType.Memo:
				case OrigamDataType.String:
					string stringValue = Convert.ToString(value);
					this.StringValue = stringValue;
					break;
                case OrigamDataType.Integer:
					int intValue = Convert.ToInt32(value);
					this.IntValue = intValue;
					break;
				case OrigamDataType.Currency:
					decimal currencyValue = Convert.ToDecimal(value);
					this.CurrencyValue = currencyValue;
					break;
				case OrigamDataType.Float:
					decimal floatValue = Convert.ToDecimal(value);
					this.FloatValue = floatValue;
					break;
				case OrigamDataType.Boolean:
					bool booleanValue = Convert.ToBoolean(value);
					this.BooleanValue = booleanValue;
					break;
				case OrigamDataType.Date:
					if (string.IsNullOrEmpty(Convert.ToString(value)))
					{
						this.DateValue = null;
					}
					else
					{
						DateTime dateValue = Convert.ToDateTime(value);
						this.DateValue = dateValue;
					}
					break;
				case OrigamDataType.UniqueIdentifier:
					Guid guidValue;
					if(value == null)
					{
						guidValue = Guid.Empty;
					}
					else
					{	
						switch(value.GetType().ToString())
						{
							case "System.String":
								guidValue = new Guid((string)value);
								break;
							case "System.Guid":
								guidValue = (Guid)value;
								break;
							default:
								throw new ArgumentOutOfRangeException("value", value, ResourceUtils.GetString("ErrorConvertToGuid"));
						}
					}
					this.GuidValue = guidValue;
					break;
			}
//
//				switch(value.GetType().ToString())
//				{
//					case "System.String":
//						this.StringValue = (string)value;
//						break;
//					case "System.Guid":
//						this.GuidValue = (Guid)value;
//						break;
//					case "System.Int32":
//						this.IntValue = (int)value;
//						break;
//				}
		}
	}
	public static object ConvertValue(OrigamDataType dataType, string stringValue, int intValue, Guid guidValue, decimal currencyValue, decimal floatValue, bool booleanValue, object dateValue)
	{
		switch(dataType)
		{
			case OrigamDataType.Xml:
			case OrigamDataType.Memo:
			case OrigamDataType.String:
				return stringValue;
			case OrigamDataType.Integer:
				return intValue;
			case OrigamDataType.UniqueIdentifier:
				return guidValue;
			case OrigamDataType.Currency:
				return currencyValue;
		
			case OrigamDataType.Float:
				return floatValue;
			case OrigamDataType.Boolean:
				return booleanValue;
			case OrigamDataType.Date:
				return dateValue;
		}
		return null;
	}
    bool _isUserDefinable = false;
    [Category("User Definition")]
    [Description("When True and when the constant is included as a menu item a different value will be stored for each user.")]
    [DefaultValue(false)]
    [XmlAttribute("userDefinable")]
    public bool IsUserDefinable
    {
        get
        {
            return _isUserDefinable;
        }
        set
        {
            _isUserDefinable = value;
        }
    }
    
    public Guid UserDefinableDefaultConstantId;
    [TypeConverter(typeof(DataConstantConverter))]
    [Category("User Definition")]
    [Description("A value that will be used by default when IsUserDefinable = true and the user did not save a value yet.")]
    [XmlReference("userDefinableDefaultConstant", "UserDefinableDefaultConstantId")]
    public DataConstant UserDefinableDefaultConstant
    {
        get
        {
            return (DataConstant)this.PersistenceProvider.RetrieveInstance(typeof(EntityFilter), new ModelElementKey(this.UserDefinableDefaultConstantId));
        }
        set
        {
            this.UserDefinableDefaultConstantId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
        }
    }
    #endregion
	#region Overriden AbstractSchemaItem Members
	
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		if(this.DataLookup != null) dependencies.Add(this.DataLookup);
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
}
