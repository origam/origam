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

using Origam.UI;
using Origam.Workbench;

namespace OrigamArchitect.Commands
{
    /// <summary>
    /// Closes an active screen.
    /// </summary>
    public class CloseWindow : AbstractMenuCommand
    {
        public override bool IsEnabled
        {
            get
            {
                return WorkbenchSingleton.Workbench.ViewContentCollection.Count > 0;
            }
            set
            {
                base.IsEnabled = value;
            }
        }

        public override void Run()
        {
            WorkbenchSingleton.Workbench.CloseContent(WorkbenchSingleton.Workbench.ActiveDocument);
        }
    }

    /// <summary>
    /// Closes all open screens.
    /// </summary>
    public class CloseAllWindows : AbstractMenuCommand
    {
        public override bool IsEnabled
        {
            get
            {
                return WorkbenchSingleton.Workbench.ViewContentCollection.Count > 0;
            }
            set
            {
                base.IsEnabled = value;
            }
        }

        public override void Run()
        {
            WorkbenchSingleton.Workbench.CloseAllViews();
        }
    }

    /// <summary>
    /// Closes all open screens except of an active one.
    /// </summary>
    public class CloseAllButThis : AbstractMenuCommand
    {
        public override bool IsEnabled
        {
            get
            {
                return WorkbenchSingleton.Workbench.ViewContentCollection.Count > 0;
            }
            set
            {
                base.IsEnabled = value;
            }
        }

        public override void Run()
        {
            WorkbenchSingleton.Workbench.CloseAllViews(WorkbenchSingleton.Workbench.ActiveDocument);
        }
    }
}
