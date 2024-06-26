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
/// The base functionalty all view contents must provide
/// </summary>
public interface IBaseViewContent : IDisposable
{
	/// <summary>
	/// The workbench window in which this view is displayed.
	/// </summary>
	IWorkbenchWindow  WorkbenchWindow 
	{
		get;
		set;
	}
	
	/// <summary>
	/// The text on the tab page when more than one view content
	/// is attached to a single window.
	/// </summary>
	string TabPageText 
	{
		get;
	}
	
	/// <summary>
	/// Is called when the window is switched to.
	/// -> Inside the tab (Called before Selected())
	/// -> Inside the workbench.
	/// </summary>
	void SwitchedTo();
	
	/// <summary>
	/// Is called when the view content is selected inside the window
	/// tab. NOT when the windows is selected.
	/// </summary>
	void Selected();
	
	/// <summary>
	/// Is called when the view content is deselected inside the window
	/// tab before the other window is selected. NOT when the windows is deselected.
	/// </summary>
	void Deselected();
	
	/// <summary>
	/// Reinitializes the content. (Re-initializes all add-in tree stuff)
	/// and redraws the content. Call this not directly unless you know
	/// what you do.
	/// </summary>
	void RedrawContent();
}
