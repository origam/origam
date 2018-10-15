using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Data.OleDb;
using System.Text;

using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;

using GotDotNet.Exslt;

using Origam.Schema;
using Origam.Query;
using Origam.Query.UI;
using Origam.Query.Data;
using Origam.Query.UI.Designer;
using Origam.DA;
using Origam.DA.ObjectPersistence;

using System.Reflection;

using System.IO; //Namespace for Filestreams
using System.Runtime.Serialization.Formatters.Binary; //Namespace for BinaryFormatter
using Microsoft.ApplicationBlocks.ConfigurationManagement;

namespace Origam.Workbench
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	internal class frmDesigner : System.Windows.Forms.Form
	{
		private DatasetExpressionDictionary _expressionDictionary = new DatasetExpressionDictionary();
		private ArrayList _extensions = new ArrayList();
		private bool _connected = false;
		//initialize data service
		DataService _dataService = new DataService();

		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Splitter splitter2;
		private System.Windows.Forms.MainMenu mnuMain;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.TabControl tabEditors;
		private System.Windows.Forms.MenuItem menuItem13;
		private System.Windows.Forms.MenuItem menuItem17;
		private Origam.Query.UI.Designer.ExpressionBrowser ebrMainBrowser;
		private System.Windows.Forms.MenuItem mnuSchemaOpen;
		private System.Windows.Forms.MenuItem mnuExpression;
		private System.Windows.Forms.MenuItem mnuExpressionNew;
		private System.Windows.Forms.MenuItem mnuExpressionClone;
		private System.Windows.Forms.MenuItem mnuExpressionEdit;
		private System.Windows.Forms.MenuItem mnuExpressionSave;
		private System.Windows.Forms.MenuItem mnuExpressionClose;
		private System.Windows.Forms.MenuItem mnuExpressionNewBinary;
		private System.Windows.Forms.MenuItem mnuExpressionNewBitwise;
		private System.Windows.Forms.MenuItem mnuExpressionNewCondition;
		private System.Windows.Forms.MenuItem mnuExpressionNewConversion;
		private System.Windows.Forms.MenuItem mnuExpressionNewField;
		private System.Windows.Forms.MenuItem mnuExpressionNewFunction;
		private System.Windows.Forms.MenuItem mnuExpressionNewParameter;
		private System.Windows.Forms.MenuItem mnuExpressionNewPrimitive;
		private System.Windows.Forms.MenuItem mnuExpressionNewQuery;
		private System.Windows.Forms.MenuItem mnuExpressionNewUnion;
		private System.Windows.Forms.MenuItem mnuExpressionNewRelation;
		private System.Windows.Forms.MenuItem mnuExpressionNewSearch;
		private System.Windows.Forms.MenuItem mnuExpressionChangeBinary;
		private System.Windows.Forms.MenuItem mnuExpressionChangeBitwise;
		private System.Windows.Forms.MenuItem mnuExpressionChangeCondition;
		private System.Windows.Forms.MenuItem mnuExpressionChangeConversion;
		private System.Windows.Forms.MenuItem mnuExpressionChangeField;
		private System.Windows.Forms.MenuItem mnuExpressionChangeFunction;
		private System.Windows.Forms.MenuItem mnuExpressionChangeParameter;
		private System.Windows.Forms.MenuItem mnuExpressionChangePrimitive;
		private System.Windows.Forms.MenuItem mnuExpressionChangeQuery;
		private System.Windows.Forms.MenuItem mnuExpressionChangeQueryUnion;
		private System.Windows.Forms.MenuItem mnuExpressionChangeRelation;
		private System.Windows.Forms.MenuItem mnuExpressionChangeSearch;
		private System.Windows.Forms.MenuItem mnuExpressionChange;
		private System.Windows.Forms.ContextMenu mnuContextExpressionNew;
		private System.Windows.Forms.ToolBarButton tbbNewExpression;
		private System.Windows.Forms.ToolBar tbrMain;
		private System.Windows.Forms.ToolBarButton tbbClosePage;
		private System.Windows.Forms.ImageList imgListButtons;
		private System.Windows.Forms.MenuItem mnuExpressionNewTable;
		private System.Windows.Forms.MenuItem mnuExpressionChangeTable;
		private System.Windows.Forms.ToolBarButton tbbSaveExpression;
		private System.Windows.Forms.TabControl tabExpressionProcessor;
		private System.Windows.Forms.TabPage tpExpressionSql;
		private System.Windows.Forms.TabPage tpExpressionTree;
		private System.Windows.Forms.TextBox txtExpressionSql;
		private System.Windows.Forms.TreeView tvwExpressionTree;
		private System.Windows.Forms.MenuItem mnuSchemaConnect;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.MenuItem mnuExpressionDelete;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.MenuItem mnuExpressionNewGroup;
		private System.Windows.Forms.ImageList imgListExpressions;
		private System.Windows.Forms.TabPage tpSqlInsert;
		private System.Windows.Forms.TabPage tpSqlUpdate;
		private System.Windows.Forms.TabPage tpSqlDelete;
		private System.Windows.Forms.TextBox txtSqlInsert;
		private System.Windows.Forms.TextBox txtSqlUpdate;
		private System.Windows.Forms.TextBox txtSqlDelete;
		private System.Windows.Forms.MenuItem mnuSchemaExit;
		private System.Windows.Forms.TabPage tpXsd;
		private System.Windows.Forms.TextBox txtXsd;
		private System.Windows.Forms.TabPage tpData;
		private System.Windows.Forms.Button btnRunQuery;
		private System.Windows.Forms.TabPage tpHtml;
		private System.Windows.Forms.TabPage tpXsl;
		private System.Windows.Forms.TextBox txtXsl;
		private System.Windows.Forms.MenuItem mnuStoreOfflineDataset;
		private System.Windows.Forms.SaveFileDialog dlgSave;
		private System.Windows.Forms.OpenFileDialog dlgOpen;
		private System.Windows.Forms.MenuItem mnuLoadOfflineDataset;
		private System.Windows.Forms.TabPage tpDebugDataset;
		private System.Windows.Forms.TextBox txtDebugDataset;
		private System.Windows.Forms.TabPage tpResultData;
		private System.Windows.Forms.PropertyGrid propertyGrid1;
		private System.Windows.Forms.Splitter splitter3;
		private AxSHDocVw.AxWebBrowser webBrowserData;
		private AxSHDocVw.AxWebBrowser webBrowser;
		private System.Windows.Forms.DataGrid grdData;
		private System.ComponentModel.IContainer components;

		public frmDesigner()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//load context menus
			foreach(MenuItem mi in mnuExpressionNew.MenuItems)
			{
				mnuContextExpressionNew.MenuItems.Add(mi.CloneMenu());
			}
		}

//		private void TestNewSchema()
//		{
//			DataStructureQuery q = new DataStructureQuery(new Guid("d3009231-bfbe-4b68-928e-2c7026439c95"));
//
//			Origam.DA.ObjectPersistence.Providers.OrigamPersistenceProvider provider = new Origam.DA.ObjectPersistence.Providers.OrigamPersistenceProvider();
//			
//			provider.DataService = _dataService;
//			provider.DataStructureQuery = q;
//			provider.Refresh();
//			
////			foreach(Schema.Schema sch in provider.RetrieveList(typeof(Schema.Schema), ""))
////				foreach(SchemaVersion ver in sch.Versions)
////					foreach(SchemaExtension ext in ver.Extensions)
////						System.Diagnostics.Debug.WriteLine(sch.Name + ": " + ver.Name + ": " + ext.Name);
//
//			EntityModelSchemaItemProvider entityModel = new EntityModelSchemaItemProvider();
//			entityModel.PersistenceProvider = provider;
//
////			foreach(Schema.EntityModel.IDataEntity entity in entityModel.Items)
////				foreach(Schema.EntityModel.IDataEntityColumn col in entity.ChildItems)
////					System.Diagnostics.Debug.WriteLine(entity.Name + "." + col.Name);
//
//			ebrMainBrowser.AddBrowserNode(entityModel);
//		}
//
		private void TestPersistence()
		{
			//DataSet ds = _dataService.LoadDataSet(new Guid("c61c6368-4736-4a43-a392-e5e11aa2f7c8".ToUpper()), "test");
			DataStructureQuery q = new DataStructureQuery(new Guid("c61c6368-4736-4a43-a392-e5e11aa2f7c8"));

			Origam.DA.ObjectPersistence.Providers.OrigamPersistenceProvider provider = new Origam.DA.ObjectPersistence.Providers.OrigamPersistenceProvider();
			
			provider.DataService = _dataService;
			provider.DataStructureQuery = q;
			provider.Refresh();

			//Array schemaArray = provider.RetrieveList(typeof(Schema.Schema), "");

			// 1. pokus
			//Key k = new Key();
			//k.Add("Schema_Id", new Guid("9CC4B1DD-E8B7-4152-8AB0-9DA8FEA4C784"));
			//Schema.Schema sch = provider.RetrieveInstance(typeof(Schema.Schema), k) as Schema.Schema;
			//MessageBox.Show(sch.Name);

//			// 2. pokus
//			foreach(TestItem ti in provider.RetrieveList(typeof(TestItem),
//				"refSchemaVersionId = '0EBACE7A-A08C-4F67-B28F-5836AC788163'"))
//			{
//				ti.TestField = ti.TestField + "*";
//				ti.Persist();
//
//				propertyGrid1.SelectedObject = ti;
//			}

//			provider.Update();

			// 3. pokus
			TestItem ti = new TestItem(Guid.NewGuid(), Guid.NewGuid());
			ti.PersistenceProvider = provider;
			ti.TestField = "Karluv otec";

			TestItem ti2 = new TestItem(Guid.NewGuid(), Guid.NewGuid());
			ti2.PersistenceProvider = provider;
			ti2.TestField = "Karel";
			ti2.ParentItem = ti;

			ti.Persist();
			ti2.Persist();

			provider.Update();
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmDesigner));
			this.ebrMainBrowser = new Origam.Query.UI.Designer.ExpressionBrowser();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.splitter2 = new System.Windows.Forms.Splitter();
			this.mnuMain = new System.Windows.Forms.MainMenu();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.mnuSchemaConnect = new System.Windows.Forms.MenuItem();
			this.menuItem2 = new System.Windows.Forms.MenuItem();
			this.mnuSchemaOpen = new System.Windows.Forms.MenuItem();
			this.mnuStoreOfflineDataset = new System.Windows.Forms.MenuItem();
			this.mnuLoadOfflineDataset = new System.Windows.Forms.MenuItem();
			this.menuItem17 = new System.Windows.Forms.MenuItem();
			this.mnuSchemaExit = new System.Windows.Forms.MenuItem();
			this.mnuExpression = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNew = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewBinary = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewBitwise = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewCondition = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewConversion = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewField = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewFunction = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewParameter = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewPrimitive = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewQuery = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewUnion = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewRelation = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewSearch = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewTable = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.mnuExpressionNewGroup = new System.Windows.Forms.MenuItem();
			this.mnuExpressionClone = new System.Windows.Forms.MenuItem();
			this.mnuExpressionSave = new System.Windows.Forms.MenuItem();
			this.mnuExpressionDelete = new System.Windows.Forms.MenuItem();
			this.mnuExpressionClose = new System.Windows.Forms.MenuItem();
			this.mnuExpressionEdit = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChange = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChangeBinary = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChangeBitwise = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChangeCondition = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChangeConversion = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChangeField = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChangeFunction = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChangeParameter = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChangePrimitive = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChangeQuery = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChangeQueryUnion = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChangeRelation = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChangeSearch = new System.Windows.Forms.MenuItem();
			this.mnuExpressionChangeTable = new System.Windows.Forms.MenuItem();
			this.menuItem13 = new System.Windows.Forms.MenuItem();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.tabExpressionProcessor = new System.Windows.Forms.TabControl();
			this.tpExpressionSql = new System.Windows.Forms.TabPage();
			this.txtExpressionSql = new System.Windows.Forms.TextBox();
			this.tpExpressionTree = new System.Windows.Forms.TabPage();
			this.tvwExpressionTree = new System.Windows.Forms.TreeView();
			this.imgListExpressions = new System.Windows.Forms.ImageList(this.components);
			this.tpSqlInsert = new System.Windows.Forms.TabPage();
			this.txtSqlInsert = new System.Windows.Forms.TextBox();
			this.tpSqlUpdate = new System.Windows.Forms.TabPage();
			this.txtSqlUpdate = new System.Windows.Forms.TextBox();
			this.tpSqlDelete = new System.Windows.Forms.TabPage();
			this.txtSqlDelete = new System.Windows.Forms.TextBox();
			this.tpXsl = new System.Windows.Forms.TabPage();
			this.txtXsl = new System.Windows.Forms.TextBox();
			this.tpXsd = new System.Windows.Forms.TabPage();
			this.txtXsd = new System.Windows.Forms.TextBox();
			this.tpData = new System.Windows.Forms.TabPage();
			this.grdData = new System.Windows.Forms.DataGrid();
			this.btnRunQuery = new System.Windows.Forms.Button();
			this.tpHtml = new System.Windows.Forms.TabPage();
			this.webBrowser = new AxSHDocVw.AxWebBrowser();
			this.tpResultData = new System.Windows.Forms.TabPage();
			this.webBrowserData = new AxSHDocVw.AxWebBrowser();
			this.tpDebugDataset = new System.Windows.Forms.TabPage();
			this.txtDebugDataset = new System.Windows.Forms.TextBox();
			this.tabEditors = new System.Windows.Forms.TabControl();
			this.tbrMain = new System.Windows.Forms.ToolBar();
			this.tbbNewExpression = new System.Windows.Forms.ToolBarButton();
			this.mnuContextExpressionNew = new System.Windows.Forms.ContextMenu();
			this.tbbClosePage = new System.Windows.Forms.ToolBarButton();
			this.tbbSaveExpression = new System.Windows.Forms.ToolBarButton();
			this.imgListButtons = new System.Windows.Forms.ImageList(this.components);
			this.dlgSave = new System.Windows.Forms.SaveFileDialog();
			this.dlgOpen = new System.Windows.Forms.OpenFileDialog();
			this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
			this.splitter3 = new System.Windows.Forms.Splitter();
			this.tabExpressionProcessor.SuspendLayout();
			this.tpExpressionSql.SuspendLayout();
			this.tpExpressionTree.SuspendLayout();
			this.tpSqlInsert.SuspendLayout();
			this.tpSqlUpdate.SuspendLayout();
			this.tpSqlDelete.SuspendLayout();
			this.tpXsl.SuspendLayout();
			this.tpXsd.SuspendLayout();
			this.tpData.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.grdData)).BeginInit();
			this.tpHtml.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.webBrowser)).BeginInit();
			this.tpResultData.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.webBrowserData)).BeginInit();
			this.tpDebugDataset.SuspendLayout();
			this.SuspendLayout();
			// 
			// ebrMainBrowser
			// 
			this.ebrMainBrowser.AllowEdit = true;
			this.ebrMainBrowser.Dock = System.Windows.Forms.DockStyle.Left;
			this.ebrMainBrowser.ExpressionDictionary = null;
			this.ebrMainBrowser.Extensions = null;
			this.ebrMainBrowser.Location = new System.Drawing.Point(0, 28);
			this.ebrMainBrowser.Name = "ebrMainBrowser";
			this.ebrMainBrowser.ShowFilter = true;
			this.ebrMainBrowser.Size = new System.Drawing.Size(248, 652);
			this.ebrMainBrowser.TabIndex = 0;
			this.ebrMainBrowser.ExpressionSelected += new System.EventHandler(this.ExpressionSelected);
			// 
			// splitter1
			// 
			this.splitter1.BackColor = System.Drawing.SystemColors.Control;
			this.splitter1.Location = new System.Drawing.Point(248, 28);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(4, 652);
			this.splitter1.TabIndex = 15;
			this.splitter1.TabStop = false;
			// 
			// splitter2
			// 
			this.splitter2.BackColor = System.Drawing.SystemColors.Control;
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter2.Location = new System.Drawing.Point(252, 496);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = new System.Drawing.Size(668, 4);
			this.splitter2.TabIndex = 18;
			this.splitter2.TabStop = false;
			// 
			// mnuMain
			// 
			this.mnuMain.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					this.menuItem1,
																					this.mnuExpression});
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 0;
			this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																					  this.mnuSchemaConnect,
																					  this.menuItem2,
																					  this.mnuSchemaOpen,
																					  this.mnuStoreOfflineDataset,
																					  this.mnuLoadOfflineDataset,
																					  this.menuItem17,
																					  this.mnuSchemaExit});
			this.menuItem1.Text = "Schema";
			// 
			// mnuSchemaConnect
			// 
			this.mnuSchemaConnect.Index = 0;
			this.mnuSchemaConnect.Text = "Connect to repository...";
			this.mnuSchemaConnect.Click += new System.EventHandler(this.mnuSchemaConnect_Click);
			// 
			// menuItem2
			// 
			this.menuItem2.Index = 1;
			this.menuItem2.Text = "-";
			// 
			// mnuSchemaOpen
			// 
			this.mnuSchemaOpen.Index = 2;
			this.mnuSchemaOpen.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
			this.mnuSchemaOpen.Text = "Open...";
			this.mnuSchemaOpen.Click += new System.EventHandler(this.mnuSchemaOpen_Click);
			// 
			// mnuStoreOfflineDataset
			// 
			this.mnuStoreOfflineDataset.Enabled = false;
			this.mnuStoreOfflineDataset.Index = 3;
			this.mnuStoreOfflineDataset.Text = "Save Offline Model...";
			this.mnuStoreOfflineDataset.Click += new System.EventHandler(this.mnuStoreOfflineDataset_Click);
			// 
			// mnuLoadOfflineDataset
			// 
			this.mnuLoadOfflineDataset.Index = 4;
			this.mnuLoadOfflineDataset.Text = "Load Offline Model...";
			this.mnuLoadOfflineDataset.Click += new System.EventHandler(this.mnuLoadOfflineDataset_Click);
			// 
			// menuItem17
			// 
			this.menuItem17.Index = 5;
			this.menuItem17.Text = "-";
			// 
			// mnuSchemaExit
			// 
			this.mnuSchemaExit.Index = 6;
			this.mnuSchemaExit.Text = "Exit";
			this.mnuSchemaExit.Click += new System.EventHandler(this.mnuSchemaExit_Click);
			// 
			// mnuExpression
			// 
			this.mnuExpression.Index = 1;
			this.mnuExpression.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						  this.mnuExpressionNew,
																						  this.mnuExpressionClone,
																						  this.mnuExpressionSave,
																						  this.mnuExpressionDelete,
																						  this.mnuExpressionClose,
																						  this.mnuExpressionEdit,
																						  this.mnuExpressionChange,
																						  this.menuItem13});
			this.mnuExpression.Text = "Expression";
			this.mnuExpression.Visible = false;
			// 
			// mnuExpressionNew
			// 
			this.mnuExpressionNew.Index = 0;
			this.mnuExpressionNew.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							 this.mnuExpressionNewBinary,
																							 this.mnuExpressionNewBitwise,
																							 this.mnuExpressionNewCondition,
																							 this.mnuExpressionNewConversion,
																							 this.mnuExpressionNewField,
																							 this.mnuExpressionNewFunction,
																							 this.mnuExpressionNewParameter,
																							 this.mnuExpressionNewPrimitive,
																							 this.mnuExpressionNewQuery,
																							 this.mnuExpressionNewUnion,
																							 this.mnuExpressionNewRelation,
																							 this.mnuExpressionNewSearch,
																							 this.mnuExpressionNewTable,
																							 this.menuItem3,
																							 this.mnuExpressionNewGroup});
			this.mnuExpressionNew.Text = "New";
			// 
			// mnuExpressionNewBinary
			// 
			this.mnuExpressionNewBinary.Index = 0;
			this.mnuExpressionNewBinary.Text = "Binary Operator Expression";
			this.mnuExpressionNewBinary.Click += new System.EventHandler(this.mnuExpressionNewBinary_Click);
			// 
			// mnuExpressionNewBitwise
			// 
			this.mnuExpressionNewBitwise.Index = 1;
			this.mnuExpressionNewBitwise.Text = "Bitwise Evaluation Expression";
			this.mnuExpressionNewBitwise.Click += new System.EventHandler(this.mnuExpressionNewBitwise_Click);
			// 
			// mnuExpressionNewCondition
			// 
			this.mnuExpressionNewCondition.Index = 2;
			this.mnuExpressionNewCondition.Text = "Condition Expression";
			this.mnuExpressionNewCondition.Click += new System.EventHandler(this.mnuExpressionNewCondition_Click);
			// 
			// mnuExpressionNewConversion
			// 
			this.mnuExpressionNewConversion.Index = 3;
			this.mnuExpressionNewConversion.Text = "Data Type Conversion Expression";
			this.mnuExpressionNewConversion.Click += new System.EventHandler(this.mnuExpressionNewConversion_Click);
			// 
			// mnuExpressionNewField
			// 
			this.mnuExpressionNewField.Index = 4;
			this.mnuExpressionNewField.Text = "Field Reference Expression";
			this.mnuExpressionNewField.Click += new System.EventHandler(this.mnuExpressionNewField_Click);
			// 
			// mnuExpressionNewFunction
			// 
			this.mnuExpressionNewFunction.Index = 5;
			this.mnuExpressionNewFunction.Text = "Function Call Expression";
			this.mnuExpressionNewFunction.Click += new System.EventHandler(this.mnuExpressionNewFunction_Click);
			// 
			// mnuExpressionNewParameter
			// 
			this.mnuExpressionNewParameter.Index = 6;
			this.mnuExpressionNewParameter.Text = "Parameter Reference Expression";
			this.mnuExpressionNewParameter.Click += new System.EventHandler(this.mnuExpressionNewParameter_Click);
			// 
			// mnuExpressionNewPrimitive
			// 
			this.mnuExpressionNewPrimitive.Index = 7;
			this.mnuExpressionNewPrimitive.Text = "Primitive Expression";
			this.mnuExpressionNewPrimitive.Click += new System.EventHandler(this.mnuExpressionNewPrimitive_Click);
			// 
			// mnuExpressionNewQuery
			// 
			this.mnuExpressionNewQuery.Index = 8;
			this.mnuExpressionNewQuery.Text = "Query Expression";
			this.mnuExpressionNewQuery.Click += new System.EventHandler(this.mnuExpressionNewQuery_Click);
			// 
			// mnuExpressionNewUnion
			// 
			this.mnuExpressionNewUnion.Index = 9;
			this.mnuExpressionNewUnion.Text = "Query Union Expression";
			this.mnuExpressionNewUnion.Click += new System.EventHandler(this.mnuExpressionNewUnion_Click);
			// 
			// mnuExpressionNewRelation
			// 
			this.mnuExpressionNewRelation.Index = 10;
			this.mnuExpressionNewRelation.Text = "Relation Expression";
			this.mnuExpressionNewRelation.Click += new System.EventHandler(this.mnuExpressionNewRelation_Click);
			// 
			// mnuExpressionNewSearch
			// 
			this.mnuExpressionNewSearch.Index = 11;
			this.mnuExpressionNewSearch.Text = "Search Expression";
			this.mnuExpressionNewSearch.Click += new System.EventHandler(this.mnuExpressionNewSearch_Click);
			// 
			// mnuExpressionNewTable
			// 
			this.mnuExpressionNewTable.Index = 12;
			this.mnuExpressionNewTable.Text = "Table Reference Expression";
			this.mnuExpressionNewTable.Click += new System.EventHandler(this.mnuExpressionNewTable_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 13;
			this.menuItem3.Text = "-";
			// 
			// mnuExpressionNewGroup
			// 
			this.mnuExpressionNewGroup.Index = 14;
			this.mnuExpressionNewGroup.Text = "Group";
			this.mnuExpressionNewGroup.Click += new System.EventHandler(this.mnuExpressionNewGroup_Click);
			// 
			// mnuExpressionClone
			// 
			this.mnuExpressionClone.Index = 1;
			this.mnuExpressionClone.Text = "Copy As New";
			this.mnuExpressionClone.Click += new System.EventHandler(this.mnuExpressionClone_Click);
			// 
			// mnuExpressionSave
			// 
			this.mnuExpressionSave.Index = 2;
			this.mnuExpressionSave.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
			this.mnuExpressionSave.Text = "Save";
			this.mnuExpressionSave.Click += new System.EventHandler(this.mnuExpressionSave_Click);
			// 
			// mnuExpressionDelete
			// 
			this.mnuExpressionDelete.Index = 3;
			this.mnuExpressionDelete.Text = "Delete";
			this.mnuExpressionDelete.Click += new System.EventHandler(this.mnuExpressionDelete_Click);
			// 
			// mnuExpressionClose
			// 
			this.mnuExpressionClose.Index = 4;
			this.mnuExpressionClose.Shortcut = System.Windows.Forms.Shortcut.CtrlF4;
			this.mnuExpressionClose.Text = "Close";
			this.mnuExpressionClose.Click += new System.EventHandler(this.mnuExpressionClose_Click);
			// 
			// mnuExpressionEdit
			// 
			this.mnuExpressionEdit.Index = 5;
			this.mnuExpressionEdit.Text = "Edit";
			// 
			// mnuExpressionChange
			// 
			this.mnuExpressionChange.Index = 6;
			this.mnuExpressionChange.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																								this.mnuExpressionChangeBinary,
																								this.mnuExpressionChangeBitwise,
																								this.mnuExpressionChangeCondition,
																								this.mnuExpressionChangeConversion,
																								this.mnuExpressionChangeField,
																								this.mnuExpressionChangeFunction,
																								this.mnuExpressionChangeParameter,
																								this.mnuExpressionChangePrimitive,
																								this.mnuExpressionChangeQuery,
																								this.mnuExpressionChangeQueryUnion,
																								this.mnuExpressionChangeRelation,
																								this.mnuExpressionChangeSearch,
																								this.mnuExpressionChangeTable});
			this.mnuExpressionChange.Text = "Change Type To";
			// 
			// mnuExpressionChangeBinary
			// 
			this.mnuExpressionChangeBinary.Index = 0;
			this.mnuExpressionChangeBinary.Text = "Binary Operator Expression";
			this.mnuExpressionChangeBinary.Click += new System.EventHandler(this.mnuExpressionChangeBinary_Click);
			// 
			// mnuExpressionChangeBitwise
			// 
			this.mnuExpressionChangeBitwise.Index = 1;
			this.mnuExpressionChangeBitwise.Text = "Bitwise Evaluation Expression";
			this.mnuExpressionChangeBitwise.Click += new System.EventHandler(this.mnuExpressionChangeBitwise_Click);
			// 
			// mnuExpressionChangeCondition
			// 
			this.mnuExpressionChangeCondition.Index = 2;
			this.mnuExpressionChangeCondition.Text = "Condition Expression";
			this.mnuExpressionChangeCondition.Click += new System.EventHandler(this.mnuExpressionChangeCondition_Click);
			// 
			// mnuExpressionChangeConversion
			// 
			this.mnuExpressionChangeConversion.Index = 3;
			this.mnuExpressionChangeConversion.Text = "Data Type Conversion Expression";
			this.mnuExpressionChangeConversion.Click += new System.EventHandler(this.mnuExpressionChangeConversion_Click);
			// 
			// mnuExpressionChangeField
			// 
			this.mnuExpressionChangeField.Index = 4;
			this.mnuExpressionChangeField.Text = "Field Reference Expression";
			this.mnuExpressionChangeField.Click += new System.EventHandler(this.mnuExpressionChangeField_Click);
			// 
			// mnuExpressionChangeFunction
			// 
			this.mnuExpressionChangeFunction.Index = 5;
			this.mnuExpressionChangeFunction.Text = "Function Call Expression";
			this.mnuExpressionChangeFunction.Click += new System.EventHandler(this.mnuExpressionChangeFunction_Click);
			// 
			// mnuExpressionChangeParameter
			// 
			this.mnuExpressionChangeParameter.Index = 6;
			this.mnuExpressionChangeParameter.Text = "Parameter Reference Expression";
			this.mnuExpressionChangeParameter.Click += new System.EventHandler(this.mnuExpressionChangeParameter_Click);
			// 
			// mnuExpressionChangePrimitive
			// 
			this.mnuExpressionChangePrimitive.Index = 7;
			this.mnuExpressionChangePrimitive.Text = "Primitive Expression";
			this.mnuExpressionChangePrimitive.Click += new System.EventHandler(this.mnuExpressionChangePrimitive_Click);
			// 
			// mnuExpressionChangeQuery
			// 
			this.mnuExpressionChangeQuery.Index = 8;
			this.mnuExpressionChangeQuery.Text = "Query Expression";
			this.mnuExpressionChangeQuery.Click += new System.EventHandler(this.mnuExpressionChangeQuery_Click);
			// 
			// mnuExpressionChangeQueryUnion
			// 
			this.mnuExpressionChangeQueryUnion.Index = 9;
			this.mnuExpressionChangeQueryUnion.Text = "Query Union Expression";
			this.mnuExpressionChangeQueryUnion.Click += new System.EventHandler(this.mnuExpressionChangeQueryUnion_Click);
			// 
			// mnuExpressionChangeRelation
			// 
			this.mnuExpressionChangeRelation.Index = 10;
			this.mnuExpressionChangeRelation.Text = "Relation Expression";
			this.mnuExpressionChangeRelation.Click += new System.EventHandler(this.mnuExpressionChangeRelation_Click);
			// 
			// mnuExpressionChangeSearch
			// 
			this.mnuExpressionChangeSearch.Index = 11;
			this.mnuExpressionChangeSearch.Text = "Search Expression";
			this.mnuExpressionChangeSearch.Click += new System.EventHandler(this.mnuExpressionChangeSearch_Click);
			// 
			// mnuExpressionChangeTable
			// 
			this.mnuExpressionChangeTable.Index = 12;
			this.mnuExpressionChangeTable.Text = "Table Reference Expression";
			this.mnuExpressionChangeTable.Click += new System.EventHandler(this.mnuExpressionChangeTable_Click);
			// 
			// menuItem13
			// 
			this.menuItem13.Index = 7;
			this.menuItem13.Text = "-";
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 680);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Size = new System.Drawing.Size(1072, 22);
			this.statusBar1.TabIndex = 19;
			// 
			// tabExpressionProcessor
			// 
			this.tabExpressionProcessor.Alignment = System.Windows.Forms.TabAlignment.Bottom;
			this.tabExpressionProcessor.Controls.Add(this.tpExpressionSql);
			this.tabExpressionProcessor.Controls.Add(this.tpExpressionTree);
			this.tabExpressionProcessor.Controls.Add(this.tpSqlInsert);
			this.tabExpressionProcessor.Controls.Add(this.tpSqlUpdate);
			this.tabExpressionProcessor.Controls.Add(this.tpSqlDelete);
			this.tabExpressionProcessor.Controls.Add(this.tpXsl);
			this.tabExpressionProcessor.Controls.Add(this.tpXsd);
			this.tabExpressionProcessor.Controls.Add(this.tpData);
			this.tabExpressionProcessor.Controls.Add(this.tpHtml);
			this.tabExpressionProcessor.Controls.Add(this.tpResultData);
			this.tabExpressionProcessor.Controls.Add(this.tpDebugDataset);
			this.tabExpressionProcessor.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabExpressionProcessor.HotTrack = true;
			this.tabExpressionProcessor.Location = new System.Drawing.Point(252, 500);
			this.tabExpressionProcessor.Name = "tabExpressionProcessor";
			this.tabExpressionProcessor.SelectedIndex = 0;
			this.tabExpressionProcessor.Size = new System.Drawing.Size(668, 180);
			this.tabExpressionProcessor.TabIndex = 20;
			// 
			// tpExpressionSql
			// 
			this.tpExpressionSql.Controls.Add(this.txtExpressionSql);
			this.tpExpressionSql.Location = new System.Drawing.Point(4, 4);
			this.tpExpressionSql.Name = "tpExpressionSql";
			this.tpExpressionSql.Size = new System.Drawing.Size(660, 154);
			this.tpExpressionSql.TabIndex = 0;
			this.tpExpressionSql.Text = "SQL";
			// 
			// txtExpressionSql
			// 
			this.txtExpressionSql.BackColor = System.Drawing.SystemColors.Window;
			this.txtExpressionSql.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtExpressionSql.Location = new System.Drawing.Point(0, 0);
			this.txtExpressionSql.Multiline = true;
			this.txtExpressionSql.Name = "txtExpressionSql";
			this.txtExpressionSql.ReadOnly = true;
			this.txtExpressionSql.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtExpressionSql.Size = new System.Drawing.Size(660, 154);
			this.txtExpressionSql.TabIndex = 0;
			this.txtExpressionSql.Text = "";
			// 
			// tpExpressionTree
			// 
			this.tpExpressionTree.Controls.Add(this.tvwExpressionTree);
			this.tpExpressionTree.Location = new System.Drawing.Point(4, 4);
			this.tpExpressionTree.Name = "tpExpressionTree";
			this.tpExpressionTree.Size = new System.Drawing.Size(660, 154);
			this.tpExpressionTree.TabIndex = 1;
			this.tpExpressionTree.Text = "Tree";
			// 
			// tvwExpressionTree
			// 
			this.tvwExpressionTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tvwExpressionTree.ImageList = this.imgListExpressions;
			this.tvwExpressionTree.Location = new System.Drawing.Point(0, 0);
			this.tvwExpressionTree.Name = "tvwExpressionTree";
			this.tvwExpressionTree.Size = new System.Drawing.Size(660, 154);
			this.tvwExpressionTree.TabIndex = 0;
			// 
			// imgListExpressions
			// 
			this.imgListExpressions.ColorDepth = System.Windows.Forms.ColorDepth.Depth24Bit;
			this.imgListExpressions.ImageSize = new System.Drawing.Size(16, 16);
			this.imgListExpressions.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListExpressions.ImageStream")));
			this.imgListExpressions.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// tpSqlInsert
			// 
			this.tpSqlInsert.Controls.Add(this.txtSqlInsert);
			this.tpSqlInsert.Location = new System.Drawing.Point(4, 4);
			this.tpSqlInsert.Name = "tpSqlInsert";
			this.tpSqlInsert.Size = new System.Drawing.Size(660, 154);
			this.tpSqlInsert.TabIndex = 2;
			this.tpSqlInsert.Text = "SQL Insert";
			// 
			// txtSqlInsert
			// 
			this.txtSqlInsert.BackColor = System.Drawing.SystemColors.Window;
			this.txtSqlInsert.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtSqlInsert.Location = new System.Drawing.Point(0, 0);
			this.txtSqlInsert.Multiline = true;
			this.txtSqlInsert.Name = "txtSqlInsert";
			this.txtSqlInsert.ReadOnly = true;
			this.txtSqlInsert.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtSqlInsert.Size = new System.Drawing.Size(660, 154);
			this.txtSqlInsert.TabIndex = 1;
			this.txtSqlInsert.Text = "";
			// 
			// tpSqlUpdate
			// 
			this.tpSqlUpdate.Controls.Add(this.txtSqlUpdate);
			this.tpSqlUpdate.Location = new System.Drawing.Point(4, 4);
			this.tpSqlUpdate.Name = "tpSqlUpdate";
			this.tpSqlUpdate.Size = new System.Drawing.Size(660, 154);
			this.tpSqlUpdate.TabIndex = 3;
			this.tpSqlUpdate.Text = "SQL Update";
			// 
			// txtSqlUpdate
			// 
			this.txtSqlUpdate.BackColor = System.Drawing.SystemColors.Window;
			this.txtSqlUpdate.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtSqlUpdate.Location = new System.Drawing.Point(0, 0);
			this.txtSqlUpdate.Multiline = true;
			this.txtSqlUpdate.Name = "txtSqlUpdate";
			this.txtSqlUpdate.ReadOnly = true;
			this.txtSqlUpdate.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtSqlUpdate.Size = new System.Drawing.Size(660, 154);
			this.txtSqlUpdate.TabIndex = 2;
			this.txtSqlUpdate.Text = "";
			// 
			// tpSqlDelete
			// 
			this.tpSqlDelete.Controls.Add(this.txtSqlDelete);
			this.tpSqlDelete.Location = new System.Drawing.Point(4, 4);
			this.tpSqlDelete.Name = "tpSqlDelete";
			this.tpSqlDelete.Size = new System.Drawing.Size(660, 154);
			this.tpSqlDelete.TabIndex = 4;
			this.tpSqlDelete.Text = "SQL Delete";
			// 
			// txtSqlDelete
			// 
			this.txtSqlDelete.BackColor = System.Drawing.SystemColors.Window;
			this.txtSqlDelete.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtSqlDelete.Location = new System.Drawing.Point(0, 0);
			this.txtSqlDelete.Multiline = true;
			this.txtSqlDelete.Name = "txtSqlDelete";
			this.txtSqlDelete.ReadOnly = true;
			this.txtSqlDelete.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtSqlDelete.Size = new System.Drawing.Size(660, 154);
			this.txtSqlDelete.TabIndex = 2;
			this.txtSqlDelete.Text = "";
			// 
			// tpXsl
			// 
			this.tpXsl.Controls.Add(this.txtXsl);
			this.tpXsl.Location = new System.Drawing.Point(4, 4);
			this.tpXsl.Name = "tpXsl";
			this.tpXsl.Size = new System.Drawing.Size(660, 154);
			this.tpXsl.TabIndex = 8;
			this.tpXsl.Text = "XSL";
			// 
			// txtXsl
			// 
			this.txtXsl.BackColor = System.Drawing.SystemColors.Window;
			this.txtXsl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtXsl.Location = new System.Drawing.Point(0, 0);
			this.txtXsl.Multiline = true;
			this.txtXsl.Name = "txtXsl";
			this.txtXsl.ReadOnly = true;
			this.txtXsl.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtXsl.Size = new System.Drawing.Size(660, 154);
			this.txtXsl.TabIndex = 2;
			this.txtXsl.Text = "";
			// 
			// tpXsd
			// 
			this.tpXsd.Controls.Add(this.txtXsd);
			this.tpXsd.Location = new System.Drawing.Point(4, 4);
			this.tpXsd.Name = "tpXsd";
			this.tpXsd.Size = new System.Drawing.Size(660, 154);
			this.tpXsd.TabIndex = 5;
			this.tpXsd.Text = "XSD";
			// 
			// txtXsd
			// 
			this.txtXsd.BackColor = System.Drawing.SystemColors.Window;
			this.txtXsd.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtXsd.Location = new System.Drawing.Point(0, 0);
			this.txtXsd.Multiline = true;
			this.txtXsd.Name = "txtXsd";
			this.txtXsd.ReadOnly = true;
			this.txtXsd.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtXsd.Size = new System.Drawing.Size(660, 154);
			this.txtXsd.TabIndex = 1;
			this.txtXsd.Text = "";
			// 
			// tpData
			// 
			this.tpData.Controls.Add(this.grdData);
			this.tpData.Controls.Add(this.btnRunQuery);
			this.tpData.Location = new System.Drawing.Point(4, 4);
			this.tpData.Name = "tpData";
			this.tpData.Size = new System.Drawing.Size(660, 154);
			this.tpData.TabIndex = 6;
			this.tpData.Text = "Data";
			// 
			// grdData
			// 
			this.grdData.DataMember = "";
			this.grdData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grdData.FlatMode = true;
			this.grdData.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.grdData.Location = new System.Drawing.Point(0, 24);
			this.grdData.Name = "grdData";
			this.grdData.ReadOnly = true;
			this.grdData.Size = new System.Drawing.Size(660, 130);
			this.grdData.TabIndex = 3;
			// 
			// btnRunQuery
			// 
			this.btnRunQuery.Dock = System.Windows.Forms.DockStyle.Top;
			this.btnRunQuery.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnRunQuery.Location = new System.Drawing.Point(0, 0);
			this.btnRunQuery.Name = "btnRunQuery";
			this.btnRunQuery.Size = new System.Drawing.Size(660, 24);
			this.btnRunQuery.TabIndex = 2;
			this.btnRunQuery.Text = "Run query";
			this.btnRunQuery.Click += new System.EventHandler(this.btnRunQuery_Click);
			// 
			// tpHtml
			// 
			this.tpHtml.Controls.Add(this.webBrowser);
			this.tpHtml.Location = new System.Drawing.Point(4, 4);
			this.tpHtml.Name = "tpHtml";
			this.tpHtml.Size = new System.Drawing.Size(660, 154);
			this.tpHtml.TabIndex = 7;
			this.tpHtml.Text = "HTML";
			// 
			// webBrowser
			// 
			this.webBrowser.ContainingControl = this;
			this.webBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webBrowser.Enabled = true;
			this.webBrowser.Location = new System.Drawing.Point(0, 0);
			this.webBrowser.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("webBrowser.OcxState")));
			this.webBrowser.Size = new System.Drawing.Size(660, 154);
			this.webBrowser.TabIndex = 1;
			// 
			// tpResultData
			// 
			this.tpResultData.Controls.Add(this.webBrowserData);
			this.tpResultData.Location = new System.Drawing.Point(4, 4);
			this.tpResultData.Name = "tpResultData";
			this.tpResultData.Size = new System.Drawing.Size(660, 154);
			this.tpResultData.TabIndex = 10;
			this.tpResultData.Text = "Result Data XML";
			// 
			// webBrowserData
			// 
			this.webBrowserData.ContainingControl = this;
			this.webBrowserData.Dock = System.Windows.Forms.DockStyle.Fill;
			this.webBrowserData.Enabled = true;
			this.webBrowserData.Location = new System.Drawing.Point(0, 0);
			this.webBrowserData.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("webBrowserData.OcxState")));
			this.webBrowserData.Size = new System.Drawing.Size(660, 154);
			this.webBrowserData.TabIndex = 0;
			// 
			// tpDebugDataset
			// 
			this.tpDebugDataset.Controls.Add(this.txtDebugDataset);
			this.tpDebugDataset.Location = new System.Drawing.Point(4, 4);
			this.tpDebugDataset.Name = "tpDebugDataset";
			this.tpDebugDataset.Size = new System.Drawing.Size(660, 154);
			this.tpDebugDataset.TabIndex = 9;
			this.tpDebugDataset.Text = "Debug Data";
			// 
			// txtDebugDataset
			// 
			this.txtDebugDataset.BackColor = System.Drawing.SystemColors.Window;
			this.txtDebugDataset.Dock = System.Windows.Forms.DockStyle.Fill;
			this.txtDebugDataset.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(238)));
			this.txtDebugDataset.Location = new System.Drawing.Point(0, 0);
			this.txtDebugDataset.Multiline = true;
			this.txtDebugDataset.Name = "txtDebugDataset";
			this.txtDebugDataset.ReadOnly = true;
			this.txtDebugDataset.ScrollBars = System.Windows.Forms.ScrollBars.Both;
			this.txtDebugDataset.Size = new System.Drawing.Size(660, 154);
			this.txtDebugDataset.TabIndex = 3;
			this.txtDebugDataset.Text = "";
			// 
			// tabEditors
			// 
			this.tabEditors.Dock = System.Windows.Forms.DockStyle.Top;
			this.tabEditors.ImageList = this.imgListExpressions;
			this.tabEditors.ItemSize = new System.Drawing.Size(10, 20);
			this.tabEditors.Location = new System.Drawing.Point(252, 28);
			this.tabEditors.Name = "tabEditors";
			this.tabEditors.Padding = new System.Drawing.Point(0, 0);
			this.tabEditors.SelectedIndex = 0;
			this.tabEditors.Size = new System.Drawing.Size(668, 468);
			this.tabEditors.TabIndex = 23;
			this.tabEditors.SelectedIndexChanged += new System.EventHandler(this.tabEditors_SelectedIndexChanged);
			// 
			// tbrMain
			// 
			this.tbrMain.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
			this.tbrMain.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
																					   this.tbbNewExpression,
																					   this.tbbClosePage,
																					   this.tbbSaveExpression});
			this.tbrMain.DropDownArrows = true;
			this.tbrMain.Enabled = false;
			this.tbrMain.ImageList = this.imgListButtons;
			this.tbrMain.Location = new System.Drawing.Point(0, 0);
			this.tbrMain.Name = "tbrMain";
			this.tbrMain.ShowToolTips = true;
			this.tbrMain.Size = new System.Drawing.Size(920, 28);
			this.tbrMain.TabIndex = 24;
			this.tbrMain.TextAlign = System.Windows.Forms.ToolBarTextAlign.Right;
			this.tbrMain.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.tbrMain_ButtonClick);
			// 
			// tbbNewExpression
			// 
			this.tbbNewExpression.DropDownMenu = this.mnuContextExpressionNew;
			this.tbbNewExpression.ImageIndex = 0;
			this.tbbNewExpression.Style = System.Windows.Forms.ToolBarButtonStyle.DropDownButton;
			this.tbbNewExpression.Tag = "NewExpression";
			this.tbbNewExpression.Text = "New Expression";
			// 
			// tbbClosePage
			// 
			this.tbbClosePage.ImageIndex = 1;
			this.tbbClosePage.Tag = "CloseExpression";
			this.tbbClosePage.Text = "Close Expression";
			// 
			// tbbSaveExpression
			// 
			this.tbbSaveExpression.ImageIndex = 2;
			this.tbbSaveExpression.Tag = "SaveExpression";
			// 
			// imgListButtons
			// 
			this.imgListButtons.ImageSize = new System.Drawing.Size(16, 16);
			this.imgListButtons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgListButtons.ImageStream")));
			this.imgListButtons.TransparentColor = System.Drawing.Color.Magenta;
			// 
			// dlgSave
			// 
			this.dlgSave.DefaultExt = "origammodel";
			this.dlgSave.Filter = "ORIGAM Data Model Files|*.origammodel";
			this.dlgSave.Title = "Store Data Model";
			// 
			// dlgOpen
			// 
			this.dlgOpen.DefaultExt = "origammodel";
			this.dlgOpen.Filter = "ORIGAM Data Model Files|*.origammodel";
			this.dlgOpen.Title = "Load Data Model";
			// 
			// propertyGrid1
			// 
			this.propertyGrid1.CommandsVisibleIfAvailable = true;
			this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Right;
			this.propertyGrid1.LargeButtons = false;
			this.propertyGrid1.LineColor = System.Drawing.SystemColors.ScrollBar;
			this.propertyGrid1.Location = new System.Drawing.Point(924, 0);
			this.propertyGrid1.Name = "propertyGrid1";
			this.propertyGrid1.Size = new System.Drawing.Size(148, 680);
			this.propertyGrid1.TabIndex = 26;
			this.propertyGrid1.Text = "propertyGrid1";
			this.propertyGrid1.ViewBackColor = System.Drawing.SystemColors.Window;
			this.propertyGrid1.ViewForeColor = System.Drawing.SystemColors.WindowText;
			this.propertyGrid1.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.propertyGrid1_PropertyValueChanged);
			// 
			// splitter3
			// 
			this.splitter3.BackColor = System.Drawing.SystemColors.Control;
			this.splitter3.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter3.Location = new System.Drawing.Point(920, 0);
			this.splitter3.Name = "splitter3";
			this.splitter3.Size = new System.Drawing.Size(4, 680);
			this.splitter3.TabIndex = 27;
			this.splitter3.TabStop = false;
			// 
			// frmDesigner
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(1072, 702);
			this.Controls.Add(this.tabExpressionProcessor);
			this.Controls.Add(this.splitter2);
			this.Controls.Add(this.tabEditors);
			this.Controls.Add(this.splitter1);
			this.Controls.Add(this.ebrMainBrowser);
			this.Controls.Add(this.tbrMain);
			this.Controls.Add(this.splitter3);
			this.Controls.Add(this.propertyGrid1);
			this.Controls.Add(this.statusBar1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Menu = this.mnuMain;
			this.Name = "frmDesigner";
			this.Text = "Designer\'ORIGAM";
			this.Load += new System.EventHandler(this.Form1_Load);
			this.tabExpressionProcessor.ResumeLayout(false);
			this.tpExpressionSql.ResumeLayout(false);
			this.tpExpressionTree.ResumeLayout(false);
			this.tpSqlInsert.ResumeLayout(false);
			this.tpSqlUpdate.ResumeLayout(false);
			this.tpSqlDelete.ResumeLayout(false);
			this.tpXsl.ResumeLayout(false);
			this.tpXsd.ResumeLayout(false);
			this.tpData.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.grdData)).EndInit();
			this.tpHtml.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.webBrowser)).EndInit();
			this.tpResultData.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.webBrowserData)).EndInit();
			this.tpDebugDataset.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion


		private void Form1_Load(object sender, System.EventArgs e)
		{
//			FieldReferenceExpression ex = new FieldReferenceExpression("x");
//			ex.TableName = "tblx";
//			ex.FieldName = "fld";
//
//			FieldReferenceExpression ex2 = new FieldReferenceExpression("x");
//			ex2.TableName = "tbl";
//			ex2.FieldName = "fld";
//			ex2.Guid = ex.Guid;
//
//			BinaryOperatorExpression exBin = new BinaryOperatorExpression("testbin");
//			exBin.Operator = BinaryOperatorExpression.BinaryOperatorType.Add;
//			exBin.Expressions.AddWithoutRecursionCheck(new ExpressionReference(ex.Guid));
//			exBin.Expressions.AddWithoutRecursionCheck(new ExpressionReference(ex.Guid));
//
//			xedField.Expression = ex;

//			dep.ProcessExpression(ex, ds);
//			dep.ProcessExpression(exBin, ds);
			//eda.UpdateDataset(ds);
		}

		dsExpressionDesigner mSchemaDataset = new dsExpressionDesigner();
		ExpressionDA _eda = new ExpressionDA();
		
		private void LoadDataset(Guid schemaVersionGuid, Guid schemaExtensionGuid)
		{
			_expressionDictionary.LoadSchema(schemaVersionGuid, schemaExtensionGuid, _eda);
			mSchemaDataset = _eda.GetDataset();
		}

		private void LoadSchema(Guid schemaVersionGuid, Guid schemaExtensionGuid)
		{
			_extensions.Clear();

			Guid mySchemaExtension = schemaExtensionGuid;
			dsExpressionDesigner.SchemaExtensionRow sch;
			//loop through tree of schemas (from the last to the top)
			do
			{
				//load the schema (first the current one)
				sch = mSchemaDataset.SchemaExtension.FindBySchemaExtensionGuidSchemaVersionGuid(mySchemaExtension, schemaVersionGuid);
				_extensions.Add(new SchemaExtensionOld(schemaVersionGuid, sch.SchemaExtensionGuid, sch.Name));
				
				if(!sch.IsParentSchemaExtensionGuidNull()) mySchemaExtension = sch.ParentSchemaExtensionGuid;
			}
			while (!sch.IsParentSchemaExtensionGuidNull());
				
			ebrMainBrowser.ExpressionDictionary = _expressionDictionary;
			ebrMainBrowser.Extensions = _extensions;

			//setting new configuration for data service
			string connString = (string)ConfigurationManager.Items["TestDbConnectionString" + ((SchemaExtensionOld)_expressionDictionary.LoadedExtensions[0]).Id];
			if(connString == null)
			{
				//we have no connection string, so we load default da configuration
				_dataService = new DataService();
			}
			else
			{
				DataServiceConfig cfg = new DataServiceConfig();
				cfg.ExtensionGuid = schemaExtensionGuid;
				cfg.SchemaGuid = schemaVersionGuid;
				cfg.ConnectionString = connString;
				cfg.MetadataConnectionString = _eda.ConnectionString;

				_dataService = new DataService(cfg);
			}

			//TestNewSchema();
			//TestPersistence();
		}

		private void ExpressionSelected(object sender, System.EventArgs e)
		{
			this.SuspendLayout();
			
			TabPage tp = null;

			foreach(TabPage t in tabEditors.TabPages)
			{
				if (t.Tag.ToString() == ebrMainBrowser.SelectedExpression.Guid.ToString())
				{
					tp = t;
					break;
				}
			}

			if (tp == null)
			{
				tp = AddExpressionPage(ebrMainBrowser.SelectedExpression);
			}

			tabEditors.SelectedTab = tp;
			RefreshExpressionProcessors(ebrMainBrowser.SelectedExpression);
			propertyGrid1.SelectedObject = ebrMainBrowser.SelectedExpression;

			this.ResumeLayout();
		}

		private TabPage AddExpressionPage(IExpression e)
		{
			TabPage tp = new TabPage(e.Name);
			
			//get expression icon
			ExpressionNode en = new ExpressionNode();
			en.ExpressionReference = new ExpressionReference(e);
			en.ExpressionType = e.Type;
			tp.ImageIndex = en.ImageIndex;

			tp.AutoScroll = true;
			tp.Tag = e.Guid.ToString();
			tp.Controls.Add(GetExpressionEditor(e));
			tp.Controls[0].Dock = DockStyle.Fill;
			tabEditors.TabPages.Add(tp);

			return tp;
		}

		private void CreateExpressionCopy()
		{
			if (tabEditors.SelectedTab != null)
			{
				IExpression e = ebrMainBrowser.SelectedExpression;
				e.Guid = Guid.NewGuid();
				e.Name = "Copy of " + e.Name;
				
				//always copy from parent extension to current extension, so we can edit
				e.SchemaExtensionGuid = ((SchemaExtensionOld)_extensions[0]).Id;
				TabPage tp = new TabPage(e.Name);
				tp = AddExpressionPage(e);
				MarkPageChanged(tp, e.Name);

				tabEditors.SelectedTab = tp;
				RefreshExpressionProcessors(e);
			}
		}

		private void CloseExpressionPage()
		{
			if (tabEditors.SelectedTab != null)
			{
				bool bCancel = false;

				if (tabEditors.SelectedTab.Text.EndsWith("*"))
				{
					DialogResult ret = MessageBox.Show("Do you want to save the edited expression?", "Save Expression", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
					switch (ret)
					{
						case DialogResult.Yes:
							ExpressionEditor ee = (ExpressionEditor)tabEditors.SelectedTab.Controls[0];
							SaveExpression(ee.Expression);
							break;
						case DialogResult.Cancel:
							bCancel = true;
							break;
					}	
				}

				if (!bCancel)
					tabEditors.TabPages.Remove(tabEditors.SelectedTab);
			}
		}

		private ExpressionEditor GetExpressionEditor(IExpression e)
		{
			ExpressionEditorFactory eeFact = new ExpressionEditorFactory();
			ExpressionEditor ee = eeFact.GetEditor(e.Type, _expressionDictionary, _extensions);
			ee.Expression = e;
			ee.ExpressionChange +=new EventHandler(ee_ExpressionChange);

			return ee;
		}
		private void ee_ExpressionChange(object sender, EventArgs e)
		{
			OnExpressionChange((ExpressionEditor)sender);
		}

		private void OnExpressionChange(ExpressionEditor editor)
		{
			if(editor != null)
			{
				MarkPageChanged((TabPage)editor.Parent, editor.Expression.Name);
				RefreshExpressionProcessors(editor.Expression);
			}
		}

		private void MarkPageChanged(TabPage t, string PageName)
		{
			t.Text = PageName + "*";
		}

		private void CreateExpression(ExpressionType Type)
		{
			ExpressionFactory ef = new ExpressionFactory(_expressionDictionary);
			IExpression newExp = ef.GetExpression(Type);

			newExp.SchemaVersionGuid = _expressionDictionary.DefaultSchemaVersion;
			SchemaExtensionOld ext = (SchemaExtensionOld)_expressionDictionary.LoadedExtensions[0];
			newExp.SchemaExtensionGuid = ext.Id;

			TabPage tp = AddExpressionPage(newExp);
			MarkPageChanged(tp, newExp.Name);
			tabEditors.SelectedTab = tp;
		}

		private void ChangeExpressionType(ExpressionType targetType)
		{
			ExpressionEditor oldEd = (ExpressionEditor)tabEditors.SelectedTab.Controls[0];
			IExpression e = oldEd.Expression;

			if(e.SchemaExtensionGuid == ((SchemaExtensionOld)_expressionDictionary.LoadedExtensions[0]).Id)
			{
				ExpressionFactory ef = new ExpressionFactory(_expressionDictionary);
			
				ExpressionEditor newEd = GetExpressionEditor(ef.ChangeExpressionType(e, targetType));

				//remove old editor
				tabEditors.SelectedTab.Controls.RemoveAt(0);
				//put new editor on tab page
				tabEditors.SelectedTab.Controls.Add(newEd);
				//change tab icon
				tabEditors.SelectedTab.ImageIndex = (int)newEd.Expression.Type;
				//mark the page changed
				MarkPageChanged(tabEditors.SelectedTab, newEd.Expression.Name);
			}
		}

		private void SaveExpression(IExpression e)
		{
			if(e.SchemaExtensionGuid == ((SchemaExtensionOld)_expressionDictionary.LoadedExtensions[0]).Id)
			{
				_expressionDictionary.UpdateExpression(e);

				//update tree view
				ebrMainBrowser.ExpressionDictionary = _expressionDictionary;
			
				//remove * from the tab caption
				if (tabEditors.SelectedTab != null)
					tabEditors.SelectedTab.Text = tabEditors.SelectedTab.Text.Substring(0, tabEditors.SelectedTab.Text.Length - 1);
			}
		}

		private void DeleteExpression(IExpression e)
		{
			if(e.SchemaExtensionGuid == ((SchemaExtensionOld)_expressionDictionary.LoadedExtensions[0]).Id)
			{
				_expressionDictionary.DeleteExpression(e);

				//update tree view
				ebrMainBrowser.ExpressionDictionary = _expressionDictionary;
			
				//remove the tab 
				if (tabEditors.SelectedTab != null)
					tabEditors.TabPages.Remove(tabEditors.SelectedTab);
			}
		}

		private void mnuSchemaOpen_Click(object sender, System.EventArgs e)
		{
			AskForExtension();
		}

		private void mnuExpressionSave_Click(object sender, System.EventArgs e)
		{
			if (tabEditors.SelectedTab != null)
			{
				if (tabEditors.SelectedTab.Text.EndsWith("*"))
				{			
					ExpressionEditor ee = (ExpressionEditor)tabEditors.SelectedTab.Controls[0];
					SaveExpression(ee.Expression);
				}
			}

		}

		private void mnuExpressionNewBinary_Click(object sender, System.EventArgs e)
		{
			CreateExpression(ExpressionType.BinaryOperatorExpression);
		}

		private void mnuExpressionNewBitwise_Click(object sender, System.EventArgs e)
		{
			CreateExpression(ExpressionType.BitwiseEvaluationExpression);
		}

		private void mnuExpressionNewCondition_Click(object sender, System.EventArgs e)
		{
			CreateExpression(ExpressionType.ConditionExpression);
		}

		private void mnuExpressionNewConversion_Click(object sender, System.EventArgs e)
		{
			CreateExpression(ExpressionType.DataTypeConversionExpression);
		}

		private void mnuExpressionNewField_Click(object sender, System.EventArgs e)
		{
			CreateExpression(ExpressionType.FieldReferenceExpression);
		}

		private void mnuExpressionNewFunction_Click(object sender, System.EventArgs e)
		{
			CreateExpression(ExpressionType.FunctionCallExpression);
		}

		private void mnuExpressionNewParameter_Click(object sender, System.EventArgs e)
		{
			CreateExpression(ExpressionType.ParameterReferenceExpression);
		}

		private void mnuExpressionNewPrimitive_Click(object sender, System.EventArgs e)
		{
			CreateExpression(ExpressionType.PrimitiveExpression);
		}

		private void mnuExpressionNewQuery_Click(object sender, System.EventArgs e)
		{
			CreateExpression(ExpressionType.QueryExpression);
		}

		private void mnuExpressionNewUnion_Click(object sender, System.EventArgs e)
		{
			CreateExpression(ExpressionType.QueryUnionExpression);
		}

		private void mnuExpressionNewRelation_Click(object sender, System.EventArgs e)
		{
			CreateExpression(ExpressionType.RelationExpression);
		}

		private void mnuExpressionNewSearch_Click(object sender, System.EventArgs e)
		{
			CreateExpression(ExpressionType.SearchExpression);
		}

		private void mnuExpressionChangeBinary_Click(object sender, System.EventArgs e)
		{
			ChangeExpressionType(ExpressionType.BinaryOperatorExpression);
		}

		private void mnuExpressionChangeBitwise_Click(object sender, System.EventArgs e)
		{
			ChangeExpressionType(ExpressionType.BitwiseEvaluationExpression);
		}

		private void mnuExpressionChangeCondition_Click(object sender, System.EventArgs e)
		{
			ChangeExpressionType(ExpressionType.ConditionExpression);
		}

		private void mnuExpressionChangeConversion_Click(object sender, System.EventArgs e)
		{
			ChangeExpressionType(ExpressionType.DataTypeConversionExpression);
		}

		private void mnuExpressionChangeField_Click(object sender, System.EventArgs e)
		{
			ChangeExpressionType(ExpressionType.FieldReferenceExpression);
		}

		private void mnuExpressionChangeFunction_Click(object sender, System.EventArgs e)
		{
			ChangeExpressionType(ExpressionType.FunctionCallExpression);
		}

		private void mnuExpressionChangeParameter_Click(object sender, System.EventArgs e)
		{
			ChangeExpressionType(ExpressionType.ParameterReferenceExpression);
		}

		private void mnuExpressionChangePrimitive_Click(object sender, System.EventArgs e)
		{
			ChangeExpressionType(ExpressionType.PrimitiveExpression);
		}

		private void mnuExpressionChangeQuery_Click(object sender, System.EventArgs e)
		{
			ChangeExpressionType(ExpressionType.QueryExpression);
		}

		private void mnuExpressionChangeQueryUnion_Click(object sender, System.EventArgs e)
		{
			ChangeExpressionType(ExpressionType.QueryUnionExpression);
		}

		private void mnuExpressionChangeRelation_Click(object sender, System.EventArgs e)
		{
			ChangeExpressionType(ExpressionType.RelationExpression);
		}

		private void mnuExpressionChangeSearch_Click(object sender, System.EventArgs e)
		{
			ChangeExpressionType(ExpressionType.SearchExpression);
		}

		private void tbrMain_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch((string)e.Button.Tag)
			{
				case "NewExpression":
					mnuExpressionNewField_Click(this, new EventArgs());
					break;

				case "CloseExpression":
					CloseExpressionPage();
					break;

				case "SaveExpression":
					mnuExpressionSave_Click(this, new EventArgs());
					break;
			}
		}

		private void RefreshExpressionProcessors(IExpression e)
		{			
			ResetExpressionProcessors();
			TreeviewExpressionProcessor tep = new TreeviewExpressionProcessor(_expressionDictionary);
			MsSqlServerExpressionProcessor sep = new MsSqlServerExpressionProcessor(_expressionDictionary);
			XmlExpressionProcessor xep = new XmlExpressionProcessor(_expressionDictionary);

			//display line feeds and tabulators in SQL statements
			sep.FormatResults = true;
			sep.AppendTableNameToField = true;

			tvwExpressionTree.Nodes.Add(tep.RenderTreeNode(e));
			tvwExpressionTree.ExpandAll();
			
			if (e.Type == ExpressionType.QueryExpression)
			{
				if(e.TableReference == null)
				{
					//top level query
					sep.AppendTableNameToField = true;
					
					try
					{
						txtExpressionSql.Text = sep.ViewSql((QueryExpression)e);
					}
					catch(Exception ex)
					{
						txtExpressionSql.Text = "Error occured: " + ex.Message;
					}
					
					try
					{
						txtXsd.Text = xep.GetXsd((QueryExpression)e);
					}
					catch(Exception ex)
					{
						txtXsd.Text = "Error occured: " + ex.Message;
					}

					try
					{
						txtXsl.Text = xep.GetXsl((QueryExpression)e);
					}
					catch(Exception ex)
					{
						txtXsl.Text = "Error occured: " + ex.Message;
					}

					try
					{
						txtSqlInsert.Text = sep.InsertSql((QueryExpression)e);
					}
					catch(Exception ex)
					{
						txtSqlInsert.Text = "Error occured: " + ex.Message;
					}

					try
					{
						txtSqlUpdate.Text = sep.UpdateSql((QueryExpression)e);
					}
					catch(Exception ex)
					{
						txtSqlUpdate.Text = "Error occured: " + ex.Message;
					}

					try
					{
						txtSqlDelete.Text = sep.DeleteSql((QueryExpression)e);
					}
					catch(Exception ex)
					{
						txtSqlDelete.Text = "Error occured: " + ex.Message;
					}
				}
				else
				{
					//sub query
					try
					{
						txtExpressionSql.Text = sep.SelectSql((QueryExpression)e);
					}
					catch(Exception ex)
					{
						txtExpressionSql.Text = "Error occured: " + ex.Message;
					}
				}
			}
			else if (e.Type == ExpressionType.QueryUnionExpression)
				txtExpressionSql.Text = sep.ViewSql((QueryUnionExpression)e);
			else if (e.Type == ExpressionType.TableReferenceExpression)
				txtExpressionSql.Text = sep.TableDefinitionSql((TableReferenceExpression)e);
			else
				txtExpressionSql.Text = sep.ExpressionSql(e);
		}

		private void ResetExpressionProcessors()
		{
			tvwExpressionTree.Nodes.Clear();
			txtExpressionSql.Text = "";
			txtSqlInsert.Text = "";
			txtSqlUpdate.Text = "";
			txtSqlDelete.Text = "";
			txtXsd.Text = "";
			txtXsl.Text = "";
			ResetDataProcessors();
		}

		private void ResetDataProcessors()
		{
			txtDebugDataset.Text = "";
			webBrowser.Navigate("about:blank");
			webBrowserData.Navigate("about:blank");
			//grdData.DataSource = null;
			//grdData.ClearFields();
		}

		private void mnuExpressionClose_Click(object sender, System.EventArgs e)
		{
			CloseExpressionPage();
		}

		private void btnClosePage_Click(object sender, EventArgs e)
		{
			CloseExpressionPage();
		}

		private void mnuExpressionNewTable_Click(object sender, System.EventArgs e)
		{
			CreateExpression(ExpressionType.TableReferenceExpression);
		}

		private void mnuExpressionChangeTable_Click(object sender, System.EventArgs e)
		{
			ChangeExpressionType(ExpressionType.TableReferenceExpression);
		}

		private void tabEditors_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			propertyGrid1.SelectedObject = null;

			if (tabEditors.SelectedTab != null)
			{
				ExpressionEditor ee = (ExpressionEditor)tabEditors.SelectedTab.Controls[0];
				RefreshExpressionProcessors(ee.Expression);
			}
			else
			{
				ResetExpressionProcessors();
			}
		}

		private void mnuSchemaNew_Click(object sender, System.EventArgs e)
		{
//			FileStream fs = new FileStream(@"D:\asQueryExpressionList.bin", FileMode.Open);
//			BinaryFormatter bf = new BinaryFormatter();
//			object tmpObject =((object)bf.Deserialize(fs));
//			fs.Close();
//
//			ExpressionCollection tmpDict = (ExpressionCollection)tmpObject;	
//	
//			foreach(Expression ex in tmpDict)
//			{
//				foreach(Expression ex2 in _expressionDictionary)
//				{
//					if (ex2.Type == ExpressionType.TableReferenceExpression)
//					{
//						TableReferenceExpression tr = (TableReferenceExpression)ex2;
//						
//						if(ex.Type == ExpressionType.FieldReferenceExpression)
//						{
//							FieldReferenceExpression fe = (FieldReferenceExpression)ex;
//							if(fe.TableName == tr.TableName)
//							{
//								ex.TableReference = new ExpressionReference(ex2.Guid);
//								ex.Group = "";
//							}
//						}
//						else
//						{
//							if(ex.Group == ex2.Name)
//							{
//								ex.TableReference = new ExpressionReference(ex2.Guid);
//								ex.Group = "";
//							}
//
//							if(ex.Type == ExpressionType.RelationExpression)
//							{
//								RelationExpression re = (RelationExpression)ex;
//								if(re.RightTableName == tr.TableName)
//								{
//									re.RelatedTable = new ExpressionReference(ex2.Guid);
//								}
//							}
//
//							if(ex.Type == ExpressionType.QueryExpression)
//							{
//								QueryExpression qe = (QueryExpression)ex;
//								if(qe.BaseTable == tr.TableName)
//								{
//									qe.BaseTableReference = new ExpressionReference(ex2.Guid);
//								}
//							}
//						}
//					}
//				}
//
//				_expressionDictionary.Add(ex);
//				SaveExpression(ex);
//			}
//			
//			ebrMainBrowser.ExpressionDictionary = _expressionDictionary;
		}

		private void mnuSchemaConnect_Click(object sender, System.EventArgs e)
		{
			Connect();
		}

		private void Connect()
		{
			MSDASC.DataLinks mydlg = new MSDASC.DataLinks();
			OleDbConnection OleCon = new OleDbConnection();
			ADODB._Connection ADOcon  = new ADODB.ConnectionClass();
			ADOcon.ConnectionString = (string)ConfigurationManager.Items["RepositoryConnectionString"];
			object obj = (object)ADOcon;
			//Cast the generic object that PromptNew returns to an ADODB._Connection.
			if(mydlg.PromptEdit(ref obj))
			{
				if(ADOcon != null)
					_eda.ConnectionString = ADOcon.ConnectionString;
			
				_connected = true;
				//mConnectionDatabase = ADOcon.DefaultDatabase;
				//UpdateWindowText();
			}
		}

		string mSchemaPath = "";
		private void UpdateWindowText()
		{
			this.Text = "Designer'ORIGAM [" + mSchemaPath + "]";
		}

		private void mnuExpressionClone_Click(object sender, System.EventArgs e)
		{
			CreateExpressionCopy();
		}

		private void mnuExpressionDelete_Click(object sender, System.EventArgs e)
		{
			if (tabEditors.SelectedTab != null)
			{
				ExpressionEditor ee = (ExpressionEditor)tabEditors.SelectedTab.Controls[0];
				DeleteExpression(ee.Expression);
			}
		}

		private void mnuExpressionNewGroup_Click(object sender, System.EventArgs e)
		{
			ebrMainBrowser.AddGroup();
		}

		private void mnuSchemaExit_Click(object sender, System.EventArgs e)
		{
			Close();
			Application.Exit();
		}

		///////////////////////////////////////////////////
		/// Runs a query and displays results in DataGrid
		///////////////////////////////////////////////////
		
		private void btnRunQuery_Click(object sender, System.EventArgs e)
		{
			IExpression exp = ActiveExpression();

			ResetDataProcessors();

			if(exp != null && (exp.Type == ExpressionType.QueryExpression || exp.Type == ExpressionType.QueryUnionExpression))
			{
				DataSet ds = new DataSet("ROOT");
				System.IO.MemoryStream msData = new MemoryStream();

				//create a style-sheet
				XmlExpressionProcessor xep = new XmlExpressionProcessor(_expressionDictionary);
				SaveStringToFile(@"Output\Style.xsl", xep.GetXsl((QueryExpression)exp));

				try
				{
					_dataService.DataStructureQuery = new DataStructureQuery(exp.Guid);
					ds = _dataService.LoadDataSet();
				}
				catch(Exception ex)
				{
					MessageBox.Show(ex.GetBaseException().Message);
					txtDebugDataset.Text = DebugClass.DataDebug(ds);
				}

				//rename the dataset in case data service returned wrong name
				ds.DataSetName = "ROOT";

				//write xml to the stream
				ds.WriteXml(msData);
				
				//and load it for XPath processing
				msData.Position = 0;
				XPathDocument xpData = new XPathDocument(msData);

				//write xml to file for debug purposes
				ds.WriteXmlSchema(@"Output\Schema.xsd");
				ds.WriteXml(@"Output\ResultData.xml", XmlWriteMode.IgnoreSchema);

				try
				{
					//read xsl
					GotDotNet.Exslt.ExsltTransform xslt = new GotDotNet.Exslt.ExsltTransform();
					xslt.Load(@"Output\TestStyle.xsl");

					//configre xsl output file
					XmlTextWriter writer = new XmlTextWriter(@"Output\Transformed.html", System.Text.Encoding.UTF8);

					//transform to file
					xslt.Transform(xpData, null, writer);
					writer.Close();
				}
				catch(Exception ex)
				{
					MessageBox.Show(ex.Message);
				}

				try
				{
					//grdData.DataSource = null;
					//grdData.DataMember = "";
					grdData.DataSource = ds;
//						grdData.Rebind();
					grdData.DataMember = ((QueryExpression)exp).BaseTableReference.Expression.Name;
				}
				catch
				{
				}					

//					try
//					{
//						//expand hiararchical grid
//						for(int i = 0; i <= grdData.Bands; i++)
//						{
//							grdData.ExpandBand(i);
//						}
//					}
//					catch(System.Exception ex)
//					{
//					}
//
//					//autosize all columns
//					foreach(C1.Win.C1TrueDBGrid.C1DisplayColumn dc in grdData.Splits[0].DisplayColumns)
//					{
//						dc.AutoSize();
//					}
				
				//System.Xml.XmlDocument xmldoc = new System.Xml.XmlDocument();
				//xmldoc.LoadXml(ds.GetXml());
				//xmldoc.
				//_gridDataSet.WriteXmlSchema("C:\\xmlschema.xsd");
				//WriteXmlToFile(_gridDataSet);
				webBrowser.Navigate(Environment.CurrentDirectory + @"\Output\Transformed.html");
				webBrowserData.Navigate(Environment.CurrentDirectory + @"\Output\ResultData.xml");
//					xedXslEditor.SourceXMLFile = Environment.CurrentDirectory + @"\Output\ResultData.xml";
//					xedXslEditor.XSLFile = Environment.CurrentDirectory + @"\Output\TestStyle.xsl";
				
			}
		}

		private void SaveStringToFile(string fileName, string value)
		{
			// create file
			System.IO.FileStream fs = new System.IO.FileStream
				(fileName, FileMode.Create, FileAccess.Write, FileShare.Write);
			fs.Close();

			//write
			StreamWriter sw = new StreamWriter(fileName, true, Encoding.UTF8);
			sw.Write(value);
			sw.Close();
		}

		private ExpressionEditor ActiveExpressionEditor()
		{
			if (tabEditors.SelectedTab != null)
				return (ExpressionEditor)tabEditors.SelectedTab.Controls[0];
			else
				return null;
		}

		private IExpression ActiveExpression()
		{
			ExpressionEditor ee = ActiveExpressionEditor();

			if(ee != null)
				return ee.Expression;
			else
				return null;
		}

		private void mnuStoreOfflineDataset_Click(object sender, System.EventArgs e)
		{
			dlgSave = new SaveFileDialog();

			if(dlgSave.ShowDialog() == DialogResult.OK)
				((DatasetExpressionDictionary)_expressionDictionary).SaveDataset(dlgSave.FileName);
		}

		/// <summary>
		/// If not connected, connect to the repository and loads the schema. Then opens dialog for selecting
		/// extension and loads the extension.
		/// </summary>
		private void AskForExtension()
		{
			//if not connected to the repository then connect
			if(!_connected)
				Connect();

			//if connected, then proceed to open schema, otherwise exit
			if(_connected)
			{
				OpenSchema f = new OpenSchema();
				f.SchemaDataset = _eda.GetSchemaDataset();
			
				if (f.ShowDialog() == DialogResult.OK)
				{
					if (f.SelectedExtension != null)
					{
						LoadDataset(f.SelectedExtension.SchemaVersionGuid, f.SelectedExtension.Id);
						LoadSchema(f.SelectedExtension.SchemaVersionGuid, f.SelectedExtension.Id);
						mSchemaPath = f.SelectedSchemaPath;
						UpdateWindowText();

						tbrMain.Enabled = true;
						mnuStoreOfflineDataset.Enabled = true;
						mnuExpression.Visible = true;
					}
				}
			}
		}

		private void mnuLoadOfflineDataset_Click(object sender, System.EventArgs e)
		{
			dlgOpen = new OpenFileDialog();

			if(dlgOpen.ShowDialog() == DialogResult.OK)
			{
				_eda.ConnectionString = "File://" + dlgOpen.FileName;
				_connected = true;

				AskForExtension();
			}
		}

		private void propertyGrid1_PropertyValueChanged(object s, System.Windows.Forms.PropertyValueChangedEventArgs e)
		{
			OnExpressionChange(ActiveExpressionEditor());
		}

	}
}
