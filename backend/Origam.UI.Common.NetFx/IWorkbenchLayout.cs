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
/// <summary>
/// The IWorkbenchLayout object is responsible for the layout of 
/// the workspace, it shows the contents, chooses the IWorkbenchWindow
/// implementation etc. it could be attached/detached at the runtime
/// to a workbench.
/// </summary>
public interface IWorkbenchLayout
{
	/// <summary>
	/// The active workbench window.
	/// </summary>
	IWorkbenchWindow ActiveWorkbenchwindow 
	{
		get;
	}
	
	/// <summary>
	/// Attaches this layout manager to a workbench object.
	/// </summary>
	void Attach(IWorkbench workbench);
	
	/// <summary>
	/// Detaches this layout manager from the current workspace.
	/// </summary>
	void Detach();
	
	/// <summary>
	/// Shows a new <see cref="IPadContent"/>.
	/// </summary>
	void ShowPad(IPadContent content);
	
	/// <summary>
	/// Activates a pad (Show only makes it visible but Activate does
	/// bring it to foreground)
	/// </summary>
	void ActivatePad(IPadContent content);
	
	/// <summary>
	/// Hides a new <see cref="IPadContent"/>.
	/// </summary>
	void HidePad(IPadContent content);
	
	/// <summary>
	/// returns true, if padContent is visible;
	/// </summary>
	bool IsVisible(IPadContent padContent);
	
	/// <summary>
	/// Re-initializes all components of the layout manager.
	/// </summary>
	void RedrawAllComponents();
	
	/// <summary>
	/// Shows a new <see cref="IViewContent"/>.
	/// </summary>
	IWorkbenchWindow ShowView(IViewContent content);
	
	/// <summary>
	/// Is called, when the workbench window which the user has into
	/// the foreground (e.g. editable) changed to a new one.
	/// </summary>
	event EventHandler ActiveWorkbenchWindowChanged;
	
	// only needed in the workspace window when the 'secondary view content' changed
	// it is somewhat like 'active workbench window changed'
	void OnActiveWorkbenchWindowChanged(EventArgs e);
}
