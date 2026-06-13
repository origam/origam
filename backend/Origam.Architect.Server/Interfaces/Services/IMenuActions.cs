#region license
/*
Copyright 2005 - 2026 Advantage Solutions, s. r. o.

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

using Origam.Architect.Server.Models.Requests.Actions;
using Origam.Architect.Server.Models.Responses.Actions;

namespace Origam.Architect.Server.Interfaces.Services;

/// <summary>
/// Create a Form Reference menu item pointing to a Screen.
/// </summary>
public interface IMenuActions
{
    /// <summary>Create a menu item under the Main Menu for the given Screen.</summary>
    CreateActionResult CreateMenuItem(CreateMenuItemModel input);
}
