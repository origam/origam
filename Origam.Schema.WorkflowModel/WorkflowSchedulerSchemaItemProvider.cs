using System;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for WorkflowSchemaItemProvider.
	/// </summary>
	public class WorkflowSchedulerSchemaItemProvider : AbstractSchemaItemProvider, ISchemaItemFactory
	{
		public WorkflowSchedulerSchemaItemProvider()
		{
		}

		#region ISchemaItemProvider Members
		public override string RootItemType
		{
			get
			{
				return WorkflowSchedule.ItemTypeConst;
			}
		}
		#endregion

		#region IBrowserNode Members

		public override string Icon
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.ImageIndex getter implementation
				return "36";
			}
		}

		public override string NodeText
		{
			get
			{
				return "Business Process Schedules";
			}
			set
			{
				base.NodeText = value;
			}
		}

		public override string NodeToolTipText
		{
			get
			{
				// TODO:  Add EntityModelSchemaItemProvider.NodeToolTipText getter implementation
				return null;
			}
		}

		#endregion

		#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[1] {typeof(WorkflowSchedule)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaVersionId, Guid schemaExtensionId, SchemaItemGroup group)
		{
			if(type == typeof(WorkflowSchedule))
			{
				WorkflowSchedule item = new WorkflowSchedule(schemaVersionId, schemaExtensionId);
				item.RootProvider = this;
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewSchedule";

				item.Group = group;
				this.ChildItems.Add(item);

				return item;
			}
			else
				throw new ArgumentOutOfRangeException("type", type, "This type is not supported by WorkflowSchedulerModel");
		}

		#endregion
	}
}
