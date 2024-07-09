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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Origam.DA;
using Origam.Schema;
using Origam.DA.Service;
using Origam.UI;
using Origam.Schema.GuiModel;
using Origam.Schema.EntityModel;
using Origam.Schema.LookupModel;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for AsDropDown.
	/// </summary>
	[ToolboxBitmap(typeof(AsDropDown))]
	public class AsDropDown : BaseDropDownControl, ILookupControl,
		IOrigamMetadataConsumer, INotifyPropertyChanged
	{
		private const int WM_KEYUP = 0x101;

		private IPersistenceService _persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		private DataTable _currentTable;
		private bool resetParameterMappings = true;
        private readonly ToolStripMenuItem mnuEdit;
        private readonly ToolStripMenuItem mnuEditList;
        private readonly ToolStripMenuItem mnuDelete;
        private ToolStripMenuItem mnuRefresh;

		public AsDropDown()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

			this.BindingContextChanged += AsDropDown_BindingContextChanged;
			this.DataBindings.CollectionChanged += DataBindings_CollectionChanged;

			mnuRefresh = new ToolStripMenuItem(ResourceUtils.GetString("MenuRefresh"));
			mnuRefresh.Click += refreshMenu_Click;
            mnuRefresh.Image = Workbench.Images.Refresh;

            mnuEdit = new ToolStripMenuItem(ResourceUtils.GetString("MenuEdit"));
			mnuEdit.Click += mnuEdit_Click;
            mnuEdit.Image = Workbench.Images.Edit;

            mnuEditList = new ToolStripMenuItem(ResourceUtils.GetString("MenuEditList"));
			mnuEditList.Click += btnOpenList_Click;
            mnuEditList.Image = Workbench.Images.Open;

            mnuDelete = new ToolStripMenuItem(ResourceUtils.GetString("MenuDelete"));
			mnuDelete.ShortcutKeys = Keys.Delete;
			mnuDelete.ShowShortcutKeys = true;
			mnuDelete.Click += mnuDelete_Click;
            mnuDelete.Image = Workbench.Images.Delete;

            this.EditControl.ContextMenuStrip = new ContextMenuStrip();
            this.EditControl.ContextMenuStrip.Items.AddRange(new ToolStripMenuItem[] { mnuRefresh, mnuEditList, mnuEdit, mnuDelete });
            this.EditControl.ContextMenuStrip.Opening += ContextMenuStrip_Opening;
			this.EditControl.CursorDownPressed += txtEdit_CursorDownPressed;
			this.EditControl.CursorUpPressed += txtEdit_CursorUpPressed;
			this.EditControl.MouseHover += txtEdit_MouseHover;
			this.EditControl.MouseEnter += txtEdit_MouseEnter;
			this.EditControl.MouseMove += txtEdit_MouseMove;
			this.EditControl.MouseDown += txtEdit_MouseDown;
			this.EditControl.KeyUp += txtEdit_KeyUp;

			this.OpenListButton.Click += this.btnOpenList_Click;
			this.OpenListButton.Enter += this.btnOpenList_Enter;

			this.readOnlyChanged += AsDropDown_readOnlyChanged;
			this.PopupHelper.PopupClosed += popupHelper_PopupClosed;
			
			
		}

		#region Overrides
		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				this.DataBindings.CollectionChanged -= DataBindings_CollectionChanged;
				if(this.EditControl.ContextMenu != null)
				{
					this.EditControl.ContextMenuStrip.Opening -= ContextMenuStrip_Opening;
					ContextMenu cm = this.EditControl.ContextMenu;
					cm.Dispose();
				}

				this.EditControl.CursorDownPressed -= txtEdit_CursorDownPressed;
				this.EditControl.CursorUpPressed -= txtEdit_CursorUpPressed;
				this.EditControl.MouseHover -= txtEdit_MouseHover;
				this.EditControl.MouseEnter -= txtEdit_MouseEnter;
				this.EditControl.MouseMove -= txtEdit_MouseMove;
				this.EditControl.MouseDown -= txtEdit_MouseDown;
				this.EditControl.KeyUp -= txtEdit_KeyUp;
				this.OpenListButton.Click -= this.btnOpenList_Click;
				this.OpenListButton.Enter -= this.btnOpenList_Enter;
				this.readOnlyChanged -= AsDropDown_readOnlyChanged;
				
				if(this.PopupHelper != null)
				{
					this.PopupHelper.PopupClosed -= popupHelper_PopupClosed; 
				}
				
				this.BindingContextChanged -= AsDropDown_BindingContextChanged;

				_lookupList = null;
				_origamMetadata = null;
				_persistence = null;

				if(_currentTable != null)
				{
					_currentTable.ColumnChanging -= _currentTable_ColumnChanging;
					_currentTable = null;
				}
			}
			base.Dispose( disposing );
		}

		public override string DefaultBindableProperty => "LookupValue";

		private object _selectedValue = null;
		public override object SelectedValue
		{
			get
			{
				if(_selectedValue == null)
				{
					return DBNull.Value;
				}
				else
				{
					return _selectedValue;
				}
			}
			set
			{
				if(value == null)
				{
					_selectedValue = DBNull.Value;
				}
				else
				{
					_selectedValue = value;
				}

				this.LookupValue = _selectedValue;
			}
		}

		#endregion

		#region Properties
		[TypeConverter(typeof(DataLookupConverter))]
		[RefreshProperties(RefreshProperties.All)]
		public AbstractDataLookup DataLookup
		{
			get => (AbstractDataLookup)_persistence
				.SchemaProvider
				.RetrieveInstance(
					typeof(AbstractDataLookup), new ModelElementKey(this.LookupId));
			set
			{
				if(value == null)
				{
					this.LookupId = Guid.Empty;
				}
				else
				{
					// if same as before, no action is needed
					if(this.LookupId == (Guid)value.PrimaryKey["Id"])
					{
						return;
					}
					
					this.LookupId = (Guid)value.PrimaryKey["Id"];
				}
				resetParameterMappings = true;
			}
		}

		private bool _showUniqueValues = false;
		public bool ShowUniqueValues
		{
			get => _showUniqueValues;
			set
			{
				_showUniqueValues = value;

				if(value) this.CacheList = false;
			}
		}

        public string SearchText => EditControl.Text;

		public bool SuppressEmptyColumns { get; set; } = false;

		#endregion

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(AsDropDown));

			this.SuspendLayout();
			// 
			// AsDropDown
			// 
			this.ForeColor = System.Drawing.Color.White;
			this.Name = "AsDropDown";
			this.Size = new System.Drawing.Size(168, 20);
			this.ResumeLayout(false);

		}
		#endregion

		#region ILookupControl Events
		public event EventHandler lookupValueChanged;
		public event EventHandler LookupDisplayTextRequested;
		public event EventHandler LookupListRefreshRequested;
		public event EventHandler LookupShowSourceListRequested;
		public event EventHandler LookupEditSourceRecordRequested;
		public event EventHandler LookupValueChangingByUser;

		protected virtual void OnLookupValueChanged(EventArgs e)
		{
			lookupValueChanged?.Invoke(this, e);
		}

		protected virtual void OnLookupDisplayTextRequested(EventArgs e)
		{
			LookupDisplayTextRequested?.Invoke(this, e);
		}

		protected virtual void OnLookupListRefreshRequested(EventArgs e)
		{
			try
			{
				if(! (this.Parent is FilterPanel))
				{
					SetEntityAndField(CurrentRow, ColumnName);
				}

				if (this.LookupListRefreshRequested != null)
				{
					// first we set the actual parameters, so we get correct list data
					if(this.ParameterMappings.Count > 0)
					{
						if(this.Parent is FilterPanel) return;
					}

					this.LookupListRefreshRequested(this, e);
				}
			}
			catch (Exception ex)
			{
				AsMessageBox.ShowError(this.FindForm(), ex.Message, "Chyba pøi obnovování seznamu '" + this.Caption + "'", ex);
			}
		}

		private string TableName
		{
			get
			{
				if(this.Parent is AsDataGrid)
				{
					AsDataGrid grid = this.Parent as AsDataGrid;

					return FormTools.FindTableByDataMember(grid.DataSource as DataSet, grid.DataMember);
				}

				return null;
			}
		}

		public DataRow CurrentRow => 
			DataBindingTools.CurrentRow(this, DefaultBindableProperty);

		protected virtual void OnLookupShowSourceListRequested(EventArgs e)
		{
			LookupShowSourceListRequested?.Invoke(this, e);
		}

		protected virtual void OnLookupEditSourceRecordRequested(EventArgs e)
		{
			LookupEditSourceRecordRequested?.Invoke(this, e);
		}

		protected virtual void OnLookupValueChangingByUser(EventArgs e)
		{
			LookupValueChangingByUser?.Invoke(this, e);
		}
		#endregion

		#region ILookupControl Members

		public bool CacheList { get; set; } = true;

		public object OriginalLookupValue
		{
			get
			{
				DataRow row = CurrentRow;
				if(row == null) return null;

				DataRowVersion version;
				switch(row.RowState)
				{
					case DataRowState.Detached:
						return DBNull.Value;
					case DataRowState.Added:
						return DBNull.Value;
					case DataRowState.Modified:
						version = DataRowVersion.Original;
						break;
					default:
						version = DataRowVersion.Current;
						break;
				}

				return row[ColumnName, version];
			}
		}

		public object LookupValue
		{
			get => this.SelectedValue;
			set
			{
				_selectedValue = value;

				OnLookupDisplayTextRequested(EventArgs.Empty);
				OnLookupValueChanged(EventArgs.Empty);
                if (PropertyChanged != null)
                {
                    PropertyChanged(this, new PropertyChangedEventArgs("LookupValue"));
                }

				if(this.SelectedValue != DBNull.Value && LookupCanEditSourceRecord)
				{
					this.EditControl.Font = new Font(this.EditControl.Font, FontStyle.Underline);
					this.EditControl.ForeColor = OrigamColorScheme.LinkColor;
				}
				else
				{
					this.EditControl.Font = new Font(this.EditControl.Font, FontStyle.Regular);
					this.EditControl.ForeColor = SystemColors.ControlText;
				}
			}
		}

		[Browsable(false)]
		public string LookupDisplayText
		{
			get => this.DisplayText;
			set
			{
				if(! _typing)
				{
					this.DisplayText = value;
				}
			}
		}

		private bool _lookupShowEditButton = false;
		public bool LookupShowEditButton
		{
			get => _lookupShowEditButton;
			set
			{
				if(this.ReadOnly)
				{
					_lookupShowEditButton = false;
					this.OpenListButton.Visible = false;
				}
				else
				{
					_lookupShowEditButton = value;
					this.OpenListButton.Visible = value;
				}
			}
		}

		public string LookupListValueMember { get; set; }

		private string _lookupListDisplayMember;
		public string LookupListDisplayMember
		{
			get => _lookupListDisplayMember;
			set
			{
				_lookupListDisplayMember = value;
				SortDataSource();
			}
		}

		public string LookupListTreeParentMember { get; set; }

		[Browsable(false)]
		public Guid LookupId { get; set; }

		[Browsable(false)]
		private Guid _entityId;
		public Guid EntityId => _entityId;

		[Browsable(false)]
		private Guid _valueFieldId;
		public Guid ValueFieldId => _valueFieldId;

		[Browsable(false)]
		public string ColumnName { get; set; }

		private DataView _lookupList;
	
		private bool _typing = false;

		[Browsable(false)]
		public DataView LookupList
		{
			get
			{
				if(_lookupList == null | CacheList == false)
				{
					OnLookupListRefreshRequested(EventArgs.Empty);
				}

				if(_lookupList == null)
				{
					throw new Exception(ResourceUtils.GetString("ErrorNoDataInList"));
				}

				return _lookupList;
			}
			set
			{
				_lookupList = value;
				SortDataSource();
			}
		}

		private Hashtable _lookupListParameters = new Hashtable();
		public Hashtable LookupListParameters => _lookupListParameters;

		#endregion

		#region Methods
		private void SortDataSource()
		{
			if(_lookupListDisplayMember == "" 
				| _lookupListDisplayMember == null
				| _lookupList == null)
			{
				return;
			}

			string firstColumn = this.LookupListDisplayMember.Split(";".ToCharArray())[0];
			_lookupList.Sort = firstColumn;
		}

		public override IDropDownPart CreatePopup()
		{
			ILookupDropDownPart popup;

			if(this.ParameterMappings.Count > 0)
			{
				// we always call for refresh if this is parametrized lookup
				OnLookupListRefreshRequested(EventArgs.Empty);
			}
			else
			{
				this.LookupList.RowFilter = "";
			}

			if(_lookupList == null) 
			{
				DataView dummy = LookupList;
			}

			if(LookupListTreeParentMember == "" | LookupListTreeParentMember == null | _lookupList.RowFilter.Length > 0)
			{
				popup = new DropDownGrid();
				popup.Width = this.Width;
			}
			else
			{
				popup = new DropDownTree();
				popup.ParentMember = LookupListTreeParentMember;
			}

			popup.DropDownControl = this;

			popup.ValueMember = this.LookupListValueMember;
			popup.DisplayMember = this.LookupListDisplayMember;

			popup.DataSource = _lookupList;


			return popup;
		}

		private bool _itemsLoaded = false;
		private void ClearMappingItems()
		{
			try
			{
				if(!_itemsLoaded)
					return;

				var col = _origamMetadata.ChildItemsByType<ColumnParameterMapping>(ColumnParameterMapping.CategoryConst).ToList();

				foreach(ColumnParameterMapping mapping in col)
				{
					mapping.IsDeleted = true;
				}
			}
			catch(Exception ex)
			{
				System.Diagnostics.Debug.WriteLine("AsDropDown:ERROR=>" + ex.ToString());
			}
		}

		public void CreateMappingItemsCollection()
		{
			if(!_itemsLoaded)
				return;

			if(this.DataLookup != null)
			{
				var _dataServiceAgent = ServiceManager.Services.GetService<IBusinessServicesService>()
					.GetAgent("DataService", null, null);
				var parameters = _dataServiceAgent.ExpectedParameterNames(this.DataLookup, "LoadData", "Parameters");
				foreach (string parameterName in parameters)
				{
					ColumnParameterMapping mapping = _origamMetadata
						.NewItem<ColumnParameterMapping>(
							_origamMetadata.SchemaExtensionId, null);
					mapping.Name = parameterName;
				}
				foreach (SchemaItemParameter schemaItemParameter in DataLookup
					         .ListDataStructure.Parameters)
				{
					var mapping =
						_origamMetadata.NewItem<ColumnParameterMapping>(
							_origamMetadata.SchemaExtensionId, null);
					mapping.Name = schemaItemParameter.Name;
				}
			}

			//Refill Parameter collection (and dictionary)
			FillParameterCache(this._origamMetadata as ControlSetItem);
		}

		private void FillParameterCache(ControlSetItem controlItem)
		{
			if( controlItem ==null)
				return;
			
			parameterMappings.Clear();
			
			foreach(var mapInfo in controlItem.ChildItemsByType<ColumnParameterMapping>(ColumnParameterMapping.CategoryConst))
			{
				if(! mapInfo.IsDeleted)
				{
					parameterMappings.Add(mapInfo);
				}
			}
		}
		#endregion

		#region Event Handlers
		private void txtEdit_KeyUp(object sender, KeyEventArgs e)
		{
			if(this.ReadOnly) return;

			bool pasting = false;

			if(e.Control & !e.Shift & e.KeyCode == Keys.C)
			{
				// Ctrl+C = Copy
				Clipboard.SetDataObject(this.EditControl.Text);
				return;
			}

			if(e.Control & !e.Shift & e.KeyCode == Keys.V)
			{
				pasting = true;
			}

			// show the dropdown part on Alt+Down
			if(e.Alt & e.KeyCode == Keys.Down)
			{
				DropDown();
				return;
			}

			if(e.Alt | (e.Control & e.KeyCode != Keys.V))
			{
				return;
			}

			//'Allow select keys without Autocompleting
			switch (e.KeyCode)
			{
				case Keys.F1:
				case Keys.F2:
				case Keys.F3:
				case Keys.F4:
				case Keys.F5:
				case Keys.F6:
				case Keys.F7:
				case Keys.F8:
				case Keys.F9:
				case Keys.F10:
				case Keys.F11:
				case Keys.F12:
				case Keys.F13:
				case Keys.F14:
				case Keys.F15:
				case Keys.F16:
				case Keys.F17:
				case Keys.F18:
				case Keys.F19:
				case Keys.F20:
				case Keys.F21:
				case Keys.F22:
				case Keys.F23:
				case Keys.F24:
				case Keys.Apps:
				case Keys.RWin:
				case Keys.LWin:
				case Keys.CapsLock:
				case Keys.NumLock:
				case Keys.Insert:
				case Keys.PrintScreen:
				case Keys.Menu:
				case Keys.Alt:
				case Keys.Control:
				case Keys.ControlKey:
				case Keys.Shift:
				case Keys.ShiftKey:
				case Keys.Left:
				case Keys.Right:
					return;

				case Keys.Tab:
					if(e.Shift & this.DroppedDown)
					{
						PopupHelper.ClosePopup();
					}
					return;

				case Keys.PageDown:
				case Keys.PageUp:
				case Keys.Home:
				case Keys.End:
				case Keys.Escape:
					if(this.DroppedDown)
					{
						PopupHelper.ClosePopup();
					}
					return;

				case Keys.Return:
					if(this.DroppedDown)
					{
						Popup.SelectItem();
					}
					return;

				case Keys.Up:
					if(this.DroppedDown)
					{
						Popup.MoveUp();
						return;
					}
					break;

				case Keys.Down:
					if(this.DroppedDown)
					{
						Popup.MoveDown();
						return;
					}
					break;

				case Keys.Delete:
					DeleteSelectedValue();
					return;
			}

			_typing = true;

			try
			{
				this.DropDown();

				this.EditControl.Font = new Font(this.EditControl.Font, FontStyle.Regular);
				this.EditControl.ForeColor = SystemColors.ControlText;

				string sTypedText = this.EditControl.Text;
				try
				{
                    FilterClientSide(sTypedText);
					// select the first found item
					if(_lookupList.Count > 0)
					{
						Popup.SelectedValue = _lookupList[0].Row[this.LookupListValueMember];
					
						// if pasting action resulted in an exact match, we close the popup
						if(_lookupList.Count == 1 & pasting)
						{
							PopupHelper.ClosePopup();
							// move the caret to the last letter, so the user does not overwrite what he has just typed in

							// removed, makes only sense if exact match is handled also with manual input, not only by pasting
							//						txtEdit.SelectionStart = txtEdit.Text.Length;
							//						txtEdit.SelectionLength = 0;
						}
					
					}
				} 
				catch {}
			}
			finally
			{
				// move the caret to the last letter, so the user can continue editing
				this.EditControl.SelectionStart = this.EditControl.Text.Length;
				this.EditControl.SelectionLength = 0;

				_typing = false;
			}
		}

        private void FilterClientSide(string sTypedText)
        {
            string rowFilter = "";
            int i = 0;
            foreach (DataColumn col in _lookupList.Table.Columns)
            {
                if (col.DataType == typeof(string) || col.DataType == typeof(Guid))
                {
                    string oper = "like";
                    string wildcard = "%";
                    if (col.DataType == typeof(Guid))
                    {
                        oper = "=";
                        wildcard = "";
                    }

                    if (i > 0) rowFilter += " OR ";
                    rowFilter += col.ColumnName + " " + oper + " '" + sTypedText.Replace("'", "''") + wildcard + "'";
                    i++;
                }
            }
            _lookupList.RowFilter = rowFilter;
        }

		private void btnOpenList_Enter(object sender, EventArgs e)
		{
			this.EditControl.Focus();
		}

		private void _currentTable_ColumnChanging(object sender, DataColumnChangeEventArgs e)
		{
			if(!(this.FindForm() is AsForm form)) return;
			if(form.FormGenerator.IgnoreDataChanges) return;
            OrigamDataRow row = e.Row as OrigamDataRow;
            if (!row.IsColumnWithValidChange(e.Column)) return;

			try
			{
				if(this.CurrentRow == null) return;
			}
			catch
			{
				return;
			}

			if(! e.Row.Equals(this.CurrentRow)) return;
			// return if nothing has changed
			if(e.Row[e.Column].Equals(e.ProposedValue)) return;

			foreach(ColumnParameterMapping mapping in this.ParameterMappings)
			{
				if(mapping.ColumnName == e.Column.ColumnName && mapping.ColumnName != this.ColumnName)
				{
					// column on which we are dependent has changed, so we clear
					if(this.LookupValue != DBNull.Value)
					{
						this.LookupValue = DBNull.Value;
					}
					return;
				}
			}

		}

		private void AsDropDown_BindingContextChanged(object sender, EventArgs e)
		{
			UpdateBindings();
		}

		private void UpdateBindings()
		{
			Binding binding = this.DataBindings[this.DefaultBindableProperty];
			if(binding == null) return;

			DataSet dataSource = binding.DataSource as DataSet;

			DataTable table = dataSource.Tables[FormTools.FindTableByDataMember(dataSource, binding.BindingMemberInfo.BindingPath)];
			DataColumn column = table.Columns[binding.BindingMemberInfo.BindingField];
			
			if(column != null)
			{
				this.ColumnName = column.ColumnName;

				if(table != _currentTable)
				{
					if(_currentTable != null)
					{
						_currentTable.ColumnChanging -= _currentTable_ColumnChanging;
					}
					_currentTable = table;
					_currentTable.ColumnChanging += _currentTable_ColumnChanging;
				}
			}
		}

		private void SetEntityAndField(DataRow row, string columnName)
		{
			if(columnName == null)
			{
				throw new InvalidOperationException(ResourceUtils.GetString("ErrorDropDownNotBound"));
			}

			if(row == null)
			{
				throw new InvalidOperationException(ResourceUtils.GetString("ErrorRecordNeeded0") 
					+ Environment.NewLine 
					+  ResourceUtils.GetString("ErrorRecordNeeded1"));
			}
			else
			{
				_entityId = (Guid)row.Table.ExtendedProperties["EntityId"];
				_valueFieldId = (Guid)row.Table.Columns[columnName].ExtendedProperties["Id"];
			}

			DataColumn column = row.Table.Columns[columnName];
			if(column.ExtendedProperties.Contains("IsState") && (bool)column.ExtendedProperties["IsState"] == true)
			{
				this.CacheList = false;
			}
		}

		private void refreshMenu_Click(object sender, EventArgs e)
		{
			OnLookupListRefreshRequested(EventArgs.Empty);

			ILookupDropDownPart popup = this.Popup as ILookupDropDownPart;

			if(popup != null & this.DroppedDown)
			{
				popup.DataSource = this.LookupList;
				popup.SelectedValue = this.LookupValue;
			}
		}
		#endregion

		#region Popup Event Handlers
		private void popupHelper_PopupClosed(object sender, PopupClosedEventArgs e)
		{
			if(e.Popup != null)
			{
				if(!(e.Popup as IDropDownPart).Canceled)
				{

					this.LookupDisplayText = (e.Popup as IDropDownPart).SelectedText;
			
					OnLookupValueChangingByUser(EventArgs.Empty);
					this.LookupValue = (e.Popup as IDropDownPart).SelectedValue;
				}
			}
			this.DroppedDown = false;
			this.EditControl.IgnoreCursorDown = false;
			this.EditControl.SelectAll();
		}

		#endregion

		#region IColumnParameterMappingConsumer Members

		ColumnParameterMappingCollection parameterMappings = new ColumnParameterMappingCollection();
		[TypeConverter(typeof(ColumnParameterMappingCollectionConverter))]
		public ColumnParameterMappingCollection ParameterMappings
		{
			get
			{
				if(this.DesignMode && resetParameterMappings)
				{
					Hashtable oldMappings = new Hashtable();
					foreach(ColumnParameterMapping mapping in parameterMappings)
					{
						oldMappings.Add(mapping.Name, mapping.ColumnName);
					}

					ClearMappingItems();
					CreateMappingItemsCollection();

					foreach(ColumnParameterMapping mapping in parameterMappings)
					{
						if(oldMappings.Contains(mapping.Name))
						{
							mapping.ColumnName = (string)oldMappings[mapping.Name];
						}
					}
					resetParameterMappings = false;
				}

				return parameterMappings;
			}
		}

		public Hashtable ParameterMappingsHashtable
		{
			get
			{
				Hashtable result = new Hashtable(this.ParameterMappings.Count);
				foreach(ColumnParameterMapping mapping in this.ParameterMappings)
				{
					result.Add(mapping.Name, mapping.ColumnName);
				}

				return result;
			}
		}

		#endregion

		#region IOrigamMetadataConsumer Members

		private ISchemaItem _origamMetadata;
		public ISchemaItem OrigamMetadata
		{
			get
			{
				if(_origamMetadata == null) return null;

				try
				{
					return _origamMetadata.PersistenceProvider.RetrieveInstance(_origamMetadata.GetType(), _origamMetadata.PrimaryKey, true) as ISchemaItem;
				}
				catch
				{
					return _origamMetadata;
				}
			}
			set
			{
				_origamMetadata = value;
				_itemsLoaded = true;

				FillParameterCache(_origamMetadata as ControlSetItem);
			}
		}

		#endregion


		private void txtEdit_CursorDownPressed(object sender, EventArgs e)
		{
			if(this.DroppedDown) Popup.MoveDown();
		}

		private void txtEdit_CursorUpPressed(object sender, EventArgs e)
		{
			if(this.DroppedDown) Popup.MoveUp();
		}

		private void txtEdit_MouseHover(object sender, EventArgs e)
		{
			this.OnMouseHover(e);
		}

		private void txtEdit_MouseEnter(object sender, EventArgs e)
		{
			this.OnMouseEnter(e);
		}

		private void txtEdit_MouseMove(object sender, MouseEventArgs e)
		{
			if(PreparedForEditingSourceRecord)
			{
				this.EditControl.Cursor = Cursors.Hand;
			}
			else
			{
				this.EditControl.Cursor = Cursors.IBeam;
			}

			this.OnMouseMove(e);
		}

		private void btnOpenList_Click(object sender, EventArgs e)
		{
			this.OnLookupShowSourceListRequested(EventArgs.Empty);
		}

		private void mnuDelete_Click(object sender, EventArgs e)
		{
			DeleteSelectedValue();
		}

		private void DeleteSelectedValue()
		{
			if(! this.ReadOnly)
			{
				OnLookupValueChangingByUser(EventArgs.Empty);
				this.LookupValue = DBNull.Value;
			}
		}

        void ContextMenuStrip_Opening(object sender, CancelEventArgs e)
        {
			mnuEdit.Visible = LookupCanEditSourceRecord;
			mnuEdit.Enabled = this.LookupValue != DBNull.Value;
			mnuDelete.Visible = ! this.ReadOnly;
			mnuEditList.Visible = this.OpenListButton.Visible;
		}

		private void DataBindings_CollectionChanged(object sender, CollectionChangeEventArgs e)
		{
			UpdateBindings();
		}

		private static bool IsControlPressed => (ModifierKeys & Keys.Control) == Keys.Control;

		public bool LookupCanEditSourceRecord { get; set; } = false;

		private bool PreparedForEditingSourceRecord 
			=> this.LookupCanEditSourceRecord &&
			   IsControlPressed && 
			   this.LookupValue != DBNull.Value;

		private void mnuEdit_Click(object sender, EventArgs e)
		{
			OnLookupEditSourceRecordRequested(EventArgs.Empty);
		}

		private void txtEdit_MouseDown(object sender, MouseEventArgs e)
		{
			if(PreparedForEditingSourceRecord)
			{
				OnLookupEditSourceRecordRequested(EventArgs.Empty);
			}
		}

		private void AsDropDown_readOnlyChanged(object sender, EventArgs e)
		{
			if(ReadOnly)
			{
				this.LookupShowEditButton = false;
			}
		}

        public event PropertyChangedEventHandler PropertyChanged;
	}
}
