using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.ComponentModel.Design;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.DA;
using Origam.DA.Service;



namespace Origam.Gui.Win
{
	//	[Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]


	
	
	[System.ComponentModel.Designer(typeof(ControlDesigner))]
	[ToolboxBitmap(typeof(AsPanel))]
	public class AsPanel2: UserControl, IContainerControl, ISupportInitialize, IAsDataServiceConsumer
	{
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.CheckBox chkGrid;
		private System.Windows.Forms.Panel pnlDataControl;
		private System.Windows.Forms.DataGrid grid;
		private Origam.Gui.Win.DbNavigator dbNav;
		private System.ComponentModel.IContainer components;

		private BindingManagerBase _bindingManager;

		public AsPanel2()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			_backColor = this.BackColor;
			_backColorActive = ColorActive(this.BackColor);

			chkGrid_CheckedChanged(null,null);
			//this.ControlAdded += new ControlEventHandler(AsPanel_ControlAdded);
			//this.BackColorChanged += new EventHandler(AsPanel_BackColorChanged);
		}


		public AsPanel2(System.ComponentModel.IContainer container) : this()
		{
			container.Add(this);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if( components != null )
					components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Component Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.grid = new System.Windows.Forms.DataGrid();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.chkGrid = new System.Windows.Forms.CheckBox();
			this.pnlDataControl = new System.Windows.Forms.Panel();
			this.dbNav = new Origam.Gui.Win.DbNavigator();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.pnlDataControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.AllowNavigation = false;
			this.grid.AlternatingBackColor = System.Drawing.Color.Beige;
			this.grid.BackColor = System.Drawing.Color.White;
			this.grid.BackgroundColor = System.Drawing.Color.Beige;
			this.grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.grid.CaptionBackColor = System.Drawing.Color.Beige;
			this.grid.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.grid.CaptionForeColor = System.Drawing.Color.DarkSlateBlue;
			this.grid.CaptionVisible = false;
			this.grid.DataMember = "";
			this.grid.FlatMode = true;
			this.grid.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.grid.ForeColor = System.Drawing.Color.DarkSlateBlue;
			this.grid.GridLineColor = System.Drawing.Color.Peru;
			this.grid.GridLineStyle = System.Windows.Forms.DataGridLineStyle.None;
			this.grid.HeaderBackColor = System.Drawing.Color.Maroon;
			this.grid.HeaderFont = new System.Drawing.Font("Microsoft Sans Serif", 8F);
			this.grid.HeaderForeColor = System.Drawing.Color.Beige;
			this.grid.LinkColor = System.Drawing.Color.Maroon;
			this.grid.Location = new System.Drawing.Point(-1000, -1000);
			this.grid.Name = "grid";
			this.grid.ParentRowsBackColor = System.Drawing.Color.BurlyWood;
			this.grid.ParentRowsForeColor = System.Drawing.Color.DarkSlateBlue;
			this.grid.ParentRowsVisible = false;
			this.grid.SelectionBackColor = System.Drawing.Color.DarkKhaki;
			this.grid.SelectionForeColor = System.Drawing.Color.GhostWhite;
			this.grid.Size = new System.Drawing.Size(176, 72);
			this.grid.TabIndex = 0;
			this.grid.TabStop = false;
			this.grid.Navigate += new System.Windows.Forms.NavigateEventHandler(this.dataGrid1_Navigate);
			// 
			// imageList1
			// 
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// chkGrid
			// 
			this.chkGrid.BackColor = System.Drawing.Color.Transparent;
			this.chkGrid.Dock = System.Windows.Forms.DockStyle.Right;
			this.chkGrid.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.chkGrid.Location = new System.Drawing.Point(440, 0);
			this.chkGrid.Name = "chkGrid";
			this.chkGrid.Size = new System.Drawing.Size(80, 24);
			this.chkGrid.TabIndex = 1;
			this.chkGrid.Text = "Grid/Fields";
			this.chkGrid.CheckedChanged += new System.EventHandler(this.chkGrid_CheckedChanged);
			// 
			// pnlDataControl
			// 
			this.pnlDataControl.BackColor = System.Drawing.Color.Transparent;
			this.pnlDataControl.Controls.Add(this.dbNav);
			this.pnlDataControl.Controls.Add(this.chkGrid);
			this.pnlDataControl.Dock = System.Windows.Forms.DockStyle.Top;
			this.pnlDataControl.Location = new System.Drawing.Point(0, 0);
			this.pnlDataControl.Name = "pnlDataControl";
			this.pnlDataControl.Size = new System.Drawing.Size(520, 24);
			this.pnlDataControl.TabIndex = 1;
			// 
			// dbNav
			// 
			this.dbNav.BackColor = System.Drawing.Color.Transparent;
			this.dbNav.Dock = System.Windows.Forms.DockStyle.Right;
			this.dbNav.Location = new System.Drawing.Point(184, 0);
			this.dbNav.Name = "dbNav";
			this.dbNav.Size = new System.Drawing.Size(256, 24);
			this.dbNav.TabIndex = 2;
			// 
			// AsPanel
			// 
			this.AutoScroll = true;
			this.BackColor = System.Drawing.Color.FloralWhite;
			this.Controls.Add(this.grid);
			this.Controls.Add(this.pnlDataControl);
			this.Name = "AsPanel";
			this.Size = new System.Drawing.Size(520, 160);
			this.Load += new System.EventHandler(this.AsPanel_Load);
			this.MouseEnter += new System.EventHandler(this.AsPanel_MouseEnter);
			this.MouseLeave += new System.EventHandler(this.AsPanel_MouseLeave);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.pnlDataControl.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion



		public bool GridVisible
		{
			get{return (chkGrid.Checked == true);}
			set
			{	
				chkGrid.Checked = value;
			}
            
		}


		//private bool _VcrPanelVisible;
		public bool VcrPanelVisible
		{
			get{return (pnlDataControl.Height == 32);}
			set
			{
				DataControlPanelMoving(value);
			}

			//_VcrPanelVisible=value;
			//				if(value)
			//				{
			//					pnlDataControl.Height = 32;
			//				}else
			//					pnlDataControl.Height = 0;
			//				}
		}


		private void DataControlPanelMoving(bool show)
		{
			if(show)
			{
				pnlDataControl.Height = 32;
				this.Height +=32;
				ChangingLocation(this as Control,32);
			}
			else
			{
				pnlDataControl.Height = 0;
				this.Height -=32;
				ChangingLocation(this as Control,-32);
			}

		}

		private void ChangingLocation(Control parent, int change)
		{
			foreach(Control ctrl in parent.Controls)
			{
				if(!(ctrl.Name =="pnlDataControl" || ctrl.Name == "grid"))
				{
					ctrl.Top +=change;
					if(ctrl.Controls!=null && ctrl.Controls.Count > 0)
					{
						ChangingLocation(ctrl, change);
					}
				}
			}

		}
		

		//implements standard Datamember
		private string _dataMember;



		[Category("Data")]
		[Editor("System.Windows.Forms.Design.DataMemberListEditor, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a",typeof(System.Drawing.Design.UITypeEditor))]
		public string DataMember
		{
			get{return _dataMember;}
			set
			{
				_dataMember=value;
				ProcessBindings();
			}
				
		}

		
		//implements standard DataSource
		private DataSet _dataSource;
		
		
		
		[Category("Data")]
		[RefreshProperties(RefreshProperties.Repaint)]
		[TypeConverter("System.Windows.Forms.Design.DataSourceConverter, System.Design, Version=1.0.3300.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
		public object DataSource
		{
			get{ return _dataSource;}
		
			set
			{ 
				_dataSource=ConvertInputData(value);
				ProcessBindings();
			}
		}


		string _panelTitle = "";
		[Category("Origam")]
		public string PanelTitle
		{
			get{ return _panelTitle;}
		
			set
			{ 
				_panelTitle = value;
			}
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

		//this Method provide functionality around controls and their DataBindings
		//when datasource or datamemer is changed we must rearrange all bindings
		private void ProcessBindings()
		{
			if(this._dataSource == null || this._dataMember == null || this._dataMember == "")
				return;
			_bindingManager=this.BindingContext[this._dataSource,this._dataMember];

			if(!this.DesignMode)		
				this.grid.TableStyles.Add(SetUpGridStyle(_dataSource,_dataMember) );

			_dataSource.Tables[FormGenerator.FindTableByDataMember(_dataSource,_dataMember)].RowChanged +=new DataRowChangeEventHandler(AsPanel_RowChanged); 
			_dataSource.Tables[FormGenerator.FindTableByDataMember(_dataSource,_dataMember)].RowDeleted +=new DataRowChangeEventHandler(AsPanel_RowDeleted);
			_dataSource.Tables[FormGenerator.FindTableByDataMember(_dataSource,_dataMember)].RowDeleting +=new DataRowChangeEventHandler(AsPanel_RowDeleting);
			
			
			this.grid.SetDataBinding(this._dataSource,this._dataMember);
			this.dbNav.SetDataBinding(this._dataSource,this._dataMember);
			_bindingManager.CurrentChanged +=new EventHandler(AsPanel_CurrentChanged);
			CheckForZeroRowCount(_bindingManager as CurrencyManager);
		
			bool lastval=chkGrid.Checked;
			chkGrid.Checked = !lastval;
			chkGrid_CheckedChanged(null,null);
			chkGrid.Checked = lastval;
		}
        
		private DataGridTableStyle SetUpGridStyle(DataSet ds, string member)
		{
		
	
			GridColumnControl controlType;

			DataGridTableStyle tableStyle= new DataGridTableStyle();
			tableStyle.ColumnHeadersVisible = true;
			
			string tableName=FormGenerator.FindTableByDataMember(ds,member);

			//tableStyle.MappingName  = member;
			tableStyle.MappingName  = tableName;
            			
			foreach(DataColumn column in ds.Tables[tableName].Columns)
			{
				bool visibleColumnInGrid=true;
				Guid lookup= Guid.Empty;
				controlType = GridColumnControl.TextBox;

				IDictionaryEnumerator myEnumerator = column.ExtendedProperties.GetEnumerator();
				while ( myEnumerator.MoveNext() )
				{
					if(myEnumerator.Key.ToString() == Const.DefaultLookupIdAttribute)
					{
						lookup=(Guid)column.ExtendedProperties[Const.DefaultLookupIdAttribute];
						controlType = GridColumnControl.ComboBox;
					}
					if(myEnumerator.Key.ToString() == Const.DefaultHideUIAttribute)
						visibleColumnInGrid=false;
					
				}
				if(visibleColumnInGrid)								
					tableStyle.GridColumnStyles.Add(CreateGridColumn(column,controlType,lookup));
			}
			return tableStyle;
		}


		private DataGridColumnStyle CreateGridColumn(	DataColumn column, 
			GridColumnControl type, 
			Guid lookup)
		{	
            
			DataGridColumnStyle result=null;
            
			if(type==GridColumnControl.TextBox)
			{

				result = new DataGridTextBoxColumn();
				result.HeaderText = column.Caption;
				result.MappingName = column.ColumnName;
				result.ReadOnly = column.ReadOnly;
				result.Width = 100;
				
			}
			else if(type==GridColumnControl.ComboBox)
			{
				result = new AsDataGridComboBoxColumn();
				result.HeaderText = column.Caption;
				result.MappingName = column.ColumnName;
				result.ReadOnly = column.ReadOnly;
				LoadComboData( (result as AsDataGridComboBoxColumn).AsCombo, lookup,column.AllowDBNull);
			}
			result.WidthChanged +=new EventHandler(result_WidthChanged);
			return result;
		}


		private void LoadComboData(ComboBox combo, Guid dataStructureId, bool allowDBNull)
		{
			DataSet comboData=new DataSet();

			if(	base.DesignMode || 
				this._dataService == null ||
				dataStructureId == Guid.Empty ||
				this._schemaVersionId == Guid.Empty)
				return;

			DataStructureQuery qry=new DataStructureQuery(dataStructureId,this._schemaVersionId);
			comboData=this._dataService.LoadDataSet(qry,null);

			if(	comboData !=null && 
				comboData.Tables[0] !=null && 
				comboData.Tables[0].Columns[0] !=null &&
				comboData.Tables[0].Columns[1] !=null)
				combo.ValueMember = comboData.Tables[0].Columns[Const.ValuelistIdField].ColumnName;
			combo.DisplayMember = comboData.Tables[0].Columns[Const.ValuelistTextField].ColumnName;
			comboData.Tables[0].Columns[Const.ValuelistIdField].AllowDBNull = true;

			//			if(allowDBNull)
			//			{
			DataRow nullRow = comboData.Tables[0].NewRow();
			nullRow[Const.ValuelistIdField] = DBNull.Value;
			nullRow[Const.ValuelistTextField] = "(prázdné)";
			comboData.Tables[0].Rows.Add(nullRow);
			//			}
			combo.DataSource = comboData.Tables[0];
		}





		public void SetDataBinding(object dataSource, string dataMember)
		{
			this.DataSource = dataSource;
			this.DataMember = dataMember;
		}

		private void dataGrid1_Navigate(object sender, System.Windows.Forms.NavigateEventArgs ne)
		{
			 
		}

		private void chkGrid_CheckedChanged(object sender, System.EventArgs e)
		{
			if(chkGrid.Checked)
			{
				//make visible
				//grid.Visible = true;
				grid.Enabled = true;
				grid.Top = 10;
				grid.Left = 10;
				grid.Dock = DockStyle.Fill;
				grid.BringToFront();
			}
			else
			{
				//hide
				//grid.Visible = false;
				grid.Enabled=false;
				grid.Dock = DockStyle.None;
				grid.Left = (grid.Width +20) * -1;
				grid.Top  = (grid.Height +20) * -1;
				grid.SendToBack();
			}
		
		}
		#region IContainerControl Members

		public bool ActivateControl(Control active)
		{
			if(this.Controls.Contains(active))
			{
				// Select the control and scroll the control into view if needed.
				active.Select();
				base.ScrollControlIntoView(active);
				base.ActiveControl = active;
				return true;
			}
			return false;

		}

		private void AsPanel_Load(object sender, System.EventArgs e)
		{
		
		}

		private void dbNav_Load(object sender, System.EventArgs e)
		{
		
		}

		public new Control ActiveControl
		{
			get
			{
				return base.ActiveControl;
			}

			set
			{
				// Make sure the control is a member of the ControlCollection.
				if(base.Controls.Contains(value))
				{
					base.ActiveControl = value;
				}
			}

		}

		#endregion

		#region ISupportInitialize Members

		public void BeginInit()
		{
			grid.BeginInit();
		}

		public void EndInit()
		{
			grid.EndInit();
		}

		#endregion


		public void CheckForZeroRowCount(CurrencyManager cm)
		{
			if(cm !=null && cm.Count == 0)
			{
				cm.AddNew();
				
			}

		}

	
		private void AsPanel_CurrentChanged(object sender, EventArgs e)
		{
			CurrencyManager cm = (sender as CurrencyManager);
			if(cm ==null || cm.Count == 0)
			{
				//if currency manager exists, and 
			
				CheckForZeroRowCount(cm);
				chkGrid.Enabled=false;
				dbNav.UpdateControls();
				return;
			}
			else if ( (cm.Current as DataRowView).IsNew)
			{
				(cm.Current as DataRowView).Row[Const.ValuelistIdField]= Guid.NewGuid();

				if((cm.Current as DataRowView).Row.Table.Columns.Contains("RecordCreated"))
					(cm.Current as DataRowView).Row["RecordCreated"]= DateTime.Now;

				chkGrid.Enabled=false;
				//(cm.List as DataView).AllowNew = false;
				//return;
			}
			else
				chkGrid.Enabled=true;
			//(cm.List as DataView).AllowNew = true;
			chkGrid.Enabled=true;
			CheckForZeroRowCount(cm);
			dbNav.UpdateControls();

		}
		

		

		private Color _backColor;
		private Color _backColorActive;
		private bool _changingColor = false;

		private void AsPanel_MouseEnter(object sender, System.EventArgs e)
		{
			//			_changingColor = true;
			//			this.BackColor = _backColorActive;
			//			_changingColor = false;
		}

		private void AsPanel_MouseLeave(object sender, System.EventArgs e)
		{
			//			_changingColor = true;
			//			this.BackColor = _backColor;
			//			_changingColor = false;
		}

		private void AsPanel_ControlAdded(object sender, ControlEventArgs e)
		{
			//e.Control.MouseEnter += new EventHandler(AsPanel_MouseEnter);
		}

		private void AsPanel_BackColorChanged(object sender, EventArgs e)
		{
			// Store the back color of this control
			if(!_changingColor)
			{
				_backColor = this.BackColor;
				_backColorActive = ColorActive(this.BackColor);
			}
		}

		private Color ColorActive(Color color)
		{
			//return Color.FromArgb(color.R +8, color.G +8, color.B +8);
			return this.BackColor;
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			//base.OnPaint (e);

			// rounded rectangle
			GraphicsPath path;
			
			Graphics graphics = e.Graphics;
			Rectangle rect = new Rectangle(0, 0, this.Width, this.Height);
			path = RoundedRect.CreatePath(rect, 7, 0, CornerType.All);
			SolidBrush brush = new SolidBrush(Color.Beige);
			graphics.FillPath(brush, path);

			// declare linear gradient brush for fill background of label
			LinearGradientBrush GBrush = new LinearGradientBrush(
				new Point(0, 0),
				new Point(this.Width, 0), Color.DarkKhaki, Color.LemonChiffon);
			
			rect = new Rectangle(0, 0, this.Width, 24);
			path = RoundedRect.CreatePath(rect, 7, 0, CornerType.Top);
			graphics.FillPath(GBrush, path);

			// draw text on label
			SolidBrush drawBrush = new SolidBrush(SystemColors.ActiveCaptionText);
			StringFormat sf = new StringFormat();
			// align with center
			sf.Alignment = StringAlignment.Near;
			// set rectangle bound text
			RectangleF rectF = new 
				RectangleF(5, 12-Font.Height/2, this.Width, 24);
			// output string
			Font font= new Font(this.Font, FontStyle.Bold);
			graphics.DrawString(this.PanelTitle, font, drawBrush, rectF, sf);
			
		}
		#region IAsDataServiceConsumer Members

		Guid _dataStructureId;

		public Guid DataStructureId
		{
			get
			{
				return _dataStructureId;
			}

			set
			{
				_dataStructureId=value;
			}

		}

		private Guid _schemaVersionId;

		public Guid DataStructureSchemaVersionId
		{
			get
			{
				return _schemaVersionId;
			}
			set
			{
				_schemaVersionId=value;
			}
		}

		private IDataService _dataService;

		public Origam.DA.IDataService DataService
		{
			get
			{
				return _dataService;
			}
			set
			{
				_dataService = value;
			}
		}

		#endregion


		private bool _insertTime=true;
		private void AsPanel_RowChanged(object sender, DataRowChangeEventArgs e)
		{
			if(e.Action == DataRowAction.Change && _insertTime)
			{
				_insertTime=false;
				e.Row["RecordUpdated"]= DateTime.Now;
				e.Row.EndEdit();
				_insertTime=true;
			}


			
					
		}

		private void result_WidthChanged(object sender, EventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("GRID COL WITH CHANGED");
		}

		private void AsPanel_RowDeleted(object sender, DataRowChangeEventArgs e)
		{
			if(e.Action == DataRowAction.Delete)
			{
				CheckForZeroRowCount(_bindingManager as CurrencyManager);
			}

		}

		private void AsPanel_RowDeleting(object sender, DataRowChangeEventArgs e)
		{
			if(e.Action == DataRowAction.Delete)
			{
				CheckForZeroRowCount(_bindingManager as CurrencyManager);
			}

		}
	}
}
