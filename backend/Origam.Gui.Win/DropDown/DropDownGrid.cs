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
using System.Data;
using System.Drawing;
using System.ComponentModel;
using System.Windows.Forms;
using Origam.UI;

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for DropDownList.
/// </summary>
public class DropDownGrid : System.Windows.Forms.Form, ILookupDropDownPart
{
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;
	private const int MAX_ITEMS = 20;
	private DropDownDataGrid grid;
	private const int MIN_ITEMS = 4;
	public DropDownGrid()
	{
		//
		// Required for Windows Form Designer support
		//
		InitializeComponent();
		//this.Height = (this.list.ItemHeight * MIN_ITEMS) + 2;
	}
	/// <summary>
	/// Clean up any resources being used.
	/// </summary>
	protected override void Dispose( bool disposing )
	{
		if( disposing )
		{
			if(components != null)
			{
				components.Dispose();
			}
			this.DataSource = null;
		}
		base.Dispose( disposing );
	}
	protected override bool ProcessTabKey(bool forward)
	{
		this.SelectItem();
		return true;
	}
//		/// <summary>
//		/// Paint the form and draw a neat border.
//		/// </summary>
//		/// <param name="e">Information about the paint event</param>
//		protected override void OnPaint(PaintEventArgs e)
//		{
//			base.OnPaint(e);
//			Rectangle borderRect = new Rectangle(this.ClientRectangle.Location, this.ClientRectangle.Size);
//			borderRect.Width -= 1;
//			borderRect.Height -= 1;
//			e.Graphics.DrawRectangle(SystemPens.ControlDark, borderRect);			
//		}
//
	#region Windows Form Designer generated code
	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{
		this.grid = new Origam.Gui.Win.DropDownGrid.DropDownDataGrid();
		((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
		this.SuspendLayout();
		// 
		// grid
		// 
		this.grid.AllowNavigation = false;
		this.grid.AlternatingBackColor = System.Drawing.SystemColors.Info;
		this.grid.BackgroundColor = System.Drawing.SystemColors.Window;
		this.grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.grid.CaptionVisible = false;
		this.grid.DataMember = "";
		this.grid.Dock = System.Windows.Forms.DockStyle.Fill;
		this.grid.FlatMode = true;
		this.grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
		this.grid.Location = new System.Drawing.Point(1, 1);
		this.grid.Name = "grid";
		this.grid.ParentRowsVisible = false;
		this.grid.ReadOnly = true;
		this.grid.RowHeadersVisible = false;
		this.grid.Size = new System.Drawing.Size(290, 214);
		this.grid.TabIndex = 0;
		// 
		// DropDownGrid
		// 
		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
		this.BackColor = System.Drawing.SystemColors.ControlDark;
		this.ClientSize = new System.Drawing.Size(292, 216);
		this.ControlBox = false;
		this.Controls.Add(this.grid);
		this.DockPadding.All = 1;
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
		this.KeyPreview = true;
		this.Name = "DropDownGrid";
		this.ShowInTaskbar = false;
		this.Text = "DropDownGrid";
		((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
		this.ResumeLayout(false);
	}
	#endregion
	#region Properties
	public override bool Focused
	{
		get
		{
			if(base.Focused | grid.Focused) 
			{
				return true;
			}

            return false;
        }
	}
	private bool _canceled = false;
	public bool Canceled
	{
		get
		{
			return _canceled;
		}
		set
		{
			_canceled = value;
		}
	}
	
	private BaseDropDownControl _dropDownControl;
	public BaseDropDownControl DropDownControl
	{
		get
		{
			return _dropDownControl;
		}
		set
		{
			_dropDownControl = value;
		}
	}
	#endregion
	#region Private Methods
	private void UpdateListSize()
	{
		if(this.IsDisposed)
        {
            return;
        }

        this.Height = 200;
		int w = 5;
		if(grid.VisibleRowCount == this.Context.Count)
		{
			int h = 5 + ((grid.PreferredRowHeight+1) * grid.VisibleRowCount);
			if(grid.ColumnHeadersVisible)
			{
				h += grid.PreferredRowHeight;
			}
			if(this.Height > h)
			{
				this.Height = h;
			}
		}
		else
		{
			// more rows than displayed, we have to calculate with the vertical scrollbar for the total width of the dropdown
			w += SystemInformation.VerticalScrollBarWidth;
		}
		foreach(DataGridColumnStyle st in grid.TableStyles[0].GridColumnStyles)
		{
			ColAutoResize(st);
			w += st.Width;
		}
		if(w < this.Width & this.ColumnList.Length == 1)
		{
			grid.TableStyles[0].GridColumnStyles[0].Width = this.Width-5;
		}
		else if(w > this.Width)
		{
			this.Width = w;
		}
		Rectangle screen = Screen.FromControl(this.DropDownControl).WorkingArea;
	    Point location = new System.Drawing.Point(
	        this.DropDownControl.ScreenLocation.X,
	        this.DropDownControl.ScreenLocation.Y);
		int screenTotalWidth = screen.X + screen.Width;
		int screenTotalHeight = screen.Y + screen.Height;
		if(location.X + this.Width > screenTotalWidth)
		{
			location.X -= location.X + this.Width - screenTotalWidth;
		}
		if(location.X < screen.X)
        {
            location.X = screen.X;
        }

        if (location.Y + this.Height > screenTotalHeight)
		{
			location.Y -= (this.DropDownControl.Height + this.Height);
		}
		this.Location = location;
	}
	private void ColAutoResize(DataGridColumnStyle style)
	{
		DataGridTableStyle myGridTable = grid.TableStyles[0];
		CurrencyManager listManager = this.Context as CurrencyManager;
		if (listManager != null)
		{
			using (Graphics graphics = grid.CreateGraphics())
			{
				Font headerFont;
				string headerText = style.HeaderText;
				headerFont = myGridTable.HeaderFont;
				int num = ((int) graphics.MeasureString(headerText, headerFont).Width) + (grid.ColumnHeadersVisible ? 25 : 0);// + this.layout.ColumnHeaders.Height) + 1;
				int count = listManager.Count;
				
				bool emptyColumn = true;
				for (int i = 0; i < count; i++)
				{
					string columnValueAtRow = (listManager.List as DataView)[i][style.MappingName].ToString();
					if(! columnValueAtRow.Equals(""))
                    {
                        emptyColumn = false;
                    }

                    int width = ((int) graphics.MeasureString(columnValueAtRow, grid.Font).Width) + 2;
					if (width > num)
					{
						num = width;
					}
				}
				if(emptyColumn & (this.DropDownControl as ILookupControl).SuppressEmptyColumns)
				{
					style.Width = 0;
				}
				else
				{
					if (style.Width != num)
					{
						style.Width = num;
					}
				}
			}
		}
	}
	#endregion
	#region Public Methods
	public void SelectItem()
	{
		this.Close();
	}
	public void MoveUp()
	{
		this.grid.Focus();
		if(this.grid.CurrentRowIndex > 0)
		{
			this.grid.CurrentRowIndex--;
		}
	}
	public void MoveDown()
	{
		this.grid.Focus();
		if(this.grid.CurrentRowIndex < this.Context.Count-1)
		{
			this.grid.CurrentRowIndex++;
		}
	}
	#endregion
	#region Event Handlers
	private void DropDownGrid_ListChanged(object sender, ListChangedEventArgs e)
	{
		UpdateListSize();
	}
	#endregion
	#region IDropDownPart Members
	public DataView DataSource
	{
		get
		{
			return this.grid.DataSource as DataView;
		}
		set
		{
			if(value != null)
			{
				value.ListChanged -= new ListChangedEventHandler(DropDownGrid_ListChanged);
				if(grid.TableStyles.Count == 0)
				{
					DataGridTableStyle ts = new DataGridTableStyle();
					ts.HeaderBackColor = OrigamColorScheme.ButtonBackColor;
					ts.HeaderForeColor = OrigamColorScheme.GridHeaderForeColor;
					ts.SelectionBackColor = OrigamColorScheme.GridSelectionBackColor;
					ts.SelectionForeColor = OrigamColorScheme.GridSelectionForeColor;
					ts.MappingName = (value as DataView).Table.TableName;
					ts.RowHeadersVisible = false;
					ts.ColumnHeadersVisible = (ColumnList.Length > 1);
					grid.ColumnHeadersVisible = ts.ColumnHeadersVisible;
					ts.AlternatingBackColor = grid.AlternatingBackColor;
					foreach(string col in ColumnList)
					{
						DataColumn dataCol = (value as DataView).Table.Columns[col];
						DropDownTextColumn c = new DropDownTextColumn();
						c.NullText = "";
						c.Alignment = (dataCol.DataType == typeof(int) | dataCol.DataType == typeof(float) | dataCol.DataType == typeof(decimal) | dataCol.DataType == typeof(double) ? HorizontalAlignment.Right : HorizontalAlignment.Left);
						c.MappingName = col;
						c.HeaderText = dataCol.Caption;
						ts.GridColumnStyles.Add(c);
					}
					grid.TableStyles.Add(ts);
				}
			}
			grid.SetDataBinding(value, "");
			if(value == null)
            {
                return;
            }

            UpdateListSize();
			value.ListChanged += new ListChangedEventHandler(DropDownGrid_ListChanged);
		}
	}
	private string _valueMember;
	public string ValueMember
	{
		get
		{
			return _valueMember;
		}
		set
		{
			_valueMember = value;
		}
	}
	private string _displayMember;
	public string DisplayMember
	{
		get
		{
			return _displayMember;
		}
		set
		{
			_displayMember = value;
		}
	}
	private string[] ColumnList
	{
		get
		{
			return this.DisplayMember.Split(";".ToCharArray());
		}
	}
	private BindingManagerBase Context
	{
		get
		{
			return this.grid.BindingContext[this.DataSource, ""];
		}
	}
	public string SelectedText
	{
		get
		{
			if(this.Context.Position >= 0)
			{
				return (this.Context.Current as DataRowView)[this.ColumnList[0]].ToString();
			}

            return null;
            //				return this.list.GetItemText(this.list.SelectedItem);
        }
		set
		{
			throw new NotImplementedException();
		}
	}
	public object SelectedValue
	{
		get
		{
			if(this.Context.Position < 0)
            {
                return DBNull.Value;
            }

            return (this.Context.Current as DataRowView)[this.ValueMember];
//				return this.list.SelectedValue;
		}
		set
		{
			if(value == null)
			{
				if(this.Context.Count > 0)
				{
					this.Context.Position = 0;
				}
			}
			else
			{
				try
				{
					DataView view = (this.Context as CurrencyManager).List as DataView;
					for(int i=0; i < view.Count; i++)
					{
						if(view[i].Row[this.ValueMember].Equals(value))
						{ 
							this.Context.Position = i;
						}
					}
				}
				catch {}
			}
		}
	}
	public string ParentMember
	{
		get
		{
			// TODO:  Add DropDownList.ParentMember getter implementation
			return null;
		}
		set
		{
			// TODO:  Add DropDownList.ParentMember setter implementation
		}
	}
	#endregion
	private class DropDownTextColumn : DataGridTextBoxColumn
	{
		public DropDownTextColumn() : base()
		{
			this.TextBox.VisibleChanged += new EventHandler(TextBox_VisibleChanged);
		}
		private void TextBox_VisibleChanged(object sender, EventArgs e)
		{
			if(this.TextBox.Visible)
			{
				this.TextBox.Hide();
			}
		}
		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			if(source.Position == rowNum)
			{
				backBrush = new SolidBrush(this.DataGridTableStyle.SelectionBackColor);
				foreBrush = new SolidBrush(this.DataGridTableStyle.SelectionForeColor);
			}
			base.Paint (g, bounds, source, rowNum, backBrush, foreBrush, alignToRight);
		}
	}
	private class DropDownDataGrid : DataGrid
	{
		public DropDownDataGrid() : base()
		{
		}
		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp (e);
			DataGrid.HitTestInfo hti = this.HitTest(e.X, e.Y);
			if(hti.Row >= 0)
			{
				DropDownGrid parent = this.Parent as DropDownGrid;
				parent.SelectItem();
			}
		}
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			DropDownGrid parent = this.Parent as DropDownGrid;
			if(parent != null)
			{
				if(keyData == Keys.Return)
				{
					parent.SelectItem();
					return true;
				}
				if(keyData == Keys.Escape)
				{
					parent.Canceled = true;
					parent.Close();
					return true;
				}
				if((keyData & Keys.KeyCode) == Keys.Tab)
				{
					parent.ProcessTabKey(true);
					return true;
				}
			}
			return base.ProcessCmdKey (ref msg, keyData);
		}
	}
}
