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
using System.Text;

namespace Origam.Workbench
{
	public class LogPadStreamWriter : TextWriter
	{
		Pads.LogPad _output;
		StringBuilder _buffer = new StringBuilder();
		char[] _newLineTest = new char[2];
		char[] _newLine = Environment.NewLine.ToCharArray();

		public LogPadStreamWriter(Pads.LogPad output)
		{
			_output = output;
		}

		public override void Write(char value)
		{
			_newLineTest[0] = _newLineTest[1];
			_newLineTest[1] = value;
			_buffer.Append(value);
			
			if(_newLine[0].Equals(_newLineTest[0]) && _newLine[1].Equals(_newLineTest[1]))
			{
				_output.AddText(_buffer.ToString());
				_buffer = new StringBuilder();
			}
		}

		public override Encoding Encoding
		{
			get { return Encoding.UTF8; }
		}
	}
}