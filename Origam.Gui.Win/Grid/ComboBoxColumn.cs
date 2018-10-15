//using System;
//using System.Windows.Forms;
//using System.Drawing;
//using System.Data;
//
//namespace Origam.Gui.Win
//{
//
//	public class AsDataGridComboBoxColumn : DataGridTextBoxColumn
//	{
//		public NoKeyUpCombo AsCombo;
//		private bool _isEditing;
//		public static int _RowCount;
//
//		public AsDataGridComboBoxColumn() : base()
//		{
//			_isEditing = false;
//			_RowCount = -1;
//	
//			AsCombo = new NoKeyUpCombo();
//			AsCombo.Height = 21;
////			AsCombo.UseTimer = false;
//			//AsCombo.DropDownStyle = ComboBoxStyle.DropDown;
//			AsCombo.CaptionLength=0;
//			AsCombo.Leave += new EventHandler(LeaveComboBox);
//			//		AsCombo.Enter += new EventHandler(ComboMadeCurrent);
////			AsCombo.SelectedValueChanged += new EventHandler(ComboStartEditing);
//			AsCombo.EditingStarted += new EventHandler(ComboStartEditing);
//			//base.TextBox.TabStop =false;
//			//this.AsCombo.TabStop = false;
//		
//		}
//		private void ComboStartEditing(object sender, EventArgs e)
//		{
//			_isEditing = true;
//			base.ColumnStartedEditing((Control) sender);
//			
//			
//		}
//
//		private void HandleScroll(object sender, EventArgs e)
//		{
//			if(AsCombo.Visible)
//			{
//				LeaveRoutine();
//			}
//
//		}
//		//		private void ComboMadeCurrent(object sender, EventArgs e)
//		//		{
//		//			_isEditing = true; 	
//		//		}
//		//		
//
//		private void LeaveRoutine()
//		{
////			Invalidate();
//			AsCombo.Hide();
////			AsCombo.SendToBack();
////			this.ConcedeFocus();
//		}
//
//
//		protected override void ConcedeFocus()
//		{
//			LeaveRoutine();
//		}
//
//		private void LeaveComboBox(object sender, EventArgs e)
//		{
//			this.AsCombo.Hide();
//			//LeaveRoutine();
//			this.DataGridTableStyle.DataGrid.Scroll -= new EventHandler(HandleScroll);
//		}
//
//		protected override void Edit(System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
//		{
//			if(rowNum < 0) return;
//
//			base.Edit(source, rowNum, bounds, readOnly, instantText , cellIsVisible);
//
//			this.TextBox.Hide();
//			AsCombo.Enabled = true;
//			AsCombo.BringToFront();
//
//			AsCombo.Parent = this.TextBox.Parent;
//			AsCombo.Location = bounds.Location;
//			AsCombo.Size = new Size(bounds.Width, this.AsCombo.Height);
//
//			AsCombo.Visible = true;
//
//			object o = base.GetColumnValueAtRow(source, rowNum);
//
//			if(o == null)
//			{
//				AsCombo.SelectedValue = DBNull.Value;
//			}
//			else
//			{
//				AsCombo.SelectedValue = o;
//			}
//
////			int newIndex = AsCombo.FindStringExact(this.TextBox.Text);
////
////			if(newIndex == -1)
////			{
////				AsCombo.SelectedValue = DBNull.Value;
////			}
////			else
////			{
////				AsCombo.SelectedItem = AsCombo.Items[newIndex];
////				//AsCombo.SetSelectedIndex(newIndex);
////			}
//
//
//			this.DataGridTableStyle.DataGrid.Scroll += new EventHandler(HandleScroll);
//
//			AsCombo.Focus();	
//		}
//
//		protected override bool Commit(System.Windows.Forms.CurrencyManager dataSource, int rowNum)
//		{
//			if(!_isEditing) return true;
//		
//			_isEditing = false;
//			SetColumnValueAtRow(dataSource, rowNum, AsCombo.Text);
//
//			Invalidate();
//
//			return true;
//		}
//
////		protected override void ConcedeFocus()
////		{
////			base.ConcedeFocus();
////		}
//
//		protected override object GetColumnValueAtRow(System.Windows.Forms.CurrencyManager source, int rowNum)
//		{
//			object s =  base.GetColumnValueAtRow(source, rowNum);
//
//			if(s == DBNull.Value) return "";
//
//			DataView sourceView = ReadDataViewFromCombo(AsCombo);
//			string colName = FormGenerator.GetColumnNameFromDisplayMember(this.AsCombo.ValueMember);
//
////			Rectangle rect = this.DataGridTableStyle.DataGrid.GetCurrentCellBounds();
////			AsCombo.Location = rect.Location;
//
//			if(sourceView.Table.PrimaryKey.Length == 1 
//				& sourceView.Table.PrimaryKey[0].ColumnName == colName)
//			{
//				DataRow foundRow = sourceView.Table.Rows.Find(s);
//
//				if(foundRow == null)
//				{
//					return "";// s;
//				}
//				else
//				{
//					return foundRow[FormGenerator.GetColumnNameFromDisplayMember(this.AsCombo.DisplayMember)];
//				}
//			}
//
//			foreach(DataRowView dv in sourceView)
//			{ 
//				if( dv.Row[colName].ToString() == s.ToString() )
//				{
//					return dv.Row[FormGenerator.GetColumnNameFromDisplayMember(this.AsCombo.DisplayMember)];
//				}
//			}
//
//			return s;
//		}
//
//		private DataView ReadDataViewFromCombo(AsCombo combo)
//		{
//			if( this.AsCombo.DataSource is DataView)
//			{
//				DataView view = this.AsCombo.DataSource as DataView;
//
//				string tableName = "";
//				bool related = false;
//
//				if(this.AsCombo.DisplayMember.IndexOf(".") > 0)
//				{
//					related = true;
//					tableName = FormGenerator.FindTableByDisplayMember(view.DataViewManager.DataSet, this.AsCombo.DisplayMember);			
//				}
//				else
//				{
//					tableName = view.Table.TableName;
//				}
//
//				DataView findRowView = view;
//				if(related)
//				{
//					foreach(DataRelation relace in  view.Table.ChildRelations)
//					{
//						if(relace.ChildTable.TableName == tableName)
//						{
//							findRowView = relace.ChildTable.DefaultView;
//							break;
//						}
//					}
//				}
//
//				return findRowView;
//			}
//
//			DataView dv = null;
//			if( this.AsCombo.DataSource is DataTable)
//			{
//				dv = (this.AsCombo.DataSource as DataTable).DefaultView;
//			}
//			else if(this.AsCombo.DataSource is DataSet)
//			{
//				string table = FormGenerator.FindTableByDisplayMember(this.AsCombo.DataSource as DataSet, this.AsCombo.DisplayMember);
//				dv = (this.AsCombo.DataSource as DataSet).Tables[table].DefaultView;
//			}
////			else
////				throw new NotImplementedException("Type " + this.AsCombo.DataSource.GetType().ToString() + " for comboboxcolumn wan't implement yet! ");
//
//			return dv;
//		}
//
//		private string GetColumnNameFromMember(string member)
//		{
//			int pos=member.LastIndexOf(".");
//			if(pos>0)
//				return member.Substring(pos+1);
//			else
//				return member;
//		}
//
//		protected override void SetColumnValueAtRow(System.Windows.Forms.CurrencyManager source, int rowNum, object value)
//		{
//			object s = null;
//			
//			int rowCount = AsCombo.Items.Count; //dv.Count;
//
//			foreach(DataRowView dv in this.AsCombo.Items)
//			{
//				string colName = FormGenerator.GetColumnNameFromDisplayMember(this.AsCombo.DisplayMember);
//
//				if( dv.Row[colName].ToString() == value.ToString() )
//				{
//					s = dv.Row[FormGenerator.GetColumnNameFromDisplayMember(this.AsCombo.ValueMember)];
//					break;
//				}
//			}
//
//			if (s == null) s = DBNull.Value;
//
////			//if things are slow, you could order your dataview
////			//& use binary search instead of this linear one
////			while (i < rowCount)
////			{
////				if( s.Equals( dv[i][GetColumnNameFromMember(this.AsCombo.DisplayMember)]))
////					break;
////				++i;
////			}
////			if(i < rowCount)
////				s =  dv[i][GetColumnNameFromMember(this.AsCombo.ValueMember)];
////			else
////				s = DBNull.Value;
//			
//			base.SetColumnValueAtRow(source, rowNum, s);
////			Application.DoEvents();
//
//
//		}
//
//
//		protected override void Dispose(bool disposing)
//		{
//			if(disposing)
//			{
//				this.AsCombo.DataSource = null;
//				this.AsCombo.Dispose();
//				this.AsCombo = null;
//			}
//
//			base.Dispose (disposing);
//		}
//	}
//
//	public class NoKeyUpCombo : AsCombo 
//	{
//		private const int WM_KEYUP = 0x101;
//		private const int WM_KEYDOWN = 0x100;
//
//		protected override bool ProcessKeyMessage(ref Message m)
//		{
//			// ignore cursor keys and tab key
//			if(m.Msg == WM_KEYDOWN)
//			{
//				if(
//					m.WParam.ToInt32() == 37
//					| m.WParam.ToInt32() == 39
//					)
//				{
//					return false;
//				}
//			}
//			
//			if(m.Msg == WM_KEYUP)
//			{
//				if(
//					m.WParam.ToInt32() == 9
//					| m.WParam.ToInt32() == 38
//					| m.WParam.ToInt32() == 40
//					)
//				{
//					return true;
//				}
//				else if(
//					m.WParam.ToInt32() == 37
//					| m.WParam.ToInt32() == 39
//					)
//				{
//					return false;
//				}
//			}
//
//			return base.ProcessKeyMessage (ref m);
//		}
//	}
//}