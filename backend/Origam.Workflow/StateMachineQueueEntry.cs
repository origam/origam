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
using System.Data;
using System.Collections;

namespace Origam.Workflow;
/// <summary>
/// Summary description for StateMachineQueueEntry.
/// </summary>
public class StateMachineQueueEntry
{
	private DataRow _row;
	private ArrayList _stateColumns;
	private Guid _entityId;
	public StateMachineQueueEntry(DataRow row, ArrayList stateColumns, Guid entityId)
	{
		_row = row;
		_stateColumns = stateColumns;
		_entityId = entityId;
	}
	public Guid EntityId
	{
		get
		{
			return _entityId;
		}
		set
		{
			_entityId = value;
		}
	}
	public DataRow Row
	{
		get
		{
			return _row;
		}
		set
		{
			_row = value;
		}
	}
	public ArrayList StateColumns
	{
		get
		{
			return _stateColumns;
		}
		set
		{
			_stateColumns = value;
		}
	}
}
