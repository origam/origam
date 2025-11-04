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

namespace Origam.Gui.Designer;

using System;
using System.ComponentModel;

// This class represents a single selected object.
internal class SelectionItem
{
    // Public objects this selection deals with
    private IComponent component; // the component that's selected
    private SelectionServiceImpl selectionMgr; // host interface
    private bool primary; // is this the primary selection?

    ///  Constructor
    internal SelectionItem(SelectionServiceImpl selectionMgr, IComponent component)
    {
        this.component = component;
        this.selectionMgr = selectionMgr;
    }

    internal IComponent Component
    {
        get { return component; }
    }

    ///     Determines if this is the primary selection.  The primary selection uses a
    ///     different set of grab handles and generally supports sizing. The caller must
    ///     verify that there is only one primary object; this merely updates the
    ///     UI.
    internal virtual bool Primary
    {
        get { return primary; }
        set
        {
            if (this.primary != value)
            {
                this.primary = value;
                if (SelectionItemInvalidate != null)
                {
                    SelectionItemInvalidate(this, EventArgs.Empty);
                }
            }
        }
    }
    internal event EventHandler SelectionItemDispose;
    internal event EventHandler SelectionItemInvalidate;

    ///     Disposes of this selection.  We dispose of our region object if it still exists and we
    ///     invalidate our UI so that we don't leave any turds laying around.
    internal virtual void Dispose()
    {
        if (primary)
        {
            selectionMgr.SetPrimarySelection((SelectionItem)null);
        }
        if (SelectionItemDispose != null)
        {
            SelectionItemDispose(this, EventArgs.Empty);
        }
    }
}
