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

public class Attachment
{
    private string _id;
    public string Id
    {
        get { return _id; }
        set { _id = value; }
    }
    private string _fileName;
    public string FileName
    {
        get { return _fileName; }
        set { _fileName = value; }
    }
    private string _description;
    public string Description
    {
        get { return _description; }
        set { _description = value; }
    }
    private DateTime _dateCreated;
    public DateTime DateCreated
    {
        get { return _dateCreated; }
        set { _dateCreated = value; }
    }
    private string _icon;
    public string Icon
    {
        get { return _icon; }
        set { _icon = value; }
    }
    private string _creatorName;
    public string CreatorName
    {
        get { return _creatorName; }
        set { _creatorName = value; }
    }
}
