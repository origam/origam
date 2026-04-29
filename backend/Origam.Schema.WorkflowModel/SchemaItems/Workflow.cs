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
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.RuleModel;

namespace Origam.Schema.WorkflowModel;

/// <summary>
/// Summary description for Workflow.
/// </summary>
[SchemaItemDescription(name: "Sequential Workflow", iconName: "sequential-workflow.png")]
[HelpTopic(topic: "Sequential+Workflows")]
[XmlModelRoot(category: CategoryConst)]
[ClassMetaVersion(versionStr: "6.0.2")]
public class Workflow : AbstractSchemaItem, IWorkflow
{
    public const string CategoryConst = "Workflow";

    public Workflow()
        : base()
    {
        Init();
    }

    public Workflow(Guid schemaExtensionId)
        : base(extensionId: schemaExtensionId)
    {
        Init();
    }

    public Workflow(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init()
    {
        this.ChildItemTypes.Add(item: typeof(ServiceMethodCallTask));
        this.ChildItemTypes.Add(item: typeof(UIFormTask));
        this.ChildItemTypes.Add(item: typeof(WorkflowCallTask));
        this.ChildItemTypes.Add(item: typeof(SetWorkflowPropertyTask));
        this.ChildItemTypes.Add(item: typeof(UpdateContextTask));
        this.ChildItemTypes.Add(item: typeof(AcceptContextStoreChangesTask));
        this.ChildItemTypes.Add(item: typeof(TransactionWorkflowBlock));
        this.ChildItemTypes.Add(item: typeof(ForeachWorkflowBlock));
        this.ChildItemTypes.Add(item: typeof(LoopWorkflowBlock));
        this.ChildItemTypes.Add(item: typeof(ContextStore));
        this.ChildItemTypes.Add(item: typeof(CheckRuleStep));
        this.ChildItemTypes.Add(item: typeof(WaitTask));
    }

    /* @Short returns a name of context store that is used for returning values.
     *        If there isn't such a context, null is returned.
     *
     * */
    public ContextStore GetReturnContext()
    {
        foreach (
            ContextStore context in ChildItemsByType<ContextStore>(
                itemType: ContextStore.CategoryConst
            )
        )
        {
            if (context.IsReturnValue)
            {
                return context;
            }
        }
        return null;
    }

    #region IWorkflowStep Members
    [DefaultValue(value: WorkflowTransactionBehavior.InheritExisting)]
    [Category(category: "Transactions"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [DisplayName(displayName: "Transaction Behavior")]
    [XmlAttribute(attributeName: "transactionBehavior")]
    [Description(
        description: "Controls how will workflow interact with incoming transactions. The default behavior is to inherit them."
    )]
    public WorkflowTransactionBehavior TransactionBehavior { get; set; } =
        WorkflowTransactionBehavior.InheritExisting;
    #endregion
    #region Overriden ISchemaItem Members

    public override string ItemType
    {
        get { return CategoryConst; }
    }
    #endregion
    #region IWorkflowStep Members
    [Browsable(browsable: false)]
    public List<WorkflowTaskDependency> Dependencies => new();

    // It does not really make sense to change this property on Workflow.
    // That is why it is not visible in the Architect and not persisted in XML.
    [Browsable(browsable: false)]
    public StepFailureMode OnFailure { get; set; } = StepFailureMode.WorkflowFails;

    [DefaultValue(value: Trace.InheritFromParent)]
    [Category(category: "Tracing"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [RuntimeConfigurable(name: "traceLevel")]
    [DisplayName(displayName: "Trace Level")]
    public Trace TraceLevel { get; set; } = Trace.InheritFromParent;

    [Category(category: "Tracing")]
    public Trace Trace
    {
        get
        {
            foreach (object s in this.ChildItemsRecursive)
            {
                if (s is IWorkflowStep workflowStep)
                {
                    if (workflowStep.Trace == Trace.Yes)
                    {
                        return Trace.Yes;
                    }

                    if (workflowStep is WorkflowCallTask workflowCallTask)
                    {
                        // skip any direct recursion
                        if (!workflowCallTask.Workflow.PrimaryKey.Equals(obj: this.PrimaryKey))
                        {
                            Trace result = workflowCallTask.Workflow.Trace;
                            if (result == Trace.Yes)
                            {
                                return Trace.Yes;
                            }
                        }
                    }
                }
            }
            return Trace.No;
        }
    }

    [Browsable(browsable: false)]
    public StartRule StartConditionRule
    {
        get { return null; }
        set { throw new InvalidOperationException(message: "Cannot set start rule to Workflow"); }
    }

    [Browsable(browsable: false)]
    public IContextStore StartConditionRuleContextStore
    {
        get { return null; }
        set { throw new InvalidOperationException(message: "Cannot set start rule to Workflow"); }
    }

    [Browsable(browsable: false)]
    public IEndRule ValidationRule
    {
        get { return null; }
        set { throw new InvalidOperationException(message: "Cannot set end rule to Workflow"); }
    }

    [Browsable(browsable: false)]
    public IContextStore ValidationRuleContextStore
    {
        get { return null; }
        set { throw new InvalidOperationException(message: "Cannot set end rule to Workflow"); }
    }

    [Browsable(browsable: false)]
    public string Roles
    {
        get { return null; }
        set { throw new InvalidOperationException(message: "Cannot set Roles to Workflow"); }
    }

    [Browsable(browsable: false)]
    public string Features
    {
        get { return null; }
        set { throw new InvalidOperationException(message: "Cannot set Features to Workflow"); }
    }
    #endregion
}
