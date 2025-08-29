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
using System.Xml.Serialization;
using Origam.DA;
using Origam.DA.Common;
using Origam.DA.EntityModel;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.EntityModel;

/// <summary>
/// Abstract implementation of IDataEntityColumn.
/// </summary>
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public abstract class AbstractDataEntityColumn : AbstractSchemaItem, IDataEntityColumn
{
    public const string CategoryConst = "DataEntityColumn";

    public AbstractDataEntityColumn()
        : base()
    {
        Init();
    }

    public AbstractDataEntityColumn(Guid schemaExtensionId)
        : base(schemaExtensionId)
    {
        Init();
    }

    public AbstractDataEntityColumn(Key primaryKey)
        : base(primaryKey)
    {
        Init();
    }

    private void Init()
    {
        ChildItemTypes.InsertRange(
            0,
            new Type[]
            {
                typeof(EntityFieldSecurityRule),
                typeof(EntityFieldDependency),
                typeof(EntityFieldDynamicLabel),
            }
        );
    }

    #region Public Methods

    public static void CopyFieldMembers(
        AbstractDataEntityColumn source,
        AbstractDataEntityColumn destination
    )
    {
        destination.AllowNulls = source.AllowNulls;
        destination.AutoIncrement = source.AutoIncrement;
        destination.AutoIncrementSeed = source.AutoIncrementSeed;
        destination.AutoIncrementStep = source.AutoIncrementStep;
        destination.Caption = source.Caption;
        destination.DataLength = source.DataLength;
        destination.DataType = source.DataType;
        destination.DefaultLookup = source.DefaultLookup;
        destination.DefaultValueParameter = source.DefaultValueParameter;
        destination.DefaultValue = source.DefaultValue;
        destination.ForeignKeyEntity = source.ForeignKeyEntity;
        destination.ForeignKeyField = source.ForeignKeyField;
        destination.IsPrimaryKey = source.IsPrimaryKey;
        destination.OnCopyAction = source.OnCopyAction;
        destination.XmlMappingType = source.XmlMappingType;
    }
    #endregion
    #region IDataEntityColumn Members

    public abstract bool ReadOnly { get; }
    private OrigamDataType _dataType;

    [Category("Entity Column")]
    [Description("Data type of this field")]
    [XmlAttribute("dataType")]
    public OrigamDataType DataType
    {
        get { return _dataType; }
        set
        {
            _dataType = value;
            OnPropertyChanged("DataType");
        }
    }
    private int _dataLength = 0;

    [Category("Entity Column"), DefaultValue(0)]
    [Description(
        "Length of this field. Used only for String data type. It is used for specifying a length of a database field (in case when the field is database mapped) and a maximum length in the user interface (in case of TextBox)."
    )]
    [PositiveValueModelElementRuleAttribute()]
    [NoLengthLimitOnMemoFieldRule]
    [XmlAttribute("dataLength")]
    public int DataLength
    {
        get { return _dataLength; }
        set { _dataLength = value; }
    }
    private bool _allowNulls = true;

    [Category("Entity Column"), DefaultValue(true)]
    [Description(
        "Indicates if the field allows empty values or not. If set to False, also the database column will be generated so that it does not allow nulls. In the user interface the user will have to enter a value before saving the record."
    )]
    [XmlAttribute("allowNulls")]
    public bool AllowNulls
    {
        get { return _allowNulls; }
        set { _allowNulls = value; }
    }
    private bool _isPrimaryKey = false;

    [Category("Entity Column"), DefaultValue(false)]
    [Description(
        "Indicates if the field is a primary key. If set to True, also a database primary key is generated. IMPORTANT: Every entity should have a primary key specified, otherwise data merges will not be able to correlate existing records. NOTE: Multi-column primary keys are possible but GUI expects always only single-column primary keys."
    )]
    [XmlAttribute("isPrimaryKey")]
    public bool IsPrimaryKey
    {
        get { return _isPrimaryKey; }
        set { _isPrimaryKey = value; }
    }
    private string _caption = "";

    [Category("Entity Column")]
    [Localizable(true)]
    [Description(
        "Default label for the field in a GUI. Audit log viewer also gets the field names from here."
    )]
    [XmlAttribute("label")]
    public string Caption
    {
        get { return _caption; }
        set { _caption = value; }
    }
    private bool _excludeFromAllFields = false;

    [Category("Entity Column"), DefaultValue(false)]
    [Description(
        "If set to True, the field will not be included in the list of fields in a Data Structure if 'AllFields=True' is set in a Data Structure Entity. This is useful e.g. for database function calls that are expensive and used only for lookups that would otherwise slow down the system if loaded e.g. to forms."
    )]
    [XmlAttribute("excludeFromAllFields")]
    public bool ExcludeFromAllFields
    {
        get { return _excludeFromAllFields; }
        set { _excludeFromAllFields = value; }
    }

    [Browsable(false)]
    public virtual bool ExcludeFromAuditing
    {
        get { return true; }
        set { throw new NotImplementedException(); }
    }
    private bool _autoIncrement = false;

    [Category("Entity Column"), DefaultValue(false)]
    [Description(
        "If set to True, the new record gets the next highest value in a data context. DataType has to be numeric."
    )]
    [XmlAttribute("autoIncrement")]
    public bool AutoIncrement
    {
        get { return _autoIncrement; }
        set { _autoIncrement = value; }
    }
    private long _autoIncrementSeed = 0;

    [Category("Entity Column"), DefaultValue((long)0)]
    [Browsable(false)]
    public long AutoIncrementSeed
    {
        get { return _autoIncrementSeed; }
        set { _autoIncrementSeed = value; }
    }
    private long _autoIncrementStep = 1;

    [Category("Entity Column"), DefaultValue((long)1)]
    [Browsable(false)]
    public long AutoIncrementStep
    {
        get { return _autoIncrementStep; }
        set
        {
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(
                    "AutoIncrementStep",
                    value,
                    ResourceUtils.GetString("ErrorAutoIncrement")
                );
            }
            _autoIncrementStep = value;
        }
    }
    public Guid DefaultLookupId;

    [TypeConverter(typeof(DataLookupConverter))]
    [Category("Entity Column")]
    [Description(
        "Lookup that will be used as default for creating GUI (putting a Drop Down Box on a form). It will be also used by an Audit Trail for converting an ID to a text value for the user. If not set, audit log will display ID's only."
    )]
    [XmlReference("defaultLookup", "DefaultLookupId")]
    public IDataLookup DefaultLookup
    {
        get
        {
            return (IDataLookup)
                this.PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(this.DefaultLookupId)
                );
        }
        set { this.DefaultLookupId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]); }
    }
    public Guid ForeignEntityId;

    [TypeConverter(typeof(EntityConverter))]
    [Category("Foreign Key")]
    [RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("foreignKeyEntity", "ForeignEntityId")]
    public IDataEntity ForeignKeyEntity
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.ForeignEntityId;
            try
            {
                return (IDataEntity)
                    this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), key);
            }
            catch
            {
                throw new Exception(ResourceUtils.GetString("ErrorForeignEntityNotFound"));
            }
        }
        set
        {
            this.ForeignKeyField = null;
            if (value == null)
            {
                this.ForeignEntityId = Guid.Empty;
            }
            else
            {
                this.ForeignEntityId = (Guid)value.PrimaryKey["Id"];
            }
        }
    }
    public Guid ForeignEntityColumnId;

    [TypeConverter(typeof(EntityForeignColumnConverter))]
    [NotNullModelElementRule("ForeignKeyEntity")]
    [Category("Foreign Key")]
    [XmlReference("foreignKeyField", "ForeignEntityColumnId")]
    public IDataEntityColumn ForeignKeyField
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.ForeignEntityColumnId;
            try
            {
                return (IDataEntityColumn)
                    this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), key);
            }
            catch
            {
                throw new Exception(ResourceUtils.GetString("ErrorForeignEntityNotFound"));
            }
        }
        set
        {
            if (value == null)
            {
                this.ForeignEntityColumnId = Guid.Empty;
            }
            else
            {
                this.ForeignEntityColumnId = (Guid)value.PrimaryKey["Id"];
            }
        }
    }
    public Guid DefaultValueId;

    [TypeConverter(typeof(DataConstantConverter))]
    [Category("Entity Column")]
    [XmlReference("defaultValue", "DefaultValueId")]
    public DataConstant DefaultValue
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.DefaultValueId;
            return (DataConstant)
                this.PersistenceProvider.RetrieveInstance(typeof(DataConstant), key);
        }
        set
        {
            if (value == null)
            {
                this.DefaultValueId = Guid.Empty;
            }
            else
            {
                this.DefaultValueId = (Guid)value.PrimaryKey["Id"];
            }
        }
    }
    public Guid defaultValueParameterId;

    [Category("Entity Column")]
    [TypeConverter(typeof(ParameterReferenceConverter))]
    [Description(
        "Choose a parameter which is used to fill this mapped column when no value is provided (by default). Takes a priority over 'DefaultValue' property."
    )]
    [XmlReference("defaultValueParameter", "defaultValueParameterId")]
    public SchemaItemParameter DefaultValueParameter
    {
        get
        {
            return (SchemaItemParameter)
                    this.PersistenceProvider.RetrieveInstance(
                        typeof(SchemaItemParameter),
                        new ModelElementKey(this.defaultValueParameterId)
                    ) as SchemaItemParameter;
        }
        set
        {
            this.defaultValueParameterId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]
            );
        }
    }
    private EntityColumnXmlMapping _xmlMappingType = EntityColumnXmlMapping.Attribute;

    [Category("Entity Column"), DefaultValue(EntityColumnXmlMapping.Attribute)]
    [XmlAttribute("xmlMappingType")]
    public EntityColumnXmlMapping XmlMappingType
    {
        get { return _xmlMappingType; }
        set { _xmlMappingType = value; }
    }
    private OnCopyActionType _onCopyAction = OnCopyActionType.Copy;

    [Category("Entity Column"), DefaultValue(OnCopyActionType.Copy)]
    [XmlAttribute("onCopyAction")]
    public OnCopyActionType OnCopyAction
    {
        get { return _onCopyAction; }
        set { _onCopyAction = value; }
    }

    [Browsable(false)]
    public List<AbstractEntitySecurityRule> RowLevelSecurityRules =>
        ChildItemsByType<AbstractEntitySecurityRule>(AbstractEntitySecurityRule.CategoryConst);

    [Browsable(false)]
    public List<EntityConditionalFormatting> ConditionalFormattingRules =>
        ChildItemsByType<EntityConditionalFormatting>(EntityConditionalFormatting.CategoryConst);

    [Browsable(false)]
    public List<EntityFieldDynamicLabel> DynamicLabels =>
        ChildItemsByType<EntityFieldDynamicLabel>(EntityFieldDynamicLabel.CategoryConst);

    [Browsable(false)]
    public DataEntityConstraint ForeignKeyConstraint
    {
        get
        {
            if (ForeignKeyEntity != null && ForeignKeyField != null)
            {
                DataEntityConstraint result = new DataEntityConstraint(ConstraintType.ForeignKey);
                result.ForeignEntity = ForeignKeyEntity as IDataEntity;
                result.Fields.Add(this);
                return result;
            }

            return null;
        }
    }
    public abstract string FieldType { get; }
    #endregion
    #region Overriden ISchemaItem Methods
    public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
    {
        return newNode is IDataEntity;
    }

    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (this.ForeignKeyEntity != null)
        {
            dependencies.Add(this.ForeignKeyEntity);
        }

        if (this.ForeignKeyField != null)
        {
            dependencies.Add(this.ForeignKeyField);
        }

        if (this.DefaultLookup != null)
        {
            dependencies.Add(this.DefaultLookup);
        }

        if (this.DefaultValue != null)
        {
            dependencies.Add(this.DefaultValue);
        }

        if (this.DefaultValueParameter != null)
        {
            dependencies.Add(this.DefaultValueParameter);
        }

        base.GetExtraDependencies(dependencies);
    }

    public override string FontStyle
    {
        get
        {
            if (!AllowNulls)
            {
                return "Bold";
            }
            return base.FontStyle;
        }
    }
    #endregion
}
