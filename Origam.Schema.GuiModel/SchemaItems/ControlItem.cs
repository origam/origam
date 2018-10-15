using System.ComponentModel;
using Origam.DA.ObjectPersistence; 
using System;

namespace Origam.Schema.GuiModel
{

	public enum ControlToolBoxVisibility {Nowhere, PanelDesigner, FormDesigner, PanelAndFormDesigner};


	/// <summary>
	/// Summary description for ControlItem.
	/// </summary>
	public class ControlItem : AbstractSchemaItem, ISchemaItemFactory
	{

		public const string ItemTypeConst = "Control";

		public ControlItem() : base(){}

		public ControlItem(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public ControlItem(Key primaryKey) : base(primaryKey)	{}


		#region Properties


		private ControlToolBoxVisibility _controlToolBoxVisibility;

		[EntityColumn("ControlToolBoxVisibility", "ControlItem")] 
		public ControlToolBoxVisibility ControlToolBoxVisibility
		{
			get
			{
				return _controlToolBoxVisibility;
			}
			set
			{
				_controlToolBoxVisibility = value;
			}
	
		}

		private string _controlType;

		[EntityColumn("ControlType", "ControlItem")] 
		public string ControlType
		{
			get
			{
				return _controlType;
			}
			set
			{
				_controlType = value;
			}
		}

		private string _controlNamespace;

		[EntityColumn("ControlNamespace", "ControlItem")] 
		public string ControlNamespace
		{
			get
			{
				return _controlNamespace;
			}
			set
			{
				_controlNamespace = value;
			}
		}

	
		private bool _isComplexType;

		[EntityColumn("IsComplexType", "ControlItem")] 
		public bool IsComplexType
		{
			get
			{
				return _isComplexType;
			}
			set
			{
				_isComplexType = value;
			}
		}

		[EntityColumn("refPanelControlSetId", "ControlItem")]  
		public Guid PanelControlSetId;

		[EntityColumn("refPanelControlSetSchemaVersionId", "ControlItem")] 
		public Guid PanelControlSetSchemaVersionId;

		public PanelControlSet PanelControlSet
		{
			get
			{
				SchemaVersionKey key = new SchemaVersionKey();
				key.Id = this.PanelControlSetId;
				key.SchemaVersionId = this.PanelControlSetSchemaVersionId;
				return (PanelControlSet)this.PersistenceProvider.RetrieveInstance(typeof(PanelControlSet), key);
			}
			set
			{
				if(value!=null)
				{
					this.PanelControlSetId = (Guid)value.PrimaryKey["Id"];
					this.PanelControlSetSchemaVersionId = (Guid)value.PrimaryKey["SchemaVersionId"];
				}
				else
				{
					this.PanelControlSetId = System.Guid.Empty;
					this.PanelControlSetSchemaVersionId = System.Guid.Empty;

				}
			}
		}

		#endregion

	#region Overriden AbstractSchemaItem Members


		[Browsable(false)] 
		public override bool CanDelete
		{
			get
			{
				if(this.PanelControlSet == null)
                    return true;
				else
					return false;
			}
		}


		public override string Icon
		{
			get
			{
				return "3";
			}
		}

		public override string NodeToolTipText
		{
			get
			{
				return this.ControlType;
			}
		}

		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ControlItem.ItemTypeConst;
			}
		}
		#endregion

	#region ISchemaItemFactory Members

		public override Type[] NewItemTypes
		{
			get
			{
				return new Type[1] {typeof(ControlPropertyItem)};
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaVersionId, Guid schemaExtensionId)
		{
			if(type == typeof(ControlPropertyItem))
			{
				ControlPropertyItem item =  new ControlPropertyItem(schemaVersionId, schemaExtensionId);
				item.PersistenceProvider = this.PersistenceProvider;
				item.Name = "NewControlPropertyItem";
				this.ChildItems.Add(item);
				return item;
			}
			else
				throw new ArgumentOutOfRangeException("type", type, "This type is not supported by EntityModel");
		}

		#endregion

	}
	
}
