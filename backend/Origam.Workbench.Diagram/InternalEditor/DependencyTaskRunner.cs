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
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.WorkflowModel;

namespace Origam.Workbench.Diagram.InternalEditor;
class DependencyTaskRunner
{
    private readonly List<IDefferedPersistenceTask> deferedTasks = new List<IDefferedPersistenceTask>();
    private readonly IPersistenceProvider persistenceProvider;
    public DependencyTaskRunner(IPersistenceProvider persistenceProvider)
    {
        this.persistenceProvider = persistenceProvider;
    }
    public void AddDependencyTask(IWorkflowStep independentItem,
        AbstractSchemaItem dependentItem, Guid triggerItemId)
    {
        deferedTasks.Add(new AddDependencyTask(
            persistenceProvider, independentItem, dependentItem, triggerItemId));
    }
    public void RemoveDependencyTask(WorkflowTaskDependency dependency, Guid triggerItemId)
    {
        deferedTasks.Add(new RemoveDependencyTask( dependency, triggerItemId));
    }
    internal void UpdateDependencies(AbstractSchemaItem persistedSchemaItem)
    {
        deferedTasks
            .ToArray()
            .Where(task => task.TryRun(persistedSchemaItem))
            .ForEach(task => deferedTasks.Remove(task));
    }
}


interface IDefferedPersistenceTask
{
    bool TryRun(AbstractSchemaItem persistedItem);
}

internal class RemoveDependencyTask : IDefferedPersistenceTask
{
	private readonly WorkflowTaskDependency dependency;
	private readonly Guid triggerItemId;
	public RemoveDependencyTask(
		WorkflowTaskDependency dependency, Guid triggerItemId)
	{
		this.dependency = dependency;
		this.triggerItemId = triggerItemId;
	}
	public bool TryRun(AbstractSchemaItem persistedItem)
	{
		if (persistedItem.Id == triggerItemId)
		{
            dependency.Delete();
            return true;			
		}
		return true;
	}
}

internal class AddDependencyTask: IDefferedPersistenceTask
{
	private readonly IPersistenceProvider persistenceProvider;
	private readonly IWorkflowStep independentItem;
	private readonly Guid triggerItemId;
	private readonly AbstractSchemaItem dependentItem;
	public AddDependencyTask(IPersistenceProvider persistenceProvider, 
		IWorkflowStep independentItem,
		AbstractSchemaItem dependentItem, Guid triggerItemId)
	{
		this.persistenceProvider = persistenceProvider;
		this.independentItem = independentItem;
		this.dependentItem = dependentItem;
		this.triggerItemId = triggerItemId;
	}
	public bool TryRun(AbstractSchemaItem persistedItem)
	{
		if (triggerItemId != persistedItem.Id) return false;			
		var workflowTaskDependency = new WorkflowTaskDependency
		{
			SchemaExtensionId = persistedItem.SchemaExtensionId,
			PersistenceProvider = persistenceProvider,
			ParentItem = dependentItem,
			Task = independentItem
		};
		workflowTaskDependency.Persist();
		return true;
	}
}

