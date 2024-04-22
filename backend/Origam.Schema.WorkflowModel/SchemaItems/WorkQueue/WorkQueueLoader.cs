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

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Summary description for WorkqueueLoader.
/// </summary>
[SchemaItemDescription("Loader", "Loaders", "loader-3.png")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class WorkqueueLoader : AbstractSchemaItem
{
	public const string CategoryConst = "WorkqueueLoader";

	public WorkqueueLoader() : base() {Init();}

	public WorkqueueLoader(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}

	public WorkqueueLoader(Key primaryKey) : base(primaryKey)	{Init();}
	
	private void Init()
	{
		}

	#region Overriden AbstractDataEntityColumn Members
		
	public override string ItemType
	{
		get
		{
				return CategoryConst;
			}
	}

	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
			dependencies.Add(this.Workflow);

			base.GetExtraDependencies (dependencies);
		}

	#endregion

	#region Properties
	public Guid WorkflowId;

	[Category("References")]
	[TypeConverter(typeof(WorkflowConverter)), NotNullModelElementRule()]
	[XmlReference("workflow", "WorkflowId")]
	public IWorkflow Workflow
	{
		get
		{
				return (IWorkflow)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.WorkflowId));
			}
		set
		{
				this.WorkflowId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
	}
	#endregion
}