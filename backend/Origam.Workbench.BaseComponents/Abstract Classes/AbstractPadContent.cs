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
using WeifenLuo.WinFormsUI.Docking;
using Origam.UI;

namespace Origam.Workbench;

public class AbstractPadContent : DockContent, IPadContent
{
	string title;
	string icon;
	string category = null;
	string[] shortcut = null;
		
	public AbstractPadContent() {}

	public AbstractPadContent(string title) : this(title, null)
	{
		}
		
	public AbstractPadContent(string title, string iconResoureName)
	{
			this.title = title;
			this.icon  = iconResoureName;
		}
		
	public string Category 
	{
		get 
		{
				return category;
			}
		set 
		{
				category = value;
			}
	}
		
	public string[] Shortcut 
	{
		get 
		{
				return shortcut;
			}
		set 
		{
				shortcut = value;
			}
	}
		
	public virtual string Title 
	{
		get 
		{
				return this.Name;
			}
	}
		
	public virtual string IconResource
	{
		get 
		{
				return icon;
			}
	}

	public virtual void RedrawContent()
	{
		}
		
	protected virtual void OnTitleChanged(EventArgs e)
	{
			if (TitleChanged != null) 
			{
				TitleChanged(this, e);
			}
		}
		
	protected virtual void OnIconChanged(EventArgs e)
	{
			if (IconChanged != null) 
			{
				IconChanged(this, e);
			}
		}

	private void InitializeComponent()
	{
			// 		// AbstractPadContent
			// 		this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(292, 273);
			this.DockAreas = ((DockAreas)(((((DockAreas.Float | DockAreas.DockLeft) 
				| DockAreas.DockRight) 
				| DockAreas.DockTop) 
				| DockAreas.DockBottom)));
			this.Name = "AbstractPadContent";

		}
		
	public event EventHandler TitleChanged;
	public event EventHandler IconChanged;
		
	public void BringPadToFront()
	{
//			if (!WorkbenchSingleton.Workbench.WorkbenchLayout.IsVisible(this)) 
//			{
//				WorkbenchSingleton.Workbench.WorkbenchLayout.ShowPad(this);
//			}
//			WorkbenchSingleton.Workbench.WorkbenchLayout.ActivatePad(this);
		}
}