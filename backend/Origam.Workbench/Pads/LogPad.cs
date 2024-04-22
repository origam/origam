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
using System.IO;

namespace Origam.Workbench.Pads;

/// <summary>
/// Summary description for LogPad.
/// </summary>
public class LogPad : OutputPad
{
	public LogPad() : base()
	{
			// Instantiate the writer 		TextWriter _writer = new LogPadStreamWriter(this); 
			// Redirect the out Console stream 		Console.SetOut(_writer);
			this.TabText = "Log";
			this.Text = "Log";
		}

	private void InitializeComponent()
	{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LogPad));
            this.SuspendLayout();
            // 	 // LogPad
            // 	 this.ClientSize = new System.Drawing.Size(352, 271);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LogPad";
            this.ResumeLayout(false);
            this.PerformLayout();

        }
}