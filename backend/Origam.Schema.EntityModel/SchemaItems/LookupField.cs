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
using System.Collections;
using System.ComponentModel;

using Origam.DA;
using Origam.DA.ObjectPersistence;
using System.Xml.Serialization;

namespace Origam.Schema.EntityModel
{
	[SchemaItemDescription("Lookup Field", "Fields", "icon_lookup-field.png")]
    [HelpTopic("Lookup+Field")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class LookupField : AbstractSchemaItem, IDataEntityColumn
	{
		public const string CategoryConst = "DataEntityColumn";
		
		public LookupField() {}

		public LookupField(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public LookupField(Key primaryKey) : base(primaryKey)	{}

		#region IDataEntityColumn Members
		
		[Browsable(false)]
		public OrigamDataType DataType
		{
			get
			{
				if(Lookup == null)
				{
					return OrigamDataType.Boolean;
				}
				if (Lookup.ValueDisplayMember.Contains(";"))
				{
					// concatenated lookup field
					return OrigamDataType.String;
				}
				if (Lookup.ValueDisplayColumn == null)
				{
					throw new ArgumentOutOfRangeException(
						$"ValueDisplayMember {Lookup.ValueDisplayMember} not found in lookup {Lookup.NodeId}.");
				}
				return Lookup.ValueDisplayColumn.DataType;
			}
			set => throw new NotSupportedException();
		}

		[Browsable(false)]
		public int DataLength
		{
			get
			{
				if(Lookup == null)
				{
					return 0;
				}
				if(Lookup.ValueColumn == null)
				{
					throw new ArgumentOutOfRangeException(
						$"ValueValueMember {Lookup.ValueValueMember} not found in lookup {Lookup.NodeId}.");
				}
				return Lookup.ValueColumn.FinalColumn.Field.DataLength;
			}
			set => throw new NotSupportedException();
		}

		[Category("Entity Column"), DefaultValue(true)]
		[XmlAttribute ("allowNulls")]
		[Description("Indicates if the field allows empty values or not. If set to False, also the database column will be generated so that it does not allow nulls. In the user interface the user will have to enter a value before saving the record.")]
		public bool AllowNulls { get; set; } = true;

		[Category("Entity Column"), DefaultValue(false)]
		[XmlAttribute ("isPrimaryKey")]
		[Description("Indicates if the field is a primary key. If set to True, also a database primary key is generated. IMPORTANT: Every entity should have a primary key specified, otherwise data merges will not be able to correlate existing records. NOTE: Multi-column primary keys are possible but GUI expects always only single-column primary keys.")]
		public bool IsPrimaryKey { get; set; } = false;

		[Category("Entity Column")]
		[XmlAttribute ("caption")]
		[Localizable(true)]
		[Description("Default label for the field in a GUI. Audit log viewer also gets the field names from here.")]
		public string Caption { get; set; } = "";

		[Category("Entity Column"), DefaultValue(false)]
		[XmlAttribute ("excludeFromAllFields")]
		[Description("If set to True, the field will not be included in the list of fields in a Data Structure if 'AllFields=True' is set in a Data Structure Entity. This is useful e.g. for database function calls that are expensive and used only for lookups that would otherwise slow down the system if loaded e.g. to forms.")]
		public bool ExcludeFromAllFields { get; set; } = false;

		[Browsable(false)]
		public bool AutoIncrement
		{
			get => false;
			set => throw new NotSupportedException();
		}

		[Browsable(false)]
		public long AutoIncrementSeed
		{
			get => 0;
			set => throw new NotSupportedException();
		}

		[Browsable(false)]
		public long AutoIncrementStep
		{
			get => 1;
			set => throw new NotSupportedException();
		}

		public Guid DefaultLookupId;

		[Browsable(false)]
		[XmlReference("defaultLookup", "DefaultLookupId")]
		public IDataLookup DefaultLookup
		{
			get
			{
				var key = new ModelElementKey
				{
					Id = DefaultLookupId
				};
				try
				{
					return (IDataLookup)PersistenceProvider.RetrieveInstance(
						typeof(AbstractSchemaItem), key);
				}
				catch
				{
					return null;
				}
			}
			set
			{
				if(value == null)
				{
					DefaultLookupId = Guid.Empty;
				}
				else
				{
					DefaultLookupId = (Guid)value.PrimaryKey["Id"];
				}
			}
		}

		[TypeConverter(typeof(DataLookupConverter))]
		[Category("Lookup")]
		[Description("Lookup to be used by the data service to lookup value by the provided Field.")]
		[NotNullModelElementRule()]
		public IDataLookup Lookup
		{
			get => DefaultLookup;
			set => DefaultLookup = value;
		}

		[Browsable(false)]
		public IDataEntity ForeignKeyEntity
		{
			get => null;
			set => throw new NotSupportedException();
		}

		[Browsable(false)]
		public IDataEntityColumn ForeignKeyField
		{
			get => null;
			set => throw new NotSupportedException();
		}

		[Browsable(false)]
		public DataConstant DefaultValue
		{
			get => null;
			set => throw new NotSupportedException();
		}

		[Browsable(false)]
		public SchemaItemParameter DefaultValueParameter
		{
			get => null;
			set => throw new NotSupportedException();
		}


		[Category("Entity Column"), DefaultValue(EntityColumnXmlMapping.Attribute)]
		[XmlAttribute ("xmlMappingType")]
		public EntityColumnXmlMapping XmlMappingType { get; set; } 
			= EntityColumnXmlMapping.Attribute;

		[Browsable(false)]
		public OnCopyActionType OnCopyAction
		{
			get => OnCopyActionType.Copy;
			set => throw new NotSupportedException();
		}

		[Browsable(false)]
		public ArrayList RowLevelSecurityRules 
			=> ChildItemsByType(AbstractEntitySecurityRule.CategoryConst);

		[Browsable(false)]
		public ArrayList ConditionalFormattingRules 
			=> ChildItemsByType(EntityConditionalFormatting.CategoryConst);

		[Browsable(false)]
		public ArrayList DynamicLabels 
			=> ChildItemsByType(EntityFieldDynamicLabel.CategoryConst);
		#endregion

		#region Properties
		public Guid FieldId;

		[TypeConverter(typeof(EntityColumnReferenceConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[NotNullModelElementRule()]
		[Category("Lookup")]
        [XmlReference("field", "FieldId")]
        public IDataEntityColumn Field
		{
			get => (IDataEntityColumn)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.FieldId));
			set => this.FieldId = (Guid)value.PrimaryKey["Id"];
		}

        [Browsable(false)]
        public DataEntityConstraint ForeignKeyConstraint => null;

        public string FieldType { get; } = "LookupField";

        #endregion

        #region Overriden AbstractSchemaItem Methods
        [Browsable(false)]
		public bool ReadOnly => false;

		public override bool CanMove(UI.IBrowserNode2 newNode) 
			=> newNode is IDataEntity;

		public override string ItemType => AbstractDataEntityColumn.CategoryConst;

		public override void GetExtraDependencies(ArrayList dependencies)
		{
			if(DefaultLookup != null)
			{
				dependencies.Add(DefaultLookup);
			}
			if(DefaultValue != null)
			{
				dependencies.Add(DefaultValue);
			}
			if(DefaultValueParameter != null)
			{
				dependencies.Add(DefaultValueParameter);
			}
			if(Field != null)
			{
				dependencies.Add(Field);
			}
			base.GetExtraDependencies (dependencies);
		}

		public override void GetParameterReferences(
			AbstractSchemaItem parentItem, Hashtable list)
		{
		}

		public override void UpdateReferences()
		{
			foreach(ISchemaItem item in RootItem.ChildItemsRecursive)
			{
				if(item.OldPrimaryKey?.Equals(Field.PrimaryKey) == true)
				{
					Field = item as IDataEntityColumn;
					break;
				}
			}
			base.UpdateReferences();
		}

		#endregion

		#region ISchemaItemFactory Members
		[Browsable(false)]
		public override Type[] NewItemTypes => new[] 
		{
				typeof(EntityFieldSecurityRule),
				typeof(EntityFieldDependency),
				typeof(EntityConditionalFormatting),
				typeof(EntityFieldDynamicLabel)
		};

		public override T NewItem<T>(
			Guid schemaExtensionId, SchemaItemGroup group)
		{
			string itemName = null;
			if(typeof(T) == typeof(EntityFieldSecurityRule))
			{
				itemName = "NewRowLevelSecurityRule";
			}
			else if(typeof(T) == typeof(EntityFieldDependency))
			{
				itemName = "NewEntityFieldDependency";
			}
			else if(typeof(T) == typeof(EntityConditionalFormatting))
			{
				itemName = "NewFormatting";
			}
			else if(typeof(T) == typeof(EntityFieldDynamicLabel))
			{
				itemName = "NewDynamicLabel";
			}
			return base.NewItem<T>(schemaExtensionId, group, itemName);
		}
		#endregion
	}
}
