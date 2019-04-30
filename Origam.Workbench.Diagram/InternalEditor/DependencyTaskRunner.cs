using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;

namespace Origam.Workbench.Diagram.InternalEditor
{
    class DependencyTaskRunner
    {
        private readonly List<IDefferedPersistenceTask> deferedTasks = new List<IDefferedPersistenceTask>();
        private readonly WorkbenchSchemaService schemaService;
        private readonly IPersistenceProvider persistenceProvider;

        public DependencyTaskRunner(IPersistenceProvider persistenceProvider, WorkbenchSchemaService schemaService)
        {
            this.persistenceProvider = persistenceProvider;
            this.schemaService = schemaService;
        }

        public void AddDependencyTask(IWorkflowStep independentItem,
            AbstractSchemaItem dependentItem, Guid triggerItemId)
        {
            deferedTasks.Add(new AddDependencyTask(
                persistenceProvider, schemaService,
                independentItem, dependentItem, triggerItemId));
        }

        public void RemoveDepedenceyTask(WorkflowTaskDependency dependency, Guid triggerItemId)
        {
            deferedTasks.Add(new RemoveDependencyTask(
                schemaService, dependency, triggerItemId));
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
		private readonly WorkbenchSchemaService schemaService;
		private readonly WorkflowTaskDependency dependency;
		private readonly Guid triggerItemId;

		public RemoveDependencyTask(WorkbenchSchemaService schemaService,
			WorkflowTaskDependency dependency, Guid triggerItemId)
		{
			this.schemaService = schemaService;
			this.dependency = dependency;
			this.triggerItemId = triggerItemId;
		}

		public bool TryRun(AbstractSchemaItem persistedItem)
		{
			if (persistedItem.Id == triggerItemId)
			{
                dependency.Delete();
                schemaService.SchemaBrowser.EbrSchemaBrowser.RefreshItem(dependency.ParentItem);
                return true;			
			}
			return true;
		}
	}
	
	internal class AddDependencyTask: IDefferedPersistenceTask
	{
		private readonly IPersistenceProvider persistenceProvider;
		private readonly WorkbenchSchemaService schemaService;
		private readonly IWorkflowStep independentItem;
		private readonly AbstractSchemaItem dependentItem;
		private readonly Guid triggerItemId;

		public AddDependencyTask(IPersistenceProvider persistenceProvider, 
			WorkbenchSchemaService schemaService, IWorkflowStep independentItem,
			AbstractSchemaItem dependentItem, Guid triggerItemId)
		{
			this.persistenceProvider = persistenceProvider;
			this.schemaService = schemaService;
			this.independentItem = independentItem;
			this.dependentItem = dependentItem;
			this.triggerItemId = triggerItemId;
		}

		public bool TryRun(AbstractSchemaItem persistedItem)
		{
			if (triggerItemId != persistedItem.Id) return false;			
			var workflowTaskDependency = new WorkflowTaskDependency
			{
				SchemaExtensionId = dependentItem.SchemaExtensionId,
				PersistenceProvider = persistenceProvider,
				ParentItem = dependentItem,
				Task = independentItem
			};
			workflowTaskDependency.Persist();
			schemaService.SchemaBrowser.EbrSchemaBrowser.RefreshItem(dependentItem);
			return true;
		}
	}
	
}