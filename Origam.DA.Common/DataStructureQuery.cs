#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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
using System.Collections.Generic;
using System.Data;

namespace Origam.DA
{
	public enum QueryDataSourceType
	{
		DataStructure = 0,
		DataStructureEntity = 1
	}

	/// <summary>
	/// This class is used to reference data structure with filters when asking IDataService for data.
	/// </summary>
	public class DataStructureQuery
	{
		public DataStructureQuery()
		{
		}

		public DataStructureQuery(Guid dataStructureId)
		{
			this.DataSourceId = dataStructureId;
		}

		public DataStructureQuery(Guid dataStructureId, Guid methodId)
		{
			this.DataSourceId = dataStructureId;
			this.MethodId = methodId;
		}

		public DataStructureQuery(Guid dataStructureId, Guid methodId, Guid defaultSetId, Guid sortSetId)
		{
			this.DataSourceId = dataStructureId;
			this.MethodId = methodId;
			this.DefaultSetId = defaultSetId;
			this.SortSetId = sortSetId;
		}

	    public DataStructureQuery(Guid dataStructureId, string entityName, string customFilters, 
	        List<Tuple<string, string>> customOrdering)
	    {
	        DataSourceId = dataStructureId;
	        Entity = entityName;
	        CustomFilters = customFilters;
	        CustomOrdering = customOrdering;
	    }

        public System.Guid DataSourceId;
		public System.Guid MethodId;
		public Guid DefaultSetId;
		public Guid SortSetId;
		public QueryParameterCollection Parameters = new QueryParameterCollection();
		public IsolationLevel IsolationLevel = IsolationLevel.ReadCommitted;

	    public List<Tuple<string, string>> CustomOrdering { get; }
	    public string CustomFilters { get;  }

        public bool Paging
		{
			get
			{
				bool pageSizeFound = false;
				bool pageNumberFound = false;
				foreach(QueryParameter param in Parameters)
				{
					if(param.Name == "_pageSize")
					{
						pageSizeFound = true;
					}
					else if(param.Name == "_pageNumber")
					{
						pageNumberFound = true;
					}
				}

				return pageSizeFound && pageNumberFound;
			}
		}

		private QueryDataSourceType _dataSourceType = QueryDataSourceType.DataStructure;
		public QueryDataSourceType DataSourceType
		{
			get
			{
				return _dataSourceType;
			}
			set
			{
				_dataSourceType = value;
			}
		}

		private bool _loadActualValuesAfterUpdate = false;
		public bool LoadActualValuesAfterUpdate
		{
			get
			{
				return _loadActualValuesAfterUpdate;
			}
			set
			{
				_loadActualValuesAfterUpdate = value;
			}
		}

		private bool _loadByIdentity = true;
		public bool LoadByIdentity
		{
			get
			{
				return _loadByIdentity;
			}
			set
			{
				_loadByIdentity = value;
			}
		}

		private bool _fireStateMachineEvents = true;
		public bool FireStateMachineEvents
		{
			get
			{
				return _fireStateMachineEvents;
			}
			set
			{
				_fireStateMachineEvents = value;
			}
		}

        private bool _synchronizeAttachmentsOnDelete = true;
        public bool SynchronizeAttachmentsOnDelete
        {
            get
            {
                return _synchronizeAttachmentsOnDelete;
            }
            set
            {
                _synchronizeAttachmentsOnDelete = value;
            }
        }
        
        private bool _enforceConstraints = true;
		public bool EnforceConstraints
		{
			get
			{
				return _enforceConstraints;
			}
			set
			{
				_enforceConstraints = value;
			}
		}

        private string _columnName;
        public string ColumnName
        {
            get
            {
                return _columnName;
            }
            set
            {
                _columnName = value;
            }
        }

        private string _entity;
	    public string Entity
        {
            get
            {
                return _entity;
            }
            set
            {
                _entity = value;
            }
        }
    }
}
