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
using System.Collections.Generic;
using System.Data;
using System.Linq;

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
			DataSourceId = dataStructureId;
		}

		public DataStructureQuery(Guid dataStructureId, Guid methodId)
		{
			DataSourceId = dataStructureId;
			MethodId = methodId;
		}

		public DataStructureQuery(Guid dataStructureId, Guid methodId, Guid defaultSetId, Guid sortSetId)
		{
			DataSourceId = dataStructureId;
			MethodId = methodId;
			DefaultSetId = defaultSetId;
			SortSetId = sortSetId;
		}

	    public int RowLimit { get; set; }
	    public int RowOffset { get; set; }
	    public Guid DataSourceId;
		public Guid MethodId;
		public Guid DefaultSetId;
		public Guid SortSetId;
		public QueryParameterCollection Parameters = new QueryParameterCollection();
		public IsolationLevel IsolationLevel = IsolationLevel.ReadCommitted;

	    public CustomOrderings CustomOrderings { get; set; }
	    public CustomFilters CustomFilters { get; set; }

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

	    public QueryDataSourceType DataSourceType { get; set; } = QueryDataSourceType.DataStructure;

	    public bool LoadActualValuesAfterUpdate { get; set; } = false;

	    public bool LoadByIdentity { get; set; } = true;

	    public bool FireStateMachineEvents { get; set; } = true;

	    public bool SynchronizeAttachmentsOnDelete { get; set; } = true;

	    public bool EnforceConstraints { get; set; } = true;
	    
	    public ColumnsInfo ColumnsInfo { get; set; } = ColumnsInfo.Empty;
	    public bool Distinct { get; set; }
	    public string Entity { get; set; }
	    public bool ForceDatabaseCalculation { get; set; }
	    public Grouping CustomGrouping { get; set; }
	    public List<Aggregation> AggregatedColumns { get; set; }
	    
	    public List<ColumnData> GetAllQueryColumns()
	    {
		    var aggregationColData =
			    (AggregatedColumns ?? new List<Aggregation>())
			    .Select(x => new ColumnData(x.SqlQueryColumnName));
		    return ColumnsInfo.Columns
			    .Concat(aggregationColData)
			    .ToList();
	    }
	}

	public class CustomFilters
	{
		private string filters = "";

		public string Filters
		{
			get => filters;
			set => filters = string.IsNullOrWhiteSpace(value) ? "" : value;
		}

		public Dictionary<string, Guid> FilterLookups { get; set; } = new Dictionary<string, Guid>();
		public bool IsEmpty => string.IsNullOrWhiteSpace(Filters);
		public bool HasLookups => FilterLookups != null && FilterLookups.Count > 0;
	}

	public class CustomOrderings
	{
		public List<Ordering> Orderings { get; }

		public bool IsEmpty => Orderings == null || Orderings.Count == 0;
		public CustomOrderings(List<Ordering> orderings)
		{
			Orderings = orderings;
			FilterLookups = orderings == null 
				? new Dictionary<string, Guid>() 
				: orderings
					.Where(ordering => ordering.LookupId != Guid.Empty)
					.ToDictionary(
						ordering => ordering.ColumnName, 
						ordering => ordering.LookupId);
		}

		public Dictionary<string, Guid> FilterLookups { get; } = new Dictionary<string, Guid>();
		
		public bool HasLookups => FilterLookups != null && FilterLookups.Count > 0;
	}
}
