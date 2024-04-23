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
using System.Collections;

namespace Origam.UI;

/// <summary>
/// This is the basic interface to the workspace.
/// </summary>
public interface IWorkbench
{
	/// <summary>
	/// The title shown in the title bar.
	/// </summary>
	string Title 
	{
		get;
		set;
	}

	/// <summary>
	/// If false it's possible to connect to data database.
	/// </summary>
	bool ApplicationDataDisconnectedMode
	{
		get;
		set;
	}

	/// <summary>
	/// A collection in which all active workspace windows are saved.
	/// </summary>
	ViewContentCollection ViewContentCollection 
	{
		get;
	}
		
	/// <summary>
	/// A collection in which all active workspace windows are saved.
	/// </summary>
	PadContentCollection PadContentCollection 
	{
		get;
	}
		
	/// <summary>
	/// The active workbench window.
	/// </summary>
	IViewContent ActiveViewContent
	{
		get;
	}
		
	IViewContent ActiveDocument
	{
		get;
	}
		
	/// <summary>
	/// Inserts a new <see cref="IViewContent"/> object in the workspace.
	/// </summary>
	void ShowView(IViewContent content);
		
	/// <summary>
	/// Inserts a new <see cref="IPadContent"/> object in the workspace.
	/// </summary>
	void ShowPad(IPadContent content);
		
	/// <summary>
	/// Returns a pad from a specific type.
	/// </summary>
	IPadContent GetPad(Type type);
		
	/// <summary>
	/// Closes the IViewContent content when content is open.
	/// </summary>
	void CloseContent(IViewContent content);
		
	/// <summary>
	/// Closes all views inside the workbench.
	/// </summary>
	void CloseAllViews();

	/// <summary>
	/// Closes all views inside the workbench except of a specific one.
	/// </summary>
	/// <param name="except">View which should remain.</param>
	void CloseAllViews(IViewContent except);

	/// <summary>
	/// Re-initializes all components of the workbench, should be called
	/// when a special property is changed that affects layout stuff.
	/// (like language change) 
	/// </summary>
	void RedrawAllComponents();
		
	/// <summary>
	/// Is called, when a workbench view was opened
	/// </summary>
	event ViewContentEventHandler ViewOpened;
		
	/// <summary>
	/// Is called, when a workbench view was closed
	/// </summary>
	event ViewContentEventHandler ViewClosed;
		
	/// <summary>
	/// Is called, when the workbench window which the user has into
	/// the foreground (e.g. editable) changed to a new one.
	/// </summary>
	event EventHandler ActiveWorkbenchWindowChanged;

	void UpdateToolbar();

	void ExitWorkbench();

	void Connect();
	void Connect(string configurationName);
	bool Disconnect();
	bool UnloadSchema();
	bool IsConnected{get;}

	int WorkflowFormsCount{get;}

	void ProcessGuiLink(IOrigamForm sourceForm, object linkTarget, Hashtable parameters);

	void ExportToExcel(string name, ArrayList list);

	bool PopulateEmptyDatabaseOnLoad { get; set; }

	void OpenForm(object owner,Hashtable parameters);
	void UpdateTitle();
}