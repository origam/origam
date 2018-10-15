using System;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for WebService.
	/// </summary>
	public class WebService : AbstractService
	{
		public WebService() : base() {}

		public WebService(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public WebService(Key primaryKey) : base(primaryKey)	{}

		public override string Icon
		{
			get
			{
				return "3";
			}
		}

	}
}
