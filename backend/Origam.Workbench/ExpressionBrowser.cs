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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Origam.DA.ObjectPersistence;
using Origam.Extensions;
using Origam.Git;
using Origam.Schema;
using Origam.Schema.Attributes;
using Origam.UI;
using Origam.Workbench.Services;

namespace Origam.Workbench;
/// <summary>
/// Summary description for ExpressionBrowser.
/// </summary>
public class ExpressionBrowser : System.Windows.Forms.UserControl
{
	private Origam.UI.NativeTreeView tvwExpressionBrowser;
	public event FilterEventHandler QueryFilterNode;
	public event EventHandler ExpressionSelected;
	public event EventHandler NodeClick;
	public event EventHandler NodeDoubleClick;
	public event EventHandler NodeUnderMouseChanged;
	public System.Windows.Forms.ImageList imgList;
	private System.Windows.Forms.ComboBox cboFilter;
	private System.ComponentModel.IContainer components;
	private delegate void TreeViewDelegate(TreeNode nod);
	private TreeNode mNodSpecial = new TreeNode("_Special");
	private Rectangle dragBoxFromMouseDown;
	private IDocumentationService _documentationService;
	private SchemaService _schemaService;
	private System.Windows.Forms.ToolTip toolTip1;
	private bool _refreshPaused = false;
	private bool _disposed = false;
	private Font _boldFont;
    private string _sourcePath;
    private FileSystemWatcher fileWatcher;
    private Timer watcherTimer;
    private Hashtable _customImages = new Hashtable();
    private bool _fileChangesPending = false;
    protected bool _supportsGit = false;
    
	public ExpressionBrowser()
	{
		// This call is required by the Windows.Forms Form Designer.
		InitializeComponent();
		_boldFont = new Font(tvwExpressionBrowser.Font, FontStyle.Bold);
        // handle the exception because of the WinForms Designer
        try
        {
			_documentationService = ServiceManager.Services.GetService(typeof(IDocumentationService)) as IDocumentationService;
			_schemaService = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
		}
		catch {}
	}
	public void RefreshRootNodeText()
	{
		TreeNode rootNode = tvwExpressionBrowser.RootNode;
		if (rootNode == null) return;
		var rootNodeTag = (Package) rootNode.Tag;
		rootNode.Text = rootNodeTag.Name;
	}
    public TreeNode GetFirstNode()
    {
        return tvwExpressionBrowser.Nodes.Count>0? tvwExpressionBrowser.Nodes[0]:null;
    }
	public void ReloadTreeAndRestoreExpansionState()
	{
		if (_schemaService.ActiveExtension == null) return;
        tvwExpressionBrowser.StoreExpansionState();
        RemoveAllNodes();
        _schemaService.ClearProviderCaches();
        AddRootNode(_schemaService.ActiveExtension);
        tvwExpressionBrowser.RestoreExpansionState();
	}
	/// <summary> 
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
		if( disposing )
		{
			components?.Dispose();
		}
		base.Dispose( disposing );
		_disposed = true;
	}
	#region Component Designer generated code
	/// <summary> 
	/// Required method for Designer support - do not modify 
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
        this.components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExpressionBrowser));
        this.imgList = new System.Windows.Forms.ImageList(this.components);
        this.cboFilter = new System.Windows.Forms.ComboBox();
        this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
        this.fileWatcher = new System.IO.FileSystemWatcher();
        this.watcherTimer = new System.Windows.Forms.Timer(this.components);
        this.tvwExpressionBrowser = new Origam.UI.NativeTreeView();
        ((System.ComponentModel.ISupportInitialize)(this.fileWatcher)).BeginInit();
        this.SuspendLayout();
        // 
        // imgList
        // 
        this.imgList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgList.ImageStream")));
        this.imgList.TransparentColor = System.Drawing.Color.Magenta;
        this.imgList.Images.SetKeyName(0, "");
        this.imgList.Images.SetKeyName(1, "");
        this.imgList.Images.SetKeyName(2, "");
        this.imgList.Images.SetKeyName(3, "");
        this.imgList.Images.SetKeyName(4, "");
        this.imgList.Images.SetKeyName(5, "");
        this.imgList.Images.SetKeyName(6, "");
        this.imgList.Images.SetKeyName(7, "");
        this.imgList.Images.SetKeyName(8, "");
        this.imgList.Images.SetKeyName(9, "");
        this.imgList.Images.SetKeyName(10, "");
        this.imgList.Images.SetKeyName(11, "");
        this.imgList.Images.SetKeyName(12, "");
        this.imgList.Images.SetKeyName(13, "");
        this.imgList.Images.SetKeyName(14, "");
        this.imgList.Images.SetKeyName(15, "");
        this.imgList.Images.SetKeyName(16, "");
        this.imgList.Images.SetKeyName(17, "");
        this.imgList.Images.SetKeyName(18, "");
        this.imgList.Images.SetKeyName(19, "");
        this.imgList.Images.SetKeyName(20, "");
        this.imgList.Images.SetKeyName(21, "");
        this.imgList.Images.SetKeyName(22, "");
        this.imgList.Images.SetKeyName(23, "");
        this.imgList.Images.SetKeyName(24, "");
        this.imgList.Images.SetKeyName(25, "");
        this.imgList.Images.SetKeyName(26, "");
        this.imgList.Images.SetKeyName(27, "");
        this.imgList.Images.SetKeyName(28, "");
        this.imgList.Images.SetKeyName(29, "");
        this.imgList.Images.SetKeyName(30, "");
        this.imgList.Images.SetKeyName(31, "");
        this.imgList.Images.SetKeyName(32, "");
        this.imgList.Images.SetKeyName(33, "");
        this.imgList.Images.SetKeyName(34, "");
        this.imgList.Images.SetKeyName(35, "");
        this.imgList.Images.SetKeyName(36, "");
        this.imgList.Images.SetKeyName(37, "");
        this.imgList.Images.SetKeyName(38, "");
        this.imgList.Images.SetKeyName(39, "");
        this.imgList.Images.SetKeyName(40, "");
        this.imgList.Images.SetKeyName(41, "");
        this.imgList.Images.SetKeyName(42, "");
        this.imgList.Images.SetKeyName(43, "");
        this.imgList.Images.SetKeyName(44, "");
        this.imgList.Images.SetKeyName(45, "");
        this.imgList.Images.SetKeyName(46, "");
        this.imgList.Images.SetKeyName(47, "");
        this.imgList.Images.SetKeyName(48, "");
        this.imgList.Images.SetKeyName(49, "");
        this.imgList.Images.SetKeyName(50, "");
        this.imgList.Images.SetKeyName(51, "");
        this.imgList.Images.SetKeyName(52, "");
        this.imgList.Images.SetKeyName(53, "");
        this.imgList.Images.SetKeyName(54, "");
        this.imgList.Images.SetKeyName(55, "");
        this.imgList.Images.SetKeyName(56, "");
        this.imgList.Images.SetKeyName(57, "");
        this.imgList.Images.SetKeyName(58, "");
        this.imgList.Images.SetKeyName(59, "");
        this.imgList.Images.SetKeyName(60, "");
        this.imgList.Images.SetKeyName(61, "");
        this.imgList.Images.SetKeyName(62, "");
        this.imgList.Images.SetKeyName(63, "");
        this.imgList.Images.SetKeyName(64, "");
        this.imgList.Images.SetKeyName(65, "");
        this.imgList.Images.SetKeyName(66, "");
        this.imgList.Images.SetKeyName(67, "");
        this.imgList.Images.SetKeyName(68, "");
        this.imgList.Images.SetKeyName(69, "");
        this.imgList.Images.SetKeyName(70, "");
        this.imgList.Images.SetKeyName(71, "");
        this.imgList.Images.SetKeyName(72, "");
        this.imgList.Images.SetKeyName(73, "");
        this.imgList.Images.SetKeyName(74, "");
        this.imgList.Images.SetKeyName(75, "");
        this.imgList.Images.SetKeyName(76, "");
        this.imgList.Images.SetKeyName(77, "");
        this.imgList.Images.SetKeyName(78, "");
        this.imgList.Images.SetKeyName(79, "");
        this.imgList.Images.SetKeyName(80, "");
        this.imgList.Images.SetKeyName(81, "");
        this.imgList.Images.SetKeyName(82, "");
        this.imgList.Images.SetKeyName(83, "");
        this.imgList.Images.SetKeyName(84, "");
        this.imgList.Images.SetKeyName(85, "icon_01_common.png");
        this.imgList.Images.SetKeyName(86, "icon_02_deployment.png");
        this.imgList.Images.SetKeyName(87, "icon_03_features.png");
        this.imgList.Images.SetKeyName(88, "icon_03_features-2.png");
        this.imgList.Images.SetKeyName(89, "icon_04_string-library.png");
        this.imgList.Images.SetKeyName(90, "icon_05_data.png");
        this.imgList.Images.SetKeyName(91, "icon_05_data-2.png");
        this.imgList.Images.SetKeyName(92, "icon_06_constants.png");
        this.imgList.Images.SetKeyName(93, "icon_07_data-structures.png");
        this.imgList.Images.SetKeyName(94, "icon_08_database-data-types.png");
        this.imgList.Images.SetKeyName(95, "icon_09_entities.png");
        this.imgList.Images.SetKeyName(96, "icon_10_functions.png");
        this.imgList.Images.SetKeyName(97, "icon_11_lookups.png");
        this.imgList.Images.SetKeyName(98, "icon_12_tree-structures.png");
        this.imgList.Images.SetKeyName(99, "icon_13_user-interface.png");
        this.imgList.Images.SetKeyName(100, "icon_14_charts.png");
        this.imgList.Images.SetKeyName(101, "icon_15_dashboard-widgets.png");
        this.imgList.Images.SetKeyName(102, "icon_16_images.png");
        this.imgList.Images.SetKeyName(103, "icon_17_keyboard-shortcuts.png");
        this.imgList.Images.SetKeyName(104, "icon_18_menu.png");
        this.imgList.Images.SetKeyName(105, "icon_19_notification-boxes.png");
        this.imgList.Images.SetKeyName(106, "icon_20_reports.png");
        this.imgList.Images.SetKeyName(107, "icon_21_screen-sections.png");
        this.imgList.Images.SetKeyName(108, "icon_21_screen-sections-2.png");
        this.imgList.Images.SetKeyName(109, "icon_22_screens.png");
        this.imgList.Images.SetKeyName(110, "icon_23_search-data-sources.png");
        this.imgList.Images.SetKeyName(111, "icon_24_styles.png");
        this.imgList.Images.SetKeyName(112, "icon_25_widgets.png");
        this.imgList.Images.SetKeyName(113, "icon_26_business-logic.png");
        this.imgList.Images.SetKeyName(114, "icon_26_business-logic-2.png");
        this.imgList.Images.SetKeyName(115, "icon_27_rules.png");
        this.imgList.Images.SetKeyName(116, "icon_28_schedule-times.png");
        this.imgList.Images.SetKeyName(117, "icon_28_schedule-times-2.png");
        this.imgList.Images.SetKeyName(118, "icon_29_schedules.png");
        this.imgList.Images.SetKeyName(119, "icon_30_sequential-workflows.png");
        this.imgList.Images.SetKeyName(120, "icon_31_services.png");
        this.imgList.Images.SetKeyName(121, "icon_31_services-2.png");
        this.imgList.Images.SetKeyName(122, "icon_32_state-workflows.png");
        this.imgList.Images.SetKeyName(123, "icon_33_transformations.png");
        this.imgList.Images.SetKeyName(124, "icon_34_work-queue-classes.png");
        this.imgList.Images.SetKeyName(125, "icon_35_API.png");
        this.imgList.Images.SetKeyName(126, "icon_36_web-api-pages.png");
        this.imgList.Images.SetKeyName(127, "icon_36_web-api-pages-2.png");
        this.imgList.Images.SetKeyName(128, "09_packages-1.ico");
        this.imgList.Images.SetKeyName(129, "37_folder-1.png");
        this.imgList.Images.SetKeyName(130, "37_folder-2.png");
        this.imgList.Images.SetKeyName(131, "37_folder-3.png");
        this.imgList.Images.SetKeyName(132, "37_folder-4.png");
        this.imgList.Images.SetKeyName(133, "37_folder-5.png");
        this.imgList.Images.SetKeyName(134, "38_folder-categories-1.png");
        this.imgList.Images.SetKeyName(135, "38_folder-categories-2.png");
        this.imgList.Images.SetKeyName(136, "38_folder-categories-3.png");
        this.imgList.Images.SetKeyName(137, "38_folder-categories-4.png");
        this.imgList.Images.SetKeyName(138, "38_folder-categories-5.png");
        this.imgList.Images.SetKeyName(139, "Home.png");
        this.imgList.Images.SetKeyName(140, "menu_folder.png");
        this.imgList.Images.SetKeyName(141, "menu_form.png");
        this.imgList.Images.SetKeyName(142, "menu_parameter.png");
        this.imgList.Images.SetKeyName(143, "menu_report.png");
        this.imgList.Images.SetKeyName(144, "menu_workflow.png");
        this.imgList.Images.SetKeyName(145, "icon_agregated-field.png");
        this.imgList.Images.SetKeyName(146, "icon_client-script-invocation.png");
        this.imgList.Images.SetKeyName(147, "icon_conditional-formatting-rule.png");
        this.imgList.Images.SetKeyName(148, "icon_database-field.png");
        this.imgList.Images.SetKeyName(149, "icon_data-constant.png");
        this.imgList.Images.SetKeyName(150, "icon_data-constant-reference.png");
        this.imgList.Images.SetKeyName(151, "icon_data-service-tooltip.png");
        this.imgList.Images.SetKeyName(152, "icon_data-structure.png");
        this.imgList.Images.SetKeyName(153, "icon_default.png");
        this.imgList.Images.SetKeyName(154, "icon_default-set.png");
        this.imgList.Images.SetKeyName(155, "icon_dependency.png");
        this.imgList.Images.SetKeyName(156, "icon_dropdown-action.png");
        this.imgList.Images.SetKeyName(157, "icon_entity.png");
        this.imgList.Images.SetKeyName(158, "icon_field.png");
        this.imgList.Images.SetKeyName(159, "icon_field-reference.png");
        this.imgList.Images.SetKeyName(160, "icon_filter.png");
        this.imgList.Images.SetKeyName(161, "icon_filter-reference.png");
        this.imgList.Images.SetKeyName(162, "icon_filter-set.png");
        this.imgList.Images.SetKeyName(163, "icon_function-call.png");
        this.imgList.Images.SetKeyName(164, "icon_index.png");
        this.imgList.Images.SetKeyName(165, "icon_index-field.png");
        this.imgList.Images.SetKeyName(166, "icon_key.png");
        this.imgList.Images.SetKeyName(167, "icon_lookup-field.png");
        this.imgList.Images.SetKeyName(168, "icon_lookup-reference.png");
        this.imgList.Images.SetKeyName(169, "icon_menu-action.png");
        this.imgList.Images.SetKeyName(170, "icon_menu-binding.png");
        this.imgList.Images.SetKeyName(171, "icon_parameter.png");
        this.imgList.Images.SetKeyName(172, "icon_parameter-mapping.png");
        this.imgList.Images.SetKeyName(173, "icon_parameter-reference.png");
        this.imgList.Images.SetKeyName(174, "icon_relationship.png");
        this.imgList.Images.SetKeyName(175, "icon_report-action.png");
        this.imgList.Images.SetKeyName(176, "icon_row-level-security-filter.png");
        this.imgList.Images.SetKeyName(177, "icon_row-level-security-rule.png");
        this.imgList.Images.SetKeyName(178, "icon_rule-set.png");
        this.imgList.Images.SetKeyName(179, "icon_rule-set-reference.png");
        this.imgList.Images.SetKeyName(180, "icon_sequential-workflow-action.png");
        this.imgList.Images.SetKeyName(181, "icon_sort-field.png");
        this.imgList.Images.SetKeyName(182, "icon_sort-set.png");
        this.imgList.Images.SetKeyName(183, "icon_template-set.png");
        this.imgList.Images.SetKeyName(184, "icon_transformation-template.png");
        this.imgList.Images.SetKeyName(185, "icon_tree-node.png");
        this.imgList.Images.SetKeyName(186, "icon_tree-structures.png");
        this.imgList.Images.SetKeyName(187, "icon_virtual-field.png");
        this.imgList.Images.SetKeyName(188, "icon_workflow-method.png");
        this.imgList.Images.SetKeyName(189, "icon_xsd-data-structure.png");
        this.imgList.Images.SetKeyName(190, "icon_deployment-version.png");
        this.imgList.Images.SetKeyName(191, "icon_feature.png");
        this.imgList.Images.SetKeyName(192, "icon_file-restore-update-activity.png");
        this.imgList.Images.SetKeyName(193, "icon_service-command-update-activity.png");
        this.imgList.Images.SetKeyName(194, "icon_string.png");
        this.imgList.Images.SetKeyName(195, "icon_database-entity.png");
        this.imgList.Images.SetKeyName(196, "icon_lookup.png");
        this.imgList.Images.SetKeyName(197, "icon_rule.png");
        this.imgList.Images.SetKeyName(198, "icon_rule-dependency.png");
        this.imgList.Images.SetKeyName(199, "icon_virtual-entity.png");
        this.imgList.Images.SetKeyName(200, "icon_deployment-version-active.png");
        this.imgList.Images.SetKeyName(201, "icon_cartesian-chart.png");
        this.imgList.Images.SetKeyName(202, "icon_column-series.png");
        this.imgList.Images.SetKeyName(203, "icon_currency-widget.png");
        this.imgList.Images.SetKeyName(204, "icon_date-widget.png");
        this.imgList.Images.SetKeyName(205, "icon_horizontal-axis.png");
        this.imgList.Images.SetKeyName(206, "icon_horizontal-container.png");
        this.imgList.Images.SetKeyName(207, "icon_chart-widget.png");
        this.imgList.Images.SetKeyName(208, "icon_checkbox-widget.png");
        this.imgList.Images.SetKeyName(209, "icon_line-series.png");
        this.imgList.Images.SetKeyName(210, "icon_line-series-2.png");
        this.imgList.Images.SetKeyName(211, "icon_lookup-widget.png");
        this.imgList.Images.SetKeyName(212, "icon_panel-widget.png");
        this.imgList.Images.SetKeyName(213, "icon_parameter-ui.png");
        this.imgList.Images.SetKeyName(214, "icon_pie-chart.png");
        this.imgList.Images.SetKeyName(215, "icon_pie-series.png");
        this.imgList.Images.SetKeyName(216, "icon_screen-mapping.png");
        this.imgList.Images.SetKeyName(217, "icon_svg-chart.png");
        this.imgList.Images.SetKeyName(218, "icon_text-widget.png");
        this.imgList.Images.SetKeyName(219, "icon_vertical-axis.png");
        this.imgList.Images.SetKeyName(220, "icon_vertical-container.png");
        this.imgList.Images.SetKeyName(221, "icon_alternative.png");
        this.imgList.Images.SetKeyName(222, "icon_crystal-report.png");
        this.imgList.Images.SetKeyName(223, "icon_dashboard.png");
        this.imgList.Images.SetKeyName(224, "icon_data-service-tooltip-ui.png");
        this.imgList.Images.SetKeyName(225, "icon_default-value-parameter.png");
        this.imgList.Images.SetKeyName(226, "icon_dynamic-menu.png");
        this.imgList.Images.SetKeyName(227, "icon_excel-report.png");
        this.imgList.Images.SetKeyName(228, "icon_file-system-report.png");
        this.imgList.Images.SetKeyName(229, "icon_image.png");
        this.imgList.Images.SetKeyName(230, "icon_notification-box.png");
        this.imgList.Images.SetKeyName(231, "icon_printit-report.png");
        this.imgList.Images.SetKeyName(232, "icon_property.png");
        this.imgList.Images.SetKeyName(233, "icon_report-reference.png");
        this.imgList.Images.SetKeyName(234, "icon_screen.png");
        this.imgList.Images.SetKeyName(235, "icon_screen-reference.png");
        this.imgList.Images.SetKeyName(236, "icon_screen-section.png");
        this.imgList.Images.SetKeyName(237, "icon_sequential-workflow-reference.png");
        this.imgList.Images.SetKeyName(238, "icon_shortcut.png");
        this.imgList.Images.SetKeyName(239, "icon_sql-server-report.png");
        this.imgList.Images.SetKeyName(240, "icon_style.png");
        this.imgList.Images.SetKeyName(241, "icon_style-property.png");
        this.imgList.Images.SetKeyName(242, "icon_submenu.png");
        this.imgList.Images.SetKeyName(243, "icon_system-function-call-ui.png");
        this.imgList.Images.SetKeyName(244, "icon_ui-data-constant.png");
        this.imgList.Images.SetKeyName(245, "icon_web-report.png");
        this.imgList.Images.SetKeyName(246, "icon_widget.png");
        this.imgList.Images.SetKeyName(247, "icon_xslt-initial-value-parameter.png");
        this.imgList.Images.SetKeyName(248, "block-for-each.png");
        this.imgList.Images.SetKeyName(249, "block-loop-1.png");
        this.imgList.Images.SetKeyName(250, "block-loop-2.png");
        this.imgList.Images.SetKeyName(251, "block-transaction.png");
        this.imgList.Images.SetKeyName(252, "block-transition.png");
        this.imgList.Images.SetKeyName(253, "complex-data-rule.png");
        this.imgList.Images.SetKeyName(254, "condition-rule.png");
        this.imgList.Images.SetKeyName(255, "context-mapping.png");
        this.imgList.Images.SetKeyName(256, "context-store.png");
        this.imgList.Images.SetKeyName(257, "context-store-reference.png");
        this.imgList.Images.SetKeyName(258, "data-constant-reference.png");
        this.imgList.Images.SetKeyName(259, "data-structure-reference.png");
        this.imgList.Images.SetKeyName(260, "dependency-blm.png");
        this.imgList.Images.SetKeyName(261, "entity-rule.png");
        this.imgList.Images.SetKeyName(262, "event.png");
        this.imgList.Images.SetKeyName(263, "event-2.png");
        this.imgList.Images.SetKeyName(264, "field-dependency.png");
        this.imgList.Images.SetKeyName(265, "input-mapping.png");
        this.imgList.Images.SetKeyName(266, "loader.png");
        this.imgList.Images.SetKeyName(267, "method-1.png");
        this.imgList.Images.SetKeyName(268, "parameter-blm.png");
        this.imgList.Images.SetKeyName(269, "parameter-mapping-blm.png");
        this.imgList.Images.SetKeyName(270, "report-reference.png");
        this.imgList.Images.SetKeyName(271, "sequential-workflow.png");
        this.imgList.Images.SetKeyName(272, "service.png");
        this.imgList.Images.SetKeyName(273, "schedule-group.png");
        this.imgList.Images.SetKeyName(274, "schedule-group-2.png");
        this.imgList.Images.SetKeyName(275, "simple-data-rule.png");
        this.imgList.Images.SetKeyName(276, "simple-schedule-1.png");
        this.imgList.Images.SetKeyName(277, "simple-schedule-2.png");
        this.imgList.Images.SetKeyName(278, "state.png");
        this.imgList.Images.SetKeyName(279, "state-workflow.png");
        this.imgList.Images.SetKeyName(280, "state-workflows.png");
        this.imgList.Images.SetKeyName(281, "system-function-call.png");
        this.imgList.Images.SetKeyName(282, "task-check-rule.png");
        this.imgList.Images.SetKeyName(283, "task-check-rule-4.png");
        this.imgList.Images.SetKeyName(284, "task-service-method-call.png");
        this.imgList.Images.SetKeyName(285, "task-set-workflow-property.png");
        this.imgList.Images.SetKeyName(286, "task-update-context-by-xpath.png");
        this.imgList.Images.SetKeyName(287, "task-user-interface.png");
        this.imgList.Images.SetKeyName(288, "task-wait-1.png");
        this.imgList.Images.SetKeyName(289, "task-wait-2.png");
        this.imgList.Images.SetKeyName(290, "task-workflow-call.png");
        this.imgList.Images.SetKeyName(291, "transformations.png");
        this.imgList.Images.SetKeyName(292, "transition.png");
        this.imgList.Images.SetKeyName(293, "validation-rule.png");
        this.imgList.Images.SetKeyName(294, "validation-rule-lookup-xpath.png");
        this.imgList.Images.SetKeyName(295, "workflow-command.png");
        this.imgList.Images.SetKeyName(296, "workflow-schedule.png");
        this.imgList.Images.SetKeyName(297, "work-queue-class.png");
        this.imgList.Images.SetKeyName(298, "xsl-transformation.png");
        this.imgList.Images.SetKeyName(299, "data-page.png");
        this.imgList.Images.SetKeyName(300, "file-download-page.png");
        this.imgList.Images.SetKeyName(301, "file-mapping.png");
        this.imgList.Images.SetKeyName(302, "redirect.png");
        this.imgList.Images.SetKeyName(303, "report-page.png");
        this.imgList.Images.SetKeyName(304, "workflow-page.png");
        this.imgList.Images.SetKeyName(305, "state.png");
        this.imgList.Images.SetKeyName(306, "state-final.png");
        this.imgList.Images.SetKeyName(307, "state-group.png");
        this.imgList.Images.SetKeyName(308, "state-initial.png");
        this.imgList.Images.SetKeyName(309, "state-running.png");
        this.imgList.Images.SetKeyName(310, "state-workflow-2.png");
        this.imgList.Images.SetKeyName(311, "state-workflows-2.png");
        this.imgList.Images.SetKeyName(312, "transition-2.png");
        this.imgList.Images.SetKeyName(313, "event-4.png");
        this.imgList.Images.SetKeyName(314, "loader-3.png");
        this.imgList.Images.SetKeyName(315, "hashtag_category.png");
        this.imgList.Images.SetKeyName(316, "hashtag_category_group.png");
        // 
        // cboFilter
        // 
        this.cboFilter.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.cboFilter.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.cboFilter.Location = new System.Drawing.Point(0, 0);
        this.cboFilter.Name = "cboFilter";
        this.cboFilter.Size = new System.Drawing.Size(176, 21);
        this.cboFilter.TabIndex = 1;
        this.cboFilter.Visible = false;
        this.cboFilter.SelectedIndexChanged += new System.EventHandler(this.cboFilter_SelectedIndexChanged);
        // 
        // toolTip1
        // 
        this.toolTip1.AutoPopDelay = 20000;
        this.toolTip1.InitialDelay = 500;
        this.toolTip1.ReshowDelay = 100;
        // 
        // fileWatcher
        // 
        this.fileWatcher.EnableRaisingEvents = true;
        this.fileWatcher.IncludeSubdirectories = true;
        this.fileWatcher.SynchronizingObject = this;
        this.fileWatcher.Changed += new System.IO.FileSystemEventHandler(this.fileWatcher_Changed);
        // 
        // watcherTimer
        // 
        this.watcherTimer.Enabled = true;
        this.watcherTimer.Interval = 1000;
        this.watcherTimer.Tick += new System.EventHandler(this.watcherTimer_Tick);
        // 
        // tvwExpressionBrowser
        // 
        this.tvwExpressionBrowser.AllowDrop = true;
        this.tvwExpressionBrowser.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
        | System.Windows.Forms.AnchorStyles.Left) 
        | System.Windows.Forms.AnchorStyles.Right)));
        this.tvwExpressionBrowser.BorderStyle = System.Windows.Forms.BorderStyle.None;
        this.tvwExpressionBrowser.FullRowSelect = true;
        this.tvwExpressionBrowser.HideSelection = false;
        this.tvwExpressionBrowser.ImageIndex = 0;
        this.tvwExpressionBrowser.ImageList = this.imgList;
        this.tvwExpressionBrowser.Indent = 16;
        this.tvwExpressionBrowser.ItemHeight = 20;
        this.tvwExpressionBrowser.Location = new System.Drawing.Point(0, 0);
        this.tvwExpressionBrowser.Name = "tvwExpressionBrowser";
        this.tvwExpressionBrowser.SelectedImageIndex = 0;
        this.tvwExpressionBrowser.ShowLines = false;
        this.tvwExpressionBrowser.Size = new System.Drawing.Size(174, 144);
        this.tvwExpressionBrowser.TabIndex = 0;
        this.tvwExpressionBrowser.BeforeLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tvwExpressionBrowser_BeforeLabelEdit);
        this.tvwExpressionBrowser.AfterLabelEdit += new System.Windows.Forms.NodeLabelEditEventHandler(this.tvwExpressionBrowser_AfterLabelEdit);
        this.tvwExpressionBrowser.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.tvwExpressionBrowser_AfterCollapse);
        this.tvwExpressionBrowser.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.tvwExpressionBrowser_BeforeExpand);
        this.tvwExpressionBrowser.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvwExpressionBrowser_AfterSelect);
        this.tvwExpressionBrowser.Click += new System.EventHandler(this.tvwExpressionBrowser_Click);
        this.tvwExpressionBrowser.DragDrop += new System.Windows.Forms.DragEventHandler(this.tvwExpressionBrowser_DragDrop);
        this.tvwExpressionBrowser.DragOver += new System.Windows.Forms.DragEventHandler(this.tvwExpressionBrowser_DragOver);
        this.tvwExpressionBrowser.DoubleClick += new System.EventHandler(this.tvwExpressionBrowser_DoubleClick);
        this.tvwExpressionBrowser.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tvwExpressionBrowser_KeyPress);
        this.tvwExpressionBrowser.MouseDown += new System.Windows.Forms.MouseEventHandler(this.tvwExpressionBrowser_MouseDown);
        this.tvwExpressionBrowser.MouseHover += new System.EventHandler(this.tvwExpressionBrowser_MouseHover);
        this.tvwExpressionBrowser.MouseMove += new System.Windows.Forms.MouseEventHandler(this.tvwExpressionBrowser_MouseMove);
        // 
        // ExpressionBrowser
        // 
        this.Controls.Add(this.cboFilter);
        this.Controls.Add(this.tvwExpressionBrowser);
        this.Name = "ExpressionBrowser";
        this.Size = new System.Drawing.Size(176, 144);
        this.BackColorChanged += new System.EventHandler(this.ExpressionBrowser_BackColorChanged);
        ((System.ComponentModel.ISupportInitialize)(this.fileWatcher)).EndInit();
        this.ResumeLayout(false);
	}
    internal void ExpandAllChildNodes(IBrowserNode browserNode)
    {
        LookUpNode(null, browserNode)?.ExpandAll(); 
    }
    #endregion
    protected virtual void OnExpressionSelected(System.EventArgs e) 
	{
		//Invokes the delegates.
		ExpressionSelected?.Invoke(this, e);
	}
	protected virtual void OnNodeClick(System.EventArgs e) 
	{
		//Invokes the delegates.
		NodeClick?.Invoke(this, e);
	}
	private bool _inNodeDoubleClick = false;
	protected virtual void OnNodeDoubleClick(System.EventArgs e) 
	{
		if(_inNodeDoubleClick) return;
		if (NodeDoubleClick != null) 
		{
			_inNodeDoubleClick = true;
			try
			{
				//Invokes the delegates.
				NodeDoubleClick(this, e); 
			}
			catch(Exception ex)
			{
				AsMessageBox.ShowError(this.FindForm(), ex.Message, ResourceUtils.GetString("ErrorArchitectCommand"), ex);
			}
			finally
			{
				_inNodeDoubleClick = false;
			}
		}
	}
	protected virtual void OnNodeUnderMouseChanged(System.EventArgs e) 
	{
		if (NodeUnderMouseChanged != null) 
		{
			//Invokes the delegates.
			NodeUnderMouseChanged(this, e); 
		}
	}
	private void tvwExpressionBrowser_DoubleClick(object sender, System.EventArgs e)
	{
		if (tvwExpressionBrowser.SelectedNode !=null)
			if (tvwExpressionBrowser.SelectedNode.Tag !=null)
				if(tvwExpressionBrowser.SelectedNode.Tag is IBrowserNode)
				{
					OnNodeDoubleClick(new EventArgs());
				}
	}

	public IBrowserNode2 ActiveNode
	{
		get
		{
			IBrowserNode2 node = null;
			if ((tvwExpressionBrowser.SelectedNode != null) && (tvwExpressionBrowser.SelectedNode.Tag !=null))
				if(tvwExpressionBrowser.SelectedNode.Tag is IBrowserNode2)
				{
					node = tvwExpressionBrowser.SelectedNode.Tag as IBrowserNode2;
				}
			return node;
		}
	}
	bool mbShowFilter = false;
	public bool ShowFilter
	{
		get => mbShowFilter;
		set
		{
			mbShowFilter = value;
			
			cboFilter.Visible = mbShowFilter;
			
			if(cboFilter.Visible)
			{
				tvwExpressionBrowser.Top = cboFilter.Height;
				tvwExpressionBrowser.Height = this.Height - cboFilter.Height;
			}
			else
			{
				tvwExpressionBrowser.Top = 0;
				tvwExpressionBrowser.Height = this.Height;
			}
		}
	}
	public bool AllowEdit
	{
		get => tvwExpressionBrowser.LabelEdit;
		set => tvwExpressionBrowser.LabelEdit = value;
	}
	public bool DisableOtherExtensionNodes { get; set; } = true;
	public bool CheckSecurity { get; set; } = false;
	public TreeNode NodeUnderMouse { get; set; } = null;
	private bool _loadingTree = false;
	private void LoadTree(TreeNode parentNode)
	{
		try
		{
			_loadingTree = true;
			LoadTreeRecursive(parentNode);
		}
		catch(Exception ex)
		{
			AsMessageBox.ShowError(this.FindForm(), ResourceUtils.GetString("ErrorWhenReadChildNodes", parentNode.FullPath, Environment.NewLine + Environment.NewLine + ex.Message),
				ResourceUtils.GetString("ErrorTitle"), ex);
		}
		finally
		{
			_loadingTree = false;
		}
	}
	private bool Filter(IBrowserNode node)
	{
		if(QueryFilterNode != null)
		{
			ExpressionBrowserEventArgs e = new ExpressionBrowserEventArgs(node);
			QueryFilterNode(this, e);
			
			return e.Filter;
		}
		return false;
	}
	private void LoadTreeRecursive(TreeNode parentNode)
	{
		try
		{
			tvwExpressionBrowser.BeginUpdate();
			if(this.IsDisposed) return;
			// remove any child nodes, we will refresh them anyway
			parentNode.Nodes.Clear();
			IBrowserNode bnode = parentNode.Tag as IBrowserNode;
			if(bnode != null && HasChildNodes(bnode))
			{
				ArrayList childNodes = new ArrayList(bnode.ChildNodes());
				Sort(childNodes);
				foreach(IBrowserNode2 child in childNodes)
				{
					bool filtered = Filter(child);
					
					if(! filtered)
					{
						bool isAuthorized = true;
						if(this.CheckSecurity)
						{
							// check if user has access to this item, if not, we don't display it
							if(child is IAuthorizationContextContainer)
							{
								IOrigamAuthorizationProvider authorizationProvider = SecurityManager.GetAuthorizationProvider();
								if(! authorizationProvider.Authorize(SecurityManager.CurrentPrincipal, (child as IAuthorizationContextContainer).AuthorizationContext))
								{
									isAuthorized = false;
								}
							}
						}
						if(isAuthorized & !child.Hide)
						{
							TreeNode tnode = RenderBrowserNode(child);
							//add dummy child node, because our node has some children
							if(HasChildNodes(child))
							{
								tnode.Nodes.Add(new DummyNode());
							}
							parentNode.Nodes.Add(tnode);
                            RecolorNode(tnode);
                        }
                    }
				}
			}
		}
		finally
		{
			tvwExpressionBrowser.EndUpdate();
		}
	}
	private void Sort(ArrayList childNodes)
	{
		if (childNodes.Count > 0)
		{
			object[] attributes = childNodes[0].GetType().GetCustomAttributes(typeof(ExpressionBrowserTreeSortAtribute), true);
			if (attributes != null && attributes.Length > 0)
			{
				ExpressionBrowserTreeSortAtribute treeSortAtribute = attributes[0] as ExpressionBrowserTreeSortAtribute;
				childNodes.Sort(treeSortAtribute.GetComparator());
			}
			else
			{
				childNodes.Sort();
			}
		}
	}
	private void cboFilter_SelectedIndexChanged(object sender, System.EventArgs e)
	{
	}
	private void tvwExpressionBrowser_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
	{
		TreeView tree = sender as TreeView;
		if(tree.GetNodeAt(e.X, e.Y) != null)
			tree.SelectedNode = tree.GetNodeAt(e.X, e.Y);
		// Starts a drag-and-drop operation with that item.
		if(e.Button == MouseButtons.Left)
		{
			Size dragSize = SystemInformation.DragSize;
			// Create a rectangle using the DragSize, with the mouse position being
			// at the center of the rectangle.
			dragBoxFromMouseDown = new Rectangle(new Point(e.X - (dragSize.Width /2),
				e.Y - (dragSize.Height /2)), dragSize);
		}
		else
			// Reset the rectangle if the mouse is not over an item in the ListBox.
			dragBoxFromMouseDown = Rectangle.Empty;
		
	}
	private void tvwExpressionBrowser_BeforeExpand(object sender, System.Windows.Forms.TreeViewCancelEventArgs e)
	{
		LoadTree(e.Node);
	}
	private void tvwExpressionBrowser_AfterCollapse(object sender, System.Windows.Forms.TreeViewEventArgs e)
	{
		if(e.Node != mNodSpecial)
		{
			e.Node.Nodes.Clear();
			e.Node.Nodes.Add(new DummyNode());
		}
	}
	private void tvwExpressionBrowser_AfterLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e)
	{
		if (e.Label == null) return;
		if (!(e.Node.Tag is IBrowserNode bn))
		{
			e.CancelEdit = true;
			return;
		}
		if (bn is SchemaItemGroup group &&
		    !group.CanRenameTo(e.Label))
		{
            e.CancelEdit = true;
            return;
		}
		_refreshPaused = true;
	    IPersistenceProvider persistenceProvider =
	        ServiceManager.Services.GetService<IPersistenceService>().SchemaProvider;
	    persistenceProvider.BeginTransaction();
        bn.NodeText = e.Label;
	    persistenceProvider.EndTransaction();
        _refreshPaused = false;
        RefreshNode(e.Node);
	}
	private void tvwExpressionBrowser_BeforeLabelEdit(object sender, System.Windows.Forms.NodeLabelEditEventArgs e)
	{
		IBrowserNode bnode = (IBrowserNode)e.Node.Tag;
		if(! (bnode != null && bnode.CanRename && _schemaService.IsItemFromExtension(bnode) & this.DisableOtherExtensionNodes))
		{
			e.CancelEdit = true;
		}
	}
	private TreeNode RenderBrowserNode(IBrowserNode2 bnode)
	{
		int imageIndex = ImageIndex(bnode);
		TreeNode tnode = new TreeNode(bnode.NodeText, imageIndex, imageIndex);
		tnode.Tag = bnode;
        tnode.NodeFont = GetFont(bnode);
		return tnode;
	}
    private Font GetFont(IBrowserNode2 bnode)
    {
        if (bnode.FontStyle.ToFont() == FontStyle.Bold)
        {
            return _boldFont;
        }
        else
        {
            return new Font(tvwExpressionBrowser.Font, bnode.FontStyle.ToFont());
        }
    }
	private void RecolorNode(TreeNode node)
	{
#if ! ORIGAM_CLIENT
        var item = node.Tag as IPersistent;
		node.BackColor = tvwExpressionBrowser.BackColor;
		node.ForeColor = Color.Black;
        if (item != null)
		{
			TreeNode parentNode = node.Parent;
			var parentItemFiles = (parentNode?.Tag as IPersistent)
				?.Files
				?.ToArray() ?? new string[0];
            if (this.DisableOtherExtensionNodes & !_schemaService.IsItemFromExtension(item))
            {
                node.ForeColor = Color.Gray;
            }
            else if (parentItemFiles.Length > 0
                     && parentItemFiles.First() == item.Files.FirstOrDefault()
                     && parentNode.ForeColor != OrigamColorScheme.TabActiveForeColor)
            {
                // same file as parent
                node.ForeColor = parentNode.ForeColor;
            }
            else if (_supportsGit && IsFileDirty(item))
            {
                node.ForeColor = OrigamColorScheme.DirtyColor;
            }
			Pads.FindSchemaItemResultsPad resultsPad = 
				WorkbenchSingleton.Workbench.GetPad(typeof(Pads.FindSchemaItemResultsPad)) as Pads.FindSchemaItemResultsPad;
            if (resultsPad != null)
            {
                foreach (ISchemaItem result in resultsPad.Results)
                {
                    IBrowserNode bnode = item as IBrowserNode;
                    if (result.PrimaryKey.Equals(item.PrimaryKey) ||
                        (bnode.GetType() == result.GetType()
                        && bnode != null
                        && !bnode.HasChildNodes
                        && result.RootItem.PrimaryKey.Equals(item.PrimaryKey)))
                    {
                        node.BackColor = OrigamColorScheme.TabActiveStartColor;
                        node.ForeColor = OrigamColorScheme.TabActiveForeColor;
                        break;
                    }
                }
            }
		}
		if(node.Tag is SchemaItemProviderGroup)
		{
			node.BackColor = Color.FromArgb(200, 200, 200);
			node.ForeColor = Color.Black;
			node.NodeFont = _boldFont;
		}
		if(node.Tag is AbstractSchemaItemProvider)
		{
			node.BackColor = OrigamColorScheme.FormBackgroundColor;
			node.NodeFont = _boldFont;
		}
#endif
	}
    private bool IsFileDirty(IPersistent item)
    {
        if (item.Files.Count == 0)
        {
            return false;
        }
        string ParentFile = item.Files.First();
        if(GitManager.GetCache().TryGetValue(ParentFile, out bool status))
        {
            return status;
        }
        if (GitManager.IsValid(_sourcePath))
        {
            GitManager gitManager = new GitManager(_sourcePath);
            foreach (string file in item.Files)
            {
                string path = Path.Combine(_sourcePath, file);
                if (File.Exists(path))
                {
                    if (gitManager.HasChanges(path))
                    {
                        GitManager.GetCache()[ParentFile] = true;
                        return true;
                    }
                }
            }
        }
        GitManager.GetCache()[ParentFile] =  false;
        return false;
    }
    private void RecolorNodesByFile(TreeNode node, string file)
    {
        if (node.Tag is IPersistent persistent
            && persistent.Files.First() == file)
        {
            RecolorNode(node);
        }
        foreach (TreeNode child in node.Nodes)
        {
            RecolorNodesByFile(child, file);
        }
    }
    private int ImageIndex(IBrowserNode bnode)
	{
		int imageIndex = -1;
		if(bnode is IBrowserNode2 && (bnode as IBrowserNode2).NodeImage != null)
		{
			Image nodeImage = (bnode as IBrowserNode2).NodeImage.ToBitmap();
			if(_customImages.Contains(bnode))
			{
				imgList.Images[(int)_customImages[bnode]] = nodeImage;
				imageIndex = (int)_customImages[bnode];
			}
			if(imageIndex == -1)
			{
				imageIndex = imgList.Images.Add(nodeImage, Color.Magenta);
				_customImages.Add(bnode, imageIndex);
			}
		}
		else
		{
            if (! int.TryParse(bnode.Icon, out imageIndex))
            {
                imageIndex = imgList.Images.IndexOfKey(bnode.Icon);
            }
		}
		return imageIndex;
	}
	public void AddRootNode(IBrowserNode2 node)
	{
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        _sourcePath = settings.ModelSourceControlLocation;
        string gitPath = GitManager.GetRepositoryPath(settings.ModelSourceControlLocation);
        if (gitPath != null)
        {
            fileWatcher.Path = gitPath;
            fileWatcher.EnableRaisingEvents = true;
            _supportsGit = true;
        }
        TreeNode tnode = RenderBrowserNode(node);
        RecolorNode(tnode);
        try
        {
			if(HasChildNodes(node))
			{
				tnode.Nodes.Add(new DummyNode());
			}
			this.tvwExpressionBrowser.Nodes.Add(tnode);
		}
		catch(Exception ex)
		{
			AsMessageBox.ShowError(this.FindForm(), ResourceUtils.GetString("ErrorWhenAddRoot", node.NodeText, Environment.NewLine + Environment.NewLine + ex.Message), 
				ResourceUtils.GetString("ErrorTitle"), ex);
		}
        tnode.Expand();
	}
	public void RemoveAllNodes()
	{
		if(! _disposed)
		{
			tvwExpressionBrowser.Nodes.Clear();
            fileWatcher.EnableRaisingEvents = false;
		}
	}
	public void RemoveBrowserNode(IBrowserNode2 browserNode)
	{
		TreeNode foundNode = null;
		foreach(TreeNode node in tvwExpressionBrowser.Nodes)
		{
			foundNode = LookUpNode(node, browserNode);
			if(foundNode != null) foundNode.Remove();
		}
	}
	public void RefreshAllNodes()
	{
		tvwExpressionBrowser.BeginUpdate();
		foreach(TreeNode node in tvwExpressionBrowser.Nodes)
		{
			LoadTree(node);
		}
		tvwExpressionBrowser.EndUpdate();
	}
	public void Redraw()
	{
		tvwExpressionBrowser.BeginUpdate();
		RecolorNodes(tvwExpressionBrowser.Nodes);
		tvwExpressionBrowser.EndUpdate();
    }
	
	private void RecolorNodes(TreeNodeCollection nodes)
	{
		foreach(TreeNode node in nodes)
		{
			RecolorNode(node);
			RecolorNodes(node.Nodes);
		}
	}
	public void RefreshActiveNode()
	{
		if(this.tvwExpressionBrowser.SelectedNode != null)
		{
			RefreshNode(tvwExpressionBrowser.SelectedNode);
		}
	}
	private bool HasChildNodes(IBrowserNode node)
	{
		if(QueryFilterNode == null)
		{
			return node.HasChildNodes;
		}
		else
		{
			foreach(IBrowserNode child in node.ChildNodes())
			{
				if(! Filter(child))
				{
					return true;
				}
			}
			return false;
		}
	}
	private void RefreshNode(TreeNode treeNode)
	{
		if(_refreshPaused) return;
		IBrowserNode2 node = treeNode.Tag as IBrowserNode2;
		if(node != null)
		{
			treeNode.Text = node.NodeText;
			treeNode.ImageIndex = ImageIndex(node);
			treeNode.SelectedImageIndex = treeNode.ImageIndex;
            treeNode.NodeFont = GetFont(node);
            if (node is IPersistent persistent
                && persistent.Files.Count > 0)
            {
                RecolorNodesByFile(tvwExpressionBrowser.RootNode,
                    persistent.Files.First());
            }
        }
		// after the node refresh is requested (because it was updated)
		// we reset cache on all the parent nodes because otherwise
		// they could return back the old--cached--version when asked
		// for child items (e.g. after collapsing and re-expanding them)
		TreeNode parent = treeNode;
		while(parent != null)
		{
            ISchemaItem item = parent.Tag as ISchemaItem;
			if(item != null)
			{
                if (item.ClearCacheOnPersist)
                {
                    item.ClearCache();
                }
			}
			parent = parent.Parent;
		}
		LoadTree(treeNode);
	}
	private TreeNode LookUpNode(TreeNode parentNode, IBrowserNode browserNode)
	{
		TreeNodeCollection collection;
		if(parentNode == null)
		{
			collection = this.tvwExpressionBrowser.Nodes;
		}
		else
		{
			// this is the node, we return
			if(parentNode.Tag == browserNode)
				return parentNode;
			// we try to compare the key
			if(parentNode.Tag is DA.ObjectPersistence.IPersistent && browserNode is DA.ObjectPersistence.IPersistent && (parentNode.Tag as DA.ObjectPersistence.IPersistent).PrimaryKey.Equals((browserNode as DA.ObjectPersistence.IPersistent).PrimaryKey))
				return parentNode;
			collection = parentNode.Nodes;
		}
		// we go to each child
		foreach(TreeNode node in collection)
		{
			// this is the child, we return
			if(node.Tag == browserNode)
				return node;
			if(node.Tag is DA.ObjectPersistence.IPersistent && browserNode is DA.ObjectPersistence.IPersistent && (node.Tag as DA.ObjectPersistence.IPersistent).PrimaryKey.Equals((browserNode as DA.ObjectPersistence.IPersistent).PrimaryKey))
				return node;
			// we try to find in child nodes of this node
			TreeNode foundNode = null;
			if(node.Nodes.Count > 0)
				foundNode = LookUpNode(node, browserNode);
			// we found it in child nodes
			if(foundNode != null)
				return foundNode;
			// we did not find, so we go to next node
		}
		return null;
	}
	private void tvwExpressionBrowser_Click(object sender, System.EventArgs e)
	{
		OnNodeClick(new EventArgs());
	}
	private void SchemaItem_Deleted(object sender, EventArgs e)
	{
		RemoveBrowserNode(sender as IBrowserNode2);
	}
	private void tvwExpressionBrowser_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
	{
		if(! AllowEdit) return;
		bool success = false;
		bool renameCopy = true;
		TreeNode node = e.Data.GetData(typeof(TreeNode)) as TreeNode;
		// if it was anything ELSE than TreeNode, we exit
		if(node == null) return;
		TreeNode dropNode = (sender as TreeView).GetNodeAt((sender as TreeView).PointToClient(new Point(e.X, e.Y))) as TreeNode;
		ISchemaItem item;
		ISchemaItem originalItem = (node.Tag as ISchemaItem); //.GetFreshItem() as ISchemaItem;
		
		if(e.Effect == DragDropEffects.Copy)
		{
			item = originalItem.Clone() as ISchemaItem;
		}
		else
		{
			item = originalItem;
		}
		if(item == null) return;
		if(dropNode.Tag == item.RootProvider)
		{
			// Moving schema item to the root (no group)
			item.Group = null;
			success = true;
		}
		else if(dropNode.Tag is SchemaItemGroup)
		{
			// Moving schema item between groups
			if((dropNode.Tag as SchemaItemGroup).ParentItem == item.ParentItem 
				&& (dropNode.Tag as SchemaItemGroup).RootProvider == item.RootProvider)
			{
				item.Group = dropNode.Tag as SchemaItemGroup;
				success = true;
			}
		}
		else
		{
			ISchemaItem dropElement = dropNode.Tag as ISchemaItem;
			if(item.CanMove(dropElement))
			{
					if(item != dropElement)		// cannot move to itself
					{
						item.ParentNode = dropElement;
						if(dropElement.IsAbstract && ! item.IsAbstract)
						{
							item.IsAbstract = true;
						}
						success = true;
						
						if(item.ParentNode != dropElement.ParentNode)
						{
							renameCopy = false;
						}
					}
			}
		}
		if(success)
		{
			if(e.Effect == DragDropEffects.Copy)
			{
				item.SetExtensionRecursive(_schemaService.ActiveExtension);
				if(renameCopy)
				{
					item.Name = GetItemText(item);
				}
			}
			else
			{
				if(node.Parent != null)	node.Remove();
				MoveItemFile(item);
			}
			LoadTree(dropNode);
			if(e.Effect == DragDropEffects.Copy)
			{
				Origam.Workbench.Commands.EditSchemaItem edit = new Origam.Workbench.Commands.EditSchemaItem();
				edit.Owner = item;
				edit.Run();
			}
		}
	}
    private string GetItemText(ISchemaItem item)
    {
		IPersistenceService persistence =
			ServiceManager.Services.GetService(typeof(IPersistenceService)) as
				IPersistenceService;
		string text = item.Name;
        ISchemaItem[] results = null;
		do
		{
			text = ResourceUtils.GetString("CopyOf", text);
			results = persistence.SchemaProvider.FullTextSearch<ISchemaItem>(text)
				.Where(searchitem=>searchitem.GetType()==item.GetType()).ToArray();
		} while (results.LongLength != 0);
		return text;
	}
    private static void MoveItemFile(ISchemaItem item)
	{
		IPersistenceService persistence =
			ServiceManager.Services.GetService(typeof(IPersistenceService)) as
				IPersistenceService;
		persistence.SchemaProvider.BeginTransaction();
		item.Persist();
		persistence.SchemaProvider.EndTransaction();
	}
	private void tvwExpressionBrowser_DragOver(object sender, System.Windows.Forms.DragEventArgs e)
	{
		TreeNode nodeUnderMouse = tvwExpressionBrowser.GetNodeAt(tvwExpressionBrowser.PointToClient(new Point( e.X, e.Y)));
		if(nodeUnderMouse != null)
		{
			if(tvwExpressionBrowser.TopNode == nodeUnderMouse)
			{
				if(tvwExpressionBrowser.TopNode.PrevVisibleNode != null)
				{
					tvwExpressionBrowser.TopNode.PrevVisibleNode.EnsureVisible();
				}
			}
			if(nodeUnderMouse.NextVisibleNode != null && nodeUnderMouse.NextVisibleNode.IsVisible == false)
			{
				nodeUnderMouse.NextVisibleNode.EnsureVisible();
			}
		}
		TreeNode node = e.Data.GetData(typeof(TreeNode)) as TreeNode;
		if(! AllowEdit) return;
		bool isCopy = (e.KeyState & 8) == 8 && (e.AllowedEffect & DragDropEffects.Copy) == DragDropEffects.Copy;
		// if it was anything ELSE than TreeNode, we exit
		if(node == null) return;
		// if the node is not from the current extension, we cannot move it, so we exit
		if(! isCopy)
		{
			if(! _schemaService.CanEditItem(node.Tag))
			{
				return;
			}
		}
		// Moving schema item between groups
		if(node.Tag is ISchemaItem)
		{
			ISchemaItem item = node.Tag as ISchemaItem;
			TreeNode dropNode = (sender as TreeView).GetNodeAt((sender as TreeView).PointToClient(new Point(e.X, e.Y))) as TreeNode;

			if(dropNode.Tag == item.RootProvider)
			{
				// we can move an item to the root -> group = null
				e.Effect = isCopy ? DragDropEffects.Copy : DragDropEffects.Move;
				return;
			}
			else if(dropNode.Tag is SchemaItemGroup)
			{
				if((dropNode.Tag as SchemaItemGroup).ParentItem == item.ParentItem 
					&& (dropNode.Tag as SchemaItemGroup).RootProvider == item.RootProvider)
				{
					e.Effect = isCopy ? DragDropEffects.Copy : DragDropEffects.Move;
					return;
				}
			}
			else
			{
				if(item.CanMove(dropNode.Tag as IBrowserNode2))
				{
					e.Effect = isCopy ? DragDropEffects.Copy : DragDropEffects.Move;
					return;
				}
			}
		}
		
		e.Effect = DragDropEffects.None;
	}
	private void tvwExpressionBrowser_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
	{
		TreeNode nodeUnderMouse = tvwExpressionBrowser.GetNodeAt(e.X, e.Y);
		if(! AllowEdit) return;
		if(NodeUnderMouse != nodeUnderMouse)
		{
			NodeUnderMouse = nodeUnderMouse;
			OnNodeUnderMouseChanged(EventArgs.Empty);
		}
		if((sender as TreeView).SelectedNode == null) return;
		if ((e.Button & MouseButtons.Left) == MouseButtons.Left) 
		{
			// If the mouse moves outside the rectangle, start the drag.
			if (dragBoxFromMouseDown != Rectangle.Empty && 
				!dragBoxFromMouseDown.Contains(e.X, e.Y)) 
			{
				
				(sender as TreeView).DoDragDrop((sender as TreeView).SelectedNode, DragDropEffects.Move | DragDropEffects.Copy);
			}
		}
	}
	private void child_Changed(object sender, EventArgs e)
	{
		if(_loadingTree) return; // do not listen to events, while loading the tree
		IBrowserNode node = sender as IBrowserNode;
		TreeNode tnode = LookUpNode(null, node);
		if(tnode == null) return;
		RefreshNode(tnode);
	}
	private void tvwExpressionBrowser_KeyPress(object sender, KeyPressEventArgs e)
	{
		if(e.KeyChar == (char)Keys.Enter)
		{
			tvwExpressionBrowser_DoubleClick(sender, EventArgs.Empty);
			e.Handled = true;
		}
	}
	private void tvwExpressionBrowser_MouseHover(object sender, EventArgs e)
	{
	}
	private void tvwExpressionBrowser_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
	{
		if(! _loadingTree)
		{
			OnNodeClick(EventArgs.Empty);
		}
	}
	public void SetToolTip(string text)
	{
		toolTip1.SetToolTip(tvwExpressionBrowser, text);
	}
	public void SelectItem(ISchemaItem item)
	{
		ArrayList items = new ArrayList();
		ISchemaItem parentItem = item;
		while(parentItem != null)
		{
			items.Add(parentItem);
			parentItem = parentItem.ParentItem;
		}
		SchemaItemGroup parentGroup = (items[items.Count-1] as ISchemaItem).Group;
		while(parentGroup != null)
		{
			items.Add(parentGroup);
			parentGroup = parentGroup.ParentGroup;
		}
		TreeNode foundNode = null;
      
        if (tvwExpressionBrowser.Nodes.Count == 1)
		{
			if(! tvwExpressionBrowser.Nodes[0].IsExpanded)
			{
				tvwExpressionBrowser.Nodes[0].Expand();
			}
			foreach(TreeNode modelGroupNode in tvwExpressionBrowser.Nodes[0].Nodes)
			{
				foreach(IBrowserNode provider in (modelGroupNode.Tag as IBrowserNode).ChildNodes())
				{
					foreach(IBrowserNode firstChild in provider.ChildNodes())
					{
						if(firstChild is DA.ObjectPersistence.IPersistent)
						{
							Key key = (firstChild as DA.ObjectPersistence.IPersistent).PrimaryKey;
							if(key.Equals((items[items.Count-1] as DA.ObjectPersistence.IPersistent).PrimaryKey))
							{
								modelGroupNode.Expand();
								foreach(TreeNode providerNode in modelGroupNode.Nodes)
								{
									if(providerNode.Tag == provider)
									{
										providerNode.Expand();
										break;
									}
								}
								foundNode = LookUpNode(null, firstChild);
								break;
							}
						}
					}
				}
				if(foundNode != null) break;
			}
		}
		if(foundNode == null) return; //don't throw this exception, because sometimes we don't find the deepest item in the tree, e.g. with forms - throw new ArgumentOutOfRangeException("item", item, "Schema item not found in the model!");
		for(int i = items.Count - 2; i >= 0; i--)
		{
			foundNode.Expand();
			TreeNode node = LookUpNode(foundNode, items[i] as IBrowserNode);
			
			// node not found, we try to find it in its subitems
			if(node == null)
			{
				foreach(TreeNode ch in foundNode.Nodes)
				{
					ch.Expand();
					node = LookUpNode(ch, items[i] as IBrowserNode);
					if(node != null) break;
					ch.Collapse();
				}
				if(node == null) break;
			}
			
			foundNode = node;
		}
		foundNode.EnsureVisible();
		tvwExpressionBrowser.SelectedNode = foundNode;
	}
	public void RefreshItem(IBrowserNode node)
	{
        if(node is null)
        {
            return;
        }
        if (! IsHandleCreated)
        {
            return;
        }
        TreeNode tnode = null;
		try
		{
			bool expandNode = false;
			tnode = LookUpNode(null, node);
			if(node is DA.ObjectPersistence.IPersistent && (node as DA.ObjectPersistence.IPersistent).IsDeleted)
			{
				if(tnode != null)
				{
					tnode.Remove();
				}
				return;
			}
			if(tnode == null) 
			{
				// node was not found, we try to find its parent
				if(node is ISchemaItem)
				{
					ISchemaItem item = node as ISchemaItem;
					IBrowserNode parent = item.ParentItem;
					if(parent == null)
					{
						parent = item.Group;
					}
					if(parent == null)
					{
						parent = item.RootProvider;
					}
					tnode = LookUpNode(null, parent);
				}
				if(tnode == null) return;
				expandNode = true;
			}
			else
			{
				// node was found, so we refresh the inner pointer to the model element
				tnode.Tag = node;
			}
            tvwExpressionBrowser.BeginUpdate();
            RefreshNode(tnode);
			if(expandNode)
			{
				tnode.Expand();
                // try to find a subfolder, if one exists and expand it
                string subfolderName = node.GetType().SchemaItemDescription()?.FolderName;
				foreach(TreeNode subnode in tnode.Nodes)
				{
					if(subnode.Tag is NonpersistentSchemaItemNode & subnode.Text == subfolderName)
					{
						subnode.Expand();
						break;
					}
				}
				tnode = LookUpNode(null, node);
			}
		}
		finally
		{
			tvwExpressionBrowser.EndUpdate();
		}
        if (tnode != null)
        {
            tvwExpressionBrowser.SelectedNode = tnode;
            if (tnode.Parent!=null && !tnode.Parent.IsVisible)
            {
                tnode.Parent.EnsureVisible();
            }
        }
    }
	private void ExpressionBrowser_BackColorChanged(object sender, System.EventArgs e)
	{
		tvwExpressionBrowser.BackColor = this.BackColor;
	}
    private void fileWatcher_Changed(object sender, FileSystemEventArgs e)
    {
        _fileChangesPending = true;
    }
    private void watcherTimer_Tick(object sender, EventArgs e)
    {
        if (_fileChangesPending)
        {
            _fileChangesPending = false;
            GitManager.GetCache().Clear();
            Redraw();
        }
    }
}
public delegate void FilterEventHandler(object sender, ExpressionBrowserEventArgs e);

public class ExpressionBrowserEventArgs : System.EventArgs
{
	public ExpressionBrowserEventArgs(object queriedObject)
	{
		QueriedObject = queriedObject;
	}
	public object QueriedObject { get; }
	public bool Filter { get; set; } = false;
}
