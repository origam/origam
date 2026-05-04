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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Summary description for DataConstant.
/// </summary>
[SchemaItemDescription(name: "Data Constant", iconName: "icon_data-constant.png")]
[HelpTopic(topic: "Data+Constants")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class DataConstant : AbstractSchemaItem
{
    public const string CategoryConst = "DataConstant";

    public DataConstant()
        : base() { }

    public DataConstant(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public DataConstant(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    [Category(category: "(Schema Item)")]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [StringNotEmptyModelElementRule]
    [XmlAttribute(attributeName: "name")]
    [Description(
        description: "Name of the model element. The name is mainly used for giving the model elements a human readable name. In some cases the name is an identificator of the model element (e.g. for defining XML structures or for requesting constants from XSLT tranformations)."
    )]
    [NoDuplicateNamesInDataConstantRuleAtribute]
    public override string Name
    {
        get => base.Name;
        set => base.Name = value;
    }
    OrigamDataType _dataType = OrigamDataType.String;

    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [NotNullModelElementRule()]
    [Category(category: "Data Constant")]
    [XmlAttribute(attributeName: "dataType")]
    public OrigamDataType DataType
    {
        get { return _dataType; }
        set { _dataType = value; }
    }
    Guid _guidValue = Guid.Empty;

    //		[RefreshProperties(RefreshProperties.Repaint)]
    [Browsable(browsable: false)]
    public Guid GuidValue
    {
        get { return _guidValue; }
        set { _guidValue = value; }
    }
    string _stringValue = "";

    [Browsable(browsable: false)]
    public string StringValue
    {
        get { return _stringValue; }
        set { _stringValue = value; }
    }
    bool _booleanValue = false;

    [Browsable(browsable: false)]
    public bool BooleanValue
    {
        get { return _booleanValue; }
        set { _booleanValue = value; }
    }
    int _intValue = 0;

    [Browsable(browsable: false)]
    public int IntValue
    {
        get { return _intValue; }
        set { _intValue = value; }
    }
    decimal _currencyValue = 0;

    [Browsable(browsable: false)]
    public decimal CurrencyValue
    {
        get { return _currencyValue; }
        set { _currencyValue = value; }
    }
    decimal _floatValue = 0;

    [Browsable(browsable: false)]
    public decimal FloatValue
    {
        get { return _floatValue; }
        set { _floatValue = value; }
    }
    object _dateValue = null;

    [Browsable(browsable: false)]
    public object DateValue
    {
        get { return _dateValue; }
        set
        {
            if (value == null | value is DateTime)
            {
                _dateValue = value;
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "DateValue",
                    actualValue: value,
                    message: ResourceUtils.GetString(key: "ErrorNotDateTime")
                );
            }
        }
    }
    public Guid DataLookupId;

    [Category(category: "Data Constant")]
    [TypeConverter(type: typeof(DataLookupConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "lookup", idField: "DataLookupId")]
    public IDataLookup DataLookup
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.DataLookupId;
            return (IDataLookup)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                this.DataLookupId = Guid.Empty;
            }
            else
            {
                this.DataLookupId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }

    [XmlAttribute(attributeName: "value")]
    [Browsable(browsable: false)]
    public string XmlValue
    {
        get => XmlTools.ConvertToString(val: Value);
        set
        {
            if (string.IsNullOrEmpty(value: value))
            {
                Value = null;
            }
            else
            {
                switch (DataType)
                {
                    case OrigamDataType.Boolean:
                    {
                        Value = XmlConvert.ToBoolean(s: value);
                        break;
                    }

                    case OrigamDataType.Float:
                    case OrigamDataType.Currency:
                    {
                        Value = XmlConvert.ToDecimal(s: value);
                        break;
                    }

                    case OrigamDataType.Date:
                    {
                        Value = XmlConvert.ToDateTime(
                            s: value,
                            dateTimeOption: XmlDateTimeSerializationMode.Unspecified
                        );
                        break;
                    }

                    case OrigamDataType.Integer:
                    {
                        Value = XmlConvert.ToInt32(s: value);
                        break;
                    }

                    case OrigamDataType.Memo:
                    case OrigamDataType.Xml:
                    case OrigamDataType.String:
                    {
                        Value = value;
                        break;
                    }

                    case OrigamDataType.UniqueIdentifier:
                    {
                        Value = new Guid(g: value);
                        break;
                    }

                    default:
                    {
                        throw new NotSupportedException(message: DataType.ToString());
                    }
                }
            }
        }
    }

    [Category(category: "Value")]
    [TypeConverter(type: typeof(DataConstantLookupReaderConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    public object Value
    {
        get
        {
            return ConvertValue(
                dataType: this.DataType,
                stringValue: this.StringValue,
                intValue: this.IntValue,
                guidValue: this.GuidValue,
                currencyValue: this.CurrencyValue,
                floatValue: this.FloatValue,
                booleanValue: this.BooleanValue,
                dateValue: this.DateValue
            );
        }
        set
        {
            switch (this.DataType)
            {
                case OrigamDataType.Xml:
                case OrigamDataType.Memo:
                case OrigamDataType.String:
                {
                    string stringValue = Convert.ToString(value: value);
                    this.StringValue = stringValue;
                    break;
                }

                case OrigamDataType.Integer:
                {
                    int intValue = Convert.ToInt32(value: value);
                    this.IntValue = intValue;
                    break;
                }

                case OrigamDataType.Currency:
                {
                    decimal currencyValue = Convert.ToDecimal(value: value);
                    this.CurrencyValue = currencyValue;
                    break;
                }

                case OrigamDataType.Float:
                {
                    decimal floatValue = Convert.ToDecimal(value: value);
                    this.FloatValue = floatValue;
                    break;
                }

                case OrigamDataType.Boolean:
                {
                    bool booleanValue = Convert.ToBoolean(value: value);
                    this.BooleanValue = booleanValue;
                    break;
                }

                case OrigamDataType.Date:
                {
                    if (string.IsNullOrEmpty(value: Convert.ToString(value: value)))
                    {
                        this.DateValue = null;
                    }
                    else
                    {
                        DateTime dateValue = Convert.ToDateTime(value: value);
                        this.DateValue = dateValue;
                    }
                    break;
                }

                case OrigamDataType.UniqueIdentifier:
                {
                    Guid guidValue;
                    if (value == null)
                    {
                        guidValue = Guid.Empty;
                    }
                    else
                    {
                        switch (value.GetType().ToString())
                        {
                            case "System.String":
                            {
                                guidValue = new Guid(g: (string)value);
                                break;
                            }
                            case "System.Guid":
                            {
                                guidValue = (Guid)value;
                                break;
                            }
                            default:
                            {
                                throw new ArgumentOutOfRangeException(
                                    paramName: "value",
                                    actualValue: value,
                                    message: ResourceUtils.GetString(key: "ErrorConvertToGuid")
                                );
                            }
                        }
                    }
                    this.GuidValue = guidValue;
                    break;
                }
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

    public static object ConvertValue(
        OrigamDataType dataType,
        string stringValue,
        int intValue,
        Guid guidValue,
        decimal currencyValue,
        decimal floatValue,
        bool booleanValue,
        object dateValue
    )
    {
        switch (dataType)
        {
            case OrigamDataType.Xml:
            case OrigamDataType.Memo:
            case OrigamDataType.String:
            {
                return stringValue;
            }
            case OrigamDataType.Integer:
            {
                return intValue;
            }
            case OrigamDataType.UniqueIdentifier:
            {
                return guidValue;
            }
            case OrigamDataType.Currency:
            {
                return currencyValue;
            }

            case OrigamDataType.Float:
            {
                return floatValue;
            }
            case OrigamDataType.Boolean:
            {
                return booleanValue;
            }
            case OrigamDataType.Date:
            {
                return dateValue;
            }
        }
        return null;
    }

    bool _isUserDefinable = false;

    [Category(category: "User Definition")]
    [Description(
        description: "When True and when the constant is included as a menu item a different value will be stored for each user."
    )]
    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "userDefinable")]
    public bool IsUserDefinable
    {
        get { return _isUserDefinable; }
        set { _isUserDefinable = value; }
    }

    public Guid UserDefinableDefaultConstantId;

    [TypeConverter(type: typeof(DataConstantConverter))]
    [Category(category: "User Definition")]
    [Description(
        description: "A value that will be used by default when IsUserDefinable = true and the user did not save a value yet."
    )]
    [XmlReference(
        attributeName: "userDefinableDefaultConstant",
        idField: "UserDefinableDefaultConstantId"
    )]
    public DataConstant UserDefinableDefaultConstant
    {
        get
        {
            return (DataConstant)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(EntityFilter),
                    primaryKey: new ModelElementKey(id: this.UserDefinableDefaultConstantId)
                );
        }
        set
        {
            this.UserDefinableDefaultConstantId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    #endregion
    #region Overriden ISchemaItem Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (this.DataLookup != null)
        {
            dependencies.Add(item: this.DataLookup);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override ISchemaItemCollection ChildItems
    {
        get { return SchemaItemCollection.Create(); }
    }
    #endregion
}
