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

namespace Origam.Workflow;
public delegate void WorkflowHostMessageEvent(object sender, WorkflowHostMessageEventArgs e);
/// <summary>
/// Summary description for WorkflowHostMessageEventArgs.
/// </summary>
public class WorkflowHostMessageEventArgs : WorkflowHostEventArgs
{
	private string _message;
	private bool _popup = true;
	public WorkflowHostMessageEventArgs(WorkflowEngine engine, string message, Exception exception, bool popup) : base(engine, exception)
	{
		_message = message;
		_popup = popup;
	}
	public string Message
	{
		get
		{
			return _message;
		}
		set
		{
			_message = value;
		}
	}
	public bool Popup
	{
		get
		{
			return _popup;
		}
		set
		{
			_popup = value;
		}
	}
}
