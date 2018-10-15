using System;
using System.Threading;
using System.Data;
using System.Drawing;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.DA;
using Origam.DA.Service;


namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for AsCombo.
	/// </summary>
	[ToolboxBitmap(typeof(XAsCombo))]
	public class XAsCombo : ComboBox, IAsCaptionControl, IAsDataServiceComboConsumer, IAsControl
	{
		public event ComboRefresh ComboRefreshWanted;
		public event EventHandler EditingStarted;

		//		private CommandBarContextMenu _contextMenu = new CommandBarContextMenu();
		private ContextMenu _contextMenu = new ContextMenu();
//		System.Timers.Timer _timer = new System.Timers.Timer(100);
		Label _captionLabel = new Label();
		
		private Guid _dataStructureId;

		public XAsCombo()
		{
			this.DropDown += new EventHandler(AsCombo_DropDown);
			this.KeyUp += new KeyEventHandler(AsCombo_KeyUp);
			this.KeyDown += new KeyEventHandler(AsCombo_KeyDown);
			this.Leave += new EventHandler(AsCombo_Leave);
			ConfigContextMenu(_contextMenu);
			this.ContextMenu = _contextMenu;
			
//			_timer.Elapsed += new System.Timers.ElapsedEventHandler(_timer_Elapsed);
//			_timer.Start();
		}


		#region Handling base events
		protected override void InitLayout()
		{
			base.InitLayout ();
			//this.DropDownStyle = ComboBoxStyle.DropDown;

			PaintCaption();

			this.Parent.Controls.Add(_captionLabel);
			
			ResetCaption();

			this.DataBindings.CollectionChanged += new CollectionChangeEventHandler(DataBindings_CollectionChanged);
			//this.DropDown += new EventHandler(AsCombo_DropDown);
		}

		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
//				_timer.Dispose();

				this.DataSource = null;
				this.DataService = null;

				if(this.Parent != null && this.Parent.Controls.Contains(_captionLabel) && _captionLabel.IsDisposed == false)
				{
					this.Parent.Controls.Remove(_captionLabel);
				}
			}

			base.Dispose (disposing);
		}

		protected override void OnMove(EventArgs e)
		{
			base.OnMove (e);

			PaintCaption();
		}
		#endregion

		#region Properties

		private bool _readOnly = false;

		[Description("Read only"), Category("Behavior"), DefaultValue(false)]
		public bool ReadOnly
		{
			get 
			{
				return _readOnly;
			}
			set
			{
				if(_readOnly != value)
				{
					_readOnly = value;
				}

				if(_readOnly)
				{
					this.BackColor = SystemColors.Control;
					this.DropDownStyle = ComboBoxStyle.Simple;
				}
				else
				{
					this.BackColor = SystemColors.Window;
					this.DropDownStyle = ComboBoxStyle.DropDown;
				}
			}
		}


		private string _rootMember = String.Empty;
		[Category("Origam")]
		public string RootMember
		{
			get	{return _rootMember;}
			set {_rootMember =value;}
		}

		[Category("Origam")]
		public bool DefaultValueDisplayMembers
		{
			get
			{
				if(this.DisplayMember.Length > 1 || this.ValueMember.Length > 1)
					return false;
				else
					return true;
			
			}
		}


		private int _captionLength = 100;
		[Category("Origam")]
		public int CaptionLength
		{
			get
			{
				return _captionLength ;
			}
			set
			{
				_captionLength = value;
				PaintCaption();
			}
		}



		private int RefreshCombo(object val)
		{
			int i=-1;
			foreach(DataRowView dv in this.Items)
			{
				string colName = FormGenerator.GetColumnNameFromDisplayMember(this.ValueMember);

				if( dv.Row[colName].ToString() ==val.ToString() )
				{
					i=this.Items.IndexOf(dv);
					this.SelectedIndex =i;
					this.RefreshItem(i);
					break;
				}
			}
			return i;
		}

		#endregion

		#region private methods
		private void ConfigContextMenu(ContextMenu menu)
		{
			MenuItem item = new MenuItem ("Obnovit");
			menu.MenuItems.Add(item);
			item.Click += new EventHandler(refreshMenuItem_Click);
		}

		private void PaintCaption()
		{
			this._captionLabel.Width = this.CaptionLength;
			this._captionLabel.BackColor = Color.Transparent;

			switch(this.CaptionPosition)
			{
				case CaptionPosition.Left:
					this._captionLabel.Visible = true;
					this._captionLabel.Top = this.Top;
					this._captionLabel.Left = this.Left - this.CaptionLength;
					break;

				case CaptionPosition.Right:
					this._captionLabel.Visible = true;
					this._captionLabel.Top = this.Top;
					this._captionLabel.Left = this.Right;
					break;
				
				case CaptionPosition.Top:
					this._captionLabel.Visible = true;
					this._captionLabel.Top = this.Top - this._captionLabel.Height;
					this._captionLabel.Left = this.Left;
					break;
				
				case CaptionPosition.Bottom:
					this._captionLabel.Visible = true;
					this._captionLabel.Top = this.Top + this.Height;
					this._captionLabel.Left = this.Left;
					break;
				
				case CaptionPosition.None:
					this._captionLabel.Visible = false;
					break;
			}
		}

		private void ResetCaption()
		{
			if(this.Caption == "")
			{
				foreach(Binding binding in this.DataBindings)
				{
					if(binding.PropertyName == this.DefaultBindableProperty)
					{
						try
						{
							this._captionLabel.Text = ColumnCaption(binding);
						}
						catch
						{
							this._captionLabel.Text = "????";
						}
					}
				}
			}
		}

		private string TableName(DataSet ds, string dataMember)
		{
			// In case that dataMember is a path through relations, we find the last table
			// so we can take a caption out of it
			string tableName = "";
			if(dataMember.IndexOf(".") > 0)
			{
				string[] path = dataMember.Split(".".ToCharArray());
				DataTable table = ds.Tables[path[0]];
				for(int i = 1; i < path.Length - 1; i++)
				{
					table = table.ChildRelations[path[i]].ChildTable;
				}

				if(table != null)
				{
					tableName = table.TableName;
				}
			}
			else
				tableName = dataMember;

			return tableName;
		}

		private string ColumnCaption(Binding binding)
		{
			if(binding.DataSource is DataSet)
			{
				DataSet dataset = binding.DataSource as DataSet;
				// Get Table
				DataTable table = dataset.Tables[TableName(dataset, binding.BindingMemberInfo.BindingMember)];

				if(table != null)
				{
					if(table.Columns.Contains(binding.BindingMemberInfo.BindingField))
					{
						return table.Columns[binding.BindingMemberInfo.BindingField].Caption;
					}
				}
			}

			return binding.BindingMemberInfo.BindingField;
		}

		private void DataBindings_CollectionChanged(object sender, CollectionChangeEventArgs e)
		{
			if(this.Caption == "")
			{
				if(e.Element != null)
				{
					if((e.Element as Binding).PropertyName == this.DefaultBindableProperty)
					{
						if(e.Action == CollectionChangeAction.Remove)
						{
							this._captionLabel.Text = "";
						}
						else
						{
							try
							{
								this._captionLabel.Text = ColumnCaption((e.Element as Binding));
							}
							catch
							{
								this._captionLabel.Text = "????";
							}
						}
					}
				}
			}
		}

		#endregion

		#region IAsCaptionControl Members

		int _gridColumnWidth;
		[Category("Origam")]
		[DefaultValue(100)] 
		public int GridColumnWidth
		{
			get
			{
				return _gridColumnWidth;
			}
			set
			{
				_gridColumnWidth = value;
			}
		}




		string _gridColumnCaption = "";
		[Category("Origam")]
		public string GridColumnCaption
		{
			get
			{
				return _gridColumnCaption;
			}
			set
			{
				_gridColumnCaption = value;
			}
		}



		string _caption = "";
		[Category("Origam")]
		public string Caption
		{
			get
			{
				return _caption;
			}
			set
			{
				_caption = value;
				this._captionLabel.Text = _caption;
				ResetCaption();
			}
		}

		CaptionPosition _captionPosition = CaptionPosition.Left;
		[Category("Origam")]
		public CaptionPosition CaptionPosition
		{
			get
			{
				return _captionPosition;
			} 
			set
			{
				_captionPosition = value;
				PaintCaption();
			}
		}
		#endregion

		#region IAsDataServiceComboConsumer Members

		[Category("Origam")]
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

		private Guid _dataStructureFilterSetId;
		[Category("Origam")]
		public Guid DataStructureFilterSetId
		{
			get
			{
				return _dataStructureFilterSetId;
			}
			set
			{
				_dataStructureFilterSetId=value;
			
			}
		}

		private Guid _dataStructureFilterSetSchemaVersionId;
		[Category("Origam")]
		public Guid DataStructureFilterSetSchemaVersionId
		{
			get
			{
				return _dataStructureFilterSetSchemaVersionId;
			}
			set
			{
				_dataStructureFilterSetSchemaVersionId=value;
			
			}
		}

		private int _dataContextIdentifier;

		[Category("Origam")]
		public int DataContextIdentifier
		{
			get
			{
				return _dataContextIdentifier;
			}
			set
			{
				_dataContextIdentifier=value;
			}
		}


		private Guid _dataStructureSchemaVersionId;
		[Browsable(false)]
		public Guid DataStructureSchemaVersionId
		{
			get
			{
				return _dataStructureSchemaVersionId;
			}
			set
			{
				_dataStructureSchemaVersionId=value;
			}
		}

		private IDataService _dataService;
		[Browsable(false)]
		public IDataService DataService
		{
			get{return _dataService;}
			set
			{
				_dataService=value;
			}
		}
		#endregion

		#region IAsControl Members

		public string DefaultBindableProperty
		{
			get
			{
				return "SelectedValue";
			}
		}

		#endregion

		private void refreshMenuItem_Click(object sender, EventArgs e)
		{
			//firing event for refresh Data
			object selectedValue = this.SelectedValue;

			if (ComboRefreshWanted !=null)
			{
				ComboRefreshWanted(this);
			}

			this.SelectedValue = selectedValue;
		}

		private void SetNullValue()
		{
			//			this.IsEditing = false;
			//			this.SelectedValue = DBNull.Value;
			this.IsEditing = true;
			this.SelectedValue = DBNull.Value;
			this.IsEditing = false;
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if(this.ReadOnly)
			{
				e.Handled = true;
			}
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if(this.ReadOnly)
			{
				e.Handled = true;
				return;
			}

			if(e.KeyCode == Keys.Delete)
			{
				SetNullValue();
			}

//			if(this.ReadOnly && (
//				//				e.KeyCode == Keys.Up	||
//				//				e.KeyCode == Keys.Down	||
//				//				e.KeyCode == Keys.Tab	||
//				e.KeyCode == Keys.Delete))
//				e.Handled = true;
//			else
				base.OnKeyDown (e);
		}
		
		private bool _isEditing = false;
		public bool IsEditing
		{
			get
			{
				return _isEditing;
			}
			set
			{
				_isEditing = value;

//				System.Diagnostics.Debug.WriteLine("IsEditing: " + value.ToString());

				if(_isEditing)
				{
					if (EditingStarted !=null)
					{
						EditingStarted(this, EventArgs.Empty);
					}
				}
			}
		}

		private void AsCombo_DropDown(object sender, EventArgs e)
		{
			this.IsEditing = true;

			// Adjust width of the combo
			ComboBox senderComboBox = (ComboBox)sender;
			int width = senderComboBox.DropDownWidth;
			Graphics g = senderComboBox.CreateGraphics();
			Font font = senderComboBox.Font;
			int vertScrollBarWidth = 
				(senderComboBox.Items.Count>senderComboBox.MaxDropDownItems)
				?SystemInformation.VerticalScrollBarWidth:0;

			int newWidth;
			foreach (object o in ((ComboBox)sender).Items)
			{
				string s = this.GetItemText(o);

				newWidth = (int) g.MeasureString(s, font).Width 
					+ vertScrollBarWidth;
				if (width < newWidth )
				{
					width = newWidth;
				}
			}
			senderComboBox.DropDownWidth = width;
		}

		private void AsCombo_KeyUp(object sender, KeyEventArgs e)
		{
			XAsCombo cbo = this;
			String sTypedText;
			Int32 iFoundIndex;
			Object oFoundItem;
			String sFoundText ;
			String sAppendText ;

			//'Allow select keys without Autocompleting
			switch (e.KeyCode)
			{
				case Keys.Control:
					return;
				case Keys.ControlKey:
					return;
				case Keys.Shift:
					return;
				case Keys.ShiftKey:
					return;
				case Keys.Back:
					return;
				case Keys.Left:
					return;
				case Keys.Right:
					return;
				case Keys.Tab:
					return;
				case Keys.Up:
					return;
				case Keys.Delete:
					return;
				case Keys.Down:
					return;
			}

			try
			{
				//'Get the Typed Text and Find it in the list
				sTypedText = cbo.Text;
				iFoundIndex = cbo.FindString(sTypedText);

				//'If we found the Typed Text in the list then Autocomplete
				if (iFoundIndex >= 0)

				{ 
					//'Get the Item from the list (Return Type depends if Datasource was bound 
					//' or List Created)
					oFoundItem = cbo.Items[iFoundIndex];

					//'Use the ListControl.GetItemText to resolve the Name in case the Combo 
					//' was Data bound
					sFoundText = cbo.GetItemText(oFoundItem);

					//'Append then found text to the typed text to preserve case
					sAppendText = sFoundText.Substring(sTypedText.Length);

					if(oFoundItem != cbo.SelectedItem)
					{
						bool wasEditing = this.IsEditing;
						this.IsEditing = true;
						_ignoreSelectedIndex = true;
						cbo.SelectedItem = oFoundItem;
						cbo.Text = sTypedText.ToString() + sAppendText.ToString();
						_ignoreSelectedIndex = false;
						//this.IsEditing = wasEditing;
					}

					//'Select the Appended Text
					cbo.SelectionStart = sTypedText.Length;
					cbo.SelectionLength = sAppendText.Length;
				}
				else
				{
					SetNullValue();
					this.Text = "";
				}
			}
			catch {}
		}

		protected override void OnSelectedValueChanged(EventArgs e)
		{
			if(this.IsEditing)
			{
				base.OnSelectedValueChanged (e);
			}
		}

		public new object SelectedValue
		{
			get
			{
				if(base.SelectedValue == null)
				{
					return DBNull.Value;
				}
				else
				{
					return base.SelectedValue;
				}
			}
			set
			{
				if(value == DBNull.Value)
				{
					bool wasEditing = this.IsEditing;
					this.IsEditing = false;
					this.SelectedIndex = -1;
					this.IsEditing = wasEditing;
					base.SelectedValue = value;
					
					// for some reason, the event is not fired after we changed the value before
					OnSelectedValueChanged(EventArgs.Empty);
				}

				_ignoreSelectedIndex = true;
				base.SelectedValue = value;
				_ignoreSelectedIndex = false;

				if(! this.Focused) this.IsEditing = false;
			}
		}

		private bool _ignoreSelectedIndex = false;

		public override int SelectedIndex
		{
			get
			{
				return base.SelectedIndex;
			}
			set
			{
				if(_ignoreSelectedIndex) 
				{
					base.SelectedIndex = value;
					return;
				}
				
				//if(this.Focused == false & this.IsEditing == true) this.IsEditing = false;

				if(this.DataBindings.Count == 0) 
				{
					base.SelectedIndex = value;
					return;
				}

				if(this.DataBindings[this.DefaultBindableProperty].BindingManagerBase == null)
				{
					base.SelectedIndex = -1;
					return;
				}

				if(this.DataBindings[this.DefaultBindableProperty].BindingManagerBase.Position == -1)
				{
					base.SelectedIndex = -1;
					return;
				}

				DataRow row = ((DataRowView)(this.DataBindings[this.DefaultBindableProperty].BindingManagerBase).Current).Row;
				string bindingField = this.DataBindings[this.DefaultBindableProperty].BindingMemberInfo.BindingField;

				if(value == -1)
				{
					if(base.SelectedIndex == -1) return;

					if(row[bindingField] != DBNull.Value)
					{
						//row[bindingField] = DBNull.Value;
					}

					base.SelectedIndex = -1;
					return;
				}

				if((value == 0 | _settingDataSource) & this.Visible)
				{
					// data source value is null, but our index is 0
					if(row[bindingField] == DBNull.Value)
					{
						base.SelectedIndex = -1;
						return;
					}

					// data source is not null, but it is not the currently selected value either
					if(! row[bindingField].Equals(this.SelectedValue))
					{
						_ignoreSelectedIndex = true;
						this.SelectedValue = row[bindingField];
						_ignoreSelectedIndex = false;
						
						// after setting the value, it is null, so it was not found in the combo,
						// so we set the data row value to null as well
						if(this.SelectedValue == DBNull.Value)
						{
							row[bindingField] = DBNull.Value;
						}
////							this.IsEditing = true;
//						this.SelectedValue = DBNull.Value;
////							this.IsEditing = false;
						return;
					}
				}

				base.SelectedIndex = value;
			}
		}

		private bool _settingDataSource = false;

		public new object DataSource
		{
			get
			{
				return base.DataSource;
			}
			set
			{
				//_ignoreSelectedIndex = true;
				_settingDataSource = true;
				string displayMember = this.DisplayMember;
				base.DataSource = value;
				this.DisplayMember = displayMember;
				_settingDataSource = false;
				//_ignoreSelectedIndex = false;
			}
		}

//		private void CheckText()
//		{
//			if(this.SelectedValue == DBNull.Value & this.Text != "")
//			{
//				this.Text = "";
//			}
//		}
//
//		private void _timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
//		{
//			CheckText();
		//		}

		private void AsCombo_KeyDown(object sender, KeyEventArgs e)
		{
			switch (e.KeyCode)
			{
				case Keys.Up:
					this.IsEditing = true;
					return;
				case Keys.Down:
					this.IsEditing = true;
					return;
			}
		}

		private void AsCombo_Leave(object sender, EventArgs e)
		{
			this.IsEditing = false;
		}
	}
}
