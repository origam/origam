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
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Origam.DA;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Gui.UI;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.GuiModel;
using Origam.Service.Core;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Gui.Win;

/// <summary>
/// Summary description for FormGenerator.
/// </summary>
///
public class FormGenerator : IDisposable
{
    private enum ProcessPropertyValueOperation
    {
        Save,
        Load,
    }

    private RuleEngine _formRuleEngine = RuleEngine.Create(
        contextStores: new Hashtable(),
        transactionId: null
    );
    private BindingContext _bindContext = new BindingContext();
    private Hashtable _propertyCache = new Hashtable();
    private Hashtable _bindings = new Hashtable();
    private Hashtable _dataConsumers = new Hashtable();
    private Guid _listDataStructureId = Guid.Empty;
    private Guid _listDataStructureMethodId = Guid.Empty;
    private string _listDataMember;
    private Key _formKey;
    private Guid _mainDataStructureId;
    private Guid _mainDataStructureMethodId;
    private Guid _mainDataStructureDefaultSetId;
    private Guid _mainDataStructureSortSetId;
    private FlowLayoutPanel _toolStripContainer;
    private DataStructureSortSet _mainDataStructureSortSet;
    private IDataLookupService _lookupManager;
    private IServiceAgent _dataServiceAgent;
    private IDocumentationService _documentationService;
    private ToolTip _toolTip;
    private ToolBar _selectionDialogToolbar;
    private Button _selectionDialogOKButton;
    private Button _selectionDialogCancelButton;
    private IEndRule _selectionDialogEndRule;
    private Hashtable _loadedPieces = new Hashtable();
    private DatasetRuleHandler _ruleHandler = new DatasetRuleHandler();
    private readonly IControlsLookUpService _controlsLookupService;

    #region Constructors
    public FormGenerator()
    {
        _lookupManager = ServiceManager.Services.GetService<IDataLookupService>();
        _controlsLookupService = ServiceManager.Services.GetService<IControlsLookUpService>();
        _documentationService = ServiceManager.Services.GetService<IDocumentationService>();
        _dataServiceAgent = ServiceManager
            .Services.GetService<IBusinessServicesService>()
            .GetAgent(serviceType: "DataService", ruleEngine: null, workflowEngine: null);
    }
    #endregion
    #region Properties
    public RuleEngine FormRuleEngine
    {
        get { return _formRuleEngine; }
    }
    DataSet _mainFormData = null;
    public DataSet DataSet
    {
        get { return _mainFormData; }
        set { _mainFormData = value; }
    }
    public IDataDocument XmlData { get; set; } = null;
    public Hashtable ControlBindings
    {
        get { return _bindings; }
    }
    public Hashtable DataConsumers
    {
        get { return _dataConsumers; }
    }
    public BindingContext BindingContext
    {
        get { return _bindContext; }
    }
    public Key FormKey
    {
        get { return _formKey; }
    }
    private bool _ignoreDataChanges = false;
    public bool IgnoreDataChanges
    {
        get { return _ignoreDataChanges; }
        set { _ignoreDataChanges = value; }
    }
    private DataStructureTemplateSet _templateSet;
    public DataStructureTemplateSet TemplateSet
    {
        get { return _templateSet; }
        set { _templateSet = value; }
    }
    private DataStructureTemplate _defaultTemplate;
    public DataStructureTemplate DefaultTemplate
    {
        get { return _defaultTemplate; }
        set { _defaultTemplate = value; }
    }
    private DataStructureRuleSet _ruleSet;
    public DataStructureRuleSet RuleSet
    {
        get { return _ruleSet; }
        set { _ruleSet = value; }
    }
    private AsForm _form = null;
    public AsForm Form
    {
        get { return _form; }
        set { _form = value; }
    }
    public Guid MainFormDataStructureId
    {
        get { return _mainDataStructureId; }
        set { _mainDataStructureId = value; }
    }
    public Guid MainFormMethodId
    {
        get { return _mainDataStructureMethodId; }
        set { _mainDataStructureMethodId = value; }
    }
    public Guid MainFormSortSetId
    {
        get { return _mainDataStructureSortSetId; }
        set { _mainDataStructureSortSetId = value; }
    }
    Hashtable _selectionParameters = new Hashtable();
    public Hashtable SelectionParameters
    {
        get { return _selectionParameters; }
    }
    #endregion
    #region Public Static Methods

    public static void SavePropertyValue(
        Control control,
        PropertyInfo property,
        PropertyValueItem propertyValueItem
    )
    {
        ProcessPropertyValue(
            control: control,
            property: property,
            propertyValueItem: propertyValueItem,
            action: ProcessPropertyValueOperation.Save
        );
    }

    public static void LoadPropertyValue(
        Control control,
        PropertyInfo property,
        PropertyValueItem propertyValueItem
    )
    {
        ProcessPropertyValue(
            control: control,
            property: property,
            propertyValueItem: propertyValueItem,
            action: ProcessPropertyValueOperation.Load
        );
    }

    public static string DataMemberFromTable(DataTable table)
    {
        string result = table.TableName;
        while (table.ParentRelations.Count > 0)
        {
            table = table.ParentRelations[index: 0].ParentTable;
            result = (table.TableName + ".") + result;
        }
        return result;
    }

    public static string FindTableByDisplayMember(DataSet ds, string member)
    {
        string tableName = "";
        if (member.IndexOf(value: ".") > 0)
        {
            string[] path = member.Split(separator: ".".ToCharArray());
            DataTable table = ds.Tables[name: path[0]];
            for (int i = 1; i < path.Length - 1; i++)
            {
                table = table.ChildRelations[name: path[i]].ChildTable;
            }
            tableName = table.TableName;
        }
        else
        {
            tableName = member;
        }

        return tableName;
    }

    public static string GetColumnNameFromDisplayMember(string member)
    {
        string columnName = "";
        if (member.IndexOf(value: ".") > 0)
        {
            string[] path = member.Split(separator: ".".ToCharArray());
            columnName = path[path.Length - 1];
        }
        else
        {
            columnName = member;
        }
        return columnName;
    }

    public static bool DisplayRuleException(IWin32Window window, RuleException ruleEx)
    {
        bool shouldReturn = false;
        if (ruleEx.IsSeverityHigh)
        {
            MessageBox.Show(
                owner: window,
                text: ruleEx.Message,
                caption: RuleEngine.ValidationNotMetMessage(),
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Stop
            );
            shouldReturn = true;
        }
        else
        {
            if (
                MessageBox.Show(
                    owner: window,
                    text: RuleEngine.ValidationContinueMessage(
                        message: ruleEx.Message + Environment.NewLine + Environment.NewLine
                    ),
                    caption: RuleEngine.ValidationWarningMessage(),
                    buttons: MessageBoxButtons.YesNo,
                    icon: MessageBoxIcon.Question
                ) == DialogResult.No
            )
            {
                shouldReturn = true;
            }
        }
        return shouldReturn;
    }
    #endregion
    #region Private Static Methods
    private static void ProcessPropertyValue(
        Control control,
        PropertyInfo property,
        PropertyValueItem propertyValueItem,
        ProcessPropertyValueOperation action
    )
    {
        object valueToSet = null;
        //property which are used only for binding (not for saving values from designer) we ignore
        if (propertyValueItem.ControlPropertyItem.IsBindOnly)
        {
            return;
        }

        try
        {
            if (action == ProcessPropertyValueOperation.Save)
            {
                valueToSet = (property.GetValue(obj: control, index: new object[0]));
            }
            switch (propertyValueItem.ControlPropertyItem.PropertyType)
            {
                case ControlPropertyValueType.Boolean:
                {
                    if (action == ProcessPropertyValueOperation.Load)
                    {
                        Reflector.SetValue(
                            memberInfo: property,
                            instance: control,
                            value: propertyValueItem.BoolValue
                        );
                    }
                    //property.SetValue(control,propertyValueItem.BoolValue, new object[0]);
                    else if (valueToSet != null)
                    {
                        propertyValueItem.BoolValue = (bool)valueToSet;
                    }

                    break;
                }

                case ControlPropertyValueType.Integer:
                {
                    if (action == ProcessPropertyValueOperation.Load)
                    {
                        if (!property.PropertyType.IsEnum)
                        {
                            Reflector.SetValue(
                                memberInfo: property,
                                instance: control,
                                value: propertyValueItem.IntValue
                            );
                        }
                        else
                        {
                            property.SetValue(
                                obj: control,
                                value: Enum.ToObject(
                                    enumType: property.PropertyType,
                                    value: Convert.ToInt32(value: propertyValueItem.IntValue)
                                ),
                                index: new object[0]
                            );
                        }
                    }
                    else
                    {
                        propertyValueItem.IntValue = (int)valueToSet;
                    }

                    break;
                }

                case ControlPropertyValueType.String:
                {
                    if (action == ProcessPropertyValueOperation.Load)
                    {
                        Reflector.SetValue(
                            memberInfo: property,
                            instance: control,
                            value: propertyValueItem.Value
                        );
                    }
                    //property.SetValue(control,propertyValueItem.Value, new object[0]);
                    else
                    {
                        propertyValueItem.Value = (string)valueToSet;
                    }

                    break;
                }

                case ControlPropertyValueType.UniqueIdentifier:
                {
                    if (action == ProcessPropertyValueOperation.Load)
                    {
                        Reflector.SetValue(
                            memberInfo: property,
                            instance: control,
                            value: propertyValueItem.GuidValue
                        );
                    }
                    //property.SetValue(control,propertyValueItem.GuidValue, new object[0]);
                    else
                    {
                        propertyValueItem.GuidValue = (Guid)valueToSet;
                    }

                    break;
                }

                case ControlPropertyValueType.Xml:
                {
                    if (action == ProcessPropertyValueOperation.Load)
                    {
                        valueToSet = FormGenerator.DeserializeValue(
                            value: propertyValueItem.Value,
                            type: property.PropertyType
                        );
                        Reflector.SetValue(
                            memberInfo: property,
                            instance: control,
                            value: valueToSet
                        );
                    }
                    else
                    {
                        propertyValueItem.Value = FormGenerator.SerializeValue(
                            itemPropertyValue: valueToSet,
                            type: property.PropertyType
                        );
                    }
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(
                owner: null,
                text: ex.Message,
                caption: ex.Source,
                exception: ex
            );
        }
    }

    private static string SerializeValue(object itemPropertyValue, Type type)
    {
        System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(
            type: type
        );
        System.IO.StringWriter writer = new System.IO.StringWriter();
        ser.Serialize(textWriter: writer, o: itemPropertyValue);
        return writer.ToString();
    }

    private static object DeserializeValue(string value, Type type)
    {
        if (string.IsNullOrEmpty(value: value))
        {
            return null;
        }

        System.Xml.Serialization.XmlSerializer ser = new System.Xml.Serialization.XmlSerializer(
            type: type
        );
        System.IO.StringReader reader = new System.IO.StringReader(s: value);
        return ser.Deserialize(textReader: reader);
    }
    #endregion
    #region Public Methods
    private IDictionary<DataGridColumnStyle, string> _gridTooltips =
        new Dictionary<DataGridColumnStyle, string>();
    private IList<Control> _tooltipControls = new List<Control>();

    public void SetTooltip(DataGridColumnStyle style, string tipText)
    {
        _gridTooltips[key: style] = tipText;
    }

    public string GetTooltip(DataGridColumnStyle style)
    {
        return _gridTooltips[key: style];
    }

    public void SetTooltip(Control component, string text)
    {
        if (_toolTip == null)
        {
            return;
        }
        if (text == null)
        {
            _toolTip.SetToolTip(control: component, caption: null);
        }
        else
        {
            _tooltipControls.Add(item: component);
            if (text == "")
            {
                text = ResourceUtils.GetString(key: "NoHelpAvailable");
            }

            _toolTip.SetToolTip(control: component, caption: text);
        }
        foreach (Control child in component.Controls)
        {
            SetTooltip(component: child, text: text);
        }
    }

    void _toolTip_Popup(object sender, PopupEventArgs e)
    {
        System.Diagnostics.Debug.WriteLine(value: e.AssociatedControl, category: "Tooltip");
        IAsCaptionControl captionControl = FindCaptionControl(control: e.AssociatedControl);
        if (captionControl == null)
        {
            _toolTip.ToolTipIcon = ToolTipIcon.None;
            _toolTip.ToolTipTitle = null;
        }
        else
        {
            _toolTip.ToolTipIcon = ToolTipIcon.Info;
            _toolTip.ToolTipTitle = captionControl.Caption;
        }
    }

    private static IAsCaptionControl FindCaptionControl(Control control)
    {
        while (control != null)
        {
            IAsCaptionControl captionControl = control as IAsCaptionControl;
            if (captionControl != null)
            {
                return captionControl;
            }
            control = control.Parent;
        }
        return null;
    }

    public void SaveData()
    {
        if (_mainFormData == null)
        {
            return;
        }
        try
        {
            Unbind();
            DataStructureQuery query = new DataStructureQuery(
                dataStructureId: _mainDataStructureId,
                methodId: _mainDataStructureMethodId,
                defaultSetId: _mainDataStructureDefaultSetId,
                sortSetId: _mainDataStructureSortSetId
            );

            query.LoadActualValuesAfterUpdate = true;
            _dataServiceAgent.MethodName = "StoreDataByQuery";
            _dataServiceAgent.Parameters.Clear();
            _dataServiceAgent.Parameters.Add(key: "Query", value: query);
            _dataServiceAgent.Parameters.Add(key: "Data", value: _mainFormData);
            string transactionId = Guid.NewGuid().ToString();
            _dataServiceAgent.TransactionId = transactionId;

            try
            {
                _dataServiceAgent.Run();
            }
            catch
            {
                ResourceMonitor.Rollback(transactionId: transactionId);
                _dataServiceAgent.TransactionId = null;
                throw;
            }
            ResourceMonitor.Commit(transactionId: transactionId);
            _dataServiceAgent.TransactionId = null;
        }
        finally
        {
            // reset temporary columns
            ResetTempColumns();
            Bind();
        }
    }

    public void RefreshMainData()
    {
        ShowProgress(text: null, position: ProgressPosition.TopRight, parent: this.Form.NameLabel);
        try
        {
            Guid dsId = (
                _listDataStructureId == Guid.Empty ? _mainDataStructureId : _listDataStructureId
            );
            Guid filterId = (
                _listDataStructureId == Guid.Empty
                    ? _mainDataStructureMethodId
                    : _listDataStructureMethodId
            );
            Guid defaultId = (
                _listDataStructureId == Guid.Empty ? _mainDataStructureDefaultSetId : Guid.Empty
            );
            Guid sortSetId = (
                _listDataStructureId == Guid.Empty ? _mainDataStructureSortSetId : Guid.Empty
            );
            DataStructureQuery query = new DataStructureQuery(
                dataStructureId: dsId,
                methodId: filterId,
                defaultSetId: defaultId,
                sortSetId: sortSetId
            );
            _dataServiceAgent.MethodName = "LoadDataByQuery";
            _dataServiceAgent.Parameters.Clear();
            _dataServiceAgent.Parameters.Add(key: "Query", value: query);
            AddQueryParameters(query: query);
            RefreshData();
        }
        finally
        {
            HideProgress();
        }
    }

    private void RefreshData()
    {
        Unbind();
        try
        {
            _dataServiceAgent.Run();
            _loadedPieces.Clear();
            if (XmlData == null)
            {
                _mainFormData.Clear();
            }
            else
            {
                DatasetTools.Clear(data: _mainFormData);
            }
            // reset temporary columns
            ResetTempColumns();
            DataSet result = _dataServiceAgent.Result as DataSet;
            _mainFormData.Merge(
                dataSet: result,
                preserveChanges: false,
                missingSchemaAction: MissingSchemaAction.Ignore
            );
        }
        finally
        {
            // on lazy loaded form the previously loaded records keep being dirty
            // for some reason
            _mainFormData.AcceptChanges();
            Bind();
        }
    }

    private void Bind()
    {
        BindControls(bindings: _bindings);
        SetDataSourceToConsumers(dataConsumers: _dataConsumers);
        SubscribeDataTableEvents();
    }

    private void Unbind()
    {
        UnSubscribeDataTableEvents();
        RemoveDataBindings(bindings: _bindings);
        RemoveDataSourcesFromConsumers(dataConsumers: _dataConsumers);
    }

    private void ResetTempColumns()
    {
        // reset temporary columns
        foreach (DataTable table in _mainFormData.Tables)
        {
            foreach (DataColumn col in table.Columns)
            {
                if (col.ExtendedProperties.Contains(key: Const.TemporaryColumnInitializedAttribute))
                {
                    col.ExtendedProperties.Remove(key: Const.TemporaryColumnInitializedAttribute);
                }
            }
        }
    }

    public Control LoadFormWithData(
        AsForm form,
        DataSet formData,
        IDataDocument xmlData,
        FormControlSet formControlSet
    )
    {
        this.Form = form;
        Control result = LoadFormWithData(
            formData: formData,
            xmlData: xmlData,
            formControlSet: formControlSet,
            methodId: Guid.Empty,
            sortSetId: Guid.Empty,
            defaultSetId: Guid.Empty,
            listDataStructureId: Guid.Empty,
            listMethodId: Guid.Empty,
            listDataMember: null
        );
        return result;
    }

    public AsForm AsyncForm;
    public FormControlSet AsyncFormControlSet;
    public Guid AsyncMethodId;
    public Guid AsyncSortSetId;
    public Guid AsyncDefaultSetId;
    public Guid AsyncListDataStructureId;
    public Guid AsyncListMethodId;
    public string AsyncListDataMember;

    public void LoadFormAsync()
    {
        this.LoadForm(
            form: AsyncForm,
            formControlSet: AsyncFormControlSet,
            methodId: AsyncMethodId,
            sortSetId: AsyncSortSetId,
            defaultSetId: AsyncDefaultSetId,
            listDataStructureId: AsyncListDataStructureId,
            listMethodId: AsyncListMethodId,
            listDataMember: AsyncListDataMember
        );
    }

    public void LoadForm(
        AsForm form,
        FormControlSet formControlSet,
        Guid methodId,
        Guid sortSetId,
        Guid defaultSetId,
        Guid listDataStructureId,
        Guid listMethodId,
        string listDataMember
    )
    {
        this.Form = form;
        LoadFormWithData(
            formControlSet: formControlSet,
            methodId: methodId,
            sortSetId: sortSetId,
            defaultSetId: defaultSetId,
            listDataStructureId: listDataStructureId,
            listMethodId: listMethodId,
            listDataMember: listDataMember
        );
    }

    public Control LoadSelectionDialog(
        IDataDocument xmlData,
        PanelControlSet panelDefinition,
        IEndRule endRule
    )
    {
        AsForm sd = new AsForm(generator: this);
        sd.KeyPreview = true;
        sd.ShowInTaskbar = false;
        sd.ControlBox = false;
        sd.MinimizeBox = false;
        sd.MaximizeBox = false;
        sd.SizeGripStyle = SizeGripStyle.Show;
        sd.StartPosition = FormStartPosition.CenterParent;
        sd.TitleName = ResourceUtils.GetString(key: "SelectionTitle");
        this.Form = sd;
        ControlSetItem rootItem = FormTools.GetItemFromControlSet(controlSet: panelDefinition);
        _mainFormData = xmlData.DataSet;
        XmlData = xmlData;
        _selectionDialogEndRule = endRule;
        string dataMember = _mainFormData.Tables[index: 0].TableName;
        AsPanel panel =
            this.LoadControl(
                cntrlSet: rootItem,
                dataMember: dataMember,
                bindings: _bindings,
                dataConsumers: _dataConsumers,
                parentControl: null,
                ignoreTabPages: true,
                readOnly: false
            ) as AsPanel;
        panel.DataMember = dataMember;
        panel.ShowNewButton = false;
        panel.ShowDeleteButton = false;
        panel.BindingContext = _bindContext;
        panel.TabIndex = 0;
        panel.Dock = DockStyle.Fill;
        panel.HideNavigationPanel = true;
        panel.IgnoreEscape = true;
        sd.Size = new Size(width: panel.Size.Width + 20, height: panel.Size.Height + 70);
        _dataConsumers.Add(key: panel, value: _mainFormData);
        _selectionDialogToolbar = new ToolBar();
        _selectionDialogToolbar.Dock = DockStyle.Top;
        _selectionDialogToolbar.TabIndex = 1;
        _selectionDialogToolbar.TabStop = false;
        _selectionDialogToolbar.Appearance = ToolBarAppearance.Flat;
        _selectionDialogToolbar.TextAlign = ToolBarTextAlign.Right;
        WorkbenchSchemaService schema =
            ServiceManager.Services.GetService(serviceType: typeof(WorkbenchSchemaService))
            as WorkbenchSchemaService;
        _selectionDialogToolbar.ImageList = schema.SchemaBrowser.ImageList;

        ToolBarButton okButton = new ToolBarButton(text: ResourceUtils.GetString(key: "OK"));
        okButton.Tag = "OK";
        okButton.ImageIndex = 71;

        ToolBarButton cancelButton = new ToolBarButton(
            text: ResourceUtils.GetString(key: "Cancel")
        );
        cancelButton.Tag = "CANCEL";
        cancelButton.ImageIndex = 72;
        _selectionDialogToolbar.Buttons.Add(button: okButton);
        _selectionDialogToolbar.Buttons.Add(button: cancelButton);
        _selectionDialogToolbar.ButtonClick += new ToolBarButtonClickEventHandler(
            selectionDialogToolbar_Click
        );
        _selectionDialogOKButton = new Button();
        _selectionDialogOKButton.Click += new EventHandler(selectionDialogOKButton_Click);
        _selectionDialogOKButton.TabStop = false;
        _selectionDialogOKButton.Left = -1000;
        _selectionDialogCancelButton = new Button();
        _selectionDialogCancelButton.Click += new EventHandler(selectionDialogCancelButton_Click);
        _selectionDialogCancelButton.TabStop = false;
        _selectionDialogCancelButton.Left = -1000;
        sd.AcceptButton = _selectionDialogOKButton;
        sd.CancelButton = _selectionDialogCancelButton;
        sd.Controls.Add(value: panel);
        sd.Controls.Add(value: _selectionDialogToolbar);
        sd.Controls.Add(value: _selectionDialogOKButton);
        sd.Controls.Add(value: _selectionDialogCancelButton);
        EndInitialization(control: panel);
        Application.DoEvents();
        RemoveNullConstraints();
        Bind();
        return sd;
    }

    public Control LoadFormWithData(
        DataSet formData,
        IDataDocument xmlData,
        FormControlSet formControlSet,
        Guid methodId,
        Guid sortSetId,
        Guid defaultSetId,
        Guid listDataStructureId,
        Guid listMethodId,
        string listDataMember
    )
    {
        if (this.Form != null)
        {
            this.Form.SuspendLayout();
        }
        Control control;
        try
        {
            if (formControlSet == null || formData == null)
            {
                return null;
            }
            ControlSetItem rootItem = FormTools.GetItemFromControlSet(controlSet: formControlSet);
            if (rootItem == null)
            {
                return null;
            }
            if (formControlSet.DataStructure == null)
            {
                throw new NullReferenceException(
                    message: ResourceUtils.GetString(key: "ErrorFormControlSetDefinition")
                );
            }
            _mainFormData = formData;
            XmlData = xmlData;
            _formKey = formControlSet.PrimaryKey;
            _mainDataStructureId = formControlSet.DataStructure.Id;
            _mainDataStructureMethodId = methodId;
            _mainDataStructureDefaultSetId = defaultSetId;
            _mainDataStructureSortSetId = sortSetId;
            _listDataStructureId = listDataStructureId;
            _listDataStructureMethodId = listMethodId;
            _listDataMember = listDataMember;
            IPersistenceService persistence =
                ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
                as IPersistenceService;
            _mainDataStructureSortSet =
                persistence.SchemaProvider.RetrieveInstance(
                    type: typeof(DataStructureSortSet),
                    primaryKey: new ModelElementKey(id: _mainDataStructureSortSetId)
                ) as DataStructureSortSet;
            CreateToolStripContainer();
            //this row actually loads the form
            control = this.LoadControl(cntrlSet: rootItem);
            if (this.Form.NotificationText != "")
            {
                CreateNotificationBox(control: control);
            }
            CreateFormLabel(control: control);
            AddToolStripContainer(control: control);
            if (control is IViewContent)
            {
                (control as IViewContent).IsDirty = false;
            }
            RemoveNullConstraints();
            SubscribeDataTableEvents();
            foreach (Control child in control.Controls)
            {
                if (child.Dock == DockStyle.Fill)
                {
                    child.Top = 30;
                    child.Left = 0;
                    child.Width = control.Width;
                    child.Height = control.Height;
                }
                if (child is IAsCaptionControl && (child as IAsCaptionControl).HideOnForm)
                {
                    child.Visible = false;
                }
                else
                {
                    child.Visible = true;
                }
            }
        }
        finally
        {
            if (this.Form != null)
            {
                this.Form.ResumeLayout(performLayout: true);
            }
        }
        return control;
    }

    private void AddToolStripContainer(Control control)
    {
        control.Controls.Add(value: _toolStripContainer);
        (control as AsForm).ToolStripContainer = _toolStripContainer;
    }

    private void CreateToolStripContainer()
    {
        _toolStripContainer = new FlowLayoutPanel();
        _toolStripContainer.AutoSize = true;
        _toolStripContainer.AutoSizeMode = AutoSizeMode.GrowAndShrink;
        _toolStripContainer.Dock = DockStyle.Top;
        _toolStripContainer.FlowDirection = FlowDirection.LeftToRight;
        _toolStripContainer.WrapContents = true;
    }

    private void CreateFormLabel(Control control)
    {
        Label nameLabel = new Label();
        nameLabel.Left = 0;
        nameLabel.Top = 0;
        nameLabel.Width = control.Width;
        nameLabel.UseMnemonic = false;
        nameLabel.Font = new Font(
            family: nameLabel.Font.FontFamily,
            emSize: 14,
            style: FontStyle.Bold
        );
        nameLabel.BackColor = control.BackColor;
        nameLabel.Height = 30;
        nameLabel.Text = this.Form.TitleName;
        nameLabel.TextAlign = ContentAlignment.MiddleLeft;
        nameLabel.Dock = DockStyle.Top;
        control.Controls.Add(value: nameLabel);
        (control as AsForm).NameLabel = nameLabel;
    }

    private void CreateNotificationBox(Control control)
    {
        NotificationList notificationBox = new NotificationList();
        notificationBox.Left = 0;
        notificationBox.Top = 0;
        notificationBox.Width = control.Width;
        notificationBox.BackColor = control.BackColor;
        notificationBox.TabStop = false;
        notificationBox.Dock = DockStyle.Top;
        control.Controls.Add(value: notificationBox);
        notificationBox.SetList(text: this.Form.NotificationText);
        int newLinesCount = this.Form.NotificationText.Split(separator: "\n".ToCharArray()).Length;
        if (newLinesCount > 0)
        {
            if (newLinesCount < 6)
            {
                notificationBox.Height = 19 * newLinesCount;
            }
            else
            {
                notificationBox.Height = 19 * 5;
            }
        }
    }

    private enum ProgressPosition
    {
        TopRight,
        Center,
    }

    MRG.Controls.UI.LoadingCircle circ = new MRG.Controls.UI.LoadingCircle();

    private void ShowProgress(string text, ProgressPosition position, Control parent)
    {
        if (parent == null)
        {
            return;
        }

        int top = 0;
        int left = 0;
        switch (position)
        {
            case ProgressPosition.Center:
            {
                top = this.Form.Height / 3;
                left = (this.Form.Width / 2) - (16);
                break;
            }

            case ProgressPosition.TopRight:
            {
                top = 2;
                left = this.Form.Width - 42;
                break;
            }
        }
        if (text != null)
        {
            this.Form.ProgressText = text;
        }
        circ.Top = top;
        circ.Left = left;
        circ.Height = 32;
        circ.Width = 32;
        circ.RotationSpeed = 100;
        circ.ParentControl = parent;
        circ.Color = OrigamColorScheme.FormLoadingStatusColor;
        circ.StylePreset = MRG.Controls.UI.LoadingCircle.StylePresets.MacOSX;
        //			this.Form.Controls.Add(circ);
        circ.Active = true;
    }

    private void HideProgress()
    {
        if (!circ.Active)
        {
            return;
        }

        if (this.Form.ProgressText != "")
        {
            this.Form.ProgressText = "";
        }
        circ.Active = false;
        circ.ParentControl.Invalidate();
        circ.ParentControl = null;
    }

    public Control LoadFormWithData(
        FormControlSet formControlSet,
        Guid methodId,
        Guid sortSetId,
        Guid defaultSetId,
        Guid listDataStructureId,
        Guid listMethodId,
        string listDataMember
    )
    {
        ShowProgress(
            text: ResourceUtils.GetString(key: "LoadingData"),
            position: ProgressPosition.Center,
            parent: this.Form
        );
        _mainDataStructureId = formControlSet.DataSourceId;
        _mainDataStructureMethodId = methodId;
        _mainDataStructureDefaultSetId = defaultSetId;
        _mainDataStructureSortSetId = sortSetId;
        _listDataStructureId = listDataStructureId;
        _listDataStructureMethodId = listMethodId;
        _listDataMember = listDataMember;
        try
        {
            LoadMainData();
        }
        finally
        {
            HideProgress();
        }
        Control result = LoadFormWithData(
            formData: _mainFormData,
            xmlData: XmlData,
            formControlSet: formControlSet,
            methodId: methodId,
            sortSetId: sortSetId,
            defaultSetId: defaultSetId,
            listDataStructureId: listDataStructureId,
            listMethodId: listMethodId,
            listDataMember: listDataMember
        );
        return result;
    }

    public Control LoadControl(ControlSetItem cntrlSet)
    {
        UnloadForm(showCurtain: false);
        Control control;
        try
        {
            ShowProgress(
                text: ResourceUtils.GetString(key: "LoadingForm"),
                position: ProgressPosition.Center,
                parent: this.Form
            );
            bool isReadOnly = false;
            if (this.Form != null)
            {
                isReadOnly = this.Form.IsReadOnly;
            }

            control = this.LoadControl(
                cntrlSet: cntrlSet,
                dataMember: null,
                bindings: _bindings,
                dataConsumers: _dataConsumers,
                parentControl: null,
                ignoreTabPages: true,
                readOnly: isReadOnly
            );
            // finish initialization of controls
            EndInitialization(control: control);
            //Application.DoEvents();
            BindControls(bindings: _bindings);
            SetDataSourceToConsumers(dataConsumers: _dataConsumers);
        }
        finally
        {
            HideProgress();
        }
        return control;
    }

    private bool _unloadingForm = false;

    public void UnloadForm(bool showCurtain)
    {
        //CurtainForm curtain = null;

        if (showCurtain)
        {
            //curtain = new CurtainForm();
            //curtain.Show(this.Form);
        }
        _unloadingForm = true;
        RemoveDataBindings(bindings: _bindings);
        RemoveDataSourcesFromConsumers(dataConsumers: _dataConsumers);
        if (this.Form != null)
        {
            ClearControls(control: this.Form);
            this.Form.Controls.Clear();
            this.Form.Invalidate();
        }
        _bindings.Clear();
        _dataConsumers.Clear();
        _unloadingForm = false;
        if (showCurtain)
        {
            //curtain.Fade();
        }
    }

    public DataSet NewRecord(IXmlContainer dataSource)
    {
        if (DefaultTemplate == null)
        {
            return null;
        }

        return TemplateTools.NewRecord(
            template: DefaultTemplate,
            dataSource: dataSource,
            dataStructureId: _mainDataStructureId
        );
    }

    public object[] AddTemplateRecord(
        DataRow parentRow,
        string dataMember,
        Guid dataStructureId,
        DataSet formData
    )
    {
        if (this.DefaultTemplate == null)
        {
            return null;
        }

        return TemplateTools.AddTemplateRecord(
            parentRow: parentRow,
            template: this.DefaultTemplate,
            dataMember: dataMember,
            dataStructureId: dataStructureId,
            formData: formData
        );
    }
    #endregion
    #region Private Methods
    public static ToolTip InitializeTooltip()
    {
        ToolTip tooltip = new ToolTip();
        tooltip.UseAnimation = false;
        tooltip.AutoPopDelay = 10000;
        tooltip.InitialDelay = 500;
        tooltip.ReshowDelay = 500;
        tooltip.IsBalloon = false;
        tooltip.ShowAlways = true;
        tooltip.ToolTipIcon = ToolTipIcon.Info;
        return tooltip;
    }

    private string GetDataMember(DataTable table)
    {
        DataTable parentTable = table;
        string result = "";
        while (parentTable != null)
        {
            if (result != "")
            {
                result = "." + result;
            }

            result = parentTable.TableName + result;
            if (parentTable.ParentRelations.Count > 0)
            {
                parentTable = parentTable.ParentRelations[index: 0].ParentTable;
            }
            else
            {
                parentTable = null;
            }
        }
        return result;
    }

    private void EndInitialization(Control control)
    {
        foreach (Control child in control.Controls)
        {
            EndInitialization(control: child);
        }
        if (control is ISupportInitialize)
        {
            (control as ISupportInitialize).EndInit();
        }
    }

    private void ClearControls(Control control)
    {
        if (
            control is BaseDropDownControl
            || control.GetType().FullName == "CrystalDecisions.Windows.Forms.CrystalReportViewer"
        )
        {
            return;
        }

        var controls = control.Controls.Cast<Control>().ToList();
        if (control == this.Form)
        {
            foreach (Control component in _tooltipControls)
            {
                SetTooltip(component: component, text: null);
            }
            _tooltipControls.Clear();
            if (_toolTip != null)
            {
                _toolTip.Popup -= _toolTip_Popup;
                _toolTip.Dispose();
            }
            _toolTip = InitializeTooltip();
            _toolTip.Popup += _toolTip_Popup;
            _controlsLookupService.RemoveLookupControlsByForm(form: this.Form);
        }
        foreach (Control child in controls)
        {
            ClearControls(control: child);
            if (child is AsPanel)
            {
                (child as AsPanel).ShowAttachmentsChanged -= new EventHandler(
                    this.Form.PanelAttachementStateHandler
                );
            }
        }
        for (int i = 0; i < controls.Count; i++)
        {
            Control child = controls[index: i];
            child.Parent = null;
            child.Dispose();
        }

        if (!control.Controls.IsReadOnly)
        {
            control.Controls.Clear();
        }
        controls.Clear();
    }

    private void RemoveDataBindings(Hashtable bindings)
    {
        // remove all bindings
        foreach (DictionaryEntry entry in bindings)
        {
            try
            {
                Control control = entry.Key as Control;
                Binding binding = entry.Value as Binding;
                RemoveBinding(control: control, binding: binding);
            }
            catch { }
        }
    }

    public static void RemoveBinding(Control control, Binding binding)
    {
        if (control.DataBindings.Count == 0)
        {
            return;
        }

        control.DataBindings.Remove(binding: binding);
    }

    private void RemoveDataSourcesFromConsumers(Hashtable dataConsumers)
    {
        // remove all references to data sources
        foreach (DictionaryEntry entry in dataConsumers)
        {
            (entry.Key as IAsDataConsumer).DataSource = null;
        }
    }

    public void SetDataSourceToConsumers()
    {
        SetDataSourceToConsumers(dataConsumers: _dataConsumers);
    }

    private void SetDataSourceToConsumers(Hashtable dataConsumers)
    {
        this.IgnoreDataChanges = true;
        var sortedPanels = new List<AsPanel>();
        foreach (DictionaryEntry entry in dataConsumers)
        {
            if (entry.Key is AsPanel asPanel)
            {
                sortedPanels.Add(item: asPanel);
            }
        }
        sortedPanels.Sort();
        try
        {
            // first controls
            foreach (DictionaryEntry entry in dataConsumers)
            {
                if ((entry.Key as IAsDataConsumer).DataSource == null)
                {
                    if (!(entry.Key is AsPanel))
                    {
                        try
                        {
                            (entry.Key as IAsDataConsumer).DataSource = entry.Value;
                        }
                        catch { }
                    }
                }
            }
            // then panels
            foreach (AsPanel panel in sortedPanels)
            {
                if (panel.DataSource == null)
                {
                    panel.DataSource = dataConsumers[key: panel];
                }
            }
            Hashtable panels = new Hashtable();
            // then panel filters
            foreach (DictionaryEntry entry in dataConsumers)
            {
                AsPanel panel = entry.Key as AsPanel;
                if (panel != null && !panels.Contains(key: panel.DataMember))
                {
                    panels.Add(key: panel.DataMember, value: panel);
                }
            }
            foreach (AsPanel panel in sortedPanels)
            {
                panel.RefreshFilter();
            }
            this.IgnoreDataChanges = false;
            foreach (AsPanel panel in sortedPanels)
            {
                panel.UpdateSorting();
            }
        }
        finally
        {
            this.IgnoreDataChanges = false;
            // for each root panel we set the current record (load data piece, etc.)
            foreach (AsPanel panel in sortedPanels)
            {
                if (panel.DataMember.IndexOf(value: ".") < 0)
                {
                    panel.SetActualRecordId();
                }
            }
        }
    }

    private void ConfigureAsTextBox(AsTextBox textBox, DataColumn column, ErrorProvider errProvider)
    {
        textBox.Text = "";
        textBox.DataType = column.DataType;
        //textBox.ErrorInfo.BeepOnError=true;
        //textBox.ErrorInfo.ErrorAction = ErrorActionEnum.ResetValue;
        //
        //			textBox.NullText = "(Prazdn})";
        //textBox.AllowDbNull = column.AllowDBNull;
        textBox.TextAlign = HorizontalAlignment.Right;

        if (column.DataType == typeof(string))
        {
            textBox.TextAlign = HorizontalAlignment.Left;
        }
        else if (column.DataType == typeof(int))
        {
            //textBox.FormatType = C1.Win.C1Input.FormatTypeEnum.Integer;
            if (!string.IsNullOrEmpty(value: textBox.CustomNumericFormat))
            {
                textBox.CustomFormat = textBox.CustomNumericFormat;
            }
            else
            {
                textBox.CustomFormat = "###,###,###,###,##0";
            }
        }
        else if (column.DataType == typeof(long))
        {
            //textBox.FormatType = C1.Win.C1Input.FormatTypeEnum.CustomFormat;
            if (!string.IsNullOrEmpty(value: textBox.CustomNumericFormat))
            {
                textBox.CustomFormat = textBox.CustomNumericFormat;
            }
            else
            {
                textBox.CustomFormat = "###,###,###,###,##0";
            }
        }
        else if (column.DataType == typeof(decimal))
        {
            if (
                column.ExtendedProperties.Contains(key: "OrigamDataType")
                && (OrigamDataType)column.ExtendedProperties[key: "OrigamDataType"]
                    == OrigamDataType.Currency
            )
            {
                //textBox.FormatType = C1.Win.C1Input.FormatTypeEnum.CustomFormat;
                textBox.CustomFormat = "###,###,###,###,##0.00##";
            }
            else
            {
                //textBox.FormatType = C1.Win.C1Input.FormatTypeEnum.CustomFormat;
                textBox.CustomFormat = "###,###,###,###,##0.##################";
            }
        }
        else if (column.DataType == typeof(float))
        {
            //textBox.FormatType = C1.Win.C1Input.FormatTypeEnum.GeneralNumber;
            textBox.CustomFormat = "###,###,###,###,##0.00##";
        }
        else
        {
            textBox.TextAlign = HorizontalAlignment.Left;
        }
    }

    private void Initialize(Guid version)
    {
        if (version == Guid.Empty)
        {
            throw new NullReferenceException(
                message: ResourceUtils.GetString(key: "ErrorVersionEmpty")
            );
        }
    }

    public void BindControls()
    {
        this.BindControls(bindings: _bindings);
    }

    public void BindControls(Control parent)
    {
        Hashtable bindings = new Hashtable();
        foreach (DictionaryEntry entry in _bindings)
        {
            Control control = entry.Key as Control;
            if (IsChildControl(parent: parent, control: control))
            {
                bindings.Add(key: entry.Key, value: entry.Value);
            }
        }
        this.BindControls(bindings: bindings);
    }

    private void BindControls(Hashtable bindings)
    {
        foreach (DictionaryEntry entry in bindings)
        {
            if (
                (entry.Key as Control).DataBindings[
                    propertyName: (entry.Value as Binding).PropertyName
                ] == null
            )
            {
                (entry.Key as Control).DataBindings.Add(binding: entry.Value as Binding);
            }
        }
    }

    public static bool IsChildControl(Control parent, Control control)
    {
        while (control != null)
        {
            if (control.Parent == parent)
            {
                return true;
            }
            control = control.Parent;
        }
        return false;
    }

    private Control LoadControl(
        ControlSetItem cntrlSet,
        string dataMember,
        Hashtable bindings,
        Hashtable dataConsumers,
        Control parentControl,
        bool ignoreTabPages,
        bool readOnly
    )
    {
        if (!FormTools.IsValid(features: cntrlSet.Features, roles: cntrlSet.Roles))
        {
            return null;
        }

        readOnly = FormTools.GetReadOnlyStatus(cntrlSet: cntrlSet, currentReadOnlyStatus: readOnly);
        //create control
        Control cntrl = CreateInstance(
            cntrlSet: cntrlSet,
            dataMember: dataMember,
            bindings: bindings,
            dataConsumers: dataConsumers,
            readOnly: readOnly
        );
        ISupportInitialize supportInitialize = cntrl as ISupportInitialize;
        if (supportInitialize != null)
        {
            supportInitialize.BeginInit();
        }
        //recursively add child controls
        var sortedChildControls = cntrlSet.ChildItemsByType<ControlSetItem>(
            itemType: ControlSetItem.CategoryConst
        );
        sortedChildControls.Sort();

        foreach (ControlSetItem childItem in sortedChildControls)
        {
            try
            {
                BuildChildItem(
                    dataMember: dataMember,
                    bindings: bindings,
                    dataConsumers: dataConsumers,
                    readOnly: readOnly,
                    cntrl: cntrl,
                    childItem: childItem
                );
            }
            catch (OrigamException)
            {
                throw;
            }
            catch (Exception e)
            {
                // add info and rethrow
                throw new OrigamException(
                    message: string.Format(
                        format: "Failed to build a widget `{0}' ({1}).",
                        arg0: childItem.Path,
                        arg1: childItem.Id
                    ),
                    innerException: e
                );
            }
        }
        // if the only child control is panel or tabcontrol, we fill the container with it
        if (cntrl.Controls.Count == 1)
        {
            Control theOnlyConrol = cntrl.Controls[index: 0] as Control;
            if (ShouldDockControl(c: theOnlyConrol))
            {
                theOnlyConrol.Dock = DockStyle.Fill;
            }
        }
        //Tag must be null
        cntrl.Tag = null;
        return cntrl;
    }

    private void BuildChildItem(
        string dataMember,
        Hashtable bindings,
        Hashtable dataConsumers,
        bool readOnly,
        Control cntrl,
        ControlSetItem childItem
    )
    {
        Control addingControl = LoadControl(
            cntrlSet: childItem,
            dataMember: dataMember,
            bindings: bindings,
            dataConsumers: dataConsumers,
            parentControl: cntrl,
            ignoreTabPages: true,
            readOnly: readOnly
        );
        if (addingControl != null)
        {
            if (cntrl is AsForm)
            {
                addingControl.Visible = false;
            }
            if (addingControl != null)
            {
                cntrl.Controls.Add(value: addingControl);
            }
            if (addingControl is AsPanel panel)
            {
                string table = FormTools.FindTableByDataMember(
                    ds: _mainFormData,
                    member: panel.DataMember
                );
                if (panel.HideNavigationPanel == false)
                {
                    ShowNavigationPanel(panel: panel, table: table);
                }
                ShowToolStrip(panel: panel, table: table, childItem: childItem);
            }
            // if Form == null, then this was called from the designer - we will not register lookup controls for that
            if ((addingControl is ILookupControl) && (Form != null))
            {
                _controlsLookupService.AddLookupControl(
                    lookupControl: addingControl as ILookupControl,
                    form: Form,
                    showEditCommand: true
                );
            }
            if (addingControl is IAsDataConsumer)
            {
                addingControl.BindingContext = _bindContext;
                dataConsumers.Add(key: addingControl, value: _mainFormData);
            }
            // for each control set DataBindings when is on its container
            LoadControlDataBindings(
                cntrl: addingControl,
                cntrSetItem: childItem,
                dataMember: dataMember,
                dataSource: _mainFormData,
                bindings: bindings,
                dataConsumers: dataConsumers
            );
        }
    }

    private void ShowToolStrip(AsPanel panel, string table, ControlSetItem childItem)
    {
        var validActions = new List<EntityUIAction>();
        Guid entityId = new Guid(
            g: _mainFormData.Tables[name: table].ExtendedProperties[key: "EntityId"].ToString()
        );
        UIActionTools.GetValidActions(
            formId: (Guid)_formKey[key: "Id"],
            panelId: childItem.ControlItem.PanelControlSet.Id,
            disableActionButtons: panel.DisableActionButtons,
            entityId: entityId,
            validActions: validActions
        );
        panel.Actions = validActions;
        if (validActions.Count > 0)
        {
            CreatePanelToolStrip(panel: panel, actions: validActions);
        }
    }

    private void ShowNavigationPanel(AsPanel panel, string table)
    {
        AttachmentHandle(panel: panel);
        if (_mainDataStructureSortSet != null)
        {
            foreach (DataStructureSortSetItem item in _mainDataStructureSortSet.ChildItems)
            {
                if (item.Entity.Name == table)
                {
                    panel.CurrentSort.Add(
                        key: item.FieldName,
                        value: new DataSortItem(
                            columnName: item.FieldName,
                            sortDirection: item.SortDirection,
                            sortOrder: item.SortOrder
                        )
                    );
                }
            }
        }
    }

    private IEnumerable<EntityUIAction> GetChildActions(EntityDropdownAction dropDownAction)
    {
        foreach (var item in dropDownAction.ChildItems)
        {
            if (item is EntityUIAction action)
            {
                yield return action;
            }
        }
    }

    private void CreatePanelToolStrip(AsPanel panel, List<EntityUIAction> actions)
    {
        AsForm parentForm = AsyncForm ?? Form;
        var toolStrip = new LabeledToolStrip(owner: parentForm);
        toolStrip.Text = panel.PanelTitle;
        _toolStripContainer.Controls.Add(value: toolStrip);
        panel.ToolStrip = toolStrip;
        var dropDownActions = actions
            .Where(predicate: action => action is EntityDropdownAction)
            .Cast<EntityDropdownAction>()
            .SelectMany(selector: GetChildActions)
            .ToList();

        foreach (EntityUIAction action in actions)
        {
            if (dropDownActions.Contains(item: action))
            {
                continue; // we don't have to do anything with these!
            }

            var actionButton = GetActionButtonInstance(action: action);
            actionButton.Enabled = false;
            actionButton.Visible = false;
            toolStrip.Items.Add(value: actionButton);
        }
        panel.ActionButtons = toolStrip.Items.Cast<ToolStripItem>().ToList();

        panel.BindActionButtons();
    }

    private ToolStripItem GetActionButtonInstance(EntityUIAction action)
    {
        if (action is EntityDropdownAction dropDownAction)
        {
            return new ToolStripActionDropDownButton(dropdownAction: dropDownAction);
        }
        return new ToolStripActionButton(action: action);
    }

    private bool ShouldDockControl(Control c)
    {
        if (
            c is AsPanel
            || c is SplitPanel
            || c is TabControl
            || c.GetType().Name == "AsReportPanel"
            || c is TreeView
        )
        {
            return true;
        }
        return false;
    }

    private void LoadControlDataBindings(
        Control cntrl,
        ControlSetItem cntrSetItem,
        string dataMember,
        object dataSource,
        Hashtable bindings,
        Hashtable dataConsumers
    )
    {
        if (dataMember == null || dataMember.Length < 1)
        {
            return;
        }

        DataTable table;

        if (dataSource is DataSet)
        {
            table = (dataSource as DataSet).Tables[
                name: FormTools.FindTableByDataMember(ds: _mainFormData, member: dataMember)
            ];
            if (table == null)
            {
                throw new NullReferenceException(
                    message: ResourceUtils.GetString(key: "ErrorTableNotFound", args: dataMember)
                );
            }
        }
        else if (dataSource is DataTable)
        {
            table = dataSource as DataTable;
        }
        else
        {
            return;
        }
        foreach (
            var bindItem in cntrSetItem.ChildItemsByType<PropertyBindingInfo>(
                itemType: PropertyBindingInfo.CategoryConst
            )
        )
        {
            string propertyName = bindItem.ControlPropertyItem.Name;
            PropertyInfo property = GetPropertyInfo(
                controlType: cntrl.GetType(),
                propertyName: propertyName
            );

            if (property != null)
            {
                if (!table.Columns.Contains(name: bindItem.Value))
                {
                    throw new InvalidOperationException(
                        message: ResourceUtils.GetString(
                            key: "ErrorColumnNotFound",
                            args: new object[] { bindItem.Value, table.TableName }
                        )
                    );
                }
                Binding bind = new Binding(
                    propertyName: propertyName,
                    dataSource: dataSource,
                    dataMember: dataMember + "." + bindItem.Value
                );
                //bind.Format +=new ConvertEventHandler(bind_Format);
                //bind.Parse +=new ConvertEventHandler(bind_Parse);

                if (
                    (cntrl is IAsCaptionControl)
                    && (
                        (cntrl as IAsCaptionControl).Caption == "LABEL"
                        || (cntrl as IAsCaptionControl).Caption == ""
                    )
                )
                {
                    (cntrl as IAsCaptionControl).Caption = table
                        .Columns[name: bindItem.Value]
                        .Caption;
                }
                if (cntrl is AsTextBox) // && table.Columns[bindItem.Value].DataType == typeof(string) )
                {
                    ConfigureAsTextBox(
                        textBox: cntrl as AsTextBox,
                        column: table.Columns[name: bindItem.Value] as DataColumn,
                        errProvider: null
                    );

                    (cntrl as AsTextBox).MaxLength =
                        table.Columns[name: bindItem.Value].MaxLength == -1
                            ? 0
                            : table.Columns[name: bindItem.Value].MaxLength;
                    //(cntrl as AsTextBox).DataField  = dataMember + "." + bindItem.Value;
                }
                bindings.Add(key: cntrl, value: bind);
                Guid columnId = (Guid)
                    table.Columns[name: bindItem.Value].ExtendedProperties[key: "Id"];
                string tipText = _documentationService.GetDocumentation(
                    schemaItemId: columnId,
                    docType: DocumentationType.USER_LONG_HELP
                );
                SetTooltip(component: cntrl, text: tipText);
            }
        }
    }

    private Control CreateInstance(
        ControlSetItem cntrlSet,
        string dataMember,
        Hashtable bindings,
        Hashtable dataConsumers,
        bool readOnly
    )
    {
        Control result = null;
        string itemDataMember = "";
        if (cntrlSet.ControlItem.IsComplexType)
        {
            #region complex control
            PanelControlSet panel = cntrlSet.ControlItem.PanelControlSet;

            if (panel != null)
            {
                itemDataMember = "";

                foreach (
                    var item in cntrlSet.ChildItemsByType<PropertyValueItem>(
                        itemType: PropertyValueItem.CategoryConst
                    )
                )
                {
                    if (item.ControlPropertyItem.Name == "DataMember")
                    {
                        itemDataMember = item.Value;
                    }
                    if (itemDataMember == null)
                    {
                        throw new NullReferenceException(
                            message: ResourceUtils.GetString(
                                key: "ErrorDataMemberNull",
                                args: cntrlSet.Name
                            )
                        );
                    }
                }
                //found datamember provide to controlcreation
                result = LoadControl(
                    cntrlSet: FormTools.GetItemFromControlSet(controlSet: panel),
                    dataMember: itemDataMember,
                    bindings: bindings,
                    dataConsumers: dataConsumers,
                    parentControl: null,
                    ignoreTabPages: true,
                    readOnly: readOnly
                );
                result.Tag = cntrlSet;
                SetControlProperties(cntrl: result, readOnly: readOnly);
                return result;
            }
            #endregion
        }

        if (
            cntrlSet == null
            || cntrlSet.ControlItem == null
            || cntrlSet.ControlItem.ControlType == null
            || cntrlSet.ControlItem.ControlNamespace == null
        )
        {
            throw new ArgumentException(
                message: ResourceUtils.GetString(key: "ErrorParameterNull"),
                paramName: "cntrlSet"
            );
        }
        if (cntrlSet.ControlItem.ControlType == typeof(AsForm).ToString() && (this.Form != null))
        {
            result = this.Form;
        }
        else
        {
            result =
                Origam.Reflector.InvokeObject(
                    classname: cntrlSet.ControlItem.ControlType,
                    assembly: cntrlSet.ControlItem.ControlNamespace
                ) as Control;
        }

        if (result == null)
        {
            throw new NullReferenceException(
                message: ResourceUtils.GetString(
                    key: "ErrorUnsupportedType",
                    args: cntrlSet.ControlItem.ControlType
                )
            );
        }

        AsPanel panelResult = result as AsPanel;
        AsForm formResult = result as AsForm;
        if (formResult != null)
        {
            formResult.FormGenerator = this;
        }
        else if (panelResult != null)
        {
            panelResult.Generator = this;
        }

        (result as Control).Tag = cntrlSet;
        //TEST
        if (result is AsDateBox)
        {
            (result as AsDateBox).Format = DateTimePickerFormat.Custom;
            (result as AsDateBox).CustomFormat = "dd.MMMM yyyy";
        }
        SetControlProperties(cntrl: result as Control, readOnly: readOnly);
        if (result is TabControl)
        {
            (result as TabControl).SelectedIndexChanged += new EventHandler(
                FormGenerator_TabPageSelectedIndexChanged
            );
        }
        return (result as Control);
    }

    private PropertyInfo GetPropertyInfo(Type controlType, string propertyName)
    {
        PropertyInfo result = null;
        object[] key = new object[2] { controlType, propertyName };
        if (_propertyCache.Contains(key: key))
        {
            result = _propertyCache[key: key] as PropertyInfo;
        }
        else
        {
            result = controlType.GetProperty(name: propertyName);
            if (result == null)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "propertyName",
                    actualValue: propertyName,
                    message: ResourceUtils.GetString(
                        key: "ErrorPropertyNotFound",
                        args: new object[] { propertyName, controlType.ToString() }
                    )
                );
            }

            _propertyCache.Add(key: key, value: result);
        }

        return result;
    }

    private void AttachmentHandle(AsPanel panel)
    {
        if (panel == null)
        {
            return;
        }

        panel.ShowAttachmentsChanged += new EventHandler(this.Form.PanelAttachementStateHandler);
    }

    private void SetControlProperties(Control cntrl, bool readOnly)
    {
        //			if(cntrl is ISupportInitialize)
        //			{
        //				(cntrl as ISupportInitialize).BeginInit();
        //			}
        if (!(cntrl.Tag is ControlSetItem))
        {
            return;
        }

        ControlSetItem cntrSetItem = cntrl.Tag as ControlSetItem;
        PropertyInfo propToSet = null;
        PropertyInfo heightProperty = null;
        PropertyValueItem heightValueItem = null;

        bool setProperty = false;

        foreach (
            var propValItem in cntrSetItem.ChildItemsByType<PropertyValueItem>(
                itemType: PropertyValueItem.CategoryConst
            )
        )
        {
            propToSet = GetPropertyInfo(
                controlType: cntrl.GetType(),
                propertyName: propValItem.ControlPropertyItem.Name
            );

            setProperty = true;
            if (readOnly)
            {
                if (propToSet.Name == "ReadOnly")
                {
                    Reflector.SetValue(memberInfo: propToSet, instance: cntrl, value: true);
                    setProperty = false;
                }
                AsPanel p = cntrl as AsPanel;
                if (p != null)
                {
                    if (propToSet.Name == "ShowDeleteButton")
                    {
                        p.ShowDeleteButton = false;
                        setProperty = false;
                    }

                    if (propToSet.Name == "ShowNewButton")
                    {
                        p.ShowNewButton = false;
                        setProperty = false;
                    }
                }
            }
            if (cntrl is AsTextBox)
            {
                if (propToSet.Name == "Height")
                {
                    heightProperty = propToSet;
                    heightValueItem = propValItem;
                    FormGenerator.LoadPropertyValue(
                        control: cntrl,
                        property: propToSet,
                        propertyValueItem: propValItem
                    );
                    setProperty = false;
                }
                else if (propToSet.Name == "Multiline")
                {
                    FormGenerator.LoadPropertyValue(
                        control: cntrl,
                        property: propToSet,
                        propertyValueItem: propValItem
                    );

                    if (heightProperty != null && heightValueItem != null)
                    {
                        FormGenerator.LoadPropertyValue(
                            control: cntrl,
                            property: heightProperty,
                            propertyValueItem: heightValueItem
                        );
                    }
                    setProperty = false;
                }
            }
            if (setProperty)
            {
                FormGenerator.LoadPropertyValue(
                    control: cntrl,
                    property: propToSet,
                    propertyValueItem: propValItem
                );
            }
        }
        if (cntrl is GroupBoxWithChamfer | cntrl is TabControl | cntrl is TabPage)
        {
            cntrl.TabStop = false;
        }
        if (cntrl is IOrigamMetadataConsumer)
        {
            (cntrl as IOrigamMetadataConsumer).OrigamMetadata = cntrSetItem;
        }
        //			if(cntrl is ISupportInitialize)
        //			{
        //				(cntrl as ISupportInitialize).EndInit();
        //			}
    }

    private PropertyValueItem FindPropertyValueItem(
        ControlSetItem controlSetItem,
        ControlPropertyItem propertyToFind
    )
    {
        PropertyValueItem result = null;
        foreach (
            var item in controlSetItem.ChildItemsByType<PropertyValueItem>(
                itemType: "PropertyValueItem"
            )
        )
        {
            if (item.ControlPropertyItem.PrimaryKey.Equals(obj: propertyToFind.PrimaryKey))
            {
                result = item;
                break;
            }
        }
        return result;
    }

    private void LoadMainData()
    {
        Guid dsId = (
            _listDataStructureId == Guid.Empty ? _mainDataStructureId : _listDataStructureId
        );
        Guid filterId = (
            _listDataStructureId == Guid.Empty
                ? _mainDataStructureMethodId
                : _listDataStructureMethodId
        );
        Guid defaultId = (
            _listDataStructureId == Guid.Empty ? _mainDataStructureDefaultSetId : Guid.Empty
        );
        Guid sortSetId = (
            _listDataStructureId == Guid.Empty ? _mainDataStructureSortSetId : Guid.Empty
        );
        DataStructureQuery query = new DataStructureQuery(
            dataStructureId: dsId,
            methodId: filterId,
            defaultSetId: defaultId,
            sortSetId: sortSetId
        );
        _dataServiceAgent.MethodName = "LoadDataByQuery";
        _dataServiceAgent.Parameters.Clear();
        _dataServiceAgent.Parameters.Add(key: "Query", value: query);
        // for lazily-loaded initial screen load the list data directly into
        // our dataset (without any merge)
        if (_listDataStructureId != Guid.Empty)
        {
            SetEmptyData();
            _dataServiceAgent.Parameters.Add(key: "Data", value: _mainFormData);
        }
        AddQueryParameters(query: query);
#if ASYNC
        SetEmptyData();

        Thread thread = new Thread(new ThreadStart(LoadData));
        thread.Name = "LoadData_" + this.Form.TitleName;
        thread.IsBackground = true;
        thread.Start();
#else
        LoadData();
#endif
    }

    private void LoadData()
    {
        _dataServiceAgent.Run();
        DataSet result = _dataServiceAgent.Result as DataSet;
#if ASYNC
        UnSubscribeDataTableEvents();
        _mainFormData.Merge(result);
        SubscribeDataTableEvents();
#else
        if (_listDataStructureId == Guid.Empty)
        {
            _mainFormData = result;
            bool selfJoinExists = false;
            foreach (DataRelation r in result.Relations)
            {
                if (r.ParentTable.Equals(obj: r.ChildTable))
                {
                    selfJoinExists = true;
                    break;
                }
            }
            if (!selfJoinExists)
            {
                // no XML for self joins (incompatible with XmlDataDocument)
                XmlData = DatasetToXml(data: result);
            }
            else
            {
                // but sort columns must be here, even though there is no xml
                DatasetTools.AddSortColumns(data: result);
            }
        }
#endif
        DatasetGenerator.ApplyDynamicDefaults(
            data: _mainFormData,
            parameters: this.SelectionParameters
        );
        _loadedPieces.Clear();
    }

    private void SetEmptyData()
    {
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        DataStructure ds =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(DataStructure),
                primaryKey: new ModelElementKey(id: _mainDataStructureId)
            ) as DataStructure;
        DataStructureDefaultSet defaultSet =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(DataStructureDefaultSet),
                primaryKey: new ModelElementKey(id: _mainDataStructureDefaultSetId)
            ) as DataStructureDefaultSet;
        DataSet data = new DatasetGenerator(userDefinedParameters: true).CreateDataSet(
            ds: ds,
            defaultSet: defaultSet
        );
        _mainFormData = data;
        XmlData = DatasetToXml(data: data);
    }

    public static IDataDocument DatasetToXml(DataSet data)
    {
        DatasetTools.AddSortColumns(data: data);
        return DataDocumentFactory.New(dataSet: data);
    }

    private void AddQueryParameters(DataStructureQuery query)
    {
        foreach (DictionaryEntry entry in this.SelectionParameters)
        {
            query.Parameters.Add(
                value: new QueryParameter(_parameterName: (string)entry.Key, value: entry.Value)
            );
        }
    }

    private void SubscribeDataTableEvents()
    {
        foreach (DataTable table in _mainFormData.Tables)
        {
            table.RowChanged -= new DataRowChangeEventHandler(table_RowChanged);
            table.RowChanged += new DataRowChangeEventHandler(table_RowChanged);
            table.ColumnChanging -= table_ColumnChanging;
            table.ColumnChanging += table_ColumnChanging;
            table.ColumnChanged -= new DataColumnChangeEventHandler(table_ColumnChanged);
            table.ColumnChanged += new DataColumnChangeEventHandler(table_ColumnChanged);
        }
    }

    private void UnSubscribeDataTableEvents()
    {
        if (_mainFormData != null)
        {
            foreach (DataTable table in _mainFormData.Tables)
            {
                table.ColumnChanging -= table_ColumnChanging;
                table.RowChanged -= new DataRowChangeEventHandler(table_RowChanged);
                table.ColumnChanged -= new DataColumnChangeEventHandler(table_ColumnChanged);
            }
        }
    }

    private void RemoveNullConstraints()
    {
        _mainFormData.RemoveNullConstraints();
    }

    #endregion
    #region Event Handlers
    /// <summary>
    /// Delayed control loading. After tab page is displayed for the first time,
    /// its child controls get loaded on the form.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void FormGenerator_TabPageSelectedIndexChanged(object sender, EventArgs e)
    {
        if (_disposing | _unloadingForm)
        {
            return;
        }

        TabPage page = (sender as TabControl).SelectedTab;

        if (page == null)
        {
            return;
        }

        page.SelectNextControl(
            ctl: page,
            forward: true,
            tabStopOnly: true,
            nested: true,
            wrap: true
        );
    }

    internal void table_rowCopied(DataRow row, IDataDocument document)
    {
        try
        {
            _ruleHandler.OnRowCopied(
                row: row,
                data: document,
                ruleSet: this.RuleSet,
                ruleEngine: _formRuleEngine
            );
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(
                owner: this.Form,
                text: ex.Message,
                caption: ResourceUtils.GetString(
                    key: "ExecuteRuleTitle",
                    args: this.Form.TitleName
                ),
                exception: ex
            );
        }
    }

    internal void table_RowChanged(object sender, DataRowChangeEventArgs e)
    {
        if (this.IgnoreDataChanges || !ValidateChanges(e: e))
        {
            return;
        }
        this.Form.IsDirty = true;
        try
        {
            _ruleHandler.OnRowChanged(
                e: e,
                data: XmlData,
                ruleSet: this.RuleSet,
                ruleEngine: _formRuleEngine
            );
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(
                owner: this.Form,
                text: ex.Message,
                caption: ResourceUtils.GetString(
                    key: "ExecuteRuleTitle",
                    args: this.Form.TitleName
                ),
                exception: ex
            );
        }
    }

    private bool ValidateChanges(DataRowChangeEventArgs e)
    {
        OrigamDataRow row = e.Row as OrigamDataRow;
        bool retVal = row.HasColumnWithValidChange();
        row.ResetColumnsWithValidChange();
        return retVal;
    }

    internal void table_RowDeleted(DataRow[] parentRows, DataRow deletedRow)
    {
        if (this.IgnoreDataChanges)
        {
            return;
        }

        this.Form.IsDirty = true;
        try
        {
            _ruleHandler.OnRowDeleted(
                parentRows: parentRows,
                deletedRow: deletedRow,
                data: XmlData,
                ruleSet: this.RuleSet,
                ruleEngine: _formRuleEngine
            );
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(
                owner: this.Form,
                text: ex.Message,
                caption: ResourceUtils.GetString(key: "DeleteRowTitle", args: this.Form.TitleName),
                exception: ex
            );
        }
    }

    private void table_ColumnChanging(object sender, DataColumnChangeEventArgs e)
    {
        if (!IgnoreDataChanges && !e.Row[column: e.Column].Equals(obj: e.ProposedValue))
        {
            (e.Row as OrigamDataRow).AddColumnWithValidChange(dataColumn: e.Column);
        }
    }

    private void table_ColumnChanged(object sender, DataColumnChangeEventArgs e)
    {
        if (
            this.IgnoreDataChanges
            || !(e.Row as OrigamDataRow).IsColumnWithValidChange(dataColumn: e.Column)
        )
        {
            System.Diagnostics.Debug.WriteLine(
                value: e.ProposedValue,
                category: "ignoring " + e.Column.Table.TableName + "." + e.Column.ColumnName
            );
            return;
        }
        try
        {
            System.Diagnostics.Debug.WriteLine(
                value: e.ProposedValue,
                category: "passing " + e.Column.Table.TableName + "." + e.Column.ColumnName
            );
            _ruleHandler.OnColumnChanged(
                e: e,
                data: XmlData,
                ruleSet: this.RuleSet,
                ruleEngine: _formRuleEngine
            );
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(
                owner: this.Form,
                text: ex.Message,
                caption: "Vykonání pravidla ve formuláři '" + this.Form.TitleName + "'",
                exception: ex
            );
        }
    }
    #endregion
    #region IDisposable Members
    private bool _disposing = false;

    public void Dispose()
    {
        _disposing = true;
        if (_selectionDialogToolbar != null)
        {
            _selectionDialogToolbar.ButtonClick -= new ToolBarButtonClickEventHandler(
                selectionDialogToolbar_Click
            );
            _selectionDialogToolbar.ImageList = null;
            _selectionDialogToolbar.Dispose();
        }
        if (_selectionDialogOKButton != null)
        {
            _selectionDialogOKButton.Click -= new EventHandler(selectionDialogOKButton_Click);
        }
        if (_selectionDialogCancelButton != null)
        {
            _selectionDialogCancelButton.Click -= new EventHandler(
                selectionDialogCancelButton_Click
            );
        }
        UnSubscribeDataTableEvents();
        UnloadForm(showCurtain: false);
        if (_toolTip != null)
        {
            _toolTip.Dispose();
            _toolTip = null;
        }
        if (_gridTooltips != null)
        {
            _gridTooltips.Clear();
        }
        _bindContext = null;
        _dataServiceAgent = null;
        _documentationService = null;
        _lookupManager = null;
        if (this.Form != null)
        {
            this.Form.CancelButton = null;
            this.Form.AcceptButton = null;
            this.Form.BindingContext = null;
            this.Form = null;
        }
        if (XmlData != null)
        {
            XmlData = null;
        }
        if (_mainFormData != null)
        {
            //_mainFormData.Clear(); - not possible, because of the XmlDataDocument
            _mainFormData = null;
        }
    }
    #endregion
    private void selectionDialogToolbar_Click(object sender, ToolBarButtonClickEventArgs e)
    {
        switch (e.Button.Tag.ToString())
        {
            case "OK":
            {
                selectionDialogOKButton_Click(sender: this, e: EventArgs.Empty);
                break;
            }

            case "CANCEL":
            {
                selectionDialogCancelButton_Click(sender: this, e: EventArgs.Empty);
                break;
            }
        }
    }

    private void selectionDialogOKButton_Click(object sender, EventArgs e)
    {
        this.Form.EndCurrentEdit();
        if (_selectionDialogEndRule != null)
        {
            RuleEngine ruleEngine = RuleEngine.Create();
            RuleExceptionDataCollection ruleExceptions = ruleEngine.EvaluateEndRule(
                rule: _selectionDialogEndRule,
                data: XmlData
            );
            if (ruleExceptions != null && ruleExceptions.Count > 0)
            {
                RuleException ex = new RuleException(result: ruleExceptions);
                bool shouldReturn = DisplayRuleException(window: this.Form, ruleEx: ex);
                if (shouldReturn)
                {
                    return;
                }
            }
        }
        foreach (DataTable table in this.DataSet.Tables)
        {
            foreach (DataRow row in table.Rows)
            {
                row.ClearErrors();
            }
        }
        this.Form.DialogResult = DialogResult.OK;
        this.Form.Close();
    }

    private void selectionDialogCancelButton_Click(object sender, EventArgs e)
    {
        this.Form.DialogResult = DialogResult.Cancel;
        this.Form.Close();
    }

    public bool LoadDataPiece(object id, string dataMember)
    {
        if (dataMember == _listDataMember && id != null && !(id is Guid && (Guid)id == Guid.Empty))
        {
            if (_loadedPieces.Contains(key: id))
            {
                return false;
            }

            ShowProgress(
                text: null,
                position: ProgressPosition.TopRight,
                parent: this.Form.NameLabel
            );
            this.IgnoreDataChanges = true;
            try
            {
                UnSubscribeDataTableEvents();
                // unbinding completely makes grid not completing navigation
                // e.g. when clicking on keyboard cursor down arrow it loads next
                // record but fails navigating there
                DataStructureQuery query = new DataStructureQuery(
                    dataStructureId: _mainDataStructureId,
                    methodId: _mainDataStructureMethodId
                );
                _dataServiceAgent.MethodName = "LoadDataByQuery";
                _dataServiceAgent.Parameters.Clear();
                _dataServiceAgent.Parameters.Add(key: "Query", value: query);
                IPersistenceService persistence =
                    ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
                    as IPersistenceService;
                DataStructureMethod fs =
                    persistence.SchemaProvider.RetrieveInstance(
                        type: typeof(DataStructureMethod),
                        primaryKey: new ModelElementKey(id: _mainDataStructureMethodId)
                    ) as DataStructureMethod;
                if (fs == null)
                {
                    throw new ArgumentNullException(
                        paramName: "MainDataStructureMethod",
                        message: ResourceUtils.GetString(key: "ErrorDelayedData")
                    );
                }

                foreach (string parameterName in fs.ParameterReferences.Keys)
                {
                    query.Parameters.Add(
                        value: new QueryParameter(_parameterName: parameterName, value: id)
                    );
                }
                _dataServiceAgent.Run();
                DataSet result = _dataServiceAgent.Result as DataSet;
                _loadedPieces.Add(key: id, value: null);
                var sortedPanels = this.Form.Panels.ToList();
                sortedPanels.Sort();
                foreach (AsPanel panel in sortedPanels)
                {
                    if (panel.DataMember != dataMember)
                    {
                        panel.BindGrid(isBinding: false);
                    }
                }
                try
                {
                    DatasetTools.BeginLoadData(dataset: _mainFormData);
                    MergeParams mergeParams = new MergeParams(
                        ProfileId: SecurityManager.CurrentUserProfile().Id
                    );
                    mergeParams.PreserveNewRowState = true;
                    DatasetTools.MergeDataSet(
                        inout_dsTarget: _mainFormData,
                        in_dsSource: result,
                        changeList: null,
                        mergeParams: mergeParams
                    );
                }
                catch (Exception ex)
                {
                    AsMessageBox.ShowError(
                        owner: this.Form,
                        text: ResourceUtils.GetString(key: "ErrorDetailedData"),
                        caption: ResourceUtils.GetString(
                            key: "ErrorFetchDataTitle",
                            args: this.Form.TitleName
                        ),
                        exception: ex
                    );
                }
                foreach (AsPanel panel in sortedPanels)
                {
                    try
                    {
                        if (panel.DataMember != dataMember)
                        {
                            CurrencyManager cm =
                                _bindContext[dataSource: this.DataSet, dataMember: panel.DataMember]
                                as CurrencyManager;
                            if (cm != null)
                            {
                                cm.Refresh();
                            }
                            panel.BindGrid();
                        }
                    }
                    catch { }
                }
                this.IgnoreDataChanges = false;
                ResetTempColumns();
                foreach (AsPanel panel in sortedPanels)
                {
                    panel.UpdateSorting();
                }
                SubscribeDataTableEvents();
            }
            finally
            {
                DatasetTools.EndLoadData(dataset: _mainFormData);
                HideProgress();
                this.IgnoreDataChanges = false;
            }
            return true;
        }

        return false;
    }
}
