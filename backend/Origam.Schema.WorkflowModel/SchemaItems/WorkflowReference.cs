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
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.ItemCollection;

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Summary description for RuleReference.
/// </summary>
[SchemaItemDescription(
    name: "Sequential Workflow Reference",
    iconName: "icon_sequential-workflow-reference.png"
)]
[HelpTopic(topic: "Sequential+Workflow+Reference")]
[DefaultProperty(name: "Workflow")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class WorkflowReference : AbstractSchemaItem
{
    public const string CategoryConst = "WorkflowReference";

    public WorkflowReference()
        : base() { }

    public WorkflowReference(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public WorkflowReference(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType => CategoryConst;

    public override void GetParameterReferences(
        ISchemaItem parentItem,
        Dictionary<string, ParameterReference> list
    )
    {
        if (this.Workflow != null)
        {
            base.GetParameterReferences(parentItem: Workflow, list: list);
        }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Workflow);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override ISchemaItemCollection ChildItems => SchemaItemCollection.Create();
    #endregion
    #region Properties
    public Guid WorkflowId;

    [Category(category: "Reference")]
    [TypeConverter(type: typeof(WorkflowConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "workflow", idField: "WorkflowId")]
    public Workflow Workflow
    {
        get =>
            (ISchemaItem)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.WorkflowId)
                ) as Workflow;
        set => this.WorkflowId = (Guid)value.PrimaryKey[key: "Id"];
    }
    #endregion
}
