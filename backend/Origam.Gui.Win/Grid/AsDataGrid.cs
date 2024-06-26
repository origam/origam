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
using System.Collections.Generic;
using System.Windows.Forms;

namespace Origam.Gui.Win;
/// <summary>
/// Summary description for AsDataGrid.
/// </summary>
public class AsDataGrid : DataGrid
{
	public event MouseEventHandler AfterMouseMove;
	public event EventHandler EditorDoubleClicked;
    private readonly ToolTip tooltip;
	private readonly ClickTracker clickTracker;
	private bool disposed;
	public AsDataGrid()
	{
        tooltip = FormGenerator.InitializeTooltip();
		clickTracker = new ClickTracker(this);
		clickTracker.SplitDoubleClikDetected += OnGridEditorDoubleClick;
	}
	public bool IgnoreLayoutEvent { get; set; } 
	public void WatchClicksToRaiseEditorDoubleClicked(IAsGridEditor gridEditor)
	{
		clickTracker.AddControl(gridEditor);
		gridEditor.EditorDoubleClick += OnGridEditorDoubleClick;
	}
	private void OnGridEditorDoubleClick(object sender, EventArgs args)
	{
		EditorDoubleClicked?.Invoke(null, EventArgs.Empty);
	}
	protected override void OnMouseHover(EventArgs e)
    {
        HitTestInfo info = HitTest(this.PointToClient(Cursor.Position));
        if (info.Column != -1)
        {
            AsForm form = this.FindForm() as AsForm;
            DataGridColumnStyle style = TableStyles[0].GridColumnStyles[info.Column];
            tooltip.ToolTipTitle = style.HeaderText;
            tooltip.Hide(this);
            tooltip.Show(form.FormGenerator.GetTooltip(style), this);
        }
        base.OnMouseHover(e);
    }
	public void InvokeClick()
	{
		OnClick(EventArgs.Empty);
	}
	public bool EnhancedFocusControl = true;
	public int HorizontalScrollPosition
	{
		get => this.HorizScrollBar.Value;
		set
		{
			if(value > this.HorizScrollBar.Maximum)
			{
				value = this.HorizScrollBar.Maximum;
			}
			if(value < this.HorizScrollBar.Minimum)
			{
				value = this.HorizScrollBar.Minimum;
			}
			this.HorizScrollBar.Value = value;
			this.GridHScrolled(this.HorizScrollBar, new ScrollEventArgs(ScrollEventType.ThumbPosition, value));
		}
	}
	public void InvokeOnEnter()
	{
		this.OnEnter(EventArgs.Empty);
	}
	protected override bool ProcessDialogKey(Keys keyData)
	{
		try
	{
			if(FilterKeyData(keyData)) return false;
			return base.ProcessDialogKey (keyData);
		}
		catch
		{
			return false;
		}
	}
	protected override void OnKeyDown(KeyEventArgs e)
	{
		if(FilterKeyData(e.KeyData)) return;
		base.OnKeyDown (e);
	}
	protected override bool ProcessKeyPreview(ref Message m)
	{
		if (m.Msg == 0x100)
		{
			KeyEventArgs ke = new KeyEventArgs(((Keys) ((int) m.WParam)) | ModifierKeys);
			if(FilterKeyData(ke.KeyData)) return false;
		}
		return base.ProcessKeyPreview (ref m);
	}
	private bool FilterKeyData(Keys keyData)
	{
		return (keyData & Keys.KeyCode) == Keys.Space & (keyData & Keys.Shift) != Keys.None;
	}
	protected override void OnEnter(EventArgs e)
	{
		try
		{
			bool doEnter = true;
			if(this.FindForm() is AsForm form)
			{
				if(form.IsFiltering || form.FormGenerator.IgnoreDataChanges)
				{
					doEnter = false;
				}
				bool found = false;
				Control activeControl = form.ActiveControl;
				Control parentControl = this;
				while(parentControl != null)
				{
					if(parentControl.Equals(activeControl))
					{
						found = true;
						break;
					}
					parentControl = parentControl.Parent;
				}
				if(! found) doEnter = false;
			}
			if(doEnter)
			{
				if (this.ListManager == null) return;
				if(this.CurrentCell.RowNumber >= this.ListManager.Count)
				{
					if(this.ListManager.Count > 0)
					{
						this.CurrentRowIndex = this.ListManager.Count - 1;
					}
				}
				if (!EnhancedFocusControl) return;
				try
				{
					base.OnEnter (e);
				}
				finally
				{
//								form.EnteringGrid = null;
				}
			}
		}
		catch
		{
		}
	}
	protected override void OnLayout(LayoutEventArgs levent)
	{
		base.OnLayout (levent);
		if(! disposed & levent.AffectedControl == null & IgnoreLayoutEvent == false)
		{
			this.OnEnter(EventArgs.Empty);
		}
	}
	protected override void OnMouseDown(MouseEventArgs e)
	{
		HitTestInfo info = this.HitTest(e.X, e.Y);
		base.OnMouseDown (e);
		// Workaround: Sometimes the grid does not navigate to the right
		// column. In that case we just retry.
        if (this.CurrentCell.RowNumber != info.Row 
            || this.CurrentCell.ColumnNumber != info.Column)
        {
            this.CurrentCell = new DataGridCell(info.Row, info.Column);
        }
		if(e.Clicks > 1
			&& this.ListManager != null
			&& info.Type == HitTestType.ColumnResize
			&& this.TableStyles.Count == 1)
		{
			this.TableStyles[0].GridColumnStyles[info.Column].Width += 1;
		}
	}
	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove (e);
		AfterMouseMove?.Invoke(this, e);
	}
	protected override void Dispose(bool disposing)
	{
		if(disposing)
		{
			this.DataSource = null;
            this.tooltip.Dispose();
		}
		base.Dispose (disposing);
		disposed = true;
	}
	protected override void OnEnabledChanged(EventArgs e)
	{
		base.OnEnabledChanged (e);
		if(this.Enabled)
		{
			this.HorizScrollBar.Enabled = true; // Index zero is the horizontal scrollbar
			this.VertScrollBar.Enabled = true; // Index one is the vertical scrollbar
		}
	}
	public CurrencyManager CurrencyManager => this.ListManager;
	public void OnControlMouseWheel(MouseEventArgs e)
	{
		OnMouseWheel (e);
	}
	
}

internal class ClickTracker 
{
	private long lastGridClick;
	public event EventHandler SplitDoubleClikDetected;
	public void AddControl(IAsGridEditor gridEditor) {
		gridEditor.EditorClick += OnControlClicked;
	}
	
	public ClickTracker(AsDataGrid grid)
	{
		grid.MouseDown += OnGridOnMouseDown;
	}
	private void OnGridOnMouseDown(object sender, MouseEventArgs args)
	{
		lastGridClick = DateTime.UtcNow.Ticks;
	}
	private void OnControlClicked(object sender, EventArgs args)
	{
		long timeFromLastGridClick_ms 
			= (DateTime.UtcNow.Ticks - lastGridClick) / 10000;
		if (timeFromLastGridClick_ms < SystemInformation.DoubleClickTime)
		{
			SplitDoubleClikDetected?.Invoke(null, EventArgs.Empty);
		}
	}
}
