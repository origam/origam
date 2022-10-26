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

namespace Origam.Schema.EntityModel
{
	/// <summary>
	/// Summary description for FunctionCall.
	/// </summary>
	[SchemaItemDescription("Function Call", "Fields", "icon_function-call.png")]
    [HelpTopic("Function+Call+Field")]
    [DefaultProperty("Function")]
    [ClassMetaVersion("6.0.0")]
    public class FunctionCall : AbstractDataEntityColumn, ISchemaItemFactory
	{
		public FunctionCall() : base() {}

		public FunctionCall(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public FunctionCall(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractDataEntityColumn Members
		
		[Browsable(false)]
		public override bool UseFolders
		{
			get
			{
				return false;
			}
		}

		public override string FieldType { get; } = "FunctionCall";

		public override bool ReadOnly
		{
			get
			{
				if(this.Function == null)
				{
					return true;
				}
				else
				{
					return this.Function.FunctionType == OrigamFunctionType.Standard;
				}
			}
		}

		public override string Icon
		{
			get
			{
				if(this.Function == null)
				{
					return "icon_function-call.png";
				}
				else
				{
					return this.Function.Icon;
				}
			}
		}

		public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
		{
			// can move inside the same entity 
			if(this.RootItem == (newNode as ISchemaItem).RootItem)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Function);

			base.GetExtraDependencies (dependencies);
		}
		#endregion

		#region Properties
		public Guid FunctionId;

		[Category("Function")]
		[TypeConverter(typeof(FunctionConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[NotNullModelElementRule()]
        [XmlReference("function", "FunctionId")]
        public Function Function
		{
			get
			{
				ModelElementKey key = new ModelElementKey();
				key.Id = this.FunctionId;

				try
				{
					return (Function)this.PersistenceProvider.RetrieveInstance(typeof(Function), key);
				}
				catch
				{
					return null;
				}
			}
			set
			{
				// We have to delete all child items
				foreach(ISchemaItem item in this.ChildItems)
				{
					item.IsDeleted = true;
				}


				if(value == null)
				{
					this.FunctionId = Guid.Empty;

					this.Name = "";
				}
				else
				{
					this.FunctionId = (Guid)value.PrimaryKey["Id"];

					if(this.Name == null)
					{
						this.Name = this.Function.Name;
					}

					// We generate all parameters to the function
					foreach(FunctionParameter parameter in this.Function.ChildItems)
					{
						FunctionCallParameter parameterRef = this.NewItem(typeof(FunctionCallParameter), this.SchemaExtensionId, null) as FunctionCallParameter;
						parameterRef.FunctionParameter = parameter;
						parameterRef.Name = parameter.Name;
						//parameterRef.Persist();
					}
				}
			}
		}

		private bool _forceDatabaseCalculation = false;
		[Category("Function"), DefaultValue(false)]
		[XmlAttribute("forceDatabaseCalculation")]
        public bool ForceDatabaseCalculation
		{
			get
			{
				return _forceDatabaseCalculation;
			}
			set
			{
				_forceDatabaseCalculation = value;
			}
		}
		#endregion

		#region ISchemaItemFactory Members

		[Browsable(false)]
		public override Type[] NewItemTypes
		{
			get
			{
				if(this.ParentItem is IDataEntity)
				{
					return base.NewItemTypes;
				}
				else
				{
					return new Type[1] {typeof(FunctionCallParameter)};
				}
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			if(type == typeof(FunctionCallParameter))
			{
				FunctionCallParameter item = new FunctionCallParameter(schemaExtensionId);
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewFunctionCallParameter";

				item.Group = group;
				item.IsAbstract = this.IsAbstract;
				this.ChildItems.Add(item);

				return item;
			}
			else
			{
				return base.NewItem(type, schemaExtensionId, group);
			}
		}

		#endregion

		#region Convert
		public override bool CanConvertTo(Type type)
		{
			return
				(
					(
						type == typeof(FieldMappingItem)
						| type == typeof(DetachedField)
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
			else if(type == typeof(DetachedField))
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
	}
}
