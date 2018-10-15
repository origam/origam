using System;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for AssemblyService.
	/// </summary>
	public class AssemblyService : AbstractService
	{
		public AssemblyService() : base() {}

		public AssemblyService(Guid schemaVersionId, Guid schemaExtensionId) : base(schemaVersionId, schemaExtensionId) {}

		public AssemblyService(Key primaryKey) : base(primaryKey)	{}

		private string _assembly = "";
		[Category("Assembly")]
		[EntityColumn("Assembly", "AssemblyService")]
		public string Assembly
		{
			get
			{
				return _assembly;
			}
			set
			{
				_assembly = value;
			}
		}

		private string _type = "";
		[Category("Assembly")]
		[EntityColumn("Type", "AssemblyService")]
		public string Type
		{
			get
			{
				return _type;
			}
			set
			{
				_type = value;
			}
		}

		private string _namespace = "";
		[Category("Assembly")]
		[EntityColumn("Namespace", "AssemblyService")]
		public string Namespace
		{
			get
			{
				return _namespace;
			}
			set
			{
				_namespace = value;
			}
		}

		public override string Icon
		{
			get
			{
				return "3";
			}
		}

	}
}
