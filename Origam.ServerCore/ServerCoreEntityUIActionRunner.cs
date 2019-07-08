#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

using Origam.Gui;
using Origam.Schema.GuiModel;
using Origam.Server;
using System;

namespace Origam.ServerCore
{
    public class ServerCoreEntityUIActionRunner : EntityUIActionRunner
    {
        private readonly SessionManager sessionManager;
        public ServerCoreEntityUIActionRunner(
            IEntityUIActionRunnerClient actionRunnerClient,
            SessionManager sessionManager) 
            : base(actionRunnerClient)
        {
            this.sessionManager = sessionManager;
        }

        protected override void ExecuteOpenFormAction(
            ExecuteActionProcessData processData)
        {
            throw new NotImplementedException();
        }

        protected override void SetTransactionId(
            ExecuteActionProcessData processData, string transactionId)
        {
            sessionManager.GetSession(processData).TransationId = transactionId;
        }

        protected override void PerformAppropriateAction(
            ExecuteActionProcessData processData)
        {
            switch (processData.Type)
            {
                case PanelActionType.QueueAction:
                    throw new NotImplementedException();
                case PanelActionType.Report:
                    throw new NotImplementedException();
                case PanelActionType.Workflow:
                    ExecuteWorkflowAction(processData);
                    break;
                case PanelActionType.ChangeUI:
                    throw new NotImplementedException();
                case PanelActionType.OpenForm:
                    throw new NotImplementedException();
                case PanelActionType.SelectionDialogAction:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

    }
}
