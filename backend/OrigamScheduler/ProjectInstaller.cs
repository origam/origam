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

using System.ComponentModel;

namespace OrigamScheduler;

/// <summary>
/// Summary description for ProjectInstaller.
/// </summary>
[RunInstaller(true)]
public class ProjectInstaller : System.Configuration.Install.Installer
{
	private System.ServiceProcess.ServiceProcessInstaller serviceProcessInstaller1;
	private System.ServiceProcess.ServiceInstaller serviceInstaller1;
	/// <summary>
	/// Required designer variable.
	/// </summary>
	private System.ComponentModel.Container components = null;

	public ProjectInstaller()
	{
		// This call is required by the Designer.
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
		this.serviceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
		this.serviceInstaller1 = new System.ServiceProcess.ServiceInstaller();
		// 
		// serviceProcessInstaller1
		// 
		this.serviceProcessInstaller1.Password = null;
		this.serviceProcessInstaller1.Username = null;
		// 
		// serviceInstaller1
		// 
		this.serviceInstaller1.DisplayName = "ORIGAM Scheduler Service";
		this.serviceInstaller1.ServiceName = "OrigamSchedulerService";
		// 
		// ProjectInstaller
		// 
		this.Installers.AddRange(new System.Configuration.Install.Installer[] {
			this.serviceProcessInstaller1,
			this.serviceInstaller1});

	}
	#endregion
}