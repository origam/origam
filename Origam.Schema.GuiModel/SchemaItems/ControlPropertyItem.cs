using Origam.DA.ObjectPersistence; 
using System;

namespace Origam.Schema.GuiModel
{
	/// <summary>
	/// Summary description for ControlPropertyItem.
	/// </summary>
	/// 

	public enum ControlPropertyValueType {integer=0, bit, str, xml, guid}


	public class ControlPropertyItem : AbstractSchemaItem 
	{
		public const string ItemTypeConst = "ControlPropertyItem";

		public ControlPropertyItem() : base(){}
		
		public ControlPropertyItem(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public ControlPropertyItem(Key primaryKey) : base(primaryKey)	{}

		#region Properties

		private ControlPropertyValueType _propertyType;

		[EntityColumn("PropertyType", "ControlPropertyItem")] 
		public ControlPropertyValueType PropertyType
		{
			get
			{
				return _propertyType;
			}
			set
			{
				_propertyType = value;
			}
		}

		private bool _isBindOnly;
		[EntityColumn("IsBindOnly", "ControlPropertyItem")] 
		public bool IsBindOnly
		{
			get
			{
				return _isBindOnly;
			}
			set
			{
				_isBindOnly = value;
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
				return this.PropertyType.ToString();
			}
		}

		[EntityColumn("ItemType")]
		public override string ItemType
		{
			get
			{
				return ControlPropertyItem.ItemTypeConst;
			}
		}
		#endregion

	}
}
