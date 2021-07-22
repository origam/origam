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

namespace Origam.Gui.Win
{
	/// <summary>
	/// Summary description for DataGridColumnStyleHolder.
	/// </summary>
	public class DataGridColumnStyleHolder : IComparable
	{
		public DataGridColumnStyleHolder(DataGridColumnStyle style, int index, bool hidden)
		{
			_index = index;
			_style = style;
			_hidden = hidden;
		}

		public override string ToString()
		{
			return this.Index.ToString();
		}

		private int _index;
		public int Index
		{
			get
			{
				return _index;
			}
			set
			{
				_index = value;
			}
		}

		private bool _hidden = false;
		public bool Hidden
		{
			get
			{
				return _hidden;
			}
			set
			{
				_hidden = value;
			}
		}

		private DataGridColumnStyle _style;
		public DataGridColumnStyle Style
		{
			get
			{
				return _style;
			}
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			if(obj is DataGridColumnStyleHolder)
			{
				return _index.CompareTo((obj as DataGridColumnStyleHolder).Index) ;
			}

			throw new ArgumentException(ResourceUtils.GetString("ErrorNotColumnStyleHolder"));
		}

		#endregion
	}
}
