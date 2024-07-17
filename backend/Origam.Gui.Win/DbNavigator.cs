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
using System.Windows.Forms;

using Origam.DA;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Gui.Win;
/// <summary>
/// 
/// </summary>
public class DbNavigator : System.Windows.Forms.Panel
{
	public event EventHandler NewRecordAdded;
	private object _dataSource;
	private string _dataMember;
	private System.Windows.Forms.ToolBarButton _moveFirstButton;
	private System.Windows.Forms.ToolBarButton _movePreviousButton;
	private System.Windows.Forms.ToolBarButton _newButton;
	private System.Windows.Forms.ToolBarButton _deleteButton;
	private System.Windows.Forms.ToolBarButton _moveNextButton;
	private System.Windows.Forms.ToolBarButton _moveLastButton;
	private System.Windows.Forms.Label _recordLabel;
	private System.Windows.Forms.ImageList _navigatorImageList;
	private System.Windows.Forms.ToolBar _navigationToolbar;
	private System.Windows.Forms.ToolBar _addDeleteToolbar;
	private System.Windows.Forms.Timer timer1;
	private System.Windows.Forms.ContextMenu templateMenu;
	private System.ComponentModel.IContainer components;
	private static ImageListStreamer ImgListStreamer = null;
	/// <summary>
	/// Initializes a new instance of the <see cref="DbNavigator"/> class.
	/// </summary>
	public DbNavigator()
	{
		// This call is required by the Windows.Forms Form Designer.
		InitializeComponent();
		if(ImgListStreamer == null)
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(DbNavigator));
			ImgListStreamer = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_navigatorImageList.ImageStream")));
		}
		this._navigatorImageList.ImageStream = ImgListStreamer;
	}
	private void OnNewRecordAdded()
	{
		if(NewRecordAdded != null)
		{
			NewRecordAdded(this, EventArgs.Empty);
		}
	}
	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
		if( disposing )
		{
			BindingManagerBase bindMan = GetBindingManager();
			if(bindMan != null)
			{
				bindMan.PositionChanged -= new EventHandler(OnBindigContextPositionChanged);
			}
			_dataSource = null;
			
			if(components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose( disposing );
	}
	#region Component Designer generated code
	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DbNavigator));
        this._navigationToolbar = new System.Windows.Forms.ToolBar();
        this._moveFirstButton = new System.Windows.Forms.ToolBarButton();
        this._movePreviousButton = new System.Windows.Forms.ToolBarButton();
        this._moveNextButton = new System.Windows.Forms.ToolBarButton();
        this._moveLastButton = new System.Windows.Forms.ToolBarButton();
        this._navigatorImageList = new System.Windows.Forms.ImageList(this.components);
        this._newButton = new System.Windows.Forms.ToolBarButton();
        this.templateMenu = new System.Windows.Forms.ContextMenu();
        this._deleteButton = new System.Windows.Forms.ToolBarButton();
        this._addDeleteToolbar = new System.Windows.Forms.ToolBar();
        this._recordLabel = new System.Windows.Forms.Label();
        this.timer1 = new System.Windows.Forms.Timer(this.components);
        this.SuspendLayout();
        // 
        // _navigationToolbar
        // 
        this._navigationToolbar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
        this._navigationToolbar.AutoSize = false;
        this._navigationToolbar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
        this._moveFirstButton,
        this._movePreviousButton,
        this._moveNextButton,
        this._moveLastButton});
        this._navigationToolbar.ButtonSize = new System.Drawing.Size(10, 10);
        this._navigationToolbar.Divider = false;
        this._navigationToolbar.Dock = System.Windows.Forms.DockStyle.Right;
        this._navigationToolbar.DropDownArrows = true;
        this._navigationToolbar.ImageList = this._navigatorImageList;
        this._navigationToolbar.Location = new System.Drawing.Point(0, 0);
        this._navigationToolbar.Name = "_navigationToolbar";
        this._navigationToolbar.ShowToolTips = true;
        this._navigationToolbar.Size = new System.Drawing.Size(96, 24);
        this._navigationToolbar.TabIndex = 0;
        this._navigationToolbar.Wrappable = false;
        this._navigationToolbar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.OnButtonClick);
        // 
        // _moveFirstButton
        // 
        this._moveFirstButton.ImageIndex = 0;
        this._moveFirstButton.Name = "_moveFirstButton";
        this._moveFirstButton.ToolTipText = "Přejít na první záznam";
        // 
        // _movePreviousButton
        // 
        this._movePreviousButton.ImageIndex = 1;
        this._movePreviousButton.Name = "_movePreviousButton";
        this._movePreviousButton.ToolTipText = "Přejít na předchozí záznam";
        // 
        // _moveNextButton
        // 
        this._moveNextButton.ImageIndex = 4;
        this._moveNextButton.Name = "_moveNextButton";
        this._moveNextButton.ToolTipText = "Přejít na další záznam";
        // 
        // _moveLastButton
        // 
        this._moveLastButton.ImageIndex = 5;
        this._moveLastButton.Name = "_moveLastButton";
        this._moveLastButton.ToolTipText = "Přejít na poslední záznam";
        // 
        // _navigatorImageList
        // 
        this._navigatorImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("_navigatorImageList.ImageStream")));
        this._navigatorImageList.TransparentColor = System.Drawing.Color.Magenta;
        this._navigatorImageList.Images.SetKeyName(0, "first_white.png");
        this._navigatorImageList.Images.SetKeyName(1, "previous_white.png");
        this._navigatorImageList.Images.SetKeyName(2, "add_white.png");
        this._navigatorImageList.Images.SetKeyName(3, "delete_white.png");
        this._navigatorImageList.Images.SetKeyName(4, "next_white.png");
        this._navigatorImageList.Images.SetKeyName(5, "last_white.png");
        // 
        // _newButton
        // 
        this._newButton.DropDownMenu = this.templateMenu;
        this._newButton.ImageIndex = 2;
        this._newButton.Name = "_newButton";
        this._newButton.ToolTipText = "Přidat záznam (Insert)";
        // 
        // templateMenu
        // 
        this.templateMenu.Popup += new System.EventHandler(this.templateMenu_Popup);
        // 
        // _deleteButton
        // 
        this._deleteButton.ImageIndex = 3;
        this._deleteButton.Name = "_deleteButton";
        this._deleteButton.ToolTipText = "Vymazat aktuální záznam (Ctrl+Delete)";
        // 
        // _addDeleteToolbar
        // 
        this._addDeleteToolbar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
        this._addDeleteToolbar.AutoSize = false;
        this._addDeleteToolbar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
        this._newButton,
        this._deleteButton});
        this._addDeleteToolbar.Divider = false;
        this._addDeleteToolbar.Dock = System.Windows.Forms.DockStyle.Left;
        this._addDeleteToolbar.DropDownArrows = true;
        this._addDeleteToolbar.ImageList = this._navigatorImageList;
        this._addDeleteToolbar.Location = new System.Drawing.Point(248, 0);
        this._addDeleteToolbar.Name = "_addDeleteToolbar";
        this._addDeleteToolbar.ShowToolTips = true;
        this._addDeleteToolbar.Size = new System.Drawing.Size(64, 24);
        this._addDeleteToolbar.TabIndex = 1;
        this._addDeleteToolbar.Wrappable = false;
        this._addDeleteToolbar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.OnButtonClick);
        // 
        // _recordLabel
        // 
        this._recordLabel.BackColor = System.Drawing.Color.Transparent;
        this._recordLabel.Dock = System.Windows.Forms.DockStyle.Fill;
        this._recordLabel.Location = new System.Drawing.Point(96, 0);
        this._recordLabel.Name = "_recordLabel";
        this._recordLabel.Size = new System.Drawing.Size(216, 24);
        this._recordLabel.TabIndex = 2;
        this._recordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // timer1
        // 
        this.timer1.Enabled = true;
        this.timer1.Interval = 200;
        this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
        // 
        // DbNavigator
        // 
        this.BackColor = System.Drawing.Color.Transparent;
        this.Controls.Add(this._recordLabel);
        this.Controls.Add(this._addDeleteToolbar);
        this.Controls.Add(this._navigationToolbar);
        this.Size = new System.Drawing.Size(312, 24);
        this.ResumeLayout(false);
	}
	#endregion
	/// <summary>
	/// Gets or sets a value indicating whether the New button 
	/// will be shown.
	/// </summary>
	/// <value>true if the New button is visible. The default is true.</value>
	/// 
	[DefaultValue(true), Category("Behavior"), 
	Description("Indicates whether New Button will be shown.")]
	public bool ShowNewButton
	{
		get 
		{
			if(_addDeleteToolbar.Buttons.Count > 0)
			{
				return _addDeleteToolbar.Buttons[0].Visible; 
			}
			else
			{
				return false;
			}
		}
		set 
		{ 
			if(_addDeleteToolbar.Buttons.Count > 0)
			{
				_addDeleteToolbar.Buttons[0].Visible = value; 
			}
		}
	}
	
	
	/// <summary>
	/// Gets or sets a value indicating whether the New button 
	/// will be shown.
	/// </summary>
	/// <value>true if the New button is visible. The default is true.</value>
	/// 
	[DefaultValue(true), Category("Behavior"), 
	Description("Indicates whether New Button will be shown.")]
	public bool ShowDeleteButton
	{
		get 
		{
			if(_addDeleteToolbar.Buttons.Count > 1)
			{
				return _addDeleteToolbar.Buttons[1].Visible; 
			}
			else
			{
				return false;
			}
		}
		set 
		{ 
			if(_addDeleteToolbar.Buttons.Count > 1)
			{
				_addDeleteToolbar.Buttons[1].Visible = value; 
			}
		}
	}
		
	
	
	
	/// <summary>
	/// Gets or sets a value indicating whether the navigator 
	/// displays a ToolTip for each button.
	/// </summary>
	/// <value>true if the navigator displays a ToolTip for each button;
	/// otherwise, false. The default is true.</value>
	/// 
	[DefaultValue(true), Category("Behavior"), 
		Description("Indicates whether tool tips will be shown.")]
	public bool ShowToolTips
	{
		get { return _navigationToolbar.ShowToolTips; }
		set { _navigationToolbar.ShowToolTips = _addDeleteToolbar.ShowToolTips = value; }
	}
	/// <summary>
	/// Indicates whether the New, Delete, OK, and Cancel butons are visible.
	/// </summary>
	/// <value>true if the New, Delete, OK, and Cancel butons are visible;
	/// otherwise, false. The default is true.</value>
//////		[DefaultValue(true), Category("Behavior"), 
//////			Description("Indicates whether the New, Delete, OK, and Cancel butons are visible.")]
//////		public bool ShowEditButtons
//////		{
//////			get	{ return _newButton.Visible; }
//////			set
//////			{
//////				_newButton.Visible = value;
//////				_deleteButton.Visible = value;
////////				_okButton.Visible = value;
////////				_cancelButton.Visible = value;
//////
//////				int toolBarWidth = value ? 96 : 48;
//////				_leftToolBar.Width = _rightToolBar.Width = toolBarWidth;
//////			}
//////		}
	/// <summary>
	/// Sets DataSource and DataMember.
	/// </summary>
	/// <param name="dataSource">The data source.</param>
	/// <param name="dataMember">The DataMember string that specifies the
	/// table to bind to within the DataSource object.</param>
	public void SetDataBinding(object dataSource, string dataMember)
	{
		BindingManagerBase bindMan = GetBindingManager();
		if(null != bindMan)
		{
			bindMan.PositionChanged -= new EventHandler(OnBindigContextPositionChanged);
		}
		_dataSource = dataSource;
		_dataMember = (null == dataMember)? "" : dataMember;
		
		bindMan = GetBindingManager();
		
		if(null != bindMan)
		{
			bindMan.PositionChanged += new EventHandler(OnBindigContextPositionChanged);
		}
		UpdateControls();
	}
	private CurrencyManager ParentManager(CurrencyManager currentManager)
	{
		if(currentManager != null && currentManager.GetType().Name == "RelatedCurrencyManager")
		{
            string propertyName = "parentManager";
            return Reflector.GetValue(currentManager.GetType(), currentManager, propertyName) as CurrencyManager;
		}
		else
		{
			return null;
		}
	}
	private bool AddNewRow(Schema.EntityModel.DataStructureTemplate template, DataRow parentRow)
	{
		object[] templatePosition = null;
		FormGenerator fg = (this.FindForm() as AsForm).FormGenerator;
		try
		{
			if(template == null)
			{
				templatePosition = fg.AddTemplateRecord(parentRow, _dataMember, fg.MainFormDataStructureId, fg.DataSet);
			}
			else
			{
				templatePosition = TemplateTools.AddTemplateRecord(parentRow, template, _dataMember, fg.MainFormDataStructureId, fg.DataSet);
			}
		}
		catch(Exception ex)
		{
			AsMessageBox.ShowError(this.FindForm(), ex.Message, "Chyba pøi zpracování šablony", ex);
			return false;
		}
		if(templatePosition == null) return false;
		(this.Parent.Parent as AsPanel).SetPosition(templatePosition);
		OnNewRecordAdded();
		return true;
	}
	public void AddNewRow()
	{
		if(this.ShowNewButton)
		{
			UpdateControls();
			
			if(_newButton.Enabled)
			{
				CurrencyManager bindMan = GetBindingManager();
				if(bindMan == null) return;
				// turn off sorting on new record
				((AsPanel)this.Parent.Parent).CurrentSort.Clear();
				DataRowView parentRowView = this.ParentRowView;
				DataRow parentRow = parentRowView == null ? null : parentRowView.Row;
				// try to add a row by using a template
				if(AddNewRow(null, parentRow)) return;
				try
				{
					(this.FindForm() as AsForm).AddingDataMember = _dataMember;
					
					bindMan.AddNew();
				}
				catch(Exception ex)
				{
					Origam.UI.AsMessageBox.ShowError(this.FindForm(), ex.Message, ResourceUtils.GetString("ErrorNewRow", (this.Parent.Parent as AsPanel).PanelTitle), ex);
				}
				finally
				{
					(this.FindForm() as AsForm).AddingDataMember = "";
				}
				OnNewRecordAdded();
			}
		}
	}
	private DataRowView ParentRowView
	{
		get
		{
			BindingManagerBase bindMan = GetBindingManager();
			CurrencyManager parentManager = this.ParentManager(bindMan as CurrencyManager);
			if(parentManager != null)
			{
				if(parentManager.Current is DataRowView)
				{
					return parentManager.Current as DataRowView;
				}
			}
			return null;
		}
	}
    public void DeleteRow()
    {
        if (this.ShowDeleteButton)
        {
            CurrencyManager bindMan = GetBindingManager();
            if (bindMan.Count < 1) return;
            if (MessageBox.Show(this.FindForm(), ResourceHelper.GetString("Data.DeleteQuestionText"), ResourceHelper.GetString("Data.DeleteQuestionTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                DataRowView row = bindMan.Current as DataRowView;
                try
                {
                    // .NET BUGFIX: Dataset does not refresh aggregated calculated columns on delete, we have to raise change event
                    if (Origam.DA.DatasetTools.IsRowAggregated(row.Row))
                    {
                        row.BeginEdit();
                        foreach (DataColumn col in row.Row.Table.Columns)
                        {
                            if (col.ReadOnly == false & (col.DataType == typeof(int) | col.DataType == typeof(float) | col.DataType == typeof(decimal) | col.DataType == typeof(long)))
                            {
                                object zero = Convert.ChangeType(0, col.DataType);
                                if (!row.Row[col].Equals(zero)) row.Row[col] = 0;
                            }
                        }
                        row.EndEdit();
                    }
                }
                catch
                {
                }
                finally
                {
                    var parentRows = new List<DataRow>();
                    foreach (DataRelation relation in row.Row.Table.ParentRelations)
                    {
                        parentRows.AddRange(row.Row.GetParentRows(relation, DataRowVersion.Default));
                    }
                    AsForm frm = this.FindForm() as AsForm;
                    frm.IsFiltering = true;
                    try
                    {
                        row.Delete();
                    }
                    finally
                    {
                        frm.IsFiltering = false;
                    }
                    // we check parent rows for errors
                    foreach (DataRow parentRow in parentRows)
                    {
                        DatasetTools.CheckRowErrorRecursive(DatasetTools.RootRow(parentRow), null, false);
                    }
                    (FindForm() as AsForm).FormGenerator.table_RowDeleted(parentRows.ToArray(), row.Row);
                    //AsPanel panel = this.Parent.Parent as AsPanel;
                    //panel.BindGrid(true);
                }
            }
        }
    }
	/// <summary>
	/// Returns the current binding manager.
	/// </summary>
	/// <returns>A reference to the binding manager.</returns>
	private CurrencyManager GetBindingManager()
	{
		if(null != _dataSource && null != BindingContext)
			return BindingContext[_dataSource, _dataMember] as CurrencyManager;
		return null;
	}
	private void OnBindigContextPositionChanged(object sender, System.EventArgs e)
	{
		UpdateControls();
	}
	private void OnButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
	{
		BindingManagerBase bindMan = GetBindingManager();
		if(bindMan==null){return;}
		AsPanel panel = this.Parent.Parent as AsPanel;
		object[] key = panel.CurrentKey;
		int count = bindMan.Count;
		this.Parent.Parent.Focus();
		this._navigationToolbar.Focus();
		Application.DoEvents();
		DataRowView parentRowView = this.ParentRowView;
		DataRow parentRow = parentRowView == null ? null : parentRowView.Row;
		if(parentRowView != null && parentRowView.IsEdit) 
		{
			parentRowView.EndEdit();
		}
		panel.SetPosition(key);
		// after loosing focus, the grid could change number of rows (e.g. asking user if he wants to cancel the edit)
		// so we check the number of rows and exit if it has changed
		if(count != bindMan.Count) 
		{
			UpdateControls();
			return;
		}
//			if(_cancelButton == e.Button)
//				bindMan.CancelCurrentEdit();
//			else if(_deleteButton == e.Button)
        this.Focus();
		if(_deleteButton == e.Button)
		{
			DeleteRow();
		}
		else
		{
			EndEdit();
			try
			{
				if(_newButton == e.Button)
				{
					this.AddNewRow();
				}
				else if(_moveFirstButton == e.Button)
				{
					bindMan.Position = 0;
				}
				else if(_movePreviousButton == e.Button)
				{
					if(bindMan.Count > 0 && bindMan.Position !=0)
						bindMan.Position--;
				}
				else if(_moveNextButton == e.Button)
				{
					if(bindMan.Count > 0 && bindMan.Position != bindMan.Count - 1)
						bindMan.Position++;
				}
				else if(_moveLastButton == e.Button)
				{
					if(bindMan.Count > 0)
						bindMan.Position = bindMan.Count - 1;
				}
			}
			catch
			{
				
			}
		}
		UpdateControls();
		
		panel.FocusControls();
	}
	private bool IsManagerBinding(CurrencyManager cm)
	{
		if(cm == null) return false;
		return (bool)Reflector.GetValue(typeof(CurrencyManager), cm, "IsBinding");
	}
	public void UpdateControls()
	{
		CurrencyManager bindMan = GetBindingManager();
		bool isBinding = IsManagerBinding(bindMan as CurrencyManager);
		if(bindMan == null || bindMan.Position < 0 || isBinding == false)
		{
			_moveFirstButton.Enabled = _movePreviousButton.Enabled =
				_moveLastButton.Enabled = _moveNextButton.Enabled = 
				_deleteButton.Enabled = false; //_endEdit.Enabled =false;
//					_cancelButton.Enabled = _okButton.Enabled = false;
			
			// enable new button only if parent's manager is not suspended
			bool enableNew = true;
            CurrencyManager parentManager = this.ParentManager(bindMan);
            if (parentManager != null)
			{
                if (!IsManagerBinding(parentManager))
				{
					// if parent binding is suspended, we will suppress new button
					enableNew = false;
				}
			}
			_newButton.Enabled = enableNew;
			
			_recordLabel.Text = @"0 / 0";
		}
		else
		{
			_newButton.Enabled = true;
			_moveFirstButton.Enabled = _movePreviousButton.Enabled = 0 < bindMan.Position;
			_moveNextButton.Enabled = _moveLastButton.Enabled = bindMan.Position < bindMan.Count - 1;
			_deleteButton.Enabled = true;
			_recordLabel.Text = string.Format(@"{0} / {1}", bindMan.Position + 1, bindMan.Count);
			if(this.FindForm() is AsForm)
			{
				FormGenerator fg = (this.FindForm() as AsForm).FormGenerator;
				if(fg != null && fg.TemplateSet != null && fg.TemplateSet.TemplatesByDataMember(_dataMember).Count > 0)
				{
					this._newButton.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
				}
			}
		}
	}
	public bool EndEdit()
	{
		BindingManagerBase bindMan = GetBindingManager();
		if(bindMan==null){return true;}
		try
		{
			if(bindMan.Position >= 0 && bindMan.Current is DataRowView) // && (bindMan.Current as DataRowView).Row.RowState != DataRowState.Unchanged)
			{
				//(bindMan.Current as DataRowView).EndEdit();
				bindMan.EndCurrentEdit();
			}
		}
		catch(Exception ex)
		{
			string msg = ResourceUtils.GetString("UpdateValue", ex.Message);
			if(DialogResult.Yes == MessageBox.Show(this.FindForm(), msg, ResourceUtils.GetString("ErrorSaveTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Error))
			{
				Application.DoEvents();
				this.Parent.Focus();
				return false;
			}
			else
			{
				bindMan.CancelCurrentEdit();
//					if(_moveFirstButton == e.Button)
//						bindMan.Position = 0;
//					else if(_newButton == e.Button)
//					{
//						bindMan.AddNew();
//					}
			}
		}
		return true;
	}
	private void timer1_Tick(object sender, System.EventArgs e)
	{
		UpdateControls();
	}
	private void templateMenu_Popup(object sender, System.EventArgs e)
	{
		templateMenu.MenuItems.Clear();
		FormGenerator fg = (this.FindForm() as AsForm).FormGenerator;
		if(fg.TemplateSet != null)
		{
			foreach(Schema.EntityModel.DataStructureTemplate template in fg.TemplateSet.TemplatesByDataMember(_dataMember))
			{
				IDocumentationService documentation = ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;
				string name = documentation.GetDocumentation(template.Id, DocumentationType.USER_SHORT_HELP);
				if(name == "") name = template.Name;
				TemplateMenuItem templateMenuItem = new TemplateMenuItem(name, template);
				templateMenuItem.Click += new EventHandler(templateMenuItem_Click);
				templateMenu.MenuItems.Add(templateMenuItem);
			}
		}
	}
	private void templateMenuItem_Click(object sender, EventArgs e)
	{
		DataRow parentRow = (this.ParentRowView == null ? null : this.ParentRowView.Row);
		AddNewRow((sender as TemplateMenuItem).Template, parentRow);
	}
}
