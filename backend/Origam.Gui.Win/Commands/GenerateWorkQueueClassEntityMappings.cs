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
using Origam.Schema.WorkflowModel;
using Origam.UI;
using Origam.Workbench;

namespace Origam.Gui.Win.Commands;
public class GenerateWorkQueueClassEntityMappings : AbstractMenuCommand
{
    SchemaBrowser _schemaBrowser = WorkbenchSingleton.Workbench.GetPad(typeof(SchemaBrowser)) as SchemaBrowser;
    public override bool IsEnabled
    {
        get
        {
            return Owner is WorkQueueClass;
        }
        set
        {
            throw new ArgumentException("Cannot set this property", "IsEnabled");
        }
    }
    public override void Run()
    {
        Origam.Schema.WorkflowModel.WorkflowHelper.GenerateWorkQueueClassEntityMappings(
            Owner as WorkQueueClass);
    }
    public override int GetImageIndex(string icon)
    {
        return _schemaBrowser.ImageIndex(icon);
    }
}
