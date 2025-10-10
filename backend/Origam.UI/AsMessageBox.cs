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
using System.Windows.Forms;

namespace Origam.UI;

/// <summary>
/// Summary description for AsMessageBox.
/// </summary>
public class AsMessageBox
{
    public static void ShowError(
        IWin32Window owner,
        string text,
        string caption,
        Exception exception
    )
    {
        ASMessageBoxForm frm = new ASMessageBoxForm(
            text,
            caption,
            MessageBoxButtons.OK,
            MessageBoxIcon.Error,
            exception,
            _debugInfoProvider
        );
        frm.ShowDialog(owner);
    }

    private static IDebugInfoProvider _debugInfoProvider = null;
    public static IDebugInfoProvider DebugInfoProvider
    {
        get { return _debugInfoProvider; }
        set { _debugInfoProvider = value; }
    }
}
