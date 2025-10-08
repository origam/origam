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
using System.ComponentModel.Design;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using MoreLinq;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Gui.Designer.Extensions;
using Origam.Gui.Win;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Services;
using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.Editors;
using Origam.Workbench.Services;

namespace Origam.Gui.Designer;

/// <summary>
/// Summary description for Form1.
/// </summary>
///
public class ControlSetEditor : AbstractEditor
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        MethodBase.GetCurrentMethod().DeclaringType
    );

    private System.Windows.Forms.PropertyGrid _propertyGrid;
    private const string origamDataSetName = "OrigamDataSet";
    private eDataSource _dataSourceMode;
    private DataSet _origamData = null;
    private object[] _selectedComponents;
    private bool _isEditingMainVersion = false;

    //private Origam.Gui.FormGenerator _generator = new FormGenerator();
    private ControlItem _panelControlItemRef = null;
    private bool ReflectChanges = false;
    private ISchemaService _schema =
        ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
    IDocumentationService _documentation =
        ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;
    private UserControlSchemaItemProvider _controls;
    private DataStructureSchemaItemProvider _dsProvider;
    private EntityModelSchemaItemProvider _deProvider;

    //Object of inerest (Root designer component)
    private Control _form;
    private Control _designView;
    private DesignerHostImpl _host;
    private IServiceContainer _serviceContainer;
    private System.Windows.Forms.Panel designSurfacePanel;
    private System.Windows.Forms.Splitter splitter2;
    private System.Windows.Forms.Panel panelProp;
    private System.Windows.Forms.TextBox txtName;
    private System.Windows.Forms.TextBox txtFeatures;
    private System.Windows.Forms.TextBox txtRoles;
    private System.Windows.Forms.NumericUpDown txtLevel;
    private System.Windows.Forms.Label lblControlSetName;
    private System.Windows.Forms.Label lblFeatures;
    private System.Windows.Forms.Label lblRoles;
    private System.Windows.Forms.Label lblLevel;
    private System.Windows.Forms.ComboBox cmbDataSources;
    private System.Windows.Forms.Label lblDataSource;
    private System.Windows.Forms.Label lblToolbox;
    private System.Windows.Forms.Label lblId;
    private System.Windows.Forms.TextBox txtId;
    private System.Windows.Forms.Label lblPackage;
    private System.Windows.Forms.TextBox txtPackage;
    private Origam.Gui.Designer.ToolboxPane _toolbox;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public ControlSetEditor()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();
        _controls =
            _schema.GetProvider(typeof(UserControlSchemaItemProvider))
            as UserControlSchemaItemProvider;
        _dsProvider =
            _schema.GetProvider(typeof(DataStructureSchemaItemProvider))
            as DataStructureSchemaItemProvider;
        _deProvider =
            _schema.GetProvider(typeof(EntityModelSchemaItemProvider))
            as EntityModelSchemaItemProvider;
        //Intitializing host designer
        _serviceContainer = new ServiceContainer();
        _serviceContainer.AddService(typeof(ToolboxPane), _toolbox);
        // create host
        _host = new DesignerHostImpl(_serviceContainer);
        _host.ParentControl = this;
        _toolbox.Host = _host;
        //add hook the designers events
        _host.ComponentRemoved += new ComponentEventHandler(Host_componentRemoved);
        _host.ComponentChanged += new ComponentChangedEventHandler(Host_componentChanged);
        _host.ComponentAdded += new ComponentEventHandler(Host_componentAdded);
        _host.Activated += new EventHandler(_host_Activated);

        this.ContentLoaded += new EventHandler(ControlSetEditor_ContentLoaded);
        propertyPad =
            WorkbenchSingleton.Workbench.GetPad(typeof(Origam.UI.IPropertyPad))
            as Origam.UI.IPropertyPad;

        _propertyGrid = propertyPad.PropertyGrid;
        //_filter = new KeystrokeMessageFilter(_host);
        //Application.AddMessageFilter(_filter);
        this.BackColor = OrigamColorScheme.FormBackgroundColor;
    }

    bool _disposing = false;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        _disposing = true;
        ReflectChanges = false;
        try
        {
            if (disposing)
            {
                _host.ComponentRemoved -= new ComponentEventHandler(Host_componentRemoved);
                _host.ComponentChanged -= new ComponentChangedEventHandler(Host_componentChanged);
                _host.ComponentAdded -= new ComponentEventHandler(Host_componentAdded);
                this.ContentLoaded -= new EventHandler(ControlSetEditor_ContentLoaded);
                ISelectionService selectionService =
                    _host.GetService(typeof(ISelectionService)) as ISelectionService;
                selectionService.SelectionChanged -= new EventHandler(
                    selectionService_SelectionChanged
                );
                _toolbox.Dispose();
                _toolbox = null;
                _host.Dispose();
                _host = null;
                if (_form != null && !_form.IsDisposed)
                {
                    _form.Dispose();
                }
                _origamData = null;
                _controls = null;
                _deProvider = null;
                _dsProvider = null;
                _panelControlItemRef = null;
                _propertyGrid = null;
                _rootControl = null;
                _schema = null;
                _form = null;

                _designView.Dispose();
                _designView = null;
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }
        finally
        {
            _disposing = false;
        }
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.designSurfacePanel = new System.Windows.Forms.Panel();
        this.splitter2 = new System.Windows.Forms.Splitter();
        this.panelProp = new System.Windows.Forms.Panel();
        this.lblToolbox = new System.Windows.Forms.Label();
        this.txtPackage = new System.Windows.Forms.TextBox();
        this.lblPackage = new System.Windows.Forms.Label();
        this.txtId = new System.Windows.Forms.TextBox();
        this.lblId = new System.Windows.Forms.Label();
        this.txtLevel = new System.Windows.Forms.NumericUpDown();
        this.lblLevel = new System.Windows.Forms.Label();
        this.txtFeatures = new System.Windows.Forms.TextBox();
        this.lblFeatures = new System.Windows.Forms.Label();
        this.txtRoles = new System.Windows.Forms.TextBox();
        this.lblRoles = new System.Windows.Forms.Label();
        this.txtName = new System.Windows.Forms.TextBox();
        this.lblControlSetName = new System.Windows.Forms.Label();
        this.cmbDataSources = new System.Windows.Forms.ComboBox();
        this.lblDataSource = new System.Windows.Forms.Label();
        this._toolbox = new Origam.Gui.Designer.ToolboxPane();
        this.panelProp.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.txtLevel)).BeginInit();
        this.SuspendLayout();
        //
        // designSurfacePanel
        //
        this.designSurfacePanel.AutoScroll = true;
        this.designSurfacePanel.BackColor = System.Drawing.Color.White;
        this.designSurfacePanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.designSurfacePanel.Location = new System.Drawing.Point(200, 40);
        this.designSurfacePanel.Name = "designSurfacePanel";
        this.designSurfacePanel.Size = new System.Drawing.Size(558, 527);
        this.designSurfacePanel.TabIndex = 1;
        //
        // splitter2
        //
        this.splitter2.Location = new System.Drawing.Point(200, 40);
        this.splitter2.Name = "splitter2";
        this.splitter2.Size = new System.Drawing.Size(4, 527);
        this.splitter2.TabIndex = 5;
        this.splitter2.TabStop = false;
        //
        // panelProp
        //
        this.panelProp.Controls.Add(this._toolbox);
        this.panelProp.Controls.Add(this.lblToolbox);
        this.panelProp.Controls.Add(this.txtPackage);
        this.panelProp.Controls.Add(this.lblPackage);
        this.panelProp.Controls.Add(this.txtId);
        this.panelProp.Controls.Add(this.lblId);
        this.panelProp.Controls.Add(this.txtLevel);
        this.panelProp.Controls.Add(this.lblLevel);
        this.panelProp.Controls.Add(this.txtFeatures);
        this.panelProp.Controls.Add(this.lblFeatures);
        this.panelProp.Controls.Add(this.txtRoles);
        this.panelProp.Controls.Add(this.lblRoles);
        this.panelProp.Controls.Add(this.txtName);
        this.panelProp.Controls.Add(this.lblControlSetName);
        this.panelProp.Controls.Add(this.cmbDataSources);
        this.panelProp.Controls.Add(this.lblDataSource);
        this.panelProp.Dock = System.Windows.Forms.DockStyle.Left;
        this.panelProp.Location = new System.Drawing.Point(0, 40);
        this.panelProp.Name = "panelProp";
        this.panelProp.Padding = new System.Windows.Forms.Padding(4, 0, 0, 4);
        this.panelProp.Size = new System.Drawing.Size(200, 527);
        this.panelProp.TabIndex = 4;
        //
        // lblToolbox
        //
        this.lblToolbox.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblToolbox.Location = new System.Drawing.Point(4, 271);
        this.lblToolbox.Name = "lblToolbox";
        this.lblToolbox.Size = new System.Drawing.Size(196, 19);
        this.lblToolbox.TabIndex = 9;
        this.lblToolbox.Text = "Toolbox:";
        this.lblToolbox.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        //
        // txtPackage
        //
        this.txtPackage.Dock = System.Windows.Forms.DockStyle.Top;
        this.txtPackage.Location = new System.Drawing.Point(4, 251);
        this.txtPackage.Name = "txtPackage";
        this.txtPackage.ReadOnly = true;
        this.txtPackage.Size = new System.Drawing.Size(196, 20);
        this.txtPackage.TabIndex = 13;
        //
        // lblPackage
        //
        this.lblPackage.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblPackage.Location = new System.Drawing.Point(4, 232);
        this.lblPackage.Name = "lblPackage";
        this.lblPackage.Size = new System.Drawing.Size(196, 19);
        this.lblPackage.TabIndex = 12;
        this.lblPackage.Text = "Package";
        this.lblPackage.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        //
        // txtId
        //
        this.txtId.Dock = System.Windows.Forms.DockStyle.Top;
        this.txtId.Location = new System.Drawing.Point(4, 212);
        this.txtId.Name = "txtId";
        this.txtId.ReadOnly = true;
        this.txtId.Size = new System.Drawing.Size(196, 20);
        this.txtId.TabIndex = 11;
        //
        // lblId
        //
        this.lblId.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblId.Location = new System.Drawing.Point(4, 193);
        this.lblId.Name = "lblId";
        this.lblId.Size = new System.Drawing.Size(196, 19);
        this.lblId.TabIndex = 10;
        this.lblId.Text = "Id:";
        this.lblId.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        //
        // txtLevel
        //
        this.txtLevel.Dock = System.Windows.Forms.DockStyle.Top;
        this.txtLevel.Location = new System.Drawing.Point(4, 173);
        this.txtLevel.Maximum = new decimal(new int[] { 2147483647, 0, 0, 0 });
        this.txtLevel.Name = "txtLevel";
        this.txtLevel.Size = new System.Drawing.Size(196, 20);
        this.txtLevel.TabIndex = 5;
        this.txtLevel.TextChanged += new System.EventHandler(this.txtLevel_TextChanged);
        //
        // lblLevel
        //
        this.lblLevel.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblLevel.Location = new System.Drawing.Point(4, 154);
        this.lblLevel.Name = "lblLevel";
        this.lblLevel.Size = new System.Drawing.Size(196, 19);
        this.lblLevel.TabIndex = 6;
        this.lblLevel.Text = "Level:";
        this.lblLevel.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        //
        // txtFeatures
        //
        this.txtFeatures.Dock = System.Windows.Forms.DockStyle.Top;
        this.txtFeatures.Location = new System.Drawing.Point(4, 134);
        this.txtFeatures.Name = "txtFeatures";
        this.txtFeatures.Size = new System.Drawing.Size(196, 20);
        this.txtFeatures.TabIndex = 5;
        this.txtFeatures.TextChanged += new System.EventHandler(this.txtFeatures_TextChanged);
        //
        // lblFeatures
        //
        this.lblFeatures.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblFeatures.Location = new System.Drawing.Point(4, 115);
        this.lblFeatures.Name = "lblFeatures";
        this.lblFeatures.Size = new System.Drawing.Size(196, 19);
        this.lblFeatures.TabIndex = 6;
        this.lblFeatures.Text = "Features:";
        this.lblFeatures.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        //
        // txtRoles
        //
        this.txtRoles.Dock = System.Windows.Forms.DockStyle.Top;
        this.txtRoles.Location = new System.Drawing.Point(4, 95);
        this.txtRoles.Name = "txtRoles";
        this.txtRoles.Size = new System.Drawing.Size(196, 20);
        this.txtRoles.TabIndex = 5;
        this.txtRoles.TextChanged += new System.EventHandler(this.txtRoles_TextChanged);
        //
        // lblRoles
        //
        this.lblRoles.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblRoles.Location = new System.Drawing.Point(4, 76);
        this.lblRoles.Name = "lblRoles";
        this.lblRoles.Size = new System.Drawing.Size(196, 19);
        this.lblRoles.TabIndex = 6;
        this.lblRoles.Text = "Roles:";
        this.lblRoles.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        //
        // txtName
        //
        this.txtName.Dock = System.Windows.Forms.DockStyle.Top;
        this.txtName.Location = new System.Drawing.Point(4, 56);
        this.txtName.Name = "txtName";
        this.txtName.Size = new System.Drawing.Size(196, 20);
        this.txtName.TabIndex = 5;
        this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
        //
        // lblControlSetName
        //
        this.lblControlSetName.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblControlSetName.Location = new System.Drawing.Point(4, 37);
        this.lblControlSetName.Name = "lblControlSetName";
        this.lblControlSetName.Size = new System.Drawing.Size(196, 19);
        this.lblControlSetName.TabIndex = 6;
        this.lblControlSetName.Text = "Name:";
        this.lblControlSetName.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        //
        // cmbDataSources
        //
        this.cmbDataSources.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
        this.cmbDataSources.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
        this.cmbDataSources.Dock = System.Windows.Forms.DockStyle.Top;
        this.cmbDataSources.Location = new System.Drawing.Point(4, 16);
        this.cmbDataSources.Name = "cmbDataSources";
        this.cmbDataSources.Size = new System.Drawing.Size(196, 21);
        this.cmbDataSources.TabIndex = 3;
        this.cmbDataSources.SelectedIndexChanged += new System.EventHandler(
            this.cmbDataSources_SelectedIndexChanged
        );
        //
        // lblDataSource
        //
        this.lblDataSource.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblDataSource.Location = new System.Drawing.Point(4, 0);
        this.lblDataSource.Name = "lblDataSource";
        this.lblDataSource.Size = new System.Drawing.Size(196, 16);
        this.lblDataSource.TabIndex = 4;
        this.lblDataSource.Text = "Data Source:";
        this.lblDataSource.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
        //
        // _toolbox
        //
        this._toolbox.Dock = System.Windows.Forms.DockStyle.Fill;
        this._toolbox.Host = null;
        this._toolbox.Location = new System.Drawing.Point(4, 290);
        this._toolbox.Name = "_toolbox";
        this._toolbox.Size = new System.Drawing.Size(196, 233);
        this._toolbox.TabIndex = 0;
        //
        // ControlSetEditor
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(837, 567);
        this.Controls.Add(this.splitter2);
        this.Controls.Add(this.designSurfacePanel);
        this.Controls.Add(this.panelProp);
        this.Font = new System.Drawing.Font(
            "Microsoft Sans Serif",
            8.25F,
            System.Drawing.FontStyle.Regular,
            System.Drawing.GraphicsUnit.Point,
            ((byte)(238))
        );
        this.KeyPreview = true;
        this.Name = "ControlSetEditor";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Closed += new System.EventHandler(this.ControlSetEditor_Closed);
        this.Load += new System.EventHandler(this.ControlSetEditor_Load);
        this.Enter += new System.EventHandler(this.ControlSetEditor_Enter);
        this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ControlSetEditor_KeyDown);
        this.Leave += new System.EventHandler(this.ControlSetEditor_Leave);
        this.Controls.SetChildIndex(this.panelProp, 0);
        this.Controls.SetChildIndex(this.designSurfacePanel, 0);
        this.Controls.SetChildIndex(this.splitter2, 0);
        this.panelProp.ResumeLayout(false);
        this.panelProp.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.txtLevel)).EndInit();
        this.ResumeLayout(false);
        this.PerformLayout();
    }
    #endregion
    #region private methods
    private void SaveMainItem()
    {
        try
        {
            ModelContent.PersistenceProvider.BeginTransaction();
            bool saveControl = false;
            if (!ControlSet.IsPersisted)
            {
                //save new control
                saveControl = true;
            }
            ControlSet.ClearCacheOnPersist = false;
            ControlSet.Persist();
            //			(_controlSet as ISchemaItem).ClearCacheOnPersist = true;
            //			_controlSet.Persist();
            // If the controlset was cloned, we clone its documentation, too.
            if (ControlSet.OldPrimaryKey != null)
            {
                List<ISchemaItem> items = ControlSet.ChildItemsRecursive;
                items.Add(ControlSet);
                _documentation.CloneDocumentation(items);
            }
            ControlSet.OldPrimaryKey = null;
            if (saveControl && IsPanel && _panelControlItemRef != null)
            {
                ControlItem newControl = _controls.NewItem<ControlItem>(
                    _schema.ActiveSchemaExtensionId,
                    null
                );
                newControl.Name = ControlSet.Name;
                newControl.IsComplexType = true;
                Type t = typeof(PanelControlSet);
                newControl.ControlType = t.ToString();
                newControl.ControlNamespace = t.Namespace;
                newControl.PanelControlSet = this.Panel;
                newControl.ControlToolBoxVisibility = ControlToolBoxVisibility.FormDesigner;
                SchemaItemAncestor ancestor = new SchemaItemAncestor();
                ancestor.SchemaItem = newControl;
                ancestor.Ancestor = _panelControlItemRef;
                ancestor.PersistenceProvider = newControl.PersistenceProvider;
                newControl.ThrowEventOnPersist = false;
                newControl.Persist();
                ancestor.Persist();
                //				newControl.ClearCacheOnPersist = true;
                //				newControl.Persist();
                newControl.ThrowEventOnPersist = true;
                return;
            }
            //object already exists in database so just update the controlName,
            //but only in case of PanelControlSet
            if (IsPanel)
            {
                ControlItem cn = FindControlItemByPanelRef(this.Panel);
                if (cn != null)
                {
                    cn.Name = this.ControlSet.Name;
                    cn.ThrowEventOnPersist = false;
                    cn.Persist();
                    cn.ThrowEventOnPersist = true;
                }
            }
        }
        finally
        {
            ModelContent.PersistenceProvider.EndTransaction();
        }
    }

    private Control LoadControl(ControlSetItem cntrlSet)
    {
        //create control
        Control cntrl = CreateInstance(cntrlSet);

        cntrl.DataBindings.CollectionChanged += new CollectionChangeEventHandler(
            DataBindings_CollectionChanged
        );
        //adding child controls
        var sortedChildControls = cntrlSet
            .ChildItemsByType<ControlSetItem>("ControlSetItem")
            .ToList();
        sortedChildControls.Sort();
        var invalidControls = new List<ControlSetItem>();
        foreach (ControlSetItem childItem in sortedChildControls)
        {
            try
            {
                Control c = LoadControl(childItem);
                cntrl.Controls.Add(c);
            }
            catch (Exception ex)
            {
                invalidControls.Add(childItem);
                throw new Exception(
                    "Error occured while generating form. ControlSet: '" + cntrlSet.Path + "'.",
                    ex
                );
            }
        }
        foreach (ControlSetItem invalidItem in invalidControls)
        {
            invalidItem.Delete();
            this.IsDirty = true;
        }
        return cntrl;
    }

    private Control CreateInstance(ControlSetItem cntrlSet)
    {
        object result = null;

        if (
            cntrlSet == null
            || cntrlSet.ControlItem == null
            || cntrlSet.ControlItem.ControlType == null
            || cntrlSet.ControlItem.ControlNamespace == null
        )
            throw new ArgumentException(
                "Parameter is null or inner parameters are null",
                "cntrlSet"
            );

        Type type;
#pragma warning disable CS0618 // Type or member is obsolete
        Assembly asm = Assembly.LoadWithPartialName(cntrlSet.ControlItem.ControlNamespace);
#pragma warning restore CS0618 // Type or member is obsolete
        type = asm.GetType(cntrlSet.ControlItem.ControlType);
        if (type == null)
            throw new NullReferenceException(
                "Unsupported type:" + cntrlSet.ControlItem.ControlType
            );

        if (cntrlSet.ControlItem.IsComplexType)
        {
            _host.PanelSet = cntrlSet.ControlItem.PanelControlSet;
            _host.IsComplexControl = true;
        }

        result = _host.CreateComponent(type, cntrlSet.ControlItem, null);
        if (result == null || (!(result is Control)))
            throw new NullReferenceException("Unsupported type: " + type.ToString());

        //very important for editing (we store in TAG all metadata (just in design time)
        (result as Control).Tag = cntrlSet;
        ControlProperties(result as Control, false);
        ControlProperties(result as Control, false);
        UpdateSpecificControlProperties(result, cntrlSet);
        LoadControlBindings(result as Control);

        return (result as Control);
    }

    private void UpdateSpecificControlProperties(object control, ISchemaItem metadata)
    {
        if (control is IOrigamMetadataConsumer)
        {
            (control as IOrigamMetadataConsumer).OrigamMetadata = metadata;
        }

        UpdateDataSource(control);
    }

    private void UpdateDataSource(object control)
    {
        IAsDataConsumer dataConsumer = control as IAsDataConsumer;
        if (this.IsForm && dataConsumer != null && _origamData != null)
        {
            dataConsumer.DataSource = _origamData;
        }
        AsPanel panel = control as AsPanel;
        if (
            panel != null
            && string.IsNullOrEmpty(panel.DataMember)
            && _dataSourceMode == eDataSource.DataStructure
        )
        {
            foreach (DataTable table in _origamData.Tables)
            {
                Guid entityId = ((ControlSetItem)panel.Tag)
                    .ControlItem
                    .PanelControlSet
                    .DataSourceId;
                if (entityId.Equals(table.ExtendedProperties["EntityId"]))
                {
                    DataTable parentTable = table;
                    string dataMember = table.TableName;
                    while (parentTable.ParentRelations.Count == 1)
                    {
                        parentTable = parentTable.ParentRelations[0].ParentTable;
                        dataMember = parentTable.TableName + "." + dataMember;
                    }
                    panel.DataMember = dataMember;
                    IsDirty = true;
                    ControlProperties(control as Control, true);
                    break;
                }
            }
        }
    }

    private void InitDesignerServices()
    {
        AddBaseServices();
        if (_form == null)
        {
            throw new NullReferenceException(
                "Component wasnt proprerly generated and will be null"
            );
        }
        IRootDesigner rootDesigner = (IRootDesigner)_host.GetDesigner(_form);
        _designView = (Control)rootDesigner.GetView(ViewTechnology.Default);
        _designView.Dock = DockStyle.Fill;
        designSurfacePanel.Controls.Add(_designView);
        // we need to subscribe to selection changed events so
        // that we can update our properties grid
        ISelectionService selectionService =
            _host.GetService(typeof(ISelectionService)) as ISelectionService;
        selectionService.SelectionChanged += new EventHandler(selectionService_SelectionChanged);
        if (cmbDataSources.SelectedItem != null)
        {
            AddDataset();
            _host.DataSet = _origamData;
        }
        // activate the host
        _host.Activate();
        _host.OnLoadComplete();
    }

    private void AddDataset()
    {
        _host.Add(_origamData, origamDataSetName);
        // hide component tray
        foreach (IExtenderProvider provider in this._host.GetExtenderProviders())
        {
            System.Windows.Forms.Design.ComponentTray tray =
                provider as System.Windows.Forms.Design.ComponentTray;
            if (tray != null)
            {
                tray.Hide();
            }
        }
    }

    private void AddBaseServices()
    {
        _toolbox.LoadToolbox(LoadToolbox());
    }

    /// <summary>
    /// Loads or saves properties of a control.
    /// </summary>
    /// <param name="cntrl">Control of which properties should be loaded/saved.</param>
    /// <param name="save">True if properties should be saved. False if properties should be loaded.</param>
    private void ControlProperties(Control cntrl, bool save)
    {
        if (cntrl.Tag is ControlSetItem)
        {
            ControlSetItem cntrSetItem = cntrl.Tag as ControlSetItem;
            var properties = new List<PropertyInfo>();
            foreach (
                var propItem in cntrSetItem.ControlItem.ChildItemsByType<ControlPropertyItem>(
                    ControlPropertyItem.CategoryConst
                )
            )
            {
                //addinng default properties to control set item
                Type t = cntrl.GetType();
                PropertyInfo property = t.GetProperty(propItem.Name);
                properties.Add(property);
                if (property == null)
                    throw new ArgumentOutOfRangeException(
                        "Name",
                        propItem.Name,
                        "Property '"
                            + propItem.Name
                            + "' not found in the control '"
                            + t.ToString()
                            + "'"
                    );
                PropertyValueItem propValItem =
                    FindPropertyValueItem(cntrSetItem, propItem, false) as PropertyValueItem;
                if (propValItem != null)
                {
                    if (save) //Setting properties
                    {
                        propValItem.ControlPropertyItem = propItem as ControlPropertyItem;
                        propValItem.Name = property.Name;
                        FormGenerator.SavePropertyValue(cntrl, property, propValItem);
                    }
                    else //loading properites
                    {
                        FormGenerator.LoadPropertyValue(cntrl, property, propValItem);
                    }
                }
            }
#if DEBUG
            string architectExePath = Assembly.GetExecutingAssembly().Location;
            string architectBinDir = Path.GetDirectoryName(architectExePath)!;
            string backendDir = Path.GetFullPath(Path.Combine(architectBinDir, "..", "..", ".."));
            string path = Path.Combine(
                backendDir,
                "Origam.Architect.Server\\Controls",
                cntrl.GetType().Name + ".cs"
            );
            if (!File.Exists(path))
            {
                string newClass = ClassGenerator.GenerateClass(cntrl.GetType().Name, properties);
                File.WriteAllText(path, newClass);
            }
#endif
        }
    }

    private void LoadControlBindings(Control cntrl)
    {
        if (cntrl == null || _origamData == null || (!(cntrl.Tag is ControlSetItem)))
            return;
        ControlSetItem cntrSetItem = cntrl.Tag as ControlSetItem;

        foreach (
            var bindItem in cntrSetItem.ChildItemsByType<PropertyBindingInfo>(
                PropertyBindingInfo.CategoryConst
            )
        )
        {
            try
            {
                if (this._dataSourceMode == eDataSource.DataEntity)
                {
                    cntrl.DataBindings.Add(
                        bindItem.ControlPropertyItem.Name,
                        _origamData,
                        _origamData.Tables[0].TableName + "." + bindItem.Value
                    );
                }
                else
                {
                    cntrl.DataBindings.Add(
                        bindItem.ControlPropertyItem.Name,
                        _origamData,
                        bindItem.DesignDataSetPath
                    );
                }
            }
            catch
            {
                //					throw new NullReferenceException("Property"
                //						+ bindItem.ControlPropertyItem.Name
                //						+ "probably doen't exist or wrong argument",ex);
            }
        }
    }

    private void SaveControlBindings(Binding bind, CollectionChangeAction action)
    {
        if (bind == null)
            return;

        Control cntrl = bind.Control;
        if (cntrl == null)
            return;

        ControlPropertyItem propItem = null;
        if (!(cntrl.Tag is ControlSetItem))
            return;
        ControlSetItem cntrSetItem = cntrl.Tag as ControlSetItem;
        propItem = FindPropertyItem(cntrl, bind.PropertyName);

        if (propItem == null)
            throw new NullReferenceException(
                "Property "
                    + bind.PropertyName
                    + " definition (control: "
                    + cntrl.Name
                    + ") doesn't exists"
            );
        PropertyBindingInfo propertyBind =
            FindPropertyValueItem(cntrSetItem, propItem, true) as PropertyBindingInfo;

        if (action == CollectionChangeAction.Remove)
        {
            propertyBind.IsDeleted = true;
            return;
        }

        if (propertyBind == null)
            throw new NullReferenceException(
                "Property binding value ("
                    + bind.PropertyName
                    + ") definition (control: "
                    + cntrl.Name
                    + ") doesn't exists or can't creat new one"
            );
        propertyBind.ControlPropertyItem = propItem;
        propertyBind.Name = bind.PropertyName;
        propertyBind.Value = bind.BindingMemberInfo.BindingField;
        propertyBind.DesignDataSetPath = bind.BindingMemberInfo.BindingMember;
    }

    private ControlPropertyItem FindPropertyItem(Control cntrl, string propertyName)
    {
        if (!(cntrl.Tag is ControlSetItem))
            return null;
        ControlSetItem cntrSetItem = cntrl.Tag as ControlSetItem;
        foreach (
            var propItem in cntrSetItem.ControlItem.ChildItemsByType<ControlPropertyItem>(
                ControlPropertyItem.CategoryConst
            )
        )
        {
            if (propItem.Name.ToUpper() == propertyName.ToUpper())
                return propItem;
        }
        return null;
    }

    private AbstractPropertyValueItem FindPropertyValueItem(
        ControlSetItem controlSetItem,
        ControlPropertyItem propertyToFind,
        bool bind
    )
    {
        AbstractPropertyValueItem result = null;
        var strType = //name of property type (property value or binding info)
        bind ? PropertyBindingInfo.CategoryConst : PropertyValueItem.CategoryConst;
        foreach (var item in controlSetItem.ChildItemsByType<AbstractPropertyValueItem>(strType))
        {
            if (Equals(item.ControlPropertyItem?.PrimaryKey, propertyToFind.PrimaryKey))
            {
                result = item;
                break;
            }
        }

        if (result == null)
        {
            //no record found then we create a new one /proretyvalue or property binding
            if (bind)
            {
                result = controlSetItem.NewItem<PropertyBindingInfo>(
                    _schema.ActiveSchemaExtensionId,
                    null
                );
            }
            else
            {
                result = controlSetItem.NewItem<PropertyValueItem>(
                    _schema.ActiveSchemaExtensionId,
                    null
                );
            }
            result.ControlPropertyItem = propertyToFind;
            result.Name = propertyToFind.Name;
        }
        return result;
    }

    private FDToolbox LoadToolbox()
    {
        FDToolbox tools = new FDToolbox();
        Category[] cats = new Category[2];
        if (this.IsForm)
        {
            //For Form Document we load second category with Panels
            Category catPanels = new Category();
            catPanels.DisplayName = "Screen Sections";
            catPanels.FDToolboxItem = LoadTools(_controls, KindOfTool.Panels);
            cats[0] = catPanels;
        }
        else if (this.IsPanel && this.Panel.DataEntity != null)
        {
            IDataEntity dataEntity = this.Panel.DataEntity;
            Category panelCat2 = new Category();
            panelCat2.DisplayName = dataEntity.Name;
            FDToolboxItem[] fieldTools = new FDToolboxItem[
                dataEntity
                    .ChildItemsByType<IDataEntityColumn>(AbstractDataEntityColumn.CategoryConst)
                    .Count
            ];
            int i = 0;
            FDToolboxItem fd_item;
            var fields = dataEntity.ChildItemsByType<IDataEntityColumn>(
                AbstractDataEntityColumn.CategoryConst
            );
            fields.Sort();
            foreach (IDataEntityColumn column in fields)
            {
                fd_item = new FDToolboxItem();
                fd_item.Type = ToolBoxConverter.Convert(column);
                fd_item.Name = column.Name;
                fd_item.IsComplexType = false;
                fd_item.PanelSetItem = null;
                fd_item.IsFieldItem = true;
                fd_item.OrigamDataType = column.DataType;
                fd_item.ColumnName = column.Name;
                fieldTools[i] = fd_item;
                i++;
            }
            panelCat2.FDToolboxItem = fieldTools;
            cats[0] = panelCat2;
        }
        // Load general widgets
        Category basicControls = new Category();
        basicControls.DisplayName = "Widgets";
        basicControls.FDToolboxItem = LoadTools(_controls, KindOfTool.Basic);
        cats[1] = basicControls;

        tools.FDToolboxCategories = cats;
        return tools;
    }

    private enum KindOfTool
    {
        Basic,
        Panels,
    };

    private FDToolboxItem[] LoadTools(ISchemaItemProvider controls, KindOfTool kind)
    {
        if (controls == null)
        {
            return null;
        }
        FDToolboxItem[] tool = new FDToolboxItem[controls.ChildItems.Count];
        int i = 0;
        List<ISchemaItem> controlList = controls.ChildItems.ToList();
        controlList.Sort();
        foreach (ControlItem item in controlList)
        {
            if (CheckControlItemForToolbox(item))
            {
                //Basic controls
                if (kind == KindOfTool.Basic && (!item.IsComplexType))
                {
                    //Basic panel controls
                    if (
                        this.IsPanel
                        && (
                            item.ControlToolBoxVisibility == ControlToolBoxVisibility.PanelDesigner
                            || item.ControlToolBoxVisibility
                                == ControlToolBoxVisibility.PanelAndFormDesigner
                        )
                    )
                    {
                        FDToolboxItem fd_item = new FDToolboxItem();
                        fd_item.Type = item.ControlType + "," + item.ControlNamespace;
                        fd_item.Name = item.Name;
                        tool[i] = fd_item;
                        i++;
                    }
                    else if (
                        this.IsForm
                        && (
                            item.ControlToolBoxVisibility == ControlToolBoxVisibility.FormDesigner
                            || item.ControlToolBoxVisibility
                                == ControlToolBoxVisibility.PanelAndFormDesigner
                        )
                    )
                    {
                        FDToolboxItem fd_item = new FDToolboxItem();
                        fd_item.Type = item.ControlType + "," + item.ControlNamespace;
                        fd_item.Name = item.Name;
                        fd_item.ControlItem = item;
                        tool[i] = fd_item;
                        i++;
                    }
                }
                else if (kind == KindOfTool.Panels)
                {
                    if (
                        item.IsComplexType
                        && item.ControlToolBoxVisibility != ControlToolBoxVisibility.Nowhere
                    )
                    {
                        FDToolboxItem fd_item = new FDToolboxItem();
                        fd_item.Type = item.ControlType + "," + item.ControlNamespace;
                        if (item.PanelControlSet == null)
                        {
                            throw new Exception(
                                $"Item {item.Name}, Id: {item.Id} cannot be displayed because its control set is null. Please make sure the item is valid."
                            );
                        }
                        fd_item.Name = item.PanelControlSet.Name;
                        fd_item.IsComplexType = true;
                        fd_item.PanelSetItem = item.PanelControlSet;
                        tool[i] = fd_item;
                        i++;
                    }
                }
            }
        }
        FDToolboxItem[] result = new FDToolboxItem[i];
        Array.Copy(tool, result, i);
        return result;
    }

    private bool CheckControlItemForToolbox(ControlItem control)
    {
        if (control.ControlType == "Origam.Gui.Win.AsForm")
            return false;

        if (control.ControlType == _rootControl.ControlItem.ControlType)
            return false;
        return true;
    }

    private ControlItem GetControlbyType(Type type)
    {
        ControlItem con = null;
        if (_controls == null)
            _controls =
                _schema.GetProvider(typeof(UserControlSchemaItemProvider))
                as UserControlSchemaItemProvider;
        foreach (ControlItem item in _controls.ChildItems)
        {
            ModelToolboxItem tbi = _toolbox.SelectedTool as ModelToolboxItem;
            if (tbi != null && tbi.IsComplexType && tbi.PanelControlSet != null)
            {
                PanelControlSet compare = tbi.PanelControlSet;
                if (
                    item.IsComplexType && item.PanelControlSet.PrimaryKey.Equals(compare.PrimaryKey)
                )
                {
                    con = item;
                    break;
                }
            }
            else
            {
                ControlItem dynamicTypeControlItem = DynamicTypeFactory.GetAssociatedControlItem(
                    type
                );
                if (dynamicTypeControlItem != null)
                {
                    return dynamicTypeControlItem;
                }
                if (item.ControlType == type.FullName)
                {
                    con = item;
                    break;
                }
            }
        }
        return con;
    }
    #endregion
    public bool IsDesignerHostFocused
    {
        get { return this.ActiveControl.Parent != panelProp; }
    }

    protected override object GetService(Type service)
    {
        if (_serviceContainer != null && _serviceContainer.GetService(service) != null)
        {
            return _serviceContainer.GetService(service);
        }
        if (_host == null)
            return null;
        return _host.GetService(service);
    }

    private IMenuCommandService MenuService()
    {
        return GetService(typeof(IMenuCommandService)) as IMenuCommandService;
    }

    private void selectionService_SelectionChanged(object sender, EventArgs e)
    {
        ISelectionService selectionService =
            _host.GetService(typeof(ISelectionService)) as ISelectionService;
        if (_propertyGrid != null && selectionService != null)
        {
            ICollection selection = selectionService.GetSelectedComponents();
            // if nothing is selected, just select the root component
            if (selection == null || selection.Count == 0)
            {
                if (!_disposing)
                {
                    propertyPad.ReadOnlyGetter = () => IsReadOnly;
                    _propertyGrid.SelectedObjects = new object[] { _host.RootComponent };
                }
            }
            // we have to copy over the selected components list
            // into an array and then set the selectedObjects property
            _selectedComponents = new Object[selection.Count];

            selection.CopyTo(_selectedComponents, 0);
            bool rightButton = ((Control.MouseButtons & MouseButtons.Right) == MouseButtons.Right);
            if (rightButton)
            {
                this.MenuService()
                    .ShowContextMenu(null, Control.MousePosition.X, Control.MousePosition.Y);
            }
#if SPECIAL_GRID
            Control selectedObject = null;
            if (this.host.Components["propertyComponent"] != null)
            {
                this.host.Remove(this.host.Components["propertyComponent"]);
            }
            if (_selectedComponents.Length > 0)
            {
                selectedObject = _selectedComponents[0] as Control;
            }
            if (selectedObject != null)
            {
                Type t = selectedObject.GetType();
                ControlSetItem ctrlSet = selectedObject.Tag as ControlSetItem;
                if (ctrlSet != null)
                {
                    Settings settings = new Settings();
                    // Load all properties we want to see
                    foreach (
                        ControlPropertyItem propItem in ctrlSet.ControlItem.ChildItemsByType(
                            ControlPropertyItem.CategoryConst
                        )
                    )
                    {
                        PropertyInfo property = t.GetProperty(propItem.Name);
                        object value = property.GetValue(selectedObject, new object[0]);
                        settings[propItem.Name] = new Setting(value, property.PropertyType);
                    }
                    if (selectedObject is AsPanel)
                    {
                        settings["DataSource"] = new Setting(
                            (selectedObject as AsPanel).DataSource,
                            typeof(object)
                        );
                    }
                    // Add bindings, we want to see this always
                    settings["DataBindings"] = new Setting(
                        selectedObject.DataBindings,
                        selectedObject.DataBindings.GetType()
                    );
                    // Add any typeconverters
                    IList members = Reflector.FindMembers(
                        t,
                        typeof(System.ComponentModel.TypeConverterAttribute),
                        new Type[0]
                    );
                    foreach (MemberAttributeInfo mi in members)
                    {
                        System.ComponentModel.TypeConverterAttribute converter =
                            mi.Attribute as System.ComponentModel.TypeConverterAttribute;
                        if (settings[mi.MemberInfo.Name] != null)
                        {
                            settings[mi.MemberInfo.Name].TypeConverter = converter;
                        }
                    }
                    // Ad any UI Type Editors
                    members = Reflector.FindMembers(
                        t,
                        typeof(System.ComponentModel.EditorAttribute),
                        new Type[0]
                    );
                    foreach (MemberAttributeInfo mi in members)
                    {
                        System.ComponentModel.EditorAttribute editor =
                            mi.Attribute as System.ComponentModel.EditorAttribute;
                        if (settings[mi.MemberInfo.Name] != null)
                        {
                            settings[mi.MemberInfo.Name].UITypeEditor = editor;
                        }
                    }
                    settings.DesignerHost = this.host;
                    propertyGrid.Settings = settings;
                }
            }
#else
            if (!_disposing)
            {
                propertyPad.ReadOnlyGetter = () => IsReadOnly;
                _propertyGrid.SelectedObjects = _selectedComponents;
            }
#endif
        }
    }

    private bool _initializingCombos = false;

    private void SettingSelectedItemForDataSourceCombo()
    {
        ISchemaItem schItem = null;
        if (_dataSourceMode == eDataSource.DataStructure)
        {
            schItem = Form.DataStructure;
        }
        else
        {
            schItem = Panel.DataEntity;
        }
        if (schItem != null)
        {
            foreach (ISchemaItem item in cmbDataSources.Items)
            {
                if (schItem.PrimaryKey.Equals(item.PrimaryKey))
                {
                    _initializingCombos = true;
                    cmbDataSources.SelectedItem = item;
                    _initializingCombos = false;
                    break;
                }
            }
        }
    }

    private void InitEditor()
    {
        SettingSelectedItemForDataSourceCombo();
        if (_form == null)
        {
            ToolboxPane.DragAndDropControl = null;
            _form = this.LoadControl(_rootControl);
        }
        if (_isEditingMainVersion)
        {
            txtName.Text = this.ControlSet.Name;
            txtId.Text = this.ControlSet.PrimaryKey["Id"].ToString();
            txtRoles.Hide();
            lblRoles.Hide();
            txtFeatures.Hide();
            lblFeatures.Hide();
            txtLevel.Hide();
            lblLevel.Hide();
        }
        else
        {
            txtName.Text = _rootControl.Name;
            txtId.Text = _rootControl.PrimaryKey["Id"].ToString();
            txtRoles.Text = _rootControl.Roles;
            txtFeatures.Text = _rootControl.Features;
            txtLevel.Text = _rootControl.Level.ToString();
        }
        txtPackage.Text = _rootControl.PackageName;
        _form.Location = new Point(15, 15);
        _form.Enabled = (!this.IsReadOnly);
        //init editor
        InitDesignerServices();
        ReflectChanges = true;
        ControlProperties(_form, true);
    }

    #region Overrides
    private enum eDataSource
    {
        DataEntity,
        DataStructure,
    };

    private void LoadComboDataSources(eDataSource type)
    {
        if (type == eDataSource.DataStructure)
        {
            foreach (ISchemaItem item in _dsProvider.ChildItems)
            {
                cmbDataSources.Items.Add(item);
            }
        }
        else if (type == eDataSource.DataEntity)
        {
            foreach (ISchemaItem item in _deProvider.ChildItems)
            {
                cmbDataSources.Items.Add(item);
            }
        }
        else
        {
            throw new ArgumentException("Wrong type");
        }
        if (cmbDataSources.Items.Count < 1)
            cmbDataSources.Enabled = false;
        cmbDataSources.Sorted = true;
    }

    public override void SaveObject()
    {
        if (cmbDataSources.SelectedItem == null)
            throw new NullReferenceException("No Datasource selected can't save");
        if (_isEditingMainVersion)
        {
            SaveMainItem();
        }
        else
        {
            _rootControl.Roles = txtRoles.Text;
            _rootControl.Features = txtFeatures.Text;
            _rootControl.Level = int.Parse(txtLevel.Text);
            base.SaveObject();
        }
    }

    private ControlItem FindControlItemByPanelRef(PanelControlSet panel)
    {
        if (panel == null || _controls == null)
            return null;

        foreach (ControlItem item in _controls.ChildItems)
        {
            if (
                item.PanelControlSet != null
                && item.PanelControlSet.PrimaryKey.Equals(panel.PrimaryKey)
            )
                return item;
        }
        return null;
    }
    #endregion
    #region Properties
    public bool IsPanel
    {
        get { return _rootControl.ParentItem is PanelControlSet; }
    }
    public bool IsForm
    {
        get { return _rootControl.ParentItem is FormControlSet; }
    }
    public AbstractControlSet ControlSet
    {
        get { return _rootControl.ParentItem as AbstractControlSet; }
    }

    public FormControlSet Form
    {
        get { return ControlSet as FormControlSet; }
    }
    public PanelControlSet Panel
    {
        get { return ControlSet as PanelControlSet; }
    }

    private ControlSetItem _rootControl;
    private IPropertyPad propertyPad;
    #endregion
    #region Add/Remove/change SchemaItem
    private void Host_componentChanged(object sender, ComponentChangedEventArgs evtArgs)
    {
        if ((evtArgs.Member == null) && (evtArgs.OldValue is Control.ControlCollection))
        {
            return;
        }
        if (
            IsReadOnly
            && !Equals(evtArgs.OldValue, evtArgs.NewValue)
            && evtArgs.Member.Category == "Layout"
        ) // changes to non Layout properties are handled by the propertyPad
        {
            Type type = evtArgs.Component.GetType();
            PropertyInfo propertyInfo = type.GetProperty(evtArgs.Member.Name);
            propertyInfo.SetValue(evtArgs.Component, evtArgs.OldValue);
            MessageBox.Show(
                this,
                Origam.Workbench.ResourceUtils.GetString("ErrorElementReadOnly"),
                Origam.Workbench.ResourceUtils.GetString("ErrorTitle"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            return;
        }
        Host_componentAdded(sender, new ComponentEventArgs(evtArgs.Component as IComponent));
    }

    private void Host_componentAdded(object sender, ComponentEventArgs evtArgs)
    {
        if (ReflectChanges && !IsReadOnly)
        {
            this.IsDirty = true;
            Control control = evtArgs.Component as Control;

            if (control == null)
                return;
            if (!(control.Tag is ControlSetItem))
            {
                //control is new in designer we create in their tag new ControlSet Item
                CreateNewControlSetItem(control, evtArgs.Component.Site.Name, null);
                UpdateSpecificControlProperties(control, control.Tag as ISchemaItem);
                // set the newly added TabControl as selected because it adds 2 TabPages immediately
                // after it is constructed and the selected component is the only way how to assign a parent
                if (control is TabControl)
                {
                    _selectedComponents = new object[1] { control };
                }
            }
            else
            {
                if (control.Parent != null && control.Parent.Tag is ControlSetItem)
                {
                    ControlSetItem parentItem = control.Parent.Tag as ControlSetItem;
                    ControlSetItem cntrSet = control.Tag as ControlSetItem;

                    if (cntrSet.IsDeleted)
                        return;

                    if (!parentItem.PrimaryKey.Equals(cntrSet.ParentItem.PrimaryKey))
                    {
                        cntrSet.ParentItem.ChildItems.Remove(cntrSet);
                        parentItem.ChildItems.Add(cntrSet);
                    }
                }
            }
            // set control properties
            ControlProperties(control, true);
        }
    }

    private void CreateNewControlSetItem(Control control, string name, ControlSetItem parent)
    {
        ControlSetItem creator = null;
        ControlItem refControl = GetControlbyType(control.GetType());
        if (refControl != null)
        {
            Control controlContainer = null;

            if (
                _selectedComponents != null
                && _selectedComponents.Length > 0
                && _selectedComponents[0] is Control
            )
            {
                controlContainer = (_selectedComponents[0] as Control);
            }
            if (parent == null)
                creator = (controlContainer.Tag as ControlSetItem);
            else
                creator = parent;
            if (creator == null)
            {
                if (control.Tag is ControlSetItem)
                {
                    creator = (control.Tag as ControlSetItem).ParentItem as ControlSetItem;
                }
            }

            ControlSetItem newItem;
            newItem = creator.NewItem<ControlSetItem>(_schema.ActiveSchemaExtensionId, null);
            newItem.ControlItem = refControl;
            newItem.Name = name;
            control.Name = name;
            control.Tag = newItem;
            control.DataBindings.CollectionChanged += new CollectionChangeEventHandler(
                DataBindings_CollectionChanged
            );

            //When creating control save all bindings which was created by host designer
            foreach (Binding bind in control.DataBindings)
            {
                SaveControlBindings(bind, CollectionChangeAction.Add);
            }
        }
    }

    private void DataBindings_CollectionChanged(object sender, CollectionChangeEventArgs e)
    {
        if (IsReadOnly)
        {
            return;
        }
        this.IsDirty = true;
        if (ReflectChanges)
        {
            SaveControlBindings((e.Element as Binding), e.Action);
        }
    }

    private void Host_componentRemoved(object sender, ComponentEventArgs evtArgs)
    {
        this.IsDirty = true;
        if (evtArgs.Component is Control)
        {
            if (ReflectChanges)
            {
                Control control = evtArgs.Component as Control;
                control.DataBindings.CollectionChanged -= new CollectionChangeEventHandler(
                    DataBindings_CollectionChanged
                );
                if (evtArgs.Component is Control && control.Tag is ControlSetItem)
                {
                    ControlSetItem item = control.Tag as ControlSetItem;
                    item.IsDeleted = true;
                }
            }
        }
    }
    #endregion

    #region DEBUG
    private void DebugBindings(object formik)
    {
        foreach (Control ctrl in ((Control)formik).Controls)
        {
            foreach (Binding bind in ctrl.DataBindings)
            {
                System.Diagnostics.Debug.WriteLine(
                    "***************************************************************************"
                );
                System.Diagnostics.Debug.WriteLine("ControlName:       " + bind.Control.Name);
                System.Diagnostics.Debug.WriteLine(
                    "ControlType:       " + bind.Control.GetType().ToString()
                );
                System.Diagnostics.Debug.WriteLine(
                    "Property:          " + bind.PropertyName.ToString()
                );
                System.Diagnostics.Debug.WriteLine(
                    "BindingMemberInfo: " + bind.BindingMemberInfo.ToString()
                );
                System.Diagnostics.Debug.WriteLine(
                    "Field:             " + bind.BindingMemberInfo.BindingField.ToString()
                );
                System.Diagnostics.Debug.WriteLine(
                    "BindingPath:       " + bind.BindingMemberInfo.BindingPath.ToString()
                );
                System.Diagnostics.Debug.WriteLine(
                    "Member:            " + bind.BindingMemberInfo.BindingMember.ToString()
                );
                System.Diagnostics.Debug.WriteLine(
                    "***************************************************************************"
                );
            }
            if (ctrl.Controls.Count > 0)
            {
                DebugBindings(ctrl);
            }
        }
    }

    private void menuItem6_Click(object sender, System.EventArgs e)
    {
        DebugBindings(_form);
    }
    #endregion
    private void ControlSetEditor_Load(object sender, System.EventArgs e) { }

    private void cmbDataSources_SelectedIndexChanged(object sender, System.EventArgs e)
    {
        if ((sender as ComboBox).SelectedItem == null)
            return;
        if (_dataSourceMode == eDataSource.DataEntity)
        {
            IDataEntity de = (sender as ComboBox).SelectedItem as IDataEntity;
            _origamData = new DatasetGenerator(false).CreateDataSet(de);
            if (!_initializingCombos)
            {
                this.Panel.DataEntity = de;
            }

            //Reload Panel
            if (ReflectChanges)
            {
                AddBaseServices();
                _host.DataSet = _origamData;
            }
        }
        else if (_dataSourceMode == eDataSource.DataStructure)
        {
            DataStructure ds = (sender as ComboBox).SelectedItem as DataStructure;
            try
            {
                _origamData = new DatasetGenerator(false).CreateDataSet(ds);
            }
            catch
            {
                _origamData = new DataSet();
            }

            if (!_initializingCombos)
            {
                this.Form.DataStructure = ds;
            }
        }
        if (_host != null && ReflectChanges)
        {
            if (_host.Components[origamDataSetName] != null)
            {
                _host.DestroyComponent(_host.Components[origamDataSetName]);
            }

            AddDataset();
        }

        if (_origamData != null)
        {
            _origamData.EnforceConstraints = false;
        }
        if (!_initializingCombos)
        {
            txtName.Text = cmbDataSources.SelectedItem.ToString();
        }
        foreach (IComponent childControl in _host.Container.Components)
        {
            UpdateDataSource(childControl);
        }
    }

    private void txtName_TextChanged(object sender, System.EventArgs e)
    {
        string text = (sender as TextBox).Text;
        ISchemaItem item = this.ModelContent;
        if (item.Name != text)
        {
            this.IsDirty = true;
            if (_isEditingMainVersion)
            {
                this.ControlSet.Name = text;
                this._rootControl.Name = text + "RootControl";
            }
            else
            {
                _rootControl.Name = text;
            }
            this.TitleName = text;
        }
    }

    private void txtRoles_TextChanged(object sender, System.EventArgs e)
    {
        if (_rootControl.Roles != txtRoles.Text)
        {
            this.IsDirty = true;
        }
    }

    private void txtFeatures_TextChanged(object sender, System.EventArgs e)
    {
        if (_rootControl.Features != txtFeatures.Text)
        {
            this.IsDirty = true;
        }
    }

    private void txtLevel_TextChanged(object sender, System.EventArgs e)
    {
        if (_rootControl.Level.ToString() != txtLevel.Text)
        {
            this.IsDirty = true;
        }
    }

    private void ControlSetEditor_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
    {
        if (IsReadOnly)
        {
            return;
        }
        IMenuCommandService menuCommandService =
            GetService(typeof(IMenuCommandService)) as IMenuCommandService;

        if (IsDesignerHostFocused)
        {
            if (e.KeyCode == Keys.Delete)
            {
                menuCommandService.GlobalInvoke(StandardCommands.Delete);
            }
            if (e.Control && e.KeyCode == Keys.C)
            {
                menuCommandService.GlobalInvoke(StandardCommands.Copy);
            }
            if (e.Control && e.Shift && e.KeyCode == Keys.T)
            {
                menuCommandService.GlobalInvoke(StandardCommands.TabOrder);
            }
            if (e.Control && e.KeyCode == Keys.X)
            {
                menuCommandService.GlobalInvoke(StandardCommands.Cut);
            }
            if (e.Control && e.KeyCode == Keys.V)
            {
                menuCommandService.GlobalInvoke(StandardCommands.Paste);
            }
        }
    }

    private void ControlSetEditor_ContentLoaded(object sender, EventArgs e)
    {
        Type type;
        AbstractControlSet controlSet = ModelContent as AbstractControlSet;
        if (controlSet == null)
        {
            // we are editing the sub-version of the screen/section
            cmbDataSources.Enabled = false;
            _rootControl = ModelContent as ControlSetItem;
            _rootControl.IsAlternative = true;
            type = ResolveType();
            SetControlItemRef(type);
            if (_rootControl.ChildItems.Count == 0)
            {
                // doesn't have any children so we create new panel as a copy of the main one
                ISchemaItem defaultItem = ControlSet.MainItem;
                foreach (ISchemaItem child in defaultItem.ChildItems)
                {
                    ISchemaItem copy = child.Clone() as ISchemaItem;
                    copy.SetExtensionRecursive(_schema.ActiveExtension);
                    _rootControl.ChildItems.Add(copy);
                }
            }
            InitNewItemEditor();
        }
        else
        {
            _isEditingMainVersion = true;
            // we are editing the main version of the screen/section
            if (controlSet.ChildItems.Count == 0)
            {
                // doesn't have any children so we create new panel
                _rootControl = controlSet.NewItem<ControlSetItem>(
                    _schema.ActiveSchemaExtensionId,
                    null
                );
                type = ResolveType();
                SetControlItemRef(type);
                InitNewItemEditor();
            }
            else
            {
                _rootControl = controlSet.MainItem;
                type = ResolveType();
                SetControlItemRef(type);
            }
        }
        if (this.IsReadOnly)
        {
            txtName.Enabled = false;
            cmbDataSources.Enabled = false;
            _toolbox.Enabled = false;
        }
        if (IsForm)
        {
            _dataSourceMode = eDataSource.DataStructure;
            //Loads all aviable DataStructures into editor
            LoadComboDataSources(_dataSourceMode);
            InitEditor();
        }
        else if (IsPanel)
        {
            _dataSourceMode = eDataSource.DataEntity;
            LoadComboDataSources(_dataSourceMode);
            InitEditor();
        }
        else
        {
            throw new ArgumentException(
                "Unsupported Type to Edit(" + ModelContent.GetType().ToString() + ")",
                "Content"
            );
        }
        this.GetAllControls()
            .OfType<ICanCangeOnPaint>()
            .ForEach(variableControl =>
            {
                variableControl.ModificationStarts += (o, args) => ReflectChanges = false;
                variableControl.ModificationEnds += (o, args) => ReflectChanges = true;
            });
    }

    private void InitNewItemEditor()
    {
        _rootControl.ControlItem = _panelControlItemRef;
        if (_rootControl.ChildItems.Count == 0)
        {
            _form = this.CreateInstance(_rootControl);
            _form.Text = "Design Surface";
            _form.Tag = _rootControl;
            _form.Height = 600;
            _form.Width = 800;
            txtName.Text = ControlSet.GetType().ToString();
        }
    }

    private void SetControlItemRef(Type type)
    {
        _panelControlItemRef = GetControlbyType(type);
        if (_panelControlItemRef == null)
            throw new NullReferenceException(
                "Type " + type + " has no reference in Meta model database"
            );
    }

    private Type ResolveType()
    {
        Type type;
        if (IsPanel)
        {
            type = typeof(AsPanel);
        }
        else if (IsForm)
        {
            type = typeof(AsForm);
        }
        else
        {
            throw new ArgumentException(
                "Unsupported Type to Edit(" + ModelContent.GetType().ToString() + ")",
                "Content"
            );
        }
        return type;
    }

    private void ControlSetEditor_Closed(object sender, System.EventArgs e)
    {
        try
        {
            // refresh the model element, e.g. if editing is cancelled, so it reads its original content
            if (this._rootControl != null && this._rootControl.IsPersisted)
            {
                this._rootControl.ClearCacheOnPersist = true;
                this._rootControl.Refresh();
                this._rootControl.ClearCacheOnPersist = false;
            }
            propertyPad.ReadOnlyGetter = null;
            _propertyGrid.SelectedObjects = null;
        }
        catch (Exception ex)
        {
            log.LogOrigamError(ex);
            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ControlSetEditor_Leave(object sender, System.EventArgs e) { }

    private void ControlSetEditor_Enter(object sender, System.EventArgs e)
    {
        try
        {
            propertyPad.ReadOnlyGetter = () => IsReadOnly;
            _propertyGrid.SelectedObjects = _selectedComponents;
        }
        catch { }
    }

    private void _host_Activated(object sender, EventArgs e)
    {
        designSurfacePanel.Focus();
    }
}
