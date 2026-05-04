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
using Origam.DA;
using Origam.Gui.UI;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.UI;
using WeifenLuo.WinFormsUI.Docking;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for AsForm.
/// </summary>
public class AsForm
    : DockContent,
        IViewContent,
        IRecordReferenceProvider,
        IOrigamForm,
        IToolStripContainer
{
    #region AttachmentSolution
    //Handle this event for getting right Id as attachment filter
    //(when you get null sender and emty Id clear Attachments table == no panel is selected for handling attchments)
    public event RecordReferencesChangedHandler RecordReferenceChanged;
    private AsPanel _currentHandledPanel;

    public event EventHandler ToolStripsLoaded;
    public event EventHandler AllToolStripsRemoved;
    public event EventHandler ToolStripsNeedUpdate
    {
        add { }
        remove { }
    }

    public List<ToolStrip> GetToolStrips(int maxWidth = -1)
    {
        return Panels.Select(selector: x => x.ToolStrip).ToList();
    }

    /// <summary>
    /// Recursively goes through all child records and finds all the record Id's
    /// </summary>
    /// <param name="row"></param>
    /// <param name="references"></param>
    internal void RetrieveChildReferences(DataRow row, Hashtable references)
    {
        foreach (DataRelation relation in row.Table.DataSet.Relations)
        {
            if (relation.ParentTable == row.Table) // we find all the child relations of our row's table
            {
                if (relation.ChildTable.Columns.Contains(name: Const.ValuelistIdField)) // only if there exists Id column
                {
                    Guid entityId = new Guid(
                        g: relation.ChildTable.ExtendedProperties[key: "EntityId"].ToString()
                    );
                    foreach (DataRow childRow in row.GetChildRows(relation: relation))
                    {
                        Guid recordId = (Guid)childRow[columnName: Const.ValuelistIdField];
                        RecordReference newRef = new RecordReference(
                            entityId: entityId,
                            recordId: recordId
                        );
                        if (!references.Contains(key: newRef.GetHashCode()))
                        {
                            references.Add(key: newRef.GetHashCode(), value: newRef);
                        }
                        RetrieveChildReferences(row: childRow, references: references); // recursion
                    }
                }
            }
        }
    }

    public void panel_RecordIdChanged(object sender, EventArgs e)
    {
        if (this.PanelBindingSuspendedTemporarily)
        {
            return;
        }

        AsPanel panel = sender as AsPanel;

        if (panel.ShowAttachments)
        {
            MainEntityId = panel.EntityId;
            SetMainRecordId(id: panel.RecordId);
            ChildRecordReferences.Clear();
            if (
                panel
                    .BindingContext[dataSource: panel.DataSource, dataMember: panel.DataMember]
                    .Position > -1
            )
            {
                DataRow row = (
                    panel
                        .BindingContext[dataSource: panel.DataSource, dataMember: panel.DataMember]
                        .Current as DataRowView
                ).Row;
                RetrieveChildReferences(row: row, references: ChildRecordReferences);
            }
            else
            {
                MainEntityId = Guid.Empty;
                MainRecordId = Guid.Empty;
                ChildRecordReferences.Clear();
            }
        }
        else
        {
            MainEntityId = Guid.Empty;
            MainRecordId = Guid.Empty;
            ChildRecordReferences.Clear();
        }
        OnRecordReferenceChanged();
    }

    private void OnRecordReferenceChanged()
    {
        RecordReferenceChanged?.Invoke(
            sender: this,
            mainEntityId: this.MainEntityId,
            mainRecordId: this.MainRecordId,
            childReferences: this.ChildRecordReferences
        );
    }

    public void PanelAttachementStateHandler(object sender, EventArgs e)
    {
        if (!(sender is AsPanel))
        {
            return;
        }

        AsPanel panel = sender as AsPanel;
        // panel is setting ShowAttachments to False
        if (
            _currentHandledPanel != null
            && panel.Name == _currentHandledPanel.Name
            && panel.ShowAttachments == false
        )
        {
            _currentHandledPanel.RecordIdChanged -= panel_RecordIdChanged;
            _currentHandledPanel = null;
            panel_RecordIdChanged(sender: sender, e: EventArgs.Empty);
            return;
        }
        // we hide attachments for the currently handled panel
        if (_currentHandledPanel != null)
        {
            _currentHandledPanel.RecordIdChanged -= panel_RecordIdChanged;
            _currentHandledPanel.ShowAttachments = false;
        }
        panel_RecordIdChanged(sender: sender, e: EventArgs.Empty);
        // and we set the currently handled panel to the new panel
        _currentHandledPanel = panel;
        // then we subscribe for changes in the new panel
        _currentHandledPanel.RecordIdChanged += panel_RecordIdChanged;
    }
    #endregion
    public AsForm()
    {
        InitializeComponent();
        SetStyle(flag: ControlStyles.DoubleBuffer, value: true);
        this.BackColor = OrigamColorScheme.FormBackgroundColor;
        this.Paint += AsForm_Paint;
    }

    public AsForm(FormGenerator generator)
        : this()
    {
        FormGenerator = generator;
    }

    public FormGenerator FormGenerator { get; set; }
    public Label NameLabel { get; set; }
    public FlowLayoutPanel ToolStripContainer
    {
        get => toolStripContainer;
        set
        {
            ToolStripsLoaded?.Invoke(sender: null, e: EventArgs.Empty);
            toolStripContainer = value;
        }
    }
    public string NotificationText { get; set; } = "";
    private string _progressText = "";
    public string ProgressText
    {
        get => _progressText;
        set
        {
            _progressText = value;
            this.Invalidate();
            Application.DoEvents();
        }
    }
    public string HelpTopic => "";
    private ComponentBindingCollection _componentBindings = new ComponentBindingCollection();
    bool _bindingsInitialized = false;
    public ComponentBindingCollection ComponentBindings
    {
        get
        {
            if (!_bindingsInitialized && _extraControlBindings != "")
            {
                System.Xml.Serialization.XmlSerializer xsr =
                    new System.Xml.Serialization.XmlSerializer(
                        type: typeof(ComponentBindingCollection)
                    );
                _componentBindings =
                    xsr.Deserialize(
                        textReader: new System.IO.StringReader(s: _extraControlBindings)
                    ) as ComponentBindingCollection;
            }
            _bindingsInitialized = true;
            return _componentBindings;
        }
    }
    private string _extraControlBindings = "";

    [Browsable(browsable: false)]
    public string ExtraControlBindings
    {
        get
        {
            if (_bindingsInitialized)
            {
                System.Text.StringBuilder sb = new System.Text.StringBuilder();
                System.Xml.Serialization.XmlRootAttribute root =
                    new System.Xml.Serialization.XmlRootAttribute(elementName: "ComponentBindings");
                System.Xml.Serialization.XmlSerializer xsr =
                    new System.Xml.Serialization.XmlSerializer(
                        type: typeof(ComponentBindingCollection),
                        root: root
                    );
                xsr.Serialize(
                    textWriter: new System.IO.StringWriter(sb: sb),
                    o: _componentBindings
                );

                return sb.ToString();
            }
            return _extraControlBindings;
        }
    }
    public bool SingleRecordEditing = false;
    public bool PanelBindingSuspendedTemporarily = false;
    #region IViewContent Members
    bool _canRefresh = true;
    private Timer timer;
    private IContainer components;

    public bool CanRefreshContent
    {
        get
        {
            if (IsSaving)
            {
                return false;
            }

            return _canRefresh;
        }
        set => _canRefresh = value;
    }
    private string _autoAddNewEntity;
    public string AutoAddNewEntity
    {
        get => _autoAddNewEntity;
        set
        {
            _autoAddNewEntity = value;
            if (value != null)
            {
                timer.Start();
            }
        }
    }
    public object EnteringGrid { get; set; }

    public void RefreshContent()
    {
        if (SaveData())
        {
            this.FormGenerator.RefreshMainData();
            this.IsDirty = false;
        }
    }

    private bool _isReadOnly = false;
    public bool IsReadOnly
    {
        get
        {
            if (IsSaving)
            {
                return true;
            }

            return _isReadOnly;
        }
        set => _isReadOnly = value;
    }
    public bool SaveOnClose { get; set; } = true;
    public event SaveEventHandler Saved
    {
        add { }
        remove { }
    }
    string _titleName = "";
    public string TitleName
    {
        get => _titleName;
        set
        {
            _titleName = value;
            this.Text = value;
            if (this.NameLabel != null)
            {
                this.NameLabel.Text = value;
            }

            OnTitleNameChanged(e: EventArgs.Empty);
        }
    }
    private string _statusText = "";
    public string StatusText
    {
        get => _statusText;
        set
        {
            _statusText = value;
            OnStatusTextChanged(e: EventArgs.Empty);
        }
    }
    public event EventHandler StatusTextChanged;

    void OnStatusTextChanged(EventArgs e)
    {
        StatusTextChanged?.Invoke(sender: this, e: e);
    }

    public event EventHandler DirtyChanged;
    public event EventHandler TitleNameChanged;

    public void SaveObject()
    {
        this.EndCurrentEdit();
        if (HasErrors(throwException: true))
        {
            return;
        }

        FormGenerator.SaveData();
    }

    private bool HasErrors(bool throwException)
    {
        if (FormGenerator.DataSet.HasErrors)
        {
            if (throwException)
            {
                throw new Exception(
                    message: ResourceUtils.GetString(
                        key: "ErrorsInForm",
                        args: Environment.NewLine
                            + Environment.NewLine
                            + DatasetTools.GetDatasetErrors(dataset: FormGenerator.DataSet)
                    )
                );
            }

            return true;
        }

        return false;
    }

    public Guid DisplayedItemId { get; set; }
    public bool IsUntitled => false;

    /// <summary>
    /// Holds workflow Id, in case, that this form is used for workflow display.
    /// </summary>
    public Guid WorkflowId { get; set; } = Guid.Empty;
    public object LoadedObject { get; private set; }

    public void LoadObject(object objectToLoad)
    {
        LoadedObject = objectToLoad;
        FormControlSet form;
        Guid methodId = Guid.Empty;
        Guid defaultSetId = Guid.Empty;
        Guid sortSetId = Guid.Empty;
        Guid listDataStructureId = Guid.Empty;
        Guid listMethodId = Guid.Empty;
        string listDataMember = null;
        if (objectToLoad is FormControlSet)
        {
            form = objectToLoad as FormControlSet;
        }
        else if (objectToLoad is FormReferenceMenuItem)
        {
            FormReferenceMenuItem formRef = objectToLoad as FormReferenceMenuItem;
            form = formRef.Screen;
            defaultSetId = formRef.DefaultSetId;
            sortSetId = formRef.SortSetId;

            if (SingleRecordEditing)
            {
                methodId = formRef.RecordEditMethodId;
            }
            else
            {
                methodId = formRef.MethodId;
                // Set listDataStructure only if we don't edit single record. If we do, we load everything at once.
                listDataStructureId = formRef.ListDataStructureId;
                listMethodId = formRef.ListMethodId;
                if (formRef.ListEntity != null)
                {
                    listDataMember = formRef.ListEntity.Name;
                }
            }
            this.FormGenerator.TemplateSet = formRef.TemplateSet;
            this.FormGenerator.DefaultTemplate = formRef.DefaultTemplate;
            this.FormGenerator.RuleSet = formRef.RuleSet;

            _isReadOnly = FormTools.IsFormMenuReadOnly(formRef: formRef);
        }
        else
        {
            throw new ArgumentOutOfRangeException(
                paramName: "objectToLoad",
                actualValue: objectToLoad,
                message: ResourceUtils.GetString(key: "UknownObject")
            );
        }
        //this.FormGenerator.LoadForm(this, form, methodId, defaultSetId, listDataStructureId, listMethodId, listDataMember);
        this.FormGenerator.AsyncForm = this;
        this.FormGenerator.AsyncFormControlSet = form;
        this.FormGenerator.AsyncMethodId = methodId;
        this.FormGenerator.AsyncDefaultSetId = defaultSetId;
        this.FormGenerator.AsyncSortSetId = sortSetId;
        this.FormGenerator.AsyncListDataStructureId = listDataStructureId;
        this.FormGenerator.AsyncListMethodId = listMethodId;
        this.FormGenerator.AsyncListDataMember = listDataMember;
        LoadFormAsync();
        ToolStripsLoaded?.Invoke(sender: null, e: EventArgs.Empty);
    }

    private void LoadFormAsync()
    {
        this.FormGenerator.LoadFormAsync();
        this.EndCurrentEdit();
        if (SingleRecordEditing)
        {
            object[] key = new object[FormGenerator.SelectionParameters.Count];
            int i = 0;
            foreach (object val in FormGenerator.SelectionParameters.Values)
            {
                key[i] = val;
                i++;
            }
            SetPosition(key: key);
        }
    }

    public event EventHandler Saving
    {
        add { }
        remove { }
    }
    public string UntitledName
    {
        get
        {
            // TODO:  Add AsForm.UntitledName getter implementation
            return null;
        }
        set
        {
            // TODO:  Add AsForm.UntitledName setter implementation
        }
    }
    private bool _isDirty;
    public virtual bool IsDirty
    {
        get
        {
            if (this.IsViewOnly)
            {
                return false;
            }

            return _isDirty;
        }
        set
        {
            if (!this.IsViewOnly)
            {
                _isDirty = value;
                OnDirtyChanged(e: EventArgs.Empty);
            }
        }
    }
    public bool IsSaving { get; set; }
    public virtual bool IsViewOnly { get; } = false;
    public string AddingDataMember { get; set; } = "";
    public bool IsFiltering = false;
    public bool CreateAsSubViewContent
    {
        get
        {
            // TODO:  Add AsForm.CreateAsSubViewContent getter implementation
            return false;
        }
    }
    #endregion
    #region IBaseViewContent Members
    public void Deselected()
    {
        // TODO:  Add AsForm.Deselected implementation
    }

    public void SwitchedTo()
    {
        // TODO:  Add AsForm.SwitchedTo implementation
    }

    public void Selected()
    {
        // TODO:  Add AsForm.Selected implementation
    }

    public IWorkbenchWindow WorkbenchWindow
    {
        get
        {
            // TODO:  Add AsForm.WorkbenchWindow getter implementation
            return null;
        }
        set
        {
            // TODO:  Add AsForm.WorkbenchWindow setter implementation
        }
    }
    public string TabPageText
    {
        get
        {
            // TODO:  Add AsForm.TabPageText getter implementation
            return null;
        }
    }

    public void RedrawContent()
    {
        // TODO:  Add AsForm.RedrawContent implementation
    }
    #endregion
    private void InitializeComponent()
    {
        this.components = new Container();
        this.timer = new Timer(container: this.components);
        //
        // timer
        //
        this.timer.Interval = 300;
        this.timer.Tick += this.timer_Tick;
        //
        // AsForm
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(width: 5, height: 13);
        this.AutoScroll = true;
        this.BackColor = System.Drawing.Color.FloralWhite;
        this.ClientSize = new System.Drawing.Size(width: 320, height: 285);
        this.DockAreas = DockAreas.Document;
        this.Name = "AsForm";
        this.Closing += this.AsForm_Closing;
    }

    protected virtual void OnDirtyChanged(EventArgs e)
    {
        OnTitleNameChanged(e: EventArgs.Empty);
        DirtyChanged?.Invoke(sender: this, e: e);
    }

    protected virtual void OnTitleNameChanged(EventArgs e)
    {
        this.Text = (IsDirty ? "*" : "") + this.TitleName;
        //this.TabText = this.TitleName;
        TitleNameChanged?.Invoke(sender: this, e: e);
    }

    public string Test()
    {
        Control focused = FindFocused(parent: this);
        if (focused == null)
        {
            return "No focused control found";
        }

        return "Type: "
            + focused.GetType()
            + Environment.NewLine
            + "Name: "
            + focused.Name
            + Environment.NewLine
            + "TabStop: "
            + focused.TabStop
            + Environment.NewLine
            + "TabIndex: "
            + focused.TabIndex;
    }

    public void EndCurrentEdit()
    {
        try
        {
            this.IsFiltering = true;
            if (this.ActiveControl is AsPanel activePanel)
            {
                activePanel.EndEdit();
            }
        }
        finally
        {
            this.IsFiltering = false;
        }
    }

    public AsPanel[] Panels
    {
        get
        {
            var list = new List<AsPanel>();
            AddPanels(parentControl: this, list: list);
            return list.ToArray();
        }
    }

    private void AddPanels(Control parentControl, List<AsPanel> list)
    {
        foreach (Control control in parentControl.Controls)
        {
            if (control is AsPanel panel)
            {
                list.Add(item: panel);
            }

            AddPanels(parentControl: control, list: list);
        }
    }

    public AsPanel FindPanel(string dataMember)
    {
        foreach (AsPanel panel in this.Panels)
        {
            if (panel.DataMember == dataMember)
            {
                return panel;
            }
        }
        return null;
    }

    private Control FindFocused(Control parent)
    {
        if (parent.Focused)
        {
            return parent;
        }

        foreach (Control control in parent.Controls)
        {
            Control focusedControl = FindFocused(parent: control);

            if (focusedControl != null)
            {
                return FindFocused(parent: control);
            }
        }
        return null;
    }

    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (keyData == (Keys.F6 | Keys.Shift))
        {
            this.SelectNextControl(
                ctl: this.ActiveControl,
                forward: false,
                tabStopOnly: true,
                nested: true,
                wrap: true
            );
            return true;
        }
        if (keyData == Keys.F6)
        {
            bool result = this.SelectNextControl(
                ctl: this.ActiveControl,
                forward: true,
                tabStopOnly: true,
                nested: true,
                wrap: true
            );
            return true;
        }
        return base.ProcessCmdKey(msg: ref msg, keyData: keyData);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_currentHandledPanel != null)
            {
                _currentHandledPanel.RecordIdChanged -= panel_RecordIdChanged;
                _currentHandledPanel = null;
            }
            if (this.FormGenerator != null)
            {
                this.FormGenerator.Dispose();
                this.FormGenerator = null;
            }
        }
        base.Dispose(disposing: disposing);
    }

    private bool SaveData()
    {
        if (FormGenerator == null)
        {
            return false;
        }

        try
        {
            this.EndCurrentEdit();
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(
                owner: this,
                text: ex.Message,
                caption: ResourceUtils.GetString(key: "ErrorWhenSaving", args: this.TitleName),
                exception: ex
            );
            return false;
        }
        if (!SaveOnClose)
        {
            try
            {
                HasErrors(throwException: true);
            }
            catch (Exception ex)
            {
                AsMessageBox.ShowError(
                    owner: this,
                    text: ex.Message,
                    caption: ResourceUtils.GetString(key: "ErrorWhenSaving", args: this.TitleName),
                    exception: ex
                );
                return false;
            }

            return true;
        }
        if (IsDirty)
        {
            DialogResult result = MessageBox.Show(
                owner: this,
                text: ResourceUtils.GetString(key: "SaveChanges", args: this.TitleName),
                caption: ResourceUtils.GetString(key: "SaveTitle"),
                buttons: MessageBoxButtons.YesNoCancel,
                icon: MessageBoxIcon.Question
            );

            switch (result)
            {
                case DialogResult.Yes:
                {
                    try
                    {
                        SaveObject();
                    }
                    catch (Exception ex)
                    {
                        AsMessageBox.ShowError(
                            owner: this,
                            text: ex.Message,
                            caption: ResourceUtils.GetString(
                                key: "ErrorWhenSaving",
                                args: this.TitleName
                            ),
                            exception: ex
                        );
                        return false;
                    }
                    return true;
                }

                case DialogResult.No:
                {
                    return true;
                }

                case DialogResult.Cancel:
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void AsForm_Closing(object sender, CancelEventArgs e)
    {
        if (this.DialogResult != DialogResult.Cancel)
        {
            if (!SaveData())
            {
                e.Cancel = true;
            }
        }
        timer.Stop();
    }

    #region IRecordReferenceProvider Members
    public Guid MainEntityId { get; private set; } = Guid.Empty;
    public Guid MainRecordId { get; private set; } = Guid.Empty;

    private void SetMainRecordId(object id)
    {
        if (id is Guid)
        {
            MainRecordId = (Guid)id;
        }
        else
        {
            MainRecordId = Guid.Empty;
        }
    }

    private bool IsManagerBinding(CurrencyManager cm)
    {
        if (cm == null)
        {
            return false;
        }

        return (bool)
            Reflector.GetValue(
                type: typeof(CurrencyManager),
                instance: cm,
                memberName: "IsBinding"
            );
    }

    private void timer_Tick(object sender, EventArgs e)
    {
        if (this.AutoAddNewEntity != null)
        {
            BindingManagerBase b = this.Controls[index: 0].BindingContext[
                dataSource: this.FormGenerator.DataSet,
                dataMember: this.AutoAddNewEntity
            ];
            if (b.Count == 0)
            {
                this.AddingDataMember = this.AutoAddNewEntity;
                try
                {
                    b.AddNew();
                }
                finally
                {
                    this.AddingDataMember = "";
                }
            }
        }
    }

    public Hashtable ChildRecordReferences { get; } = new Hashtable();
    private List<Control> _disabledControls = null;
    private FlowLayoutPanel toolStripContainer;

    public void BeginDisable()
    {
        _disabledControls = new List<Control>();
        foreach (Control child in this.Controls)
        {
            if (child.Enabled)
            {
                _disabledControls.Add(item: child);
                child.Enabled = false;
            }
        }
    }

    public void EndDisable()
    {
        if (_disabledControls == null)
        {
            throw new InvalidOperationException(
                message: ResourceUtils.GetString(key: "ErrorEndDisable")
            );
        }

        foreach (Control control in _disabledControls)
        {
            control.Enabled = true;
        }
        _disabledControls = null;
    }
    #endregion
    #region IOrigamForm Members
    public bool SetPosition(object[] key)
    {
        // take the first root panel and try to set the position
        foreach (AsPanel panel in this.Panels)
        {
            if (panel.DataMember.IndexOf(value: ".") == -1)
            {
                panel.SetPosition(primaryKey: key);
                panel.FocusGridFirstColumn();
                return true;
            }
        }
        return false;
    }

    public Key PrimaryKey => this.FormGenerator.FormKey;
    #endregion
    private void AsForm_Paint(object sender, PaintEventArgs e)
    {
        try
        {
            System.Drawing.Graphics g = e.Graphics;
            string text = this.ProgressText;
            System.Drawing.Font font = new System.Drawing.Font(
                family: this.Font.FontFamily,
                emSize: 10,
                style: System.Drawing.FontStyle.Bold
            );
            float stringWidth = g.MeasureString(text: text, font: font).Width;
            float stringHeight = g.MeasureString(text: text, font: font).Height;
            g.Clear(color: this.BackColor);
            g.DrawString(
                s: text,
                font: font,
                brush: new System.Drawing.SolidBrush(
                    color: OrigamColorScheme.FormLoadingStatusColor
                ),
                x: (this.Width / 2) - (stringWidth / 2),
                y: (this.Height / 3) + 64
            );
        }
        catch { }
    }

    protected void InvokeToolStripsRemoved()
    {
        AllToolStripsRemoved(sender: this, e: EventArgs.Empty);
    }
}
