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
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.XPath;
using MoreLinq;
using Origam.DA;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Gui.UI;
using Origam.Rule;
using Origam.Rule.Xslt;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.RuleModel;
using Origam.Service.Core;
using Origam.Services;
using Origam.UI;
using Origam.Workbench.BaseComponents;
using Origam.Workbench.Services;

namespace Origam.Workbench.Editors;
/// <summary>L
/// Summary description for XslTransformationEditor.
/// </summary>
public class XslEditor : AbstractEditor, IToolStripContainer
{
	private TextBox txtName;
	private Label lblXslName;
	private OpenFileDialog openDialog;
	
	private Workbench.Pads.OutputPad _output;
	private bool _isEditing = false;
    private Panel panel4;
    private TabControl tabControl;
    private TabPage tabXsl;
    private Origam.Windows.Editor.XmlEditor txtText;
    private TabPage tabSource;
    private Origam.Windows.Editor.XmlEditor txtSource;
    private TextBox txtXpath;
    private TabPage tabParameters;
    private TableLayoutPanel tableLayoutPanel1;
    private FlowLayoutPanel flowLayoutPanel1;
    private Label label2;
    private ComboBox parameterTypeComboBox;
    private MemoryListBox parameterList;
    private Origam.Windows.Editor.XmlEditor paremeterEditor;
    private TabPage tabResult;
    private Origam.Windows.Editor.XmlEditor txtResult;
    private TabPage tabDataResult;
    private DataGrid grdResult;
    private TabPage settingsTab;
    private TextBox txtPackage;
    private TextBox txtId;
    private ModelComboBox cboDataStructure;
    private ModelComboBox cboSourceStructure;
    private Label lblId;
    private Label label1;
    private ModelComboBox cboRuleSet;
    private Label lblDataStructure;
    private Label lblPackage;
    private Label lblRuleSet;
    private Label lblSource;
    private TreeView tvwSource;
    private Label lblDestination;
    private TreeView tvwDestination;
	private LabeledToolStrip toolStrip;
    private ModelComboBox cboTraceLevel;
    private Label lblTraceLevel;
    private readonly ParameterListUpdater parameterListUpdater;
	public override List<ToolStrip> GetToolStrips(int maxWidth) => new List<ToolStrip>{toolStrip}; 
	
	public XslEditor()
	{
		InitializeComponent();
		this.ContentLoaded += XslEditor_ContentLoaded;
		_output = WorkbenchSingleton.Workbench
			.GetPad(typeof(Workbench.Pads.OutputPad)) as Workbench.Pads.OutputPad;
		this.txtText.ContentChanged += TextArea_KeyDown;
		cboDataStructure.SelectedValueChanged +=
			cboDataStructure_SelectedValueChanged;
		txtName.Click += txtName_TextChanged;
		cboSourceStructure.SelectedValueChanged +=
			cboSourceStructure_SelectedValueChanged;
		
		this.BackColor = OrigamColorScheme.FormBackgroundColor;
		InitParameterTypeComboBox();
		paremeterEditor.Text = "";
		txtResult.Text = "";
		
		InitToolStrip();
		parameterListUpdater = new ParameterListUpdater(parameterList);
	}
	private void InitToolStrip()
	{
		toolStrip = new LabeledToolStrip(this);
		toolStrip.Text = "Xsl Transformation";
		var transformToolStripButton = new BigToolStripButton();
		transformToolStripButton.Click += btnTransform_Click;
		transformToolStripButton.Text = "Transform";
		transformToolStripButton.Image = ImageRes.Transform;
		toolStrip.Items.Add(transformToolStripButton);
		var validateToolStripButton = new BigToolStripButton();
		validateToolStripButton.Click += btnValidate_Click;
		validateToolStripButton.Text = "Validate";
		validateToolStripButton.Image = ImageRes.Validate;
		toolStrip.Items.Add(validateToolStripButton);
		
		var loadToolStripButton = new BigToolStripButton();
		loadToolStripButton.Click += btnLoadSource_Click;
		loadToolStripButton.Text = "Load Source";
		loadToolStripButton.Image = ImageRes.LoadSource;
		toolStrip.Items.Add(loadToolStripButton);
	}
	private void InitParameterTypeComboBox()
	{
		object[] allAdapDataTypes = Enum.GetValues(typeof(OrigamDataType))
			.Cast<object>()
			.ToArray();
		parameterTypeComboBox.Items.AddRange(allAdapDataTypes);
		parameterTypeComboBox.SelectedIndex = 0;
	}
    #region Overriden AbstractViewContent Members
	public override void SaveObject()
	{
		if(!ValidateXslt())
		{
			DialogResult result = MessageBox.Show(this, "XSLT validation did not succeed. Do you want to save the transformation anyway?", "XSLT Validation Failed", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
			if(result == DialogResult.No)
			{
				return;
			}
		}
		if(ModelContent is XslRule)
		{
			(ModelContent as XslRule).Xsl = txtText.Text;
		}
		else if(ModelContent is XslTransformation)
		{
			(ModelContent as XslTransformation).TextStore = txtText.Text;
		}
		else
		{
			throw new InvalidCastException("Object type not supported.");
		}
		base.SaveObject();
	}
	#endregion
	private void InitializeComponent()
	{
        this.txtName = new System.Windows.Forms.TextBox();
        this.lblXslName = new System.Windows.Forms.Label();
        this.openDialog = new System.Windows.Forms.OpenFileDialog();
        this.panel4 = new System.Windows.Forms.Panel();
        this.tabControl = new System.Windows.Forms.TabControl();
        this.tabXsl = new System.Windows.Forms.TabPage();
        this.txtText = new Origam.Windows.Editor.XmlEditor();
        this.tabSource = new System.Windows.Forms.TabPage();
        this.txtSource = new Origam.Windows.Editor.XmlEditor();
        this.txtXpath = new System.Windows.Forms.TextBox();
        this.tabParameters = new System.Windows.Forms.TabPage();
        this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
        this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
        this.label2 = new System.Windows.Forms.Label();
        this.parameterTypeComboBox = new System.Windows.Forms.ComboBox();
        this.parameterList = new Origam.Workbench.Editors.MemoryListBox();
        this.paremeterEditor = new Origam.Windows.Editor.XmlEditor();
        this.tabResult = new System.Windows.Forms.TabPage();
        this.txtResult = new Origam.Windows.Editor.XmlEditor();
        this.tabDataResult = new System.Windows.Forms.TabPage();
        this.grdResult = new System.Windows.Forms.DataGrid();
        this.settingsTab = new System.Windows.Forms.TabPage();
        this.txtPackage = new System.Windows.Forms.TextBox();
        this.txtId = new System.Windows.Forms.TextBox();
        this.cboDataStructure = new Origam.UI.ModelComboBox();
        this.cboSourceStructure = new Origam.UI.ModelComboBox();
        this.lblId = new System.Windows.Forms.Label();
        this.label1 = new System.Windows.Forms.Label();
        this.cboRuleSet = new Origam.UI.ModelComboBox();
        this.lblDataStructure = new System.Windows.Forms.Label();
        this.lblPackage = new System.Windows.Forms.Label();
        this.lblRuleSet = new System.Windows.Forms.Label();
        this.lblSource = new System.Windows.Forms.Label();
        this.tvwSource = new System.Windows.Forms.TreeView();
        this.lblDestination = new System.Windows.Forms.Label();
        this.tvwDestination = new System.Windows.Forms.TreeView();
        this.cboTraceLevel = new Origam.UI.ModelComboBox();
        this.lblTraceLevel = new System.Windows.Forms.Label();
        this.panel4.SuspendLayout();
        this.tabControl.SuspendLayout();
        this.tabXsl.SuspendLayout();
        this.tabSource.SuspendLayout();
        this.tabParameters.SuspendLayout();
        this.tableLayoutPanel1.SuspendLayout();
        this.flowLayoutPanel1.SuspendLayout();
        this.tabResult.SuspendLayout();
        this.tabDataResult.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.grdResult)).BeginInit();
        this.settingsTab.SuspendLayout();
        this.SuspendLayout();
        // 
        // txtName
        // 
        this.txtName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.txtName.Location = new System.Drawing.Point(128, 7);
        this.txtName.Name = "txtName";
        this.txtName.Size = new System.Drawing.Size(500, 20);
        this.txtName.TabIndex = 3;
        this.txtName.TextChanged += new System.EventHandler(this.txtName_TextChanged);
        // 
        // lblXslName
        // 
        this.lblXslName.Location = new System.Drawing.Point(8, 7);
        this.lblXslName.Name = "lblXslName";
        this.lblXslName.Size = new System.Drawing.Size(112, 17);
        this.lblXslName.TabIndex = 2;
        this.lblXslName.Text = "Name";
        // 
        // openDialog
        // 
        this.openDialog.DefaultExt = "*.xml";
        // 
        // panel4
        // 
        this.panel4.Controls.Add(this.tabControl);
        this.panel4.Controls.Add(this.txtName);
        this.panel4.Controls.Add(this.lblXslName);
        this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
        this.panel4.Location = new System.Drawing.Point(0, 32);
        this.panel4.Name = "panel4";
        this.panel4.Size = new System.Drawing.Size(768, 541);
        this.panel4.TabIndex = 16;
        // 
        // tabControl
        // 
        this.tabControl.Alignment = System.Windows.Forms.TabAlignment.Bottom;
        this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.tabControl.Controls.Add(this.tabXsl);
        this.tabControl.Controls.Add(this.tabSource);
        this.tabControl.Controls.Add(this.tabParameters);
        this.tabControl.Controls.Add(this.tabResult);
        this.tabControl.Controls.Add(this.tabDataResult);
        this.tabControl.Controls.Add(this.settingsTab);
        this.tabControl.Location = new System.Drawing.Point(0, 31);
        this.tabControl.Multiline = true;
        this.tabControl.Name = "tabControl";
        this.tabControl.SelectedIndex = 0;
        this.tabControl.Size = new System.Drawing.Size(762, 506);
        this.tabControl.TabIndex = 8;
        this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
        // 
        // tabXsl
        // 
        this.tabXsl.Controls.Add(this.txtText);
        this.tabXsl.Location = new System.Drawing.Point(4, 4);
        this.tabXsl.Name = "tabXsl";
        this.tabXsl.Size = new System.Drawing.Size(754, 480);
        this.tabXsl.TabIndex = 0;
        this.tabXsl.Text = "XSL";
        // 
        // txtText
        // 
        this.txtText.BackColor = System.Drawing.Color.White;
        this.txtText.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtText.IsReadOnly = false;
        this.txtText.Location = new System.Drawing.Point(0, 0);
        this.txtText.Margin = new System.Windows.Forms.Padding(4);
        this.txtText.Name = "txtText";
        this.txtText.ResultSchema = null;
        this.txtText.Size = new System.Drawing.Size(754, 480);
        this.txtText.TabIndex = 8;
        // 
        // tabSource
        // 
        this.tabSource.Controls.Add(this.txtSource);
        this.tabSource.Controls.Add(this.txtXpath);
        this.tabSource.Location = new System.Drawing.Point(4, 4);
        this.tabSource.Name = "tabSource";
        this.tabSource.Size = new System.Drawing.Size(535, 373);
        this.tabSource.TabIndex = 1;
        this.tabSource.Text = "Source XML";
        // 
        // txtSource
        // 
        this.txtSource.BackColor = System.Drawing.Color.White;
        this.txtSource.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtSource.IsReadOnly = false;
        this.txtSource.Location = new System.Drawing.Point(0, 20);
        this.txtSource.Margin = new System.Windows.Forms.Padding(4);
        this.txtSource.Name = "txtSource";
        this.txtSource.ResultSchema = null;
        this.txtSource.Size = new System.Drawing.Size(535, 353);
        this.txtSource.TabIndex = 7;
        // 
        // txtXpath
        // 
        this.txtXpath.Dock = System.Windows.Forms.DockStyle.Top;
        this.txtXpath.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        this.txtXpath.Location = new System.Drawing.Point(0, 0);
        this.txtXpath.Name = "txtXpath";
        this.txtXpath.Size = new System.Drawing.Size(535, 20);
        this.txtXpath.TabIndex = 8;
        // 
        // tabParameters
        // 
        this.tabParameters.Controls.Add(this.tableLayoutPanel1);
        this.tabParameters.Location = new System.Drawing.Point(4, 4);
        this.tabParameters.Name = "tabParameters";
        this.tabParameters.Padding = new System.Windows.Forms.Padding(3);
        this.tabParameters.Size = new System.Drawing.Size(535, 373);
        this.tabParameters.TabIndex = 6;
        this.tabParameters.Text = "Input Parameters";
        this.tabParameters.UseVisualStyleBackColor = true;
        // 
        // tableLayoutPanel1
        // 
        this.tableLayoutPanel1.ColumnCount = 2;
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 19.50887F));
        this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 80.49113F));
        this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 0);
        this.tableLayoutPanel1.Controls.Add(this.parameterList, 0, 1);
        this.tableLayoutPanel1.Controls.Add(this.paremeterEditor, 1, 1);
        this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tableLayoutPanel1.Location = new System.Drawing.Point(3, 3);
        this.tableLayoutPanel1.Name = "tableLayoutPanel1";
        this.tableLayoutPanel1.RowCount = 2;
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 9.82801F));
        this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 90.17199F));
        this.tableLayoutPanel1.Size = new System.Drawing.Size(529, 367);
        this.tableLayoutPanel1.TabIndex = 14;
        // 
        // flowLayoutPanel1
        // 
        this.flowLayoutPanel1.Controls.Add(this.label2);
        this.flowLayoutPanel1.Controls.Add(this.parameterTypeComboBox);
        this.flowLayoutPanel1.Location = new System.Drawing.Point(106, 3);
        this.flowLayoutPanel1.MinimumSize = new System.Drawing.Size(180, 32);
        this.flowLayoutPanel1.Name = "flowLayoutPanel1";
        this.flowLayoutPanel1.Size = new System.Drawing.Size(180, 32);
        this.flowLayoutPanel1.TabIndex = 13;
        // 
        // label2
        // 
        this.label2.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.label2.AutoSize = true;
        this.label2.Location = new System.Drawing.Point(3, 7);
        this.label2.Name = "label2";
        this.label2.Size = new System.Drawing.Size(31, 13);
        this.label2.TabIndex = 12;
        this.label2.Text = "Type";
        // 
        // parameterTypeComboBox
        // 
        this.parameterTypeComboBox.Anchor = System.Windows.Forms.AnchorStyles.Left;
        this.parameterTypeComboBox.FormattingEnabled = true;
        this.parameterTypeComboBox.Location = new System.Drawing.Point(40, 3);
        this.parameterTypeComboBox.Name = "parameterTypeComboBox";
        this.parameterTypeComboBox.Size = new System.Drawing.Size(86, 21);
        this.parameterTypeComboBox.TabIndex = 10;
        // 
        // parameterList
        // 
        this.parameterList.Dock = System.Windows.Forms.DockStyle.Fill;
        this.parameterList.FormattingEnabled = true;
        this.parameterList.Location = new System.Drawing.Point(3, 39);
        this.parameterList.MinimumSize = new System.Drawing.Size(120, 0);
        this.parameterList.Name = "parameterList";
        this.parameterList.Size = new System.Drawing.Size(120, 325);
        this.parameterList.TabIndex = 11;
        this.parameterList.SelectedIndexChanged += new System.EventHandler(this.paremeterList_SelectedIndexChanged);
        // 
        // paremeterEditor
        // 
        this.paremeterEditor.BackColor = System.Drawing.Color.White;
        this.paremeterEditor.Dock = System.Windows.Forms.DockStyle.Fill;
        this.paremeterEditor.IsReadOnly = false;
        this.paremeterEditor.Location = new System.Drawing.Point(107, 40);
        this.paremeterEditor.Margin = new System.Windows.Forms.Padding(4);
        this.paremeterEditor.Name = "paremeterEditor";
        this.paremeterEditor.ResultSchema = null;
        this.paremeterEditor.Size = new System.Drawing.Size(418, 323);
        this.paremeterEditor.TabIndex = 9;
        // 
        // tabResult
        // 
        this.tabResult.Controls.Add(this.txtResult);
        this.tabResult.Location = new System.Drawing.Point(4, 4);
        this.tabResult.Name = "tabResult";
        this.tabResult.Size = new System.Drawing.Size(535, 373);
        this.tabResult.TabIndex = 2;
        this.tabResult.Text = "Result";
        // 
        // txtResult
        // 
        this.txtResult.BackColor = System.Drawing.Color.White;
        this.txtResult.Dock = System.Windows.Forms.DockStyle.Fill;
        this.txtResult.IsReadOnly = false;
        this.txtResult.Location = new System.Drawing.Point(0, 0);
        this.txtResult.Margin = new System.Windows.Forms.Padding(4);
        this.txtResult.Name = "txtResult";
        this.txtResult.ResultSchema = null;
        this.txtResult.Size = new System.Drawing.Size(535, 373);
        this.txtResult.TabIndex = 8;
        // 
        // tabDataResult
        // 
        this.tabDataResult.Controls.Add(this.grdResult);
        this.tabDataResult.Location = new System.Drawing.Point(4, 4);
        this.tabDataResult.Name = "tabDataResult";
        this.tabDataResult.Size = new System.Drawing.Size(535, 373);
        this.tabDataResult.TabIndex = 3;
        this.tabDataResult.Text = "Data Result";
        // 
        // grdResult
        // 
        this.grdResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.grdResult.DataMember = "";
        this.grdResult.Dock = System.Windows.Forms.DockStyle.Fill;
        this.grdResult.HeaderForeColor = System.Drawing.SystemColors.ControlText;
        this.grdResult.Location = new System.Drawing.Point(0, 0);
        this.grdResult.Name = "grdResult";
        this.grdResult.Size = new System.Drawing.Size(535, 373);
        this.grdResult.TabIndex = 0;
        // 
        // settingsTab
        // 
        this.settingsTab.Controls.Add(this.cboTraceLevel);
        this.settingsTab.Controls.Add(this.lblTraceLevel);
        this.settingsTab.Controls.Add(this.txtPackage);
        this.settingsTab.Controls.Add(this.txtId);
        this.settingsTab.Controls.Add(this.cboDataStructure);
        this.settingsTab.Controls.Add(this.cboSourceStructure);
        this.settingsTab.Controls.Add(this.lblId);
        this.settingsTab.Controls.Add(this.label1);
        this.settingsTab.Controls.Add(this.cboRuleSet);
        this.settingsTab.Controls.Add(this.lblDataStructure);
        this.settingsTab.Controls.Add(this.lblPackage);
        this.settingsTab.Controls.Add(this.lblRuleSet);
        this.settingsTab.Location = new System.Drawing.Point(4, 4);
        this.settingsTab.Name = "settingsTab";
        this.settingsTab.Padding = new System.Windows.Forms.Padding(3);
        this.settingsTab.Size = new System.Drawing.Size(754, 480);
        this.settingsTab.TabIndex = 5;
        this.settingsTab.Text = "Settings";
        this.settingsTab.UseVisualStyleBackColor = true;
        // 
        // txtPackage
        // 
        this.txtPackage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.txtPackage.Location = new System.Drawing.Point(381, 10);
        this.txtPackage.Name = "txtPackage";
        this.txtPackage.ReadOnly = true;
        this.txtPackage.Size = new System.Drawing.Size(235, 20);
        this.txtPackage.TabIndex = 2;
        // 
        // txtId
        // 
        this.txtId.Location = new System.Drawing.Point(125, 10);
        this.txtId.Name = "txtId";
        this.txtId.ReadOnly = true;
        this.txtId.Size = new System.Drawing.Size(204, 20);
        this.txtId.TabIndex = 1;
        // 
        // cboDataStructure
        // 
        this.cboDataStructure.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.cboDataStructure.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
        this.cboDataStructure.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
        this.cboDataStructure.Location = new System.Drawing.Point(125, 84);
        this.cboDataStructure.Name = "cboDataStructure";
        this.cboDataStructure.Size = new System.Drawing.Size(491, 21);
        this.cboDataStructure.Sorted = true;
        this.cboDataStructure.TabIndex = 6;
        // 
        // cboSourceStructure
        // 
        this.cboSourceStructure.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.cboSourceStructure.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
        this.cboSourceStructure.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
        this.cboSourceStructure.Location = new System.Drawing.Point(125, 59);
        this.cboSourceStructure.Name = "cboSourceStructure";
        this.cboSourceStructure.Size = new System.Drawing.Size(491, 21);
        this.cboSourceStructure.Sorted = true;
        this.cboSourceStructure.TabIndex = 5;
        // 
        // lblId
        // 
        this.lblId.Location = new System.Drawing.Point(7, 12);
        this.lblId.Name = "lblId";
        this.lblId.Size = new System.Drawing.Size(112, 16);
        this.lblId.TabIndex = 0;
        this.lblId.Text = "Id";
        // 
        // label1
        // 
        this.label1.Location = new System.Drawing.Point(5, 59);
        this.label1.Name = "label1";
        this.label1.Size = new System.Drawing.Size(120, 17);
        this.label1.TabIndex = 4;
        this.label1.Text = "Source Structure:";
        // 
        // cboRuleSet
        // 
        this.cboRuleSet.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.cboRuleSet.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
        this.cboRuleSet.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
        this.cboRuleSet.Location = new System.Drawing.Point(125, 108);
        this.cboRuleSet.Name = "cboRuleSet";
        this.cboRuleSet.Size = new System.Drawing.Size(491, 21);
        this.cboRuleSet.Sorted = true;
        this.cboRuleSet.TabIndex = 7;
        // 
        // lblDataStructure
        // 
        this.lblDataStructure.Location = new System.Drawing.Point(5, 84);
        this.lblDataStructure.Name = "lblDataStructure";
        this.lblDataStructure.Size = new System.Drawing.Size(120, 16);
        this.lblDataStructure.TabIndex = 6;
        this.lblDataStructure.Text = "Destination Structure:";
        // 
        // lblPackage
        // 
        this.lblPackage.Location = new System.Drawing.Point(334, 12);
        this.lblPackage.Name = "lblPackage";
        this.lblPackage.Size = new System.Drawing.Size(55, 16);
        this.lblPackage.TabIndex = 14;
        this.lblPackage.Text = "Package";
        // 
        // lblRuleSet
        // 
        this.lblRuleSet.Location = new System.Drawing.Point(7, 108);
        this.lblRuleSet.Name = "lblRuleSet";
        this.lblRuleSet.Size = new System.Drawing.Size(110, 20);
        this.lblRuleSet.TabIndex = 13;
        this.lblRuleSet.Text = "Rule Set:";
        // 
        // lblSource
        // 
        this.lblSource.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblSource.Location = new System.Drawing.Point(0, 0);
        this.lblSource.Name = "lblSource";
        this.lblSource.Size = new System.Drawing.Size(269, 20);
        this.lblSource.TabIndex = 0;
        // 
        // tvwSource
        // 
        this.tvwSource.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tvwSource.LineColor = System.Drawing.Color.Empty;
        this.tvwSource.Location = new System.Drawing.Point(0, 20);
        this.tvwSource.Name = "tvwSource";
        this.tvwSource.Size = new System.Drawing.Size(269, 393);
        this.tvwSource.TabIndex = 1;
        // 
        // lblDestination
        // 
        this.lblDestination.Dock = System.Windows.Forms.DockStyle.Top;
        this.lblDestination.Location = new System.Drawing.Point(0, 0);
        this.lblDestination.Name = "lblDestination";
        this.lblDestination.Size = new System.Drawing.Size(258, 20);
        this.lblDestination.TabIndex = 1;
        // 
        // tvwDestination
        // 
        this.tvwDestination.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tvwDestination.LineColor = System.Drawing.Color.Empty;
        this.tvwDestination.Location = new System.Drawing.Point(0, 20);
        this.tvwDestination.Name = "tvwDestination";
        this.tvwDestination.Size = new System.Drawing.Size(258, 393);
        this.tvwDestination.TabIndex = 2;
        // 
        // chboTraceLevel
        // 
        this.cboTraceLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.cboTraceLevel.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.Append;
        this.cboTraceLevel.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.ListItems;
        this.cboTraceLevel.Location = new System.Drawing.Point(125, 132);
        this.cboTraceLevel.Name = "cboTraceLevel";
        this.cboTraceLevel.Size = new System.Drawing.Size(491, 21);
        this.cboTraceLevel.Sorted = true;
        this.cboTraceLevel.TabIndex = 17;
        this.cboTraceLevel.SelectedIndexChanged += new System.EventHandler(this.cboTraceLevel_SelectedIndexChanged);
        // 
        // lblTraceLevel
        // 
        this.lblTraceLevel.Location = new System.Drawing.Point(7, 135);
        this.lblTraceLevel.Name = "lblTraceLevel";
        this.lblTraceLevel.Size = new System.Drawing.Size(110, 20);
        this.lblTraceLevel.TabIndex = 18;
        this.lblTraceLevel.Text = "Trace Level:";
        // 
        // XslEditor
        // 
        this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
        this.ClientSize = new System.Drawing.Size(847, 573);
        this.Controls.Add(this.panel4);
        this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
        this.Name = "XslEditor";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Controls.SetChildIndex(this.panel4, 0);
        this.panel4.ResumeLayout(false);
        this.panel4.PerformLayout();
        this.tabControl.ResumeLayout(false);
        this.tabXsl.ResumeLayout(false);
        this.tabSource.ResumeLayout(false);
        this.tabSource.PerformLayout();
        this.tabParameters.ResumeLayout(false);
        this.tableLayoutPanel1.ResumeLayout(false);
        this.flowLayoutPanel1.ResumeLayout(false);
        this.flowLayoutPanel1.PerformLayout();
        this.tabResult.ResumeLayout(false);
        this.tabDataResult.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)(this.grdResult)).EndInit();
        this.settingsTab.ResumeLayout(false);
        this.settingsTab.PerformLayout();
        this.ResumeLayout(false);
        this.PerformLayout();
	}
	
	private void txtName_TextChanged(object sender, EventArgs e)
	{
		if(ModelContent != null & ! this.IsViewOnly)
		{
			if(! _isEditing)
			{
				ModelContent.Name = txtName.Text;
				this.IsDirty = true;
				this.TitleName = ModelContent.Name;
			}
		}
	}
	private void LoadDataStructures(ComboBox control)
	{
		_isEditing = true;
		cboRuleSet.Items.Clear();
		IDataStructure oldValue = control.SelectedItem as IDataStructure;
		ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
		DataStructureSchemaItemProvider structures = schema.GetProvider(typeof(DataStructureSchemaItemProvider)) as DataStructureSchemaItemProvider;
		control.BeginUpdate();
		control.Items.Clear();
		foreach(IDataStructure structure in structures.ChildItems)
		{
			control.Items.Add(structure);
		}
		
		control.EndUpdate();
		if(oldValue != null)
		{
			if(control.Items.Contains(oldValue))
			{
				control.SelectedItem = oldValue;
			}
		}
		_isEditing = false;
	}
	private void cboDataStructure_SelectedValueChanged(object sender, EventArgs e)
	{
		if(! _isEditing & ! this.IsViewOnly)
		{
            if (ModelContent is XslRule)
			{
				(ModelContent as XslRule).Structure = cboDataStructure.SelectedItem as IDataStructure;
				this.IsDirty = true;
			}
            txtText.ResultSchema = txtResult.ResultSchema 
                = GetSchema(cboDataStructure.SelectedItem);
        }
        cboRuleSet.BeginUpdate();
        cboRuleSet.Items.Clear();
        if (cboDataStructure.SelectedItem != null)
        {
            IDataStructure structure = cboDataStructure.SelectedItem as IDataStructure;
            foreach (DataStructureRuleSet ruleSet in structure.ChildItemsByType<DataStructureRuleSet>(DataStructureRuleSet.CategoryConst))
            {
                cboRuleSet.Items.Add(ruleSet);
            }
        }
        cboRuleSet.EndUpdate();
    }
    private string GetSchema(object data)
    {
        DataStructure ds = data as DataStructure;
        XsdDataStructure xsdds = data as XsdDataStructure;
        if (ds != null)
        {
            DatasetGenerator gen = new DatasetGenerator(true);
            return gen.CreateDataSet(ds).GetXmlSchema();
        }
        if (xsdds != null)
        {
            return xsdds.Xsd;
        }
        return null;
    }
	private void TextArea_KeyDown(object sender, EventArgs e)
	{
		if(ModelContent != null & ! this.IsViewOnly)
		{
			if(! _isEditing)
			{
				this.IsDirty = true;
			}
		}
	}
	private void XslEditor_ContentLoaded(object sender, EventArgs e)
	{
        LoadDataStructures(cboDataStructure);
        LoadDataStructures(cboSourceStructure);
        txtSource.Text = "<ROOT>\r\n</ROOT>";
		if(this.IsViewOnly)
		{
			txtName.ReadOnly = true;
			txtText.IsReadOnly = true;
			cboDataStructure.Enabled = false;
			cboSourceStructure.Enabled = false;
			cboRuleSet.Enabled = false;
		}
		if(ModelContent is XslRule)
		{
			XslRule _xslRule = ModelContent as XslRule;
			_isEditing = true;
			txtName.Text = _xslRule.Name;
			txtText.Text = _xslRule.Xsl ?? "";
			txtId.Text = _xslRule.Id.ToString();
            txtPackage.Text = _xslRule.PackageName;
			if(_xslRule.Structure != null)
			{
				cboDataStructure.SelectedItem = _xslRule.Structure;
            }
			cboTraceLevel.Items.Add(Trace.InheritFromParent);
			cboTraceLevel.Items.Add(Trace.Yes);
			cboTraceLevel.Items.Add(Trace.No);
			cboTraceLevel.SelectedItem = _xslRule.TraceLevel;
            if (txtText.Text == "") txtText.Text = 
			"<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
			"<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"" + Environment.NewLine +
			"\txmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"" + Environment.NewLine +
			"\txmlns:date=\"http://exslt.org/dates-and-times\" exclude-result-prefixes=\"AS date\">" + Environment.NewLine + Environment.NewLine +
		    "\t<xsl:include href=\"model://e1c65fcd-118d-4eb3-9c2f-aa27fec132ba\"/>" + Environment.NewLine + Environment.NewLine +
			"\t<xsl:template match=\"ROOT\">" + Environment.NewLine +   
			"\t\t<RuleExceptionDataCollection xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\">" + Environment.NewLine +
			"\t\t\t<xsl:apply-templates select=\"\"/>" + Environment.NewLine +
			"\t\t</RuleExceptionDataCollection>" + Environment.NewLine +
			"\t</xsl:template>" + Environment.NewLine + Environment.NewLine +
								   "\t<xsl:template match=\"\">" + Environment.NewLine +   
								   "\t\t<xsl:if test=\"\">" + Environment.NewLine +
								   "\t\t\t<xsl:call-template name=\"Exception\">" + Environment.NewLine +
								   "\t\t\t\t<xsl:with-param name=\"FieldName\"><xsl:value-of select=\"''\"/></xsl:with-param>" + Environment.NewLine +
								   "\t\t\t\t<xsl:with-param name=\"EntityName\"><xsl:value-of select=\"''\"/></xsl:with-param>" + Environment.NewLine +
								   "\t\t\t\t<xsl:with-param name=\"Message\"><xsl:value-of select=\"''\"/></xsl:with-param>" + Environment.NewLine +
								   "\t\t\t\t<xsl:with-param name=\"Severity\"><xsl:value-of select=\"'High'\"/></xsl:with-param>" + Environment.NewLine +
								   "\t\t\t</xsl:call-template>" + Environment.NewLine +
								   "\t\t</xsl:if>" + Environment.NewLine +
								   "\t</xsl:template>" + Environment.NewLine + Environment.NewLine +
			"</xsl:stylesheet>";
			_isEditing = false;
		}
		else if(ModelContent is XslTransformation)
		{
			cboTraceLevel.Visible = false;
			lblTraceLevel.Visible = false;
			
			XslTransformation _XslTransformation = ModelContent as XslTransformation;
			_isEditing = true;
			txtName.Text = _XslTransformation.Name;
			txtText.Text = _XslTransformation.TextStore == null ? "" : _XslTransformation.TextStore;
			txtId.Text = _XslTransformation.Id.ToString();
            txtPackage.Text = _XslTransformation.PackageName;
			if(txtText.Text == "") txtText.Text = 
									   "<?xml version=\"1.0\" encoding=\"UTF-8\"?>" + Environment.NewLine +
									   "<xsl:stylesheet xmlns:xsl=\"http://www.w3.org/1999/XSL/Transform\" version=\"1.0\"" + Environment.NewLine +
									   "\txmlns:AS=\"http://schema.advantages.cz/AsapFunctions\"" + Environment.NewLine +
									   "\txmlns:date=\"http://exslt.org/dates-and-times\" exclude-result-prefixes=\"AS date\">" + Environment.NewLine + Environment.NewLine +
									   "\t<xsl:template match=\"ROOT\">" + Environment.NewLine +
									   "\t\t<ROOT>" + Environment.NewLine +
									   "\t\t\t<xsl:apply-templates select=\"\"/>" + Environment.NewLine +
									   "\t\t</ROOT>" + Environment.NewLine +
									   "\t</xsl:template>" + Environment.NewLine + Environment.NewLine +
									   "\t<xsl:template match=\"\">" + Environment.NewLine +
									   "\t\t<xsl:copy>" + Environment.NewLine +
									   "\t\t\t<xsl:copy-of select=\"@*\"/>" + Environment.NewLine +
									   "\t\t\t<xsl:attribute name=\"\"><xsl:value-of select=\"\"/></xsl:attribute>" + Environment.NewLine +
									   "\t\t\t<xsl:copy-of select=\"*\"/>" + Environment.NewLine +
									   "\t\t</xsl:copy>" + Environment.NewLine +
									   "\t</xsl:template>" + Environment.NewLine +
									   "</xsl:stylesheet>";
			_isEditing = false;
		}
		else
		{
			throw new InvalidCastException("Object type not supported.");
		}
    }
    private void btnLoadSource_Click(object sender, EventArgs e)
	{
		if(openDialog.ShowDialog() == DialogResult.OK)
		{
			System.IO.StreamReader reader = null;
			try
			{
				reader = new System.IO.StreamReader(openDialog.OpenFile());
				txtSource.Text = reader.ReadToEnd();
			}
			finally
			{
				if(reader != null) reader.Close();
			}
			tabControl.SelectedTab = tabSource;
		}
	}
	private void btnTransform_Click(object sender, EventArgs e)
	{
	    IXmlContainer result = this.Transform(txtText.Text, txtSource.Text, false);
		string resultText = GetFormattedXml(result?.Xml);
				
		txtResult.Text = resultText;
				
		grdResult.DataSource = null;
		if(result == null) return;
		try
		{
			if(result is IDataDocument)
			{
				grdResult.DataSource = (result as IDataDocument).DataSet;
			}
			else
			{
				DataSet data = new DataSet();
				data.ReadXml(new XmlNodeReader(result.Xml));
				grdResult.DataSource = data;
			}
		}
		catch{}
		tabControl.SelectedTab = tabResult;
	}
	private IXmlContainer Transform(string xslt, string sourceXml, bool validateOnly)
	{
        Workbench.Commands.ViewOutputPad outputPad =
            new Workbench.Commands.ViewOutputPad();
        outputPad.Run();
        Workbench.Pads.OutputPad output = WorkbenchSingleton.Workbench.GetPad(
            typeof(Workbench.Pads.OutputPad)) as Workbench.Pads.OutputPad;
		output.SetOutputText("");
		string transactionId = Guid.NewGuid().ToString();
		try
		{	
			IServiceAgent transformer = (ServiceManager.Services.GetService(
                typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent(
                "DataTransformationService", 
                RuleEngine.Create(new Hashtable(), null), null);
			var doc = new XmlContainer(sourceXml);
			transformer.MethodName = "TransformText";
			transformer.Parameters.Add("XslScript", xslt);
			transformer.Parameters.Add("Data", doc);
			transformer.Parameters.Add("ValidateOnly", validateOnly);
			transformer.TransactionId = transactionId;
			transformer.OutputStructure = cboDataStructure.SelectedItem as IDataStructure;
			// resolve transformation input parameters and try to put an empty xml document to each just
			// in case it expects a node set as a parameter
			var xsltParams = XmlTools.ResolveTransformationParameters(xslt);
			RefreshParameterList();
			LoadDisplayedParameterData();
			Hashtable parameterValues = GetParameterValues(xsltParams);
			transformer.Parameters.Add("Parameters", parameterValues);
			transformer.Run();
		    IXmlContainer result = transformer.Result as IXmlContainer;
		    if (result == null) return new XmlContainer();
			// rule handling
			DataStructureRuleSet ruleSet = cboRuleSet.SelectedItem as DataStructureRuleSet;
		    IDataDocument dataDoc = result as IDataDocument;
		    if (dataDoc != null)
		    {
		        if (dataDoc.DataSet.HasErrors == false && ruleSet != null)
		        {
		            RuleEngine re = RuleEngine.Create(null, null);
		            re.ProcessRules(dataDoc, ruleSet, null);
		        }
		    }
		    return result;
        }
		catch(Exception ex)
		{
			StringBuilder sb = new StringBuilder();
			ErrorMessage(ex, sb);
			_output.SetOutputText(sb.ToString());
			return null;
		}
		finally
		{
			ResourceMonitor.Rollback(transactionId);
		}
	}
	private Hashtable GetParameterValues(IList<string> paramNames)
	{
		var parHashtable = new Hashtable();
		foreach (var paramName in paramNames)
		{			
			ParameterData correspondingData = parameterList.Items
			   .Cast<ParameterData>()
			   .FirstOrDefault(parData => parData.Name == paramName) 
			   ?? throw new ArgumentException( $"Parameter named {paramName} was not found among Input Parameters.");
			
			parHashtable.Add(paramName,correspondingData.Value);
		}
		return parHashtable;
	}
	private void LoadDisplayedParameterData()
	{
		var previousParData = (ParameterData) parameterList.SelectedItem;
		UpdateParameterData(previousParData);
	}
	private void ErrorMessage(Exception ex, StringBuilder sb)
	{
		sb.Append(ex.Message);
		sb.Append(Environment.NewLine);
		if(ex.InnerException != null)
		{
			ErrorMessage(ex.InnerException, sb);
		}
	}
	protected override bool ProcessDialogKey(Keys keyData)
	{
		if(keyData == Keys.Enter)
		{
			ProcessXpath(txtXpath.Text, txtSource.Text);
			return true;
		}
		else
		{
			return base.ProcessDialogKey (keyData);
		}
	}
	private void ProcessXpath(string xpath, string xml)
	{
		try
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);
			XPathNavigator nav = doc.CreateNavigator();
			XPathExpression expr = nav.Compile(xpath);
			OrigamXsltContext ctx = OrigamXsltContext.Create(
				new NameTable(), null);
			expr.SetContext(ctx);
			object result = nav.Evaluate(expr);
			if(result is XPathNodeIterator)
			{
				XPathNodeIterator iterator = result as XPathNodeIterator;
				StringBuilder builder = new StringBuilder();
				builder.Append("Returned " + iterator.Count.ToString() + " nodes:");
				builder.Append(Environment.NewLine);
				for(int i = 0; i < iterator.Count; i++)
				{
					iterator.MoveNext();
					XmlNode node = ((IHasXmlNode)iterator.Current).GetNode();
					builder.Append(GetFormattedXml(node) + Environment.NewLine);
				}
				_output.SetOutputText(builder.ToString());
			}
			else
			{
				_output.SetOutputText(string.Format("Returned value:\n{0}", result));
			}
		}
		catch(Exception ex)
		{
			_output.SetOutputText(ex.Message);
		}
	}
	private string GetFormattedXml(XmlNode node)
	{
		if(node == null) return "";
		string resultText = "";
		System.IO.StringWriter swriter = new System.IO.StringWriter();
		XmlTextWriter xwriter = new XmlTextWriter(swriter);
		try
		{
			xwriter.Formatting = Formatting.Indented;
			node.WriteTo(xwriter);
			resultText = swriter.ToString();
		}
		finally
		{	
			xwriter.Close();
			swriter.Close();
		}
		return resultText;
	}
	private XmlDocument LoadXslt()
	{
		try
		{
			XmlDocument doc = new XmlDocument();
			doc.LoadXml(txtText.Text);
			return doc;
		}
		catch(Exception ex)
		{
			_output.SetOutputText(ex.Message);
			return null;
		}
	}
	private bool ValidateXslt()
	{
		if(LoadXslt() == null || this.Transform(txtText.Text, "<ROOT/>", true) == null)
		{
			MessageBox.Show(this, "XSLT validation failed. See output for details.", "XSLT Validation", MessageBoxButtons.OK, MessageBoxIcon.Error);
			return false;
		}
		_output.SetOutputText("XSLT is valid.");
		return true;
	}
	private void btnValidate_Click(object sender, EventArgs e)
	{
		if(ValidateXslt()) MessageBox.Show(this, "The stylesheet is valid.", "XSLT Validation", MessageBoxButtons.OK, MessageBoxIcon.Information);
	}
    private void cboSourceStructure_SelectedValueChanged(object sender, EventArgs e)
    {
        txtSource.ResultSchema = GetSchema(cboSourceStructure.SelectedItem);
    }
    private void paremeterList_SelectedIndexChanged(object sender, EventArgs e)
    {
        var previousParData = (ParameterData)parameterList.PreviouslySelectedItem;
        UpdateParameterData(previousParData);
        var selectedParameterName = (ParameterData)parameterList.SelectedItem;
        DisplayParameterData(selectedParameterName);
    }
    private void UpdateParameterData(ParameterData parData)
    {
	    if (parData == null) return;
        parData.Type = (OrigamDataType)parameterTypeComboBox.SelectedItem;
        parData.Text = paremeterEditor.Text;
    }
    private void DisplayParameterData(ParameterData parData)
    {
	    if (parData == null) return;
        parameterTypeComboBox.SelectedItem = parData.Type;
        paremeterEditor.Text = parData.Text;
    }
    private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (tabControl.SelectedTab == tabParameters)
        {
            RefreshParameterList();
            parameterList.SelectFirstIfAny();
            paremeterEditor.Focus();
        } else if (tabControl.SelectedTab == tabXsl)
        {
            txtText.Focus();
        }else if (tabControl.SelectedTab == tabSource)
        {
            txtSource.Focus();
        }else if (tabControl.SelectedTab == tabResult)
		{
			txtResult.Focus();
		}
        LoadDisplayedParameterData();
    }
	private void RefreshParameterList()
	{
		XmlDocument xsltDoc = LoadXslt();
		parameterListUpdater.Refresh(xsltDoc);
	}
    private void cboTraceLevel_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (ModelContent is XslRule rule)
        {
	        ModelComboBox comboBox = sender as ModelComboBox;
	        var traceLevelBefore = rule.TraceLevel; 
	        rule.TraceLevel = (Trace)comboBox.SelectedItem;
	        if (traceLevelBefore != rule.TraceLevel)
	        {
				IsDirty = true;
	        }
        }
    }
}
internal class ParameterListUpdater
{
	private readonly MemoryListBox parameterList;
	private IDictionary<string, string> aliasToNameSpaceDict;
	public ParameterListUpdater(MemoryListBox parameterList)
	{
		this.parameterList = parameterList;
	}
	public void Refresh(XmlDocument xsltDoc)
	{
		if (xsltDoc == null)
		{
			aliasToNameSpaceDict = new Dictionary<string, string>();
			parameterList.Items.Clear();
			return;
		}
		
		UpdateNameSpaceDictionary(xsltDoc);
        var newParamNodes = XmlTools.ResolveTransformationParameterElements(xsltDoc);
		RemoveParametersIfNotIn(newParamNodes);
		AddNewParametersIfNotAlredyInList(newParamNodes);
	}
	private void UpdateNameSpaceDictionary(XmlDocument xsltDoc)
	{
		XPathDocument x = new XPathDocument(new StringReader(xsltDoc.InnerXml));
		XPathNavigator navigator = x.CreateNavigator();
		navigator.MoveToFollowing(XPathNodeType.Element);
		aliasToNameSpaceDict = navigator
			.GetNamespacesInScope(XmlNamespaceScope.All)
			.Invert();
	}
	private void RemoveParametersIfNotIn(IList<XmlElement> newParamNodes)
	{
		List<string> newParNames = newParamNodes
			.Select(node => node.Attributes["name"].Value)
			.ToList();
		
		parameterList.Items
			.Cast<ParameterData>()
			.Where(parData => !newParNames.Contains(parData.Name))
			.ToList()
			.ForEach(item => parameterList.RemoveAndKeepSomeSelected(item));
	}
	private void AddNewParametersIfNotAlredyInList(IList<XmlElement> paramNodes)
	{
		List<string> oldParNames = parameterList.Items
			.Cast<ParameterData>()
			.Select(parData => parData.Name)
			.ToList();
		
		paramNodes
			.Where(paramNode =>
				!oldParNames.Contains(paramNode.Attributes["name"].Value))
			.ForEach(paramNode =>
				parameterList.Items.Add(NodeToParamData(paramNode)));
	}
	private ParameterData NodeToParamData(XmlNode parameterNode)
	{
        string name = parameterNode.Attributes["name"].Value;
        try
        {
            string asPrefix = aliasToNameSpaceDict[XmlTools.AsNameSpace];
            string typeAttribute = $"{asPrefix}:DataType";
            string type = parameterNode.Attributes[typeAttribute]?.Value;
            return new ParameterData(name, type);
        }catch (KeyNotFoundException)
        {
            return new ParameterData(name, null);
        }
	}
}
internal class ParameterData
{
    public string Name { get; }
    public string Text { get; set; } = "";
    public OrigamDataType Type { get; set; } = OrigamDataType.String;
    public ParameterData(string name,string type)
    {
	    this.Name = name;
	    Type = StringTypeToParameterDataType(type);
    }
    private OrigamDataType StringTypeToParameterDataType(string type)
    {
	    if(type == null) return OrigamDataType.String;
	    return Enum.GetValues(typeof(OrigamDataType))
		    .Cast<OrigamDataType?>()
		    .FirstOrDefault(origamType => origamType.ToString() == type)
		    ?? throw new ArgumentException($"parameter type {type} is not OrigamDataType.");
    }
    public object Value  {
		get
		{
			if (Type == OrigamDataType.Xml)
			{
				return new XmlContainer(Text);
			} else
			{
				Type systemType = DatasetGenerator.ConvertDataType(Type);
				return DatasetTools.ConvertValue(Text, systemType);
			}
		}
	}
    public override string ToString() => Name;
}
