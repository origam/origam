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

using System.Collections.Generic;
using System.Data;

namespace Origam.Server
{
    public class EntityExportInfo
    {
        private string _entity;
        private List<EntityExportField> _fields = new List<EntityExportField>();
        private List<object> _rowIds = new List<object>();
        private string _sessionFormIdentifier;
        private SessionStore _ss;

        public ILazyRowLoadInput LazyLoadedEntityInput { get; set; }
        
        public string Entity
        {
            get { return _entity; }
            set { _entity = value; }
        }

        public List<EntityExportField> Fields
        {
            get { return _fields; }
            set { _fields = value; }
        }

        public List<object> RowIds
        {
            get { return _rowIds; }
            set { _rowIds = value; }
        }

        public string SessionFormIdentifier
        {
            get { return _sessionFormIdentifier; }
            set { _sessionFormIdentifier = value; }
        }

        public DataTable Table
        {
            get 
            {
                return Store.GetDataTable(Entity, DataSource);
            }
        }

        public DataSet DataSource
        {
            get
            {
                if (Store.IsPagedLoading 
					&& Store.DataListEntity == Entity)
                {
                    return Store.DataList;
                }
                else
                {
                    return Store.Data;
                }
            }
        }

        public SessionStore Store
        {
            get { return _ss; }
            set { _ss = value; }
        }
    }
}
