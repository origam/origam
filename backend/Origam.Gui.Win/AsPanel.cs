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
using System.Xml;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using Origam.Services;
using Origam.Schema;
using Origam.Workbench.Services;
using Origam.Schema.EntityModel;
using Origam.Schema.LookupModel;
using Origam.Schema.GuiModel;
using Origam.DA;
using Origam.UI;
using Origam.Workbench;
using Origam.Workbench.Pads;
using Origam.Rule;
using System.Collections.Generic;
using System.Linq;
using Origam.Extensions;
using Origam.Schema.GuiModel.Designer;
using Origam.Gui;
using Origam.Schema.MenuModel;
using Origam.Gui.UI;
using Origam.Service.Core;

namespace Origam.Gui.Win;
[Designer(typeof(ControlDesigner))]
[ToolboxBitmap(typeof(AsPanel), "AsPanel.bmp")]
public class AsPanel: BasePanel, IAsDataConsumer, IOrigamMetadataConsumer, 
    ISupportInitialize, IComparable
{
	#region Events
	public event EventHandler RecordIdChanged;
	public event EventHandler ShowAttachmentsChanged;
	private event EventHandler AttachmentCountCalculated;
	#endregion
	#region Private Fields
	private AttachmentPad _attachmentPad;
	private ImageList imageList1;
	private AsPanelTitle pnlDataControl;
	internal AsDataGrid grid;
	IGridBuilder _gridBuilder = new DataGridBuilder();
		
	private DbNavigator dbNav;
	private IContainer components;
	
	private CurrencyManager _bindingManager;
	private ToolBarButton btnGrid;
	private ToolBarButton btnAttachment;
	private Panel pnlButtonHolder;
	private ToolBar toolBar;
	private ToolBarButton btnFilter;
	private FilterPanel pnlFilter;
	private System.Windows.Forms.ContextMenu filterMenu;
	private MenuItem mnuFilterAdd;
	private MenuItem mnuFilterDelete;
	private MenuItem mnuFilterClear;
	private MenuItem menuItem4;
	private MenuItem mnuSetDefaultFilter;
	private MenuItem menuItem2;
	private MenuItem mnuUnsetDefaultFilter;
	private ToolBarButton btnAuditLog;
	private AuditLogPad _auditLogPad;
	private IPersistenceService _persistence;
	private DataGridFilterFactory _filterFactory;
	private ErrorProvider errorProvider1;
	private readonly ActionButtonManager actionButtonManager;
	private bool _isNewRow = false;
	private object[] _newRowKey;
	private bool _defaultFilterUsed = false;
	bool _refreshingFilter = false;
	private bool _internalDisposing = false;
	private bool _originalDisplayDeleteButton = false;
	DataSet _userConfig = null;
	private static ImageListStreamer ImgListStreamer = null;
	#endregion
	#region Constructors
	public AsPanel()
	{
		// This call is required by the Windows.Forms Form Designer.
		InitializeComponent();
	
		if(ImgListStreamer == null)
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AsPanel));
			ImgListStreamer = ((ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
		}
		this.imageList1.ImageStream = ImgListStreamer;
		this.BackColor = OrigamColorScheme.FormBackgroundColor;
		pnlDataControl.StartColor = OrigamColorScheme.TitleInactiveStartColor;
		pnlDataControl.EndColor = OrigamColorScheme.TitleInactiveEndColor;
		pnlDataControl.ForeColor = OrigamColorScheme.TitleInactiveForeColor;
		pnlDataControl.MiddleStartColor = OrigamColorScheme.TitleInactiveMiddleStartColor;
		pnlDataControl.MiddleEndColor = OrigamColorScheme.TitleInactiveMiddleEndColor;
		if(! this.DesignMode)
		{
			_auditLogPad = WorkbenchSingleton.Workbench.GetPad(typeof(AuditLogPad)) as AuditLogPad;
			_persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
			
			_attachmentPad = WorkbenchSingleton.Workbench.GetPad(typeof(AttachmentPad)) as AttachmentPad;
			if(_attachmentPad != null)
			{
				_attachmentPad.AttachmentsUpdated += _attachmentPad_AttachmentsUpdated;
			}
		}
		this.Leave += AsPanel_Leave;
		this.AttachmentCountCalculated += AsPanel_AttachmentCountCalculated;
		actionButtonManager = new ActionButtonManager(
			bindingManagerGetter: () => _bindingManager,
			parentIdGetter: () => EntityId,
			dataSourceGetter: () => (DataSet) DataSource,
			formPanelIdGetter: () => FormPanelId,
			dataMemberGetter: () => DataMember,
			toolStripGetter: () => ToolStrip,
			formGeneratorGetter: () => Generator,
			formIdGetter: () => (Guid)(FindForm() as AsForm).PrimaryKey["Id"]);
	}
	public AsPanel(IContainer container) : this()
	{
		container.Add(this);
	}
	#endregion
	#region Component Designer generated code
	/// <summary>
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AsPanel));
        this.imageList1 = new System.Windows.Forms.ImageList(this.components);
        this.pnlDataControl = new Origam.Gui.Win.AsPanelTitle();
        this.dbNav = new Origam.Gui.Win.DbNavigator();
        this.pnlButtonHolder = new System.Windows.Forms.Panel();
        this.toolBar = new System.Windows.Forms.ToolBar();
        this.btnGrid = new System.Windows.Forms.ToolBarButton();
        this.btnAttachment = new System.Windows.Forms.ToolBarButton();
        this.btnAuditLog = new System.Windows.Forms.ToolBarButton();
        this.btnFilter = new System.Windows.Forms.ToolBarButton();
        this.filterMenu = new System.Windows.Forms.ContextMenu();
        this.mnuSetDefaultFilter = new System.Windows.Forms.MenuItem();
        this.mnuUnsetDefaultFilter = new System.Windows.Forms.MenuItem();
        this.menuItem2 = new System.Windows.Forms.MenuItem();
        this.mnuFilterAdd = new System.Windows.Forms.MenuItem();
        this.mnuFilterDelete = new System.Windows.Forms.MenuItem();
        this.mnuFilterClear = new System.Windows.Forms.MenuItem();
        this.menuItem4 = new System.Windows.Forms.MenuItem();
        this.pnlFilter = new Origam.Gui.Win.FilterPanel();
        this.pnlDataControl.SuspendLayout();
        this.pnlButtonHolder.SuspendLayout();
        this.SuspendLayout();
        // 
        // imageList1
        // 
        this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
        this.imageList1.TransparentColor = System.Drawing.Color.Magenta;
        this.imageList1.Images.SetKeyName(0, "grid_white.png");
        this.imageList1.Images.SetKeyName(1, "attachment_white.png");
        this.imageList1.Images.SetKeyName(2, "filter.png");
        this.imageList1.Images.SetKeyName(3, "audit_white.png");
        this.imageList1.Images.SetKeyName(4, "export_to_excel.png");
        this.imageList1.Images.SetKeyName(5, "attachment_notification.png");
        this.imageList1.Images.SetKeyName(6, "attachment_white.png");
        // 
        // pnlDataControl
        // 
        this.pnlDataControl.BackColor = System.Drawing.Color.Transparent;
        this.pnlDataControl.Controls.Add(this.dbNav);
        this.pnlDataControl.Controls.Add(this.pnlButtonHolder);
        this.pnlDataControl.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlDataControl.EndColor = System.Drawing.Color.DarkKhaki;
        this.pnlDataControl.ForeColor = System.Drawing.Color.Black;
        this.pnlDataControl.Location = new System.Drawing.Point(0, 0);
        this.pnlDataControl.MiddleEndColor = System.Drawing.Color.Empty;
        this.pnlDataControl.MiddleStartColor = System.Drawing.Color.Empty;
        this.pnlDataControl.Name = "pnlDataControl";
        this.pnlDataControl.PanelIcon = null;
        this.pnlDataControl.PanelTitle = "";
        this.pnlDataControl.Size = new System.Drawing.Size(552, 24);
        this.pnlDataControl.StartColor = System.Drawing.Color.PaleGoldenrod;
        this.pnlDataControl.StatusIcon = null;
        this.pnlDataControl.TabIndex = 1;
        this.pnlDataControl.TabStop = true;
        this.pnlDataControl.Click += new System.EventHandler(this.pnlDataControl_Click);
        // 
        // dbNav
        // 
        this.dbNav.BackColor = System.Drawing.Color.Transparent;
        this.dbNav.Dock = System.Windows.Forms.DockStyle.Right;
        this.dbNav.ForeColor = System.Drawing.Color.Black;
        this.dbNav.Location = new System.Drawing.Point(184, 0);
        this.dbNav.Name = "dbNav";
        this.dbNav.Size = new System.Drawing.Size(232, 24);
        this.dbNav.TabIndex = 2;
        this.dbNav.NewRecordAdded += new System.EventHandler(this.dbNav_NewRecordAdded);
        // 
        // pnlButtonHolder
        // 
        this.pnlButtonHolder.Controls.Add(this.toolBar);
        this.pnlButtonHolder.Dock = System.Windows.Forms.DockStyle.Right;
        this.pnlButtonHolder.Location = new System.Drawing.Point(416, 0);
        this.pnlButtonHolder.Name = "pnlButtonHolder";
        this.pnlButtonHolder.Size = new System.Drawing.Size(136, 24);
        this.pnlButtonHolder.TabIndex = 5;
        // 
        // toolBar
        // 
        this.toolBar.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
        this.toolBar.AutoSize = false;
        this.toolBar.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
        this.btnGrid,
        this.btnAttachment,
        this.btnAuditLog,
        this.btnFilter});
        this.toolBar.ButtonSize = new System.Drawing.Size(17, 17);
        this.toolBar.Divider = false;
        this.toolBar.DropDownArrows = true;
        this.toolBar.ImageList = this.imageList1;
        this.toolBar.Location = new System.Drawing.Point(0, 0);
        this.toolBar.Name = "toolBar";
        this.toolBar.ShowToolTips = true;
        this.toolBar.Size = new System.Drawing.Size(136, 24);
        this.toolBar.TabIndex = 4;
        this.toolBar.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar_ButtonClick);
        this.toolBar.Validated += new System.EventHandler(this.toolBar_Validated);
        // 
        // btnGrid
        // 
        this.btnGrid.ImageIndex = 0;
        this.btnGrid.Name = "btnGrid";
        this.btnGrid.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
        this.btnGrid.ToolTipText = "Přepne mezi zobrazením tabulky a formuláře (Ctrl+G)";
        // 
        // btnAttachment
        // 
        this.btnAttachment.ImageIndex = 1;
        this.btnAttachment.Name = "btnAttachment";
        this.btnAttachment.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
        this.btnAttachment.ToolTipText = "Zapne zobrazování příloh v panelu Přílohy (Ctrl+A)";
        // 
        // btnAuditLog
        // 
        this.btnAuditLog.ImageIndex = 3;
        this.btnAuditLog.Name = "btnAuditLog";
        this.btnAuditLog.ToolTipText = "Zobrazí historii změn aktuálního záznamu (Ctrl+H)";
        // 
        // btnFilter
        // 
        this.btnFilter.DropDownMenu = this.filterMenu;
        this.btnFilter.ImageIndex = 2;
        this.btnFilter.Name = "btnFilter";
        this.btnFilter.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
        this.btnFilter.ToolTipText = "Zapne/vypne filtr na aktuální seznam (Ctrl+F)";
        // 
        // filterMenu
        // 
        this.filterMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
        this.mnuSetDefaultFilter,
        this.mnuUnsetDefaultFilter,
        this.menuItem2,
        this.mnuFilterAdd,
        this.mnuFilterDelete,
        this.mnuFilterClear,
        this.menuItem4});
        this.filterMenu.Popup += new System.EventHandler(this.filterMenu_Popup);
        this.filterMenu.Disposed += new System.EventHandler(this.filterMenu_Disposed);
        // 
        // mnuSetDefaultFilter
        // 
        this.mnuSetDefaultFilter.Enabled = false;
        this.mnuSetDefaultFilter.Index = 0;
        this.mnuSetDefaultFilter.Text = "&Nastavit aktuální filtr jako standardní";
        this.mnuSetDefaultFilter.Click += new System.EventHandler(this.mnuSetDefaultFilter_Click);
        // 
        // mnuUnsetDefaultFilter
        // 
        this.mnuUnsetDefaultFilter.Index = 1;
        this.mnuUnsetDefaultFilter.Text = "Z&rušit standardní filtr";
        this.mnuUnsetDefaultFilter.Click += new System.EventHandler(this.mnuUnsetDefaultFilter_Click);
        // 
        // menuItem2
        // 
        this.menuItem2.Index = 2;
        this.menuItem2.Text = "-";
        // 
        // mnuFilterAdd
        // 
        this.mnuFilterAdd.Enabled = false;
        this.mnuFilterAdd.Index = 3;
        this.mnuFilterAdd.Text = "&Uložit aktuální filtr";
        this.mnuFilterAdd.Click += new System.EventHandler(this.mnuFilterAdd_Click);
        // 
        // mnuFilterDelete
        // 
        this.mnuFilterDelete.Enabled = false;
        this.mnuFilterDelete.Index = 4;
        this.mnuFilterDelete.Text = "&Smazat";
        this.mnuFilterDelete.Click += new System.EventHandler(this.mnuFilterDelete_Click);
        // 
        // mnuFilterClear
        // 
        this.mnuFilterClear.Enabled = false;
        this.mnuFilterClear.Index = 5;
        this.mnuFilterClear.Text = "&Zrušit filtr";
        this.mnuFilterClear.Click += new System.EventHandler(this.mnuFilterClear_Click);
        // 
        // menuItem4
        // 
        this.menuItem4.Index = 6;
        this.menuItem4.Text = "-";
        // 
        // pnlFilter
        // 
        this.pnlFilter.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlFilter.Location = new System.Drawing.Point(0, 24);
        this.pnlFilter.Name = "pnlFilter";
        this.pnlFilter.Query = null;
        this.pnlFilter.Size = new System.Drawing.Size(552, 49);
        this.pnlFilter.TabIndex = 2;
        this.pnlFilter.TabStop = false;
        this.pnlFilter.Visible = false;
        // 
        // AsPanel
        // 
        this.AutoScroll = true;
        this.BackColor = System.Drawing.Color.FloralWhite;
        this.Controls.Add(this.pnlFilter);
        this.Controls.Add(this.pnlDataControl);
        this.Name = "AsPanel";
        this.Size = new System.Drawing.Size(552, 160);
        this.Load += new System.EventHandler(this.AsPanel_Load_1);
        this.Paint += new System.Windows.Forms.PaintEventHandler(this.AsPanel_Paint);
        this.Enter += new System.EventHandler(this.AsPanel_Enter);
        this.GotFocus += new System.EventHandler(this.AsPanel_GotFocus);
        this.pnlDataControl.ResumeLayout(false);
        this.pnlButtonHolder.ResumeLayout(false);
        this.ResumeLayout(false);
	}
    bool _filterMenuDisposed = false;
    void filterMenu_Disposed(object sender, EventArgs e)
    {
        _filterMenuDisposed = true;
    }
	#endregion
	#region Properties
	public object[] CurrentKey { get; private set; } 
	internal Hashtable CurrentSort { get; } = new Hashtable();
	public bool IgnoreEscape { get; set; } 
	public bool ShowAttachments
	{
		get => this.btnAttachment.Pushed;
		set
		{
			this.btnAttachment.Pushed = value;
			OnShowAttachmentsChanged();
		}
	}
	
	public Guid EntityId
	{
		get
		{
			string tableName = FormTools.FindTableByDataMember(this.DataSource as DataSet, this.DataMember);
			if(string.IsNullOrEmpty(tableName))
			{
				return Guid.Empty;
			}
			else
			{
				return new Guid((this.DataSource as DataSet).Tables[tableName].ExtendedProperties["EntityId"].ToString()); 
			}
		}
	}
	public object RecordId { get; private set; }
	public string DefaultConfiguration { get; set; }
	public DataGrid Grid => grid as DataGrid;
	[Browsable(false)]
	public FormGenerator Generator { get; set; }
	[Category("(ORIGAM)")]
	public PanelControlSet Panel
	{
		get
		{
			if(this.Tag is ControlSetItem)
			{
				return (this.Tag as ControlSetItem).ControlItem.PanelControlSet;
			}
			
			return null;
		}
	}
	[Browsable(false)]
	public bool FilterActive => pnlFilter.FilterActive;
	public bool FilterVisible
	{
		get => btnFilter.Pushed;
		set
		{
            btnFilter.Pushed = value;
			pnlFilter.Visible = value;
			if(value)
			{
				this.ShowGrid = true;
				this._filterFactory.FocusFilterPanel();
			}
		}
	}
    public List<EntityUIAction> Actions { get; set; }
	public ToolStrip ToolStrip { get; set; }
	public IList<ToolStripItem> ActionButtons
	{
		set => actionButtonManager.ActionButtons = value;
	}
	private Guid _iconId = Guid.Empty;
	[Browsable(false)]
	public Guid IconId
	{
		get => _iconId;
		set 
		{ 
			_iconId = value;
			SetPanelTitleIcon();
		}
	}
	[TypeConverter(typeof(GraphicsConverter))]
	public Schema.GuiModel.Graphics TitleIcon
	{
		get
		{
			ModelElementKey key = new ModelElementKey();
			key.Id = this.IconId;
			return (Schema.GuiModel.Graphics)_persistence.SchemaProvider.RetrieveInstance(typeof(AbstractSchemaItem), key);
		}
		set
		{
			if(value == null)
			{
				this.IconId = Guid.Empty;
			}
			else
			{
				this.IconId = (Guid)value.PrimaryKey["Id"];
			}
			SetPanelTitleIcon();
		}
	}
	/// <summary>
	/// Gets or sets a value indicating whether the New button 
	/// will be shown.
	/// </summary>
	/// <value>true if the New button is visible. The default is true.</value>
	/// 
	[DefaultValue(false), Category("Behavior"), 
	Description("Indicates whether New Button will be shown.")]
	public bool ShowNewButton
	{
		get => dbNav.ShowNewButton;
		set => dbNav.ShowNewButton = value;
	}
	[Browsable(false)]
	public bool OriginalShowNewButton { get; private set; }
	/// <summary>
	/// Gets or sets a value indicating whether the Copy button 
	/// will be hidden.
	/// </summary>
	/// <value>True if the Copy button is hidden. The default is false.</value>
	/// 
	[DefaultValue(false), Category("Behavior"), 
	Description("Indicates whether Copy Button will be hidden.")]
	public bool HideCopyButton { get; set; }
	/// <summary>
	/// Gets or sets a value indicating whether the Delete button 
	/// will be shown.
	/// </summary>
	/// <value>True if the Delete button is visible. The default is true.</value>
	/// 
	[DefaultValue(false), Category("Behavior"), 
	Description("Indicates whether Delete Button will be shown.")]
	public bool ShowDeleteButton
	{
		get => dbNav.ShowDeleteButton;
		set => dbNav.ShowDeleteButton = value;
	}
	[DefaultValue(true), Category("Behavior"), 
	Description("Indicates whether Grid/Form Toggle Button will be shown.")]
	public bool ShowGridButton
	{
		get => btnGrid.Visible;
		set 
		{
			btnGrid.Visible = value; 
			if(value)
			{
				toolBar.Width += 23;
			}
			else
			{
				toolBar.Width -= 23;
			}
		}
	}
	[DefaultValue(true), Category("Behavior"), 
	Description("Indicates whether Attachments Button will be shown.")]
	public bool ShowAttachmentsButton
	{
		get => btnAttachment.Visible;
		set 
		{
			btnAttachment.Visible = value; 
			if(value)
			{
				toolBar.Width += 23;
			}
			else
			{
				toolBar.Width -= 23;
			}
		}
	}
	[DefaultValue(true), Category("Behavior"), 
	Description("Indicates whether AuditLog Button will be shown.")]
	public bool ShowAuditLogButton
	{
		get => btnAuditLog.Visible;
		set 
		{
			btnAuditLog.Visible = value; 
			if(value)
			{
				toolBar.Width += 23;
			}
			else
			{
				toolBar.Width -= 23;
			}
		}
	}
	[Category("Misc"), 
	Description("Member will be treated as ordered - it will be read only and a special UI components will be available.")]
	public string OrderMember { get; set; }
	[Browsable(false)]
	public bool OriginalShowDeleteButton => _originalDisplayDeleteButton;
	[DefaultValue(false)]
	public bool GridVisible
	{
		get => btnGrid.Pushed;
		set
		{
			if(this.GridVisible == value) return;
			// store the current position
			object[] key = CurrentKey;
			// change grid visibility
			btnGrid.Pushed = value;
			ProcessGridBinding();
			// restore the saved position
			SetPosition(key);
		}
	}
	private bool _hideNavigationPanel = false;
	[DefaultValue(false)]
	public bool HideNavigationPanel
	{
		get => _hideNavigationPanel;
		set
		{
			_hideNavigationPanel = value;
			pnlDataControl.Visible = !value;
		}
	}
    [Description("This setting is only applied on the action buttons placed on the toolbar.")]
	public bool DisableActionButtons { get; set; }
	[Category("(ORIGAM)")]
	public string PanelTitle
	{
		get => this.pnlDataControl.PanelTitle;
		set => this.pnlDataControl.PanelTitle = value;
	}
	public string SelectionMember { get; set; }
	[Category("Calendar View")]
	public string CalendarDateDueMember { get; set; }
	[Category("Calendar View")]
	public string CalendarDateFromMember { get; set; }
	[Category("Calendar View")]
	public string CalendarDateToMember { get; set; }
	[Category("Calendar View")]
	public string CalendarNameMember { get; set; }
	[Category("Calendar View")]
	public string CalendarDescriptionMember { get; set; }
	[Category("Calendar View")]
	public string CalendarIsFinishedMember { get; set; }
	[Category("Calendar View")]
	public string CalendarResourceIdMember { get; set; }
	[Category("Calendar View")]
	public string CalendarResourceNameLookupField { get; set; }
	[Category("Calendar View"), DefaultValue(false)]
	public bool CalendarShowAllResources { get; set; }
	[Browsable(false)]
	public Guid CalendarRowHeightConstantId { get; set; }
	[TypeConverter(typeof(DataConstantConverter))]
	[Category("Calendar View")]
	public DataConstant CalendarRowHeight
	{
		get => (DataConstant)_persistence.SchemaProvider.RetrieveInstance(typeof(DataConstant), new ModelElementKey(this.CalendarRowHeightConstantId));
		set => this.CalendarRowHeightConstantId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
	}
	[Category("Calendar View")]
	public string CalendarCustomSortMember { get; set; } = null;
	[Category("Calendar View")]
	public AsPanelCalendarViewEnum DefaultCalendarView { get; set; }
	[Category("Calendar View")]
	public bool IsCalendarSupported { get; set; }
	[Category("Calendar View")]
	public bool IsCalendarVisible { get; set; }
	[Browsable(false)]
    public Guid CalendarViewStyleId { get; set; }
	[TypeConverter(typeof(StylesConverter))]
    [Category("Calendar View")]
    public UIStyle CalendarViewStyle
    {
        get => (UIStyle)_persistence.SchemaProvider.RetrieveInstance(
            typeof(UIStyle), new ModelElementKey(this.CalendarViewStyleId));
		set => this.CalendarViewStyleId = (value == null 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
	}
	[Category("Pipeline View")]
	public bool IsPipelineSupported { get; set; }
	[Category("Pipeline View")]
	public bool IsPipelineVisible { get; set; }
	[Category("Pipeline View")]
	public string PipelineNameMember { get; set; }
	[Category("Pipeline View")]
	public string PipelineDateMember { get; set; }
	[Category("Pipeline View")]
	public string PipelinePriceMember { get; set; }
	[Category("Pipeline View")]
	public string PipelineStateMember { get; set; }
	[Browsable(false)]
	public Guid PipelineStateLookupId { get; set; }
	[TypeConverter(typeof(DataLookupConverter))]
	[Category("Pipeline View")]
	public AbstractDataLookup PipelineStateLookup
	{
		get => (AbstractDataLookup)_persistence.SchemaProvider
			.RetrieveInstance(typeof(AbstractDataLookup),
				new ModelElementKey(this.PipelineStateLookupId));
		set => this.PipelineStateLookupId 
			= (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
	}
	[Category("Map View")]
	public string MapLocationMember { get; set; }
	[Category("Map View")]
	public string MapAzimuthMember { get; set; }
	[Category("Map View")]
	public string MapColorMember { get; set; }
	[Category("Map View")]
	public string MapIconMember { get; set; }
	[Category("Map View")]
	public string MapTextMember { get; set; }
	[Category("Map View")]
	public string MapTextLocationMember { get; set; }
	[Category("Map View")]
	public string MapTextRotationMember { get; set; }
	[Category("Map View")]
	public string MapTextColorMember { get; set; }
	[Category("Map View")]
	public string MapCenter { get; set; }
	[Category("Map View")]
	public string MapLayers { get; set; }
	[Category("Map View")]
	public int MapResolution { get; set; }
	[Category("Map View")]
	[DefaultValue(false)]
	public bool IsMapSupported { get; set; }
	[Category("Map View")]
	[DefaultValue(false)]
	public bool IsMapVisible { get; set; }
	[DefaultValue(false)]
	public bool IsGridHeightDynamic { get; set; } = false;
	[DefaultValue(0)]
	public int MaxDynamicGridHeight { get; set; } = 0;
	[DefaultValue(false)]
	public bool NewRecordInDetailView { get; set; } = false;
	public string ImplicitFilter { get; set; } = null;
	[DefaultValue(false), Category("Drag & Drop")]
	public bool IsDraggingEnabled { get; set; } = false;
	[Category("Drag & Drop")]
	public string DraggingLabelMember { get; set; } = null;
	[Category("Visual Editor View")]
	public bool IsVisualEditorSupported { get; set; }
	[Category("Visual Editor View")]
	public bool IsVisualEditorVisible { get; set; }
	#endregion
	#region Private Properties
	private bool ShowGrid
	{
		get => this.btnGrid.Pushed;
		set
		{
			this.btnGrid.Pushed = value;
			if(_userConfig != null)
			{
				_userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["DefaultView"] = (value ? OrigamPanelViewMode.Grid : OrigamPanelViewMode.Form);
				SaveUserConfig();
			}
			ProcessGridBinding();
		}
	}
	#endregion
	#region Overriden Methods
	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
		_internalDisposing = true;
		if( disposing )
		{
			Generator?.SetTooltip(this.pnlDataControl, null);
			this.toolBar.ButtonClick -= this.toolBar_ButtonClick;
			this.toolBar.Validated -= this.toolBar_Validated;
			this.filterMenu.Popup -= this.filterMenu_Popup;
			this.pnlDataControl.Click -= this.pnlDataControl_Click;
            if (!_filterMenuDisposed)
            {
                this.mnuUnsetDefaultFilter.Click -= this.mnuUnsetDefaultFilter_Click;
                this.mnuSetDefaultFilter.Click -= this.mnuSetDefaultFilter_Click;
                this.mnuFilterAdd.Click -= this.mnuFilterAdd_Click;
                this.mnuFilterDelete.Click -= this.mnuFilterDelete_Click;
                this.mnuFilterClear.Click -= this.mnuFilterClear_Click;
                filterMenu.Dispose();
            }
			_persistence = null;
			if(_filterFactory != null)
			{
				_filterFactory.DataViewQueryChanged -=filterFactory_DataViewQueryChanged; 
				_filterFactory.Dispose();
				_filterFactory = null;
			}
			if(errorProvider1 != null)
			{
				errorProvider1.DataSource = null;
				errorProvider1.Dispose();
				errorProvider1 = null;
			}
			if(_gridBuilder != null)
			{
				_gridBuilder.Dispose();
				_gridBuilder = null;
			}
			this.Leave -= AsPanel_Leave;
			if(_bindingManager != null)
			{
				UnsubscribeBindingManagerEvents();
				_bindingManager = null;
			}
			if(grid != null)
			{
				grid.VisibleChanged -= grid_VisibleChanged;			
				grid.Dispose();
				grid = null;
			}
			if(_attachmentPad != null)
			{
				_attachmentPad.AttachmentsUpdated -= _attachmentPad_AttachmentsUpdated;
				_attachmentPad = null;
			}
			actionButtonManager.Dispose();
			_dataSource = null;
			Generator = null;
			_auditLogPad = null;
			components?.Dispose();
		}
		base.Dispose( disposing );
	}
	protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
	{
		if(IgnoreEscape & keyData == Keys.Escape)
		{
			return false;
		}
		if(keyData == Keys.Insert & this.HideNavigationPanel == false)
		{
			ValidateChildren();
			this.dbNav.AddNewRow();
// will be called with dbNav_NewRecordAdded event				FocusControls();
			return true;
		}
		if(keyData == Keys.Escape & btnGrid.Pushed == false)
		{
			if(IsBinding())
			{
				ValidateChildren();
				_bindingManager.CancelCurrentEdit();
				FocusControls();
				return true;
			}
		}
		if(keyData == (Keys.Enter | Keys.Control))
		{
			if(IsBinding())
			{
				int cnt = _bindingManager.Count;
				try
				{
					Control focused = this.ActiveControl;
					this.EndEdit();
					if(cnt == _bindingManager.Count)
					{
                        ValidateChildren();
						this.Focus();
						focused?.Focus();
						FocusControls();
					}
				}
				catch(Exception ex)
				{
					AsMessageBox.ShowError(this.FindForm(), ex.Message, "Chyba v panelu '" + this.PanelTitle + "'", ex);
				}
				return true;
			}
		}
		if(keyData == (Keys.Delete | Keys.Control) & this.HideNavigationPanel == false)
		{
			this.dbNav.DeleteRow();
			FocusControls();
			return true;
		}
#if ORIGAM_CLIENT
		if(keyData == (Keys.A | Keys.Control))
		{
			this.ShowAttachments = ! this.ShowAttachments;
			FocusControls();
			return true;
		}
#endif
		if(keyData == (Keys.H | Keys.Control))
		{
			this.ShowAudit();
			return true;
		}
		if(keyData == (Keys.G | Keys.Control) & this.HideNavigationPanel == false)
		{
            ValidateChildren();
			this.ShowGrid = ! this.ShowGrid;
			FocusControls();
			return true;
		}
		if(keyData == (Keys.F | Keys.Control) & this.HideNavigationPanel == false)
		{
			this.FilterVisible = ! this.FilterVisible;
			return true;
		}
		if(keyData == (Keys.K | Keys.Control) & this.HideNavigationPanel == false)
		{
			if(this.ShowNewButton)
			{
				this.CopyData();
				return true;
			}
		}
		return base.ProcessCmdKey (ref msg, keyData);
	}
	protected override bool ProcessMnemonic(char charCode)
	{
		if(IsMnemonic(charCode))
		{
			FocusControls();
			return true;
		}
		return false;
	}
	#endregion
	#region Public Methods
	public void SetDataBinding(object dataSource, string dataMember)
	{
		this.DataSource = dataSource;
		this.DataMember = dataMember;
	}
	public bool EndEdit()
	{
		object[] key = CurrentKey;
		bool result = this.EndEdit(true);
		SetPosition(key);
		return result;
	}
    
    public override bool ValidateChildren()
    {
        if (this.GridVisible)
        {
            this.dbNav.Focus();
        }
        else
        {
            if (ActiveControl != null)
            {
	            this.Focus();
            }
        }
        return base.ValidateChildren();
    }
	public bool EndEdit(bool validate)
	{
        if (validate)
		{
            this.ValidateChildren();
		}
		if(IsBinding())
		{
			try
			{
                _bindingManager.EndCurrentEdit();
            }
			catch(Exception ex)
			{
				throw new Exception(ResourceUtils.GetString("ErrorWhenConfirm", this.PanelTitle), ex);
			}
		}
		_cachedFormattingId = Guid.Empty;
		try
		{
			UpdateColor();
		}
		catch(Exception ex)
		{
			AsMessageBox.ShowError(this.FindForm(), ex.Message, "Chyba pøi aktualizaci barvy", ex);
		}
		return true;
	}
	public void RefreshFilter()
	{
		_refreshingFilter = true;
		try
		{
			if(this.FilterActive)
			{
				_filterFactory.RefreshFilter();
			}
			RestorePosition();
		}
		finally
		{
			_refreshingFilter = false;
		}
	}
	public bool SetPosition(object[] primaryKey)
	{
		if(CurrentKey == null || ! CurrentKey.Equals(primaryKey))
		{
			_auditLogPad?.ClearList();
			CurrentKey = primaryKey;
			return RestorePosition();
		}
		return false;
	}
	public EntityFormatting Formatting(DataRow row, object id)
	{
		if(IsPkNull(id)) return null;
		RuleEngine ruleEngine = Generator.FormRuleEngine;
		if(ruleEngine == null) return null;
		Guid entityId = this.EntityId;
		if(entityId == Guid.Empty) return null;
		EntityFormatting formatting;
		if(id.Equals(_cachedFormattingId))
		{
			formatting = _cachedFormatting;
		}
		else
		{
			if(! DatasetTools.HasRowValidParent(row)) return null;
			XmlContainer data = DatasetTools.GetRowXml(row, DataRowVersion.Default);
			formatting = ruleEngine.Formatting(data, entityId, Guid.Empty, null);
		}
	
		_cachedFormatting = formatting;
		_cachedFormattingId = id;
		return formatting;
	}
	
	public bool IsColumnSorted(string columnName)
	{
		return CurrentSort.Contains(columnName);
	}
	public DataStructureColumnSortDirection ColumnSortDirection(string columnName)
	{
		if(! CurrentSort.Contains(columnName)) throw new ArgumentOutOfRangeException("columnName", columnName, ResourceUtils.GetString("ErrorSortDirection"));
		DataSortItem sortItem = CurrentSort[columnName] as DataSortItem;
		return sortItem.SortDirection;
	}
	public void Sort(string columnName, DataStructureColumnSortDirection sortDirection)
	{
		CurrentSort.Clear();
		AddSort(columnName, sortDirection);
	}
	public void AddSort(string columnName, DataStructureColumnSortDirection sortDirection)
	{
		if(CurrentSort.Contains(columnName)) throw new Exception(ResourceUtils.GetString("ErrorAlreadySorted", columnName));
		CurrentSort.Add(columnName, new DataSortItem(columnName, sortDirection, CurrentSort.Count));
		UpdateSorting();
		SetActualRecordId();
	}
	public void ReverseSort(string columnName)
	{
		if(! CurrentSort.Contains(columnName)) throw new ArgumentOutOfRangeException("columnName", columnName, ResourceUtils.GetString("ErrorReverseSort"));
		DataSortItem sortItem = CurrentSort[columnName] as DataSortItem;
		sortItem.SortDirection =
			sortItem.SortDirection == DataStructureColumnSortDirection.Ascending ? 
			DataStructureColumnSortDirection.Descending :
			DataStructureColumnSortDirection.Ascending;
		UpdateSorting();
		SetActualRecordId();
	}
	#endregion
	#region Private Methods
	private void LoadUserConfig()
	{
		AsForm form = this.FindForm() as AsForm;
		if(form != null)
		{
            UserProfile profile = SecurityManager.CurrentUserProfile();
            _userConfig = OrigamPanelConfigDA.LoadConfigData(this.FormPanelId, form.WorkflowId, profile.Id);
			// Create default config if none exists
			if(_userConfig.Tables["OrigamFormPanelConfig"].Rows.Count == 0)
			{
				OrigamPanelConfigDA.CreatePanelConfigRow(_userConfig.Tables["OrigamFormPanelConfig"], this.FormPanelId, form.WorkflowId, profile.Id, (this.ShowGrid ? OrigamPanelViewMode.Grid : OrigamPanelViewMode.Form));
			}
			else
			{
				if(! this.HideNavigationPanel)
				{
					// there is a config already, so we set it
					DataRow configRow = _userConfig.Tables["OrigamFormPanelConfig"].Rows[0];
					btnGrid.Pushed = (OrigamPanelViewMode)configRow["DefaultView"] != OrigamPanelViewMode.Form;
					ProcessGridBinding();
				}
			}
		}
	}
	
	private void SaveUserConfig()
	{
		if(_userConfig == null) return;
		AsForm form = this.FindForm() as AsForm;
		if(form == null) return;
        UserProfile profile = SecurityManager.CurrentUserProfile();
		try
		{
			OrigamPanelConfigDA.SaveUserConfig(_userConfig, this.FormPanelId, form.WorkflowId, profile.Id);
		}
		catch(Exception ex)
		{
			AsMessageBox.ShowError(this.FindForm(), ResourceUtils.GetString("ErrorWhenSavingConfig", this.PanelTitle), ResourceUtils.GetString("SaveConfigTitle"), ex);
		}
	}
	public void FocusGridFirstColumn()
	{
		if(btnGrid.Pushed)
		{
			if((grid as AsDataGrid).CurrentCell.ColumnNumber != 0)
			{
				(grid as AsDataGrid).CurrentCell = new DataGridCell((grid as AsDataGrid).CurrentRowIndex, 0);
			}
		}
		else
		{
			this.Focus();
			this.SelectNextControl(this, true, true, true, true);
		}
	}
	public void FocusControls()
	{
		if(_internalDisposing | this.Focused | this.DesignMode | this.Disposing | _refreshingFilter | _settingDataSource | (this.Generator != null && this.Generator.IgnoreDataChanges)) return;
		this.Focus();
		if(this.btnGrid.Pushed)
		{
			// grid mode
			if(grid != null)
			{
				try
				{
					if(! this.grid.Focused)
					{
						this.grid.Focus();
					}
					(grid as AsDataGrid).InvokeOnEnter();
				}
				catch {}
			}
		}
	}
	private void UpdateTooltip(DataRow row)
	{
		Generator.SetTooltip(this.pnlDataControl, "");
		if(row == null) return;
		string recordCreatedByName = "";
		string recordCreatedDate = "";
		string recordUpdatedByName = "";
		string recordUpdatedDate = "";
		string idText = "";
		if(row.Table.PrimaryKey.Length > 0)
		{
			string pkName = row.Table.PrimaryKey[0].ColumnName;
			if(row.Table.Columns.Contains(pkName))
			{
				idText = row[pkName].ToString();
			}
		}
		if(row.Table.Columns.Contains("RecordCreated"))
		{
			recordCreatedDate = row["RecordCreated"].ToString();
		}
		if(row.Table.Columns.Contains("RecordUpdated"))
		{
			recordUpdatedDate = row["RecordUpdated"].ToString();
		}
		if(row.Table.Columns.Contains("RecordCreatedBy"))
		{
			object id = row["RecordCreatedBy"];
			if(id != DBNull.Value)
			{
				try
				{
					IOrigamProfileProvider profileProvider = SecurityManager.GetProfileProvider();
					UserProfile profile = profileProvider.GetProfile((Guid)id) as UserProfile;
					recordCreatedByName = profile.FullName;
				}
				catch(Exception ex)
				{
					recordCreatedByName = "Chyba: " + ex.Message;
				}
			}
		}
		if(row.Table.Columns.Contains("RecordUpdatedBy"))
		{
			object id = row["RecordUpdatedBy"];
			if(id != DBNull.Value)
			{
				try
				{
					IOrigamProfileProvider profileProvider = SecurityManager.GetProfileProvider();
					UserProfile profile = profileProvider.GetProfile((Guid)id) as UserProfile;
					recordUpdatedByName = profile.FullName;
				}
				catch(Exception ex)
				{
					recordCreatedByName = "Chyba: " + ex.Message;
				}
			}
		}
		string tooltip = ResourceUtils.GetString("TooltipSegmentCreated") + recordCreatedByName + (recordCreatedDate != "" ? " (" + recordCreatedDate + ")" : "")
			+ Environment.NewLine
			+ ResourceUtils.GetString("TooltipSegmentLastChange")  + recordUpdatedByName + (recordUpdatedDate != "" ? " (" + recordUpdatedDate + ")" : "")
			+ Environment.NewLine
			+ ResourceUtils.GetString("TooltipSegmentId") + idText;
		Generator.SetTooltip(this.pnlDataControl, tooltip);
	}
	private bool RestorePosition()
	{
		bool result = false;
		UpdateErrorProvider();
		if(this._bindingManager != null && this._bindingManager.List is DataView)
		{
			if(CurrentKey == null & this._bindingManager.Position >= 0 && this._bindingManager.Current is DataRowView)
			{
				this.OnRecordIdChanged((this._bindingManager.Current as DataRowView).Row);
			}
			CurrencyManager cm = this._bindingManager;
			DataView view = (DataView)cm.List;
			try
			{
				if(CurrentKey != null && CurrentKey.Length > 0)
				{
					DataRow foundRow = view.Table.Rows.Find(CurrentKey);
					
					if(foundRow != null)
					{
						int i = 0;
						foreach(DataRowView rv in view)
						{
							if(rv.Row == foundRow)
							{
								if(cm.Position == i)
								{
									// We have to call this even if the row is actual, because of delayed data loading
									// The delayed data loading has to reload the details of this master (if any).
									OnRecordIdChanged(foundRow);
								}
								else
								{
									cm.Position = i;
								}
								result = true;
								break;
							}
							i++;
						}
					}
				}
			}
			catch{}
		}
		FocusControls();
		return result;
	}
	private bool IsMnemonic(char charCode)
	{
		if(this.PanelTitle == null) return false;
		int i = this.PanelTitle.IndexOf("&");
		if(i >= 0 & i < this.PanelTitle.Length)
		{
			if(String.Compare(charCode.ToString(), this.PanelTitle.Substring(i+1, 1), true) == 0)
			{
				return true;
			}
		}
		return false;
	}
	
	private DataSet ConvertInputData(object data)
	{
		//all other datasources has to be implemented
		// DataTable, DataView, DataSet, DataViewManager, 
		// IListSource, IList
		if(data is DataSet)
		{
			return (DataSet)data;
		}
		else
		{
			return null;
		}
			
	}
	/// <summary>
	/// This method provides functionality around controls and their DataBindings
	/// when datasource or datamember is changed we must rearrange all bindings
	/// </summary>
	private void ProcessBindings()
	{
		this.pnlFilter.Width  = this.Width;
		if(	this._dataSource == null	|| this.DataMember == null || 
			this.DataMember == ""		|| this.DesignMode)
			return;
		try
		{
			_bindingManager = this.BindingContext[this._dataSource,this.DataMember] as CurrencyManager;
		}
		catch
		{
			// no rows, no binding manager?
			return;
		}
		
		if(_filterFactory == null)
		{
			_filterFactory = new DataGridFilterFactory(this.pnlFilter, this.Generator, this.DataMember, this.PanelUniqueId, _gridBuilder);
			_filterFactory.DataViewQueryChanged += filterFactory_DataViewQueryChanged; 
			_gridBuilder.FilterFactory = _filterFactory;
		}
		if(grid == null)
		{
			// grid will be added to Controls collection by the factory, so we don't have to add it on the panel
			grid = _gridBuilder.CreateGrid(this.DataSource, this.DataMember, this, this.FormPanelId, this.Generator.FormRuleEngine);
			grid.HandleCreated += grid_HandleCreated;
			grid.VisibleChanged += grid_VisibleChanged;	
			grid.DoubleClick +=
				(sender, args) => actionButtonManager.RunDefaultAction();
			grid.EditorDoubleClicked +=
				(sender, args) => actionButtonManager.RunDefaultAction();
		}
		
		BindGrid();
		this.dbNav.SetDataBinding(this._dataSource,this.DataMember);
		
		SubscribeBindingManagerEvents();
		ProcessGridBinding();
		AsForm form = this.FindForm() as AsForm;
		if(! _defaultFilterUsed & _userConfig != null & ! form.SingleRecordEditing)
		{
			// Load default filter, if it exists
			DataRow configRow = _userConfig.Tables["OrigamFormPanelConfig"].Rows[0];
			if(configRow["refOrigamPanelFilterId"] != DBNull.Value)
			{
				OrigamPanelFilter filter = _filterFactory.LoadFilter((Guid)configRow["refOrigamPanelFilterId"]);
				if(filter.PanelFilter.Rows.Count > 0)
				{
					_filterFactory.ApplyFilter(filter.PanelFilter.Rows[0] as OrigamPanelFilter.PanelFilterRow);
					this.FilterVisible = true;
				}
			}
			// after setting the default filter, we don't do that again (e.g. if refreshing data)
			_defaultFilterUsed = true;
		}
	}
    private CurrencyManager ParentManager(CurrencyManager currentManager)
    {
        if (currentManager != null && currentManager.GetType().Name == "RelatedCurrencyManager")
        {
            string propertyName = "parentManager";
            return Reflector.GetValue(currentManager.GetType(), currentManager, propertyName) as CurrencyManager;
        }
        else
        {
            return null;
        }
    }
	private static bool IsManagerBinding(CurrencyManager cm)
	{
		if(cm == null) return false;
		return (bool)Reflector.GetValue(typeof(CurrencyManager), cm, "IsBinding");
	}
	private bool IsParentManagerBinding(CurrencyManager currentManager)
	{
		CurrencyManager parentManager = this.ParentManager(currentManager);
		if(parentManager == null)
		{
			return true;
		}
		else
		{
			return IsManagerBinding(parentManager);
		}
	}
    public void BindGrid(bool isBinding)
	{
		if(grid == null) return;
		if(isBinding & !this.HideNavigationPanel)
		{
			_gridBuilder.UpdateDataSource(grid, this.DataSource, this.DataMember);
		}
		else
		{
			_gridBuilder.UpdateDataSource(grid, null, this.DataMember);
		}
	}
	public void BindGrid()
	{
		CurrencyManager cm = _bindingManager;
		if(IsBinding(cm) == false || this.IsParentManagerBinding(cm) == false || this.GridVisible == false)
        {
            BindGrid(false);
        }
		else
		{
			BindGrid(true);
		}
	}
	private void ManageChildBindingContexts(CurrencyManager currentManager, bool suspend)
	{
		if((this.FindForm() as AsForm).PanelBindingSuspendedTemporarily) return;
		foreach(DictionaryEntry entry in this.BindingContext as IEnumerable)
		{
			CurrencyManager childManager = (entry.Value as WeakReference).Target as CurrencyManager;
			CurrencyManager parentManager = this.ParentManager(childManager);
			if(parentManager == currentManager)
			{
				if(suspend)
				{
					try
					{
						if(IsManagerBinding(childManager)) childManager.SuspendBinding();
					}
					catch {}
				}
				else
				{
					if(! IsManagerBinding(childManager)) childManager.ResumeBinding();
				}
			}
		}
	}
	private object[] PositionKey(DataRow row)
	{
		return DatasetTools.PrimaryKey(row);
	}
	bool _noRecordIdChangedRecursion = false;
	private void OnRecordIdChanged(DataRow row)
	{
		if(Generator.IgnoreDataChanges) return;
		if(_noRecordIdChangedRecursion) return;
		bool raiseRecordIdChangedEvent = true;
		try
		{
			if(row == null)
			{
				CurrentKey = null;
			}
			else
			{
				CurrentKey = PositionKey(row);
				// don't do anything if there is no primary key
				if(CurrentKey.Length > 0)
				{
					// store the "Id" column
					string pkName = row.Table.PrimaryKey[0].ColumnName;
					object recordId = row[pkName];
					if(row.RowState == DataRowState.Unchanged)
					{
						try
						{
							if(Generator.LoadDataPiece(recordId, this.DataMember))
							{
								foreach(DataColumn col in row.Table.Columns)
								{
									this.UpdateTempSortColumn(col, row, LookupId(col.ColumnName), true);
								}
								ManageChildBindingContexts(_bindingManager, false);
							}
						}
						catch(Exception ex)
						{
							AsMessageBox.ShowError(this.FindForm(), ex.Message, "Chyba pøi naèítání detailù", ex);
						}
					}
					if(recordId == RecordId) 
					{
						raiseRecordIdChangedEvent = false;
					}
					else
					{
						RecordId = recordId;
					}
				}
			}
			if(raiseRecordIdChangedEvent & RecordIdChanged != null)
			{
				RecordIdChanged(this, EventArgs.Empty);
			}
		}
		catch{}
		finally
		{
			UpdateFilter();
			UpdateSorting();
			UpdateErrorProvider();
			actionButtonManager.UpdateActionButtons();
			UpdateTooltip(row);
			UpdateRowLevelSecurity();
			UpdateColor();
			UpdateAttachmentIcon(row);
        }
	}
	private void UpdateErrorProvider()
	{
		CurrencyManager cm = this._bindingManager;
		if(IsBinding(cm) == false || this.IsParentManagerBinding(cm) == false)
		{
			if(this.errorProvider1 != null)
			{
				ResetErrors(this);
				this.errorProvider1.DataSource = null;
				this.errorProvider1.Dispose();
				this.errorProvider1 = null;
			}
		}
		else
		{
			if(this.errorProvider1 == null)
			{
				this.errorProvider1 = new ErrorProvider(this);
				this.errorProvider1.BindToDataAndErrors(this.DataSource, this.DataMember);
			}
		}
	}
	/// <summary>
	/// Resets all the error icons, because sometimes they hang on the form even thouth errorProvider is reset.
	/// </summary>
	/// <param name="control">Control on which child controls to reset the errors.</param>
	private void ResetErrors(Control control)
	{
		foreach(Control child in control.Controls)
		{
			ResetErrors(child);
		}
		this.errorProvider1.SetError(control, "");
	}
	private void UpdateControlsForZeroRows(Control control)
	{
		if((this.FindForm() as AsForm).PanelBindingSuspendedTemporarily) return;
		bool enable;
		if(IsBinding())
		{
			return;	// we have to return, because enable state was set by the row-level-security setting
		}
		else
		{
			enable = false;
			this.ShowNewButton = OriginalShowNewButton;
			this.ShowDeleteButton = _originalDisplayDeleteButton;
		}
		if(! btnGrid.Pushed)
		{
			foreach(Control item in control.Controls)
			{
				if(item is IAsControl)
				{
					item.Enabled = enable;
					item.Visible = ( item is IAsCaptionControl && (item as IAsCaptionControl).HideOnForm ? false : true);
				}
				if(item.Controls.Count > 0)
				{
					UpdateControlsForZeroRows (item);
				}
			}
		}
	}
	private bool IsBinding()
	{
		return IsBinding(_bindingManager);
	}
	private bool IsBinding(BindingManagerBase bindingManager)
	{
		if(bindingManager == null ||  bindingManager.Position < 0 || IsManagerBinding(bindingManager as CurrencyManager) == false)
        {
            return false;
        }
		else
		{
			return true;
		}
	}
	private void VisibleControl(Control control, bool visible)
	{
		foreach(Control item in control.Controls)
		{
			if(item is IAsControl)
			{
				if(visible)
				{
					item.Visible = (item is IAsCaptionControl && (item as IAsCaptionControl).HideOnForm ? false : true);
				}
				else
				{
					item.Visible = false;
				}
				item.Enabled = visible;
			}
			if(item.Controls.Count>0 && (item != this.pnlFilter) )
			{
				VisibleControl (item, visible);
			}
		}
	}
	private void OnShowAttachmentsChanged()
	{
		SetActualRecordId();
		ShowAttachmentsChanged?.Invoke(this, EventArgs.Empty);
		if(this.ShowAttachments)
		{
			Workbench.Commands.ViewAttachmentPad cmd = new Workbench.Commands.ViewAttachmentPad();
			cmd.Run();
		}
	}
	public void SetActualRecordId()
	{
		//Send current Id 
		CurrencyManager cm = _bindingManager;
		if(IsBinding())
		{
			DataRowView dView=(cm.Current as DataRowView);
			OnRecordIdChanged(dView.Row);
		}
		else
		{
			OnRecordIdChanged(null);
		}
	}
	private void ShowAudit()
	{
		SetActualRecordId();
		if(this.RecordId is Guid)
		{
			_auditLogPad.GetAuditLog(this.EntityId, (Guid)this.RecordId, null);
				
			Workbench.Commands.ViewAuditLogPad cmd = new Workbench.Commands.ViewAuditLogPad();
			cmd.Run();
		}
	}
	private void ProcessGridBinding()
	{
		if(this._dataSource == null	|| this.DataMember == null || 
			this.DataMember == ""  || this.DesignMode)
		{
			return;
		}	
		if(btnGrid.Pushed)
		{
			DisableFormViewBinding();
		}
		else
		{
			EnableFormViewBinding();
		}
		actionButtonManager.UpdateActionButtons();
		FocusControls();
	}
	private void EnableFormViewBinding()
	{
		// enable form view binding
		{
			Generator.BindControls(this);
		}
		BindGrid(false);
		if (btnFilter.Pushed)
		{
			this.FilterVisible = false;
		}
		if (IsBinding())
		{
			VisibleControl(this as Control, true);
			UpdateRowLevelSecurity(true);
			UpdateColor();
		} else
		{
			UpdateControlsForZeroRows(this);
		}
		this.AutoScroll = true;
		grid.Visible = false;
		grid.Enabled = false;
		grid.Dock = DockStyle.None;
		grid.Left = (grid.Width + 20) * -1;
		grid.Top = (grid.Height + 20) * -1;
		grid.SendToBack();
	}
	private void DisableFormViewBinding()
	{
		// disable form view binding
		{
			BindingsCollection bindingsCollection =
				BindingContext[DataSource, DataMember].Bindings;
			IList<Binding> toRemove = new List<Binding>();
			foreach (Binding binding in bindingsCollection)
			{
				if (FormGenerator.IsChildControl(this, binding.Control))
				{
					System.Diagnostics.Debug.WriteLine(
						binding.BindingMemberInfo.BindingField, "Removing binding");
					toRemove.Add(binding);
				}
			}
			foreach (var binding in toRemove)
			{
				FormGenerator.RemoveBinding(binding.Control, binding);
			}
		}
		VisibleControl(this as Control, false);
		this.AutoScroll = false;
		BindGrid(IsBinding());
		grid.Visible = true;
		grid.Enabled = true;
		grid.BringToFront();
		grid.Dock = DockStyle.Fill;
		grid.Controls[0].Enabled = true; // Index zero is the horizontal scrollbar
		grid.Controls[1].Enabled = true; // Index one is the vertical scrollbar
	}
	
	private void SetPanelTitleIcon()
	{
		pnlDataControl.PanelIcon = _iconId != Guid.Empty ? this.TitleIcon.GraphicsData : null;
	}
	private void CopyData()
	{
		try
		{
			if(_bindingManager.Position >= 0)
			{
				DataRow row = (_bindingManager.Current as DataRowView).Row;
		
                DataSet tmpDS = DatasetTools.CloneDataSet(row.Table.DataSet, false);
                object profileId = SecurityManager.CurrentUserProfile().Id;
				var toSkip = new List<string>();
				var toAdd = new List<string>();
				foreach(AsPanel panel in (this.FindForm() as AsForm).Panels)
				{
					if(panel.OriginalShowNewButton)
					{
						toAdd.Add(FormTools.FindTableByDataMember(tmpDS, panel.DataMember));
					}
				}
				foreach(AsPanel panel in (this.FindForm() as AsForm).Panels)
				{
					string tableName = FormTools.FindTableByDataMember(tmpDS, panel.DataMember);
					if(! panel.OriginalShowNewButton && ! toAdd.Contains(tableName))
					{
						toSkip.Add(tableName);
					}
				}
				DatasetTools.GetDataSlice(tmpDS, new List<DataRow>{row}, 
                    profileId, true, toSkip);
				
				try
				{
					row.Table.DataSet.EnforceConstraints = false;
					this.EndEdit();
					Generator.IgnoreDataChanges = true;
					DatasetTools.MergeDataSet(row.Table.DataSet, tmpDS, null, new MergeParams(profileId));
					Generator.IgnoreDataChanges = false;
					SetPosition(PositionKey(tmpDS.Tables[row.Table.TableName].Rows[0]));
					
					if(_bindingManager.Position >= 0)
					{
						DataRow newRow = (_bindingManager.Current as DataRowView).Row;
						Generator.table_rowCopied(newRow, Generator.XmlData);
					}
					this.EndEdit();
					this.FocusGridFirstColumn();
				}
				finally
				{
					row.Table.DataSet.EnforceConstraints = true;
				}
			}
		}
		catch(Exception ex)
		{
			AsMessageBox.ShowError(this.FindForm(), ResourceUtils.GetString("ErrorWhenCopy"), ResourceUtils.GetString("ErrorWhenCopyTitle"), ex);
		}
	}
	private object _lastRowLevelSecurityRecordId = null;
    private void UpdateRowLevelSecurity()
    {
        UpdateRowLevelSecurity(false);
    }
	private void UpdateRowLevelSecurity(bool forceUpdate)
	{
		if(! forceUpdate && _lastRowLevelSecurityRecordId != null &&
			_lastRowLevelSecurityRecordId == this.RecordId)
		{
			return;
		}
		if((this.FindForm() as AsForm).PanelBindingSuspendedTemporarily) return;
		if(! IsBinding()) return;
	
		RuleEngine ruleEngine = Generator.FormRuleEngine;
		if(ruleEngine == null) return;
		Guid entityId = this.EntityId;
		if(entityId == Guid.Empty) return;
		Guid fieldId = Guid.Empty;
		DataRow row = (this._bindingManager.Current as DataRowView).Row;
		if(! DatasetTools.HasRowValidParent(row)) return;
		bool isNewRow = (row.RowState == DataRowState.Added | row.RowState == DataRowState.Detached);
		XmlContainer originalData = DatasetTools.GetRowXml(row, DataRowVersion.Original);
		XmlContainer actualData = DatasetTools.GetRowXml(row, row.HasVersion(DataRowVersion.Proposed) ? DataRowVersion.Proposed : DataRowVersion.Default);
		if(! this.GridVisible) 
		{
			foreach(Control control in this.BoundControls)
			{
				foreach(Binding b in control.DataBindings)
				{
					string field = b.BindingMemberInfo.BindingField;
					fieldId = (Guid)row.Table.Columns[field].ExtendedProperties["Id"];
					control.Enabled = ruleEngine.EvaluateRowLevelSecurityState(originalData, actualData, field, CredentialType.Update, entityId, fieldId, isNewRow);
					if(control is IAsCaptionControl)
					{
						string caption = ruleEngine.DynamicLabel(actualData, entityId, fieldId, null);
						if(caption != null) (control as IAsCaptionControl).Caption = caption;
					}
					if(control is IAsCaptionControl && (control as IAsCaptionControl).HideOnForm)
					{
						control.Visible = false;
					}
					else
					{
						control.Visible = ruleEngine.EvaluateRowLevelSecurityState(originalData, actualData, field, CredentialType.Read, entityId, fieldId, isNewRow);
					}
				}
			}
		}
		if(_originalDisplayDeleteButton)
		{
			this.ShowDeleteButton = ruleEngine.EvaluateRowLevelSecurityState(originalData, actualData, null, CredentialType.Delete, entityId, fieldId, isNewRow);
		}
		if(OriginalShowNewButton)
		{
			this.ShowNewButton = ruleEngine.EvaluateRowLevelSecurityState(originalData, actualData, null, CredentialType.Create, entityId, fieldId, isNewRow);
		}
		_lastRowLevelSecurityRecordId = this.RecordId;
	}
	private object _cachedFormattingId = null;
	private EntityFormatting _cachedFormatting = null;
	private bool IsPkNull(object pk)
	{
		if(pk == null) return true;
		if(pk is Guid && (Guid)pk == Guid.Empty) return true;
		return false;
	}
	private void UpdateColor()
	{
		try
		{
			if(! IsBinding() || IsPkNull(this.RecordId)) 
			{
				SetStandardColors();
				return;
			}
			DataRow row = (this._bindingManager.Current as DataRowView).Row;
			EntityFormatting formatting = this.Formatting(row, this.RecordId);
	
			if(formatting == null)
			{
				SetStandardColors();
			}
			else
			{
				bool isActive = IsActive();
				SetStandardStartColor(isActive);
				SetStandardMiddleStartColor(isActive);
				SetStandardMiddleEndColor(isActive);
				
				if(formatting.UseDefaultBackColor) 
				{
					SetStandardEndColor(isActive);
				}
				else
				{
					this.pnlDataControl.EndColor = formatting.BackColor;
				}
				if(formatting.UseDefaultForeColor)
				{
					SetStandardForeColor(isActive);
				}
				else
				{
					this.pnlDataControl.ForeColor = formatting.ForeColor;
				}
			}
		}
		catch(Exception ex)
		{
			throw new Exception(ResourceUtils.GetString("ErrorWhenColorUpdate"), ex);
		}
	}
	private void UpdateAttachmentIcon()
	{
		if(IsBinding()) 
		{
			DataRow row = (this._bindingManager.Current as DataRowView).Row;
			UpdateAttachmentIcon(row);
		}
	}
	private long _attachmentCount = 0;
	private void UpdateAttachmentIcon(DataRow row)
	{
		if(row == null) return;
		if(!row.Table.Columns.Contains(Const.ValuelistIdField)) return;
		if(!(row[Const.ValuelistIdField] is Guid)) return;
		
		OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
		btnAttachment.ToolTipText = ResourceUtils.GetString("TooltipAttachment");
		btnAttachment.ImageIndex = 1;
		if(! settings.CheckAttachmentsOnRecordSelection) 
		{
			btnAttachment.ImageIndex = 7;
			return;
		}
		if(!(row != null && row.Table.Columns.Contains(Const.ValuelistIdField))) return;
		AsForm form = this.ParentForm as AsForm;
		Hashtable references = new Hashtable();
		RecordReference newRef = new RecordReference(this.EntityId, (Guid)row[Const.ValuelistIdField]);
		references.Add(newRef.GetHashCode(), newRef);
		form.RetrieveChildReferences(row, references);
		if(references.Count > 1000)
		{
			btnAttachment.ImageIndex = 7;
			return;
		}
		CalculateAttachmentCount(references);
	}
	private void CalculateAttachmentCount(Hashtable references)
	{
		IDataLookupService ls = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
		long count = 0;
		foreach(RecordReference r in references.Values)
		{
			lock(ls)
			{
				object result = ls.GetDisplayText(new Guid("fbf2cadd-e529-401d-80ce-d68de0a89f13"), r.RecordId, false, false, null);
				if(result is int)
				{
					result = (int)result;
				}
				count += (long)result;
			}
		}
		_attachmentCount = count;
		this.AttachmentCountCalculated(this, EventArgs.Empty);
	}
	private void AsPanel_AttachmentCountCalculated(object sender, EventArgs e)
	{
		btnAttachment.ToolTipText = ResourceUtils.GetString("TooltipAttachment");
		if(_attachmentCount > 0)
		{
			btnAttachment.ImageIndex = 5;
			btnAttachment.ToolTipText = ResourceUtils.GetString("TooltipSegmentAttachmentCount") + _attachmentCount.ToString() + Environment.NewLine + btnAttachment.ToolTipText;
		}
		else
		{
			btnAttachment.ImageIndex = 1;
		}
	}
	private bool IsActive()
	{
		return (this.ContainsFocus | _entering) & !_leaving;
	}
	private void SetStandardColors()
	{
		bool isActive = IsActive();
		SetStandardStartColor(isActive);
		SetStandardEndColor(isActive);
		SetStandardMiddleStartColor(isActive);
		SetStandardMiddleEndColor(isActive);
		SetStandardForeColor(isActive);
	}
	private void SetStandardStartColor(bool isActive)
	{
		this.pnlDataControl.StartColor = (isActive ? OrigamColorScheme.TitleActiveStartColor : OrigamColorScheme.TitleInactiveStartColor);
	}
	private void SetStandardEndColor(bool isActive)
	{
		this.pnlDataControl.EndColor = (isActive ? OrigamColorScheme.TitleActiveEndColor : OrigamColorScheme.TitleInactiveEndColor);;
	}
	private void SetStandardMiddleStartColor(bool isActive)
	{
		this.pnlDataControl.MiddleStartColor = (isActive ? OrigamColorScheme.TitleActiveMiddleStartColor : OrigamColorScheme.TitleInactiveMiddleStartColor);
	}
	private void SetStandardMiddleEndColor(bool isActive)
	{
		this.pnlDataControl.MiddleEndColor = (isActive ? OrigamColorScheme.TitleActiveMiddleEndColor : OrigamColorScheme.TitleInactiveMiddleEndColor);
	}
	private void SetStandardForeColor(bool isActive)
	{
		this.pnlDataControl.ForeColor = (isActive ? OrigamColorScheme.TitleActiveForeColor : OrigamColorScheme.TitleInactiveForeColor);
	}
	private ArrayList BoundControls
	{
		get
		{
			ArrayList list = new ArrayList();
			GetBoundControls(this, list);
			return list;
		}
	}
	private void GetBoundControls(Control control, ArrayList list)
	{
		foreach(Control child in control.Controls)
		{
			if(child.DataBindings.Count > 0)
			{
				list.Add(child);
			}
			GetBoundControls(child, list);
		}
	}
    bool _updatingFilter = false;
	private void UpdateFilter()
	{
        if (!this.FilterVisible ||  _updatingFilter)
        {
            return;
        }
        _updatingFilter = true;
        try
        {
            if (_bindingManager.List != null)
            {
                DataView view = _bindingManager.List as DataView;
                string query = _filterFactory.Query;
                if (view.RowFilter != query)
                {
                    view.RowFilter = query;
                    this.EndEdit(false);
                }
                if (query == null | query == "")
                {
                    pnlDataControl.StatusIcon = null;
                }
                else
                {
                    pnlDataControl.StatusIcon = imageList1.Images[6] as Bitmap;
                }
            }
        }
        finally
        {
            _updatingFilter = false;
        }
	}
	public void UpdateSorting()
	{
		if(Generator.IgnoreDataChanges) return;
		if(this.HideNavigationPanel) return;
		if(_bindingManager == null) return;
		DataView view = this._bindingManager.List as DataView;
		if(view == null) return;
		StringBuilder sortString = new StringBuilder();
		ArrayList sortList = new ArrayList(CurrentSort.Values);
		sortList.Sort();
		foreach(DataSortItem item in sortList)
		{
			if(sortString.Length > 0) sortString.Append(", ");
			sortString.Append(GetSortColumn(item.ColumnName, view.Table));
			sortString.Append(" ");
			sortString.Append(item.SortDirection == DataStructureColumnSortDirection.Ascending ? "ASC" : "DESC");
		}
		string finalSort = sortString.ToString();
		if(view.Sort == finalSort) return;
        int position = 0;
        if (grid != null)
        {
            position = (grid as AsDataGrid).HorizontalScrollPosition;
        }
		bool prevIgnoreDataChanges = Generator.IgnoreDataChanges;
		try
		{
			Generator.IgnoreDataChanges = true;
			if(view.Count > 1)
			{
				 view.Sort = finalSort;
			}
		}
		finally
		{
			Generator.IgnoreDataChanges = prevIgnoreDataChanges;
            if (grid != null)
            {
                (grid as AsDataGrid).HorizontalScrollPosition = position;
            }
		}
	}
	#endregion
	#region Event Handlers
	private void mnuFilterClear_Click(object sender, EventArgs e)
	{
		this._filterFactory.ClearQueryFields();
	}
	private void mnuFilterAdd_Click(object sender, EventArgs e)
	{
		StoredFilterProperties dialog = new StoredFilterProperties();
		dialog.ShowDialog();
		if(!dialog.Canceled)
		{
			this._filterFactory.StoreCurrentFilter(dialog.txtFilterName.Text, dialog.chkIsGlobal.Checked);
		}
	}
	private void mnuFilterDelete_Click(object sender, EventArgs e)
	{
		if(_filterFactory.CurrentStoredFilter != null)
		{
			try
			{
				_filterFactory.DeleteFilter(_filterFactory.CurrentStoredFilter);
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this.FindForm(), ex.Message, ResourceUtils.GetString("ErrorWhenFilterDelete", this.PanelTitle), ex);
			}
		}
	}
	private void mnuUnsetDefaultFilter_Click(object sender, EventArgs e)
	{
		if(_userConfig == null) return;
		OrigamPanelFilter filter = _filterFactory.LoadFilter((Guid)_userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["refOrigamPanelFilterId"]);
		if(filter.PanelFilter.Rows.Count > 0)
		{
			filter.PanelFilter.Rows[0].Delete();
		}
		_userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["refOrigamPanelFilterId"] = DBNull.Value;
		// first save user config - so reference to the filter is cleared
		SaveUserConfig();
		// then save (delete) the filter
		_filterFactory.PersistFilter(filter);
	}
	private void mnuSetDefaultFilter_Click(object sender, EventArgs e)
	{
		if(_userConfig == null) return;
		OrigamPanelFilter filter;
		if(_userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["refOrigamPanelFilterId"] == DBNull.Value)
		{
			filter = new OrigamPanelFilter();
		}
		else
		{
			filter = _filterFactory.LoadFilter((Guid)_userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["refOrigamPanelFilterId"]);
		}
		if(filter.PanelFilter.Rows.Count == 0)
		{
			OrigamPanelFilter.PanelFilterRow filterRow = filter.PanelFilter.NewPanelFilterRow();
			filterRow.Id = Guid.NewGuid();
		
			_filterFactory.GetFilterFromCurrent(filterRow, "default", false, true, Guid.Empty);
			_userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["refOrigamPanelFilterId"] = filterRow.Id;
		}
		else
		{
			_filterFactory.GetFilterFromCurrent(filter.PanelFilter.Rows[0] as OrigamPanelFilter.PanelFilterRow, "default", false, true, Guid.Empty);
		}
		_filterFactory.PersistFilter(filter);
		SaveUserConfig();
	}
	private void filterMenu_Popup(object sender, EventArgs e)
	{
		filterMenu.MenuItems.Clear();
		this.filterMenu.MenuItems.AddRange(new MenuItem[] {
																				   this.mnuSetDefaultFilter,
																				   this.mnuUnsetDefaultFilter,
																				   this.menuItem2,
																				   this.mnuFilterAdd,
																				   this.mnuFilterDelete,
																				   this.mnuFilterClear,
																				   this.menuItem4});
		if(_userConfig == null)
		{
			this.mnuUnsetDefaultFilter.Enabled = false;
			this.mnuSetDefaultFilter.Enabled = false;
		}
		else
		{
			this.mnuUnsetDefaultFilter.Enabled = _userConfig.Tables["OrigamFormPanelConfig"].Rows[0]["refOrigamPanelFilterId"] != DBNull.Value;
			this.mnuSetDefaultFilter.Enabled = FilterVisible; 
		}
		this.mnuFilterAdd.Enabled = this.FilterActive;
		this.mnuFilterClear.Enabled = this.FilterActive;
		this.mnuFilterDelete.Enabled = _filterFactory.CurrentStoredFilter != null;
		try
		{
			foreach(OrigamPanelFilter.PanelFilterRow filter in _filterFactory.StoredFilters.PanelFilter.Rows)
			{
				try
				{
					FilterMenuItem storedFilterItem = new FilterMenuItem(filter.Name);
					storedFilterItem.Enabled = true; 
					storedFilterItem.Filter = filter;
					storedFilterItem.Checked = filter == _filterFactory.CurrentStoredFilter;
					storedFilterItem.Click += storedFilterItem_Click;
					filterMenu.MenuItems.Add(storedFilterItem);
				}
				catch {}
			}
		}
		catch(Exception ex)
		{
			AsMessageBox.ShowError(this.FindForm(), ex.Message, ResourceUtils.GetString("ErrorWhenFilterListe", this.PanelTitle), ex);
		}
	}
	private void storedFilterItem_Click(object sender, EventArgs e)
	{
        ValidateChildren();
		if(! FilterVisible) FilterVisible = true;
		FilterMenuItem filterMenu = sender as FilterMenuItem;
		_filterFactory.CurrentStoredFilter = filterMenu.Filter;
	}
	private void dbNav_NewRecordAdded(object sender, EventArgs e)
	{
		// we check if the user can actually add the row here, because this happens really
		// after new record is added by the user (not by databinding - which is buggy and adds records anytime)
		UpdateRowLevelSecurity();
		if(! this.ShowNewButton)
		{
			AsMessageBox.ShowError(this.FindForm(), ResourceUtils.GetString("NoAuthorization"), ResourceUtils.GetString("AuthorizationTitle", this.PanelTitle), null);
			this.BindingContext[this.DataSource, this.DataMember].CancelCurrentEdit();
		}
		FocusControls();
		FocusGridFirstColumn();
	}
	public void UpdateBindings()
	{
		UpdateErrorProvider();
		try
		{
			CurrencyManager cm = _bindingManager;
		
			if(IsBinding(cm) == false || this.IsParentManagerBinding(cm) == false
				|| ((cm.Current as DataRowView).IsNew && (this.FindForm() as AsForm).AddingDataMember != this.DataMember))
			{
				ManageChildBindingContexts(cm, true);
				BindGrid();
				OnRecordIdChanged(null);
			}
			else
			{
				BindGrid();
		
				ManageChildBindingContexts(cm, false);
			}
		}
		finally
		{
			UpdateRowLevelSecurity();
			UpdateControlsForZeroRows(this);
			dbNav.UpdateControls();
		}
	}
    private int _prevCount = 0;
	private void _bindingManager_PositionChanged(object sender, EventArgs e)
	{
        int count = (sender as CurrencyManager).List.Count;
        if (count == _prevCount && count == 0)
        {
            return;
        }
        _prevCount = count;
        System.Diagnostics.Debug.WriteLine(this.DataMember + " " + (sender as BindingManagerBase).Position, "PositionChanged");
		UpdateBindings();
	}
	
	private void AsPanel_CurrentChanged(object sender, EventArgs e)
	{
		// _isNewRow does not seem to be handled here anymore because it does not go
        // into recursion anymore. This block might be deleted.
        if(_isNewRow)
		{
            System.Diagnostics.Debug.WriteLine("Position: " + (sender as CurrencyManager).Position, "IsNewRow=False");
            _isNewRow = false;
			object[] key = _newRowKey;
			_newRowKey = null;
			// WORKAROUND BUG: new record while we are adding a new record somewhere else
			if((this.FindForm() as AsForm).AddingDataMember != this.DataMember)
			{
				return;
			}
			// if position will change, we return, because this event will be repeated on row change
			if(SetPosition(key))
			{
				return;
			}
			// otherwise we continue, new record has been cancelled
		}
		CurrencyManager cm = (sender as CurrencyManager);
		try
		{
			if(cm.Position >= 0)
			{
				DataRowView row = cm.Current as DataRowView;
				if (row.IsNew ) 
				{
					_isNewRow = true;
					System.Diagnostics.Debug.WriteLine("Position: " + cm.Position, "IsNewRow=True (begin method)");
			
					if(row.Row.IsNull(Const.ValuelistIdField))
					{
						row.Row[Const.ValuelistIdField]= Guid.NewGuid();
					}
					else
					{
						// When adding a new record through datagrid, this event is fired twice. 
						// Once it assigns the new GUID and second time it goes here. But the first
						// time it does not fires any RuleEngine events, so here we assign the
						// value again (same id), so rules are fired.
						row.Row[Const.ValuelistIdField] = row.Row[Const.ValuelistIdField];
					}
					_newRowKey = DatasetTools.PrimaryKey(row.Row);
					if(row.Row.Table.Columns.Contains("RecordCreated") && row.Row.IsNull("RecordCreated"))
					{
						row.Row["RecordCreated"]= DateTime.Now;
					}
					try
					{
						if(row.Row.Table.Columns.Contains("RecordCreatedBy") && row.Row.IsNull("RecordCreatedBy"))
						{
							UserProfile profile = SecurityManager.CurrentUserProfile();
							row.Row["RecordCreatedBy"] = profile.Id;
						}
					}
					catch
					{
					}
					System.Diagnostics.Debug.WriteLine("Position: " + cm.Position, "IsNewRow=false (end method)");
					_isNewRow = false;
				}
			}
		}
		finally
		{
			UpdateBindings();
			if(cm.Position >= 0)
			{
				DataRowView dView = (cm.Current as DataRowView);
				if(dView.Row.Table.Columns.Contains(Const.ValuelistIdField))
				{
					OnRecordIdChanged(dView.Row);
				}
			}
		}
	}
	private bool _entering = false;
	private void AsPanel_Enter(object sender, EventArgs e)
	{
		if(this.DesignMode) return;
		try
		{
			_entering = true;
			UpdateColor();
			if(btnGrid.Pushed)	FocusControls(); // only when grid is active, otherwise we have a problem with recursive OnEnter, when user clicks inside the form in a form view
		}
		finally
		{
			_entering = false;
		}
	}
	private void pnlDataControl_Click(object sender, EventArgs e)
	{
		FocusControls();
	}
	private void toolBar_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
	{
		object[] position = CurrentKey;
		this.toolBar.Focus();
		// after leaving focus, the data might have been resorted, so we set the old position
		SetPosition(position);
		try
		{
			this.Cursor = Cursors.WaitCursor;
			if( e.Button == btnAttachment )
			{
				OnShowAttachmentsChanged();
			}
			else if( e.Button == btnGrid )
			{
				this.ShowGrid = btnGrid.Pushed;
			}
			else if( e.Button == btnFilter)
			{
				this.FilterVisible = !this.FilterVisible;
			}
			else if( e.Button == btnAuditLog)
			{
				ShowAudit();
			}
		}
		catch(Exception ex)
		{
			this.Cursor = Cursors.Default;
			AsMessageBox.ShowError(this.FindForm(), ex.Message, ResourceUtils.GetString("ErrorInPanel", this.PanelTitle), ex);
		}
		finally
		{
			this.Cursor = Cursors.Default;
		}
	}
	private void toolBar_Validated(object sender, EventArgs e)
	{
	}
	private void filterFactory_DataViewQueryChanged(object sender, string query)
	{
		if(! this.IsParentManagerBinding(_bindingManager))
		{
			// if parent binding is suspended, we will not do any filtering
			return;
		}
		CurrencyManager cm = _bindingManager;
		if( cm?.List is DataView)
		{
			try
			{
				if((cm.List as DataView).RowFilter != query)
				{
					(cm.List as DataView).RowFilter = query;
					UpdateFilter();
					SetActualRecordId();
				}
			} 
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this.FindForm(), ex.Message, ex.Source, ex);
			}
		}
	}
	private void AsPanel_Load_1(object sender, EventArgs e)
	{
		if(this.DesignMode)
		{
			pnlFilter.Visible = false;
		}
        		
	}
	private void grid_VisibleChanged(object sender, EventArgs e)
	{
		Control g = sender as Control;
		g?.PerformLayout();
	}
	private bool _leaving = false;
	private void AsPanel_Leave(object sender, EventArgs e)
	{
		if(this.DesignMode) return;
		object[] key = CurrentKey;
		try
		{
			_leaving = true;
			UpdateColor();
			if(this.DataSource == null) return;
			// We have to do this, because otherwise child relations bindings would not be
			// notified that e.g. we added a new row (bug in data binding?)
			if(IsBinding())
			{
				this.EndEdit(!this.GridVisible);
				SetPosition(key);
			}
		}
		catch(Exception ex)
		{
			AsMessageBox.ShowError(this.FindForm(), ex.Message, ResourceUtils.GetString("ErrorLeavingPanel", this.PanelTitle), ex);
		}
		finally
		{
			_leaving = false;
		}
	}
	private void grid_HandleCreated(object sender, EventArgs e)
	{
		grid.HandleCreated -= grid_HandleCreated;
	}
	private void AsPanel_GotFocus(object sender, EventArgs e)
	{
		if(this.btnGrid.Pushed) FocusControls();
	}
	private void AsPanel_Paint(object sender, PaintEventArgs e)
	{
		bool shouldDrawLine = false;
		Control current = this;
		while(current != null)
		{
			if(current.Top != 0)
			{
				break;
			}
			if(current.Parent is TabPage)
			{
				shouldDrawLine = true;
				break;
			}
			current = current.Parent;
		}
		if(shouldDrawLine)
		{
			Rectangle rect = new Rectangle(0, 0, this.Width, 5);
			LinearGradientBrush b = new LinearGradientBrush(rect, OrigamColorScheme.TitleActiveStartColor, this.BackColor, LinearGradientMode.Vertical);
			e.Graphics.FillRectangle(b, rect);
		}
	}
	private void _attachmentPad_AttachmentsUpdated(object sender, EventArgs e)
	{
		UpdateAttachmentIcon();
	}
	#endregion
	#region IAsDataServiceComplexConsumer Members
	//implements standard Datamember
	[Category("Data")]
	[Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",typeof(System.Drawing.Design.UITypeEditor))]
	public string DataMember { get; set; }
	//implements standard DataSource
	private DataSet _dataSource;
	private bool _settingDataSource = false;
	[Category("Data")]
	[RefreshProperties(RefreshProperties.Repaint)]
	[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
	public object DataSource
	{
		get => _dataSource;
		set
		{
			_settingDataSource = true;
			try
			{
				DataSet newData = ConvertInputData(value);
				if(!this.DesignMode)
				{
					string oldTable = FormTools.FindTableByDataMember(_dataSource, this.DataMember);
					string newTable = FormTools.FindTableByDataMember(newData, this.DataMember);
					if(_dataSource != newData)
					{
						if(oldTable != "" && _dataSource.Tables.Contains(oldTable))
						{
							_dataSource.Tables[oldTable].ColumnChanged -= AsPanel_ColumnChanged;
						}
						if(newTable != "" && newData.Tables.Contains(newTable))
						{
							DataTable t = newData.Tables[newTable];
							// we subscribe to the event, so we can update lookup sort values, when changed
							t.ColumnChanged += AsPanel_ColumnChanged;
						}
						UnsubscribeBindingManagerEvents();
						if(newData == null)
						{
							_bindingManager = null;
						
							if(grid != null)
							{
								_gridBuilder.UpdateDataSource(grid, null, this.DataMember);
							}
						}
						_dataSource = newData;
						ProcessBindings();
						FocusControls();
					}
				}
				else
				{
					_dataSource = newData;
				}
			}
			finally
			{
				_settingDataSource = false;
			}
		}
	}
	#endregion
	#region IOrigamMetadataConsumer Members
	private Guid _panelUniqueId;
	public Guid PanelUniqueId
	{
		get
		{
			if(this.OrigamMetadata == null)
			{
				return _panelUniqueId;
			}
			else if(this.OrigamMetadata.ParentItem is PanelControlSet)
			{
				// if this is a panel directly placed on the form - e.g. selection dialog
				return this.OrigamMetadata.Id;
			}
			else
			{
				// normal form
				return FormTools.GetItemFromControlSet((this.OrigamMetadata as ControlSetItem).ControlItem.PanelControlSet).Id;
			}
		}
		set => _panelUniqueId = value;
	}
	public Guid FormPanelId
	{
		get
		{
			if(this.OrigamMetadata == null)
			{
				return _panelUniqueId;
			}
			else
			{
				// normal form
				return this.OrigamMetadata.Id;
			}
		}
	}
	#endregion
	#region ISupportInitialize Members
	public void BeginInit()
	{
	}
	public void EndInit()
	{
		// here we load the user's defaults
		LoadUserConfig();
		_originalDisplayDeleteButton = this.ShowDeleteButton;
		OriginalShowNewButton = this.ShowNewButton;
	}
	#endregion
	private void AsPanel_ColumnChanged(object sender, DataColumnChangeEventArgs e)
	{
        // When current row changes databinding tries to add a new row to all
        // children relations for unknown reason and then cancels the add.
        // We try to skip this event so we do not recalculate all the rules etc.
        if (e.Row.RowState == DataRowState.Detached && !_isNewRow) return;
        OrigamDataRow row = e.Row as OrigamDataRow;
        if (!row.IsColumnWithValidChange(e.Column)) return;
		if(_updatingSortColumn) return;
		UpdateTempSortColumn(e.Column, e.Row, LookupId(e.Column.ColumnName), false);
		_lastRowLevelSecurityRecordId = Guid.Empty;
		UpdateRowLevelSecurity();
		actionButtonManager.UpdateActionButtons();
	}
	public string GetSortColumn(string originalColumnName)
	{
		if(this._bindingManager == null) return originalColumnName;
		if(this._bindingManager.List == null) return originalColumnName;
		DataView view = this._bindingManager.List as DataView;
		if(view == null) return originalColumnName;
		return GetSortColumn(originalColumnName, view.Table);
	}
	public string GetSortColumn(string originalColumnName, DataTable table)
	{
		foreach(DataColumn tempColumn in table.Columns)
		{
			if(tempColumn.ExtendedProperties.Contains(Const.TemporaryColumnAttribute) && (string)tempColumn.ExtendedProperties[Const.TemporaryColumnAttribute] == originalColumnName && tempColumn.ExtendedProperties.Contains(Const.TemporaryColumnInitializedAttribute))
			{
				return tempColumn.ColumnName;
			}
		}
		// not found, we create one
		AsDataGrid grid = this.grid as AsDataGrid;
		
		if(grid != null)
		{
			DataGridDropdownColumn gridCol = grid.TableStyles[0].GridColumnStyles[originalColumnName] as DataGridDropdownColumn;
			// only create lookup sort columns for DropDown (lookup) grid columns
			if(gridCol != null)
			{
				try
				{
					grid.IgnoreLayoutEvent = true;
					Guid lookupId = gridCol.DropDown.LookupId;
					string lookupColumnName = DatasetTools.SortColumnName(originalColumnName);
					if(! table.Columns.Contains(lookupColumnName))
					{
						// temp sort column does not exist, we sort by the original column
						return lookupColumnName;
					}
					// and we fill the column with all the looked up data
					UpdateTempSortColumn(table.Columns[originalColumnName], null, lookupId, true);
					table.Columns[lookupColumnName].ExtendedProperties.Add(Const.TemporaryColumnInitializedAttribute, true);
					return lookupColumnName;
				}
				finally
				{
					grid.IgnoreLayoutEvent = false;
				}
			}
		}
		// the column was not lookup column, we sort by the column itself
		return originalColumnName;
	}
	private Guid LookupId(string originalColumnName)
	{
		if(this.grid is AsDataGrid grid)
		{
			if(grid.TableStyles[0].GridColumnStyles[originalColumnName] is DataGridDropdownColumn gridCol)
			{
				return gridCol.DropDown.LookupId;
			}
		}
		return Guid.Empty;
	}
	private bool _updatingSortColumn = false;
	private void UpdateTempSortColumn(DataColumn col, DataRow singleRow, Guid lookupId, bool acceptChangesOnUnchanged)
	{
		if(singleRow != null && singleRow.RowState == DataRowState.Deleted) return;
		if(lookupId.Equals(Guid.Empty)) return;
		if(_updatingSortColumn) return;
		DataTable table = col.Table;
		foreach(DataColumn tempColumn in table.Columns)
		{
			// find the temp column for this column
			if(tempColumn.ExtendedProperties.Contains(Const.TemporaryColumnAttribute) && (string)tempColumn.ExtendedProperties[Const.TemporaryColumnAttribute] == col.ColumnName)
			{
				// we found a dummy sort column and we update its value
				//Guid lookupId = (Guid)tempColumn.ExtendedProperties[Const.TemporaryColumnLookupAttribute];
				IDataLookupService lookupService = ServiceManager.Services.GetService(typeof(IDataLookupService)) as IDataLookupService;
				bool prevIgnoreDataChanges = this.Generator.IgnoreDataChanges;
				_updatingSortColumn = true;
				this.Generator.IgnoreDataChanges = true;
				try
				{
					if(singleRow != null)
					{
						// if single row is provided
						bool unchanged = (singleRow.RowState == DataRowState.Unchanged);
						singleRow[tempColumn] = lookupService.GetDisplayText(lookupId, singleRow[col], null);
						if(unchanged & acceptChangesOnUnchanged) singleRow.AcceptChanges();
					}
					else
					{
						try
						{
							this._gridBuilder.UpdateDataSource(this.grid, null, this.DataMember);
							if(this.errorProvider1 != null)
							{
								this.errorProvider1.DataSource = null;
							}
							this.Generator.Form.Cursor = Cursors.WaitCursor;
							//							table.BeginLoadData();
							// all rows will be scanned
							for(int i = 0; i < table.Rows.Count; i++)
							{
								DataRow row = table.Rows[i];
								if(row.RowState != DataRowState.Deleted 
                                    && row.RowState != DataRowState.Detached)
								{
									bool unchanged = (row.RowState == DataRowState.Unchanged);
									object newValue = lookupService.GetDisplayText(lookupId, row[col], null);
								
									if(row[tempColumn] != newValue 
										&& ! (row.IsNull(tempColumn) && row.IsNull(col)))
									{
										row[tempColumn] = newValue;
										// If the row's state was originaly Unchanged, our change set it to Modified.
										// Therefore we set it back to Unchanged by accepting the changes.
										if(unchanged & acceptChangesOnUnchanged) row.AcceptChanges();
									}
								}
							}
						}
						finally
						{
							//							table.EndLoadData();
							UpdateErrorProvider();
							BindGrid();
							// Workaround: After sorting with a lookup column the grid sometimes
							// gets read only. This is fixed by moving around using a scrollbar.
							// No better solution as of now...
                            (grid as AsDataGrid).HorizontalScrollPosition++;
                            (grid as AsDataGrid).HorizontalScrollPosition--;
                            this.Generator.Form.Cursor = Cursors.Default;
						}
					}
				}
				finally
				{
					_updatingSortColumn = false;
					this.Generator.IgnoreDataChanges = prevIgnoreDataChanges;
				}
				break;
			}
		}
	}
	public void BindActionButtons()
	{
		actionButtonManager.BindActionButtons();
	}
	private void SubscribeBindingManagerEvents()
	{
		if(_bindingManager == null) return;
		_bindingManager.CurrentChanged +=AsPanel_CurrentChanged;
		_bindingManager.PositionChanged +=_bindingManager_PositionChanged;
	}
	private void UnsubscribeBindingManagerEvents()
	{
		if(_bindingManager == null) return;
		_bindingManager.CurrentChanged -=AsPanel_CurrentChanged;
		_bindingManager.PositionChanged -=_bindingManager_PositionChanged;
	}
	#region IComparable Members
	public int CompareTo(object obj)
	{
		AsPanel panel = obj as AsPanel;
		if(panel == null) throw new ArgumentOutOfRangeException("obj", obj, ResourceUtils.GetString("ErrorCompareAsPanel"));
		
		return this.DataMember.Split(".".ToCharArray()).Length.CompareTo(panel.DataMember.Split(".".ToCharArray()).Length);
	}
	#endregion
}
