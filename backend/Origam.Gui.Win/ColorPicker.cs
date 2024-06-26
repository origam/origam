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
using System.Drawing;
using System.Windows.Forms;

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for ColorPicker.
/// </summary>
public class ColorPicker : BaseCaptionControl
{
	public event EventHandler selectedColorChanged;
	private System.Windows.Forms.Button button1;
	private System.Windows.Forms.ColorDialog colorDialog1;
	/// <summary> 
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;
	public ColorPicker()
	{
		// This call is required by the Windows.Forms Form Designer.
		InitializeComponent();
		// TODO: Add any initialization after the InitializeComponent call
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
		this.button1 = new System.Windows.Forms.Button();
		this.colorDialog1 = new System.Windows.Forms.ColorDialog();
		this.SuspendLayout();
		// 
		// button1
		// 
		this.button1.Location = new System.Drawing.Point(0, 0);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(24, 24);
		this.button1.TabIndex = 0;
		this.button1.Text = ">";
		this.button1.Click += new System.EventHandler(this.button1_Click);
		// 
		// ColorPicker
		// 
		this.Controls.Add(this.button1);
		this.Name = "ColorPicker";
		this.Size = new System.Drawing.Size(24, 24);
		this.ResumeLayout(false);
	}
	#endregion
	public override string DefaultBindableProperty
	{
		get
		{
			return "SelectedColor";
		}
	}
	private int _selectedColor = 0;
	public object SelectedColor
	{
		get
		{
			if(_selectedColor == -1)
			{
				return DBNull.Value;
			}
			else
			{
				return _selectedColor;
			}
		}
		set
		{
			if(value == DBNull.Value)
			{
				_selectedColor = -1;
				this.button1.BackColor = Color.Transparent;
				OnSelectedColorChanged(EventArgs.Empty);
			}
			else
			{
				if(_selectedColor != (int)value)
				{
					_selectedColor = (int)value;
					this.button1.BackColor = Color.FromArgb((int)value);
					OnSelectedColorChanged(EventArgs.Empty);
				}
			}
		}
	}
	private bool _readOnly = false;
	public bool ReadOnly
	{
		get
		{
			return _readOnly;
		}
		set
		{
			_readOnly = value;
		}
	}
	void OnSelectedColorChanged(EventArgs e)
	{
		if (selectedColorChanged != null) 
		{
			selectedColorChanged(this, e);
		}
	}
	private void button1_Click(object sender, System.EventArgs e)
	{
		if(_readOnly) return;
		colorDialog1.Color = this.button1.BackColor;
		DialogResult result = colorDialog1.ShowDialog(this.FindForm() as IWin32Window);
		
		this.SelectedColor = colorDialog1.Color.ToArgb();
	}
}
