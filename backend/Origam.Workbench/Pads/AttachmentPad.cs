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
using System.Windows.Forms;
using System.Data;

using Origam.UI;
using Origam.DA;
using Origam.Workbench.Services;

namespace Origam.Workbench.Pads;
/// <summary>
/// Summary description for AttachmentPad.
/// </summary>
public class AttachmentPad : AbstractPadContent
{
	private class OtherTextColumn : DataGridTextBoxColumn
	{
		public OtherTextColumn() : base()
		{
			this.TextBox.ReadOnly = true;
			this.TextBox.ReadOnlyChanged += new EventHandler(TextBox_ReadOnlyChanged);
		}
		private void TextBox_ReadOnlyChanged(object sender, EventArgs e)
		{
			this.TextBox.ReadOnly = true;
		}
	}
	private const string FILE_NAME_COLUMN = "FileName";
	private const string ID_COLUMN = "Id";
	private const string PARENT_COLUMN = "refParentRecordId";
	private const string DATA_COLUMN = "Data";
	private const string DATE_COLUMN = "RecordCreated";
	private const string PARENT_ENTITY_COLUMN = "refParentRecordEntityId";
	private System.Windows.Forms.DataGrid dataGrid1;
	private bool _supportAttachments = true;
	public event EventHandler AttachmentsUpdated;
	private CurrencyManager _cm = null;
	private DataStructureQuery _query = null;
	private DataSet _datasetX;
	private System.Windows.Forms.OpenFileDialog _dlgOpen = new OpenFileDialog();
	private System.Windows.Forms.ContextMenu mnuContext;
	private System.Windows.Forms.MenuItem menuItem1;
	private System.Windows.Forms.MenuItem menuItem2;
	private System.Windows.Forms.Panel panel1;
	private System.Windows.Forms.ToolBarButton butLoad;
	private System.Windows.Forms.ToolBarButton butSave;
	private System.Windows.Forms.ToolBarButton butShow;
	private System.Windows.Forms.ToolBarButton toolBarButton4;
	private System.Windows.Forms.ToolBarButton butRefresh;
	private System.Windows.Forms.ImageList imageList1;
	private System.ComponentModel.IContainer components;
	private System.Windows.Forms.ToolBar toolBar;
	private System.Windows.Forms.ToolBarButton butDelete;
	private Origam.Workbench.AttachmentDataset _dataset;
	private System.Windows.Forms.DataGridTableStyle dataGridTableStyle1;
	private OtherTextColumn colDate;
	private System.Windows.Forms.DataGridTextBoxColumn colRemark;
	private OtherTextColumn colName;
	private System.Windows.Forms.SaveFileDialog _dlgSave = new SaveFileDialog();
	public AttachmentPad()
	{
		InitializeComponent();
	}
	#region Windows Form Designer generated code
	private void InitializeComponent()
	{
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AttachmentPad));
        this.dataGrid1 = new System.Windows.Forms.DataGrid();
        this._dataset = new Origam.Workbench.AttachmentDataset();
        this.dataGridTableStyle1 = new System.Windows.Forms.DataGridTableStyle();
        this.colName = new Origam.Workbench.Pads.AttachmentPad.OtherTextColumn();
        this.colDate = new Origam.Workbench.Pads.AttachmentPad.OtherTextColumn();
        this.colRemark = new System.Windows.Forms.DataGridTextBoxColumn();
        this.mnuContext = new System.Windows.Forms.ContextMenu();
        this.menuItem1 = new System.Windows.Forms.MenuItem();
        this.menuItem2 = new System.Windows.Forms.MenuItem();
        this.panel1 = new System.Windows.Forms.Panel();
        this.toolBar = new System.Windows.Forms.ToolBar();
        this.butLoad = new System.Windows.Forms.ToolBarButton();
        this.butSave = new System.Windows.Forms.ToolBarButton();
        this.butShow = new System.Windows.Forms.ToolBarButton();
        this.toolBarButton4 = new System.Windows.Forms.ToolBarButton();
        this.butRefresh = new System.Windows.Forms.ToolBarButton();
        this.butDelete = new System.Windows.Forms.ToolBarButton();
        this.imageList1 = new System.Windows.Forms.ImageList(this.components);
        ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this._dataset)).BeginInit();
        this.panel1.SuspendLayout();
        this.SuspendLayout();
        // 
        // dataGrid1
        // 
        this.dataGrid1.AllowSorting = false;
        this.dataGrid1.AlternatingBackColor = System.Drawing.Color.LightGoldenrodYellow;
        this.dataGrid1.BackColor = System.Drawing.Color.White;
        this.dataGrid1.BackgroundColor = System.Drawing.Color.LightGoldenrodYellow;
        this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.dataGrid1.CaptionBackColor = System.Drawing.Color.LightGoldenrodYellow;
        this.dataGrid1.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
        this.dataGrid1.CaptionForeColor = System.Drawing.Color.DarkSlateBlue;
        this.dataGrid1.CaptionVisible = false;
        this.dataGrid1.DataMember = "Attachment";
        this.dataGrid1.DataSource = this._dataset;
        this.dataGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dataGrid1.FlatMode = true;
        this.dataGrid1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
        this.dataGrid1.ForeColor = System.Drawing.Color.DarkSlateBlue;
        this.dataGrid1.GridLineColor = System.Drawing.Color.Peru;
        this.dataGrid1.GridLineStyle = System.Windows.Forms.DataGridLineStyle.None;
        this.dataGrid1.HeaderBackColor = System.Drawing.Color.Maroon;
        this.dataGrid1.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
        this.dataGrid1.HeaderForeColor = System.Drawing.Color.LightGoldenrodYellow;
        this.dataGrid1.LinkColor = System.Drawing.Color.Maroon;
        this.dataGrid1.Location = new System.Drawing.Point(0, 24);
        this.dataGrid1.Name = "dataGrid1";
        this.dataGrid1.ParentRowsBackColor = System.Drawing.Color.BurlyWood;
        this.dataGrid1.ParentRowsForeColor = System.Drawing.Color.DarkSlateBlue;
        this.dataGrid1.SelectionBackColor = System.Drawing.Color.DarkSlateBlue;
        this.dataGrid1.SelectionForeColor = System.Drawing.Color.GhostWhite;
        this.dataGrid1.Size = new System.Drawing.Size(592, 189);
        this.dataGrid1.TabIndex = 0;
        this.dataGrid1.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
        this.dataGridTableStyle1});
        this.dataGrid1.Visible = false;
        this.dataGrid1.Leave += new System.EventHandler(this.dataGrid1_Leave);
        // 
        // _dataset
        // 
        this._dataset.DataSetName = "AttachmentDataset";
        this._dataset.Locale = new System.Globalization.CultureInfo("cs-CZ");
        this._dataset.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
        // 
        // dataGridTableStyle1
        // 
        this.dataGridTableStyle1.AlternatingBackColor = System.Drawing.Color.LightGoldenrodYellow;
        this.dataGridTableStyle1.BackColor = System.Drawing.Color.White;
        this.dataGridTableStyle1.DataGrid = this.dataGrid1;
        this.dataGridTableStyle1.ForeColor = System.Drawing.Color.DarkSlateBlue;
        this.dataGridTableStyle1.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
        this.colName,
        this.colDate,
        this.colRemark});
        this.dataGridTableStyle1.GridLineColor = System.Drawing.Color.Peru;
        this.dataGridTableStyle1.GridLineStyle = System.Windows.Forms.DataGridLineStyle.None;
        this.dataGridTableStyle1.HeaderBackColor = System.Drawing.Color.Maroon;
        this.dataGridTableStyle1.HeaderForeColor = System.Drawing.Color.LightGoldenrodYellow;
        this.dataGridTableStyle1.LinkColor = System.Drawing.Color.Maroon;
        this.dataGridTableStyle1.MappingName = "Attachment";
        this.dataGridTableStyle1.SelectionBackColor = System.Drawing.Color.DarkSlateBlue;
        this.dataGridTableStyle1.SelectionForeColor = System.Drawing.Color.GhostWhite;
        // 
        // colName
        // 
        this.colName.Format = "";
        this.colName.FormatInfo = null;
        this.colName.HeaderText = "File name";
        this.colName.MappingName = "FileName";
        this.colName.NullText = "(empty)";
        this.colName.Width = 75;
        // 
        // colDate
        // 
        this.colDate.Format = "";
        this.colDate.FormatInfo = null;
        this.colDate.HeaderText = "Date";
        this.colDate.MappingName = "RecordCreated";
        this.colDate.NullText = "(empty)";
        this.colDate.Width = 75;
        // 
        // colRemark
        // 
        this.colRemark.Format = "";
        this.colRemark.FormatInfo = null;
        this.colRemark.HeaderText = "Remark";
        this.colRemark.MappingName = "Note";
        this.colRemark.NullText = "(empty)";
        this.colRemark.Width = 200;
        // 
        // mnuContext
        // 
        this.mnuContext.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
        this.menuItem1,
        this.menuItem2});
        // 
        // menuItem1
        // 
        this.menuItem1.Index = 0;
        this.menuItem1.Text = "";
        // 
        // menuItem2
        // 
        this.menuItem2.Index = 1;
        this.menuItem2.Text = "";
        // 
        // panel1
        // 
        this.panel1.BackColor = System.Drawing.Color.Transparent;
        this.panel1.Controls.Add(this.toolBar);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
        this.panel1.Location = new System.Drawing.Point(0, 0);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(592, 24);
        this.panel1.TabIndex = 5;
        // 
        // toolBar
        // 
        this.toolBar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
        this.toolBar.AutoSize = false;
        this.toolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
        this.butLoad,
        this.butSave,
        this.butShow,
        this.toolBarButton4,
        this.butRefresh,
        this.butDelete});
        this.toolBar.ButtonSize = new System.Drawing.Size(17, 17);
        this.toolBar.Divider = false;
        this.toolBar.DropDownArrows = true;
        this.toolBar.ImageList = this.imageList1;
        this.toolBar.Location = new System.Drawing.Point(0, 0);
        this.toolBar.Name = "toolBar";
        this.toolBar.ShowToolTips = true;
        this.toolBar.Size = new System.Drawing.Size(592, 19);
        this.toolBar.TabIndex = 5;
        this.toolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar_ButtonClick);
        // 
        // butLoad
        // 
        this.butLoad.ImageIndex = 0;
        this.butLoad.Name = "butLoad";
        this.butLoad.ToolTipText = "Appends the attachment to the current record";
        // 
        // butSave
        // 
        this.butSave.ImageIndex = 1;
        this.butSave.Name = "butSave";
        this.butSave.ToolTipText = "Saves the attachment to a file";
        // 
        // butShow
        // 
        this.butShow.ImageIndex = 2;
        this.butShow.Name = "butShow";
        this.butShow.ToolTipText = "Displays the attachment in an associated viewer";
        // 
        // toolBarButton4
        // 
        this.toolBarButton4.Name = "toolBarButton4";
        this.toolBarButton4.Style = System.Windows.Forms.ToolBarButtonStyle.Separator;
        // 
        // butRefresh
        // 
        this.butRefresh.ImageIndex = 3;
        this.butRefresh.Name = "butRefresh";
        this.butRefresh.ToolTipText = "Saves changes to the database";
        // 
        // butDelete
        // 
        this.butDelete.ImageIndex = 4;
        this.butDelete.Name = "butDelete";
        this.butDelete.ToolTipText = "Removes the attachment";
        // 
        // imageList1
        // 
        this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
        this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
        this.imageList1.Images.SetKeyName(0, "");
        this.imageList1.Images.SetKeyName(1, "");
        this.imageList1.Images.SetKeyName(2, "");
        this.imageList1.Images.SetKeyName(3, "");
        this.imageList1.Images.SetKeyName(4, "");
        // 
        // AttachmentPad
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.BackColor = System.Drawing.Color.LightGoldenrodYellow;
        this.ClientSize = new System.Drawing.Size(592, 213);
        this.Controls.Add(this.dataGrid1);
        this.Controls.Add(this.panel1);
        this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft) 
        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight) 
        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop) 
        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
        this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        this.HideOnClose = true;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "AttachmentPad";
        this.TabText = "Attachments";
        this.Text = "Attachments";
        this.Load += new System.EventHandler(this.AttachmentPad_Load);
        ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this._dataset)).EndInit();
        this.panel1.ResumeLayout(false);
        this.ResumeLayout(false);
	}
	#endregion
	#region Event Handlers
	private void AttachmentPad_Load(object sender, System.EventArgs e)
	{
		UpdateToolbar();
	}
	
	private void _cm_CurrentChanged(object sender, EventArgs e)
	{
		UpdateToolbar();
		CurrencyManager cm = (sender as CurrencyManager);
		if (cm != null &&  cm.Count > 0 && (cm.Current as DataRowView).IsNew)
		{
			(cm.Current as DataRowView).Row[ID_COLUMN] = Guid.NewGuid();
			(cm.Current as DataRowView).Row[PARENT_COLUMN] = this.ParentId;
			(cm.Current as DataRowView).Row[PARENT_ENTITY_COLUMN] = this.ParentEntityId;
		}
	}
	private void toolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
	{
		(sender as ToolBar).Focus();
		switch( toolBar.Buttons.IndexOf(e.Button) )
		{
			case 0:
				this.AttLoad();
				break;
			case 1:
				this.AttSave();
				break;
			case 2:
				this.AttShow();
				break;
				//case 3 - SEPARATOR
			case 4:
				this.AttUpdate();
				break;
			case 5:	// delete
				this.AttDelete();
				break;
		}
	}
	private void dataGrid1_Leave(object sender, EventArgs e)
	{
		if(_cm != null)
		{
			_cm.EndCurrentEdit();
		}
	}
	#endregion
	#region Private Methods
	private void RetrieveAttachments(Guid entityId, Guid recordId, bool merge)
	{
		if(recordId == Guid.Empty)
		{
			_dataset.Clear();
			return;
		}
		_query = new DataStructureQuery(new Guid("04a07967-4b59-4c14-8320-e6d073f6f77f"), new Guid("b3624c91-526d-4b2b-a282-6d99e62a1eb5"));
		_query.Parameters.Add(new QueryParameter("Attachment_parRefParentRecordId", recordId));
        
#if ORIGAM_CLIENT
#else
		try
		{
#endif
			IServiceAgent dataServiceAgent = GetDataServiceAgent();
			dataServiceAgent.MethodName = "LoadDataByQuery";
			dataServiceAgent.Parameters.Clear();
			dataServiceAgent.Parameters.Add("Query", _query);
			dataServiceAgent.Run();
			DataSet result = dataServiceAgent.Result as DataSet;
			if(merge)
			{
				_dataset.Merge(result);
			}
			else
			{
				_dataset.Clear();
				_dataset.Merge(result);
				_dataset.Attachment.DefaultView.AllowNew = false;
				_dataset.Attachment.DefaultView.AllowDelete = false;
				
				_cm = this.BindingContext[_dataset, _dataset.Attachment.TableName] as CurrencyManager;
				_cm.CurrentChanged -=new EventHandler(_cm_CurrentChanged);
				_cm.CurrentChanged +=new EventHandler(_cm_CurrentChanged);
				
				this.dataGrid1.DataMember = "";
				this.dataGrid1.DataSource = _dataset.Attachment.DefaultView;
			}
#if ORIGAM_CLIENT
#else
		}
		catch
		{
			// Attachments have problem - probably database does not contain correct table
			// so we don't support attachments
			_supportAttachments = false;
			return;
		}
#endif
	}
	private void AttLoad()
	{
		if (_dlgOpen.ShowDialog()==DialogResult.OK)
		{
			DataRow row;
			(dataGrid1.DataSource as DataView).AllowNew = true;
			_cm.AddNew();
			row = (_cm.Current as DataRowView).Row;
			//int FileNameIndex= row.Table.Columns.IndexOf(FILE_NAME_COLUMN);
			row[FILE_NAME_COLUMN] = System.IO.Path.GetFileName(_dlgOpen.FileName);
			row[DATE_COLUMN] = DateTime.Now;
			
			try
			{
				ByteArrayConverter.SaveToDataSet(_dlgOpen.FileName, row, DATA_COLUMN); //sloupec blob ma index 1
				_cm.EndCurrentEdit();
				AttUpdate();
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this, ex.Message, ResourceUtils.GetString("ErrorTitle"), ex);
				_cm.CancelCurrentEdit();
			}
			finally
			{
				(dataGrid1.DataSource as DataView).AllowNew = false;
			}
		}
	}
	private void AttSave()
	{
		DataRow row = CurrentRow;
		if(row == null)
		{
			MessageBox.Show(ResourceUtils.GetString("NoAttachments"));
			return;
		}
		_dlgSave.OverwritePrompt = true;
		_dlgSave.FileName = (string)row[FILE_NAME_COLUMN];
		if (_dlgSave.ShowDialog()==DialogResult.OK)
		{
			try
			{
				ByteArrayConverter.SaveFromDataSet(_dlgSave.FileName, row, DATA_COLUMN, false);
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this, ex.Message, ResourceUtils.GetString("ErrorTitle"), ex);
			}
		}
	}
	private void AttShow()
	{
		DataRow row = CurrentRow;
		if(row == null)
		{
			MessageBox.Show(this, ResourceUtils.GetString("NoAttachments"), ResourceUtils.GetString("ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		 
		string fileName = System.IO.Path.GetTempPath();
		string filePath = System.IO.Path.Combine(fileName, (string)row[FILE_NAME_COLUMN]);
		ByteArrayConverter.SaveFromDataSet(filePath, row, DATA_COLUMN, false);
		try
		{
			System.Diagnostics.Process.Start(filePath);
		}
		catch
		{
			MessageBox.Show(this, ResourceUtils.GetString("ErrorExecuteFile", filePath), ResourceUtils.GetString("ErrorTitle"), MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
		}
	}
	private void AttDelete()
	{
		DataRow row = CurrentRow;
		if(row == null)
		{
			MessageBox.Show(this, "Data neobsahují žádné pøílohy", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
			return;
		}
		if(CanDelete())
		{
			DataView view = dataGrid1.DataSource as DataView;
			
			view.AllowDelete = true;
			row.Delete();
			view.AllowDelete = false;
			AttUpdate();
		}
	}
	private bool CanDelete()
	{
		return MessageBox.Show(this, ResourceHelper.GetString("Attachments.DeleteQuestionText"), ResourceHelper.GetString("Attachments.DeleteQuestionTitle"), MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
	}
	private void AttUpdate()
	{
		IServiceAgent dataServiceAgent = GetDataServiceAgent();
		if(dataServiceAgent == null || _dataset == null)
			return;
		dataServiceAgent.MethodName = "StoreDataByQuery";
		dataServiceAgent.Parameters.Clear();
		dataServiceAgent.Parameters.Add("Query", _query);
		dataServiceAgent.Parameters.Add("Data", _dataset);
		dataServiceAgent.Run();
		OnAttachmentsUpdated();
	}
	private IServiceAgent GetDataServiceAgent()
	{
		IBusinessServicesService services = ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService;
		return services==null?null:services.GetAgent("DataService", null, null);
	}
	#endregion
	#region Private Properties
	private DataRow CurrentRow
	{
		get
		{
			try
			{
				return _dataset.Attachment.DefaultView[dataGrid1.CurrentRowIndex].Row;
			}
			catch
			{
				return null;
			}
		}
	}
	private Guid _parentId;
	private Guid ParentId 
	{
		get
		{
			return _parentId;
		}
		set
		{
			_parentId=value;
		}
	}
	private Guid _parentEntityId;
	private Guid ParentEntityId 
	{
		get
		{
			return _parentEntityId;
		}
		set
		{
			_parentEntityId = value;
		}
	}
	private Hashtable _childReferences;
	private Hashtable ChildReferences
	{
		get
		{
			return _childReferences;
		}
		set
		{
			_childReferences = value;
		}
	}
	#endregion
	#region Public Methods
	public void GetAttachments(Guid mainEntityId, Guid mainRecordId, Hashtable childReferences)
	{
		this.ParentId = mainRecordId;
		this.ParentEntityId = mainEntityId;
		this.ChildReferences = childReferences;
		if( GetDataServiceAgent() !=null ) GetAttachments();
	}
	public void GetAttachments()
	{
		if(!_supportAttachments) return;
//			try
//			{
			RetrieveAttachments(this.ParentEntityId, this.ParentId, false);
			foreach(RecordReference reference in this.ChildReferences.Values)
			{
				RetrieveAttachments(reference.EntityId, reference.RecordId, true);
			}
			UpdateToolbar();
//			}
//			catch
//			{
//			}
	}
	#endregion
	#region Events
	protected virtual void OnAttachmentsUpdated() 
	{
		if (AttachmentsUpdated != null) 
		{
			//Invokes the delegates.
			AttachmentsUpdated(this, EventArgs.Empty); 
		}
	}
	#endregion
	private void UpdateToolbar()
	{
		this.dataGrid1.Visible = (this.ParentId != Guid.Empty);
		this.toolBar.Enabled =  (this.ParentId != Guid.Empty);
	}
	protected override void Dispose(bool disposing)
	{
		if(disposing)
		{
			if(_childReferences != null)
			{
				_childReferences.Clear();
			}
			_cm = null;
			if(_dataset != null)
			{
				_dataset.Dispose();
				_dataset = null;
			}
			if(_datasetX != null)
			{
				_datasetX.Dispose();
				_datasetX = null;
			}
			_query = null;
		}
		base.Dispose (disposing);
	}
}
