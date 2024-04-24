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

namespace Origam.Workbench
{
	public enum GlobalTransactionPointType
	{
		Form = 0,
		Workflow = 1,
		Attachment = 2
	}

	public class GlobalTransactionPointEventArgs : EventArgs
	{
		private GlobalTransactionPoint _point;

		public GlobalTransactionPointEventArgs(GlobalTransactionPoint point)
		{
			this.Point = point;
		}

		public GlobalTransactionPoint Point
		{
			get
			{
				return _point;
			}
			set
			{
				_point = value;
			}
		}
	}

	/// <summary>
	/// Summary description for GlobalTransactionPoint.
	/// </summary>
	public class GlobalTransactionPoint
	{
		public GlobalTransactionPoint(GlobalTransactionPointType type, DateTime time, string description, string savePointName)
		{
			this.Type = type;
			this.Time = time;
			this.Description = description;
			this.SavePointName = savePointName;
		}

		public string SavePointName;
		public DateTime Time;
		public string Description;
		public GlobalTransactionPointType Type;
	}
}
