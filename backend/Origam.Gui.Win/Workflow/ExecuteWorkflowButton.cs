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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Origam.DA;
using Origam.DA.Service;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using Origam.Workflow;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for ExecuteWorkflowButton.
/// </summary>
public class ExecuteWorkflowButton : Button, IOrigamMetadataConsumer, IAsDataConsumer
{
    #region Constructors
    public ExecuteWorkflowButton()
        : base()
    {
        this.FlatStyle = FlatStyle.Popup;
        this.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
    }
    #endregion
    #region Private Members
    private WorkflowEngine _engine;
    private ContextStore _mergeBackStore = null;
    private object _dataSource;
    private string _dataMember;
    private ISchemaItem _origamMetadata;
    private Guid _workflowId;
    private ColumnParameterMappingCollection _parameterMappingCollection =
        new ColumnParameterMappingCollection();
    private bool _fillingParameterCache = false;
    private Guid _iconId = Guid.Empty;
    private WorkflowHostEventArgs _resultEventArgs;
    private ServiceOutputMethod _mergeType = ServiceOutputMethod.AppendMergeExisting;
    #endregion
    #region Private Methods
    private void CreateMappingItemsCollection()
    {
        if (this.Workflow == null)
        {
            return;
        }
        // create any missing parameter mappings
        foreach (
            var store in Workflow.ChildItemsByType<ContextStore>(
                itemType: ContextStore.CategoryConst
            )
        )
        {
            string parameterName = store.Name;
            if (this._origamMetadata.GetChildByName(name: parameterName) == null)
            {
                ColumnParameterMapping mapping = _origamMetadata.NewItem<ColumnParameterMapping>(
                    schemaExtensionId: _origamMetadata.SchemaExtensionId,
                    group: null
                );
                mapping.Name = parameterName;
            }
        }
        var toDelete = new List<ColumnParameterMapping>();
        // delete all parameter mappings whose's context stores do not exist anymore
        foreach (
            var mapping in _origamMetadata.ChildItemsByType<ColumnParameterMapping>(
                itemType: ColumnParameterMapping.CategoryConst
            )
        )
        {
            bool found = false;
            foreach (
                var store in Workflow.ChildItemsByType<ContextStore>(
                    itemType: ContextStore.CategoryConst
                )
            )
            {
                if (store.Name == mapping.Name)
                {
                    found = true;
                }
            }
            if (!found)
            {
                toDelete.Add(item: mapping);
            }
        }
        foreach (ISchemaItem mapping in toDelete)
        {
            mapping.IsDeleted = true;
        }
        //Refill Parameter collection (and dictionary)
        FillParameterCache(controlItem: this._origamMetadata as ControlSetItem);
    }

    private void ClearMappingItemsOnly()
    {
        try
        {
            if (_origamMetadata == null)
            {
                return;
            }

            var col = _origamMetadata
                .ChildItemsByType<ColumnParameterMapping>(
                    itemType: ColumnParameterMapping.CategoryConst
                )
                .ToList();
            foreach (ColumnParameterMapping mapping in col)
            {
                mapping.IsDeleted = true;
            }
        }
        catch (Exception ex)
        {
            _ = ex.ToString();
#if DEBUG
            System.Diagnostics.Debug.WriteLine(message: "AsReportPanel:ERROR=>" + ex.ToString());
#endif
        }
    }

    private void FillParameterCache(ControlSetItem controlItem)
    {
        if (controlItem == null)
        {
            return;
        }

        _fillingParameterCache = true;
        ParameterMappings.Clear();
        foreach (
            var mapInfo in controlItem.ChildItemsByType<ColumnParameterMapping>(
                itemType: ColumnParameterMapping.CategoryConst
            )
        )
        {
            if (!mapInfo.IsDeleted) // skip any deleted mapping infos
            {
                ParameterMappings.Add(value: mapInfo);
            }
        }
        _fillingParameterCache = false;
    }

    private void SetIcon()
    {
        if (this.Icon == null)
        {
            this.Image = null;
        }
        else
        {
            this.Image = this.Icon.GraphicsData;
        }
    }

    private DataSet GetDataSlice(ContextStore store, DataRow row)
    {
        DataSet target = new DatasetGenerator(userDefinedParameters: true).CreateDataSet(
            ds: store.Structure as DataStructure
        );
        DatasetTools.GetDataSlice(target: target, rows: new List<DataRow> { row });
        target.AcceptChanges();
        return target;
    }
    #endregion
    #region Public Properties
    [DefaultValue(value: ServiceOutputMethod.AppendMergeExisting)]
    public ServiceOutputMethod MergeType
    {
        get { return _mergeType; }
        set { _mergeType = value; }
    }

    [Category(category: "Data")]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [TypeConverter(
        typeName: "System.Windows.Forms.Design.DataSourceConverter, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a"
    )]
    public object DataSource
    {
        get { return _dataSource; }
        set { _dataSource = value; }
    }

    [Category(category: "Data")]
    [Editor(
        typeName: "System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",
        baseType: typeof(System.Drawing.Design.UITypeEditor)
    )]
    public string DataMember
    {
        get { return _dataMember; }
        set { _dataMember = value; }
    }

    [Browsable(browsable: false)]
    public Guid WorkflowId
    {
        get { return _workflowId; }
        set { _workflowId = value; }
    }

    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [TypeConverter(type: typeof(WorkflowConverter))]
    public IWorkflow Workflow
    {
        get
        {
            if (this._origamMetadata == null)
            {
                return null;
            }

            return (IWorkflow)
                this._origamMetadata.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: _workflowId)
                );
        }
        set
        {
            if (value == null)
            {
                this.WorkflowId = Guid.Empty;
                ClearMappingItemsOnly();
            }
            else
            {
                if (this.WorkflowId == (Guid)value.PrimaryKey[key: "Id"])
                {
                    return;
                }
                this.WorkflowId = (Guid)value.PrimaryKey[key: "Id"];
                //ClearMappingItemsOnly();
                CreateMappingItemsCollection();
            }
        }
    }
    private WorkflowExecutionType _actionType = WorkflowExecutionType.NoFormMerge;
    public WorkflowExecutionType ActionType
    {
        get { return _actionType; }
        set { _actionType = value; }
    }

    [TypeConverter(type: typeof(ColumnParameterMappingCollectionConverter))]
    public ColumnParameterMappingCollection ParameterMappings
    {
        get
        {
            if (!_fillingParameterCache)
            {
                CreateMappingItemsCollection();
            }
            return _parameterMappingCollection;
        }
    }

    [Browsable(browsable: false)]
    public Guid IconId
    {
        get { return _iconId; }
        set
        {
            _iconId = value;
            SetIcon();
        }
    }

    [TypeConverter(type: typeof(GraphicsConverter))]
    public Origam.Schema.GuiModel.Graphics Icon
    {
        get
        {
            if (_origamMetadata == null)
            {
                return null;
            }
            return (Origam.Schema.GuiModel.Graphics)
                _origamMetadata.PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: IconId)
                );
        }
        set
        {
            if (value == null)
            {
                this.IconId = Guid.Empty;
            }
            else
            {
                this.IconId = (Guid)value.PrimaryKey[key: "Id"];
            }
            SetIcon();
        }
    }
    #endregion
    #region Overriden Members
    protected override void OnClick(EventArgs e)
    {
        WorkflowHost host = WorkflowHost.DefaultHost;
        if (this.BindingContext == null)
        {
            return;
        }

        if (this.BindingContext[dataSource: this.DataSource, dataMember: this.DataMember] == null)
        {
            return;
        }

        if (
            this.BindingContext[dataSource: this.DataSource, dataMember: this.DataMember].Position
            < 0
        )
        {
            return;
        }

        if ((this.DataSource as DataSet).HasErrors)
        {
            Origam.UI.AsMessageBox.ShowError(
                owner: this.FindForm(),
                text: Origam.Workflow.ResourceUtils.GetString(key: "ErrorsInForm"),
                caption: Origam.Workflow.ResourceUtils.GetString(key: "ExecuteActionTitle"),
                exception: null
            );
            return;
        }
        try
        {
            if (this.ActionType == WorkflowExecutionType.NoFormMerge)
            {
                try
                {
                    (this.FindForm() as AsForm).BeginDisable();
                    Application.DoEvents();
                    Cursor.Current = Cursors.WaitCursor;
                    _engine = new WorkflowEngine();
                    _engine.PersistenceProvider = _origamMetadata.PersistenceProvider;
                    _engine.WorkflowBlock = this.Workflow;
                    Hashtable parameters = this.Parameters();
                    foreach (DictionaryEntry entry in parameters)
                    {
                        string name = (string)entry.Key;
                        ContextStore context = null;
                        Key contextKey = null;
                        foreach (
                            var store in Workflow.ChildItemsByType<ContextStore>(
                                itemType: ContextStore.CategoryConst
                            )
                        )
                        {
                            if (store.Name == name)
                            {
                                contextKey = store.PrimaryKey;
                                context = store;
                            }
                        }
                        if (contextKey == null)
                        {
                            throw new ArgumentOutOfRangeException(
                                paramName: "mapping.Name",
                                actualValue: name,
                                message: Origam.Workflow.ResourceUtils.GetString(
                                    key: "ErrorContextStoreNotFound"
                                )
                            );
                        }
                        if (entry.Value is XmlDocument)
                        {
                            _mergeBackStore = context;
                        }
                        _engine.InputContexts.Add(key: contextKey, value: entry.Value);
                    }
                    host.WorkflowFinished += new WorkflowHostEvent(Host_WorkflowFinished);
                    host.WorkflowMessage += new WorkflowHostMessageEvent(Host_WorkflowMessage);
                    host.ExecuteWorkflow(engine: _engine);
                }
                catch (Exception ex)
                {
                    UnsubscribeEvents();
                    (this.FindForm() as AsForm).EndDisable();
                    HandleException(ex: ex);
                    return;
                }
            }
            else if (this.ActionType == WorkflowExecutionType.ShowNewFormNoMerge)
            {
                WorkflowForm form = Origam.Workflow.Gui.Win.WorkflowHelper.CreateWorkflowForm(
                    host: host,
                    icon: null,
                    titleName: this.Text,
                    workflowId: (Guid)this.Workflow.PrimaryKey[key: "Id"]
                );
                WorkflowEngine workflowEngine = WorkflowEngine.PrepareWorkflow(
                    workflow: this.Workflow,
                    parameters: this.Parameters(),
                    isRepeatable: false,
                    titleName: this.Text
                );
                form.WorkflowEngine = workflowEngine;
                host.ExecuteWorkflow(engine: workflowEngine);
            }
            else
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "ActionType",
                    actualValue: this.ActionType,
                    message: Origam.Workflow.ResourceUtils.GetString(
                        key: "ErrorUnsupportedActionType"
                    )
                );
            }
        }
        catch (Exception ex)
        {
            HandleException(ex: ex);
        }
    }

    private void Host_WorkflowFinished(object sender, WorkflowHostEventArgs e)
    {
        _resultEventArgs = e;
        this.Invoke(method: new MethodInvoker(this.FinishWorkflow));
    }

    private void FinishWorkflow()
    {
        WorkflowHostEventArgs e = _resultEventArgs;
        if (e.Engine.WorkflowUniqueId.Equals(g: _engine.WorkflowUniqueId))
        {
            UnsubscribeEvents();
            if (e.Exception != null)
            {
                (this.FindForm() as AsForm).EndDisable();
                HandleException(ex: e.Exception);
                return;
            }
            try
            {
                if (_mergeBackStore != null)
                {
                    object profileId = SecurityManager.CurrentUserProfile().Id;
                    DataRowView current =
                        this.BindingContext[
                            dataSource: this.DataSource,
                            dataMember: this.DataMember
                        ].Current as DataRowView;
                    try
                    {
                        current.Row.Table.DataSet.EnforceConstraints = false;
                        foreach (DataTable table in current.Row.Table.DataSet.Tables)
                        {
                            table.BeginLoadData();
                        }
                        MergeParams mergeParams = new MergeParams(ProfileId: profileId);
                        mergeParams.TrueDelete = MergeType == ServiceOutputMethod.FullMerge;
                        DatasetTools.MergeDataSet(
                            inout_dsTarget: current.Row.Table.DataSet,
                            in_dsSource: (
                                _engine.RuleEngine.GetContext(contextStore: _mergeBackStore)
                                as IDataDocument
                            ).DataSet,
                            changeList: null,
                            mergeParams: mergeParams
                        );
                    }
                    finally
                    {
                        foreach (DataTable table in current.Row.Table.DataSet.Tables)
                        {
                            table.EndLoadData();
                        }
                        current.Row.Table.DataSet.EnforceConstraints = true;
                    }
                }
            }
            catch (Exception ex)
            {
                HandleException(ex: ex);
            }
            (this.FindForm() as AsForm).EndDisable();
        }
    }

    private void Host_WorkflowMessage(object sender, WorkflowHostMessageEventArgs e)
    {
        if (e.Engine.WorkflowUniqueId.Equals(g: _engine.WorkflowUniqueId))
        {
            if (e.Exception != null)
            {
                UnsubscribeEvents();
                HandleException(ex: e.Exception);
                (this.FindForm() as AsForm).EndDisable();
            }
        }
    }

    private void UnsubscribeEvents()
    {
        if (_engine.Host != null)
        {
            _engine.Host.WorkflowFinished -= new WorkflowHostEvent(Host_WorkflowFinished);
            _engine.Host.WorkflowMessage -= new WorkflowHostMessageEvent(Host_WorkflowMessage);
        }
    }

    private Hashtable Parameters()
    {
        if (this.DataMember == null)
        {
            throw new NullReferenceException(
                message: Origam.Workflow.ResourceUtils.GetString(key: "ErrorNoDataMember")
            );
        }
        Hashtable result = new Hashtable();
        DataRowView current =
            this.BindingContext[dataSource: this.DataSource, dataMember: this.DataMember].Current
            as DataRowView;
        foreach (ColumnParameterMapping mapping in this.ParameterMappings)
        {
            ContextStore context = null;
            Key contextKey = null;
            foreach (
                var store in Workflow.ChildItemsByType<ContextStore>(
                    itemType: ContextStore.CategoryConst
                )
            )
            {
                if (store.Name == mapping.Name)
                {
                    contextKey = store.PrimaryKey;
                    context = store;
                }
            }
            if (contextKey == null)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "mapping.Name",
                    actualValue: mapping.Name,
                    message: Origam.Workflow.ResourceUtils.GetString(
                        key: "ErrorContextStoreNotFound"
                    )
                );
            }
            if (mapping.ColumnName == "/")
            {
                result.Add(
                    key: mapping.Name,
                    value: DataDocumentFactory.New(dataSet: current.Row.Table.DataSet.Copy())
                );
            }
            else if (mapping.ColumnName == ".")
            {
                result.Add(
                    key: mapping.Name,
                    value: DataDocumentFactory.New(
                        dataSet: GetDataSlice(store: context, row: current.Row)
                    )
                );
            }
            else if (mapping.ColumnName != null && mapping.ColumnName != "")
            {
                bool found = false;
                foreach (DataColumn column in current.Row.Table.Columns)
                {
                    if (column.ColumnName == mapping.ColumnName)
                    {
                        DataRowVersion version = DataRowVersion.Default;
                        result.Add(
                            key: mapping.Name,
                            value: current.Row[column: column, version: version]
                        );
                        found = true;
                    }
                }
                if (!found)
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "Field",
                        actualValue: mapping.ColumnName,
                        message: Origam.Workflow.ResourceUtils.GetString(key: "ErrorFieldNotFound")
                    );
                }
            }
        }
        return result;
    }

    private void HandleException(Exception ex)
    {
        string caption;
        if (ex is RuleException)
        {
            MessageBox.Show(
                owner: this.FindForm(),
                text: ex.Message,
                caption: RuleEngine.ValidationNotMetMessage(),
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error
            );
        }
        else
        {
            caption = Origam.Workflow.ResourceUtils.GetString(
                key: "ErrorMessage",
                args: new object[] { this.Text, this.FindForm().Text }
            );
            Origam.UI.AsMessageBox.ShowError(
                owner: this.FindForm(),
                text: ex.Message,
                caption: caption,
                exception: ex
            );
        }
        Cursor.Current = Cursors.Default;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _origamMetadata = null;
            _dataSource = null;
        }
        base.Dispose(disposing: disposing);
    }
    #endregion
    #region IOrigamMetadataConsumer Members
    public ISchemaItem OrigamMetadata
    {
        get { return _origamMetadata; }
        set
        {
            _origamMetadata = value;
            SetIcon();
            FillParameterCache(controlItem: _origamMetadata as ControlSetItem);
        }
    }
    #endregion
}
