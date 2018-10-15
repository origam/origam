using System;
using Origam.DA.ObjectPersistence; 
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;



namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for PanelControlSet.
	/// </summary>
	public class PanelControlSet : AbstractControlSet
	{

		private static SchemaService _schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
		private UserControlSchemaItemProvider _controls=_schema.GetProvider(typeof(UserControlSchemaItemProvider)) as UserControlSchemaItemProvider;

		public const string ItemTypeConst = "PanelControlSet";

		public PanelControlSet() : base() {}
		
		public PanelControlSet(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public PanelControlSet(Key primaryKey) : base(primaryKey) {}

        //refDataSource means for PanelCOntolSet reference on DataEntity object
        // (for FormControlSet refDataSource means reference on DataStructure object
		[EntityColumn("refDataSourceId", "ControlSet")]  
		public Guid DataSourceId;

		[EntityColumn("refDataSourceVersionId", "ControlSet")] 
		public Guid DataSourceSchemaVersionId;

		public IDataEntity DataEntity
		{
			get
			{
				SchemaVersionKey key = new SchemaVersionKey();
				key.Id = this.DataSourceId;
				key.SchemaVersionId = this.DataSourceSchemaVersionId;

				return (IDataEntity)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				this.DataSourceId = (Guid)value.PrimaryKey["Id"];
				this.DataSourceSchemaVersionId = (Guid)value.PrimaryKey["SchemaVersionId"];
			}
		}


		
		#region Overriden AbstractSchemaItem Members

		public override bool IsDeleted
		{
			get
			{
				return base.IsDeleted;
			}
			set
			{
				//1) find controlItem
				foreach(ControlItem item in _controls.ChildItems)
				{
					if(	item.PanelControlSet !=null && 
						item.PanelControlSet.PrimaryKey.Equals(this.PrimaryKey) && 
						item.IsComplexType && (!item.IsDeleted) )
					{
						//2) delete reference in ControlItem and set is complex on false
						item.PanelControlSet=null;
						item.IsComplexType = false;
						//3) delete founded ControlItem
						item.IsDeleted=true;
						break;
					}
				}
				
				//if all done delete main control
				base.IsDeleted = value;
			}
		}



		public override string Icon
		{
			get
			{
				return "14";
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
				return ItemTypeConst;
			}
		}

	
		#endregion			
	
	}
}

