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
using System.Text;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using Origam;
using Origam.Workbench.Pads;
using Origam.Workbench;
using Origam.Workbench.Services;
using Origam.DA;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.UI;
using Origam.Workbench.Services.CoreServices;
using Origam.DA.Service;
using static Origam.DA.Common.Enums;
using MoreLinq;

namespace OrigamArchitect;
/// <summary>
/// Summary description for SchemaCompareEditor.
/// </summary>
public class SchemaCompareEditor : AbstractViewContent
{
	private List<SchemaDbCompareResult> _results = new ();
	WorkbenchSchemaService _schema = ServiceManager.Services.GetService(typeof(WorkbenchSchemaService)) as WorkbenchSchemaService;
	private System.Windows.Forms.ColumnHeader colType;
	private System.Windows.Forms.ColumnHeader colName;
	private System.Windows.Forms.ColumnHeader colRemark;
	private System.Windows.Forms.ListView lvwResults;
	private System.Windows.Forms.Panel panel1;
	private System.Windows.Forms.Button btnScript;
	private System.Windows.Forms.Label label1;
	private System.Windows.Forms.Label label2;
	private System.Windows.Forms.GroupBox groupBox1;
	private System.Windows.Forms.GroupBox groupBox2;
	private System.Windows.Forms.Label label3;
	private System.Windows.Forms.ComboBox cboDeploymentVersion;
	private System.Windows.Forms.Button btnAddToDeployment;
	private System.Windows.Forms.ComboBox cboFilter;
	private ContextMenuStrip contextMenu;
	private System.Windows.Forms.Label label4;
	private System.Windows.Forms.Button btnAddToModel;
    private Label label5;
    private ComboBox cboDatabaseType;
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;
	public SchemaCompareEditor()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
		this.Icon = Icon.FromHandle(new Bitmap(Images.DeploymentScriptGenerator).GetHicon());
		cboFilter.Items.AddRange(new object[] {"Missing in Database", "Missing in Model", "Different"});
		cboFilter.SelectedIndex = 0;
		lvwResults.SmallImageList = _schema.SchemaBrowser.ImageList;
		DeploymentVersion currentVersion = null;
		// load versions combo box
		var deploymentVersions = _schema.GetProvider<DeploymentSchemaItemProvider>()
			.ChildItems
			.Cast<DeploymentVersion>()
			.OrderBy(deploymentVersion => deploymentVersion.Version);
		foreach(DeploymentVersion version in deploymentVersions)
		{
			// only version from the current extension
			if(version.Package.PrimaryKey.Equals(_schema.ActiveExtension.PrimaryKey))
			{
				if(version.IsCurrentVersion)
                {
                    currentVersion = version;
                }

                cboDeploymentVersion.Items.Add(version);
			}
		}
		// select active version
		if(currentVersion != null)
		{
			cboDeploymentVersion.SelectedItem = currentVersion;
		}
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        Platform[] platforms = settings.GetAllPlatform();
        cboDatabaseType.Enabled = false;
        platforms?.ForEach(platform =>
        {
                cboDatabaseType.Items.Add(platform);
                cboDatabaseType.Enabled = true;
        });
        cboDatabaseType.SelectedIndex = cboDatabaseType.Items.Count-1;
        contextMenu = new ContextMenuStrip();
        ToolStripMenuItem item = new ToolStripMenuItem(strings.GoToDefinition_MenuItem);
		item.Click += new EventHandler(gotoDefinition_click);
		contextMenu.Items.Add(item);
		lvwResults.ContextMenuStrip = contextMenu;
		this.label3.BackColor = OrigamColorScheme.FormBackgroundColor;
		this.label4.BackColor = OrigamColorScheme.FormBackgroundColor;
		this.btnAddToDeployment.ForeColor = OrigamColorScheme.ButtonForeColor;
		this.btnAddToDeployment.BackColor = OrigamColorScheme.ButtonBackColor;
		this.btnAddToModel.ForeColor = OrigamColorScheme.ButtonForeColor;
		this.btnAddToModel.BackColor = OrigamColorScheme.ButtonBackColor;
		this.btnScript.ForeColor = OrigamColorScheme.ButtonForeColor;
		this.btnScript.BackColor = OrigamColorScheme.ButtonBackColor;
		this.DisplayResults();
	}
	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
		if( disposing )
		{
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SchemaCompareEditor));
        this.colType = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.colName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.colRemark = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
        this.lvwResults = new System.Windows.Forms.ListView();
        this.panel1 = new System.Windows.Forms.Panel();
        this.label3 = new System.Windows.Forms.Label();
        this.groupBox2 = new System.Windows.Forms.GroupBox();
        this.btnScript = new System.Windows.Forms.Button();
        this.groupBox1 = new System.Windows.Forms.GroupBox();
        this.label5 = new System.Windows.Forms.Label();
        this.cboDatabaseType = new System.Windows.Forms.ComboBox();
        this.btnAddToModel = new System.Windows.Forms.Button();
        this.label1 = new System.Windows.Forms.Label();
        this.cboDeploymentVersion = new System.Windows.Forms.ComboBox();
        this.btnAddToDeployment = new System.Windows.Forms.Button();
        this.label2 = new System.Windows.Forms.Label();
        this.cboFilter = new System.Windows.Forms.ComboBox();
        this.label4 = new System.Windows.Forms.Label();
        this.panel1.SuspendLayout();
        this.groupBox2.SuspendLayout();
        this.groupBox1.SuspendLayout();
        this.SuspendLayout();
        // 
        // colType
        // 
        this.colType.Text = global::OrigamArchitect.strings.Type_TableColumn;
        this.colType.Width = 188;
        // 
        // colName
        // 
        this.colName.Text = global::OrigamArchitect.strings.Name_TableColumn;
        this.colName.Width = 259;
        // 
        // colRemark
        // 
        this.colRemark.Text = global::OrigamArchitect.strings.Remark_TableColumn;
        this.colRemark.Width = 369;
        // 
        // lvwResults
        // 
        this.lvwResults.AllowColumnReorder = true;
        this.lvwResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.lvwResults.CheckBoxes = true;
        this.lvwResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
        this.colType,
        this.colName,
        this.colRemark});
        this.lvwResults.Dock = System.Windows.Forms.DockStyle.Fill;
        this.lvwResults.FullRowSelect = true;
        this.lvwResults.GridLines = true;
        this.lvwResults.Location = new System.Drawing.Point(184, 23);
        this.lvwResults.Name = "lvwResults";
        this.lvwResults.Size = new System.Drawing.Size(712, 367);
        this.lvwResults.Sorting = System.Windows.Forms.SortOrder.Ascending;
        this.lvwResults.TabIndex = 0;
        this.lvwResults.UseCompatibleStateImageBehavior = false;
        this.lvwResults.View = System.Windows.Forms.View.Details;
        this.lvwResults.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.lvwResults_ItemCheck);
        // 
        // panel1
        // 
        this.panel1.BackColor = System.Drawing.Color.White;
        this.panel1.Controls.Add(this.label3);
        this.panel1.Controls.Add(this.groupBox2);
        this.panel1.Controls.Add(this.groupBox1);
        this.panel1.Controls.Add(this.label2);
        this.panel1.Controls.Add(this.cboFilter);
        this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
        this.panel1.Location = new System.Drawing.Point(0, 0);
        this.panel1.Name = "panel1";
        this.panel1.Size = new System.Drawing.Size(184, 390);
        this.panel1.TabIndex = 1;
        // 
        // label3
        // 
        this.label3.BackColor = System.Drawing.Color.Transparent;
        this.label3.Location = new System.Drawing.Point(0, 0);
        this.label3.Name = "label3";
        this.label3.Size = new System.Drawing.Size(184, 23);
        this.label3.TabIndex = 8;
        this.label3.Text = "Options";
        this.label3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // groupBox2
        // 
        this.groupBox2.Controls.Add(this.btnScript);
        this.groupBox2.Location = new System.Drawing.Point(8, 248);
        this.groupBox2.Name = "groupBox2";
        this.groupBox2.Size = new System.Drawing.Size(168, 59);
        this.groupBox2.TabIndex = 7;
        this.groupBox2.TabStop = false;
        this.groupBox2.Text = "Source Code";
        // 
        // btnScript
        // 
        this.btnScript.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnScript.Location = new System.Drawing.Point(16, 22);
        this.btnScript.Name = "btnScript";
        this.btnScript.Size = new System.Drawing.Size(136, 22);
        this.btnScript.TabIndex = 0;
        this.btnScript.Text = "&Preview";
        this.btnScript.Click += new System.EventHandler(this.btnScript_Click);
        // 
        // groupBox1
        // 
        this.groupBox1.Controls.Add(this.label5);
        this.groupBox1.Controls.Add(this.cboDatabaseType);
        this.groupBox1.Controls.Add(this.btnAddToModel);
        this.groupBox1.Controls.Add(this.label1);
        this.groupBox1.Controls.Add(this.cboDeploymentVersion);
        this.groupBox1.Controls.Add(this.btnAddToDeployment);
        this.groupBox1.Location = new System.Drawing.Point(8, 74);
        this.groupBox1.Name = "groupBox1";
        this.groupBox1.Size = new System.Drawing.Size(168, 168);
        this.groupBox1.TabIndex = 6;
        this.groupBox1.TabStop = false;
        this.groupBox1.Text = "Deployment Script";
        // 
        // label5
        // 
        this.label5.AutoSize = true;
        this.label5.Location = new System.Drawing.Point(16, 24);
        this.label5.Name = "label5";
        this.label5.Size = new System.Drawing.Size(83, 13);
        this.label5.TabIndex = 7;
        this.label5.Text = "Database Type:";
        // 
        // cboDatabaseType
        // 
        this.cboDatabaseType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboDatabaseType.FormattingEnabled = true;
        this.cboDatabaseType.Location = new System.Drawing.Point(13, 43);
        this.cboDatabaseType.Name = "cboDatabaseType";
        this.cboDatabaseType.Size = new System.Drawing.Size(136, 21);
        this.cboDatabaseType.TabIndex = 6;
        this.cboDatabaseType.SelectedIndexChanged += new System.EventHandler(this.CboDatabaseType_SelectedIndexChanged);
        // 
        // btnAddToModel
        // 
        this.btnAddToModel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnAddToModel.Location = new System.Drawing.Point(13, 128);
        this.btnAddToModel.Name = "btnAddToModel";
        this.btnAddToModel.Size = new System.Drawing.Size(136, 22);
        this.btnAddToModel.TabIndex = 5;
        this.btnAddToModel.Text = global::OrigamArchitect.strings.AddToModel_Button;
        this.btnAddToModel.Visible = false;
        this.btnAddToModel.Click += new System.EventHandler(this.btnAddToModel_Click);
        // 
        // label1
        // 
        this.label1.Location = new System.Drawing.Point(13, 76);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(80, 15);
        this.label1.TabIndex = 3;
        this.label1.Text = "Version:";
        // 
        // cboDeploymentVersion
        // 
        this.cboDeploymentVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboDeploymentVersion.Location = new System.Drawing.Point(13, 95);
        this.cboDeploymentVersion.Name = "cboDeploymentVersion";
        this.cboDeploymentVersion.Size = new System.Drawing.Size(136, 21);
        this.cboDeploymentVersion.TabIndex = 2;
        // 
        // btnAddToDeployment
        // 
        this.btnAddToDeployment.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnAddToDeployment.Location = new System.Drawing.Point(13, 128);
        this.btnAddToDeployment.Name = "btnAddToDeployment";
        this.btnAddToDeployment.Size = new System.Drawing.Size(136, 22);
        this.btnAddToDeployment.TabIndex = 4;
        this.btnAddToDeployment.Text = global::OrigamArchitect.strings.AddToDeployment_Button;
        this.btnAddToDeployment.Click += new System.EventHandler(this.btnAddToDeployment_Click);
        // 
        // label2
        // 
        this.label2.Location = new System.Drawing.Point(8, 36);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(31, 14);
        this.label2.TabIndex = 5;
        this.label2.Text = "Filter:";
        // 
        // cboFilter
        // 
        this.cboFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboFilter.Location = new System.Drawing.Point(46, 33);
        this.cboFilter.Name = "cboFilter";
        this.cboFilter.Size = new System.Drawing.Size(128, 21);
        this.cboFilter.TabIndex = 1;
        this.cboFilter.SelectedIndexChanged += new System.EventHandler(this.cboFilter_SelectedIndexChanged);
        // 
        // label4
        // 
        this.label4.BackColor = System.Drawing.Color.Transparent;
        this.label4.Dock = System.Windows.Forms.DockStyle.Top;
        this.label4.Location = new System.Drawing.Point(184, 0);
        this.label4.Name = "label4";
        this.label4.Size = new System.Drawing.Size(712, 23);
        this.label4.TabIndex = 9;
        this.label4.Text = "Results";
        this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        // 
        // SchemaCompareEditor
        // 
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Inherit;
        this.ClientSize = new System.Drawing.Size(896, 390);
        this.Controls.Add(this.lvwResults);
        this.Controls.Add(this.label4);
        this.Controls.Add(this.panel1);
        this.DockAreas = WeifenLuo.WinFormsUI.Docking.DockAreas.Document;
        this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.IsViewOnly = true;
        this.Name = "SchemaCompareEditor";
        this.ShowInTaskbar = false;
        this.TabText = global::OrigamArchitect.strings.DeploymentScriptGenerator_Title;
        this.Text = "Deployment Script Generator";
        this.TitleName = global::OrigamArchitect.strings.DeploymentScriptGenerator_Title;
        this.panel1.ResumeLayout(false);
        this.groupBox2.ResumeLayout(false);
        this.groupBox1.ResumeLayout(false);
        this.groupBox1.PerformLayout();
        this.ResumeLayout(false);
	}
	#endregion
	#region Public Methods
	public override void RefreshContent()
	{
		this.DisplayResults();
	}
	
	public override bool CanRefreshContent
	{
		get
		{
			return true;
		}
		set
		{
			throw new InvalidOperationException();
		}
	}
	#endregion
	#region Private Methods
	private void DisplayResults()
	{
		IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		AbstractSqlDataService da = (AbstractSqlDataService)DataServiceFactory.GetDataService();
        da.PersistenceProvider = persistence.SchemaProvider;
        Platform platform = (Platform)cboDatabaseType.SelectedItem;
        AbstractSqlDataService DaPlatform = (AbstractSqlDataService)DataServiceFactory.GetDataService(platform);
        DaPlatform.PersistenceProvider = persistence.SchemaProvider;
        _results = DaPlatform.CompareSchema(persistence.SchemaProvider);
        foreach (var result in _results)
        {
	        result.Platform = platform;
        }
        RenderList();
	}
	private List<SchemaDbCompareResult> SelectedResults()
	{
		var result = new List<SchemaDbCompareResult>();
		foreach(ListViewItem item in lvwResults.CheckedItems)
		{
			result.Add((SchemaDbCompareResult)item.Tag);
		}
		return result;
	}
	private void RenderList()
	{
		btnAddToDeployment.Visible = IsModeMissingInDatabase() 
            || IsModeExistingButDifferent();
		btnAddToModel.Visible = IsModeMissingInSchema();
		groupBox2.Visible = IsModeMissingInDatabase()
            || IsModeExistingButDifferent();
        try
		{
			lvwResults.Items.Clear();
            lvwResults.CheckBoxes = true;
			lvwResults.BeginUpdate();
			foreach(SchemaDbCompareResult result in _results)
			{
				if(ShouldDisplayResult(result))
				{
                    SchemaItemDescriptionAttribute desc = result.SchemaItemType.SchemaItemDescription();
                    ListViewItem item = new ListViewItem(
                        new string[] {desc == null ? result.SchemaItemType.Name : desc.Name,
										result.ItemName,
										result.Remark 
									}
						);
					item.Tag = result;
                    int imageIndex = -1;
                    object icon = result.SchemaItemType.SchemaItemIcon();
                    if (icon is string)
                    {
                        imageIndex = lvwResults.SmallImageList.Images.IndexOfKey((string)icon);
                    }
                    else
                    {
                        imageIndex = (int)icon;
                    }
					item.ImageIndex = imageIndex;
					lvwResults.Items.Add(item);
				}
			}
		}
		finally
		{
			lvwResults.EndUpdate();
		}
	}
	private bool IsModeExistingButDifferent()
	{
		return cboFilter.SelectedIndex == 2;
	}
	private bool IsModeMissingInDatabase()
	{
		return cboFilter.SelectedIndex == 0;
	}
	private bool IsModeMissingInSchema()
	{
		return cboFilter.SelectedIndex == 1;
	}
	private bool ShouldDisplayResult(SchemaDbCompareResult result)
	{
		return (result.ResultType == DbCompareResultType.ExistingButDifferent && IsModeExistingButDifferent())
			| (result.ResultType == DbCompareResultType.MissingInDatabase && IsModeMissingInDatabase())
			| (result.ResultType == DbCompareResultType.MissingInSchema && IsModeMissingInSchema());
	}
	#endregion
	#region Event Handlers
	private void lvwResults_ItemCheck(object sender, System.Windows.Forms.ItemCheckEventArgs e)
	{
        SchemaDbCompareResult result = lvwResults.Items[e.Index].Tag 
            as SchemaDbCompareResult;
        if (string.IsNullOrEmpty(result.Script) && result.SchemaItem == null)
		{
			e.NewValue = CheckState.Unchecked;
		}
	}
	private void btnScript_Click(object sender, System.EventArgs e)
	{
		OutputPad _pad = WorkbenchSingleton.Workbench.GetPad(typeof(OutputPad)) as OutputPad;
		StringBuilder script = new StringBuilder();	
		foreach(SchemaDbCompareResult result in SelectedResults())
		{
			if(!string.IsNullOrEmpty(result.Script))
			{
				script.Append(result.Script);
				script.Append(Environment.NewLine);
				script.Append(Environment.NewLine);
			}
		}
		foreach(SchemaDbCompareResult result in SelectedResults())
		{
			if(!string.IsNullOrEmpty(result.Script2))
			{
				script.Append(result.Script2);
				script.Append(Environment.NewLine);
				script.Append(Environment.NewLine);
			}
		}
		_pad.SetOutputText(script.ToString());
		WorkbenchSingleton.Workbench.ShowPad(_pad);
	}
	private void cboFilter_SelectedIndexChanged(object sender, System.EventArgs e)
	{
		RenderList();
	}
	private void btnAddToDeployment_Click(object sender, System.EventArgs e)
	{
		IService dataService = null;
       
        foreach (IService service in 
            _schema.GetProvider(typeof(ServiceSchemaItemProvider)).ChildItems)
		{
			if(service.Name == "DataService")
			{
				dataService = service;
				break;
			}
		}
		if(cboDeploymentVersion.SelectedIndex < 0)
		{
			MessageBox.Show(this, strings.SelectDeploymentVersionMessage, 
                strings.SelectDeploymentTitle, MessageBoxButtons.OK, MessageBoxIcon.Stop);
			return;
		}
		DeploymentVersion version = cboDeploymentVersion.SelectedItem as DeploymentVersion;
		var generatedActivities = new List<ISchemaItem>();
		foreach(SchemaDbCompareResult result in SelectedResults())
		{
			if(!string.IsNullOrEmpty(result.Script))
			{
                DatabaseType dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), 
                    result.Platform.GetParseEnum(result.Platform.DataService).ToString());
				generatedActivities.Add(
					AddActivity(result.SchemaItem.ModelDescription() 
					+ "_" + result.ItemName, result.Script, version, dataService, dbType)
					);
			}
		}
		foreach(SchemaDbCompareResult result in SelectedResults())
		{
			if(!string.IsNullOrEmpty(result.Script2))
			{
                DatabaseType dbType = (DatabaseType)Enum.Parse(typeof(DatabaseType), 
                    result.Platform.GetParseEnum(result.Platform.DataService).ToString());
                generatedActivities.Add(
					AddActivity(result.SchemaItem.ModelDescription() 
					+ "_" + result.ItemName, result.Script2, version, dataService, dbType)
					);
			}
		}
		if(generatedActivities.Count > 0)
		{
			if(MessageBox.Show(this, 
				strings.PutItemsToSearchResultQuestion, 
				strings.SelectDeploymentTitle, 
				MessageBoxButtons.YesNo, 
				MessageBoxIcon.Question) == DialogResult.Yes)
			{
				FindSchemaItemResultsPad findResults = WorkbenchSingleton.Workbench.GetPad(typeof(FindSchemaItemResultsPad)) as FindSchemaItemResultsPad;
				findResults.DisplayResults(generatedActivities.ToArray());
			}
		}
		// deselect all items
		foreach(ListViewItem item in lvwResults.Items)
		{
			item.Checked = false;
		}
	}
	private ServiceCommandUpdateScriptActivity AddActivity(
		string name, string command, DeploymentVersion version, 
        IService dataService, DatabaseType databaseType)
	{
		var activity = version.NewItem<ServiceCommandUpdateScriptActivity>(
			_schema.ActiveSchemaExtensionId, null);
		activity.Name = activity.ActivityOrder.ToString("00000") + "_" + name.Replace(" ", "_");
		activity.Service = dataService;
		activity.CommandText = command;
        activity.DatabaseType = databaseType;
		activity.Persist();
        return activity;
	}
	#endregion
	private void gotoDefinition_click(object sender, EventArgs e)
	{
		SchemaDbCompareResult result = 
            lvwResults.SelectedItems[0].Tag as SchemaDbCompareResult;
		if(result != null)
		{
			ISchemaItem toSelect = result.SchemaItem == null 
                ? result.ParentSchemaItem as ISchemaItem 
                : result.SchemaItem;
			if(toSelect != null)
			{
				try
				{
					SchemaBrowser schemaBrowser = 
                        WorkbenchSingleton.Workbench.GetPad(
                            typeof(SchemaBrowser)) as SchemaBrowser;
					schemaBrowser.EbrSchemaBrowser.SelectItem(toSelect);
                    WorkbenchSingleton.Workbench.ShowPad(schemaBrowser);
                }
				catch(Exception ex)
				{
					AsMessageBox.ShowError(this, ex.Message,strings.GenericError_Title, ex);
				}
			}
		}
	}
	private void btnAddToModel_Click(object sender, System.EventArgs e)
	{
		// check if it is possible to generate everything
		foreach(SchemaDbCompareResult result in SelectedResults())
		{
			if(result.ResultType == DbCompareResultType.MissingInSchema)
			{
				if(result.SchemaItem == null)
				{
					AsMessageBox.ShowError(this,
                        string.Format(strings.ModelGenerationNotSupported_Message, result.SchemaItemType),
                        strings.ModelGenerationTitle,
                        null);
					return;
				}
			}
		}
		// if passed we will append the parts to the model
		foreach(SchemaDbCompareResult result in SelectedResults())
		{
			if(result.ResultType == DbCompareResultType.MissingInSchema)
			{
			    var schemaItem = result.SchemaItem;
			    schemaItem.Group = _schema
			        .GetProvider<EntityModelSchemaItemProvider>()
			        .GetGroup(_schema.ActiveExtension.Name);
			    schemaItem.RootProvider.ChildItems.Add(schemaItem);
                schemaItem.Persist();
                RemoveFromList(result);
            }
		}
		MessageBox.Show(this, strings.ModelGeneratedMassage, strings.ModelGenerationTitle, MessageBoxButtons.OK, MessageBoxIcon.Information);
	}
	private void RemoveFromList(SchemaDbCompareResult result)
	{
		foreach(ListViewItem item in lvwResults.Items)
		{
			if(result.Equals(item.Tag))
			{
				lvwResults.Items.Remove(item);
			}
		}
	}
    private void CboDatabaseType_SelectedIndexChanged(object sender, EventArgs e)
    {
        DisplayResults();
    }
}
