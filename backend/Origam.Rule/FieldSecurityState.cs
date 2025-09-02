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

namespace Origam.Rule;

/// <summary>
/// Summary description for ColumnSecurityState.
/// </summary>
public class FieldSecurityState
{
    string _name;
    bool _allowUpdate;
    bool _allowRead;
    string _dynamicLabel;
    int _backgroundColor;
    int _foregroundColor;

    public FieldSecurityState(
        string name,
        bool allowUpdate,
        bool allowRead,
        string dynamicLabel,
        int backgroundColor,
        int foregroundColor
    )
    {
        _name = name;
        _allowUpdate = allowUpdate;
        _allowRead = allowRead;
        _dynamicLabel = dynamicLabel;
        _backgroundColor = backgroundColor;
        _foregroundColor = foregroundColor;
    }

    public string Name
    {
        get { return _name; }
        set { _name = value; }
    }
    public string DynamicLabel
    {
        get { return _dynamicLabel; }
        set { _dynamicLabel = value; }
    }
    public int BackgroundColor
    {
        get { return _backgroundColor; }
        set { _backgroundColor = value; }
    }
    public int ForegroundColor
    {
        get { return _foregroundColor; }
        set { _foregroundColor = value; }
    }
    public bool AllowUpdate
    {
        get { return _allowUpdate; }
        set { _allowUpdate = value; }
    }
    public bool AllowRead
    {
        get { return _allowRead; }
        set { _allowRead = value; }
    }
}
