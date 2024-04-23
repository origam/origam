#region license
/*
Copyright 2005 - 2022 Advantage Solutions, s. r. o.

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
using Origam.Service.Core;

namespace Origam.Workflow.Tasks;

public class AcceptContextStoreChangesEngineTask 
    : AbstractWorkflowEngineTask
{
    protected override void OnExecute()
    {
            var acceptContextStoreChangesTask = Step as AcceptContextStoreChangesTask;
            if(!(Engine.RuleEngine.GetContext(
                   acceptContextStoreChangesTask.ContextStore) 
                   is IDataDocument dataDocument))
            {
                throw new ArgumentException(
                    @"Context store doesn't implement IDataDocument interface.",
                    $"ContextStore:{acceptContextStoreChangesTask.ContextStore.Name}");
            }
            dataDocument.DataSet.AcceptChanges();
        }
}