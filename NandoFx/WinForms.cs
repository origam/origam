#region  NandoF library -- © 2006 Nando Florestan
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

namespace NandoF {
	
	#region using
	//using System.Data;
	using System.Windows.Forms;
	using Exception     = System.Exception;
	using StringBuilder = System.Text.StringBuilder;
	using StreamReader  = System.IO.StreamReader;
	using StreamWriter  = System.IO.StreamWriter;
	using Console       = System.Console;
	using Trace         = System.Diagnostics.Trace;
	using Regex         = System.Text.RegularExpressions.Regex;
	#endregion
	
	/// <summary>This class contains common helper code to be reused in
	/// Windows.Forms projects.</summary>
	public class WinForms
	{
		/// <summary>Asks the operating system to launch a file or
		/// document in a new process</summary>
		/// <param name="FileOrDoc">The application or document to be launched</param>
		/// <param name="Arguments">Arguments to be passed to the new app</param>
		public static void ShellLaunch(string fileOrDoc, string arguments) {
			System.Diagnostics.Process proc = new System.Diagnostics.Process();
			proc.EnableRaisingEvents=false;
			proc.StartInfo.FileName=fileOrDoc;
			proc.StartInfo.Arguments=arguments;
			try {proc.Start();}
			catch {}
		}
		
		/// <summary>Assembles a title to be displayed in the title bar of an
		/// application, using Application.ProductName etc.</summary>
		/// <param name="Suffix">Text after app name and version.
		/// Example: "beta 2".</param>
		/// <param name="FileName">Name of currently open file; null if none</param>
		public static string BuildWindowTitle(string suffix, string fileName) {
			StringBuilder sb = new StringBuilder(256);
			sb.Append(Application.ProductName);
			sb.Append(" ");
			sb.Append(Application.ProductVersion.Substring(0,3));
			if (suffix != null) sb.Append(" " + suffix);
			// If a file was loaded, show filename too (without directory)
			if (fileName != null) {
				sb.Append(" - " + System.IO.Path.GetFileName(fileName));
			}
			return sb.ToString();
		}
		
		/// <summary>A general error handler that displays the error message in
		/// a message box.</summary>
		/// <param name="x">A System.Exception object</param>
		public static void ShowError(System.Exception x) {
			MessageBox.Show(x.Message, "Error", MessageBoxButtons.OK,
			                MessageBoxIcon.Warning,
			                MessageBoxDefaultButton.Button1);
		}
		
		/// <summary>Another general error handler that displays the error message
		/// in a status bar after showing the message box.</summary>
		/// <param name="x">A System.Exception object</param>
		/// <param name="StatBar">A Windows.Forms.StatusBar object</param>
		public static void ShowError(System.Exception x, StatusBar statBar) {
			ShowError(x);
			statBar.Text = x.Message;
		}
		
//		/// <summary>
//		/// Crummy solution for finding out which is the current row in a DataGrid.
//		/// TODO: Bug: If user does column sorting, this chooses incorrect row.
//		/// </summary>
//		/// <param name="grid">The DataGrid to examine</param>
//		/// <returns>A DataRow object</returns>
//		public static DataRow GetCurrentRowFromDataGrid(DataGrid grid) {
//			DataGridCell myCell = grid.CurrentCell;
//			DataTable myTable;
//			// Assumes the DataGrid is bound to a DataTable.
//			if (grid.DataSource.GetType() == typeof(DataTable)) {
//				myTable = (DataTable)grid.DataSource;
//				return myTable.Rows[myCell.RowNumber];
//			}
//			else throw new Exception
//				("Method GetCurrentRowFromDataGrid needs an implementation for " +
//				 grid.DataSource.GetType().ToString());
//		}
	}
	
	/*
	public class SingletonForms
	{
		// Keeps references to Windows forms and allows only one of each to be open
		static System.Collections.ArrayList cache
			= new System.Collections.ArrayList();
		
		static public System.Windows.Forms.Form Show(System.Type formType)
		{
			// Only one instance can be created of this class, SingletonForms
			// if (instance==null) instance = new SingletonForms();
			
			// Lets try to open a windows form of the given type
			foreach(object cachedObj in cache) {
				if (cachedObj.GetType() != formType) continue;
				// Now we have found the cached form, so try to open it
				System.Windows.Forms.MessageBox.Show("found it");
				System.Windows.Forms.Form f = (System.Windows.Forms.Form)cachedObj;
				try {
					f.Show();
					f.BringToFront();
				}
				catch (System.ObjectDisposedException) {
					try {
						cache.Remove(cachedObj);
						object newObj = System.Activator.CreateInstance(formType);
						f = (System.Windows.Forms.Form)newObj;
						f.Show();
					}
					catch (Exception x) {
						MessageBox.Show(x.ToString(), "Could not open the window",
						                MessageBoxButtons.OK, MessageBoxIcon.Hand);
						return null;
					}
				}
				cache.Add(f);
				return f;
			}
			// If the form does not yet exist in the cache
			object obj = System.Activator.CreateInstance(formType);
			System.Windows.Forms.Form frm = (System.Windows.Forms.Form)obj;
			cache.Add(frm);
			frm.Show();
			return frm;
		}
	}
	*/
}   // end namespace
