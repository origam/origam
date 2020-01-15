#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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

using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;

namespace Origam.Schema.WorkflowModel
{
	/// <summary>
	/// Summary description for RuleReference.
	/// </summary>
	[SchemaItemDescription("Sequential Workflow Reference", "icon_sequential-workflow-reference.png")]
    [HelpTopic("Sequential+Workflow+Reference")]
    [DefaultProperty("Workflow")]
	[XmlModelRoot(ItemTypeConst)]
	public class WorkflowReference : AbstractSchemaItem
	{
		public const string ItemTypeConst = "WorkflowReference";

		public WorkflowReference() : base() {}

		public WorkflowReference(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public WorkflowReference(Key primaryKey) : base(primaryKey)	{}
	
		#region Overriden AbstractDataEntityColumn Members
		
		[EntityColumn("ItemType")]
		public override string ItemType => ItemTypeConst;

		public override void GetParameterReferences(AbstractSchemaItem parentItem, System.Collections.Hashtable list)
		{
			if(this.Workflow != null)
				base.GetParameterReferences(this.Workflow as AbstractSchemaItem, list);
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Workflow);

			base.GetExtraDependencies (dependencies);
		}

		public override SchemaItemCollection ChildItems => new SchemaItemCollection();
		#endregion

		#region Properties
		[EntityColumn("G01")]  
		public Guid WorkflowId;

		[Category("Reference")]
		[TypeConverter(typeof(WorkflowConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("workflow", "WorkflowId")]
		public Workflow Workflow
		{
			get => (AbstractSchemaItem)this.PersistenceProvider
				.RetrieveInstance(typeof(AbstractSchemaItem)
					, new ModelElementKey(this.WorkflowId)) as Workflow;
			
			set => this.WorkflowId = (Guid)value.PrimaryKey["Id"];
		}
		#endregion
	}
}
