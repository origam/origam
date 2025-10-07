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
/// The IPadContent interface is the basic interface to all "tool" windows
/// in ORIGAM Workbench.
/// </summary>
public interface IPadContent : IDisposable
{
    /// <summary>
    /// Returns the title of the pad.
    /// </summary>
    string Title { get; }

    /// <summary>
    /// Returns the icon bitmap resource name of the pad. May be null, if the pad has no
    /// icon defined.
    /// </summary>
    string IconResource { get; }

    /// <summary>
    /// Returns the category (this is used for defining where the menu item to
    /// this pad goes)
    /// </summary>
    string Category { get; set; }

    /// <summary>
    /// Returns the menu shortcut for the view menu item.
    /// </summary>
    string[] Shortcut { get; set; }

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
    void Close();
}
