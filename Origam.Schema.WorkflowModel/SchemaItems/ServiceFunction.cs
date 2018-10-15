using System;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for ServiceFunction.
	/// </summary>
	public class ServiceFunction : AbstractSchemaItem, IService, ISchemaItemFactory
	{
		public const string ItemTypeConst = "ServiceFunction";

		public ServiceFunction() : base() {}

		public ServiceFunction(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public ServiceFunction(Key primaryKey) : base(primaryKey)	{}

		#region Overriden AbstractSchemaItem Members
		
		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ItemTypeConst;
			}
		}

		public override string Icon
		{
			get
			{
				return "5";
			}
		}

		public override string NodeToolTipText
		{
			get
			{
				return null;
			}
		}
		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				// TODO:  Add Workflow.NewItemTypes getter implementation
				return null;
			}
		}

		public override SchemaItemGroup NewGroup(Guid schemaVersionId, Guid schemaExtensionId)
		{
			// TODO:  Add Workflow.NewGroup implementation
			return null;
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaVersionId, Guid schemaExtensionId)
		{
			// TODO:  Add Workflow.NewItem implementation
			return null;
		}

		#endregion
	}
}
