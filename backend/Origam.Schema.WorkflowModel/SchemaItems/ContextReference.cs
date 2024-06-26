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
/// Summary description for ContextReference.
/// </summary>
[SchemaItemDescription("Context Store Reference", "Parameters", "context-store-reference.png")]
[HelpTopic("Context+Store+Reference")]
[DefaultProperty("ContextStore")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class ContextReference : AbstractSchemaItem, IContextReference
{
	public const string CategoryConst = "WorkflowContextReference";
	public ContextReference() : base() {}
	public ContextReference(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public ContextReference(Key primaryKey) : base(primaryKey)	{}
	#region Overriden AbstractSchemaItem Members
	
	public override string ItemType => CategoryConst;
	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
		XsltDependencyHelper.GetDependencies(this, dependencies, this.XPath);
		dependencies.Add(this.ContextStore);
		base.GetExtraDependencies (dependencies);
	}
	public override void UpdateReferences()
	{
		foreach(ISchemaItem item in this.RootItem.ChildItemsRecursive)
		{
			if(item.OldPrimaryKey != null)
			{
				if(item.OldPrimaryKey.Equals(this.ContextStore.PrimaryKey))
				{
					this.ContextStore = item as IContextStore;
					break;
				}
			}
		}
		base.UpdateReferences ();
	}
	public override bool CanMove(Origam.UI.IBrowserNode2 newNode) => newNode == this.ParentItem;
	#endregion
	#region IContextReference Members
	
	public Guid ContextStoreId;
	[TypeConverter(typeof(ContextStoreConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
	[XmlReference("contextStore", "ContextStoreId")]
	public IContextStore ContextStore
	{
		get
		{
			ModelElementKey key = new ModelElementKey();
			key.Id = this.ContextStoreId;
			return (IContextStore)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
		}
		set
		{
			if(value == null)
			{
				this.ContextStoreId = Guid.Empty;
				this.Name = "";
			}
			else
			{
				this.ContextStoreId = (Guid)value.PrimaryKey["Id"];
				if(this.Name == "NewContextReference")
                {
					this.Name = this.ContextStore.Name;
				}
			}
		}
	}
	
	[NotNullModelElementRule()]
	[DefaultValue("/")]
	[XmlAttribute ("xPath")]
	public string XPath { get; set; } = "/";
	
	[DefaultValue(OrigamDataType.String)]
	[Description("Select a data type which will be used to convert a value to. Very handy to use e.g. when called service accepts an array.")]
	[XmlAttribute ("castToDataType")]
	public OrigamDataType CastToDataType { get; set; } = OrigamDataType.String;
	#endregion
}
