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

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Summary description for ForeachWorkflowBlock.
/// </summary>
[SchemaItemDescription(name: "(Block) Loop", folderName: "Tasks", iconName: "block-loop-1.png")]
[HelpTopic(topic: "Loop+Block")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class LoopWorkflowBlock : AbstractWorkflowBlock
{
    public LoopWorkflowBlock()
        : base() { }

    public LoopWorkflowBlock(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public LoopWorkflowBlock(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden ISchemaItem Members
    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        XsltDependencyHelper.GetDependencies(
            item: this,
            dependencies: dependencies,
            text: this.LoopConditionXPath
        );
        dependencies.Add(item: this.LoopConditionContextStore);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (item.OldPrimaryKey.Equals(obj: this.LoopConditionContextStore.PrimaryKey))
                {
                    this.LoopConditionContextStore = item as IContextStore;
                    break;
                }
            }
        }
        base.UpdateReferences();
    }
    #endregion
    #region Properties
    public Guid ContextStoreId;

    [TypeConverter(type: typeof(ContextStoreConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "loopConditionContextStore", idField: "ContextStoreId")]
    public IContextStore LoopConditionContextStore
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.ContextStoreId;
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
                this.ContextStoreId = Guid.Empty;
            }
            else
            {
                this.ContextStoreId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }
    string _xpath;

    [StringNotEmptyModelElementRule()]
    [XmlAttribute(attributeName: "loopConditionXPath")]
    public string LoopConditionXPath
    {
        get { return _xpath; }
        set { _xpath = value; }
    }
    #endregion
}
