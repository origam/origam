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
using System.Xml;
using System.Collections;
using System.Data;
using System.Windows.Forms;
using System.ComponentModel;

using Origam.Gui.Win;
using Origam.Workbench.Services;
using Origam.DA;
using Origam.DA.Service;

using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.Schema.GuiModel;
using Origam.Rule;
using System.Collections.Generic;
using Origam.Workflow;
using Origam;
using Origam.Extensions;
using Origam.Service.Core;

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
    private AbstractSchemaItem _origamMetadata;
    private Guid _workflowId;
    private ColumnParameterMappingCollection _parameterMappingCollection = new ColumnParameterMappingCollection();
    private bool _fillingParameterCache = false;
    private Guid _iconId = Guid.Empty;
    private WorkflowHostEventArgs _resultEventArgs;
    private ServiceOutputMethod _mergeType = ServiceOutputMethod.AppendMergeExisting;
    #endregion

    #region Private Methods
    private void CreateMappingItemsCollection()
    {
            if(this.Workflow == null) return;

            // create any missing parameter mappings
            foreach(ContextStore store in this.Workflow.ChildItemsByType(ContextStore.CategoryConst))
            {
                string parameterName = store.Name;

                if(this._origamMetadata.GetChildByName(parameterName) == null)
                {
                    ColumnParameterMapping mapping = _origamMetadata
                        .NewItem<ColumnParameterMapping>(
                            _origamMetadata.SchemaExtensionId, null);
                    mapping.Name = parameterName;
                }
            }

            ArrayList toDelete = new ArrayList();
            // delete all parameter mappings whose's context stores do not exist anymore
            foreach(AbstractSchemaItem mapping in this._origamMetadata.ChildItemsByType(ColumnParameterMapping.CategoryConst))
            {
                bool found = false;
                foreach(ContextStore store in this.Workflow.ChildItemsByType(ContextStore.CategoryConst))
                {
                    if(store.Name == mapping.Name) found = true;
                }

                if(!found)
                {
                    toDelete.Add(mapping);
                }
            }

            foreach(AbstractSchemaItem mapping in toDelete)
            {
                mapping.IsDeleted = true;
            }

            //Refill Parameter collection (and dictionary)
            FillParameterCache(this._origamMetadata as ControlSetItem);
        }

    private void ClearMappingItemsOnly()
    {
        try
        {
            if(_origamMetadata == null) return;

            ArrayList col = new ArrayList(_origamMetadata.ChildItemsByType(ColumnParameterMapping.CategoryConst));

            foreach(ColumnParameterMapping mapping in col)
            {
                mapping.IsDeleted = true;
            }
        }
        catch(Exception ex)
        {
            _ = ex.ToString();
#if DEBUG
            System.Diagnostics.Debug.WriteLine("AsReportPanel:ERROR=>" + ex.ToString());
#endif
        }
    }

    private void FillParameterCache(ControlSetItem controlItem)
    {
            if(controlItem == null) return;

            _fillingParameterCache = true;

            ParameterMappings.Clear();

            foreach(ColumnParameterMapping mapInfo in controlItem.ChildItemsByType(ColumnParameterMapping.CategoryConst))
            {
                if(!mapInfo.IsDeleted)	// skip any deleted mapping infos
                {
                    ParameterMappings.Add(mapInfo);
                }
            }

            _fillingParameterCache = false;
        }

    private void SetIcon()
    {
            if(this.Icon == null)
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
            DataSet target = new DatasetGenerator(true).CreateDataSet(store.Structure as DataStructure);

            DatasetTools.GetDataSlice(target, new List<DataRow> { row });

            target.AcceptChanges();

            return target;
        }
    #endregion

    #region Public Properties
    [DefaultValue(ServiceOutputMethod.AppendMergeExisting)]
    public ServiceOutputMethod MergeType
    {
        get
        {
                return _mergeType;
            }
        set
        {
                _mergeType = value;
            }
    }

    [Category("Data")]
    [RefreshProperties(RefreshProperties.Repaint)]
    [TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public object DataSource
    {
        get
        {
                return _dataSource;
            }

        set
        {
                _dataSource = value;
            }
    }

    [Category("Data")]
    [Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
    public string DataMember
    {
        get
        {
                return _dataMember;
            }
        set
        {
                _dataMember = value;
            }
    }

    [Browsable(false)]
    public Guid WorkflowId
    {
        get
        {
                return _workflowId;
            }
        set
        {
                _workflowId = value;
            }
    }

    [RefreshProperties(RefreshProperties.Repaint)]
    [TypeConverter(typeof(WorkflowConverter))]
    public IWorkflow Workflow
    {
        get
        {
                if(this._origamMetadata == null)
                    return null;

                return (IWorkflow)this._origamMetadata.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(_workflowId));
            }
        set
        {
                if(value == null)
                {
                    this.WorkflowId = Guid.Empty;
                    ClearMappingItemsOnly();
                }
                else
                {
                    if(this.WorkflowId == (Guid)value.PrimaryKey["Id"])
                    {
                        return;
                    }

                    this.WorkflowId = (Guid)value.PrimaryKey["Id"];
                    //ClearMappingItemsOnly();
                    CreateMappingItemsCollection();
                }
            }
    }

    private WorkflowExecutionType _actionType = WorkflowExecutionType.NoFormMerge;
    public WorkflowExecutionType ActionType
    {
        get
        {
                return _actionType;
            }
        set
        {
                _actionType = value;
            }
    }

    [TypeConverter(typeof(ColumnParameterMappingCollectionConverter))]
    public ColumnParameterMappingCollection ParameterMappings
    {
        get
        {
                if(!_fillingParameterCache)
                {
                    CreateMappingItemsCollection();
                }

                return _parameterMappingCollection;
            }
    }

    [Browsable(false)]
    public Guid IconId
    {
        get
        {
                return _iconId;
            }
        set
        {
                _iconId = value;

                SetIcon();
            }
    }

    [TypeConverter(typeof(GraphicsConverter))]
    public Origam.Schema.GuiModel.Graphics Icon
    {
        get
        {
                if(_origamMetadata == null)
                {
                    return null;
                }

                return (Origam.Schema.GuiModel.Graphics)_origamMetadata.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(IconId));
            }
        set
        {
                if(value == null)
                {
                    this.IconId = Guid.Empty;
                }
                else
                {
                    this.IconId = (Guid)value.PrimaryKey["Id"];
                }

                SetIcon();
            }
    }
    #endregion

    #region Overriden Members
    protected override void OnClick(EventArgs e)
    {
            WorkflowHost host = WorkflowHost.DefaultHost;

            if(this.BindingContext == null) return;
            if(this.BindingContext[this.DataSource, this.DataMember] == null) return;
            if(this.BindingContext[this.DataSource, this.DataMember].Position < 0) return;

            if((this.DataSource as DataSet).HasErrors)
            {
                Origam.UI.AsMessageBox.ShowError(this.FindForm(), Origam.Workflow.ResourceUtils.GetString("ErrorsInForm"), Origam.Workflow.ResourceUtils.GetString("ExecuteActionTitle"), null);
                return;
            }

            try
            {
                if(this.ActionType == WorkflowExecutionType.NoFormMerge)
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

                        foreach(DictionaryEntry entry in parameters)
                        {
                            string name = (string)entry.Key;

                            ContextStore context = null;
                            Key contextKey = null;
                            foreach(ContextStore store in this.Workflow.ChildItemsByType(ContextStore.CategoryConst))
                            {
                                if(store.Name == name)
                                {
                                    contextKey = store.PrimaryKey;
                                    context = store;
                                }
                            }

                            if(contextKey == null)
                            {
                                throw new ArgumentOutOfRangeException("mapping.Name", name, Origam.Workflow.ResourceUtils.GetString("ErrorContextStoreNotFound"));
                            }

                            if(entry.Value is XmlDocument)
                            {
                                _mergeBackStore = context;
                            }

                            _engine.InputContexts.Add(contextKey, entry.Value);
                        }

                        host.WorkflowFinished += new WorkflowHostEvent(Host_WorkflowFinished);
                        host.WorkflowMessage += new WorkflowHostMessageEvent(Host_WorkflowMessage);
                        host.ExecuteWorkflow(_engine);
                    }
                    catch(Exception ex)
                    {
                        UnsubscribeEvents();
                        (this.FindForm() as AsForm).EndDisable();
                        HandleException(ex);
                        return;
                    }
                }
                else if(this.ActionType == WorkflowExecutionType.ShowNewFormNoMerge)
                {
                    WorkflowForm form = Origam.Workflow.Gui.Win.WorkflowHelper.CreateWorkflowForm(host, null, this.Text, (Guid)this.Workflow.PrimaryKey["Id"]);
                    WorkflowEngine workflowEngine = WorkflowEngine.PrepareWorkflow(this.Workflow, this.Parameters(), false, this.Text);
                    form.WorkflowEngine = workflowEngine;
                    host.ExecuteWorkflow(workflowEngine);
                }
                else
                {
                    throw new ArgumentOutOfRangeException("ActionType", this.ActionType, Origam.Workflow.ResourceUtils.GetString("ErrorUnsupportedActionType"));
                }
            }
            catch(Exception ex)
            {
                HandleException(ex);
            }
        }

    private void Host_WorkflowFinished(object sender, WorkflowHostEventArgs e)
    {
            _resultEventArgs = e;
            this.Invoke(new MethodInvoker(this.FinishWorkflow));
        }

    private void FinishWorkflow()
    {
            WorkflowHostEventArgs e = _resultEventArgs;

            if(e.Engine.WorkflowUniqueId.Equals(_engine.WorkflowUniqueId))
            {
                UnsubscribeEvents();

                if(e.Exception != null)
                {
                    (this.FindForm() as AsForm).EndDisable();
                    HandleException(e.Exception);
                    return;
                }

                try
                {
                    if(_mergeBackStore != null)
                    {
                        object profileId = SecurityManager.CurrentUserProfile().Id;

                        DataRowView current = this.BindingContext[this.DataSource, this.DataMember].Current as DataRowView;

                        try
                        {
                            current.Row.Table.DataSet.EnforceConstraints = false;
                            foreach(DataTable table in current.Row.Table.DataSet.Tables)
                            {
                                table.BeginLoadData();
                            }
                            MergeParams mergeParams = new MergeParams(profileId);
                            mergeParams.TrueDelete
                                = MergeType == ServiceOutputMethod.FullMerge;
                            DatasetTools.MergeDataSet(current.Row.Table.DataSet,
                                (_engine.RuleEngine.GetContext(_mergeBackStore) as IDataDocument).DataSet,
                                null, mergeParams);
                        }
                        finally
                        {
                            foreach(DataTable table in current.Row.Table.DataSet.Tables)
                            {
                                table.EndLoadData();
                            }
                            current.Row.Table.DataSet.EnforceConstraints = true;
                        }
                    }
                }
                catch(Exception ex)
                {
                    HandleException(ex);
                }

                (this.FindForm() as AsForm).EndDisable();
            }
        }

    private void Host_WorkflowMessage(object sender, WorkflowHostMessageEventArgs e)
    {
            if(e.Engine.WorkflowUniqueId.Equals(_engine.WorkflowUniqueId))
            {
                if(e.Exception != null)
                {
                    UnsubscribeEvents();
                    HandleException(e.Exception);
                    (this.FindForm() as AsForm).EndDisable();
                }
            }
        }

    private void UnsubscribeEvents()
    {
            if(_engine.Host != null)
            {
                _engine.Host.WorkflowFinished -= new WorkflowHostEvent(Host_WorkflowFinished);
                _engine.Host.WorkflowMessage -= new WorkflowHostMessageEvent(Host_WorkflowMessage);
            }
        }

    private Hashtable Parameters()
    {
            if(this.DataMember == null)
            {
                throw new NullReferenceException(Origam.Workflow.ResourceUtils.GetString("ErrorNoDataMember"));
            }

            Hashtable result = new Hashtable();

            DataRowView current = this.BindingContext[this.DataSource, this.DataMember].Current as DataRowView;

            foreach(ColumnParameterMapping mapping in this.ParameterMappings)
            {
                ContextStore context = null;
                Key contextKey = null;
                foreach(ContextStore store in this.Workflow.ChildItemsByType(ContextStore.CategoryConst))
                {
                    if(store.Name == mapping.Name)
                    {
                        contextKey = store.PrimaryKey;
                        context = store;
                    }
                }

                if(contextKey == null)
                {
                    throw new ArgumentOutOfRangeException("mapping.Name", mapping.Name, Origam.Workflow.ResourceUtils.GetString("ErrorContextStoreNotFound"));
                }

                if(mapping.ColumnName == "/")
                {
                    result.Add(mapping.Name, DataDocumentFactory.New(current.Row.Table.DataSet.Copy()));
                }
                else if(mapping.ColumnName == ".")
                {
                    result.Add(mapping.Name, DataDocumentFactory.New(GetDataSlice(context, current.Row)));
                }
                else if(mapping.ColumnName != null && mapping.ColumnName != "")
                {
                    bool found = false;
                    foreach(DataColumn column in current.Row.Table.Columns)
                    {
                        if(column.ColumnName == mapping.ColumnName)
                        {
                            DataRowVersion version = DataRowVersion.Default;

                            result.Add(mapping.Name, current.Row[column, version]);
                            found = true;
                        }
                    }

                    if(!found)
                    {
                        throw new ArgumentOutOfRangeException("Field", mapping.ColumnName, Origam.Workflow.ResourceUtils.GetString("ErrorFieldNotFound"));
                    }
                }
            }

            return result;
        }

    private void HandleException(Exception ex)
    {
            string caption;
            if(ex is RuleException)
            {
                MessageBox.Show(this.FindForm(), ex.Message, RuleEngine.ValidationNotMetMessage(),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                caption = Origam.Workflow.ResourceUtils.GetString("ErrorMessage", this.Text, this.FindForm().Text);
                Origam.UI.AsMessageBox.ShowError(this.FindForm(), ex.Message, caption, ex);
            }

            Cursor.Current = Cursors.Default;
        }

    protected override void Dispose(bool disposing)
    {
            if(disposing)
            {
                _origamMetadata = null;
                _dataSource = null;
            }

            base.Dispose(disposing);
        }

    #endregion

    #region IOrigamMetadataConsumer Members

    public AbstractSchemaItem OrigamMetadata
    {
        get
        {
                return _origamMetadata;
            }
        set
        {
                _origamMetadata = value;

                SetIcon();
                FillParameterCache(_origamMetadata as ControlSetItem);
            }
    }
    #endregion
}