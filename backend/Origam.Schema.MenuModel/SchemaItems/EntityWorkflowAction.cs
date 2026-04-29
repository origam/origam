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
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Workbench.Services;

namespace Origam.Schema.MenuModel;

/// <summary>
/// Summary description for EntitySecurityRule.
/// </summary>
[SchemaItemDescription(
    name: "Sequential Workflow Action",
    folderName: "UI Actions",
    iconName: "icon_sequential-workflow-action.png"
)]
[HelpTopic(topic: "Sequential+Workflow+Action")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class EntityWorkflowAction : EntityUIAction
{
    public EntityWorkflowAction()
        : base()
    {
        Init();
    }

    public EntityWorkflowAction(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId)
    {
        Init();
    }

    public EntityWorkflowAction(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init()
    {
        ChildItemTypes.Add(item: typeof(EntityWorkflowActionScriptCall));
    }

    #region Overriden AbstractDataEntityColumn Members

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: this.Workflow);
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override IList<string> NewTypeNames
    {
        get
        {
            try
            {
                IBusinessServicesService agents =
                    ServiceManager.Services.GetService(
                        serviceType: typeof(IBusinessServicesService)
                    ) as IBusinessServicesService;
                IServiceAgent agent = agents.GetAgent(
                    serviceType: "WorkflowService",
                    ruleEngine: null,
                    workflowEngine: null
                );
                return agent.ExpectedParameterNames(
                    item: Workflow,
                    method: "ExecuteWorkflow",
                    parameter: "Parameters"
                );
            }
            catch
            {
                return new string[] { };
            }
        }
    }
    #endregion
    #region Properties
    private ServiceOutputMethod _mergeType = ServiceOutputMethod.AppendMergeExisting;

    [DefaultValue(value: ServiceOutputMethod.AppendMergeExisting)]
    [XmlAttribute(attributeName: "mergeType")]
    public ServiceOutputMethod MergeType
    {
        get { return _mergeType; }
        set { _mergeType = value; }
    }
    private bool _saveAfterWorkflow = false;

    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "saveAfterWorkflow")]
    public bool SaveAfterWorkflow
    {
        get { return _saveAfterWorkflow; }
        set { _saveAfterWorkflow = value; }
    }
    private bool _requestSaveBeforeWorkflow = false;

    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "requestSaveBeforeWorkflow")]
    public bool RequestSaveBeforeWorkflow
    {
        get { return _requestSaveBeforeWorkflow; }
        set { _requestSaveBeforeWorkflow = value; }
    }
    private bool _commitChangesAfterMerge = false;

    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "commitChangesAfterMerge")]
    public bool CommitChangesAfterMerge
    {
        get { return _commitChangesAfterMerge; }
        set { _commitChangesAfterMerge = value; }
    }
    private bool _cleanDataBeforeMerge = false;

    [DefaultValue(value: false)]
    [XmlAttribute(attributeName: "cleanDataBeforeMerge")]
    public bool CleanDataBeforeMerge
    {
        get { return _cleanDataBeforeMerge; }
        set { _cleanDataBeforeMerge = value; }
    }
    private bool _refreshPortalAfterFinish = false;

    [DefaultValue(value: false)]
    [Description(
        description: "If true, the client will refresh its menu after finishing the action."
    )]
    [XmlAttribute(attributeName: "refreshPortalAfterFinish")]
    public bool RefreshPortalAfterFinish
    {
        get { return _refreshPortalAfterFinish; }
        set { _refreshPortalAfterFinish = value; }
    }
    private ModalDialogCloseType _closeType = ModalDialogCloseType.None;

    [DefaultValue(value: ModalDialogCloseType.None)]
    [XmlAttribute(attributeName: "closeType")]
    public ModalDialogCloseType CloseType
    {
        get { return _closeType; }
        set { _closeType = value; }
    }
    private SaveRefreshType _refreshAfterWorkflow = SaveRefreshType.RefreshChangedRecords;

    [DefaultValue(value: SaveRefreshType.RefreshChangedRecords)]
    [XmlAttribute(attributeName: "refreshAfterWorkflow")]
    public SaveRefreshType RefreshAfterWorkflow
    {
        get { return _refreshAfterWorkflow; }
        set { _refreshAfterWorkflow = value; }
    }
    public Guid WorkflowId;

    [Category(category: "References")]
    [TypeConverter(type: typeof(WorkflowConverter))]
    [NotNullModelElementRule()]
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
