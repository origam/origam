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

#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Threading;

using Origam.Workflow;

namespace Origam.Gui;
public class WorkflowCallbackHandler
{
    private Guid _workflowInstanceId;
    private WorkflowHost _host;
    private ManualResetEvent _manualEvent = new ManualResetEvent(false);
    private WorkflowHostEventArgs _result;
    public WorkflowCallbackHandler(WorkflowHost host, Guid workflowInstanceId)
    {
        _host = host;
        _workflowInstanceId = workflowInstanceId;
    }
    public Guid WorkflowInstanceId
    {
        get { return _workflowInstanceId; }
        set { _workflowInstanceId = value; }
    }
    public ManualResetEvent Event
    {
        get { return _manualEvent; }
    }
    public WorkflowHostEventArgs Result
    {
        get { return _result; }
    }
    public WorkflowHost Host
    {
        get { return _host; }
        set { _host = value; }
    }
    public void Subscribe()
    {
        this.Host.FormRequested += new WorkflowHostFormEvent(Host_FormRequested);
        this.Host.WorkflowFinished += new WorkflowHostEvent(Host_WorkflowFinished);
        this.Host.WorkflowMessage += new WorkflowHostMessageEvent(Host_WorkflowMessage);
    }
    void Host_WorkflowMessage(object sender, WorkflowHostMessageEventArgs e)
    {
        if(e.Engine.WorkflowInstanceId == this.WorkflowInstanceId)
        {
            if(e.Popup)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("WorkflowMessage - Popup: " + e.Message);
#endif
                this.Host.WorkflowMessage -= new WorkflowHostMessageEvent(Host_WorkflowMessage);
                _result = e;
                _manualEvent.Set();
            }
            else
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("WorkflowMessage - No popup: " + e.Message);
#endif
            }
        }
    }
    void Host_WorkflowFinished(object sender, WorkflowHostEventArgs e)
    {
        if(e.Engine.WorkflowInstanceId == this.WorkflowInstanceId)
        {
            if(e.Engine.CallingWorkflow == null)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine("WorkflowFinished");
#endif
                this.Host.WorkflowFinished -= new WorkflowHostEvent(Host_WorkflowFinished);
                _result = e;
                _manualEvent.Set();
            }
        }
    }
    void Host_FormRequested(object sender, WorkflowHostFormEventArgs e)
    {
        if(e.Engine.WorkflowInstanceId == this.WorkflowInstanceId)
        {
#if DEBUG
            System.Diagnostics.Debug.WriteLine("FormRequested");
#endif
            this.Host.FormRequested -= new WorkflowHostFormEvent(Host_FormRequested);
            _result = e;
            _manualEvent.Set();
        }
    }
}
