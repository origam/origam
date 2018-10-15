#region  NandoF library -- Copyright 2005-2006 Nando Florestan
/*
This library is free software; you can redistribute it and/or modify
it under the terms of the Lesser GNU General Public License as published by
the Free Software Foundation; either version 2.1 of the License, or
(at your option) any later version.

This software is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License
along with this program; if not, see http://www.gnu.org/copyleft/lesser.html
 */
#endregion

using Environment           = System.Environment;
using Exception             = System.Exception;
using System.Windows.Forms;

namespace NandoF
{
	/// <summary>Helps separating the user interface from other layers.
	/// When implemented by any kind of user interface, allows it
	/// to display information coming from the core of the application,
	/// through these callbacks.</summary>
	/// <remarks>This is especially useful when your app core has a lengthy
	/// operation to do -- for instance, processing many files. Then these
	/// callbacks help the user interface display information.</remarks>
	public interface IView
	{
		void Show(string s);
		void Show(Exception x);
	}
	
	/// <summary>This component is a configurable implementation of IView
	/// for Windows.Forms apps, allowing you to plug controls in order to
	/// easily show text in several ways.</summary>
	public class WinFormsView : IView
	{
		public WinFormsView(StatusBar statBar, string exceptionLogFile)  {
			StatBar = statBar;
			this.exceptionLogFile = exceptionLogFile;
		}
		public WinFormsView()  {}
		
		public StatusBar StatBar;
		public TextBox   statusTextBox;
		public string    exceptionLogFile;
		public bool      MsgBoxOnExceptions = true;
		
		public void Show(string s) {
			if (StatBar != null)  StatBar.Text = s;
			if (statusTextBox != null)  {
				statusTextBox.Text += s + Environment.NewLine;
				statusTextBox.Select(statusTextBox.Text.Length, 0);
				statusTextBox.ScrollToCaret();
			}
			Application.DoEvents();
		}
		
		public void Show(Exception x)  {
			Show(x.Message);
			string xs = x.ToString();
			if (MsgBoxOnExceptions)
				MessageBox.Show(xs, "Exception",
				                MessageBoxButtons.OK, MessageBoxIcon.Error);
			if (exceptionLogFile != null)
				Filez.WriteTextToFile(xs, exceptionLogFile, true);
		}
	}
}
