using System;
using Origam.DA.ObjectPersistence; 

namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for PropertyValueItem.
	/// </summary>
	/// 


	public class PropertyBindingInfo : AbstractPropertyValueItem
	{
		public const string ItemTypeConst = "PropertyBindingInfo";

		public PropertyBindingInfo() : base(){}
		
		public PropertyBindingInfo(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public PropertyBindingInfo(Key primaryKey) : base(primaryKey)	{}


		private string _designDataSetPath;

		[EntityColumn("DesignDataSetPath", "PropertyValueItem")] 
		public string DesignDataSetPath
		{
			get
			{
				return _designDataSetPath;
			}
			set
			{
				_designDataSetPath=value;
			}

		}

		


		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return PropertyBindingInfo.ItemTypeConst;
			}
		}
	}





	public class PropertyValueItem : AbstractPropertyValueItem
	{

		public const string ItemTypeConst = "PropertyValueItem";

		public PropertyValueItem() : base(){}
		
		public PropertyValueItem(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public PropertyValueItem(Key primaryKey) : base(primaryKey)	{}


		private int _intValue;

		#region Properties
		[EntityColumn("IntValue", "PropertyValueItem")] 
		public int IntValue
		{
			get
			{
				return _intValue;
			}
			set
			{
				_intValue=value;
			}

		}

		private bool _boolValue;

		[EntityColumn("BoolValue", "PropertyValueItem")] 
		public bool BoolValue
		{
			get
			{
				return _boolValue;
			}
			set
			{
				_boolValue=value;
			}

		}


		private Guid _guidValue;

		[EntityColumn("GuidValue", "PropertyValueItem")] 
		public Guid GuidValue
		{
			get
			{
				return _guidValue;
			}
			set
			{
				_guidValue=value;
			}

		}


		#endregion

		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return PropertyValueItem.ItemTypeConst;
			}
		}
	}


	public abstract class AbstractPropertyValueItem  : AbstractSchemaItem
	{
		public AbstractPropertyValueItem() : base(){}
		
		public AbstractPropertyValueItem(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public AbstractPropertyValueItem(Key primaryKey) : base(primaryKey)	{}

		#region Properties


		[EntityColumn("refControlPropertyId", "PropertyValueItem")]  
		public Guid ControlPropertyId;

		[EntityColumn("refControlPropertySchemaVersionId", "PropertyValueItem")] 
		public Guid ControlPropertySchemaVersionId;

		public ControlPropertyItem ControlPropertyItem
		{
			get
			{
				SchemaVersionKey key = new SchemaVersionKey();
				key.Id = this.ControlPropertyId;
				key.SchemaVersionId = this.ControlPropertySchemaVersionId;

				return (ControlPropertyItem)this.PersistenceProvider.RetrieveInstance(typeof(ControlPropertyItem), key);
			}
			set
			{
				this.ControlPropertyId = (Guid)value.PrimaryKey["Id"];
				this.ControlPropertySchemaVersionId = (Guid)value.PrimaryKey["SchemaVersionId"];
			}
		}

		private string _value;

		[EntityColumn("Value", "PropertyValueItem")] 
		public string Value
		{
			get
			{
				return _value;
			}
			set
			{
				_value=value;
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
				return this.Value;
			}
		}
		
		//public abstract string ItemType {get;}
		
		#endregion
	}
}
