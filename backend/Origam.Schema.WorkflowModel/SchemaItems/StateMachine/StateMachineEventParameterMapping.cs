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

using Origam.DA.Common;
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.WorkflowModel;
public enum WorkflowEntityParameterMappingType
{
	Original,
	Current,
	ChangedFlag
}
/// <summary>
/// Summary description for ContextStoreLink.
/// </summary>
[SchemaItemDescription("Parameter Mapping", "Parameter Mappings", "parameter-blm.png")]
[HelpTopic("Data+Event+Parameter+Mapping")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.1")]
public class StateMachineEventParameterMapping : AbstractSchemaItem
{
	public const string CategoryConst = "WorkflowEntityParameterMapping";
	public StateMachineEventParameterMapping() : base() {}
	public StateMachineEventParameterMapping(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public StateMachineEventParameterMapping(Key primaryKey) : base(primaryKey)	{}
	#region Overriden AbstractSchemaItem Members
	
	public override string ItemType => CategoryConst;
	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
		dependencies.Add(this.Field);
		dependencies.Add(this.ContextStore);
		base.GetExtraDependencies (dependencies);
	}
	public override SchemaItemCollection ChildItems => new SchemaItemCollection();
	#endregion
	#region Properties
	[XmlAttribute ("wfParameterType")]
	public WorkflowEntityParameterMappingType Type { get; set; } = 
		WorkflowEntityParameterMappingType.Current;
	
	public Guid FieldId;
	[TypeConverter(typeof(StateMachineAllFieldConverter))]
	[XmlReference("field", "FieldId")]
	public IDataEntityColumn Field
	{
		get
		{
			ModelElementKey key = new ModelElementKey(this.FieldId);
			return (IDataEntityColumn)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
		}
		set => this.FieldId = (Guid)value.PrimaryKey["Id"];
	}
	
	public Guid ContextStoreId;
	[TypeConverter(typeof(StateMachineEventParameterMappingContextStoreConverter))]
	[XmlReference("contextStore", "ContextStoreId")]
	public IContextStore ContextStore
	{
		get
		{
			ModelElementKey key = new ModelElementKey(this.ContextStoreId);
			return (IContextStore)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
		}
		set
		{
			this.ContextStoreId = (Guid)value.PrimaryKey["Id"];
			this.Name = (this.ContextStore == null ? "" : this.ContextStore.Name);
		}
	}
	#endregion
}
