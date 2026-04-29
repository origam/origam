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

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Summary description for WorkqueueLoader.
/// </summary>
[SchemaItemDescription(name: "Loader", folderName: "Loaders", iconName: "loader-3.png")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class WorkqueueLoader : AbstractSchemaItem
{
    public const string CategoryConst = "WorkqueueLoader";

    public WorkqueueLoader()
        : base()
    {
        Init();
    }

    public WorkqueueLoader(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public WorkqueueLoader(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init() { }

    #region Overriden AbstractDataEntityColumn Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Workflow);
        base.GetExtraDependencies(dependencies: dependencies);
    }
    #endregion
    #region Properties
    public Guid WorkflowId;

    [Category(category: "References")]
    [TypeConverter(type: typeof(WorkflowConverter)), NotNullModelElementRule()]
    [XmlReference(attributeName: "workflow", idField: "WorkflowId")]
    public IWorkflow Workflow
    {
        get
        {
            return (IWorkflow)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: this.WorkflowId)
                );
        }
        set { this.WorkflowId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"]); }
    }
    #endregion
}
