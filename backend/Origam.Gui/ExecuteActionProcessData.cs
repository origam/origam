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

using System.Collections;
using System.Collections.Generic;
using System.Data;
using Origam;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Gui;
public class ExecuteActionProcessData
{
    public string SessionFormIdentifier { get; set; }
    public string RequestingGrid { get; set; }
    public string ActionId { get; set; }
    public string Entity { get; set; }
    public List<string> SelectedIds { get; set; }
    public PanelActionType Type { get; set; }
    public UserProfile Profile { get; } = new UserProfile();
    public DataTable DataTable { get; set; }
    public IList<DataRow> SelectedRows { get; set; }
    public IParameterService ParameterService { get; set; }
    public EntityUIAction Action { get; set; } = null;
    public Hashtable Parameters { get; set; } = new Hashtable();
    public bool IsModalDialog { get; set; }
}
