using System;
using Origam.DA.ObjectPersistence; 


namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for AbstractControlSet.
	/// </summary>
	public abstract class AbstractControlSet : AbstractSchemaItem, IControlSet, ISchemaItemFactory 
	{
		public AbstractControlSet() : base() {}
		
		public AbstractControlSet(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public AbstractControlSet(Key primaryKey) : base(primaryKey) {}
			
		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[1] {typeof(ControlSetItem)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaVersionId, Guid schemaExtensionId)
		{
			if(type == typeof(ControlSetItem))
			{
				ControlSetItem item = new ControlSetItem(schemaVersionId, schemaExtensionId);
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewComponent";

				this.ChildItems.Add(item);

				return item;
			}
			else
				throw new ArgumentOutOfRangeException("type", type, "This type is not supported by FormControlSet");
		}

	
		#endregion
	}

	
}
