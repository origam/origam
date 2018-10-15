using System;
using System.ComponentModel;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;


namespace Origam.Gui.Win
{

	public class AsDataGridComboBoxColumn : DataGridColumnStyle 
	{
		
		private AsCombo _comboBox;
		private int m_previouslyEditedCellRow;
		private DataGridColumnStylePadding m_padding;

		public AsDataGridComboBoxColumn() : base() 
		{
			
			_comboBox = new AsCombo();
			_comboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			_comboBox.Visible = false;
			_comboBox.SizeChanged += new EventHandler( ComboBox_SizeChanged );

						
			this.ControlSize = _comboBox.Size;
			//this.Padding = new DataGridColumnStylePadding( 4, 8, 4, 8 );
			this.Padding = new DataGridColumnStylePadding( 1, 1, 1, 1 );
			this.Width = this.GetPreferredSize( null, null ).Width;

		}

		public AsCombo AsCombo 
		{
			get { return _comboBox; }
			set { _comboBox=value;}
		}

		public DataGridColumnStylePadding Padding 
		{
			get { return m_padding; }
			set { m_padding = value; }
		}

		public Size ControlSize 
		{
			get { return _comboBox.Size; }
			set { _comboBox.Size = value; }
		}

		protected override void Abort(int rowNum) 
		{
						
			// reset combobox
			_comboBox.Visible = false;
			
		}

		protected override void Edit(CurrencyManager source, int rowNum, Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible) 
		{
	
			// get cursor coordinates
			Point p = this.DataGridTableStyle.DataGrid.PointToClient( Cursor.Position );

			// get control bounds
			Rectangle controlBounds = this.GetControlBounds( bounds );

			// get cursor bounds
			Rectangle cursorBounds = new Rectangle( p.X, p.Y, 1, 1 );

			object colValue = this.GetColumnValueAtRow( source, rowNum );

			foreach(System.Data.DataRowView rvView in _comboBox.Items)
			{
				if(rvView.Row["Id"].ToString()==colValue.ToString())
				{
					_comboBox.SelectedIndex = _comboBox.Items.IndexOf(rvView);
					break;
				}
			}

			_comboBox.Location = new Point( controlBounds.X, controlBounds.Y );
			_comboBox.Visible = true;

			if ( cursorBounds.IntersectsWith( controlBounds ) ) 
			{
				_comboBox.DroppedDown = true;
			}

			m_previouslyEditedCellRow = rowNum;
		
		}

		protected override bool Commit( CurrencyManager dataSource, int rowNum ) 
		{
			
//			if ( m_previouslyEditedCellRow == rowNum && dataSource.Position == rowNum) 
			if ( m_previouslyEditedCellRow == rowNum) 
			{
				try
				{
					if(_comboBox.SelectedValue == DBNull.Value || _comboBox.SelectedValue==null)
						this.SetColumnValueAtRow( dataSource, rowNum, DBNull.Value);
					else
						this.SetColumnValueAtRow( dataSource, rowNum, _comboBox.SelectedValue);

				}
				catch (Exception ex)
				{
					System.Diagnostics.Debug.WriteLine(ex.ToString());
				}
			}

			_comboBox.Visible = false;
			
			return true;
		
		}

		protected override void SetDataGridInColumn( DataGrid value ) 
		{
			
			base.SetDataGridInColumn( value );

			if ( !value.Controls.Contains( _comboBox ) ) 
			{
				value.Controls.Add( _comboBox );
			}
			value.Scroll +=new EventHandler(value_Scroll);

		}
			
		protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, CurrencyManager source, int rowNum, System.Drawing.Brush backBrush, System.Drawing.Brush foreBrush, bool alignToRight) 
		{

			g.FillRectangle( new SolidBrush( Color.White ), bounds );

			StringFormat sf = new StringFormat();
			sf.Alignment = StringAlignment.Near;
			sf.LineAlignment = StringAlignment.Center;

			Rectangle controlBounds = this.GetControlBounds( bounds );
			
			object colValue = this.GetColumnValueAtRow( source, rowNum );
			int index=0;


			foreach(System.Data.DataRowView rvView in _comboBox.Items)
			{
				if(rvView.Row["Id"].ToString()==colValue.ToString())
				{
					index= _comboBox.Items.IndexOf(rvView);
					break;
				}
			}


			string selectedItem="";

			if(_comboBox.Items.Count >0)
				selectedItem =  (_comboBox.Items[ index ] as System.Data.DataRowView).Row["Text"].ToString();
			

			Rectangle textRegion = new Rectangle( 
				controlBounds.X + 1,
				controlBounds.Y + 1,
				controlBounds.Width - 1,
				( int ) g.MeasureString( selectedItem, _comboBox.Font ).Height );
			
			g.DrawString( selectedItem, _comboBox.Font, foreBrush, textRegion, sf );
			
			ControlPaint.DrawBorder3D( g, controlBounds, Border3DStyle.Sunken);
			


			Rectangle buttonBounds = controlBounds;
			buttonBounds.Inflate(-2,-2);

			ControlPaint.DrawComboButton( 
				g, 
				buttonBounds.X + ( controlBounds.Width - 20 ), 
				buttonBounds.Y, 
				16, 
				17, 
				ButtonState.Normal );

		}

		private void ComboBox_SizeChanged(object sender, EventArgs e) 
		{
			
			this.ControlSize = _comboBox.Size;
			this.Width = this.GetPreferredSize( null, null ).Width;
			this.Invalidate();

		}


        //private Rectangle _lastusedBounds = new Rectangle(1,1,1,1);

		private Rectangle GetControlBounds( Rectangle cellBounds ) 
		{
//			if(cellBounds.X==0 || cellBounds.Y==0)
//			{
//				_comboBox.Visible =false;
//                
//			}

			//return _lastusedBounds;

			Rectangle controlBounds = new Rectangle( 
				cellBounds.X + this.Padding.Left, 
				cellBounds.Y + this.Padding.Top, 
				this.ControlSize.Width,
				this.ControlSize.Height );
			
			//_lastusedBounds	=controlBounds;
			return controlBounds;

		}

		#region The rest of the DataGridColumnStyle methods

		protected override int GetMinimumHeight() 
		{
			return GetPreferredHeight( null, null );
		}

		protected override int GetPreferredHeight(System.Drawing.Graphics g, object value) 
		{
			return this.ControlSize.Height + this.Padding.Top + this.Padding.Bottom;
		}

		protected override System.Drawing.Size GetPreferredSize(System.Drawing.Graphics g, object value) 
		{
			
			int width = this.ControlSize.Width + this.Padding.Left + this.Padding.Right;
			int height = this.ControlSize.Height + this.Padding.Top + this.Padding.Bottom;

			return new Size( width, height );

		}

		protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, CurrencyManager source, int rowNum) 
		{
			this.Paint( g, bounds, source, rowNum, false );
		}

		protected override void Paint(System.Drawing.Graphics g, System.Drawing.Rectangle bounds, CurrencyManager source, int rowNum, bool alignToRight) 
		{
			this.Paint( g, bounds, source, rowNum, Brushes.White, Brushes.Black, false );
		}

		#endregion The rest of the DataGridColumnStyle methods

		private void value_Scroll(object sender, EventArgs e)
		{
			_comboBox.Hide();
		}
	}

}	
