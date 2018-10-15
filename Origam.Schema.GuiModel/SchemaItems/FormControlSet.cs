using System;
using Origam.DA.ObjectPersistence; 
using Origam.Schema.EntityModel;

namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for FormControlSet.
	/// </summary>
	public class FormControlSet : AbstractControlSet 
	{
		public const string ItemTypeConst = "FormControlSet";

		public FormControlSet() : base() {}
		
		public FormControlSet(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public FormControlSet(Key primaryKey) : base(primaryKey) {}

		//refDataSource means for PanelCOntolSet reference on DataEntity object
		// (for FormControlSet refDataSource means reference on DataStructure object
		[EntityColumn("refDataSourceId", "ControlSet")]  
		public Guid DataSourceId;

		[EntityColumn("refDataSourceVersionId", "ControlSet")] 
		public Guid DataSourceSchemaVersionId;

		public DataStructure DataStructure
		{
			get
			{
				SchemaVersionKey key = new SchemaVersionKey();
				key.Id = this.DataSourceId;
				key.SchemaVersionId = this.DataSourceSchemaVersionId;

				return (DataStructure)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
			}
			set
			{
				this.DataSourceId = (Guid)value.PrimaryKey["Id"];
				this.DataSourceSchemaVersionId = (Guid)value.PrimaryKey["SchemaVersionId"];
			}
		}
		
		#region Overriden AbstractSchemaItem Members
		public override string Icon
		{
			get
			{
				return "13";
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
				return FormControlSet.ItemTypeConst;
			}
		}

		public override Origam.UI.BrowserNodeCollection ChildNodes()
		{
			return new Origam.UI.BrowserNodeCollection();
		}

		#endregion			


	}
}
