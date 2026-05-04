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
using Origam.DA;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.RuleModel;

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Summary description for AbstractWorkflowStep.
/// </summary>
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.2")]
public abstract class AbstractWorkflowStep : AbstractSchemaItem, IWorkflowStep
{
    public const string CategoryConst = "WorkflowTask";

    public AbstractWorkflowStep()
        : base()
    {
        Init();
    }

    public AbstractWorkflowStep(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public AbstractWorkflowStep(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init()
    {
        this.ChildItemTypes.Add(item: typeof(WorkflowTaskDependency));
    }

    #region Overriden ISchemaItem Members
    public override string ItemType
    {
        get { return CategoryConst; }
    }

    [Browsable(browsable: false)]
    public override bool CanMove(UI.IBrowserNode2 destinationNode)
    {
        if (
            !(destinationNode is ISchemaItem destinationSchemaItem)
            || destinationSchemaItem.RootItem == null
            || this.RootItem != destinationSchemaItem.RootItem
            || !(destinationNode is IWorkflowBlock)
            || (destinationNode is AbstractWorkflowBlock && Dependencies.Count > 0)
        )
        {
            return false;
        }
        return true;
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (this.StartConditionRule != null)
        {
            dependencies.Add(item: this.StartConditionRule);
        }

        if (this.StartConditionRuleContextStore != null)
        {
            dependencies.Add(item: this.StartConditionRuleContextStore);
        }

        if (this.ValidationRule != null)
        {
            dependencies.Add(item: this.ValidationRule);
        }

        if (this.ValidationRuleContextStore != null)
        {
            dependencies.Add(item: this.ValidationRuleContextStore);
        }

        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override void UpdateReferences()
    {
        foreach (ISchemaItem item in this.RootItem.ChildItemsRecursive)
        {
            if (item.OldPrimaryKey != null)
            {
                if (
                    this.StartConditionRuleContextStore != null
                    && item.OldPrimaryKey.Equals(
                        obj: this.StartConditionRuleContextStore.PrimaryKey
                    )
                )
                {
                    this.StartConditionRuleContextStore = item as IContextStore;
                }
                if (
                    this.ValidationRuleContextStore != null
                    && item.OldPrimaryKey.Equals(obj: this.ValidationRuleContextStore.PrimaryKey)
                )
                {
                    this.ValidationRuleContextStore = item as IContextStore;
                }
            }
        }
        base.UpdateReferences();
    }
    #endregion
    #region IWorkflowStep Members
    [Browsable(browsable: false)]
    public List<WorkflowTaskDependency> Dependencies =>
        ChildItemsByType<WorkflowTaskDependency>(itemType: WorkflowTaskDependency.CategoryConst);

    [Category(category: "Error Handling")]
    [XmlAttribute(attributeName: "onFailure")]
    [Description(
        description: $"Exception thrown in this step will cause the parent workflow to fail when set to {nameof(StepFailureMode.WorkflowFails)}. The exception will be ignored when set to {nameof(StepFailureMode.Suppress)}."
    )]
    public StepFailureMode OnFailure { set; get; }

    [DefaultValue(value: Trace.InheritFromParent)]
    [Category(category: "Tracing"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [RuntimeConfigurable(name: "traceLevel")]
    public Trace TraceLevel { get; set; } = Trace.InheritFromParent;

    [Category(category: "Tracing")]
    public Trace Trace =>
        TraceLevel == Trace.InheritFromParent ? GetValueOfFirstNonInheritParent() : TraceLevel;

    private Trace GetValueOfFirstNonInheritParent()
    {
        IWorkflowStep parentStep = ParentItem as IWorkflowStep;
        while (parentStep != null)
        {
            if (parentStep.TraceLevel != Trace.InheritFromParent)
            {
                return parentStep.TraceLevel;
            }

            parentStep = parentStep.ParentItem as IWorkflowStep;
        }
        return Trace.InheritFromParent;
    }

    public Guid StartRuleId;

    [Category(category: "Rules")]
    [TypeConverter(type: typeof(StartRuleConverter))]
    [NotNullModelElementRule(conditionField: "StartConditionRuleContextStore")]
    [XmlReference(attributeName: "startConditionRule", idField: "StartRuleId")]
    public StartRule StartConditionRule
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.StartRuleId;
            return (StartRule)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                this.StartRuleId = Guid.Empty;
            }
            else
            {
                this.StartRuleId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }

    public Guid StartRuleContextStoreId;

    [Category(category: "Rules")]
    [TypeConverter(type: typeof(ContextStoreConverter))]
    [NotNullModelElementRule(conditionField: "StartConditionRule")]
    [XmlReference(
        attributeName: "startConditionRuleContextStore",
        idField: "StartRuleContextStoreId"
    )]
    public IContextStore StartConditionRuleContextStore
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.StartRuleContextStoreId;
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
                this.StartRuleContextStoreId = Guid.Empty;
            }
            else
            {
                this.StartRuleContextStoreId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }

    public Guid ValidationRuleId;

    [Category(category: "Rules")]
    [TypeConverter(type: typeof(EndRuleConverter))]
    [NotNullModelElementRule(conditionField: "ValidationRuleContextStore")]
    [XmlReference(attributeName: "validationRule", idField: "ValidationRuleId")]
    public IEndRule ValidationRule
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.ValidationRuleId;
            return (IEndRule)
                this.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: key
                );
        }
        set
        {
            if (value == null)
            {
                this.ValidationRuleId = Guid.Empty;
            }
            else
            {
                this.ValidationRuleId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }

    public Guid ValidationRuleContextStoreId;

    [Category(category: "Rules")]
    [TypeConverter(type: typeof(ContextStoreConverter))]
    [NotNullModelElementRule(conditionField: "ValidationRule")]
    [XmlReference(
        attributeName: "validationRuleContextStore",
        idField: "ValidationRuleContextStoreId"
    )]
    public IContextStore ValidationRuleContextStore
    {
        get
        {
            ModelElementKey key = new ModelElementKey();
            key.Id = this.ValidationRuleContextStoreId;
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
                this.ValidationRuleContextStoreId = Guid.Empty;
            }
            else
            {
                this.ValidationRuleContextStoreId = (Guid)value.PrimaryKey[key: "Id"];
            }
        }
    }

    [Category(category: "Rules")]
    [XmlAttribute(attributeName: "roles")]
    public string Roles { get; set; } = "*";

    [Category(category: "Rules")]
    [XmlAttribute(attributeName: "features")]
    public string Features { get; set; }
    #endregion
}
