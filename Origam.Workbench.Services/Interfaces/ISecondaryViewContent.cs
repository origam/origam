using System;

namespace Origam.Workbench
{
	/// <summary>
	/// The base interface for secondary view contents
	/// (designer, doc viewer etc.)
	/// </summary>
	public interface ISecondaryViewContent : IBaseViewContent
	{
		/// <summary>
		/// Is called before the save operation of the main IViewContent
		/// </summary>
		void NotifyBeforeSave();
		
		void NotifyAfterSave(bool successful);
	}
}
