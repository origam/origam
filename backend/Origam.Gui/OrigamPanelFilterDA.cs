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
using System.Data;

using Origam.DA;
using Origam.Workbench.Services;

namespace Origam.Gui;
/// <summary>
/// Summary description for OrigamPanelFilterDA.
/// </summary>
public class OrigamPanelFilterDA
{
	public static OrigamPanelFilter LoadFilters(Guid panelId)
	{
		IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);
        UserProfile profile = SecurityManager.CurrentUserProfile();
		// retrieve filters for the current panel from the database
		DataStructureQuery query = new DataStructureQuery(
			new Guid("210dc168-891d-4597-a6c0-cc19fee45b4b"),
			new Guid("847ed860-8f9a-4cc4-bc1c-ba18fb183d13"));
		query.Parameters.Add(new QueryParameter("PanelFilter_parPanelId", panelId));
		query.Parameters.Add(new QueryParameter("PanelFilter_parProfileId", profile.Id));
		dataServiceAgent.MethodName = "LoadDataByQuery";
		dataServiceAgent.Parameters.Clear();
		dataServiceAgent.Parameters.Add("Query", query);
		dataServiceAgent.Run();
		OrigamPanelFilter storedFilters = new OrigamPanelFilter();
		storedFilters.Merge(dataServiceAgent.Result as DataSet);
		return storedFilters;
	}
	public static void PersistFilter(OrigamPanelFilter filter)
	{
		IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);
		// persist filters to the database
		DataStructureQuery query = new DataStructureQuery(
			new Guid("210dc168-891d-4597-a6c0-cc19fee45b4b"));
		dataServiceAgent.MethodName = "StoreDataByQuery";
		dataServiceAgent.Parameters.Clear();
		dataServiceAgent.Parameters.Add("Query", query);
		dataServiceAgent.Parameters.Add("Data", filter);
		dataServiceAgent.Run();
	}
	public static OrigamPanelFilter LoadFilter(Guid id)
	{
		IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);
		OrigamPanelFilter result = new OrigamPanelFilter();
		DataStructureQuery query = new DataStructureQuery(
			new Guid("210dc168-891d-4597-a6c0-cc19fee45b4b"),
			new Guid("30a60305-a260-4dd3-a2f3-830e7ab1d1ec")
			);
		query.Parameters.Add(new QueryParameter("PanelFilter_parId", id));
		dataServiceAgent.MethodName = "LoadDataByQuery";
		dataServiceAgent.Parameters.Clear();
		dataServiceAgent.Parameters.Add("Query", query);
		dataServiceAgent.Run();
		result.Merge(dataServiceAgent.Result as DataSet);
		return result;
	}
	public static object StoredFilterValue(OrigamPanelFilter.PanelFilterDetailRow row, Type type, int valueNumber)
	{
		if(type == typeof(bool))
		{
			return StoredFilterValue(row, "BoolValue", valueNumber);
		}
		else if(type == typeof(int) | type == typeof(decimal) | type == typeof(long) | type == typeof(float))
		{
			return StoredFilterValue(row, "CurrencyValue", valueNumber);
		}
		else if(type == typeof(Guid) || type == typeof(string))
		{
			object result = StoredFilterValue(row, "GuidValue", valueNumber);
			// try string value if there is no guid
			if(result == null)
			{
				result = StoredFilterValue(row, "StringValue", valueNumber);
			}
			return result;
		}
		else if(type == typeof(DateTime))
		{
			return StoredFilterValue(row, "DateValue", valueNumber);
		}
		else
		{
			return StoredFilterValue(row, "StringValue", valueNumber);
		}
	}
	public static void AddPanelFilterDetailRow(OrigamPanelFilter filterDS, Guid profileId, Guid filterId, string columnName, int oper, object value1, object value2)
	{
		OrigamPanelFilter.PanelFilterDetailRow detail =  filterDS.PanelFilterDetail.NewPanelFilterDetailRow();
		detail.Id = Guid.NewGuid();
		detail.RecordCreated = DateTime.Now;
		detail.RecordCreatedBy = profileId;
		detail.refOrigamPanelFilterId = filterId;
		detail.ColumnName = columnName;
		detail.Operator = oper;
		//////// VALUE 1 ////////////////
		if(value1 is bool)
		{
			detail.BoolValue = (bool)value1; 
		}
		else if(value1 is Guid)
		{
			detail.GuidValue = (Guid)value1;
		}
		else if(value1 is DateTime)
		{
			detail.DateValue = (DateTime)value1;
		}
		else if(value1 is int | value1 is float | value1 is decimal | value1 is long)
		{
			detail.CurrencyValue = Convert.ToDecimal(value1);
		}
		else if(value1 != null)
		{
			detail.StringValue = value1.ToString();
		}
		//////// VALUE 2 ////////////////
		if(value2 is bool)
		{
			detail.BoolValue2 = (bool)value2; 
		}
		else if(value2 is Guid)
		{
			detail.GuidValue2 = (Guid)value2;
		}
		else if(value2 is DateTime)
		{
			detail.DateValue2 = (DateTime)value2;
		}
		else if(value2 is int | value2 is float | value2 is decimal | value2 is long)
		{
			detail.CurrencyValue2 = Convert.ToDecimal(value2);
		}
		else if(value2 != null)
		{
			detail.StringValue2 = value2.ToString();
		}
		filterDS.PanelFilterDetail.Rows.Add(detail);
	}
	private static object StoredFilterValue(OrigamPanelFilter.PanelFilterDetailRow row, string columnName, int valueNumber)
	{
		string col =  (valueNumber == 1 ? columnName : columnName + valueNumber.ToString());
		if(row.IsNull(col))
		{
			return null;
		}
		else
		{
			return row[col];
		}
	}
}
