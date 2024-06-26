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
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel;
/// <summary>
/// Summary description for EntitySecurityRule.
/// </summary>
[SchemaItemDescription("Parameter Mapping", "Parameter Mappings", "parameter-blm.png")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class WorkQueueWorkflowCommandParameterMapping : AbstractSchemaItem, IComparable
{
	public const string CategoryConst = "WorkQueueWorkflowCommandParameterMapping";
	public WorkQueueWorkflowCommandParameterMapping() : base() {}
	public WorkQueueWorkflowCommandParameterMapping(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public WorkQueueWorkflowCommandParameterMapping(Key primaryKey) : base(primaryKey)	{}

	#region Overriden AbstractDataEntityColumn Members
	
	public override string ItemType => CategoryConst;
	#endregion
	#region Properties
	[XmlAttribute ("value")]
	public WorkQueueCommandParameterMappingType Value { get; set; } = WorkQueueCommandParameterMappingType.QueueEntries;
	#endregion
}
public enum WorkQueueCommandParameterMappingType
{
	QueueEntries = 0,
	Parameter1 = 1,
	Parameter2 = 2
}
