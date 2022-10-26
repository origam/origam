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
using Origam.DA.EntityModel;

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Maps a physical database field to an entity column.
	/// </summary>
	[SchemaItemDescription("Database Field", "Fields", 
        "icon_database-field.png")]
    [HelpTopic("Database+Field")]
    [ClassMetaVersion("6.0.0")]
	public class FieldMappingItem : AbstractDataEntityColumn, ISchemaItemFactory,
        IDatabaseDataTypeMapping
	{
		public FieldMappingItem() : base() {}

		public FieldMappingItem(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public FieldMappingItem(Key primaryKey) : base(primaryKey)	{}

		#region Properties
		
		[NoDuplicateNamesInParentRule]
		[Category("(Schema Item)")]
		[StringNotEmptyModelElementRule]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlAttribute("name")]
		public override string Name
		{
			get => base.Name; 
			set => base.Name = value;
		}
		
		private string _sourceFieldName;
		[Category("Mapping")]
		[StringNotEmptyModelElementRule()]
        [XmlAttribute("mappedColumnName")]
        [DisplayName("Mapped Column Name")]
		public string MappedColumnName
		{
			get
			{
				if(_sourceFieldName == null) return null;

				return _sourceFieldName.Trim();
			}
			set
			{
				_sourceFieldName = value;
			}
		}

        public Guid dataTypeMappingId;

        [Category("Mapping")]
        [TypeConverter(typeof(DataTypeMappingConverter))]
        [Description("Optional specific data type. If not specified a default type will be assigned based on the main Data Type.")]
        [DisplayName("Mapped Data Type")]
        [XmlReference("mappedDataType", "dataTypeMappingId")]
        public DatabaseDataType MappedDataType
        {
            get
            {
                return (DatabaseDataType)PersistenceProvider.RetrieveInstance(
                    typeof(DatabaseDataType), new ModelElementKey(dataTypeMappingId)) 
                    as DatabaseDataType;
            }
            set
            {
                dataTypeMappingId = (value == null ? Guid.Empty 
                    : (Guid)value.PrimaryKey["Id"]);
            }

        }
		#endregion

		#region Overriden AbstractDataEntityColumn Members
		protected bool _excludeFromAuditing = false;
		[Browsable(true)]
		[Category("Entity Column"), DefaultValue(false)]
		[Description("When turned on this field's changes will not be recorded in the audit log (e.g. password fields).")]
        [XmlAttribute("excludeFromAuditing")]
        public override bool ExcludeFromAuditing
		{
			get
			{
				return _excludeFromAuditing;
			}
			set
			{
				_excludeFromAuditing = value;
			}
		}

        public override string FieldType { get; } = "FieldMappingItem";

        [Browsable(false)]
		public override bool ReadOnly
		{
			get
			{
				return false;
			}
		}

		public override void GetParameterReferences(AbstractSchemaItem parentItem, System.Collections.Hashtable list)
		{
			return;
		}

		public override void OnNameChanged(string originalName)
		{
			if(MappedColumnName == "" 
				|| MappedColumnName == null
				|| MappedColumnName == originalName)
			{
				MappedColumnName = this.Name;
			}
		}

        public override void OnPropertyChanged(string propertyName)
        {
            if (propertyName == "DataType")
            {
                MappedDataType = null;
            }
            base.OnPropertyChanged(propertyName);
        }
		#endregion

		#region Convert
		public override bool CanConvertTo(Type type)
		{
			return
				(
				(
					type == typeof(DetachedField)
				)
				&
				(
				this.ParentItem is IDataEntity
				)
				);
		}

		public override ISchemaItem ConvertTo(Type type)
		{
			AbstractSchemaItem converted = this.ParentItem.NewItem(type, this.SchemaExtensionId, this.Group) as AbstractSchemaItem;

			if(converted is AbstractDataEntityColumn)
			{
				AbstractDataEntityColumn.CopyFieldMembers(this, converted as AbstractDataEntityColumn);
			}

			if(type == typeof(DetachedField))
			{
			}
			else
			{
				return base.ConvertTo(type);
			}

			// does the common conversion tasks and persists both this and converted objects
			AbstractSchemaItem.FinishConversion(this, converted);

			return converted;
		}
		#endregion

		public static IDataEntity GetLocalizationTable(TableMappingItem tmi)
		{
			if (tmi == null) return null;
			if (tmi.LocalizationRelation != null)
			{
				return tmi.LocalizationRelation.AssociatedEntity;
			}
			return null;
		}

		[Browsable(false)]
		public FieldMappingItem GetLocalizationField(TableMappingItem tmi)
		{
			if (this.DataType != OrigamDataType.String && this.DataType != OrigamDataType.Memo)
			{
				// non-string data types couldn't be localized
				return null;
			}
			IDataEntity localizationTable = GetLocalizationTable(tmi);
			if (localizationTable == null) return null;

			// find column in localization table
			FieldMappingItem col = localizationTable.GetChildByName(this.Name) as FieldMappingItem;
			return col;			
		}
	}
}
