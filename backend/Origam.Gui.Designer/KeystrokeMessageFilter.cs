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

using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

/// This filter is used to catch keyboard input that is meant for the designer.
/// It does not prevent the message from continuing, but instead merely
/// deciphers the keystroke and performs the appropriate MenuCommand.
public class KeystrokeMessageFilter : System.Windows.Forms.IMessageFilter
{
    private IDesignerHost host;

    public KeystrokeMessageFilter(IDesignerHost host)
    {
        this.host = host;
    }

    #region Implementation of IMessageFilter
    public bool PreFilterMessage(ref Message m)
    {
        // Catch WM_KEYCHAR if the designerView has focus
        if (
            (m.Msg == 0x0100)
            && ((host as DesignerHostImpl).ParentControl as ControlSetEditor).IsDesignerHostFocused
        )
        {
            IMenuCommandService mcs =
                host.GetService(typeof(IMenuCommandService)) as IMenuCommandService;
            // WM_KEYCHAR only tells us the last key pressed. Thus we check
            // Control for modifier keys (Control, Shift, etc.)
            //
            switch (((int)m.WParam) | ((int)Control.ModifierKeys))
            {
                case (int)Keys.Up:
                    mcs.GlobalInvoke(MenuCommands.KeyMoveUp);
                    break;
                case (int)Keys.Down:
                    mcs.GlobalInvoke(MenuCommands.KeyMoveDown);
                    break;
                case (int)Keys.Right:
                    mcs.GlobalInvoke(MenuCommands.KeyMoveRight);
                    break;
                case (int)Keys.Left:
                    mcs.GlobalInvoke(MenuCommands.KeyMoveLeft);
                    break;
                case (int)(Keys.Control | Keys.Up):
                    mcs.GlobalInvoke(MenuCommands.KeyNudgeUp);
                    break;
                case (int)(Keys.Control | Keys.Down):
                    mcs.GlobalInvoke(MenuCommands.KeyNudgeDown);
                    break;
                case (int)(Keys.Control | Keys.Right):
                    mcs.GlobalInvoke(MenuCommands.KeyNudgeRight);
                    break;
                case (int)(Keys.Control | Keys.Left):
                    mcs.GlobalInvoke(MenuCommands.KeyNudgeLeft);
                    break;
                case (int)(Keys.Shift | Keys.Up):
                    mcs.GlobalInvoke(MenuCommands.KeySizeHeightDecrease);
                    break;
                case (int)(Keys.Shift | Keys.Down):
                    mcs.GlobalInvoke(MenuCommands.KeySizeHeightIncrease);
                    break;
                case (int)(Keys.Shift | Keys.Right):
                    mcs.GlobalInvoke(MenuCommands.KeySizeWidthIncrease);
                    break;
                case (int)(Keys.Shift | Keys.Left):
                    mcs.GlobalInvoke(MenuCommands.KeySizeWidthDecrease);
                    break;
                case (int)(Keys.Control | Keys.Shift | Keys.Up):
                    mcs.GlobalInvoke(MenuCommands.KeyNudgeHeightIncrease);
                    break;
                case (int)(Keys.Control | Keys.Shift | Keys.Down):
                    mcs.GlobalInvoke(MenuCommands.KeyNudgeHeightDecrease);
                    break;
                case (int)(Keys.Control | Keys.Shift | Keys.Right):
                    mcs.GlobalInvoke(MenuCommands.KeyNudgeWidthIncrease);
                    break;
                case (int)(Keys.ControlKey | Keys.Shift | Keys.Left):
                    mcs.GlobalInvoke(MenuCommands.KeyNudgeWidthDecrease);
                    break;
                case (int)(Keys.Escape):
                    mcs.GlobalInvoke(MenuCommands.KeyCancel);
                    break;
                case (int)(Keys.Shift | Keys.Escape):
                    mcs.GlobalInvoke(MenuCommands.KeyReverseCancel);
                    break;
                case (int)(Keys.Enter):
                    mcs.GlobalInvoke(MenuCommands.KeyDefaultAction);
                    break;
            }
        }
        // Never filter the message
        return false;
    }
    #endregion
}
