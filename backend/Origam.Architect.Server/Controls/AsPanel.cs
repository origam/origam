using System.ComponentModel;

namespace Origam.Architect.Server.Controls;

public class AsPanel: ControlBase
{
    [Category("Map View")]
    public string MapTextColorMember { get; set; }

    [Category("Map View")]
    public string MapLayers { get; set; }

    [Category("Map View")]
    public string MapAzimuthMember { get; set; }

    [Category("Calendar View")]
    public string CalendarDescriptionMember { get; set; }

    [Category("Map View")]
    public string MapTextRotationMember { get; set; }

    public int MaxDynamicGridHeight { get; set; } = 0;
    
    [Category("Map View")]
    public string MapTextLocationMember { get; set; }

    public bool HideNavigationPanel { get; set; } = false;

    [Category("Map View")]
    public string MapLocationMember { get; set; }


    [Category("Calendar View")]
    public string CalendarDateFromMember { get; set; }

    [Category("Pipeline View")]
    public bool IsPipelineSupported { get; set; }

    [Localizable(true)]
    [MergableProperty(false)]
    public int TabIndex { get; set; }

    // [Category("Calendar View")]
    // public AsPanelCalendarViewEnum DefaultCalendarView { get; set; }

    [Category("Map View")]
    public bool IsMapVisible { get; set; } = false;
    
    [Category("Behavior")]
    [Description("Indicates whether Copy Button will be hidden.")]
    public bool HideCopyButton { get; set; } = false;

    [Category("Calendar View")]
    public bool IsCalendarVisible { get; set; }

    [Category("Misc")]
    [Description("Member will be treated as ordered - it will be read only and a special UI components will be available.")]
    public string OrderMember { get; set; }

    [Category("Pipeline View")]
    public string PipelinePriceMember { get; set; }

    [Category("Calendar View")]
    public string CalendarNameMember { get; set; }

    [Category("Pipeline View")]
    public string PipelineNameMember { get; set; }

    public string DefaultConfiguration { get; set; }

    [Category("Map View")]
    public string MapIconMember { get; set; }

    public bool GridVisible { get; set; } = false;

    [Browsable(false)]
    public Guid CalendarViewStyleId { get; set; }

    [Category("Calendar View")]
    public string CalendarDateDueMember { get; set; }

    [Browsable(false)]
    public Guid CalendarRowHeightConstantId { get; set; }

    [Category("Calendar View")]
    public string CalendarResourceIdMember { get; set; }

    [Category("Data")]
    [Editor("System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", "System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
    public string DataMember { get; set; }

    [Category("(ORIGAM)")]
    public string PanelTitle { get; set; }

    [Browsable(false)]
    public Guid PipelineStateLookupId { get; set; }

    [Category("Map View")]
    public string MapCenter { get; set; }

    [Category("Map View")]
    public string MapColorMember { get; set; }

    [Category("Calendar View")]
    public bool CalendarShowAllResources { get; set; } = false;

    [Category("Calendar View")]
    public bool IsCalendarSupported { get; set; }

    [Category("Map View")]
    public int MapResolution { get; set; }
    
    [Category("Behavior")]
    [Description("Indicates whether New Button will be shown.")]
    public bool ShowNewButton { get; set; } = false;

    [Category("Visual Editor View")]
    public bool IsVisualEditorVisible { get; set; }

    [Category("Pipeline View")]
    public string PipelineDateMember { get; set; }

    [Category("Visual Editor View")]
    public bool IsVisualEditorSupported { get; set; }

    public string ImplicitFilter { get; set; }

    [Category("Pipeline View")]
    public bool IsPipelineVisible { get; set; }

    [Description("This setting is only applied on the action buttons placed on the toolbar.")]
    public bool DisableActionButtons { get; set; }

    [Browsable(false)]
    public Guid IconId { get; set; }

    [Category("Behavior")]
    [Description("Indicates whether Delete Button will be shown.")]
    public bool ShowDeleteButton { get; set; } = false;

    [Category("Drag & Drop")]
    public string DraggingLabelMember { get; set; }

    [Category("Map View")]
    public string MapTextMember { get; set; }

    [Browsable(false)]
    public Guid IndependentDataSourceMethodId { get; set; }

    [Category("Calendar View")]
    public string CalendarCustomSortMember { get; set; }

    [Category("Map View")]
    public bool IsMapSupported { get; set; } = false;
    
    public bool IsGridHeightDynamic { get; set; } = false;

    [Browsable(false)]
    public Guid IndependentDataSourceSortId { get; set; }

    public bool NewRecordInDetailView { get; set; } = false;

    [Browsable(false)]
    public Guid IndependentDataSourceId { get; set; }

    [Category("Pipeline View")]
    public string PipelineStateMember { get; set; }
    
    [Category("Drag & Drop")]
    public bool IsDraggingEnabled { get; set; } = false;

    public string SelectionMember { get; set; }

    [Category("Calendar View")]
    public string CalendarDateToMember { get; set; }

    [Category("Calendar View")]
    public string CalendarResourceNameLookupField { get; set; }

    [Category("Calendar View")]
    public string CalendarIsFinishedMember { get; set; }
}
