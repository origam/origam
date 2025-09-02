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

namespace Origam.Server;

public class PortalResultSession
{
    private Guid _formSessionId;
    private string _objectId;
    private bool _isDirty;
    private bool _askWorkflowClose;
    private string _type;
    private string _caption;
    private string _icon;

    public PortalResultSession(
        Guid formSessionId,
        string objectId,
        bool isDirty,
        UIRequestType type,
        string caption,
        string icon,
        bool askWorkflowClose
    )
    {
        _formSessionId = formSessionId;
        _objectId = objectId;
        _isDirty = isDirty;
        _type = type.ToString();
        _caption = caption;
        _icon = icon;
        _askWorkflowClose = askWorkflowClose;
    }

    public Guid FormSessionId
    {
        get { return _formSessionId; }
        set { _formSessionId = value; }
    }
    public string ObjectId
    {
        get { return _objectId; }
        set { _objectId = value; }
    }

    public bool IsDirty
    {
        get { return _isDirty; }
        set { _isDirty = value; }
    }
    public bool AskWorkflowClose
    {
        get { return _askWorkflowClose; }
        set { _askWorkflowClose = value; }
    }
    public string Type
    {
        get { return _type; }
        set { _type = value; }
    }
    public string Caption
    {
        get { return _caption; }
        set { _caption = value; }
    }
    public string Icon
    {
        get { return _icon; }
        set { _icon = value; }
    }
}
