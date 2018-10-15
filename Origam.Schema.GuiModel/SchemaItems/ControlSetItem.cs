using System;
using Origam.DA.ObjectPersistence; 

namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for ControlSetItem.
	/// </summary>
	public class ControlSetItem  : AbstractSchemaItem, ISchemaItemFactory 
	{
		public const string ItemTypeConst = "ControlSetItem";

		public ControlSetItem() : base(){}
		
		public ControlSetItem(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public ControlSetItem(Key primaryKey) : base(primaryKey)	{}

		#region Properties


		[EntityColumn("refControlId", "ControlSetItem")]  
		public Guid ControlId;

		[EntityColumn("refControlSchemaVersionId", "ControlSetItem")] 
		public Guid ControlSchemaVersionId;

		public ControlItem ControlItem
		{
			get
			{
				SchemaVersionKey key = new SchemaVersionKey();
				key.Id = this.ControlId;
				key.SchemaVersionId = this.ControlSchemaVersionId;

				return (ControlItem)this.PersistenceProvider.RetrieveInstance(typeof(ControlItem), key);
			}
			set
			{
				this.ControlId = (Guid)value.PrimaryKey["Id"];
				this.ControlSchemaVersionId = (Guid)value.PrimaryKey["SchemaVersionId"];
			}
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

		public override string NodeToolTipText
		{
			get
			{
				return null;
			}
		}

		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ControlSetItem.ItemTypeConst;
			}
		}
		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[3] {typeof(ControlSetItem), typeof(PropertyValueItem), typeof(PropertyBindingInfo)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaVersionId, Guid schemaExtensionId)
		{
			if(type == typeof(PropertyValueItem))
			{
				PropertyValueItem item = new PropertyValueItem(schemaVersionId, schemaExtensionId);
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewPropertyValue";

				this.ChildItems.Add(item);

				return item;
			}
			else if(type == typeof(ControlSetItem))
			{
				ControlSetItem item = new ControlSetItem(schemaVersionId, schemaExtensionId);
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewControlSetItem";

				this.ChildItems.Add(item);

				return item;
			}
			else if(type == typeof(PropertyBindingInfo))
			{
				PropertyBindingInfo item = new PropertyBindingInfo(schemaVersionId, schemaExtensionId);
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewPropertyBindingInfo";

				this.ChildItems.Add(item);

				return item;
			}
			else
				throw new ArgumentOutOfRangeException("type", type, "This type is not supported by ControlSetItem");
		}

		#endregion
	}
}
