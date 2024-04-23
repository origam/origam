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

/// <summary>
/// Summary description for ContextStoreLink.
/// </summary>
[SchemaItemDescription("Parameter Mapping", "Parameter Mappings", 17)]
[HelpTopic("Dynamic+State+Workflow+Parameter")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class StateMachineDynamicLookupParameterMapping : AbstractSchemaItem
{
	public const string CategoryConst = "StateMachineDynamicLookupParameterMapping";

	public StateMachineDynamicLookupParameterMapping() : base() {}

	public StateMachineDynamicLookupParameterMapping(Guid schemaExtensionId) : base(schemaExtensionId) {}

	public StateMachineDynamicLookupParameterMapping(Key primaryKey) : base(primaryKey)	{}

	#region Overriden AbstractSchemaItem Members
		
	public override string ItemType
	{
		get
		{
				return CategoryConst;
			}
	}

	public override string Icon
	{
		get
		{
				return "17";
			}
	}

	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
			dependencies.Add(this.Field);

			base.GetExtraDependencies (dependencies);
		}

	public override SchemaItemCollection ChildItems
	{
		get
		{
				return new SchemaItemCollection();
			}
	}
	#endregion

	#region Properties
	WorkflowEntityParameterMappingType _type = WorkflowEntityParameterMappingType.Current;
	[XmlAttribute("type")]
	public WorkflowEntityParameterMappingType Type
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
		set
		{
				this.FieldId = (Guid)value.PrimaryKey["Id"];
			}
	}
	#endregion
}