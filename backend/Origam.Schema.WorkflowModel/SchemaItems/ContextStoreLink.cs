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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.WorkflowModel;

public enum ContextStoreLinkDirection
{
    Input,
    Output,
    Return,
}

/// <summary>
/// Summary description for ContextStoreLink.
/// </summary>
[SchemaItemDescription(
    name: "Context Mapping",
    folderName: "Context Mappings",
    iconName: "context-mapping.png"
)]
[HelpTopic(topic: "Workflow+Call+Context+Mapping")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class ContextStoreLink : AbstractSchemaItem
{
    public const string CategoryConst = "ContextStoreLink";

    public ContextStoreLink()
        : base() { }

    public ContextStoreLink(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public ContextStoreLink(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden ISchemaItem Members
    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        XsltDependencyHelper.GetDependencies(
            item: this,
            dependencies: dependencies,
            text: this.XPath
        );
        dependencies.Add(item: this.CallerContextStore);
        dependencies.Add(item: this.TargetContextStore);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.CallerContextStore.PrimaryKey))
                {
                    this.CallerContextStore = item as IContextStore;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }

    public override ISchemaItemCollection ChildItems => SchemaItemCollection.Create();
    #endregion
    #region Properties
    [XmlAttribute(attributeName: "direction")]
    public ContextStoreLinkDirection Direction { get; set; } = ContextStoreLinkDirection.Input;
    public Guid CallerContextStoreId;

    [TypeConverter(type: typeof(ContextStoreConverter))]
    [XmlReference(attributeName: "callerContextStore", idField: "CallerContextStoreId")]
    public IContextStore CallerContextStore
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.CallerContextStoreId;
            return (IContextStore)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                this.CallerContextStoreId = Guid.Empty;
            }
            else
            {
                this.CallerContextStoreId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }

    public Guid TargetContextStoreId;

    [TypeConverter(type: typeof(WorkflowCallTargetContextStoreConverter))]
    [XmlReference(attributeName: "targetContextStore", idField: "TargetContextStoreId")]
    public IContextStore TargetContextStore
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.TargetContextStoreId;
            return (IContextStore)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                this.TargetContextStoreId = Guid.Empty;
            }
            else
            {
                this.TargetContextStoreId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }

    [DefaultValue(value: "/")]
    [XmlAttribute(attributeName: "xPath")]
    public string XPath { get; set; } = "/";
    #endregion
}
