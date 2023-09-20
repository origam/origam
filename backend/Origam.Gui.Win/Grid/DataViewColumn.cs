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
using System.Windows.Forms;
using System.Drawing;
using System.Data;
using Origam.Rule;
using Origam.Schema.EntityModel;

namespace Origam.Gui.Win
{

	public class AsDataViewColumn : DataGridTextBoxColumn
	{
		public AsDateBox AsDateBox;
		private bool _isEditing;

		private CurrencyManager dataSource;
		private int rowNum;
		
		
		public AsDataViewColumn() : base()
		{
			_isEditing = false;
			AsDateBox = new AsDateBox();
			AsDateBox.NoKeyUp = true;
			AsDateBox.CaptionPosition = CaptionPosition.None;
			AsDateBox.Visible = false;
			base.TextBox.TabStop =false;
			this.AsDateBox.TabStop = false;
			AsDateBox.onTextBoxValueChanged += OnAsDateBoxTextValueChanged;

		}

		private void OnAsDateBoxTextValueChanged(object sender, EventArgs args)
		{
			// dataSource is null if this method gets called vefore Commit method
			if (dataSource == null) return;
			
			// Workaround to write chages made in textBox to dataSource 
			// and redraw when Tab is pressed. 
			// It looks like without this handler the Commit method gets 
			// called before AsDateBox has a chance to update it's state.
			// This handler is called after the update is complete so that
			// the Commit method can update the dataSource with the new data.  
			if (GetColumnValueAtRow(dataSource, rowNum) != AsDateBox.DateValue)
			{
				_isEditing = true;
				Commit(dataSource, rowNum);
			}
		}

		private bool _alwaysReadOnly = false;
		public bool AlwaysReadOnly
		{
			get => _alwaysReadOnly;
			set
			{
				_alwaysReadOnly = value;
				this.AsDateBox.ReadOnly = value;
			}
		}

		private bool _isDisposed = false;
		protected override void Dispose(bool disposing)
		{
			if(disposing)
			{
				if(AsDateBox != null)
				{
					AsDateBox.dateValueChanged -= new EventHandler(AsDateBox_dateValueChanged);
					AsDateBox.onTextBoxValueChanged -= OnAsDateBoxTextValueChanged;
					AsDateBox.Dispose();
					AsDateBox = null;
				}
			}

			base.Dispose (disposing);

			_isDisposed = true;
		}

		protected override void Abort(int rowNum)
		{
			if(_isDisposed) return;

			_isEditing = false;
			AsDateBox.dateValueChanged -= new EventHandler(AsDateBox_dateValueChanged);
			Invalidate();

			base.Abort(rowNum);
		}


		protected override void Edit(System.Windows.Forms.CurrencyManager source, int rowNum, System.Drawing.Rectangle bounds, bool readOnly, string instantText, bool cellIsVisible)
		{
			if(! AlwaysReadOnly)
			{
				RuleEngine ruleEngine = (this.DataGridTableStyle.DataGrid.FindForm() as AsForm).FormGenerator.FormRuleEngine;
				if(ruleEngine != null)
				{
					AsDateBox.ReadOnly = ! ruleEngine.EvaluateRowLevelSecurityState((source.Current as DataRowView).Row, this.MappingName, Schema.EntityModel.CredentialType.Update);
				}
			}
			else
			{
				AsDateBox.ReadOnly = true;
			}

			if(cellIsVisible)
			{
				base.Edit(source,rowNum, bounds, readOnly, instantText , cellIsVisible);

				this.AsDateBox.Enabled = true;
		
				AsDateBox.Parent = this.TextBox.Parent;
				AsDateBox.Location = bounds.Location;
				//AsDateBox.Location = this.TextBox.Location;
				AsDateBox.Size = new Size(this.TextBox.Size.Width, AsDateBox.Size.Height);

				try
				{	
					object val= this.GetColumnValueAtRow(source, rowNum);
					AsDateBox.DateValue = val;
					//AsDateBox.DateValue =  Convert.ToDateTime(this.TextBox.Text);
				}
				catch
				{
					AsDateBox.DateValue=DBNull.Value;
				}
			
				if(this.AsDateBox.ReadOnly)
				{
					this.TextBox.Visible = true;
					AsDateBox.Visible = false;
				}
				else
				{
					this.TextBox.Visible = false;
					AsDateBox.Visible = true;
					AsDateBox.BringToFront();
					AsDateBox.Focus();	
				}

				AsDateBox.dateValueChanged += new EventHandler(AsDateBox_dateValueChanged);
			}
			else
			{
				this.AsDateBox.Bounds = Rectangle.Empty;
//				this.AsDateBox.Visible = false;
//				this.AsDateBox.Enabled = false;
			}
		}

		protected override bool Commit(System.Windows.Forms.CurrencyManager dataSource, int rowNum)
		{
			AsDateBox.dateValueChanged -= new EventHandler(AsDateBox_dateValueChanged);
			this.dataSource = dataSource;
			this.rowNum = rowNum;

			if(_isEditing)
			{
				try 
				{
					SetColumnValueAtRow(dataSource, rowNum, AsDateBox.DateValue);
                    // force form items to reread data values to prevent loss of data
                    DataBindingTools.UpdateBindedFormComponent(
                        dataSource.Bindings, MappingName);
				} 
				catch (Exception) 
				{
					Abort(rowNum);
					return false;
				}
			}
			else
			{
				Abort(rowNum);
			}
			
			_isEditing = false;
			this.AsDateBox.Hide();
			Invalidate();
			return true;
		}

		protected override void ReleaseHostedControl()
		{
			base.ReleaseHostedControl ();

			this.AsDateBox.Parent = null;
		}

		protected override void ConcedeFocus()
		{
			AsDateBox.Hide();
			base.ConcedeFocus();
		}

		protected override void SetColumnValueAtRow(System.Windows.Forms.CurrencyManager source, int rowNum, object value)
		{
			if(_isEditing)
			{
				base.SetColumnValueAtRow(source, rowNum, value);
			}

		}

		protected override void Paint(Graphics g, Rectangle bounds, CurrencyManager source, int rowNum, Brush backBrush, Brush foreBrush, bool alignToRight)
		{
			Brush myBackBrush = backBrush;
			Brush myForeBrush = foreBrush;

			EntityFormatting formatting = DataGridColumnStyleHelper.Formatting(this, source, rowNum);
			if(formatting != null)
			{
				if(!formatting.UseDefaultBackColor) myBackBrush = new SolidBrush(formatting.BackColor);
				if(!formatting.UseDefaultForeColor) myForeBrush = new SolidBrush(formatting.ForeColor);
			}

			base.Paint (g, bounds, source, rowNum, myBackBrush, myForeBrush, alignToRight);
		}


		private void AsDateBox_dateValueChanged(object sender, EventArgs e)
		{
			_isEditing = true;
			base.ColumnStartedEditing((Control) sender);
		}



	}
}
