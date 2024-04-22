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

namespace Origam.Server;

public class HelpTooltip
{
    private string _id;
    public string Id
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

    private string _context;
    public string Context
    {
        get
        {
                return _context;
            }
        set
        {
                _context = value;
            }
    }

    private string _subContext;
    public string SubContext
    {
        get
        {
                return _subContext;
            }
        set
        {
                _subContext = value;
            }
    }

    private string _relatedComponent;
    public string RelatedComponent
    {
        get
        {
                return _relatedComponent;
            }
        set
        {
                _relatedComponent = value;
            }
    }

    private string _objectId;
    public string ObjectId
    {
        get
        {
                return _objectId;
            }
        set
        {
                _objectId = value;
            }
    }

    private string _text;
    public string Text
    {
        get
        {
                return _text;
            }
        set
        {
                _text = value;
            }
    }

    private int _position;
    public int Position
    {
        get
        {
                return _position;
            }
        set
        {
                _position = value;
            }
    }

    private int _destroyCondition;
    public int DestroyCondition
    {
        get
        {
                return _destroyCondition;
            }
        set
        {
                _destroyCondition = value;
            }
    }

    private string _destroyParameter;
    public string DestroyParameter
    {
        get
        {
                return _destroyParameter;
            }
        set
        {
                _destroyParameter = value;
            }
    }
}