#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System;

namespace Origam.UI;
public delegate void SaveEventHandler(object sender, SaveEventArgs e);

public class SaveEventArgs : System.EventArgs
{
	bool successful;
	
	public bool Successful 
	{
		get 
		{
			return successful;
		}
	}
	
	public SaveEventArgs(bool successful)
	{
		this.successful = successful;
	}
}
/// <summary>
/// IViewContent is the base interface for all editable data
/// inside ORIGAM Workbench.
/// </summary>
public interface IViewContent : IBaseViewContent
{
	/// <summary>
	/// A generic name for the file, when it does have no file name
	/// (e.g. newly created files)
	/// </summary>
	string UntitledName 
	{
		get;
		set;
	}
	
	/// <summary>
	/// This is the whole name of the content, e.g. the file name or
	/// the url depending on the type of the content.
	/// </summary>
	/// <returns>
	/// Title Name, if not set it returns UntitledName
	/// </returns>
	string TitleName 
	{
		get;
		set;
	}
	string StatusText 
	{
		get;
		set;
	}
	Guid DisplayedItemId 
	{
		get;
		set;
	}
    /// <summary>
    /// Returns a help topic (relative URL to the base help) that will be 
    /// opened when pressing F1.
    /// </summary>
    string HelpTopic
    {
        get;
    }
	
	/// <summary>
	/// If this property returns true the view is untitled.
	/// </summary>
	/// <returns>
	/// True, if TitleName not set.
	/// </returns>
	bool IsUntitled 
	{
		get;
	}
	
	/// <summary>
	/// If this property returns true the content has changed since
	/// the last load/save operation.
	/// </summary>
	bool IsDirty 
	{
		get;
		set;
	}
	
	bool CanRefreshContent
	{
		get;
		set;
	}
	/// <summary>
	/// If this property returns true the content could not be altered.
	/// </summary>
	bool IsReadOnly 
	{
		get;
		set;
	}
	
	/// <summary>
	/// If this property returns true the content can't be written.
	/// </summary>
	bool IsViewOnly 
	{
		get;
	}
	
	/// <summary>
	/// If this property is true, content will be created in the tab page
	/// </summary>
	bool CreateAsSubViewContent 
	{
		get;
	}
	void RefreshContent();
	/// <summary>
	/// Saves this content to the last load/save location.
	/// </summary>
	void SaveObject();
	
	/// <summary>
	/// Loads the content from the location <code>fileName</code>
	/// </summary>
	void LoadObject(object objectToLoad);
	object LoadedObject { get; }
	string Test();
	/// <summary>
	/// Is called each time the name for the content has changed.
	/// </summary>
	event EventHandler TitleNameChanged;
	
	/// <summary>
	/// Is called when the content is changed after a save/load operation
	/// and this signals that changes could be saved.
	/// </summary>
	event EventHandler DirtyChanged;
	
	event EventHandler     Saving;
	event SaveEventHandler Saved;
	event EventHandler StatusTextChanged;
}
