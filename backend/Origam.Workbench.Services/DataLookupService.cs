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
using System.Text;
using System.Data;
using System.Collections;
using System.Security.Principal;
using Origam.DA;
using Origam.Services;
using Origam.UI;
using Origam.Schema;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using System.Collections.Generic;
using System.Linq;
using Origam.DA.Service;
using log4net;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Workbench.Services;
public class ParameterizedEventArgs : EventArgs
{
	public ParameterizedEventArgs()
	{
	}
	public IOrigamForm SourceForm;
	public readonly Dictionary<string, object> Parameters = new ();
}
/// <summary>
/// Summary description for LookupManager.
/// </summary>
public class DataLookupService : IDataLookupService
{
	public const string SCHEMA_LOOKUP_ID = "3396e71f-6ee9-4c1d-8fad-739822c8df96";
	private static readonly ILog log =
		LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
	private enum QueryType
	{
		List,
		Value,
		ValueCacheList
	}
	private Hashtable _valueCache = new Hashtable();
	public DataLookupService()
	{
	}
	#region Properties
//		private IDataService _dataService;
//		public IDataService DataService
//		{
//			get
//			{
//				return _dataService;
//			}
//			set
//			{
//				_dataService = value;
//			}
//		}
	#endregion
	
	public DataView GetList(Guid lookupId, string transactionId)
	{
		return GetList(lookupId, new Dictionary<string, object>(), transactionId);
	}
	public DataView GetList(Guid lookupId, Dictionary<string, object> parameters, string transactionId)
	{
		IServiceAgent dataServiceAgent = GetAgent();
		DataServiceDataLookup lookup = GetLookup(lookupId) as DataServiceDataLookup;
		DataStructureQuery query = GetQuery(lookup, QueryType.List);
		foreach(var entry in parameters)
		{
			query.Parameters.Add(new QueryParameter(entry.Key, entry.Value));
		}
		dataServiceAgent.TransactionId = transactionId;
		dataServiceAgent.MethodName = "LoadDataByQuery";
		dataServiceAgent.Parameters.Clear();
		dataServiceAgent.Parameters.Add("Query", query);
		dataServiceAgent.Run();
		DataSet data = dataServiceAgent.Result as DataSet;
		DataView view = new DataView(data.Tables[0]);
		PostProcessLookupList(lookup, view.Table);
		view.Table.AcceptChanges();
		return view;
	}
	public object GetDisplayText(Guid lookupId, object lookupValue, string transactionId)
	{
		return GetDisplayText(lookupId, lookupValue, true, true, transactionId);
	}
	public object GetDisplayText(Guid lookupId, object lookupValue, bool useCache, bool returnMessageIfNull, string transactionId)
	{
		if(lookupValue == DBNull.Value | lookupValue == null)
		{
			return "";
		}
		else
		{
			Hashtable parameters = new Hashtable(1);
			DataServiceDataLookup lookup = GetLookup(lookupId) as DataServiceDataLookup;
			if(lookup.ValueMethod != null)
			{
				var keys = lookup.ValueMethod.ParameterReferences.Keys;
				foreach(string parameterName in keys)
				{
					parameters.Add(parameterName, lookupValue);
				}
			}
			return GetDisplayText(lookupId, parameters, useCache, returnMessageIfNull, transactionId);
		}
	}
	public object GetDisplayText(Guid lookupId, Dictionary<string, object> parameters, bool useCache, bool returnMessageIfNull, string transactionId)
	{
		string internalTransactionId = transactionId;
		if(parameters == null) throw new NullReferenceException(ResourceUtils.GetString("ErrorParametersNull"));
		bool canUseCache = (parameters.Count == 1 & useCache);
		object cachableValue = null;
		object val = null;
		DataServiceDataLookup lookup = GetLookup(lookupId) as DataServiceDataLookup;
		if(canUseCache)
		{
			foreach(var entry in parameters)
			{
				cachableValue = entry.Value;
			}
			if(_valueCache.ContainsKey(lookupId))
			{
				Hashtable cache = _valueCache[lookupId] as Hashtable;
				if(cache.ContainsKey(cachableValue))
				{
					return cache[cachableValue];
				}
			}
			else
			{
				lock(_valueCache)
				{
					_valueCache.Add(lookupId, new Hashtable());
					OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
				
					// fill cache with all values
					if(settings.UseProgressiveCaching)
					{
						IServiceAgent dataServiceAgent = GetAgent();
						DataStructureQuery cacheQuery = GetQuery(lookup, QueryType.ValueCacheList);
						dataServiceAgent.MethodName = "LoadDataByQuery";
						dataServiceAgent.Parameters.Clear();
						dataServiceAgent.Parameters.Add("Query", cacheQuery);
						dataServiceAgent.TransactionId = internalTransactionId;
						dataServiceAgent.Run();
						DataSet data = dataServiceAgent.Result as DataSet;
						string[] columns = lookup.ValueDisplayMember.Split(";".ToCharArray());
						foreach(DataRow row in data.Tables[0].Rows)
						{
							object currentVal = ValueFromRow(row, columns);
							object id = row[lookup.ValueValueMember];
							(_valueCache[lookupId] as Hashtable).Add(id, currentVal);
							if(id.Equals(cachableValue))
							{
								val = currentVal;
							}
						}
						if(val != null) return val;
					}
				}
			}
		}
		if(lookup.ValueDisplayMember.IndexOf(".") > -1) return "wrong display member";
		DataStructureQuery query = GetQuery(lookup, QueryType.Value);
		//if(lookup.ValueFilterSet == null) throw new NullReferenceException("ValueFilterSet cannot be null. Cannot get display text for lookup '" + lookup.Name + "'");
		foreach(var parameter in parameters)
		{
			query.Parameters.Add(new QueryParameter(parameter.Key, parameter.Value));
		}
		bool error = false;
		try
		{
			if(lookup.ValueDisplayMember.IndexOf(";") > 0)
			{
				IServiceAgent dataServiceAgent = GetAgent();
				dataServiceAgent.MethodName = "LoadDataByQuery";
				dataServiceAgent.Parameters.Clear();
				dataServiceAgent.Parameters.Add("Query", query);
				dataServiceAgent.TransactionId = internalTransactionId;
				dataServiceAgent.Run();
				DataSet data = dataServiceAgent.Result as DataSet;
				string[] columns = lookup.ValueDisplayMember.Split(";".ToCharArray());
				if(data.Tables[0].Rows.Count == 0)
				{
					val = null;
				}
				else
				{
					val = ValueFromRow(data.Tables[0].Rows[0], columns);
				}
			}
			else
			{
				IServiceAgent dataServiceAgent = GetAgent();
				dataServiceAgent.MethodName = "GetScalarValueByQuery";
				dataServiceAgent.Parameters.Clear();
				dataServiceAgent.Parameters.Add("Query", query);
				dataServiceAgent.Parameters.Add("ColumnName", lookup.ValueDisplayMember);
				dataServiceAgent.TransactionId = internalTransactionId;
				dataServiceAgent.Run();
				object result = dataServiceAgent.Result;
				if(result == null)
				{
					val = null;
				}
				else
				{
					val = result;
				}
			}
		}
		catch(System.Threading.ThreadAbortException)
		{
		}
		catch(Exception ex)
		{
			if(returnMessageIfNull)
			{
				val = ex.Message;
				error = true;
			}
			else
			{
				throw new OrigamException(ResourceUtils.GetString("ErrorGetLookupText", lookupId.ToString()), ex);
			}
		}
		if(val == null & returnMessageIfNull) 
		{
			string parameterString = "";
			foreach(var parameter in parameters)
			{
				if(parameterString != "") parameterString += ", ";
				parameterString += parameter.Key + ": " + parameter.Value;
			}
			val = "Záznam nedostupný (" + parameterString + ")";
		}
		else
		{
			if(canUseCache & error == false)
			{
				if(!(_valueCache[lookupId] as Hashtable).Contains(cachableValue))
				{
					lock(_valueCache[lookupId])
					{
						(_valueCache[lookupId] as Hashtable).Add(cachableValue, val);
					}
				}
			}
		}
		return val;
	}
	public string ValueFromRow(DataRow row, string[] columns)
	{
		StringBuilder resultBuilder = new StringBuilder();
		foreach(string column in columns)
		{
			object columnValue = row[column];
			if(columnValue != DBNull.Value && columnValue.ToString() != "")
			{
				if(resultBuilder.Length > 0)
				{
					resultBuilder.Append(", ");
				}
				DateTime dateValue = DateTime.MinValue;
				if(columnValue is DateTime) dateValue = (DateTime)columnValue;
				if(columnValue is DateTime &&
					(
					dateValue.Hour == 0
					& dateValue.Minute == 0
					& dateValue.Second == 0
					)
					)
				{
					resultBuilder.Append(dateValue.ToShortDateString());
				}
				else
				{
					resultBuilder.Append(columnValue.ToString());
				}
			}
		}
		return resultBuilder.ToString();
	}
	public object LinkTarget(ILookupControl lookupControl, object value)
	{
		DataLookupMenuBinding binding = GetMenuBindingElement(GetLookup(lookupControl.LookupId), value);
		if(binding != null)
		{
			return binding.MenuItem;
		}
		else
		{
			return null;
		}
	}
	
	public Dictionary<string, object> LinkParameters(object linkTarget, object value)
	{
		var result = new Dictionary<string, object>();
		AbstractMenuItem menu = linkTarget as AbstractMenuItem;
		if(menu != null)
		{
			if(menu is FormReferenceMenuItem)
			{
				FormReferenceMenuItem formRef = menu as FormReferenceMenuItem;
				if(formRef.RecordEditMethod == null)
				{
					result.Add("Id", value);
				}
				else
				{
					foreach(var entry in (menu as FormReferenceMenuItem).RecordEditMethod.ParameterReferences)
					{
						result.Add(entry.Key, value);
					}
				}
			}		
		}
		return result;
	}
	#region Private Methods
	private IServiceAgent GetAgent()
	{
		return (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);
	}
	/// <summary>
	/// Returns lookup schema item
	/// </summary>
	/// <param name="lookupId"></param>
	/// <returns></returns>
	public AbstractDataLookup GetLookup(Guid lookupId)
	{
		ModelElementKey key = new ModelElementKey(lookupId);
		IPersistenceService persistence = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		AbstractDataLookup lookup = persistence.SchemaProvider.RetrieveInstance(typeof(AbstractDataLookup), key) as AbstractDataLookup;
		if (lookup == null)
		{
			throw new OrigamException(
				string.Format("Couldn't find a lookup with id {0}.", lookupId));
		}
		return lookup;
	}
	private DataStructureQuery GetQuery(AbstractDataLookup lookup, QueryType queryType)
	{
		DataServiceDataLookup dataLookup = lookup as DataServiceDataLookup;
		if(dataLookup != null)
		{
			switch(queryType)
			{
				case QueryType.List:
					return new DataStructureQuery(dataLookup.ListDataStructureId, dataLookup.ListDataStructureMethodId, Guid.Empty, dataLookup.ListDataStructureSortSetId);
				case QueryType.Value:
					if(dataLookup.ValueDataStructureMethodId == null)
					{ log.Warn("DataLookup has no ValueDataStructureMethodId !!"); }
					return new DataStructureQuery(dataLookup.ValueDataStructureId, dataLookup.ValueDataStructureMethodId, Guid.Empty, dataLookup.ValueDataStructureSortSetId);
				case QueryType.ValueCacheList:
					return new DataStructureQuery(dataLookup.ValueDataStructureId);
			}
		}
		throw new ArgumentOutOfRangeException(ResourceUtils.GetString("ErrorUnknownLookupType"));
	}
	private bool HasEditListMenuBinding(AbstractDataLookup lookup)
	{
		IOrigamAuthorizationProvider authorizationProvider = SecurityManager.GetAuthorizationProvider();
		IPrincipal principal = SecurityManager.CurrentPrincipal;
		
		foreach(DataLookupMenuBinding binding in lookup.MenuBindings)
		{
			if(
				binding.SelectionLookup == null 
				&& AuthorizeMenuBinding(authorizationProvider, principal, binding)
				)
			{
				return true;
			}
		}
		return false;
	}
	private bool HasEditRecordMenuBinding(AbstractDataLookup lookup)
	{
		IOrigamAuthorizationProvider authorizationProvider = SecurityManager.GetAuthorizationProvider();
		foreach(DataLookupMenuBinding binding in lookup.MenuBindings)
		{
			if(AuthorizeMenuBinding(authorizationProvider, SecurityManager.CurrentPrincipal, binding))
			{
				return true;
			}
		}
		return false;
	}
	private bool HasMenuBindingWithSelection(AbstractDataLookup lookup)
	{
		IOrigamAuthorizationProvider authorizationProvider = SecurityManager.GetAuthorizationProvider();
		IPrincipal principal = SecurityManager.CurrentPrincipal;
		
		foreach(DataLookupMenuBinding binding in lookup.MenuBindings)
		{
			if(binding.SelectionLookup != null && AuthorizeMenuBinding(authorizationProvider, principal, binding))
			{
				return true;
			}
		}
		return false;
	}
	public bool HasMenuBindingWithSelection(Guid lookupId)
	{
		IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		AbstractDataLookup lookup = (AbstractDataLookup)ps.SchemaProvider.RetrieveInstance(typeof(AbstractDataLookup), new ModelElementKey(lookupId));
		return HasMenuBindingWithSelection(lookup);
	}
	public IMenuBindingResult GetMenuBinding(Guid lookupId, object value)
	{
		IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
		AbstractDataLookup lookup = (AbstractDataLookup)ps.SchemaProvider.RetrieveInstance(typeof(AbstractDataLookup), new ModelElementKey(lookupId));
        DataLookupMenuBinding binding = GetMenuBindingElement(lookup, value);
        if (binding == null)
        {
            return new MenuBindingResult();
        }
        else
        {
            return new MenuBindingResult(binding.MenuItemId.ToString(), binding.SelectionPanelId);
        }
	}
	public NewRecordScreenBinding GetNewRecordScreenBinding(AbstractDataLookup lookup)
	{
		return lookup.ChildItems
			.ToGeneric()
			.OfType<NewRecordScreenBinding>()
			.FirstOrDefault(x => x.IsAvailable);
	}
	public DataLookupMenuBinding GetMenuBindingElement(AbstractDataLookup lookup, object value)
	{
		IOrigamAuthorizationProvider authorizationProvider = SecurityManager.GetAuthorizationProvider();
		IPrincipal principal = SecurityManager.CurrentPrincipal;
		Hashtable selectionCache = new Hashtable();
        ArrayList menuBindings = lookup.MenuBindings;
        menuBindings.Sort();
		
		foreach(DataLookupMenuBinding binding in menuBindings)
		{
			// get list
			if(value == null && binding.SelectionLookup == null && AuthorizeMenuBinding(authorizationProvider, principal, binding))
			{
				return binding;
			}
			// get record - without selection
			else if(
				HasMenuBindingWithSelection(lookup) == false
				&& binding.SelectionLookup == null 
				&& AuthorizeMenuBinding(authorizationProvider, principal, binding)
				)
			{
				return binding;
			}
			// get record - with selection
			else if(
				value != null
				&& binding.SelectionLookup != null
				&& AuthorizeMenuBinding(authorizationProvider, principal, binding)
				)
			{
				if(! selectionCache.Contains(binding.SelectionLookupId))
				{
					selectionCache[binding.SelectionLookupId] = GetDisplayText(binding.SelectionLookupId, value, false, false, null);
				}
				IParameterService paramService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
				
				if(binding.SelectionConstant == null)
				{
					// if anything was found by the lookup, this is the right menu
					if(selectionCache[binding.SelectionLookupId] != null)
					{
						return binding;
					}
				}
				else
				{
					// if specific value was found by the lookup, this is the right menu
					object paramValue = paramService.GetParameterValue(binding.SelectionConstantId);
					if(selectionCache[binding.SelectionLookupId] != null && selectionCache[binding.SelectionLookupId].Equals(paramValue))
					{
						return binding;
					}
				}
			}
		}
		return null;
	}
    public bool AuthorizeMenuBinding(IOrigamAuthorizationProvider authorizationProvider, IPrincipal principal, DataLookupMenuBinding binding)
	{
		IParameterService param = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
		return(authorizationProvider.Authorize(principal, binding.AuthorizationContext)
				&& authorizationProvider.Authorize(principal, binding.MenuItem.AuthorizationContext)
				&& param.IsFeatureOn(binding.MenuItem.Features)
			);
	}
	#endregion
	#region Lookup Control Event Handlers
	private void lookupControl_LookupDisplayTextRequested(object sender, EventArgs e)
	{
		ILookupControl control = sender as ILookupControl;
		control.LookupDisplayText = GetDisplayText(control.LookupId, control.LookupValue, null).ToString();
	}
	public DataTable GetList(LookupListRequest request)
	{
		DataServiceDataLookup lookup = GetLookup(request.LookupId) as DataServiceDataLookup;
		DataStructureQuery query = GetQuery(lookup, QueryType.List);
		if(request.CurrentRow != null)
		{
			// set parameters
            foreach (DictionaryEntry entry in DatasetTools.RetrieveParemeters(
                request.ParameterMappings,
                new List<DataRow>{ request.CurrentRow })) 
			{
				query.Parameters.Add(new QueryParameter((string)entry.Key, entry.Value));
			}
		}
        if(lookup.IsFilteredServerside)
        {
            if(lookup.ListMethod == null)
            {
                throw new ArgumentNullException("ListMethod", "Lookup is defined as IsFilteredServerside but ListMethod is not set.");
            }
            if(lookup.ServersideFilterParameter == null)
            {
                throw new ArgumentNullException("ServersideFilterParameter", "Lookup is defined as IsFilteredServerside but ServersideFilterParameter is not set.");
            }
            query.Parameters.Add(
				new QueryParameter(lookup.ServersideFilterParameter, request.SearchText));
			if(request.PageNumber != -1)
			{
				query.Parameters.Add(
					new QueryParameter("_pageSize", request.PageSize));
				query.Parameters.Add(
					new QueryParameter("_pageNumber", request.PageNumber));
			}
        }
		IServiceAgent dataServiceAgent = GetAgent();
		dataServiceAgent.TransactionId = request.TransactionId;
		dataServiceAgent.MethodName = "LoadDataByQuery";
		dataServiceAgent.Parameters.Clear();
		dataServiceAgent.Parameters.Add("Query", query);
		dataServiceAgent.Run();
		DataSet data = dataServiceAgent.Result as DataSet;
		DataTable lookupTable = data.Tables[0];
		IStateMachineService stateMachine = ServiceManager.Services.GetService(typeof(IStateMachineService)) as IStateMachineService;
		object originalValue = DBNull.Value;
		if(request.CurrentRow != null)		// only go to state machine evaluation, if the control can give us current row context
		{
			Guid entityId = (Guid)request.CurrentRow.Table.ExtendedProperties["EntityId"];
			Guid fieldId = (Guid)request.CurrentRow.Table.Columns[request.FieldName].ExtendedProperties["Id"];
			switch(request.CurrentRow.RowState)
			{
				case DataRowState.Detached:
					break;
				case DataRowState.Added:
					break;
				case DataRowState.Modified:
					originalValue = request.CurrentRow[request.FieldName, DataRowVersion.Original];
					break;
				default:
					originalValue = request.CurrentRow[request.FieldName, DataRowVersion.Current];
					break;
			}
			object[] allowedStates = stateMachine.AllowedStateValues(
				entityId, fieldId, originalValue, request.CurrentRow, null);
			if(allowedStates != null)
			{
				// this is a state machine, so we remove all non-allowed states from the list
				foreach(DataRow row in lookupTable.Rows)
				{
					object rowValue = row[lookup.ListValueMember];
					bool found = false;
					foreach(object allowedValue in allowedStates)
					{
						if(allowedValue.Equals(rowValue))
						{
							found = true;
							break;
						}
					}
					if(! found)
					{
						row.Delete();
					}
				}
			}
		}
		PostProcessLookupList(lookup, lookupTable);
		// filter unique values
		if(request.ShowUniqueValues)
		{
			foreach(DataRow row in lookupTable.Rows)
			{
				if(request.CurrentRow != null)
				{
					if(request.CurrentRow.Table.Select(request.FieldName + "= '" + row[lookup.ValueValueMember] + "'").GetLength(0) > 0)
					{
						row.Delete();
					}
				}
			}
		}
		lookupTable.AcceptChanges();
		return lookupTable;
	}
	private void PostProcessLookupList(DataServiceDataLookup lookup, DataTable lookupTable)
	{
		// filter roles
		if(lookup.RoleFilterMember != "" & lookup.RoleFilterMember != null)
		{
			if(! lookupTable.Columns.Contains(lookup.RoleFilterMember))
			{
				throw new ArgumentOutOfRangeException("RoleFilterMember", lookup.RoleFilterMember, ResourceUtils.GetString("ErrorNoSuchColumn"));
			}
			IOrigamAuthorizationProvider authorizationProvider = SecurityManager.GetAuthorizationProvider();
			foreach(DataRow row in lookupTable.Rows)
			{
				object context = row[lookup.RoleFilterMember];
				if(! authorizationProvider.Authorize(SecurityManager.CurrentPrincipal, context == DBNull.Value ? null : (string)context))
				{
					row.Delete();
				}	
			}
		}
		// filter features
		if(lookup.FeatureFilterMember != "" & lookup.FeatureFilterMember != null)
		{
			if(! lookupTable.Columns.Contains(lookup.FeatureFilterMember))
			{
				throw new ArgumentOutOfRangeException("FeatureFilterMember", lookup.FeatureFilterMember, ResourceUtils.GetString("ErrorNoSuchColumn"));
			}
			IParameterService param = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
			foreach(DataRow row in lookupTable.Rows)
			{
				if(! param.IsFeatureOn(lookup.FeatureFilterMember))
				{
					row.Delete();
				}	
			}
		}
	}
	private void lookupControl_LookupListRefreshRequested(object sender, EventArgs e)
	{
		ILookupControl control = sender as ILookupControl;
		OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
		if( ! settings.UseProgressiveCaching)
		{
			// clear cache
			if(_valueCache.ContainsKey(control.LookupId))
			{
				_valueCache.Remove(control.LookupId);
			}
			// refresh current text
			lookupControl_LookupDisplayTextRequested(sender, e);
		}
		DataTable lookupTable = GetList(
			ILookupControlToLookupListRequest(control));
		DataView view = new DataView(lookupTable);
		control.LookupList = view;
	}
	private LookupListRequest ILookupControlToLookupListRequest(ILookupControl control)
	{
		LookupListRequest request = new LookupListRequest();
		request.LookupId = control.LookupId;
		request.FieldName = control.ColumnName;
		request.ParameterMappings = control.ParameterMappingsHashtable;
		request.CurrentRow = control.CurrentRow;
		request.ShowUniqueValues = control.ShowUniqueValues;
        request.SearchText = ""; // control.SearchText.Replace("*", "%");
		return request;
	}
	#endregion
	#region IService Members
	public void UnloadService()
	{
		_valueCache.Clear();
	}
	public void InitializeService()
	{
		// TODO:  Add LookupManager.InitializeService implementation
	}
    public object CreateRecord(Guid lookupId, Dictionary<string, object> values, string transactionId)
    {
        Guid newId = Guid.NewGuid();
        var lookup = GetLookup(lookupId);
        DatasetGenerator dg = new DatasetGenerator(true);
        DataSet data = dg.CreateDataSet(lookup.ListDataStructure);
        Schema.EntityModel.DataStructureEntity entity = 
            lookup.ListDataStructure.Entities[0] as Schema.EntityModel.DataStructureEntity;
        if (!entity.AllFields)
        {
            throw new Exception("Data structure entity " + entity.Path + " has to have AllFields = true in order to create new records through lookups.");
        }
        DataTable table = data.Tables[entity.Name];
        var row = table.NewRow();
        row[lookup.ListValueMember] = newId;
        DatasetTools.UpdateOrigamSystemColumns(row, true, SecurityManager.CurrentUserProfile().Id);
        foreach (var item in values)
        {
            string columnName = (string)item.Key;
            if (!table.Columns.Contains(columnName))
            {
                throw new ArgumentOutOfRangeException("columnName", columnName, "Field not found in the ListDataStructure of lookup " + lookup.Path);
            }
            Type type = table.Columns[columnName].DataType;
            object value = DBNull.Value;
            if (item.Value != null)
            {
                if (type == typeof(Guid))
                {
                    value = new Guid(item.Value.ToString());
                }
                else
                {
                    value = Convert.ChangeType(item.Value, type);
                }
            }
            row[item.Key] = value;
        }
        table.Rows.Add(row);
        DataService.Instance.StoreData(lookup.ListDataStructureId, data, false, transactionId);
        return newId;
    }
    #endregion
    public void RemoveFromCache(Guid id)
    {
        if (_valueCache.Contains(id))
        {
            _valueCache.Remove(id);
        }
    }
}
