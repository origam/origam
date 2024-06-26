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
using System.Collections.Generic;
using Origam.Schema.GuiModel;

namespace Origam.Server;
public class DashboardToolboxItem
{
    private Guid _id;
    public Guid Id
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
    private string _caption;
    public string Caption
    {
        get
        {
            return _caption;
        }
        set
        {
            _caption = value;
        }
    }
    private DashboardToolboxItemType _type = DashboardToolboxItemType.Component;
    public DashboardToolboxItemType Type
    {
        get
        {
            return _type;
        }
        set
        {
            _type = value;
        }
    }
    IList<DashboardToolboxItem> _childWidgets = new List<DashboardToolboxItem>();
    public IList<DashboardToolboxItem> ChildWidgets
    {
        get
        {
            return _childWidgets;
        }
    }
    IList<DashboardToolboxItemParameter> _parameters = new List<DashboardToolboxItemParameter>();
    public IList<DashboardToolboxItemParameter> Parameters
    {
        get
        {
            return _parameters;
        }
    }
    private IList<DashboardWidgetProperty> _properties = new List<DashboardWidgetProperty>();
    public IList<DashboardWidgetProperty> Properties
    {
        get
        {
            return _properties;
        }
    }
}
