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
public enum ContextStoreLinkDirection
{
	Input,
	Output,
	Return
}
/// <summary>
/// Summary description for ContextStoreLink.
/// </summary>
[SchemaItemDescription("Context Mapping", "Context Mappings", "context-mapping.png")]
[HelpTopic("Workflow+Call+Context+Mapping")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class ContextStoreLink : AbstractSchemaItem
{
	public const string CategoryConst = "ContextStoreLink";
	public ContextStoreLink() : base() {}
	public ContextStoreLink(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public ContextStoreLink(Key primaryKey) : base(primaryKey)	{}
	#region Overriden ISchemaItem Members
	public override string ItemType => CategoryConst;
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		XsltDependencyHelper.GetDependencies(this, dependencies, this.XPath);
		dependencies.Add(this.CallerContextStore);
		dependencies.Add(this.TargetContextStore);
		base.GetExtraDependencies (dependencies);
	}
	public override void UpdateReferences()
	{
		foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
		{
			if(item.OldPrimaryKey != null)
			{
				if(item.OldPrimaryKey.Equals(this.CallerContextStore.PrimaryKey))
				{
					this.CallerContextStore = item as IContextStore;
					break;
				}
			}
		}
		base.UpdateReferences ();
	}
	public override ISchemaItemCollection ChildItems => SchemaItemCollection.Create();
	#endregion
	#region Properties
	[XmlAttribute ("direction")]
	public ContextStoreLinkDirection Direction { get; set; } = ContextStoreLinkDirection.Input;
	public Guid CallerContextStoreId;
	[TypeConverter(typeof(ContextStoreConverter))]
	[XmlReference("callerContextStore", "CallerContextStoreId")]
	public IContextStore CallerContextStore
	{
		get
		{
			ModelElementKey key = new ModelElementKey();
			key.Id = this.CallerContextStoreId;
			return (IContextStore)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), key);
		}
		set
		{
			if(value == null)
			{
				this.CallerContextStoreId = Guid.Empty;
			}
			else
			{
				this.CallerContextStoreId = (Guid)value.PrimaryKey["Id"];
			}
		}
	}
	
	public Guid TargetContextStoreId;
	[TypeConverter(typeof(WorkflowCallTargetContextStoreConverter))]
	[XmlReference("targetContextStore", "TargetContextStoreId")]
	public IContextStore TargetContextStore
	{
		get
		{
			ModelElementKey key = new ModelElementKey();
			key.Id = this.TargetContextStoreId;
			return (IContextStore)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), key);
		}
		set
		{
			if(value == null)
			{
				this.TargetContextStoreId = Guid.Empty;
			}
			else
			{
				this.TargetContextStoreId = (Guid)value.PrimaryKey["Id"];
			}
		}
	}
	
    [DefaultValue("/")]
	[XmlAttribute ("xPath")]
	public string XPath { get; set; } = "/";
	#endregion
}
