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
/// The IWorkbenchWindow is the basic interface to a window which
/// shows a view (represented by the IViewContent object).
/// </summary>
public interface IWorkbenchWindow
{
    /// <summary>
    /// The window title.
    /// </summary>
    string Title { get; set; }

    /// <summary>
    /// The current view content which is shown inside this window.
    /// </summary>
    IViewContent ViewContent { get; }

    IBaseViewContent ActiveViewContent { get; }

    /// <summary>
    /// Closes the window, if force == true it closes the window
    /// without ask, even the content is dirty.
    /// </summary>
    /// <returns>true, if window is closed</returns>
    bool CloseWindow(bool force);

    /// <summary>
    /// Brings this window to front and sets the user focus to this
    /// window.
    /// </summary>
    void SelectWindow();

    void RedrawContent();

    void SwitchView(int viewNumber);

    //		void OnWindowSelected(EventArgs e);
    /// <summary>
    /// Only for internal use.
    /// </summary>
    void OnWindowSelected(EventArgs e);
    void OnWindowDeselected(EventArgs e);

    void AttachSecondaryViewContent(ISecondaryViewContent secondaryViewContent);

    /// <summary>
    /// Is called when the window is selected.
    /// </summary>
    event EventHandler WindowSelected;

    /// <summary>
    /// Is called when the window is deselected.
    /// </summary>
    event EventHandler WindowDeselected;

    /// <summary>
    /// Is called when the title of this window has changed.
    /// </summary>
    event EventHandler TitleChanged;

    /// <summary>
    /// Is called after the window closes.
    /// </summary>
    event EventHandler CloseEvent;
}
