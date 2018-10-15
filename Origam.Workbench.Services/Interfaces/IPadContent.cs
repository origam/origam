using System;
using System.Windows.Forms;

namespace Origam.Workbench
{
	/// <summary>
	/// The IPadContent interface is the basic interface to all "tool" windows
	/// in ORIGAM Workbench.
	/// </summary>
	public interface IPadContent : IDisposable
	{
		/// <summary>
		/// Returns the title of the pad.
		/// </summary>
		string Title 
		{
			get;
		}
		
		/// <summary>
		/// Returns the icon bitmap resource name of the pad. May be null, if the pad has no
		/// icon defined.
		/// </summary>
		string IconResource
		{
			get;
		}
		
		/// <summary>
		/// Returns the category (this is used for defining where the menu item to
		/// this pad goes)
		/// </summary>
		string Category 
		{
			get;
			set;
		}
		
		/// <summary>
		/// Returns the menu shortcut for the view menu item.
		/// </summary>
		string[] Shortcut 
		{
			get;
			set;
		}
				
		/// <summary>
		/// Re-initializes all components of the pad. Don't call unless
		/// you know what you do.
		/// </summary>
		void RedrawContent();
		
		/// <summary>
		/// Is called when the title of this pad has changed.
		/// </summary>
		event EventHandler TitleChanged;
		
		/// <summary>
		/// Is called when the icon of this pad has changed.
		/// </summary>
		event EventHandler IconChanged;
		
		/// <summary>
		/// Tries to make the pad visible to the user.
		/// </summary>
		void BringPadToFront();
	}
}
