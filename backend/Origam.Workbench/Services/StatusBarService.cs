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

namespace Origam.Workbench.Services;
/// <summary>
/// Summary description for StatusBarService.
/// </summary>
public class StatusBarService : IStatusBarService
{
	public StatusBarService(StatusBar statusBar)
	{
	    this.statusBar = statusBar;
	}
	#region IService Members
	public event System.EventHandler Unload;
	public event System.EventHandler Initialize;
	public void UnloadService()
	{
		OnUnload(EventArgs.Empty);
	}
	public void InitializeService()
	{
		OnInitialize(EventArgs.Empty);
	}
	protected void OnInitialize(EventArgs e)
	{
		if (Initialize != null) 
		{
			Initialize(this, e);
		}
	}
	
	protected void OnUnload(EventArgs e)
	{
		if (Unload != null) 
		{
			Unload(this, e);
		}
	}
	#endregion
	#region Properties
	private StatusBar statusBar;
	#endregion
	#region Public Functions
    delegate void SetStatusTextDelegate(string text);
    public void SetStatusText(string text)
	{
		if(CanSetStatus())
		{
            if (this.statusBar.InvokeRequired)
            {
                SetStatusTextDelegate setText = new SetStatusTextDelegate(SetStatusText);
                this.statusBar.Invoke(setText, new object[] { text }); 
            }
            else
            {
                this.statusBar.Panels[0].Text = text;
            }
		}
	}
	public void SetStatusMemory(long bytes)
	{
		if(CanSetStatus())
		{
			this.statusBar.Panels[1].Text = bytes.ToString("N");
			// Application.DoEvents();
		}
	}
	#endregion
	private bool CanSetStatus()
	{
		return ! (this.statusBar == null || this.statusBar.IsDisposed);
	}
}
