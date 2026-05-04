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
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;

namespace Origam.Schema.WorkflowModel;

[SchemaItemDescription(name: "Workflow Schedule", iconName: "workflow-schedule.png")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.0")]
public class WorkflowSchedule : AbstractSchemaItem
{
    public const string CategoryConst = "WorkflowSchedule";

    public WorkflowSchedule() { }

    public WorkflowSchedule(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId) { }

    public WorkflowSchedule(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    #region Properties
    public Guid WorkflowId;

    [TypeConverter(type: typeof(WorkflowConverter))]
    [XmlReference(attributeName: "workflow", idField: "WorkflowId")]
    public IWorkflow Workflow
    {
        get
        {
            var key = new ModelElementKey { Id = WorkflowId };
            return (IWorkflow)
                PersistenceProvider.RetrieveInstance(type: typeof(ISchemaItem), primaryKey: key);
        }
        set
        {
            if (value == null)
            {
                WorkflowId = Guid.Empty;
            }
            else
            {
                WorkflowId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }

    public Guid ScheduleTimeId;

    [TypeConverter(type: typeof(ScheduleTimeConverter))]
    [XmlReference(attributeName: "scheduleTime", idField: "ScheduleTimeId")]
    public AbstractScheduleTime ScheduleTime
    {
        get
        {
            var key = new ModelElementKey { Id = ScheduleTimeId };
            return (AbstractScheduleTime)
                PersistenceProvider.RetrieveInstance(type: typeof(ISchemaItem), primaryKey: key);
        }
        set
        {
            if (value == null)
            {
                ScheduleTimeId = Guid.Empty;
            }
            else
            {
                ScheduleTimeId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }
    #endregion
    #region Overriden ISchemaItem Members

    public override string ItemType => CategoryConst;

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: Workflow);
        dependencies.Add(item: ScheduleTime);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override bool UseFolders => false;
    #endregion
    #region ISchemaItemFactory Members
    [Browsable(browsable: false)]
    public override Type[] NewItemTypes =>
        new[]
        {
            typeof(DataConstantReference),
            typeof(SystemFunctionCall),
            typeof(ReportReference),
        };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        string itemName = null;
        if (typeof(T) == typeof(DataConstantReference))
        {
            itemName = "NewDataConstantReference";
        }
        else if (typeof(T) == typeof(ReportReference))
        {
            itemName = "NewReportReference";
        }
        else if (typeof(T) == typeof(SystemFunctionCall))
        {
            itemName = "NewSystemFunctionCall";
        }
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: itemName
        );
    }
    #endregion
}
