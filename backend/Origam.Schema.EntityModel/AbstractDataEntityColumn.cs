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
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public abstract class AbstractDataEntityColumn : AbstractSchemaItem, IDataEntityColumn
{
    public const string CategoryConst = "DataEntityColumn";

    public AbstractDataEntityColumn()
        : base()
    {
        Init();
    }

    public AbstractDataEntityColumn(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public AbstractDataEntityColumn(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init()
    {
        ChildItemTypes.InsertRange(
            index: 0,
            collection: new Type[]
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

    [Category(category: "Entity Column")]
    [Description(description: "Data type of this field")]
    [XmlAttribute(attributeName: "dataType")]
    public OrigamDataType DataType
    {
        get { return _dataType; }
        set
        {
            _dataType = value;
            OnPropertyChanged(propertyName: "DataType");
        }
    }
    private int _dataLength = 0;

    [Category(category: "Entity Column"), DefaultValue(value: 0)]
    [Description(
        description: "Length of this field. Used only for String data type. It is used for specifying a length of a database field (in case when the field is database mapped) and a maximum length in the user interface (in case of TextBox)."
    )]
    [PositiveValueModelElementRuleAttribute()]
    [NoLengthLimitOnMemoFieldRule]
    [XmlAttribute(attributeName: "dataLength")]
    public int DataLength
    {
        get { return _dataLength; }
        set { _dataLength = value; }
    }
    private bool _allowNulls = true;

    [Category(category: "Entity Column"), DefaultValue(value: true)]
    [Description(
        description: "Indicates if the field allows empty values or not. If set to False, also the database column will be generated so that it does not allow nulls. In the user interface the user will have to enter a value before saving the record."
    )]
    [XmlAttribute(attributeName: "allowNulls")]
    public bool AllowNulls
    {
        get { return _allowNulls; }
        set { _allowNulls = value; }
    }
    private bool _isPrimaryKey = false;

    [Category(category: "Entity Column"), DefaultValue(value: false)]
    [Description(
        description: "Indicates if the field is a primary key. If set to True, also a database primary key is generated. IMPORTANT: Every entity should have a primary key specified, otherwise data merges will not be able to correlate existing records. NOTE: Multi-column primary keys are possible but GUI expects always only single-column primary keys."
    )]
    [XmlAttribute(attributeName: "isPrimaryKey")]
    public bool IsPrimaryKey
    {
        get { return _isPrimaryKey; }
        set { _isPrimaryKey = value; }
    }
    private string _caption = "";

    [Category(category: "Entity Column")]
    [Localizable(isLocalizable: true)]
    [Description(
        description: "Default label for the field in a GUI. Audit log viewer also gets the field names from here."
    )]
    [XmlAttribute(attributeName: "label")]
    public string Caption
    {
        get { return _caption; }
        set { _caption = value; }
    }
    private bool _excludeFromAllFields = false;

    [Category(category: "Entity Column"), DefaultValue(value: false)]
    [Description(
        description: "If set to True, the field will not be included in the list of fields in a Data Structure if 'AllFields=True' is set in a Data Structure Entity. This is useful e.g. for database function calls that are expensive and used only for lookups that would otherwise slow down the system if loaded e.g. to forms."
    )]
    [XmlAttribute(attributeName: "excludeFromAllFields")]
    public bool ExcludeFromAllFields
    {
        get { return _excludeFromAllFields; }
        set { _excludeFromAllFields = value; }
    }

    [Browsable(browsable: false)]
    public virtual bool ExcludeFromAuditing
    {
        get { return true; }
        set { throw new NotImplementedException(); }
    }
    private bool _autoIncrement = false;

    [Category(category: "Entity Column"), DefaultValue(value: false)]
    [Description(
        description: "If set to True, the new record gets the next highest value in a data context. DataType has to be numeric."
    )]
    [XmlAttribute(attributeName: "autoIncrement")]
    public bool AutoIncrement
    {
        get { return _autoIncrement; }
        set { _autoIncrement = value; }
    }
    private long _autoIncrementSeed = 0;

    [Category(category: "Entity Column"), DefaultValue(value: (long)0)]
    [Browsable(browsable: false)]
    public long AutoIncrementSeed
    {
        get { return _autoIncrementSeed; }
        set { _autoIncrementSeed = value; }
    }
    private long _autoIncrementStep = 1;

    [Category(category: "Entity Column"), DefaultValue(value: (long)1)]
    [Browsable(browsable: false)]
    public long AutoIncrementStep
    {
        get { return _autoIncrementStep; }
        set
        {
            if (value == 0)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "AutoIncrementStep",
                    actualValue: value,
                    message: ResourceUtils.GetString(key: "ErrorAutoIncrement")
                );
            }
            _autoIncrementStep = value;
        }
    }
    public Guid DefaultLookupId;

    [TypeConverter(type: typeof(DataLookupConverter))]
    [Category(category: "Entity Column")]
    [Description(
        description: "Lookup that will be used as default for creating GUI (putting a Drop Down Box on a form). It will be also used by an Audit Trail for converting an ID to a text value for the user. If not set, audit log will display ID's only."
    )]
    [XmlReference(attributeName: "defaultLookup", idField: "DefaultLookupId")]
    public IDataLookup DefaultLookup
    {
        get
        {
            return (IDataLookup)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.DefaultLookupId)
                );
        }
        set
        {
            this.DefaultLookupId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]);
        }
    }
    public Guid ForeignEntityId;

    [TypeConverter(type: typeof(EntityConverter))]
    [Category(category: "Foreign Key")]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "foreignKeyEntity", idField: "ForeignEntityId")]
    public IDataEntity ForeignKeyEntity
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.ForeignEntityId;
            try
            {
                return (IDataEntity)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: key
                    );
            }
            catch
            {
                throw new Exception(
                    message: ResourceUtils.GetString(key: "ErrorForeignEntityNotFound")
                );
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
                this.ForeignEntityId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }
    public Guid ForeignEntityColumnId;

    [TypeConverter(type: typeof(EntityForeignColumnConverter))]
    [NotNullModelElementRule(conditionField: "ForeignKeyEntity")]
    [Category(category: "Foreign Key")]
    [XmlReference(attributeName: "foreignKeyField", idField: "ForeignEntityColumnId")]
    public IDataEntityColumn ForeignKeyField
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.ForeignEntityColumnId;
            try
            {
                return (IDataEntityColumn)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(ISchemaItem),
                        primaryKey: key
                    );
            }
            catch
            {
                throw new Exception(
                    message: ResourceUtils.GetString(key: "ErrorForeignEntityNotFound")
                );
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
                this.ForeignEntityColumnId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }
    public Guid DefaultValueId;

    [TypeConverter(type: typeof(DataConstantConverter))]
    [Category(category: "Entity Column")]
    [XmlReference(attributeName: "defaultValue", idField: "DefaultValueId")]
    public DataConstant DefaultValue
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.DefaultValueId;
            return (DataConstant)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(DataConstant),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                this.DefaultValueId = Guid.Empty;
            }
            else
            {
                this.DefaultValueId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }
    public Guid defaultValueParameterId;

    [Category(category: "Entity Column")]
    [TypeConverter(type: typeof(ParameterReferenceConverter))]
    [Description(
        description: "Choose a parameter which is used to fill this mapped column when no value is provided (by default). Takes a priority over 'DefaultValue' property."
    )]
    [XmlReference(attributeName: "defaultValueParameter", idField: "defaultValueParameterId")]
    public SchemaItemParameter DefaultValueParameter
    {
        get
        {
            return (SchemaItemParameter)
                    this.PersistenceProvider.RetrieveInstance(
                        type: typeof(SchemaItemParameter),
                        primaryKey: new ModelElementKey(id: this.defaultValueParameterId)
                    ) as SchemaItemParameter;
        }
        set
        {
            this.defaultValueParameterId = (
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]
            );
        }
    }
    private EntityColumnXmlMapping _xmlMappingType = EntityColumnXmlMapping.Attribute;

    [Category(category: "Entity Column"), DefaultValue(value: EntityColumnXmlMapping.Attribute)]
    [XmlAttribute(attributeName: "xmlMappingType")]
    public EntityColumnXmlMapping XmlMappingType
    {
        get { return _xmlMappingType; }
        set { _xmlMappingType = value; }
    }
    private OnCopyActionType _onCopyAction = OnCopyActionType.Copy;

    [Category(category: "Entity Column"), DefaultValue(value: OnCopyActionType.Copy)]
    [XmlAttribute(attributeName: "onCopyAction")]
    public OnCopyActionType OnCopyAction
    {
        get { return _onCopyAction; }
        set { _onCopyAction = value; }
    }

    [Browsable(browsable: false)]
    public List<AbstractEntitySecurityRule> RowLevelSecurityRules =>
        ChildItemsByType<AbstractEntitySecurityRule>(
            itemType: AbstractEntitySecurityRule.CategoryConst
        );

    [Browsable(browsable: false)]
    public List<EntityConditionalFormatting> ConditionalFormattingRules =>
        ChildItemsByType<EntityConditionalFormatting>(
            itemType: EntityConditionalFormatting.CategoryConst
        );

    [Browsable(browsable: false)]
    public List<EntityFieldDynamicLabel> DynamicLabels =>
        ChildItemsByType<EntityFieldDynamicLabel>(itemType: EntityFieldDynamicLabel.CategoryConst);

    [Browsable(browsable: false)]
    public DataEntityConstraint ForeignKeyConstraint
    {
        get
        {
            if (ForeignKeyEntity != null && ForeignKeyField != null)
            {
                DataEntityConstraint result = new DataEntityConstraint(
                    type: ConstraintType.ForeignKey
                );
                result.ForeignEntity = ForeignKeyEntity as IDataEntity;
                result.Fields.Add(item: this);
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
            dependencies.Add(item: this.ForeignKeyEntity);
        }

        if (this.ForeignKeyField != null)
        {
            dependencies.Add(item: this.ForeignKeyField);
        }

        if (this.DefaultLookup != null)
        {
            dependencies.Add(item: this.DefaultLookup);
        }

        if (this.DefaultValue != null)
        {
            dependencies.Add(item: this.DefaultValue);
        }

        if (this.DefaultValueParameter != null)
        {
            dependencies.Add(item: this.DefaultValueParameter);
        }

        base.GetExtraDependencies(dependencies: dependencies);
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
