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
using Origam.Gui;
using Origam.Schema.MenuModel;
using Origam.Workbench;

namespace Origam.Gui.Win;

public class DesktopEntityUIActionRunner : EntityUIActionRunner
{
    public DesktopEntityUIActionRunner(IEntityUIActionRunnerClient actionRunnerClient)
        : base(actionRunnerClient) { }

    protected override void ExecuteOpenFormAction(ExecuteActionProcessData processData)
    {
        object menuItem;
        switch (processData.Action)
        {
            case EntityMenuAction action:
                menuItem = action.Menu;
                break;
            case EntityWorkflowAction action:
                menuItem = action.Workflow;
                break;
            case EntityReportAction action:
                menuItem = action.Report;
                break;
            default:
                throw new NotImplementedException(
                    $"Cannot execule action type {processData.Action.GetType()}"
                );
        }
        WorkbenchSingleton.Workbench.OpenForm(menuItem, processData.Parameters);
    }

    protected override void SetTransactionId(
        ExecuteActionProcessData processData,
        string transactionId
    ) { }
}
