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
using Origam.Schema;
using System.Xml;

namespace Origam.Server
{
    public class DashboardToolboxItemParameter
    {
        public DashboardToolboxItemParameter()
        {
        }

        public DashboardToolboxItemParameter(string name, string caption, OrigamDataType dataType, Guid lookupId)
        {
            _name = name;
            _caption = caption;
            _dataType = dataType;
            _lookupId = lookupId;
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
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

        private Guid _lookupId;
        public Guid LookupId
        {
            get
            {
                return _lookupId;
            }
            set
            {
                _lookupId = value;
            }
        }

        private OrigamDataType _dataType;
        public OrigamDataType DataType
        {
            get
            {
                return _dataType;
            }
            set
            {
                _dataType = value;
            }
        }

        private XmlDocument _controlDefinition;
        public XmlDocument ControlDefinition
        {
            get
            {
                return _controlDefinition;
            }
            set
            {
                _controlDefinition = value;
            }
        }
    }
}
