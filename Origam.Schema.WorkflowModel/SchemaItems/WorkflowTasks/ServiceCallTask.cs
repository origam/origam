using System;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for ServiceCallTask.
	/// </summary>
	public class ServiceCallTask : WorkflowTask
	{
		public ServiceCallTask() : base() {}

		public ServiceCallTask(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public ServiceCallTask(Key primaryKey) : base(primaryKey)	{}
	}
}
