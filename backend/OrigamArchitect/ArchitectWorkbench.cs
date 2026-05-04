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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CSharpFunctionalExtensions;
using JR.Utils.GUI.Forms;
using MoreLinq;
using Origam;
using Origam.DA.ObjectPersistence;
using Origam.DA.Service;
using Origam.DA.Service.MetaModelUpgrade;
using Origam.Excel;
using Origam.Extensions;
using Origam.Gui.UI;
using Origam.Gui.Win;
using Origam.Gui.Win.Commands;
using Origam.OrigamEngine;
using Origam.Rule;
using Origam.Schema;
using Origam.Schema.DeploymentModel;
using Origam.Schema.EntityModel;
using Origam.Schema.MenuModel;
using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.BaseComponents;
using Origam.Workbench.Commands;
using Origam.Workbench.Editors;
using Origam.Workbench.Pads;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;
using Origam.Workflow;
using Origam.Workflow.Gui.Win;
using OrigamArchitect.Commands;
using WeifenLuo.WinFormsUI.Docking;

namespace OrigamArchitect;

/// <summary>
/// Summary description for MainForm.
/// </summary>
internal class frmMain : Form, IWorkbench
{
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: MethodBase.GetCurrentMethod().DeclaringType
    );
    private CancellationTokenSource modelCheckCancellationTokenSource =
        new CancellationTokenSource();
    private readonly Dictionary<IToolStripContainer, List<ToolStrip>> loadedForms =
        new Dictionary<IToolStripContainer, List<ToolStrip>>();

    AsMenu _fileMenu = null;
    AsMenu _schemaMenu = null;
    AsMenu _viewMenu = null;
    AsMenu _toolsMenu = null;
    AsMenu _helpMenu = null;
    AsMenu _windowMenu = null;
    WorkbenchSchemaService _schema;
    private StatusBarService _statusBarService;
    bool closeAll = false;

    // Toolboxes
    SchemaBrowser _schemaBrowserPad;
#if ORIGAM_CLIENT
    AttachmentPad _attachmentPad;
    AuditLogPad _auditLogPad;
#else
    WorkflowWatchPad _workflowWatchPad;
    AttachmentPad _attachmentPad;
    AuditLogPad _auditLogPad;
    DocumentationPad _documentationPad;
    FindSchemaItemResultsPad _findSchemaItemResultsPad;
    FindRulesPad _findRulesPad;
    LogPad _logPad;
#endif
    PropertyPad _propertyPad;
    WorkflowPlayerPad _workflowPad;
    OutputPad _outputPad;
    ServerLogPad _serverLogPad;
    ExtensionPad _extensionPad;
    Hashtable _shortcuts = new Hashtable();
    private string _configFilePath;
    private FormWindowState lastWindowState;

    public const bool IgnoreHTTPSErrors = false;

    private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
    private System.Windows.Forms.StatusBar statusBar;
    private System.Windows.Forms.StatusBarPanel sbpText;
    private System.Windows.Forms.StatusBarPanel sbpMemory;
    private MenuStrip menuStrip;
    private LabeledToolStrip ducumentToolStrip;
    private LabeledToolStrip toolsToolStrip;
    private PictureBox logoPictureBox;
    private FlowLayoutPanel toolStripFlowLayoutPanel;
    private Panel toolStripPanel;
    private TableLayoutPanel rightToolsTripLayoutPanel;
    private ComboBox searchComboBox;

    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.Container components = null;

    public frmMain()
    {
        //
        // Required for Windows Form Designer support
        //
        InitializeComponent();
        WorkflowHost.DefaultHost.SupportsUI = true;
        dockPanel.Theme = new OrigamTheme();
        dockPanel.ShowDocumentIcon = false;
        string appDataPath = Environment.GetFolderPath(
            folder: Environment.SpecialFolder.ApplicationData
        );
        string origamPath = Path.Combine(path1: appDataPath, path2: "ORIGAM");
#if ORIGAM_CLIENT
        string configFileName = "ClientWorkspace.config";
#else
        string configFileName = "ArchitectWorkspace.config";
#endif
        if (!Directory.Exists(path: origamPath))
        {
            Directory.CreateDirectory(path: origamPath);
        }
        _configFilePath = Path.Combine(path1: origamPath, path2: configFileName);
        TypeDescriptor.AddAttributes(
            type: typeof(ISchemaItem),
            attributes: new EditorAttribute(
                type: typeof(ModelUIEditor),
                baseType: typeof(System.Drawing.Design.UITypeEditor)
            )
        );
        TypeDescriptor.AddAttributes(
            type: typeof(SchemaItemAncestorCollection),
            attributes: new EditorAttribute(
                type: typeof(SchemaItemAncestorCollectionEditor),
                baseType: typeof(System.Drawing.Design.UITypeEditor)
            )
        );
        TypeDescriptor.AddAttributes(
            type: typeof(Bitmap),
            attributes: new EditorAttribute(
                type: typeof(System.Drawing.Design.BitmapEditor),
                baseType: typeof(System.Drawing.Design.UITypeEditor)
            )
        );
        TypeDescriptor.AddAttributes(
            type: typeof(Byte[]),
            attributes: new EditorAttribute(
                type: typeof(Byte[]),
                baseType: typeof(FileSelectionUITypeEditor)
            )
        );
    }

    public void OpenForm(object owner, Hashtable parameters)
    {
        ExecuteSchemaItem cmd = new ExecuteSchemaItem();
        cmd.Owner = owner;

        foreach (DictionaryEntry entry in parameters)
        {
            cmd.Parameters.Add(key: entry.Key, value: entry.Value);
        }

        cmd.Run();
    }

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing: disposing);
    }

    private void OnChildFormToolStripsLoaded(object sender, EventArgs e)
    {
        UpdateToolStrips();
    }

    private void OnAllToolStripsRemovedFromALoadedForm(object sender, EventArgs e)
    {
        var toolStripContainer = (IToolStripContainer)sender;
        if (!loadedForms.ContainsKey(key: toolStripContainer))
        {
            return;
        }

        RemoveToolStrips(toolStripOwner: toolStripContainer);
        loadedForms.Remove(key: toolStripContainer);
    }

    private void OnToolStripsNeedUpdate(object sender, EventArgs args)
    {
        UpdateToolStrips();
    }

    private void CleanUpToolStripsWhenClosing(IViewContent closingForm)
    {
        if (closingForm is IToolStripContainer toolStripContainer)
        {
            RemoveToolStrips(toolStripOwner: toolStripContainer);
            loadedForms.Remove(key: toolStripContainer);
        }
    }

    private void UpdateToolStrips()
    {
        loadedForms.Keys.ForEach(action: RemoveToolStrips);
        if (ActiveDocument is IToolStripContainer toolStripContainer)
        {
            toolStripContainer.AllToolStripsRemoved -= OnAllToolStripsRemovedFromALoadedForm;
            toolStripContainer.AllToolStripsRemoved += OnAllToolStripsRemovedFromALoadedForm;
            toolStripContainer.ToolStripsNeedUpdate -= OnToolStripsNeedUpdate;
            toolStripContainer.ToolStripsNeedUpdate += OnToolStripsNeedUpdate;
            int widthOfDisplayedToolStrips = toolStripPanel
                .Controls.Cast<Control>()
                .Select(selector: x => x.Width)
                .Sum();
            int availableWidth = toolStripPanel.Width - widthOfDisplayedToolStrips;
            loadedForms[key: toolStripContainer] = toolStripContainer.GetToolStrips(
                maxWidth: availableWidth
            );
            loadedForms[key: toolStripContainer]
                .Where(predicate: ts => ts != null)
                .ForEach(action: ts => toolStripFlowLayoutPanel.Controls.Add(value: ts));
        }
    }

    private void RemoveToolStrips(IToolStripContainer toolStripOwner)
    {
        toolStripFlowLayoutPanel
            .Controls.OfType<LabeledToolStrip>()
            .Where(predicate: toolStrip => toolStrip.Owner == toolStripOwner)
            .ToList()
            .ForEach(action: toolStrip =>
                toolStripFlowLayoutPanel.Controls.Remove(value: toolStrip)
            );
    }

    #region Windows Form Designer generated code
    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources =
            new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
        this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
        this.statusBar = new System.Windows.Forms.StatusBar();
        this.sbpText = new System.Windows.Forms.StatusBarPanel();
        this.sbpMemory = new System.Windows.Forms.StatusBarPanel();
        this.menuStrip = new System.Windows.Forms.MenuStrip();
        this.ducumentToolStrip = new Origam.Gui.UI.LabeledToolStrip(null);
        this.toolsToolStrip = new Origam.Gui.UI.LabeledToolStrip(null);
        this.toolStripFlowLayoutPanel = new System.Windows.Forms.FlowLayoutPanel();
        this.logoPictureBox = new System.Windows.Forms.PictureBox();
        this.toolStripPanel = new System.Windows.Forms.Panel();
        this.rightToolsTripLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
        this.searchComboBox = new System.Windows.Forms.ComboBox();
        ((System.ComponentModel.ISupportInitialize)(this.sbpText)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.sbpMemory)).BeginInit();
        this.toolStripFlowLayoutPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
        this.toolStripPanel.SuspendLayout();
        this.rightToolsTripLayoutPanel.SuspendLayout();
        this.SuspendLayout();
        //
        // dockPanel
        //
        this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.dockPanel.Font = new System.Drawing.Font(
            "Tahoma",
            11F,
            System.Drawing.FontStyle.Regular,
            System.Drawing.GraphicsUnit.World
        );
        this.dockPanel.Location = new System.Drawing.Point(0, 119);
        this.dockPanel.Name = "dockPanel";
        this.dockPanel.ShowDocumentIcon = true;
        this.dockPanel.Size = new System.Drawing.Size(864, 464);
        this.dockPanel.TabIndex = 7;
        //
        // statusBar
        //
        this.statusBar.Font = new System.Drawing.Font(
            "Microsoft Sans Serif",
            8.25F,
            System.Drawing.FontStyle.Bold,
            System.Drawing.GraphicsUnit.Point,
            ((byte)(238))
        );
        this.statusBar.Location = new System.Drawing.Point(0, 583);
        this.statusBar.Name = "statusBar";
        this.statusBar.Panels.AddRange(
            new System.Windows.Forms.StatusBarPanel[] { this.sbpText, this.sbpMemory }
        );
        this.statusBar.ShowPanels = true;
        this.statusBar.Size = new System.Drawing.Size(864, 28);
        this.statusBar.TabIndex = 9;
        //
        // sbpText
        //
        this.sbpText.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
        this.sbpText.Name = "sbpText";
        this.sbpText.Width = 833;
        //
        // sbpMemory
        //
        this.sbpMemory.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
        this.sbpMemory.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Contents;
        this.sbpMemory.Name = "sbpMemory";
        this.sbpMemory.Width = 10;
        //
        // menuStrip
        //
        this.menuStrip.ImageScalingSize = new System.Drawing.Size(20, 20);
        this.menuStrip.Location = new System.Drawing.Point(0, 0);
        this.menuStrip.Name = "menuStrip";
        this.menuStrip.Size = new System.Drawing.Size(864, 24);
        this.menuStrip.TabIndex = 1;
        this.menuStrip.Text = "menuStrip1";
        //
        // ducumentToolStrip
        //
        this.ducumentToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
        this.ducumentToolStrip.Location = new System.Drawing.Point(0, 0);
        this.ducumentToolStrip.MinimumSize = new System.Drawing.Size(0, 95);
        this.ducumentToolStrip.Name = "ducumentToolStrip";
        this.ducumentToolStrip.Size = new System.Drawing.Size(111, 95);
        this.ducumentToolStrip.TabIndex = 0;
        this.ducumentToolStrip.Text = strings.DocumentToolStripText;
        //
        // toolsToolStrip
        //
        this.toolsToolStrip.ImageScalingSize = new System.Drawing.Size(24, 24);
        this.toolsToolStrip.Location = new System.Drawing.Point(0, 0);
        this.toolsToolStrip.MinimumSize = new System.Drawing.Size(0, 95);
        this.toolsToolStrip.Name = "toolsToolStrip";
        this.toolsToolStrip.Size = new System.Drawing.Size(111, 95);
        this.toolsToolStrip.TabIndex = 0;
        this.toolsToolStrip.Text = strings.ToolsToolStripText;
        //
        // toolStripFlowLayoutPanel
        //
        this.toolStripFlowLayoutPanel.AutoSize = true;
        this.toolStripFlowLayoutPanel.Controls.Add(this.ducumentToolStrip);
        this.toolStripFlowLayoutPanel.Controls.Add(this.toolsToolStrip);
        this.toolStripFlowLayoutPanel.Dock = System.Windows.Forms.DockStyle.Left;
        this.toolStripFlowLayoutPanel.Location = new System.Drawing.Point(0, 0);
        this.toolStripFlowLayoutPanel.MaximumSize = new System.Drawing.Size(10000, 95);
        this.toolStripFlowLayoutPanel.MinimumSize = new System.Drawing.Size(100, 95);
        this.toolStripFlowLayoutPanel.Name = "toolStripFlowLayoutPanel";
        this.toolStripFlowLayoutPanel.Size = new System.Drawing.Size(222, 95);
        this.toolStripFlowLayoutPanel.TabIndex = 0;
        this.toolStripFlowLayoutPanel.WrapContents = false;
        //
        // logoPictureBox
        //
        this.logoPictureBox.Dock = System.Windows.Forms.DockStyle.Bottom;
        this.logoPictureBox.Image = (
            (System.Drawing.Image)(resources.GetObject("logoPictureBox.Image"))
        );
        this.logoPictureBox.Location = new System.Drawing.Point(3, 12);
        this.logoPictureBox.Name = "logoPictureBox";
        this.logoPictureBox.Padding = new System.Windows.Forms.Padding(0, 0, 5, 0);
        this.logoPictureBox.Size = new System.Drawing.Size(194, 32);
        this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
        this.logoPictureBox.TabIndex = 1;
        this.logoPictureBox.TabStop = false;
        //
        // toolStripPanel
        //
        this.toolStripPanel.Controls.Add(this.rightToolsTripLayoutPanel);
        this.toolStripPanel.Controls.Add(this.toolStripFlowLayoutPanel);
        this.toolStripPanel.Dock = System.Windows.Forms.DockStyle.Top;
        this.toolStripPanel.Location = new System.Drawing.Point(0, 24);
        this.toolStripPanel.MaximumSize = new System.Drawing.Size(10000, 95);
        this.toolStripPanel.MinimumSize = new System.Drawing.Size(150, 95);
        this.toolStripPanel.Name = "toolStripPanel";
        this.toolStripPanel.Size = new System.Drawing.Size(864, 95);
        this.toolStripPanel.TabIndex = 9;
        //
        // rightToolsTripLayoutPanel
        //
        this.rightToolsTripLayoutPanel.ColumnCount = 1;
        this.rightToolsTripLayoutPanel.ColumnStyles.Add(
            new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F)
        );
        this.rightToolsTripLayoutPanel.ColumnStyles.Add(
            new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F)
        );
        this.rightToolsTripLayoutPanel.ColumnStyles.Add(
            new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F)
        );
        this.rightToolsTripLayoutPanel.Controls.Add(this.logoPictureBox, 0, 0);
        this.rightToolsTripLayoutPanel.Controls.Add(this.searchComboBox, 0, 1);
        this.rightToolsTripLayoutPanel.Dock = System.Windows.Forms.DockStyle.Right;
        this.rightToolsTripLayoutPanel.Location = new System.Drawing.Point(664, 0);
        this.rightToolsTripLayoutPanel.Name = "rightToolsTripLayoutPanel";
        this.rightToolsTripLayoutPanel.RowCount = 2;
        this.rightToolsTripLayoutPanel.RowStyles.Add(
            new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F)
        );
        this.rightToolsTripLayoutPanel.RowStyles.Add(
            new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F)
        );
        this.rightToolsTripLayoutPanel.Size = new System.Drawing.Size(200, 95);
        this.rightToolsTripLayoutPanel.TabIndex = 2;
        //
        // searchComboBox
        //
        this.searchComboBox.Dock = System.Windows.Forms.DockStyle.Top;
        this.searchComboBox.FormattingEnabled = true;
        this.searchComboBox.Location = new System.Drawing.Point(3, 50);
        this.searchComboBox.Name = "searchComboBox";
        this.searchComboBox.Size = new System.Drawing.Size(194, 24);
        this.searchComboBox.TabIndex = 2;
        this.searchComboBox.TabStop = false;
        this.searchComboBox.KeyDown += searchBox_KeyDown;
        this.searchComboBox.Text = "Search";
        //
        // frmMain
        //
        this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
        this.BackColor = System.Drawing.SystemColors.Control;
        this.ClientSize = new System.Drawing.Size(864, 611);
        this.Controls.Add(this.dockPanel);
        this.Controls.Add(this.toolStripPanel);
        this.Controls.Add(this.menuStrip);
        this.Controls.Add(this.statusBar);
        this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
        this.IsMdiContainer = true;
        this.MainMenuStrip = this.menuStrip;
        this.Name = "frmMain";
        this.Text = "ORIGAM Architect";
        this.Closing += new System.ComponentModel.CancelEventHandler(this.frmMain_Closing);
        this.Load += new System.EventHandler(this.frmMain_Load);
        ((System.ComponentModel.ISupportInitialize)(this.sbpText)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.sbpMemory)).EndInit();
        this.toolStripFlowLayoutPanel.ResumeLayout(false);
        this.toolStripFlowLayoutPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
        this.toolStripPanel.ResumeLayout(false);
        this.toolStripPanel.PerformLayout();
        this.rightToolsTripLayoutPanel.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();
    }
    #endregion
    internal bool AdministratorMode { get; set; } = false;
    public bool PopulateEmptyDatabaseOnLoad { get; set; } = true;
    public bool ApplicationDataDisconnectedMode { get; set; } = false;

    public void UpdateToolbar()
    {
        if (this.Disposing)
        {
            return;
        }

        foreach (object item in ducumentToolStrip.Items)
        {
            (item as IStatusUpdate)?.UpdateItemsToDisplay();
        }
        foreach (object item in toolsToolStrip.Items)
        {
            (item as IStatusUpdate)?.UpdateItemsToDisplay();
        }
    }

    private void PrepareMenuBars()
    {
        ducumentToolStrip.ItemClicked += toolStrip_ItemClicked;
        toolsToolStrip.ItemClicked += toolStrip_ItemClicked;
#if ORIGAM_CLIENT
        _fileMenu = new AsMenu(caller: this, text: strings.File_Menu);
        _viewMenu = new AsMenu(caller: this, text: strings.View_Menu);
        _helpMenu = new AsMenu(caller: this, text: strings.Help_Menu);
        _windowMenu = new AsMenu(caller: this, text: strings.Window_Menu);
        menuStrip.Items.Add(value: _fileMenu);
        menuStrip.Items.Add(value: _viewMenu);
        menuStrip.Items.Add(value: _windowMenu);
        menuStrip.Items.Add(value: _helpMenu);
#else
        _fileMenu = new AsMenu(this, strings.File_Menu);
        _schemaMenu = new AsMenu(this, strings.Model_Menu);
        _viewMenu = new AsMenu(this, strings.View_Menu);
        _toolsMenu = new AsMenu(this, strings.Tools_Menu);
        _helpMenu = new AsMenu(this, strings.Help_Menu);
        _windowMenu = new AsMenu(this, strings.Window_Menu);
        menuStrip.Items.Add(_fileMenu);
        menuStrip.Items.Add(_viewMenu);
        menuStrip.Items.Add(_schemaMenu);
        menuStrip.Items.Add(_toolsMenu);
        menuStrip.Items.Add(_windowMenu);
        menuStrip.Items.Add(_helpMenu);
#endif
    }

    private void FinishMenuBars()
    {
        UpdateMenu();
        UpdateToolbar();
    }

    private void CreateMainMenu()
    {
        ducumentToolStrip.Items.Clear();
        toolsToolStrip.Items.Clear();
        _fileMenu.Clear();
        _shortcuts.Clear();
        _schemaMenu?.Clear();
        _viewMenu.Clear();
        _toolsMenu?.Clear();
        _helpMenu.Clear();
        _windowMenu?.Clear();
        CreateHelpMenu();
        CreateFileMenu();
        CreateWindowMenu();
        CreateViewMenuDisconnected();
        CreateProcessBrowserContextMenu();
        foreach (object o in menuStrip.Items)
        {
            (o as AsMenu)?.PopulateMenu();
        }
    }

    private void UnloadMainMenu()
    {
        CreateMainMenu();
        UpdateMenu();
        UpdateToolbar();
    }

    private void CreateMainMenuConnect()
    {
#if ORIGAM_CLIENT
        CreateViewMenu();
#else
        CreateViewMenu();
        CreateModelMenu();
        CreateToolsMenu();
        CreateModelBrowserContextMenu();
#endif
        foreach (object o in menuStrip.Items)
        {
            (o as AsMenu)?.PopulateMenu();
        }
        UpdateMenu();
    }

    private void CreateProcessBrowserContextMenu()
    {
        // context menu for workflowPlayerPad
        AsContextMenu workflowPlayerContextMenu = new AsContextMenu(caller: this);
        AsMenuCommand mnuMakeWorkflowRecurring = new AsMenuCommand(
            label: strings.Retry_MenuCommand
        );
        mnuMakeWorkflowRecurring.Command = new Commands.MakeWorkflowRecurring();
        mnuMakeWorkflowRecurring.Command.Owner = mnuMakeWorkflowRecurring;
        mnuMakeWorkflowRecurring.Image = Images.RecurringWorkflow;
        mnuMakeWorkflowRecurring.Click += MenuItemClick;
        workflowPlayerContextMenu.AddSubItem(subItem: mnuMakeWorkflowRecurring);
        _workflowPad.ebrSchemaBrowser.ContextMenuStrip = workflowPlayerContextMenu;
    }

    private void CreateModelBrowserContextMenu()
    {
        IList<ToolStripItem> clonedIems = CloneToolStripItems(items: _schemaMenu.SubItems);

        AsContextMenu schemaContextMenu = new AsContextMenu(caller: this);
        schemaContextMenu.AddSubItems(newItems: clonedIems);

        _schema.SchemaContextMenu = schemaContextMenu;
    }

    private IList<ToolStripItem> CloneToolStripItems(IList<ToolStripItem> items)
    {
        var clonedItems = new List<ToolStripItem>();
        foreach (var item in items)
        {
            switch (item)
            {
                case AsMenuCommand command:
                {
                    clonedItems.Add(item: new AsMenuCommand(other: command));
                    break;
                }

                case ToolStripSeparator _:
                {
                    clonedItems.Add(item: new ToolStripSeparator());
                    break;
                }

                default:
                {
                    throw new Exception(
                        message: $"Need a copy constructor for {item.GetType()} here."
                    );
                }
            }
        }
        return clonedItems;
    }

    private void CreateFileMenu()
    {
#if ORIGAM_CLIENT
        AsMenuCommand mnuSave = CreateMenuItem(
            text: strings.Save_MenuCommand,
            command: new SaveContent(),
            image: Images.Save,
            shortcut: Keys.Control | Keys.S,
            parentMenu: _fileMenu
        );
        AsMenuCommand mnuRefresh = CreateMenuItem(
            text: strings.Refresh_MenuCommand,
            command: new RefreshContent(),
            image: Images.Refresh,
            shortcut: Keys.Control | Keys.R,
            parentMenu: _fileMenu
        );
        AsMenuCommand mnuFinishWorkflowTask = CreateMenuItem(
            text: strings.FinishTask_MenuCommand,
            command: new Commands.FinishWorkflowTask(),
            image: Images.Forward,
            shortcut: Keys.F5,
            parentMenu: _fileMenu
        );
        _fileMenu.SubItems.Add(item: CreateSeparator());
        CreateMenuItem(
            text: strings.Connect_MenuItem,
            command: new ConnectRepository(),
            image: Images.Home,
            shortcut: Keys.None,
            parentMenu: _fileMenu
        );
        CreateMenuItem(
            text: strings.Disconnect_MenuItem,
            command: new DisconnectRepository(),
            image: null,
            shortcut: Keys.None,
            parentMenu: _fileMenu
        );
        if (AdministratorMode)
        {
            CreateMenuItem(
                text: strings.ConnectionConfig_MenuItem,
                command: new Origam.Workbench.Commands.EditConfiguration(),
                image: Images.ConnectionConfiguration,
                shortcut: Keys.None,
                parentMenu: _fileMenu
            );
            _fileMenu.SubItems.Add(item: CreateSeparator());

            CreateMenuItem(
                text: strings.RunRefreshActions_MenuItem,
                command: new DeployVersion(),
                image: null,
                shortcut: Keys.None,
                parentMenu: _fileMenu
            );
        }
        _fileMenu.SubItems.Add(item: CreateSeparator());

        CreateMenuItem(
            text: strings.Exit_MenuItem,
            command: new ExitWorkbench(),
            image: null,
            shortcut: Keys.None,
            parentMenu: _fileMenu
        );

        ducumentToolStrip.Items.Add(
            value: CreateButtonFromMenu(menu: mnuSave, newImage: ImageRes.Save)
        );
        ducumentToolStrip.Items.Add(
            value: CreateButtonFromMenu(menu: mnuRefresh, newImage: ImageRes.Refresh)
        );
        ducumentToolStrip.Items.Add(
            value: CreateButtonFromMenu(menu: mnuFinishWorkflowTask, newImage: ImageRes.FinishTask)
        );

        searchComboBox.Enabled = false;
        searchComboBox.Visible = false;
        rightToolsTripLayoutPanel.Controls.Remove(value: this.searchComboBox);
        rightToolsTripLayoutPanel.RowCount = 1;
        logoPictureBox.Dock = DockStyle.Fill;
#else
        CreateMenuItem(
            strings.NewProject_MenuItem,
            new Commands.CreateNewProject(),
            Images.New,
            Keys.None,
            _fileMenu
        );
        _fileMenu.SubItems.Add(CreateSeparator());

        AsMenuCommand mnuSave = CreateMenuItem(
            strings.Save_MenuCommand,
            new SaveContent(),
            Images.Save,
            Keys.Control | Keys.S,
            _fileMenu
        );
        AsMenuCommand mnuRefresh = CreateMenuItem(
            strings.Refresh_MenuCommand,
            new RefreshContent(),
            Images.Refresh,
            Keys.Control | Keys.R,
            _fileMenu
        );
        AsMenuCommand mnuFinishWorkflowTask = CreateMenuItem(
            strings.FinishTask_MenuCommand,
            new Commands.FinishWorkflowTask(),
            Images.Forward,
            Keys.F5,
            _fileMenu
        );

        _fileMenu.SubItems.Add(CreateSeparator());
        CreateMenuItem(
            strings.Connect_MenuItem,
            new ConnectRepository(),
            Images.Home,
            Keys.None,
            _fileMenu
        );
        CreateMenuItem(
            strings.Disconnect_MenuItem,
            new DisconnectRepository(),
            null,
            Keys.None,
            _fileMenu
        );
        CreateMenuItem(
            strings.ConnectionConfig_MenuItem,
            new EditConfiguration(),
            Images.ConnectionConfiguration,
            Keys.None,
            _fileMenu
        );
        AsMenuCommand mnuServerRestart = CreateMenuItem(
            strings.SetServerRestart_MenuItem,
            new Commands.SetServerRestart(),
            Images.RestartServer,
            Keys.None,
            _toolsMenu
        );
        using (SqlViewer vwr = new SqlViewer(null))
        {
            CreateMenuWithSubmenu(
                strings.SqlConsole_MenuItem,
                vwr.Icon.ToBitmap(),
                new ShowSqlConsoleMenuBuilder(),
                _toolsMenu
            );
        }
        _fileMenu.SubItems.Add(CreateSeparator());
        CreateMenuItem(
            strings.RunUpdateScripts_MenuItem,
            new DeployVersion(),
            null,
            Keys.None,
            _fileMenu
        );

        _fileMenu.SubItems.Add(CreateSeparator());

        CreateMenuItem(strings.Exit_MenuItem, new ExitWorkbench(), null, Keys.None, _fileMenu);
        ducumentToolStrip.Items.Add(CreateButtonFromMenu(mnuSave, ImageRes.Save));
        ducumentToolStrip.Items.Add(CreateButtonFromMenu(mnuRefresh, ImageRes.Refresh));
        ducumentToolStrip.Items.Add(
            CreateButtonFromMenu(mnuFinishWorkflowTask, ImageRes.FinishTask)
        );
        toolsToolStrip.Items.Add(CreateButtonFromMenu(mnuServerRestart, ImageRes.RestartServer));
#endif
    }

    private void CreateHelpMenu()
    {
#if ORIGAM_CLIENT
        CreateMenuItem(
            text: strings.About_MenuItem,
            command: new Commands.ViewAboutScreen(),
            image: null,
            shortcut: Keys.None,
            parentMenu: _helpMenu
        );
#else
        CreateMenuItem(strings.Help_MenuItem, new Commands.ShowHelp(), null, Keys.F1, _helpMenu);
        CreateMenuItem(
            strings.CommunityForums_MenuItem,
            new Commands.ShowCommunity(),
            null,
            Keys.None,
            _helpMenu
        );
        _helpMenu.SubItems.Add(CreateSeparator());
        CreateMenuItem(
            strings.About_MenuItem,
            new Commands.ViewAboutScreen(),
            null,
            Keys.None,
            _helpMenu
        );
#endif
    }

    private void CreateWindowMenu()
    {
#if ORIGAM_CLIENT
        CreateMenuItem(
            text: strings.Close_MenuItem,
            command: new Commands.CloseWindow(),
            image: null,
            shortcut: Keys.None,
            parentMenu: _windowMenu
        );
        CreateMenuItem(
            text: strings.CloseAll_MenuItem,
            command: new Commands.CloseAllWindows(),
            image: null,
            shortcut: Keys.None,
            parentMenu: _windowMenu
        );
        CreateMenuItem(
            text: strings.CloseAllButThis_MenuItem,
            command: new Commands.CloseAllButThis(),
            image: null,
            shortcut: Keys.None,
            parentMenu: _windowMenu
        );
#else
        CreateMenuItem(
            strings.Close_MenuItem,
            new Commands.CloseWindow(),
            null,
            Keys.None,
            _windowMenu
        );
        CreateMenuItem(
            strings.CloseAll_MenuItem,
            new Commands.CloseAllWindows(),
            null,
            Keys.None,
            _windowMenu
        );
        CreateMenuItem(
            strings.CloseAllButThis_MenuItem,
            new Commands.CloseAllButThis(),
            null,
            Keys.None,
            _windowMenu
        );
#endif
    }

    private void CreateViewMenuDisconnected()
    {
#if ORIGAM_CLIENT
        if (AdministratorMode)
        {
            CreateMenuItem(
                text: strings.Properties_MenuItem,
                command: new ViewPropertyPad(),
                image: Images.PropertyPad,
                shortcut: Keys.F4,
                parentMenu: _viewMenu
            );
            CreateMenuItem(
                text: strings.Output_MenuItem,
                command: new ViewOutputPad(),
                image: Images.Output,
                shortcut: Keys.None,
                parentMenu: _viewMenu
            );
        }
        CreateMenuItem(
            text: strings.Menu_MenuItem,
            command: new Commands.ViewProcessBrowserPad(),
            image: _workflowPad.Icon.ToBitmap(),
            shortcut: Keys.F2,
            parentMenu: _viewMenu
        );
        CreateMenuItem(
            text: strings.WorkQueue_MenuItem,
            command: new Commands.ViewWorkQueuePad(),
            image: null,
            shortcut: Keys.None,
            parentMenu: _viewMenu
        );
#else
        CreateMenuItem(
            strings.Properties_MenuItem,
            new ViewPropertyPad(),
            _propertyPad.Icon.ToBitmap(),
            Keys.F4,
            _viewMenu
        );
        CreateMenuItem(
            strings.Output_MenuItem,
            new ViewOutputPad(),
            _outputPad.Icon.ToBitmap(),
            Keys.None,
            _viewMenu
        );
        CreateMenuItem(
            strings.Log_MenuItem,
            new ViewLogPad(),
            _logPad.Icon.ToBitmap(),
            Keys.None,
            _viewMenu
        );
        CreateMenuItem(
            strings.ServerLog_MenuItem,
            new ViewServerLogPad(),
            _serverLogPad.Icon.ToBitmap(),
            Keys.None,
            _viewMenu
        );
        CreateMenuItem(
            strings.ModelBrowser_MenuItem,
            new ViewSchemaBrowserPad(),
            _schemaBrowserPad.Icon.ToBitmap(),
            Keys.F3,
            _viewMenu
        );
        CreateMenuItem(
            strings.WorkQueue_MenuItem,
            new Commands.ViewWorkQueuePad(),
            null,
            Keys.None,
            _viewMenu
        );
#endif
    }

    private void CreateViewMenu()
    {
#if ORIGAM_CLIENT
        CreateMenuItem(
            text: strings.Attachements_MenuItem,
            command: new ViewAttachmentPad(),
            image: Images.Attachment,
            shortcut: Keys.None,
            parentMenu: _viewMenu
        );
        CreateMenuItem(
            text: strings.AuditLog_MenuItem,
            command: new ViewAuditLogPad(),
            image: Images.History,
            shortcut: Keys.None,
            parentMenu: _viewMenu
        );
#else
        CreateMenuItem(
            strings.PackageBrowser_MenuItem,
            new ViewExtensionPad(),
            _extensionPad.Icon.ToBitmap(),
            Keys.None,
            _viewMenu
        );
        CreateMenuItem(
            strings.WorkflowWatch_MenuItem,
            new Commands.ViewWorkflowWatchPad(),
            _workflowWatchPad.Icon.ToBitmap(),
            Keys.None,
            _viewMenu
        );
        CreateMenuItem(
            strings.Documentation_MenuItem,
            new ViewDocumentationPad(),
            _documentationPad.Icon.ToBitmap(),
            Keys.None,
            _viewMenu
        );
        CreateMenuItem(
            strings.FindSchemaItemResults_MenuItem,
            new ViewFindSchemaItemResultsPad(),
            _findSchemaItemResultsPad.Icon.ToBitmap(),
            Keys.None,
            _viewMenu
        );
#endif
    }

    private void CreateModelMenu()
    {
        AsMenuCommand schemaNewMenu = CreateMenuWithSubmenu(
            name: strings.New_MenuItem,
            image: ImageRes.icon_new,
            builder: new SchemaItemEditorsMenuBuilder(),
            parentMenu: _schemaMenu
        );
        CreateMenuItem(
            text: strings.NewGroup_MenuItem,
            command: new AddNewGroup(),
            image: ImageRes.icon_new_group,
            shortcut: Keys.None,
            parentMenu: _schemaMenu
        );
        CreateMenuItem(
            text: strings.RepeatNew_MenuItem,
            command: new AddRepeatingSchemaItem(),
            image: ImageRes.icon_repeat_new,
            shortcut: Keys.F12,
            parentMenu: _schemaMenu
        );
        CreateMenuWithSubmenu(
            name: strings.Actions_MenuItem,
            image: ImageRes.icon_actions,
            builder: new SchemaActionsMenuBuilder(),
            parentMenu: _schemaMenu
        );
        CreateMenuWithSubmenu(
            name: strings.ConvertTo_MenuItem,
            image: ImageRes.icon_convert_to,
            builder: new SchemaItemConvertMenuBuilder(),
            parentMenu: _schemaMenu
        );
        CreateMenuItem(
            text: strings.MoveToPackage_MenuItem,
            command: new MoveToAnotherPackage(),
            image: ImageRes.icon_move_to_package,
            shortcut: Keys.None,
            parentMenu: _schemaMenu
        );

        _schemaMenu.SubItems.Add(item: CreateSeparator());
        CreateMenuItem(
            text: strings.ExpandAll,
            command: new ExpandAllActiveSchemaItem(),
            image: ImageRes.Arrow,
            shortcut: Keys.None,
            parentMenu: _schemaMenu
        );
        AsMenuCommand mnuEditSchemaItem = CreateMenuItem(
            text: strings.EditItem_MenuItem,
            command: new EditActiveSchemaItem(),
            image: ImageRes.icon_edit_item,
            shortcut: Keys.None,
            parentMenu: _schemaMenu
        );
        AsMenuCommand mnuDelete = CreateMenuItem(
            text: strings.Delete_MenuItem,
            command: new DeleteActiveNode(),
            image: ImageRes.icon_delete,
            shortcut: Keys.None,
            parentMenu: _schemaMenu
        );
        CreateMenuItem(
            text: strings.Execute_MenuItem,
            command: new ExecuteActiveSchemaItem(),
            image: ImageRes.icon_execute,
            shortcut: Keys.Control | Keys.X,
            parentMenu: _schemaMenu
        );
        _schemaMenu.SubItems.Add(item: CreateSeparator());

        CreateMenuItem(
            text: strings.FindDependencies_MenuItem,
            command: new ShowDependencies(),
            image: ImageRes.icon_find_dependencies,
            shortcut: Keys.None,
            parentMenu: _schemaMenu
        );
        CreateMenuItem(
            text: strings.FindReferences_MenuItem,
            command: new ShowUsage(),
            image: ImageRes.icon_find_references,
            shortcut: Keys.None,
            parentMenu: _schemaMenu
        );
        _schemaMenu.SubItems.Add(item: CreateSeparator());
        CreateMenuItem(
            text: strings.SourceXml_MenuItem,
            command: new ShowExplorerXml(),
            image: ImageRes.icon_show_in_explorer,
            shortcut: Keys.None,
            parentMenu: _schemaMenu
        );
        CreateMenuItem(
            text: strings.XmlConsole,
            command: new ShowConsoleXml(),
            image: ImageRes.icon_show_xml,
            shortcut: Keys.None,
            parentMenu: _schemaMenu
        );
        AsMenuCommand schemamenuGit = CreateMenuWithSubmenu(
            name: "Git",
            image: Images.Git,
            builder: new GitMenuBuilder(),
            parentMenu: _schemaMenu
        );
    }

    private void CreateToolsMenu()
    {
        CreateMenuItem(
            text: strings.DeploymentScriptGenerator_MenuItem,
            command: new Commands.ShowDbCompare(),
            image: Images.DeploymentScriptGenerator,
            shortcut: Keys.None,
            parentMenu: _toolsMenu
        );
        CreateMenuItem(
            text: strings.ShowWebApplication_MenuItem,
            command: new Commands.ShowWebApplication(),
            image: null,
            shortcut: Keys.None,
            parentMenu: _toolsMenu
        );
        CreateMenuItem(
            text: strings.GenerateGUID_MenuItem,
            command: new Commands.GenerateGuid(),
            image: null,
            shortcut: Keys.Control | Keys.Shift | Keys.G,
            parentMenu: _toolsMenu
        );
        CreateMenuItem(
            text: strings.DumpWindowXML_MenuItem,
            command: new Commands.DumpWindowXml(),
            image: null,
            shortcut: Keys.None,
            parentMenu: _toolsMenu
        );
        CreateMenuItem(
            text: strings.ShowTrace_MenuItem,
            command: new Commands.ShowTrace(),
            image: null,
            shortcut: Keys.Control | Keys.T,
            parentMenu: _toolsMenu
        );
        CreateMenuItem(
            text: strings.ShowRuleTrace_MenuItem,
            command: new Commands.ShowRuleTrace(),
            image: null,
            shortcut: Keys.None,
            parentMenu: _toolsMenu
        );
        CreateMenuItem(
            text: strings.ResetUserCache_MenuItem,
            command: new Commands.ResetUserCaches(),
            image: null,
            shortcut: Keys.None,
            parentMenu: _toolsMenu
        );
        CreateMenuItem(
            text: strings.RebuildLocalizationFiles_MenuItem,
            command: new Commands.GenerateLocalizationFile(),
            image: null,
            shortcut: Keys.None,
            parentMenu: _toolsMenu
        );
    }

    private void UpdateMenu()
    {
        if (this.Disposing)
        {
            return;
        }

        foreach (object item in menuStrip.Items)
        {
            if (item is IStatusUpdate)
            {
                (item as IStatusUpdate).UpdateItemsToDisplay();
            }
        }
    }

    private ToolStripSeparator CreateSeparator()
    {
        return new ToolStripSeparator();
    }

    private AsMenuCommand CreateMenuWithSubmenu(
        string name,
        Image image,
        ISubmenuBuilder builder,
        AsMenu parentMenu
    )
    {
        AsMenuCommand result = new AsMenuCommand(label: name);
        if (image != null)
        {
            result.Image = image;
        }
        result.SubItems.Add(item: builder);
        parentMenu.SubItems.Add(item: result);
        return result;
    }

    private AsButtonCommand CreateButtonFromMenu(AsMenuCommand menu)
    {
        return new AsButtonCommand(label: menu.Description)
        {
            Image = menu.Image,
            Command = menu.Command,
        };
    }

    private AsButtonCommand CreateButtonFromMenu(AsMenuCommand menu, Image newImage)
    {
        return new AsButtonCommand(label: menu.Description)
        {
            Image = newImage,
            Command = menu.Command,
        };
    }

    private AsMenuCommand CreateMenuItem(string text, ICommand command, Image image)
    {
        AsMenuCommand menuItem = new AsMenuCommand(label: text, menuCommand: command);
        menuItem.Click += MenuItemClick;

        if (image != null)
        {
            menuItem.Image = image;
        }
        return menuItem;
    }

    private AsMenuCommand CreateMenuItem(
        string text,
        ICommand command,
        Image image,
        Keys shortcut,
        AsMenu parentMenu
    )
    {
        AsMenuCommand result = CreateMenuItem(text: text, command: command, image: image);
        if (shortcut != Keys.None)
        {
            result.ShortcutKeys = shortcut;
            _shortcuts[key: shortcut] = result;
        }
        parentMenu.SubItems.Add(item: result);
        return result;
    }

    private IDockContent GetContentFromPersistString(string persistString)
    {
        foreach (IPadContent pad in this.PadContentCollection)
        {
            if (persistString == pad.GetType().ToString())
            {
                return pad as DockContent;
            }
        }

        return null;
    }

    private void LoadWorkspace()
    {
        dockPanel.SuspendLayout(allWindows: true);
        try
        {
            if (File.Exists(path: _configFilePath) && dockPanel.Contents.Count == 0)
            {
                dockPanel.LoadFromXml(
                    fileName: _configFilePath,
                    deserializeContent: GetContentFromPersistString
                );
            }
            else
            {
                // Default Workspace
#if ORIGAM_CLIENT
                new Commands.ViewProcessBrowserPad().Run();
#else
                new ViewExtensionPad().Run();
#endif
            }
        }
        finally
        {
            dockPanel.ResumeLayout(performLayout: true, allWindows: true);
        }
    }

    private void frmMain_Closing(object sender, CancelEventArgs e)
    {
        if (!Disconnect())
        {
            e.Cancel = true;
        }
        else
        {
            e.Cancel = false;
        }
    }

    #region IWorkbench Members
    public bool IsConnected { get; private set; }
    public string Title
    {
        get => this.Text;
        set => this.Text = value;
    }
    public ViewContentCollection ViewContentCollection { get; } = new ViewContentCollection();
    public PadContentCollection PadContentCollection { get; } = new PadContentCollection();
    public IViewContent ActiveViewContent => this.dockPanel.ActiveContent as IViewContent;
    public IViewContent ActiveDocument => this.dockPanel.ActiveDocument as IViewContent;
    public int WorkflowFormsCount
    {
        get
        {
            int result = 0;
            foreach (IViewContent content in this.ViewContentCollection)
            {
                if (content is Origam.Workflow.WorkflowForm)
                {
                    result++;
                }
            }
            return result;
        }
    }

    public void ShowView(IViewContent content)
    {
        if (
            ConfigurationManager.GetActiveConfiguration() is OrigamSettings settings
            && settings.MaxOpenTabs > 0
            && this.dockPanel.DocumentsCount == settings.MaxOpenTabs
        )
        {
            throw new Exception(
                message: "Too many open documents. Please close some documents first."
            );
        }
        ViewContentCollection.Add(value: content);
        ((DockContent)content).Show(dockPanel: dockPanel);
        ((DockContent)content).Closed += DockContentClosed;
        OnViewOpened(e: new ViewContentEventArgs(content: content));
    }

    public void ShowPad(IPadContent content)
    {
        DockContent dock = content as DockContent;
        switch (dock.DockState)
        {
            case DockState.DockBottomAutoHide:
            {
                dock.DockState = DockState.DockBottom;
                break;
            }

            case DockState.DockLeftAutoHide:
            {
                dock.DockState = DockState.DockLeft;
                break;
            }

            case DockState.DockRightAutoHide:
            {
                dock.DockState = DockState.DockRight;
                break;
            }

            case DockState.DockTopAutoHide:
            {
                dock.DockState = DockState.DockTop;
                break;
            }
        }
        dock.Show(dockPanel: dockPanel);
    }

    public T GetPad<T>()
        where T : IPadContent
    {
        return (T)GetPad(type: typeof(T));
    }

    public IPadContent GetPad(Type type)
    {
        foreach (IPadContent pad in PadContentCollection)
        {
            // try the type itself
            if (pad.GetType() == type)
            {
                return pad;
            }
            if (type.IsInterface)
            {
                // try all interfaces
                foreach (Type interfaceType in pad.GetType().GetInterfaces())
                {
                    if (interfaceType == type)
                    {
                        return pad;
                    }
                }
            }
        }
        return null;
    }

    public void CloseContent(IViewContent content)
    {
        ((DockContent)content).Close();
    }

    public void CloseAllViews(IViewContent except)
    {
        foreach (DockContent content in this.dockPanel.DocumentsToArray())
        {
            if (except == null || !except.Equals(obj: content))
            {
                content.Close();
                Application.DoEvents();
            }
        }
    }

    public void CloseAllViews()
    {
        CloseAllViews(except: null);
    }

    public void RedrawAllComponents()
    {
        // TODO:  Add frmMain.RedrawAllComponents implementation
    }

    public event Origam.UI.ViewContentEventHandler ViewOpened;
    public event Origam.UI.ViewContentEventHandler ViewClosed;
    public event System.EventHandler ActiveWorkbenchWindowChanged;
    #endregion
    protected override void OnResizeEnd(EventArgs e)
    {
        UpdateToolStrips();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e: e);
        if (WindowState != lastWindowState)
        {
            lastWindowState = WindowState;
            UpdateToolStrips();
        }
    }

    protected virtual void OnViewOpened(ViewContentEventArgs e)
    {
        e.Content.DirtyChanged += Content_DirtyChanged;
        UpdateToolbar();
        HandleAttachments();
        if (ActiveMdiChild is IToolStripContainer tsContainer)
        {
            tsContainer.ToolStripsLoaded += OnChildFormToolStripsLoaded;
        }
        ViewOpened?.Invoke(sender: this, e: e);
    }

    protected virtual void OnViewClosed(ViewContentEventArgs e)
    {
        if (e.Content is IRecordReferenceProvider)
        {
            (e.Content as IRecordReferenceProvider).RecordReferenceChanged -=
                AttachmentDocument_ParentIdChanged;
        }

        this.ViewContentCollection.Remove(value: e.Content);
        UpdateToolbar();
        HandleAttachments();
        ViewClosed?.Invoke(sender: this, e: e);
        CleanUpToolStripsWhenClosing(closingForm: e.Content);
    }

    void OnActiveWindowChanged(object sender, EventArgs e)
    {
        UpdateToolbar();
        HandleAttachments();
        if (!closeAll && ActiveWorkbenchWindowChanged != null)
        {
            ActiveWorkbenchWindowChanged(sender: this, e: e);
        }
        if (ActiveDocument is AbstractEditor editor)
        {
            editor.ActionsBuilder = new SchemaActionsMenuBuilder();
            editor.NewElementsBuilder = new SchemaItemEditorsMenuBuilder(showDialog: true);
        }
        UpdateToolStrips();
    }

    private void HandleAttachments()
    {
        if (this.ActiveDocument is IRecordReferenceProvider)
        {
            // Last window closed
            if (this.ViewContentCollection.Count == 0)
            {
                AttachmentDocument_ParentIdChanged(
                    sender: this,
                    mainEntityId: Guid.Empty,
                    mainRecordId: Guid.Empty,
                    childReferences: new Hashtable()
                );
                return;
            }
            // deactivate all attachment notifications
            foreach (IViewContent content in this.ViewContentCollection)
            {
                if (content is IRecordReferenceProvider)
                {
                    (content as IRecordReferenceProvider).RecordReferenceChanged -=
                        AttachmentDocument_ParentIdChanged;
                }
            }
            // Check if there is still an active document
            if (this.ActiveDocument == null)
            {
                AttachmentDocument_ParentIdChanged(
                    sender: this,
                    mainEntityId: Guid.Empty,
                    mainRecordId: Guid.Empty,
                    childReferences: new Hashtable()
                );
            }
            else
            {
                // fire current parent id
                AttachmentDocument_ParentIdChanged(
                    sender: this,
                    mainEntityId: (this.ActiveDocument as IRecordReferenceProvider).MainEntityId,
                    mainRecordId: (this.ActiveDocument as IRecordReferenceProvider).MainRecordId,
                    childReferences: (
                        this.ActiveDocument as IRecordReferenceProvider
                    ).ChildRecordReferences
                );

                // subscribe to notifications
                (this.ActiveDocument as IRecordReferenceProvider).RecordReferenceChanged +=
                    AttachmentDocument_ParentIdChanged;
            }
        }
        else
        {
            AttachmentDocument_ParentIdChanged(
                sender: this,
                mainEntityId: Guid.Empty,
                mainRecordId: Guid.Empty,
                childReferences: new Hashtable()
            );
        }
    }

    private void MenuItemClick(object sender, EventArgs e)
    {
        // User clicked the menu, so we run the command associated with the
        // menu item.
        UpdateMenu();
        UpdateToolbar();
        try
        {
            this.Cursor = Cursors.WaitCursor;
            if (sender is AsMenuCommand)
            {
                if (((AsMenuCommand)sender).IsEnabled)
                {
                    ((AsMenuCommand)sender).Command.Run();
                }
            }
            else if (sender is AsButtonCommand)
            {
                if (((AsButtonCommand)sender).IsEnabled)
                {
                    ((AsButtonCommand)sender).Command.Run();
                }
            }
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(
                owner: this,
                text: ex.Message,
                caption: strings.GenericError_Title,
                exception: ex
            );
            this.Cursor = Cursors.Default;
        }
        finally
        {
            this.Cursor = Cursors.Default;
        }
    }

    private void DockContentClosed(object sender, EventArgs e)
    {
        try
        {
            OnViewClosed(e: new ViewContentEventArgs(content: sender as IViewContent));
            (sender as DockContent).Closed -= DockContentClosed;

            (sender as DockContent).DockPanel = null;
            _statusBarService.SetStatusMemory(bytes: GC.GetTotalMemory(forceFullCollection: true));
        }
        catch { }
    }

    public void Connect()
    {
        Connect(configurationName: null);
        SubscribeToPersistenceServiceEvents();
    }

    private void SubscribeToPersistenceServiceEvents()
    {
        var currentPersistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        if (currentPersistenceService is FilePersistenceService filePersistService)
        {
            filePersistService.ReloadNeeded += OnFilePersistServiceReloadRequested;
        }
    }

    void OnFilePersistServiceReloadRequested(object sender, FileSystemChangeEventArgs args)
    {
        FilePersistenceService filePersistenceService = (FilePersistenceService)sender;
        FilePersistenceProvider filePersistenceProvider = (FilePersistenceProvider)
            filePersistenceService.SchemaProvider;
        bool reloadConfirmed = true;
        if (ShouldRaiseWarning(filePersistProvider: filePersistenceProvider, file: args.File))
        {
            reloadConfirmed = this.RunWithInvoke(func: () => ConfirmReload(args: args));
        }
        if (reloadConfirmed)
        {
            Maybe<XmlLoadError> mayBeError = TryLoadModelFiles(
                filePersistService: filePersistenceService
            );
            if (mayBeError.HasValue)
            {
                this.RunWithInvoke(func: Disconnect);
            }
            else
            {
                this.RunWithInvokeAsync(action: () =>
                    UpdateUIAfterReload(
                        filePersistenceProvider: filePersistenceProvider,
                        args: args
                    )
                );
            }
            this.RunWithInvoke(action: RunBackgroundInitializationTasks);
        }
    }

    private bool ConfirmReload(FileSystemChangeEventArgs args)
    {
        // try/catch block for unhandled exceptions
        // main try/catch block doesn't show error message
        // and leaves application in unstable state
        try
        {
            DialogResult dialogResult = FlexibleMessageBox.Show(
                owner: this,
                text: $"Model file changes detected!{Environment.NewLine}{Environment.NewLine}{args}.{Environment.NewLine}{Environment.NewLine}Do you want to reload the model?",
                caption: "Changes in Model Directory Detected",
                buttons: MessageBoxButtons.YesNo
            );
            return dialogResult == DialogResult.Yes;
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(
                owner: null,
                text: ex.Message,
                caption: strings.GenericError_Title,
                exception: ex
            );
        }
        return false;
    }

    private void UpdateUIAfterReload(
        FilePersistenceProvider filePersistenceProvider,
        FileSystemChangeEventArgs args
    )
    {
        // try/catch block for unhandled exceptions
        // main try/catch block doesn't show error message
        // and leaves application in unstable state
        try
        {
            GetPad<SchemaBrowser>()?.EbrSchemaBrowser.ReloadTreeAndRestoreExpansionState();
            GetPad<ExtensionPad>()?.LoadPackages();
            GetPad<FindSchemaItemResultsPad>()?.Clear();
            GetPad<DocumentationPad>()?.Reload();
            ReloadOpenWindows(filePersistenceProvider: filePersistenceProvider);
        }
        catch (Exception ex)
        {
            AsMessageBox.ShowError(
                owner: null,
                text: ex.Message,
                caption: strings.GenericError_Title,
                exception: ex
            );
        }
    }

    private bool ShouldRaiseWarning(FilePersistenceProvider filePersistProvider, FileInfo file)
    {
        return ViewContentCollection
            .Cast<IViewContent>()
            .Select(selector: view =>
                filePersistProvider.FindPersistedObjectInfo(id: view.DisplayedItemId)
            )
            .Where(predicate: objInfo => objInfo != null)
            .Any(predicate: objInfo => objInfo.OrigamFile.Path.EqualsTo(file: file));
    }

    private Maybe<XmlLoadError> TryLoadModelFiles(FilePersistenceService filePersistService)
    {
        Maybe<XmlLoadError> maybeError = filePersistService.Reload();
        if (maybeError.HasNoValue)
        {
            return null;
        }

        XmlLoadError error = maybeError.Value;
        this.RunWithInvoke(func: () => MessageBox.Show(owner: this, text: error.Message));
        return maybeError;
    }

    private bool TryHandleOldVersionFound(FilePersistenceService filePersistService, string message)
    {
        DialogResult updateVersionsResult = MessageBox.Show(
            owner: this,
            text: $"{message}{Environment.NewLine}Do you want to upgrade this file and all other files with old versions?",
            caption: "Old Meta Model Version Detected",
            buttons: MessageBoxButtons.YesNo
        );
        if (updateVersionsResult != DialogResult.Yes)
        {
            return false;
        }

        MessageBox.Show(
            owner: this,
            text: $"This functionality has not been implemented yet.{Environment.NewLine}No files will be reloaded!"
        );
        Maybe<XmlLoadError> reloadError = filePersistService.Reload();
        if (reloadError.HasValue)
        {
            throw new NotImplementedException();
        }
        return false;
    }

    private void ReloadOpenWindows(FilePersistenceProvider filePersistenceProvider)
    {
        List<IViewContent> openViewList = ViewContentCollection
            .Cast<IViewContent>()
            .Where(predicate: x =>
                CanBeReOpened(filePersistenceProvider: filePersistenceProvider, viewContent: x)
            )
            .ToList();

        IViewContent originallyActiveContent = (IViewContent)dockPanel.ActiveDocument;
        ReopenViewContents(
            filePersistenceProvider: filePersistenceProvider,
            openViewList: openViewList
        );
        if (originallyActiveContent is SchemaCompareEditor)
        {
            ActivateViewByType(typeToActivate: typeof(SchemaCompareEditor));
        }
        else
        {
            ActivateViewByContent(refContent: originallyActiveContent);
        }
    }

    private static bool CanBeReOpened(
        FilePersistenceProvider filePersistenceProvider,
        IViewContent viewContent
    )
    {
        if (viewContent is SchemaCompareEditor)
        {
            return true;
        }

        if (viewContent.LoadedObject == null)
        {
            return false;
        }

        IPersistent loadedObject = (IPersistent)viewContent.LoadedObject;
        return filePersistenceProvider.Has(id: loadedObject.Id);
    }

    private void ActivateViewByType(Type typeToActivate)
    {
        ViewContentCollection
            .Cast<IViewContent>()
            .Where(predicate: cont => cont.GetType() == typeToActivate)
            .Cast<DockContent>()
            .First()
            .Activate();
    }

    private void ActivateViewByContent(IViewContent refContent)
    {
        if (refContent == null)
        {
            return;
        }

        IBrowserNode2 loadedObject = (IBrowserNode2)refContent.LoadedObject;

        ViewContentCollection
            .Cast<IViewContent>()
            .Where(predicate: cont => cont.LoadedObject != null)
            .Where(predicate: cont =>
                ((IBrowserNode2)cont.LoadedObject).NodeId == loadedObject.NodeId
            )
            .Where(predicate: content => refContent.GetType() == content.GetType())
            .Cast<DockContent>()
            .SingleOrDefault()
            ?.Activate();
    }

    private void ReopenViewContents(
        FilePersistenceProvider filePersistenceProvider,
        List<IViewContent> openViewList
    )
    {
        foreach (IViewContent viewContent in openViewList)
        {
            Maybe<AbstractCommand> maybeReOpenCommand = GetCommandToReOpen(
                viewContent: viewContent
            );
            if (maybeReOpenCommand.HasNoValue)
            {
                continue;
            }
            CloseContent(content: viewContent);
            AbstractCommand reOpenCommand = maybeReOpenCommand.Value;
            if (viewContent is SchemaCompareEditor)
            {
                reOpenCommand.Run();
                continue;
            }
            IFilePersistent loadedObject = RefreshLoadedObject(
                filePersistenceProvider: filePersistenceProvider,
                viewContent: viewContent
            );
            reOpenCommand.Owner = loadedObject;
            reOpenCommand.Run();
        }
    }

    private static IFilePersistent RefreshLoadedObject(
        FilePersistenceProvider filePersistenceProvider,
        IViewContent viewContent
    )
    {
        var loadedObject = (IFilePersistent)viewContent.LoadedObject;
        if (loadedObject == null)
        {
            throw new Exception(message: "loadedObject not set");
        }

        filePersistenceProvider.RefreshInstance(persistentObject: loadedObject);
        return loadedObject;
    }

    private Maybe<AbstractCommand> GetCommandToReOpen(IViewContent viewContent)
    {
        if (viewContent is SchemaCompareEditor)
        {
            return new ShowDbCompare();
        }

        if (viewContent is AbstractViewContent)
        {
            return new EditSchemaItem();
        }

        if (viewContent is AsForm)
        {
            return new ExecuteSchemaItem();
        }

        return null;
    }

    public void Connect(string configurationName)
    {
        // Ask for configuration
        if (!LoadConfiguration(configurationName: configurationName))
        {
            return;
        }
        Application.DoEvents();
        foreach (DockContent content in this.dockPanel.Documents.ToList())
        {
            content.Close();
        }
        if (this.dockPanel.DocumentsCount > 0)
        {
            // could not close all the documents
            return;
        }
        try
        {
            _statusBarService.SetStatusText(text: strings.ConnectingToModelRepository_StatusText);
            InitPersistenceService();
            _schema.SchemaBrowser = _schemaBrowserPad;
            // Init services
            InitializeConnectedServices();
            // Initialize model-connected user interface
            InitializeConnectedPads();
            CreateMainMenuConnect();
            IsConnected = true;
#if !ORIGAM_CLIENT
            RunBackgroundInitializationTasks();
#endif
#if ORIGAM_CLIENT
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            try
            {
                _schema.LoadSchema(
                    schemaExtensionId: settings.DefaultSchemaExtensionId,
                    isInteractive: true
                );
            }
            catch (Exception ex)
            {
                this.Show();
                AsMessageBox.ShowError(
                    owner: this,
                    text: ex.Message,
                    caption: strings.LoadingModelErrorTitle,
                    exception: ex
                );

                Disconnect();
                return;
            }

            CheckModelRootPackageVersion();
#endif
            UpdateTitle();
        }
        finally
        {
            _statusBarService.SetStatusText(text: "");
            this.WindowState = FormWindowState.Maximized;
        }
#if ORIGAM_CLIENT
        Commands.ViewProcessBrowserPad cmd = new Commands.ViewProcessBrowserPad();
#else
        ViewExtensionPad cmd = new ViewExtensionPad();
#endif
        this.LoadWorkspace();
        cmd.Run();
    }

    private void SubscribeToUpgradeServiceEvents()
    {
        var metaModelUpgradeService =
            ServiceManager.Services.GetService<IMetaModelUpgradeService>();
        metaModelUpgradeService.UpgradeStarted += (sender, args) =>
        {
            Task.Run(action: () =>
            {
                using (
                    var form = new ModelUpgradeForm(
                        metaModelUpgradeService: metaModelUpgradeService
                    )
                )
                {
                    form.StartPosition = FormStartPosition.CenterScreen;
                    form.ShowDialog();
                }
            });
        };
    }

    public void RunBackgroundInitializationTasks()
    {
        var currentPersistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        if (!(currentPersistenceService is FilePersistenceService))
        {
            return;
        }

        var cancellationToken = modelCheckCancellationTokenSource.Token;
        Task.Factory.StartNew(
                action: () =>
                {
                    using (
                        FilePersistenceService independentPersistenceService =
                            new FilePersistenceBuilder().CreateNoBinFilePersistenceService()
                    )
                    {
                        IndexReferences(
                            independentPersistenceService: independentPersistenceService,
                            cancellationToken: cancellationToken
                        );
                        DoModelChecks(
                            independentPersistenceService: independentPersistenceService,
                            cancellationToken: cancellationToken
                        );
                    }
                },
                cancellationToken: cancellationToken
            )
            .ContinueWith(
                continuationAction: TaskErrorHandler,
                scheduler: TaskScheduler.FromCurrentSynchronizationContext()
            );
    }

    private void IndexReferences(
        FilePersistenceService independentPersistenceService,
        CancellationToken cancellationToken
    )
    {
        try
        {
            _statusBarService.SetStatusText(text: "Indexing references...");
            ReferenceIndexManager.Clear(fullClear: false);
            independentPersistenceService
                .SchemaProvider.RetrieveList<IFilePersistent>()
                .OfType<ISchemaItem>()
                .AsParallel()
                .ForEach(action: item =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    ReferenceIndexManager.Add(item: item);
                });
            ReferenceIndexManager.Initialize();
        }
        finally
        {
            _statusBarService.SetStatusText(text: "");
        }
    }

    private void TaskErrorHandler(Task previousTask)
    {
        try
        {
            previousTask.Wait();
        }
        catch (AggregateException ae)
        {
            bool actualExceptionsExist = ae.Flatten()
                .InnerExceptions.Any(predicate: x => !(x is OperationCanceledException));
            if (actualExceptionsExist)
            {
                log.LogOrigamError(ex: ae);
                this.RunWithInvoke(action: () =>
                    AsMessageBox.ShowError(
                        owner: this,
                        text: ae.Message,
                        caption: strings.GenericError_Title,
                        exception: ae
                    )
                );
            }
        }
    }

    private void DoModelChecks(
        FilePersistenceService independentPersistenceService,
        CancellationToken cancellationToken
    )
    {
        List<Dictionary<ISchemaItem, string>> errorFragments = ModelRules.GetErrors(
            schemaProviders: new OrigamProviderBuilder()
                .SetSchemaProvider(schemaProvider: independentPersistenceService.SchemaProvider)
                .GetAll(),
            independentPersistenceService: independentPersistenceService,
            cancellationToken: cancellationToken
        );
        var persistenceProvider = (FilePersistenceProvider)
            independentPersistenceService.SchemaProvider;
        var errorSections = persistenceProvider.GetFileErrors(
            ignoreDirectoryNames: new[] { ".git", "l10n" },
            cancellationToken: cancellationToken
        );
        if (errorFragments.Count != 0)
        {
            FindRulesPad resultsPad =
                WorkbenchSingleton.Workbench.GetPad(type: typeof(FindRulesPad)) as FindRulesPad;
            this.RunWithInvoke(action: () =>
            {
                DialogResult dialogResult = MessageBox.Show(
                    text: "Some model elements do not satisfy model integrity rules. Do you want to show the rule violations?",
                    caption: "Model Errors",
                    buttons: MessageBoxButtons.YesNo,
                    icon: MessageBoxIcon.Exclamation
                );
                if (dialogResult == DialogResult.Yes)
                {
                    resultsPad.DisplayResults(results: errorFragments);
                }
            });
        }
        if (errorSections.Count != 0)
        {
            this.RunWithInvoke(action: () =>
            {
                var modelCheckResultWindow = new ModelCheckResultWindow(
                    modelErrorSections: errorSections
                );
                modelCheckResultWindow.Show(owner: this);
            });
        }
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == 16)
        {
            CancelEventArgs args1 = new CancelEventArgs(cancel: false);
            if (m.Msg == 0x16)
            {
                args1.Cancel = m.WParam == IntPtr.Zero;
            }
            else
            {
                args1.Cancel = !base.Validate();
                this.OnClosing(e: args1);
                if (m.Msg == 0x11)
                {
                    m.Result = args1.Cancel ? IntPtr.Zero : ((IntPtr)1);
                }
            }
            if ((m.Msg != 0x11) && !args1.Cancel)
            {
                this.OnClosed(e: EventArgs.Empty);
                base.Dispose();
            }
        }
        else
        {
            base.WndProc(m: ref m);
        }
    }

    public bool UnloadSchema()
    {
        CloseAllViews();
        // make sure the application knows all windows were closed otherwise
        // it might throw an error when trying to set an active form which
        // is not there anymore
        Application.DoEvents();
        if (this.dockPanel.DocumentsCount > 0)
        {
            // could not close all the documents
            return false;
        }
        _workflowPad.OrigamMenu = null;

#if ! ORIGAM_CLIENT
        _findSchemaItemResultsPad?.ResetResults();
        _documentationPad?.ClearDocumentation();
#endif
        _auditLogPad?.ClearList();
        return true;
    }

    public bool Disconnect()
    {
        try
        {
            if (IsConnected)
            {
                SaveWorkspace();
            }
            if (!_schema.Disconnect())
            {
                return false;
            }

            ClearReferenceIndex();
            UnloadConnectedServices();
            UnloadConnectedPads();
            UnloadMainMenu();
            IsConnected = false;
            ConfigurationManager.SetActiveConfiguration(configuration: null);
            UpdateTitle();
            modelCheckCancellationTokenSource.Cancel();
            modelCheckCancellationTokenSource = new CancellationTokenSource();
            return true;
        }
        catch (Exception ex)
        {
            log.LogOrigamError(ex: ex);
            string message = $"{ex.Message}\n{ex.StackTrace}";
            if (ex is AggregateException aggregateException)
            {
                var innerExceptions = aggregateException.Flatten().InnerExceptions.ToList();
                if (
                    innerExceptions.Count == 1
                    && innerExceptions[index: 0] is TaskCanceledException
                )
                {
                    return true;
                }
                message = string.Join(
                    separator: "\n\n",
                    values: innerExceptions.Select(selector: x => $"{x.Message}\n{x.StackTrace}")
                );
            }
            MessageBox.Show(
                owner: this,
                text: message,
                caption: "Error",
                buttons: MessageBoxButtons.OK,
                icon: MessageBoxIcon.Error
            );
        }
        return true;
    }

    private void ClearReferenceIndex()
    {
        IPersistenceService persistenceService =
            ServiceManager.Services.GetService<IPersistenceService>();
        if (persistenceService != null)
        {
            ReferenceIndexManager.Clear(fullClear: true);
        }
    }

    private void UnloadConnectedPads()
    {
        GetPad<AuditLogPad>().ClearList();
        GetPad<ExtensionPad>().UnloadPackages();
        GetPad<FindSchemaItemResultsPad>()?.Clear();
        GetPad<FindRulesPad>()?.Clear();
    }

    private void SaveWorkspace()
    {
        try
        {
            dockPanel.SaveAsXml(fileName: _configFilePath);
        }
        catch { }
    }

    /// <summary>
    /// Get configuration and load it. If there is more than 1 configuration, display selection dialog.
    /// </summary>
    private bool LoadConfiguration(string configurationName)
    {
        OrigamSettingsCollection configurations =
            ConfigurationManager.GetAllUserHomeConfigurations();
        if (configurationName != null)
        {
            foreach (OrigamSettings config in configurations)
            {
                if (config.Name == configurationName)
                {
                    ConfigurationManager.SetActiveConfiguration(configuration: config);
                    return true;
                }
            }
            throw new ArgumentOutOfRangeException(
                paramName: nameof(configurationName),
                actualValue: configurationName,
                message: strings.ConfigurationNotFound_ExceptionMessage
            );
        }

        if (configurations.Count == 0)
        {
            Commands.CreateNewProject cmd = new Commands.CreateNewProject();
            cmd.Run();
            return false;
        }

        if (configurations.Count == 1)
        {
            ConfigurationManager.SetActiveConfiguration(configuration: configurations[index: 0]);
        }
        else
        {
            ConfigurationSelector selector = new ConfigurationSelector();
            selector.Configurations = configurations;
            if (selector.ShowDialog(owner: this) == DialogResult.OK)
            {
                ConfigurationManager.SetActiveConfiguration(
                    configuration: selector.SelectedConfiguration
                );
            }
            else
            {
                return false;
            }
            Application.DoEvents();
        }
        return true;
    }

    private void UnloadConnectedServices()
    {
        IControlsLookUpService controlsLookupService =
            ServiceManager.Services.GetService<IControlsLookUpService>();
        if (controlsLookupService != null)
        {
            ServiceManager.Services.UnloadService(service: controlsLookupService);
        }
        var persistenceService = ServiceManager.Services.GetService<IPersistenceService>();
        if (persistenceService is FilePersistenceService filePersistService)
        {
            filePersistService.ReloadNeeded -= OnFilePersistServiceReloadRequested;
        }
        FilePersistenceBuilder.Clear();
        OrigamEngine.UnloadConnectedServices();
    }

    /// <summary>
    /// After configuration is selected, connect to the repository and load the model list from the repository.
    /// </summary>
    private void InitPersistenceService()
    {
        IPersistenceService persistence = OrigamEngine.CreatePersistenceService();
        ServiceManager.Services.AddService(service: persistence);
    }

    private void frmMain_Load(object sender, EventArgs e)
    {
        try
        {
            AppDomain.CurrentDomain.SetPrincipalPolicy(
                policy: System.Security.Principal.PrincipalPolicy.WindowsPrincipal
            );
            AsMessageBox.DebugInfoProvider = new Origam.Workflow.DebugInfo();
            SplashScreen splash = new SplashScreen();
            splash.Show();
            Application.DoEvents();
            InitializeDefaultServices();
            SubscribeToUpgradeServiceEvents();
            InitializeDefaultPads();
            //this.LoadWorkspace();
            // Menu and toolbars
            PrepareMenuBars();
            CreateMainMenu();
            FinishMenuBars();
            this.dockPanel.ActiveDocumentChanged += dockPanel_ActiveDocumentChanged;
            this.dockPanel.ContentRemoved += dockPanel_ContentRemoved;
            this.dockPanel.ActiveContentChanged += dockPanel_ActiveContentChanged;

            Connect();
            splash.Dispose();
        }
        catch (Exception ex)
        {
            this.RunWithInvoke(action: () =>
                AsMessageBox.ShowError(
                    owner: this,
                    text: ex.Message,
                    caption: strings.GenericError_Title,
                    exception: ex
                )
            );
        }
    }

    private void InitializeDefaultPads()
    {
        // this will not be used in the Client, but we need to have an instance, because icons are taken from it
        _extensionPad = CreatePad<ExtensionPad>();
        _attachmentPad = CreatePad<AttachmentPad>();
        _auditLogPad = CreatePad<AuditLogPad>();
        _schema.SchemaListBrowser = _extensionPad;
#if ORIGAM_CLIENT
        _schemaBrowserPad = new SchemaBrowser();
        if (AdministratorMode)
        {
            _propertyPad = new PropertyPad();
            this.PadContentCollection.Add(value: _propertyPad);
            _outputPad = new OutputPad();
            this.PadContentCollection.Add(value: _outputPad);
        }
#else
        _schemaBrowserPad = CreatePad<SchemaBrowser>();
        _workflowWatchPad = CreatePad<WorkflowWatchPad>();
        _propertyPad = CreatePad<PropertyPad>();
        _logPad = CreatePad<LogPad>();
        _outputPad = CreatePad<OutputPad>();
        _documentationPad = CreatePad<DocumentationPad>();
        _findSchemaItemResultsPad = CreatePad<FindSchemaItemResultsPad>();
        _findRulesPad = CreatePad<FindRulesPad>();
#endif
        _workflowPad = CreatePad<WorkflowPlayerPad>();
        _serverLogPad = CreatePad<ServerLogPad>();
        CreatePad<WorkQueuePad>();
    }

    private void InitializeConnectedPads()
    {
#if !ORIGAM_CLIENT
        GetPad<ExtensionPad>()?.LoadPackages();
#endif
    }

    private T CreatePad<T>()
    {
        var pad = Activator.CreateInstance<T>();
        PadContentCollection.Add(value: (IPadContent)pad);
        return pad;
    }

    /// <summary>
    /// After successfully loading the model list, we initialize model-connected services
    /// </summary>
    private void InitializeConnectedServices()
    {
        ServiceManager.Services.AddService(
            service: new ServiceAgentFactory(
                fromExternalAgent: externalAgent => new ExternalAgentWrapper(
                    externalServiceAgent: externalAgent
                )
            )
        );
        ServiceManager.Services.AddService(service: new StateMachineService());
        ServiceManager.Services.AddService(service: OrigamEngine.CreateDocumentationService());
        ServiceManager.Services.AddService(service: new TracingService());
        ServiceManager.Services.AddService(service: new DataLookupService());
        ServiceManager.Services.AddService(service: new ControlsLookUpService());
        ServiceManager.Services.AddService(service: new DeploymentService());
        ServiceManager.Services.AddService(service: new ParameterService());
        ServiceManager.Services.AddService(
            service: new Origam.Workflow.WorkQueue.WorkQueueService()
        );
        ServiceManager.Services.AddService(service: new AttachmentService());
        ServiceManager.Services.AddService(service: new RuleEngineService());

        var settings = ConfigurationManager.GetActiveConfiguration();
        ServiceManager.Services.AddService(service: new DatabaseProfileService(settings: settings));
    }

    private void InitializeDefaultServices()
    {
        ServiceManager.Services.AddService(service: new MetaModelUpgradeService());
        // Status bar service
        _statusBarService = new StatusBarService(statusBar: statusBar);
        ServiceManager.Services.AddService(service: _statusBarService);
        // Schema service
        _schema = new WorkbenchSchemaService();
        ServiceManager.Services.AddService(service: _schema);
        _schema.SchemaLoaded += _schema_SchemaLoaded;
        _schema.SchemaUnloading += _schema_SchemaUnloading;
#if ! ORIGAM_CLIENT
        _schema.ActiveNodeChanged += _schema_ActiveNodeChanged;
#endif
    }

    private void _schema_ActiveNodeChanged(object sender, EventArgs e)
    {
        UpdateToolbar();
        AbstractSqlDataService abstractSqlDataService =
            DataServiceFactory.GetDataService() as AbstractSqlDataService;
        AbstractSqlCommandGenerator abstractSqlCommandGenerator = (AbstractSqlCommandGenerator)
            abstractSqlDataService.DbDataAdapterFactory;
        if (_schema.ActiveSchemaItem != null)
        {
            ShowDocumentation cmd = new ShowDocumentation();
            cmd.Run();
        }
        if (_schema.ActiveNode is TableMappingItem)
        {
            try
            {
                _outputPad.SetOutputText(
                    sText: abstractSqlCommandGenerator.TableDefinitionDdl(
                        table: _schema.ActiveNode as TableMappingItem
                    )
                );
                _outputPad.AppendText(
                    sText: abstractSqlCommandGenerator.ForeignKeyConstraintsDdl(
                        table: _schema.ActiveNode as TableMappingItem
                    )
                );
            }
            catch (Exception ex)
            {
                _outputPad.SetOutputText(sText: ex.Message);
            }
        }
        if (_schema.ActiveNode is Function)
        {
            try
            {
                _outputPad.SetOutputText(
                    sText: abstractSqlCommandGenerator.FunctionDefinitionDdl(
                        function: _schema.ActiveNode as Function
                    )
                );
            }
            catch (Exception ex)
            {
                _outputPad.SetOutputText(sText: ex.Message);
            }
        }
        if (_schema.ActiveNode is DataStructure)
        {
            try
            {
                DatasetGenerator gen = new DatasetGenerator(userDefinedParameters: false);
                _outputPad.SetOutputText(
                    sText: gen.CreateDataSet(ds: _schema.ActiveNode as DataStructure).GetXmlSchema()
                );
            }
            catch (Exception ex)
            {
                _outputPad.SetOutputText(sText: ex.Message);
            }
        }
    }

    private void dockPanel_ActiveDocumentChanged(object sender, EventArgs e)
    {
        if (this.dockPanel.ActiveDocument != null)
        {
            OnActiveWindowChanged(sender: sender, e: new EventArgs());
        }
    }

    private void dockPanel_ContentRemoved(object sender, DockContentEventArgs e)
    {
        UpdateToolbar();
        if (e.Content is IViewContent)
        {
            (e.Content as IViewContent).StatusTextChanged -= ActiveViewContent_StatusTextChanged;
            _statusBarService.SetStatusText(text: "");
        }
    }

    private void dockPanel_ActiveContentChanged(object sender, EventArgs e)
    {
        UpdateToolbar();
        if (this.ActiveViewContent != null)
        {
            this.ActiveViewContent.StatusTextChanged += new EventHandler(
                ActiveViewContent_StatusTextChanged
            );

            // do this last, because there is DoEvents inside, which can cause ActiveViewContent to be null
            _statusBarService.SetStatusText(text: this.ActiveViewContent.StatusText);
        }
    }

    private void Content_DirtyChanged(object sender, EventArgs e)
    {
        UpdateToolbar();
    }

    private void ActiveViewContent_StatusTextChanged(object sender, EventArgs e)
    {
        if (this.ActiveViewContent != null)
        {
            _statusBarService.SetStatusText(text: this.ActiveViewContent.StatusText);
        }
    }

    // Handle keyboard shortcuts
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        if (_shortcuts.Contains(key: keyData))
        {
            ToolStripItem menu = _shortcuts[key: keyData] as ToolStripItem;
            AsMenuCommand cmd = menu as AsMenuCommand;
            if (menu.Enabled || (cmd != null && cmd.IsEnabled))
            {
                if (cmd != null)
                {
                    // PerformClick() does not fire anything if Enabled=false
                    cmd.UpdateItemsToDisplay();
                }
                menu.PerformClick();
                return true;
            }
        }
        return base.ProcessCmdKey(msg: ref msg, keyData: keyData);
    }

    private void _schema_SchemaLoaded(object sender, bool isInteractive)
    {
        OrigamEngine.InitializeSchemaItemProviders(service: _schema);
        IDeploymentService deployment = ServiceManager.Services.GetService<IDeploymentService>();
        IParameterService parameterService =
            ServiceManager.Services.GetService<IParameterService>();
#if ORIGAM_CLIENT
        deployment.CanUpdate(extension: _schema.ActiveExtension);
        string modelVersion = _schema.ActiveExtension.Version;
        string dbVersion = deployment.CurrentDeployedVersion(extension: _schema.ActiveExtension);
        if (modelVersion != dbVersion)
        {
            if (AdministratorMode)
            {
                string message = string.Format(
                    format: strings.ModelDatabaseVersionMissmatch_Message,
                    arg0: Environment.NewLine,
                    arg1: modelVersion + Environment.NewLine,
                    arg2: dbVersion
                );
                AsMessageBox.ShowError(
                    owner: this,
                    text: message,
                    caption: strings.ConnectionTitle,
                    exception: null
                );
            }
            else
            {
                string message = string.Format(
                    format: strings.ModelDatabaseVersionMissmatchNoAdmin_Message,
                    arg0: Environment.NewLine,
                    arg1: modelVersion + Environment.NewLine,
                    arg2: dbVersion
                );
                throw new Exception(message: message);
            }
        }
#else
        ApplicationDataDisconnectedMode = !TestConnectionToApplicationDataDatabase();
        if (ApplicationDataDisconnectedMode)
        {
            parameterService.PrepareParameters();
            UpdateTitle();
            return;
        }
        bool isEmpty = deployment.IsEmptyDatabase();
        // data database is empty and we are not supposed to ask for running init scripts
        // that means the new project wizard is running and will take care
        if (isEmpty && !PopulateEmptyDatabaseOnLoad)
        {
            return;
        }
        if (isEmpty)
        {
            if (
                MessageBox.Show(
                    strings.RunInitDatabaseScriptsQuestion,
                    strings.DatabaseEmptyTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1
                ) == DialogResult.Yes
            )
            {
                deployment.Deploy();
            }
        }
        RunDeploymentScripts(deployment, isInteractive);
#endif
        try
        {
            parameterService.RefreshParameters();
        }
        catch (Exception ex)
        {
            // show the error but go on
            // error can occur e.g. when duplicate constant name is loaded, e.g. due to incompatible packages
            AsMessageBox.ShowError(
                owner: this,
                text: ex.Message,
                caption: strings.ErrorWhileLoadingParameters_Message,
                exception: ex
            );
        }
#if ! ORIGAM_CLIENT
        // we have to initialize the new user after parameter service gets loaded
        // otherwise it would fail generating SQL statements
        if (isEmpty)
        {
            string userName = SecurityManager.CurrentPrincipal.Identity.Name;
            if (
                MessageBox.Show(
                    string.Format(strings.AddUserToUserList_Question, userName),
                    strings.DatabaseEmptyTitle,
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1
                ) == DialogResult.Yes
            )
            {
                IOrigamProfileProvider profileProvider = SecurityManager.GetProfileProvider();
                profileProvider.AddUser("Architect (" + userName + ")", userName);
            }
        }
#endif
        MenuSchemaItemProvider menuProvider =
            _schema.GetProvider(type: typeof(MenuSchemaItemProvider)) as MenuSchemaItemProvider;
        if (menuProvider.ChildItems.Count > 0)
        {
            _workflowPad.OrigamMenu = menuProvider.MainMenu;
        }
        UpdateTitle();
    }

    private void RunDeploymentScripts(IDeploymentService deployment, bool isInteractive)
    {
        DeployVersion deployCommand = new DeployVersion();
        if (!deployCommand.IsEnabled)
        {
            return;
        }
        if (
            isInteractive
            || MessageBox.Show(
                text: strings.RunDeploymentScriptsQuestion,
                caption: strings.DeploymentSctiptsPending_Title,
                buttons: MessageBoxButtons.YesNo,
                icon: MessageBoxIcon.Question,
                defaultButton: MessageBoxDefaultButton.Button1
            ) == DialogResult.Yes
        )
        {
            PackageVersion deployedPackageVersion = deployment.CurrentDeployedVersion(
                extension: _schema.ActiveExtension
            );
            if (!isInteractive && deployedPackageVersion == PackageVersion.Zero)
            {
                if (
                    MessageBox.Show(
                        text: strings.DeploySinglePackageQuestion,
                        caption: strings.DeploymentSctiptsPending_Title,
                        buttons: MessageBoxButtons.YesNo,
                        icon: MessageBoxIcon.Question,
                        defaultButton: MessageBoxDefaultButton.Button1
                    ) == DialogResult.Yes
                )
                {
                    deployment.ForceDeployCurrentPackage();
                    GetPad<ExtensionPad>()?.LoadPackages();
                    return;
                }
            }
            deployment.Deploy();
            GetPad<ExtensionPad>()?.LoadPackages();
        }
    }

    public void UpdateTitle()
    {
#if ORIGAM_CLIENT
        Title = "";
#else
        Title = strings.OrigamArchitect_Title;
#endif
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        if (settings == null)
        {
            return;
        }
        if (settings.TitleText != "")
        {
            if (Title != "")
            {
                Title += " - ";
            }
            Title += settings.TitleText;
        }
        if (_schema?.ActiveExtension == null)
        {
            return;
        }
#if !ORIGAM_CLIENT
        Title += " - ";
        Title += _schema.ActiveExtension.Name;
#endif
        Title += string.Format(
            format: strings.ModelVersion_Title,
            arg0: _schema?.ActiveExtension?.VersionString,
            arg1: (ApplicationDataDisconnectedMode ? strings.Disconnected : "")
        );
    }

    private void AttachmentDocument_ParentIdChanged(
        object sender,
        Guid mainEntityId,
        Guid mainRecordId,
        Hashtable childReferences
    )
    {
        try
        {
            _attachmentPad?.GetAttachments(
                mainEntityId: mainEntityId,
                mainRecordId: mainRecordId,
                childReferences: childReferences
            );
        }
        catch { }
    }

    public void ProcessGuiLink(
        IOrigamForm sourceForm,
        object linkTarget,
        Dictionary<string, object> parameters
    )
    {
        AbstractMenuItem targetMenuItem = linkTarget as AbstractMenuItem;
        OrigamArchitect.Commands.ExecuteSchemaItem cmd =
            new OrigamArchitect.Commands.ExecuteSchemaItem();
        if (sourceForm != null && (targetMenuItem is FormReferenceMenuItem))
        {
            if (
                (targetMenuItem as FormReferenceMenuItem).Screen.PrimaryKey.Equals(
                    obj: sourceForm.PrimaryKey
                )
            )
            {
                object[] val = new object[parameters.Count];
                int i = 0;
                foreach (var entry in parameters)
                {
                    val[i] = entry.Value;

                    i++;
                }
                sourceForm.SetPosition(key: val);
                return;
            }
        }
        foreach (var entry in parameters)
        {
            cmd.Parameters.Add(key: entry.Key, value: entry.Value);
        }
        cmd.RecordEditingMode = true;
        cmd.Owner = targetMenuItem;
        cmd.Run();
    }

    public void ExitWorkbench()
    {
        this.Close();
    }

    private void _schema_SchemaUnloading(object sender, CancelEventArgs e)
    {
        e.Cancel = !UnloadSchema();
    }

    private void CheckModelRootPackageVersion()
    {
        if (!AdministratorMode)
        {
            bool found = false;
            foreach (Package extension in _schema.LoadedPackages)
            {
                if (
                    (Guid)extension.PrimaryKey[key: "Id"]
                    == new Guid(g: "147FA70D-6519-4393-B5D0-87931F9FD609")
                )
                {
                    if (extension.Version < new PackageVersion(completeVersionString: "5.0.0"))
                    {
                        MessageBox.Show(
                            owner: this,
                            text: strings.ModelVersionErrorMessage,
                            caption: strings.ModelVersionTitle,
                            buttons: MessageBoxButtons.OK,
                            icon: MessageBoxIcon.Error
                        );
                        Disconnect();
                        return;
                    }
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                MessageBox.Show(
                    owner: this,
                    text: strings.ModelMissingMessage,
                    caption: strings.ModelVersionTitle,
                    buttons: MessageBoxButtons.OK,
                    icon: MessageBoxIcon.Error
                );
                Disconnect();
                return;
            }
        }
    }

    private void searchBox_KeyDown(object sender, KeyEventArgs e)
    {
#if ORIGAM_CLIENT
#else
        if (e.KeyCode == Keys.Enter)
        {
            if (_schema.IsSchemaLoaded)
            {
                string text = (sender as ComboBox).Text;
                if (text.Equals(String.Empty))
                {
                    return;
                }

                _findSchemaItemResultsPad.ResetResults();
                IPersistenceService persistence =
                    ServiceManager.Services.GetService(typeof(IPersistenceService))
                    as IPersistenceService;
                ISchemaItem[] results = persistence.SchemaProvider.FullTextSearch<ISchemaItem>(
                    text
                );
                if (results.LongLength > 0)
                {
                    _findSchemaItemResultsPad.DisplayResults(results);
                }
                MessageBox.Show(
                    this,
                    string.Format(strings.ResultCountMassage, results.LongLength),
                    strings.SearchResultTitle,
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            e.Handled = true;
        }
#endif
    }

    void toolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
    {
        MenuItemClick(sender: e.ClickedItem, e: EventArgs.Empty);
    }

    private SaveFileDialog GetExportToExcelSaveDialog(ExcelFormat excelFormat)
    {
        SaveFileDialog sfd = new SaveFileDialog();
        if (excelFormat == ExcelFormat.XLSX)
        {
            sfd.Filter = "Microsoft Excel | *.xlsx";
            sfd.DefaultExt = "xlsx";
        }
        else
        {
            sfd.Filter = "Microsoft Excel | *.xls";
            sfd.DefaultExt = "xls";
        }
        sfd.Title = strings.ExcepExport_Title;
        return sfd;
    }

    private static bool TestConnectionToApplicationDataDatabase()
    {
        AbstractSqlDataService abstractSqlDataService =
            DataServiceFactory.GetDataService() as AbstractSqlDataService;
        try
        {
            abstractSqlDataService.ExecuteUpdate(command: "SELECT 1", transactionId: null);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
