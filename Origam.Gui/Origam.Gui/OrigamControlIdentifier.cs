using System;

namespace Origam.Gui
{
	/// <summary>
	/// Every Origam Control Has in Tag property OrigamControlIdentifier.
	/// ControlID guid which indentifies him in origam GUI database
	/// ControlDataColumnGuid and ControlDataSourceGuid shows the data background
	/// </summary>
	/// 
	
	public struct OrigamControlIdentifier
	{
		public Guid ControlSetItemGuid;			//Control Guid
        public Guid ControlSetItemParentGuid;	//Reference to parent control
		public Guid ControlSetGuid;				//reference to ControlSet Definition
		public Guid ControlGuid;				//reference to type
								
		object Tag;			
	}

	public struct OrigamGuid
	{
		public const string NULL_GUID =  "00000000-0000-0000-0000-000000000000";
		public Guid NullGuid()
		{
				return new Guid("00000000-0000-0000-0000-000000000000");
		}
		public Guid NewGuid()
		{
			return Guid.NewGuid();
		}
	}
}
