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
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Origam.Workbench;
using Origam.UI;

[assembly: log4net.Config.XmlConfigurator(Watch=true)]

namespace OrigamArchitect;
/// <summary>
/// Summary description for MainClass.
/// </summary>
public class MainClass
{
	private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	/// <summary>
	/// The main entry point for the application.
	/// </summary>
	[STAThread]
	static void Main(string[] args) 
	{
		log.Info("ORIGAM Desktop starting.");
		Application.ThreadException += Application_ThreadException;
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
		try
		{
			Application.EnableVisualStyles();
			Application.DoEvents();
		}
		catch{}
		try
		{
#if !ORIGAM_CLIENT
			Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
			Thread.CurrentThread.CurrentCulture =  new CultureInfo("en-US");
#endif				
			frmMain form = new frmMain();
			WorkbenchSingleton.Workbench = form;
			if(args.Length > 0 && args[0].ToUpper() == "/A")
			{
				form.AdministratorMode = true;
			}
			
			Application.Run(WorkbenchSingleton.Workbench as Form);
		}
		catch(Exception ex)
		{
			HandleUnhandledException(ex);
		}
	}
	private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
	{
		HandleUnhandledException(e.Exception);
	}
	private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
	{
		HandleUnhandledException(e.ExceptionObject);
	}
	private static void HandleUnhandledException(object o)
	{
		Exception ex = o as Exception;
        if (ex == null)
		{
		    log.Fatal(o);
            AsMessageBox.ShowError(null, o.ToString(), strings.GenericError_Title, null);
		}
		else
		{
		    log.Fatal("HandleUnhandledException", ex);
            if (ex.StackTrace.IndexOf("System.Windows.Forms.ParkingWindow") > 0)
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}
			else
			{
				AsMessageBox.ShowError(null, ex.Message,strings.GenericError_Title, ex);
			}
		}
	}
}
