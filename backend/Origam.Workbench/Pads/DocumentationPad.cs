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
using System.Data;
using System.Linq;
using System.Windows.Forms;
using MoreLinq;
using Origam.Workbench.Services;

namespace Origam.Workbench.Pads;
/// <summary>
/// Summary description for DocumentationPad.
/// </summary>
public class DocumentationPad : AbstractPadContent
{
	private System.Windows.Forms.DataGrid dataGrid1;
	private System.Windows.Forms.ImageList imageList1;
	private System.Windows.Forms.ToolBar toolBar;
	private System.Windows.Forms.ToolBarButton butRefresh;
	private DocumentationComplete documentationComplete;
	private System.Windows.Forms.DataGridTableStyle dataGridTableStyle1;
	private System.ComponentModel.IContainer components;
	private Services.SchemaService _schemaService;
	private Services.IDocumentationService _documentationService;
	private CurrencyManager _cm;
	private Guid _schemaItemId;
	private Origam.UI.DataGridComboBoxColumn colCategory;
	private System.Windows.Forms.DataGridTextBoxColumn colText;
	private System.Data.DataSet dsCategories;
	private System.Data.DataTable tblCategory;
	private System.Data.DataColumn categoryNameTableColumn;
	public DocumentationPad()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
        _schemaService = ServiceManager.Services.GetService(typeof(Services.SchemaService)) as Services.SchemaService;
        _schemaService.SchemaLoaded += _schemaService_SchemaLoaded;
        _schemaService.SchemaUnloaded += _schemaService_SchemaUnloaded;
        this.colText.TextBox.Multiline = true;
		this.dataGridTableStyle1.PreferredRowHeight *= 2;  
		tblCategory.Rows.Add(new object[] {DBNull.Value});
		Enum.GetValues(typeof(DocumentationType))
			.Cast<DocumentationType>()
			.ForEach(docType=> tblCategory.Rows.Add(new object[] {docType.ToString()}));
		this.colCategory.ColumnComboBox.DataSource = tblCategory.DefaultView;
		this.colCategory.ColumnComboBox.ValueMember = "Name";
		this.colCategory.ColumnComboBox.DisplayMember = "Name";
		_cm = this.BindingContext[documentationComplete, documentationComplete.Documentation.TableName] as CurrencyManager;
		_cm.CurrentChanged += _cm_CurrentChanged;
		this.BackColor = Origam.UI.OrigamColorScheme.FormBackgroundColor;
		this.dataGrid1.BackgroundColor = this.BackColor;
	}
    private void _schemaService_SchemaUnloaded(object sender, EventArgs e)
    {
        documentationComplete.Clear();
        _documentationService = null;
    }
    private void _schemaService_SchemaLoaded(object sender,  bool isInteractive)
    {
        _documentationService = ServiceManager.Services.GetService<IDocumentationService>();
        if (_documentationService == null)
        {
            throw new NullReferenceException("Documentation service not found");
        }
    }
    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose( bool disposing )
	{
		if( disposing )
		{
			_cm = null;
			_documentationService = null;
			_schemaService = null;
			if(components != null)
			{
				components.Dispose();
			}
		}
		base.Dispose( disposing );
	}
	#region Windows Form Designer generated code
	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DocumentationPad));
        this.dataGrid1 = new System.Windows.Forms.DataGrid();
        this.documentationComplete = new Origam.Workbench.Services.DocumentationComplete();
        this.dataGridTableStyle1 = new System.Windows.Forms.DataGridTableStyle();
        this.colCategory = new Origam.UI.DataGridComboBoxColumn();
        this.colText = new System.Windows.Forms.DataGridTextBoxColumn();
        this.tblCategory = new System.Data.DataTable();
        this.categoryNameTableColumn = new System.Data.DataColumn();
        this.imageList1 = new System.Windows.Forms.ImageList(this.components);
        this.toolBar = new System.Windows.Forms.ToolBar();
        this.butRefresh = new System.Windows.Forms.ToolBarButton();
        this.dsCategories = new System.Data.DataSet();
        ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.documentationComplete)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.tblCategory)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.dsCategories)).BeginInit();
        this.SuspendLayout();
        // 
        // dataGrid1
        // 
        this.dataGrid1.AllowSorting = false;
        this.dataGrid1.AlternatingBackColor = System.Drawing.Color.LightGoldenrodYellow;
        this.dataGrid1.BackColor = System.Drawing.Color.White;
        this.dataGrid1.BackgroundColor = System.Drawing.Color.LightGoldenrodYellow;
        this.dataGrid1.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.dataGrid1.CaptionBackColor = System.Drawing.Color.LightGoldenrodYellow;
        this.dataGrid1.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
        this.dataGrid1.CaptionForeColor = System.Drawing.Color.DarkSlateBlue;
        this.dataGrid1.DataMember = "Documentation";
        this.dataGrid1.DataSource = this.documentationComplete;
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
        this.dataGrid1.SelectionBackColor = System.Drawing.Color.DarkSlateBlue;
        this.dataGrid1.SelectionForeColor = System.Drawing.Color.GhostWhite;
        this.dataGrid1.Size = new System.Drawing.Size(600, 333);
        this.dataGrid1.TabIndex = 1;
        this.dataGrid1.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
        this.dataGridTableStyle1});
        this.dataGrid1.TabStop = false;
        this.dataGrid1.SizeChanged += new System.EventHandler(this.dataGrid1_SizeChanged);
        // 
        // documentationComplete
        // 
        this.documentationComplete.DataSetName = "DocumentationComplete";
        this.documentationComplete.Locale = new System.Globalization.CultureInfo("cs-CZ");
        this.documentationComplete.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
        // 
        // dataGridTableStyle1
        // 
        this.dataGridTableStyle1.AllowSorting = false;
        this.dataGridTableStyle1.DataGrid = this.dataGrid1;
        this.dataGridTableStyle1.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
        this.colCategory,
        this.colText});
        this.dataGridTableStyle1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.dataGridTableStyle1.MappingName = "Documentation";
        // 
        // colCategory
        // 
        this.colCategory.Format = "";
        this.colCategory.FormatInfo = null;
        this.colCategory.HeaderText = "Category";
        this.colCategory.MappingName = "Category";
        this.colCategory.Width = 150;
        // 
        // colText
        // 
        this.colText.Format = "";
        this.colText.FormatInfo = null;
        this.colText.HeaderText = "Text";
        this.colText.MappingName = "Data";
        this.colText.Width = 500;
        // 
        // tblCategory
        // 
        this.tblCategory.Columns.AddRange(new System.Data.DataColumn[] {
        this.categoryNameTableColumn});
        this.tblCategory.TableName = "Category";
        // 
        // categoryNameTableColumn
        // 
        this.categoryNameTableColumn.ColumnName = "Name";
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
        // toolBar
        // 
        this.toolBar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
        this.toolBar.AutoSize = false;
        this.toolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
        this.butRefresh});
        this.toolBar.ButtonSize = new System.Drawing.Size(17, 17);
        this.toolBar.Divider = false;
        this.toolBar.DropDownArrows = true;
        this.toolBar.ImageList = this.imageList1;
        this.toolBar.Location = new System.Drawing.Point(0, 0);
        this.toolBar.Name = "toolBar";
        this.toolBar.ShowToolTips = true;
        this.toolBar.Size = new System.Drawing.Size(600, 19);
        this.toolBar.TabIndex = 6;
        this.toolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar_ButtonClick);
        // 
        // butRefresh
        // 
        this.butRefresh.ImageIndex = 3;
        this.butRefresh.Name = "butRefresh";
        this.butRefresh.ToolTipText = "Saves changes to the database";
        // 
        // dsCategories
        // 
        this.dsCategories.DataSetName = "NewDataSet";
        this.dsCategories.Locale = new System.Globalization.CultureInfo("cs-CZ");
        this.dsCategories.Tables.AddRange(new System.Data.DataTable[] {
        this.tblCategory});
        // 
        // DocumentationPad
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.BackColor = System.Drawing.Color.LightGoldenrodYellow;
        this.ClientSize = new System.Drawing.Size(600, 333);
        this.Controls.Add(this.toolBar);
        this.Controls.Add(this.dataGrid1);
        this.DockAreas = ((WeifenLuo.WinFormsUI.Docking.DockAreas)(((((WeifenLuo.WinFormsUI.Docking.DockAreas.Float | WeifenLuo.WinFormsUI.Docking.DockAreas.DockLeft) 
        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockRight) 
        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockTop) 
        | WeifenLuo.WinFormsUI.Docking.DockAreas.DockBottom)));
        this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        this.HideOnClose = true;
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.Name = "DocumentationPad";
        this.TabText = "Documentation";
        this.Text = "Documentation";
        ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.documentationComplete)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.tblCategory)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.dsCategories)).EndInit();
        this.ResumeLayout(false);
	}
	#endregion
	public void Reload()
	{
        if ((_cm.Position == -1) || (_cm.Current == null))
        {
            return;
        }
		DocumentationComplete.DocumentationRow row =
			((DataRowView)_cm.Current).Row as DocumentationComplete.DocumentationRow;
		ShowDocumentation(row.refSchemaItemId);
	}
	private void Save()
	{
		_documentationService.SaveDocumentation(documentationComplete, _schemaItemId);
	}
	public void ClearDocumentation()
	{
		documentationComplete.Clear();
	}
	public void ShowDocumentation(Guid schemaItemId)
	{
		documentationComplete = 
			_documentationService.LoadDocumentation(schemaItemId);
		_schemaItemId = schemaItemId;
		this.dataGrid1.DataSource = this.documentationComplete;
		_cm = this.BindingContext[documentationComplete, documentationComplete.Documentation.TableName] as CurrencyManager;
		_cm.CurrentChanged += _cm_CurrentChanged;
	}
	private void toolBar_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
	{
		(sender as ToolBar).Focus();
		if(e.Button == butRefresh)
		{
			Save();
		}
	}
	private void _cm_CurrentChanged(object sender, EventArgs e)
	{
		CurrencyManager cm = (sender as CurrencyManager);
		if (cm != null &&  cm.Count > 0 && (cm.Current as DataRowView).IsNew)
		{
			DocumentationComplete.DocumentationRow row = (cm.Current as DataRowView).Row as DocumentationComplete.DocumentationRow;
			row.Id = Guid.NewGuid();
			row.refSchemaItemId = _schemaItemId;
		}
	}
	private void dataGrid1_SizeChanged(object sender, System.EventArgs e)
	{
		colText.Width = dataGrid1.Width - colCategory.Width - 50;
	}
	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if(keyData == (Keys.S | Keys.Control))
		{
			Save();
			return true;
		}

        return base.ProcessCmdKey(ref msg, keyData);
    }
}
