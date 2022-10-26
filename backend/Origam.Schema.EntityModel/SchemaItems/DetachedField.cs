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
using System.Collections;
using System.Xml.Serialization;
using Origam.DA.EntityModel;

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for DetachedField.
	/// </summary>
	[SchemaItemDescription("Virtual Field", "Fields", "icon_virtual-field.png")]
    [HelpTopic("Virtual+Field")]
    [ClassMetaVersion("6.0.0")]
	public class DetachedField : AbstractDataEntityColumn, IRelationReference
	{
		public DetachedField() : base() {}

		public DetachedField(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public DetachedField(Key primaryKey) : base(primaryKey)	{}

		#region Properties
		public Guid ArrayRelationId;

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
		
		[Category("Array")]
		[TypeConverter(typeof(EntityRelationConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("arrayRelation", "ArrayRelationId")]
		public IAssociation ArrayRelation
		{
			get
			{
				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ArrayRelationId)) as IAssociation;
			}
			set
			{
				this.ArrayRelationId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
				this.ArrayValueField = null;
			}
		}

		[Browsable(false)]
			// only for IReference needs, for public access we have ArrayRelation
		public IAssociation Relation
		{
			get
			{
				return this.ArrayRelation;
			}
			set
			{
				this.ArrayRelation = value;
                this.ArrayValueField = null;
			}
		}
 
		public Guid ArrayValueFieldId;

		[Category("Array")]
		[TypeConverter(typeof(EntityRelationColumnsConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
        [XmlReference("arrayValueField", "ArrayValueFieldId")]
		public IDataEntityColumn ArrayValueField
		{
			get
			{
				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ArrayValueFieldId)) as IDataEntityColumn;
			}
			set
			{
				this.ArrayValueFieldId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);

				if(value != null)
				{
					this.DataType = OrigamDataType.Array;
					this.DefaultLookup = this.ArrayValueField.DefaultLookup;
					this.Caption = this.ArrayValueField.Caption;
				}
			}
		}
		#endregion

		#region Overriden AbstractDataEntityColumn Members

		public override string FieldType { get; } = "DetachedField";

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

        public override void GetExtraDependencies(ArrayList dependencies)
        {
            if(ArrayRelation != null)
            {
                dependencies.Add(ArrayRelation);
            }
            if(ArrayValueField !=null)
            {
                dependencies.Add(ArrayValueField);
            }
            base.GetExtraDependencies(dependencies);
        }

        public override void UpdateReferences()
        {
            if (this.ArrayRelation != null)
            {
                foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
                {
                    if (item.OldPrimaryKey != null)
                    {
                        if (item.OldPrimaryKey.Equals(this.ArrayRelation.PrimaryKey))
                        {
                            // store the old field because setting an entity will reset the field
                            IDataEntityColumn oldField = this.ArrayValueField;
                            this.ArrayRelation = item as IAssociation;
                            this.ArrayValueField = oldField;
                            break;
                        }
                    }
                }
            }
            base.UpdateReferences();
        }
        #endregion

		#region Convert
		public override bool CanConvertTo(Type type)
		{
			return
				(
				(
				type == typeof(FieldMappingItem)
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

			if(converted is FieldMappingItem)
			{
				(converted as FieldMappingItem).MappedColumnName = this.Name;
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
	}
}
