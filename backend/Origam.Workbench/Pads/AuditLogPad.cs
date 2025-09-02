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
using System.Windows.Forms;
using System.Data;
using Origam.DA;
using Origam.Workbench.Services;
using Origam.Schema;

namespace Origam.Workbench.Pads;
/// <summary>
/// Summary description for AttachmentPad.
/// </summary>
public class AuditLogPad : AbstractPadContent
{
	private class OtherTextColumn : DataGridTextBoxColumn
	{
		public OtherTextColumn() : base()
		{
			this.TextBox.ReadOnly = true;
		}
	}
	private class FieldNameColumn : DataGridTextBoxColumn
	{
		public FieldNameColumn() : base()
		{
		}
		protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
		{
			Guid columnId = (Guid)base.GetColumnValueAtRow (source, rowNum);
			ISchemaItem item;
			try
			{
				SchemaService schema = ServiceManager.Services
					.GetService<SchemaService>();
				IPersistenceService persistence;
				persistence = ServiceManager.Services
					.GetService<IPersistenceService>();
				item = persistence.SchemaProvider
					.RetrieveInstance<ISchemaItem>(columnId);
			}
			catch
			{
				return columnId;
			}
			if(item == null)
            {
                return columnId;
            }

            if (item is ICaptionSchemaItem captionItem)
			{
				if(!string.IsNullOrEmpty(captionItem.Caption))
				{
					return captionItem.Caption;
				}

                return item.Name;
            }

            return item.Name;
        }
	}
	private class ActionTypeColumn : DataGridTextBoxColumn
	{
		public ActionTypeColumn() : base()
		{
			this.TextBox.ReadOnly = true;
		}
		protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
		{
			switch((int)base.GetColumnValueAtRow (source, rowNum))
			{
				case 4:
					return ResourceUtils.GetString("New");
				case 8:
					return ResourceUtils.GetString("Deleted");
				case 16:
					return ResourceUtils.GetString("Change");
				case 32:
					return ResourceUtils.GetString("Deduplication");
				default:
					return "?";
			}
		}
	}
	private class UserNameColumn : DataGridTextBoxColumn
	{
		public UserNameColumn() : base()
		{
			this.TextBox.ReadOnly = true;
		}
		protected override object GetColumnValueAtRow(CurrencyManager source, int rowNum)
		{
			object o = base.GetColumnValueAtRow(source, rowNum);
			if(o == DBNull.Value)
            {
                return null;
            }

            IOrigamProfileProvider profileProvider = SecurityManager.GetProfileProvider();
			try
			{
				UserProfile profile = (profileProvider as IOrigamProfileProvider).GetProfile((Guid)o) as UserProfile;
				return profile.FullName;
			}
			catch(Exception ex)
			{
				return ex.Message;
			}
		}
		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
			}
			base.Dispose (disposing);
		}
	}
	private System.Windows.Forms.DataGrid dataGrid1;
	private bool _supportLog = true;
	private DataAuditLog _dataset;
	private ActionTypeColumn col0;
	private OtherTextColumn col1;
	private OtherTextColumn col2;
	private OtherTextColumn col3;
	private FieldNameColumn col4;
	private UserNameColumn col5;
	private System.Windows.Forms.DataGridTableStyle dataGridTableStyle1;
	public AuditLogPad()
	{
		InitializeComponent();
	}
	#region Windows Form Designer generated code
	private void InitializeComponent()
	{
		System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AuditLogPad));
		this._dataset = new Origam.DA.DataAuditLog();
		this.dataGrid1 = new System.Windows.Forms.DataGrid();
		this.dataGridTableStyle1 = new System.Windows.Forms.DataGridTableStyle();
		this.col0 = new Origam.Workbench.Pads.AuditLogPad.ActionTypeColumn();
		this.col4 = new Origam.Workbench.Pads.AuditLogPad.FieldNameColumn();
		this.col2 = new Origam.Workbench.Pads.AuditLogPad.OtherTextColumn();
		this.col3 = new Origam.Workbench.Pads.AuditLogPad.OtherTextColumn();
		this.col1 = new Origam.Workbench.Pads.AuditLogPad.OtherTextColumn();
		this.col5 = new UserNameColumn();
		((System.ComponentModel.ISupportInitialize)(this._dataset)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
		this.SuspendLayout();
		// 
		// _dataset
		// 
		this._dataset.DataSetName = "DataAuditLog";
		this._dataset.Locale = new System.Globalization.CultureInfo("cs-CZ");
		// 
		// dataGrid1
		// 
		this.dataGrid1.AlternatingBackColor = System.Drawing.Color.LightGoldenrodYellow;
		this.dataGrid1.BackColor = System.Drawing.Color.White;
		this.dataGrid1.BackgroundColor = System.Drawing.Color.LightGoldenrodYellow;
		this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.dataGrid1.CaptionBackColor = System.Drawing.Color.LightGoldenrodYellow;
		this.dataGrid1.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
		this.dataGrid1.CaptionForeColor = System.Drawing.Color.DarkSlateBlue;
		this.dataGrid1.CaptionVisible = false;
		this.dataGrid1.DataMember = "AuditRecord";
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
		this.dataGrid1.Location = new System.Drawing.Point(0, 0);
		this.dataGrid1.Name = "dataGrid1";
		this.dataGrid1.ParentRowsBackColor = System.Drawing.Color.BurlyWood;
		this.dataGrid1.ParentRowsForeColor = System.Drawing.Color.DarkSlateBlue;
		this.dataGrid1.ParentRowsVisible = false;
		this.dataGrid1.ReadOnly = true;
		this.dataGrid1.SelectionBackColor = System.Drawing.Color.DarkSlateBlue;
		this.dataGrid1.SelectionForeColor = System.Drawing.Color.GhostWhite;
		this.dataGrid1.Size = new System.Drawing.Size(728, 397);
		this.dataGrid1.TabIndex = 0;
		this.dataGrid1.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
																							  this.dataGridTableStyle1});
		this.dataGrid1.VisibleChanged += new System.EventHandler(this.dataGrid1_VisibleChanged);
		// 
		// dataGridTableStyle1
		// 
		this.dataGridTableStyle1.AlternatingBackColor = System.Drawing.Color.LightGoldenrodYellow;
		this.dataGridTableStyle1.BackColor = System.Drawing.Color.White;
		this.dataGridTableStyle1.DataGrid = this.dataGrid1;
		this.dataGridTableStyle1.ForeColor = System.Drawing.Color.DarkSlateBlue;
		this.dataGridTableStyle1.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
																											  this.col0,
																											  this.col4,
																											  this.col2,
																											  this.col3,
																											  this.col1,
																											  this.col5});
		this.dataGridTableStyle1.GridLineColor = System.Drawing.Color.Peru;
		this.dataGridTableStyle1.HeaderBackColor = System.Drawing.Color.Maroon;
		this.dataGridTableStyle1.HeaderForeColor = System.Drawing.Color.LightGoldenrodYellow;
		this.dataGridTableStyle1.LinkColor = System.Drawing.Color.Maroon;
		this.dataGridTableStyle1.MappingName = "AuditRecord";
		this.dataGridTableStyle1.SelectionBackColor = System.Drawing.Color.DarkSlateBlue;
		this.dataGridTableStyle1.SelectionForeColor = System.Drawing.Color.GhostWhite;
		// 
		// col0
		// 
		this.col0.Format = "";
		this.col0.FormatInfo = null;
		this.col0.HeaderText = ResourceUtils.GetString("TypeTitle");
		this.col0.MappingName = "ActionType";
		this.col0.Width = 50;
		// 
		// col4
		// 
		this.col4.Format = "";
		this.col4.FormatInfo = null;
		this.col4.HeaderText = ResourceUtils.GetString("FieldTitle");
		this.col4.MappingName = "refColumnId";
		this.col4.Width = 75;
		// 
		// col2
		// 
		this.col2.Format = "";
		this.col2.FormatInfo = null;
		this.col2.HeaderText = ResourceUtils.GetString("OldValueTitle");
		this.col2.MappingName = "OldValue";
		this.col2.NullText = ResourceUtils.GetString("Empty");
		this.col2.Width = 200;
		// 
		// col3
		// 
		this.col3.Format = "";
		this.col3.FormatInfo = null;
		this.col3.HeaderText = ResourceUtils.GetString("NewValueTitle");
		this.col3.MappingName = "NewValue";
		this.col3.NullText = ResourceUtils.GetString("Empty");
		this.col3.Width = 200;
		// 
		// col1
		// 
		this.col1.Format = "";
		this.col1.FormatInfo = null;
		this.col1.HeaderText = ResourceUtils.GetString("DateTitle");
		this.col1.MappingName = "RecordCreated";
		this.col1.Width = 75;
		// 
		// col5
		// 
		this.col5.Format = "";
		this.col5.FormatInfo = null;
		this.col5.HeaderText = ResourceUtils.GetString("UserTitle");
		this.col5.MappingName = "RecordCreatedBy";
		this.col5.Width = 200;
		// 
		// AuditLogPad
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.BackColor = System.Drawing.Color.LightGoldenrodYellow;
		this.ClientSize = new System.Drawing.Size(728, 397);
		this.Controls.Add(this.dataGrid1);
		this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft) 
			| WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight) 
			| WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop) 
			| WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
		this.HideOnClose = true;
		this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
		this.Name = "AuditLogPad";
		this.TabText = ResourceUtils.GetString("AuditTitle");
		this.Text = ResourceUtils.GetString("AuditTitle");
		((System.ComponentModel.ISupportInitialize)(this._dataset)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
		this.ResumeLayout(false);
	}
	#endregion
	#region Private Methods
	private void RetrieveLog(Guid entityId, Guid recordId)
	{
		try
		{
			DataSet result = AuditLogDA.RetrieveLog(entityId, recordId);
			_dataset.Merge(result);
		}
		catch
		{
			// Log has problem - probably database does not contain correct table
			// so we don't support log
			_supportLog = false;
			return;
		}
	}
	#endregion
	#region Private Properties
	private DataRow CurrentRow
	{
		get
		{
			try
			{
				return _dataset.AuditRecord.DefaultView[dataGrid1.CurrentRowIndex].Row;
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
	#endregion
	#region Public Methods
	public void GetAuditLog(Guid mainEntityId, Guid mainRecordId)
	{
		this.ParentId = mainRecordId;
		this.ParentEntityId = mainEntityId;
		GetLog();
	}
	public void GetLog()
	{
		if(!_supportLog)
        {
            return;
        }

        try
		{
			this.dataGrid1.DataSource = null;
			RetrieveLog(this.ParentEntityId, this.ParentId);
			_dataset.AuditRecord.DefaultView.AllowNew = false;
			_dataset.AuditRecord.DefaultView.AllowDelete = false;
			_dataset.AuditRecord.DefaultView.Sort = "RecordCreated DESC";
			this.dataGrid1.DataSource = _dataset.AuditRecord.DefaultView;
		}
		catch
		{
		}
	}
	public void ClearList()
	{
		if(_dataset != null)
		{
			_dataset.Clear();
		}
	}
	#endregion
	private void dataGrid1_VisibleChanged(object sender, System.EventArgs e)
	{
		this.PerformLayout();
	}
	protected override void Dispose(bool disposing)
	{
		if(disposing)
		{
			if(_dataset != null)
			{
				_dataset.Dispose();
				_dataset = null;
			}
		}
		base.Dispose (disposing);
	}
}
