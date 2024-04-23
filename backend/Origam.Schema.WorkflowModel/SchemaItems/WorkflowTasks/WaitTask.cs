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
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Summary description for WorkflowCallTask.
/// </summary>
[SchemaItemDescription("(Task) Wait", "Tasks", "task-wait-2.png")]
[HelpTopic("Wait+Task")]
[ClassMetaVersion("6.0.0")]
public class WaitTask : AbstractWorkflowStep
{
	public WaitTask() : base() {}

	public WaitTask(Guid schemaExtensionId) : base(schemaExtensionId) {}

	public WaitTask(Key primaryKey) : base(primaryKey)	{}

	public override string Icon
	{
		get
		{
				return "16";
			}
	}

	#region Properties
	public Guid WaitTimeDataConstantId;

	[Category("Reference")]
	[TypeConverter(typeof(DataConstantConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
	[Description("Number of milliseconds to wait.")]
	[XmlReference("waitTime", "WaitTimeDataConstantId")]
	public DataConstant WaitTime
	{
		get
		{
				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.WaitTimeDataConstantId)) as DataConstant;
			}
		set
		{
				this.WaitTimeDataConstantId = (Guid)value.PrimaryKey["Id"];
			}
	}
	#endregion
}