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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.WorkflowModel;
/// <summary>
/// Summary description for ContextStore.
/// </summary>
[SchemaItemDescription("Input Mapping", "Input Mappings", "input-mapping.png")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class WorkQueueClassEntityMapping : AbstractSchemaItem, IComparable
{
	public const string CategoryConst = "WorkQueueClassEntityMapping";
	public WorkQueueClassEntityMapping() : base() {}
	public WorkQueueClassEntityMapping(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public WorkQueueClassEntityMapping(Key primaryKey) : base(primaryKey)	{}
	#region Overriden ISchemaItem Members
	
	public override string ItemType => CategoryConst;
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		XsltDependencyHelper.GetDependencies(this, dependencies, this.XPath);
		base.GetExtraDependencies (dependencies);
	}
	public override ISchemaItemCollection ChildItems => SchemaItemCollection.Create();
	public override bool CanMove(Origam.UI.IBrowserNode2 newNode) => newNode.GetType().Equals(this.ParentItem.GetType());
	#endregion
	#region Properties
//		public Guid FieldId;
//
//		[TypeConverter(typeof(WorkQueueClassEntityMappingFieldConverter))]
//		[RefreshProperties(RefreshProperties.Repaint)]
//		public IDataEntityColumn Field
//		{
//			get
//			{
//				return (IDataEntityColumn)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), new ModelElementKey(this.FieldId));
//			}
//			set
//			{
//				this.FieldId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
//			}
//		}
	[XmlAttribute ("xPath")]
	public string XPath { get; set; }
	
	[XmlAttribute ("formatPattern")]
	public string FormatPattern { get; set; }
	[Category("GUI")]
	[XmlAttribute ("sortOrder")]
	public int SortOrder { get; set; }
	#endregion
	#region IComparable Members
	public override int CompareTo(object obj)
	{
		WorkQueueClassEntityMapping compareItem = obj as WorkQueueClassEntityMapping;
		if(compareItem == null)
        {
            return base.CompareTo(obj);
        }
        else
        {
            return this.SortOrder.CompareTo(compareItem.SortOrder);
        }
	}
	#endregion
}
