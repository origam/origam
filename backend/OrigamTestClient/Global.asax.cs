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
using System.Web;
using System.Web.SessionState;

namespace OrigamTestClient;
/// <summary>
/// Summary description for Global.
/// </summary>
public class Global : System.Web.HttpApplication
{
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.IContainer components = null;
	public Global()
	{
		InitializeComponent();
	}	
	
	protected void Application_Start(Object sender, EventArgs e)
	{
	}
	protected void Session_Start(Object sender, EventArgs e)
	{
	}
	protected void Application_BeginRequest(Object sender, EventArgs e)
	{
	}
	protected void Application_EndRequest(Object sender, EventArgs e)
	{
	}
	protected void Application_AuthenticateRequest(Object sender, EventArgs e)
	{
	}
	protected void Application_Error(Object sender, EventArgs e)
	{
	}
	protected void Session_End(Object sender, EventArgs e)
	{
	}
	protected void Application_End(Object sender, EventArgs e)
	{
	}
		
	#region Web Form Designer generated code
	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	private void InitializeComponent()
	{    
		this.components = new System.ComponentModel.Container();
	}
	#endregion
}
