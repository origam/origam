using System;
using Origam.UI;

namespace Origam.Schema
{
	/// <summary>
	/// Summary description for Schema.
	/// </summary>
	public class SchemaExtensionOld
	{
		public SchemaExtensionOld(Guid schemaVersionGuid, Guid id, string name)
		{
			this.Id = id;
			this.Name = name;
			this.SchemaVersionGuid = schemaVersionGuid;
		}

		System.Guid mSchemaVersionGuid;
		public System.Guid SchemaVersionGuid {get {return mSchemaVersionGuid;} set {mSchemaVersionGuid = value;}}

		string _name;
		public string Name {get {return _name;} set {_name = value;}}

		public override string ToString() 
		{
			return this.Name;
		}

		public Guid Id;
	}
}
