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
using Origam.UI;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Gui.UI;
using WeifenLuo.WinFormsUI.Docking;

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for AsForm.
/// </summary>
public class AsForm : DockContent, IViewContent, IRecordReferenceProvider,
	IOrigamForm, IToolStripContainer
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
        return Panels.Select(x => x.ToolStrip).ToList();
    }
	/// <summary>
	/// Recursively goes through all child records and finds all the record Id's
	/// </summary>
	/// <param name="row"></param>
	/// <param name="references"></param>
	internal void RetrieveChildReferences(DataRow row, Hashtable references)
	{
		foreach(DataRelation relation in row.Table.DataSet.Relations)
		{
			if(relation.ParentTable == row.Table)				// we find all the child relations of our row's table
			{
				if(relation.ChildTable.Columns.Contains(Const.ValuelistIdField)) // only if there exists Id column
				{
					Guid entityId = new Guid(relation.ChildTable.ExtendedProperties["EntityId"].ToString()); 
					foreach(DataRow childRow in row.GetChildRows(relation))
					{
						Guid recordId = (Guid)childRow[Const.ValuelistIdField];
							RecordReference newRef = new RecordReference(entityId, recordId);
							if(! references.Contains(newRef.GetHashCode()))
							{
								references.Add(newRef.GetHashCode(), newRef);
							}
						RetrieveChildReferences(childRow, references);	// recursion
					}
				}
			}
		}
	}
	public void panel_RecordIdChanged (object sender, EventArgs e)
	{
		if(this.PanelBindingSuspendedTemporarily) return;
		AsPanel panel = sender as AsPanel;
        
		if(panel.ShowAttachments)
		{
			MainEntityId = panel.EntityId;
			SetMainRecordId(panel.RecordId);
			ChildRecordReferences.Clear();
			if(panel.BindingContext[panel.DataSource, panel.DataMember].Position > -1)
			{
				DataRow row = (panel.BindingContext[panel.DataSource, panel.DataMember].Current as DataRowView).Row;
				RetrieveChildReferences(row, ChildRecordReferences);
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
		RecordReferenceChanged?.Invoke(this, this.MainEntityId, this.MainRecordId, this.ChildRecordReferences);
	}
	public void PanelAttachementStateHandler (object sender, EventArgs e)
	{
		if( !(sender is AsPanel) )
			return;
		AsPanel panel = sender as AsPanel;
		// panel is setting ShowAttachments to False
		if(_currentHandledPanel != null && panel.Name == _currentHandledPanel.Name && panel.ShowAttachments == false)
		{
			_currentHandledPanel.RecordIdChanged -= panel_RecordIdChanged;
			_currentHandledPanel = null;
			panel_RecordIdChanged(sender, EventArgs.Empty);
			return;
		}
		// we hide attachments for the currently handled panel
		if(_currentHandledPanel != null)
		{
			_currentHandledPanel.RecordIdChanged -= panel_RecordIdChanged;
			_currentHandledPanel.ShowAttachments = false;
		}
		panel_RecordIdChanged(sender, EventArgs.Empty);
		// and we set the currently handled panel to the new panel
		_currentHandledPanel = panel;
		// then we subscribe for changes in the new panel
		_currentHandledPanel.RecordIdChanged += panel_RecordIdChanged;
	}
	#endregion
    public AsForm()
	{
		InitializeComponent();
		SetStyle(ControlStyles.DoubleBuffer, true);
		this.BackColor = OrigamColorScheme.FormBackgroundColor;
		this.Paint += AsForm_Paint;
	}
	public AsForm(FormGenerator generator) : this()
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
			ToolStripsLoaded?.Invoke(null, EventArgs.Empty);
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
			if(! _bindingsInitialized && _extraControlBindings != "")
			{
				System.Xml.Serialization.XmlSerializer xsr = new System.Xml.Serialization.XmlSerializer(typeof(ComponentBindingCollection));
				_componentBindings = xsr.Deserialize(new System.IO.StringReader(_extraControlBindings)) as ComponentBindingCollection;
			}
			_bindingsInitialized = true;
			return _componentBindings;
		}
	}
	private string _extraControlBindings = "";
	[Browsable(false)]
	public string ExtraControlBindings
	{
		get
		{
			if(_bindingsInitialized)
			{
				System.Text.StringBuilder sb = new System.Text.StringBuilder();
				System.Xml.Serialization.XmlRootAttribute root = new System.Xml.Serialization.XmlRootAttribute("ComponentBindings");
				System.Xml.Serialization.XmlSerializer xsr = new System.Xml.Serialization.XmlSerializer(typeof(ComponentBindingCollection), root);
				xsr.Serialize(new System.IO.StringWriter(sb), _componentBindings);
				
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
			if(IsSaving)
			{
				return false;
			}
			else
			{
				return _canRefresh;
			}
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
			if(value != null) timer.Start();
		}
	}
	public object EnteringGrid { get; set; }
	public void RefreshContent()
	{
		if(SaveData()) 
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
			if(IsSaving)
			{
				return true;
			}
			else
			{
				return _isReadOnly;
			}
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
			if(this.NameLabel != null)
			{
				this.NameLabel.Text = value;
			}
			
			OnTitleNameChanged(EventArgs.Empty);
		}
	}
	private string _statusText = "";
	public string StatusText
	{
		get => _statusText;
		set
		{
			_statusText = value;
			OnStatusTextChanged(EventArgs.Empty);
		}
	}
	public event EventHandler StatusTextChanged;
	void OnStatusTextChanged(EventArgs e)
	{
		StatusTextChanged?.Invoke(this, e);
	}
	public event EventHandler DirtyChanged;
	public event EventHandler TitleNameChanged;
	public void SaveObject()
	{
		this.EndCurrentEdit();
		if(HasErrors(true)) return;
		FormGenerator.SaveData();
	}
	private bool HasErrors(bool throwException)
	{
		if(FormGenerator.DataSet.HasErrors)
		{
			if(throwException) throw new Exception(ResourceUtils.GetString("ErrorsInForm", Environment.NewLine + Environment.NewLine + DatasetTools.GetDatasetErrors(FormGenerator.DataSet)));
			return true;
		}
		else
		{
			return false;
		}
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
		if(objectToLoad is FormControlSet)
		{
			form = objectToLoad as FormControlSet;
		}
		else if(objectToLoad is FormReferenceMenuItem)
		{
			FormReferenceMenuItem formRef = objectToLoad as FormReferenceMenuItem;
			form = formRef.Screen;
			defaultSetId = formRef.DefaultSetId;
			sortSetId = formRef.SortSetId;
			
			if(SingleRecordEditing)
			{
				methodId = formRef.RecordEditMethodId;
			}
			else
			{
				methodId = formRef.MethodId;
				// Set listDataStructure only if we don't edit single record. If we do, we load everything at once.
				listDataStructureId = formRef.ListDataStructureId;
				listMethodId = formRef.ListMethodId;
				if(formRef.ListEntity != null)
				{
					listDataMember = formRef.ListEntity.Name;
				}
			}
			this.FormGenerator.TemplateSet = formRef.TemplateSet;
			this.FormGenerator.DefaultTemplate = formRef.DefaultTemplate;
			this.FormGenerator.RuleSet = formRef.RuleSet;
			
			_isReadOnly = FormTools.IsFormMenuReadOnly(formRef);
		}
		else
		{
			throw new ArgumentOutOfRangeException("objectToLoad", objectToLoad, ResourceUtils.GetString("UknownObject"));
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
		ToolStripsLoaded?.Invoke(null, EventArgs.Empty);
	}
	
	private void LoadFormAsync()
	{
        this.FormGenerator.LoadFormAsync();
		this.EndCurrentEdit();
		if(SingleRecordEditing)
		{
			object[] key = new object[FormGenerator.SelectionParameters.Count];
			int i = 0;
			foreach(object val in FormGenerator.SelectionParameters.Values)
			{
				key[i] = val;
				i++;
			}
			SetPosition(key);
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
			if(this.IsViewOnly)
			{
				return false;
			}
			else
			{
				return _isDirty;
			}
		}
		set
		{
			if(!this.IsViewOnly)
			{
				_isDirty=value;
				OnDirtyChanged(EventArgs.Empty);
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
        this.timer = new Timer(this.components);
        //
		// timer
		// 
		this.timer.Interval = 300;
		this.timer.Tick += this.timer_Tick;
        // 
		// AsForm
		// 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.AutoScroll = true;
		this.BackColor = System.Drawing.Color.FloralWhite;
		this.ClientSize = new System.Drawing.Size(320, 285);
		this.DockAreas = DockAreas.Document;
		this.Name = "AsForm";
		this.Closing += this.AsForm_Closing;
	}
	protected virtual void OnDirtyChanged(EventArgs e)
	{
		OnTitleNameChanged(EventArgs.Empty);
		DirtyChanged?.Invoke(this, e);
	}
	protected virtual void OnTitleNameChanged(EventArgs e)
	{	
		this.Text = (IsDirty ? "*" : "") + this.TitleName;
		//this.TabText = this.TitleName;
		TitleNameChanged?.Invoke(this, e);
	}
	public string Test()
	{	
		Control focused = FindFocused(this);
		if(focused == null)
			return "No focused control found";
		else
			return "Type: " + focused.GetType() + Environment.NewLine
				+ "Name: " + focused.Name + Environment.NewLine
				+ "TabStop: " + focused.TabStop + Environment.NewLine
				+ "TabIndex: " + focused.TabIndex;
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
			AddPanels(this, list);
			return list.ToArray();
		}
	}
	private void AddPanels(Control parentControl, List<AsPanel> list)
	{
		foreach(Control control in parentControl.Controls)
		{
			if(control is AsPanel panel)
			{
				list.Add(panel);
			}
			
			AddPanels(control, list);
		}
	}
	public AsPanel FindPanel(string dataMember)
	{
		foreach(AsPanel panel in this.Panels)
		{
			if(panel.DataMember == dataMember) return panel;
		}
		return null;
	}
	private Control FindFocused(Control parent)
	{
		if(parent.Focused)
			return parent;
		foreach(Control control in parent.Controls)
		{
			Control focusedControl = FindFocused(control);
			
			if(focusedControl != null)
				return FindFocused(control);
		}
		return null;
	}
	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if(keyData == (Keys.F6 | Keys.Shift))
		{
			this.SelectNextControl(this.ActiveControl, false, true, true, true);
			return true;
		}
		if(keyData == Keys.F6)
		{
			bool result = this.SelectNextControl(this.ActiveControl, true, true, true, true);
			return true;
		}
		return base.ProcessCmdKey (ref msg, keyData);
	}
	protected override void Dispose(bool disposing)
	{
		if(disposing)
		{
			if(_currentHandledPanel != null)
			{
				_currentHandledPanel.RecordIdChanged -= panel_RecordIdChanged;
				_currentHandledPanel = null;
			}
			if(this.FormGenerator != null)
			{
				this.FormGenerator.Dispose();
				this.FormGenerator = null;
			}
		}
		base.Dispose (disposing);
	}
	private bool SaveData()
	{
		if(FormGenerator == null) return false;
		try
		{
			this.EndCurrentEdit();
		}
		catch(Exception ex)
		{
			AsMessageBox.ShowError(this, ex.Message, ResourceUtils.GetString("ErrorWhenSaving", this.TitleName), ex);
			return false;
		}
		if(! SaveOnClose)
		{
			try
			{
				HasErrors(true);
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this, ex.Message, ResourceUtils.GetString("ErrorWhenSaving", this.TitleName), ex);
				return false;
			}
		
			return true;
		}
		if(IsDirty)
		{
			DialogResult result = MessageBox.Show(this,
				ResourceUtils.GetString("SaveChanges", this.TitleName), 
				ResourceUtils.GetString("SaveTitle"), 
				MessageBoxButtons.YesNoCancel, 
				MessageBoxIcon.Question);
		
			switch(result)
			{
				case DialogResult.Yes:
					try
					{
						SaveObject();
					}
					catch(Exception ex)
					{
						AsMessageBox.ShowError(this, ex.Message, ResourceUtils.GetString("ErrorWhenSaving", this.TitleName), ex);
						return false;
					}
					return true;
				case DialogResult.No:
					return true;
		
				case DialogResult.Cancel:
					return false;
			}
		}
		return true;
	}
	private void AsForm_Closing(object sender, CancelEventArgs e)
	{
		if(this.DialogResult != DialogResult.Cancel)
		{
			if(! SaveData())
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
		if(id is Guid)
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
		if(cm == null) return false;
		return (bool)Reflector.GetValue(typeof(CurrencyManager), cm, "IsBinding");
	}
	private void timer_Tick(object sender, EventArgs e)
	{
		if(this.AutoAddNewEntity != null)
		{
			BindingManagerBase b = this.Controls[0].BindingContext[this.FormGenerator.DataSet, this.AutoAddNewEntity];
			if(b.Count == 0)
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
		foreach(Control child in this.Controls)
		{
			if(child.Enabled)
			{
				_disabledControls.Add(child);
				child.Enabled = false;
			}
		}
	}
	public void EndDisable()
	{
		if(_disabledControls == null) throw new InvalidOperationException(ResourceUtils.GetString("ErrorEndDisable"));
		foreach(Control control in _disabledControls)
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
		foreach(AsPanel panel in this.Panels)
		{
			if(panel.DataMember.IndexOf(".") == -1)
			{
				panel.SetPosition(key);
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
			System.Drawing.Font font = new System.Drawing.Font(this.Font.FontFamily, 10, System.Drawing.FontStyle.Bold);
			float stringWidth = g.MeasureString(text, font).Width;
			float stringHeight = g.MeasureString(text, font).Height;
			g.Clear(this.BackColor);
			g.DrawString(text, font, new System.Drawing.SolidBrush(OrigamColorScheme.FormLoadingStatusColor), this.Width / 2 - (stringWidth / 2), this.Height / 3 + 64);
		}
		catch{}
	}
	protected void InvokeToolStripsRemoved()
	{
		AllToolStripsRemoved(this,EventArgs.Empty);
	}
}
