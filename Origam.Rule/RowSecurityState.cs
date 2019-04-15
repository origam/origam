#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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

using System.Collections;

namespace Origam.Rule
{
	/// <summary>
	/// Summary description for RowSecurityState.
	/// </summary>
	public class RowSecurityState
	{
		object _id;
		int _backgroundColor;
		int _foregroundColor;
		bool _allowDelete;
		bool _allowCreate;
		ArrayList _relations = new ArrayList();
		ArrayList _columns = new ArrayList();
		ArrayList _disabledActions = new ArrayList();

		public RowSecurityState(object id, int backgroundColor, int foregroundColor, bool allowDelete, bool allowCreate)
		{
			_id = id;
			_backgroundColor = backgroundColor;
			_foregroundColor = foregroundColor;
			_allowDelete = allowDelete;
			_allowCreate = allowCreate;
		}

		public object Id 
		{
			get
			{
				return _id;
			}
			set
			{
				_id = value;
			}
		}

		public int BackgroundColor
		{
			get
			{
				return _backgroundColor;
			}
			set
			{
				_backgroundColor = value;
			}
		}

		public int ForegroundColor
		{
			get
			{
				return _foregroundColor;
			}
			set
			{
				_foregroundColor = value;
			}
		}

		public bool AllowDelete
		{
			get
			{
				return _allowDelete;
			}
			set
			{
				_allowDelete = value;
			}
		}

		public bool AllowCreate
		{
			get
			{
				return _allowCreate;
			}
			set
			{
				_allowCreate = value;
			}
		}

		public ArrayList Columns
		{
			get
			{
				return _columns;
			}
		}

		public ArrayList Relations
		{
			get
			{
				return _relations;
			}
		}

		public ArrayList DisabledActions
		{
			get
			{
				return _disabledActions;
			}
            set
            {
                _disabledActions = value;
            }
		}
	}
}
