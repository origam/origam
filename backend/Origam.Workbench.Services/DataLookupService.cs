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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Text;
using log4net;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.LookupModel;
using Origam.Schema.MenuModel;
using Origam.Service.Core;
using Origam.Services;
using Origam.UI;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Workbench.Services;

public class ParameterizedEventArgs : EventArgs
{
    public ParameterizedEventArgs() { }

    public IOrigamForm SourceForm;
    public readonly Dictionary<string, object> Parameters = new();
}

/// <summary>
/// Summary description for LookupManager.
/// </summary>
public class DataLookupService : IDataLookupService
{
    public const string SCHEMA_LOOKUP_ID = "3396e71f-6ee9-4c1d-8fad-739822c8df96";
    private static readonly ILog log = LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    private enum QueryType
    {
        List,
        Value,
        ValueCacheList,
    }

    private Hashtable _valueCache = new Hashtable();

    public DataLookupService() { }
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
        return GetList(
            lookupId: lookupId,
            parameters: new Dictionary<string, object>(),
            transactionId: transactionId
        );
    }

    public DataView GetList(
        Guid lookupId,
        Dictionary<string, object> parameters,
        string transactionId
    )
    {
        IServiceAgent dataServiceAgent = GetAgent();
        DataServiceDataLookup lookup = GetLookup(lookupId: lookupId) as DataServiceDataLookup;
        DataStructureQuery query = GetQuery(lookup: lookup, queryType: QueryType.List);
        foreach (var entry in parameters)
        {
            query.Parameters.Add(
                value: new QueryParameter(_parameterName: entry.Key, value: entry.Value)
            );
        }
        dataServiceAgent.TransactionId = transactionId;
        dataServiceAgent.MethodName = "LoadDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: query);
        dataServiceAgent.Run();
        DataSet data = dataServiceAgent.Result as DataSet;
        DataView view = new DataView(table: data.Tables[index: 0]);
        PostProcessLookupList(lookup: lookup, lookupTable: view.Table);
        view.Table.AcceptChanges();
        return view;
    }

    public object GetDisplayText(Guid lookupId, object lookupValue, string transactionId)
    {
        return GetDisplayText(
            lookupId: lookupId,
            lookupValue: lookupValue,
            useCache: true,
            returnMessageIfNull: true,
            transactionId: transactionId
        );
    }

    public object GetDisplayText(
        Guid lookupId,
        object lookupValue,
        bool useCache,
        bool returnMessageIfNull,
        string transactionId
    )
    {
        if (lookupValue == DBNull.Value | lookupValue == null)
        {
            return "";
        }
        var parameters = new Dictionary<string, object>();
        DataServiceDataLookup lookup = GetLookup(lookupId: lookupId) as DataServiceDataLookup;
        if (lookup.ValueMethod != null)
        {
            var keys = lookup.ValueMethod.ParameterReferences.Keys;
            foreach (string parameterName in keys)
            {
                parameters.Add(key: parameterName, value: lookupValue);
            }
        }

        return GetDisplayText(
            lookupId: lookupId,
            parameters: parameters,
            useCache: useCache,
            returnMessageIfNull: returnMessageIfNull,
            transactionId: transactionId
        );
    }

    public object GetDisplayText(
        Guid lookupId,
        Dictionary<string, object> parameters,
        bool useCache,
        bool returnMessageIfNull,
        string transactionId
    )
    {
        string internalTransactionId = transactionId;
        if (parameters == null)
        {
            throw new NullReferenceException(
                message: ResourceUtils.GetString(key: "ErrorParametersNull")
            );
        }

        bool canUseCache = (parameters.Count == 1 & useCache);
        object cachableValue = null;
        object val = null;
        DataServiceDataLookup lookup = GetLookup(lookupId: lookupId) as DataServiceDataLookup;
        if (canUseCache)
        {
            foreach (var entry in parameters)
            {
                cachableValue = entry.Value;
            }
            if (_valueCache.ContainsKey(key: lookupId))
            {
                Hashtable cache = _valueCache[key: lookupId] as Hashtable;
                if (cache.ContainsKey(key: cachableValue))
                {
                    return cache[key: cachableValue];
                }
            }
            else
            {
                lock (_valueCache)
                {
                    _valueCache.Add(key: lookupId, value: new Hashtable());
                    OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();

                    // fill cache with all values
                    if (settings.UseProgressiveCaching)
                    {
                        IServiceAgent dataServiceAgent = GetAgent();
                        DataStructureQuery cacheQuery = GetQuery(
                            lookup: lookup,
                            queryType: QueryType.ValueCacheList
                        );
                        dataServiceAgent.MethodName = "LoadDataByQuery";
                        dataServiceAgent.Parameters.Clear();
                        dataServiceAgent.Parameters.Add(key: "Query", value: cacheQuery);
                        dataServiceAgent.TransactionId = internalTransactionId;
                        dataServiceAgent.Run();
                        DataSet data = dataServiceAgent.Result as DataSet;
                        string[] columns = lookup.ValueDisplayMember.Split(
                            separator: ";".ToCharArray()
                        );
                        foreach (DataRow row in data.Tables[index: 0].Rows)
                        {
                            object currentVal = ValueFromRow(row: row, columns: columns);
                            object id = row[columnName: lookup.ValueValueMember];
                            (_valueCache[key: lookupId] as Hashtable).Add(
                                key: id,
                                value: currentVal
                            );
                            if (id.Equals(obj: cachableValue))
                            {
                                val = currentVal;
                            }
                        }
                        if (val != null)
                        {
                            return val;
                        }
                    }
                }
            }
        }
        if (lookup.ValueDisplayMember.IndexOf(value: ".") > -1)
        {
            return "wrong display member";
        }

        DataStructureQuery query = GetQuery(lookup: lookup, queryType: QueryType.Value);
        //if(lookup.ValueFilterSet == null) throw new NullReferenceException("ValueFilterSet cannot be null. Cannot get display text for lookup '" + lookup.Name + "'");
        foreach (var parameter in parameters)
        {
            query.Parameters.Add(
                value: new QueryParameter(_parameterName: parameter.Key, value: parameter.Value)
            );
        }
        bool error = false;
        try
        {
            if (lookup.ValueDisplayMember.IndexOf(value: ";") > 0)
            {
                IServiceAgent dataServiceAgent = GetAgent();
                dataServiceAgent.MethodName = "LoadDataByQuery";
                dataServiceAgent.Parameters.Clear();
                dataServiceAgent.Parameters.Add(key: "Query", value: query);
                dataServiceAgent.TransactionId = internalTransactionId;
                dataServiceAgent.Run();
                DataSet data = dataServiceAgent.Result as DataSet;
                string[] columns = lookup.ValueDisplayMember.Split(separator: ";".ToCharArray());
                if (data.Tables[index: 0].Rows.Count == 0)
                {
                    val = null;
                }
                else
                {
                    val = ValueFromRow(row: data.Tables[index: 0].Rows[index: 0], columns: columns);
                }
            }
            else
            {
                IServiceAgent dataServiceAgent = GetAgent();
                dataServiceAgent.MethodName = "GetScalarValueByQuery";
                dataServiceAgent.Parameters.Clear();
                dataServiceAgent.Parameters.Add(key: "Query", value: query);
                dataServiceAgent.Parameters.Add(
                    key: "ColumnName",
                    value: lookup.ValueDisplayMember
                );
                dataServiceAgent.TransactionId = internalTransactionId;
                dataServiceAgent.Run();
                object result = dataServiceAgent.Result;
                if (result == null)
                {
                    val = null;
                }
                else
                {
                    val = result;
                }
            }
        }
        catch (System.Threading.ThreadAbortException) { }
        catch (RuleException)
        {
            throw;
        }
        catch (Exception ex)
        {
            if (returnMessageIfNull)
            {
                val = ex.Message;
                error = true;
            }
            else
            {
                throw new OrigamException(
                    message: ResourceUtils.GetString(
                        key: "ErrorGetLookupText",
                        args: lookupId.ToString()
                    ),
                    innerException: ex
                );
            }
        }
        if (val == null & returnMessageIfNull)
        {
            string parameterString = "";
            foreach (var parameter in parameters)
            {
                if (parameterString != "")
                {
                    parameterString += ", ";
                }

                parameterString += parameter.Key + ": " + parameter.Value;
            }
            val = "Záznam nedostupný (" + parameterString + ")";
        }
        else
        {
            if (canUseCache & error == false)
            {
                if (!(_valueCache[key: lookupId] as Hashtable).Contains(key: cachableValue))
                {
                    lock (_valueCache[key: lookupId])
                    {
                        (_valueCache[key: lookupId] as Hashtable).Add(
                            key: cachableValue,
                            value: val
                        );
                    }
                }
            }
        }
        return val;
    }

    public string ValueFromRow(DataRow row, string[] columns)
    {
        StringBuilder resultBuilder = new StringBuilder();
        foreach (string column in columns)
        {
            object columnValue = row[columnName: column];
            if (columnValue != DBNull.Value && columnValue.ToString() != "")
            {
                if (resultBuilder.Length > 0)
                {
                    resultBuilder.Append(value: ", ");
                }
                DateTime dateValue = DateTime.MinValue;
                if (columnValue is DateTime)
                {
                    dateValue = (DateTime)columnValue;
                }

                if (
                    columnValue is DateTime
                    && (dateValue.Hour == 0 & dateValue.Minute == 0 & dateValue.Second == 0)
                )
                {
                    resultBuilder.Append(value: dateValue.ToShortDateString());
                }
                else
                {
                    resultBuilder.Append(value: columnValue.ToString());
                }
            }
        }
        return resultBuilder.ToString();
    }

    public object LinkTarget(ILookupControl lookupControl, object value)
    {
        DataLookupMenuBinding binding = GetMenuBindingElement(
            lookup: GetLookup(lookupId: lookupControl.LookupId),
            value: value
        );
        if (binding != null)
        {
            return binding.MenuItem;
        }

        return null;
    }

    public Dictionary<string, object> LinkParameters(object linkTarget, object value)
    {
        var result = new Dictionary<string, object>();
        AbstractMenuItem menu = linkTarget as AbstractMenuItem;
        if (menu != null)
        {
            if (menu is FormReferenceMenuItem)
            {
                FormReferenceMenuItem formRef = menu as FormReferenceMenuItem;
                if (formRef.RecordEditMethod == null)
                {
                    result.Add(key: "Id", value: value);
                }
                else
                {
                    foreach (
                        var entry in (menu as FormReferenceMenuItem)
                            .RecordEditMethod
                            .ParameterReferences
                    )
                    {
                        result.Add(key: entry.Key, value: value);
                    }
                }
            }
        }
        return result;
    }

    #region Private Methods
    private IServiceAgent GetAgent()
    {
        return (
            ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
            as IBusinessServicesService
        ).GetAgent(serviceType: "DataService", ruleEngine: null, workflowEngine: null);
    }

    /// <summary>
    /// Returns lookup schema item
    /// </summary>
    /// <param name="lookupId"></param>
    /// <returns></returns>
    public AbstractDataLookup GetLookup(Guid lookupId)
    {
        ModelElementKey key = new ModelElementKey(id: lookupId);
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        AbstractDataLookup lookup =
            persistence.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractDataLookup),
                primaryKey: key
            ) as AbstractDataLookup;
        if (lookup == null)
        {
            throw new OrigamException(
                message: string.Format(
                    format: "Couldn't find a lookup with id {0}.",
                    arg0: lookupId
                )
            );
        }
        return lookup;
    }

    private DataStructureQuery GetQuery(AbstractDataLookup lookup, QueryType queryType)
    {
        DataServiceDataLookup dataLookup = lookup as DataServiceDataLookup;
        if (dataLookup != null)
        {
            switch (queryType)
            {
                case QueryType.List:
                {
                    return new DataStructureQuery(
                        dataStructureId: dataLookup.ListDataStructureId,
                        methodId: dataLookup.ListDataStructureMethodId,
                        defaultSetId: Guid.Empty,
                        sortSetId: dataLookup.ListDataStructureSortSetId
                    );
                }
                case QueryType.Value:
                {
                    if (dataLookup.ValueDataStructureMethodId == null)
                    {
                        log.Warn(message: "DataLookup has no ValueDataStructureMethodId !!");
                    }
                    return new DataStructureQuery(
                        dataStructureId: dataLookup.ValueDataStructureId,
                        methodId: dataLookup.ValueDataStructureMethodId,
                        defaultSetId: Guid.Empty,
                        sortSetId: dataLookup.ValueDataStructureSortSetId
                    );
                }

                case QueryType.ValueCacheList:
                {
                    return new DataStructureQuery(dataStructureId: dataLookup.ValueDataStructureId);
                }
            }
        }
        throw new ArgumentOutOfRangeException(
            paramName: ResourceUtils.GetString(key: "ErrorUnknownLookupType")
        );
    }

    private bool HasEditListMenuBinding(AbstractDataLookup lookup)
    {
        IOrigamAuthorizationProvider authorizationProvider =
            SecurityManager.GetAuthorizationProvider();
        IPrincipal principal = SecurityManager.CurrentPrincipal;

        foreach (DataLookupMenuBinding binding in lookup.MenuBindings)
        {
            if (
                binding.SelectionLookup == null
                && AuthorizeMenuBinding(
                    authorizationProvider: authorizationProvider,
                    principal: principal,
                    binding: binding
                )
            )
            {
                return true;
            }
        }
        return false;
    }

    private bool HasEditRecordMenuBinding(AbstractDataLookup lookup)
    {
        IOrigamAuthorizationProvider authorizationProvider =
            SecurityManager.GetAuthorizationProvider();
        foreach (DataLookupMenuBinding binding in lookup.MenuBindings)
        {
            if (
                AuthorizeMenuBinding(
                    authorizationProvider: authorizationProvider,
                    principal: SecurityManager.CurrentPrincipal,
                    binding: binding
                )
            )
            {
                return true;
            }
        }
        return false;
    }

    private bool HasMenuBindingWithSelection(AbstractDataLookup lookup)
    {
        IOrigamAuthorizationProvider authorizationProvider =
            SecurityManager.GetAuthorizationProvider();
        IPrincipal principal = SecurityManager.CurrentPrincipal;

        foreach (DataLookupMenuBinding binding in lookup.MenuBindings)
        {
            if (
                binding.SelectionLookup != null
                && AuthorizeMenuBinding(
                    authorizationProvider: authorizationProvider,
                    principal: principal,
                    binding: binding
                )
            )
            {
                return true;
            }
        }
        return false;
    }

    public bool HasMenuBindingWithSelection(Guid lookupId)
    {
        IPersistenceService ps =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        AbstractDataLookup lookup = (AbstractDataLookup)
            ps.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractDataLookup),
                primaryKey: new ModelElementKey(id: lookupId)
            );
        return HasMenuBindingWithSelection(lookup: lookup);
    }

    public IMenuBindingResult GetMenuBinding(Guid lookupId, object value)
    {
        IPersistenceService ps =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        AbstractDataLookup lookup = (AbstractDataLookup)
            ps.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractDataLookup),
                primaryKey: new ModelElementKey(id: lookupId)
            );
        DataLookupMenuBinding binding = GetMenuBindingElement(lookup: lookup, value: value);
        if (binding == null)
        {
            return new MenuBindingResult();
        }

        return new MenuBindingResult(
            menuId: binding.MenuItemId.ToString(),
            panelId: binding.SelectionPanelId
        );
    }

    public NewRecordScreenBinding GetNewRecordScreenBinding(AbstractDataLookup lookup)
    {
        return lookup
            .ChildItems.OfType<NewRecordScreenBinding>()
            .FirstOrDefault(predicate: x => x.IsAvailable);
    }

    public DataLookupMenuBinding GetMenuBindingElement(AbstractDataLookup lookup, object value)
    {
        IOrigamAuthorizationProvider authorizationProvider =
            SecurityManager.GetAuthorizationProvider();
        IPrincipal principal = SecurityManager.CurrentPrincipal;
        Hashtable selectionCache = new Hashtable();
        List<DataLookupMenuBinding> menuBindings = lookup.MenuBindings;
        menuBindings.Sort();

        foreach (DataLookupMenuBinding binding in menuBindings)
        {
            // get list
            if (
                value == null
                && binding.SelectionLookup == null
                && AuthorizeMenuBinding(
                    authorizationProvider: authorizationProvider,
                    principal: principal,
                    binding: binding
                )
            )
            {
                return binding;
            }
            // get record - without selection

            if (
                HasMenuBindingWithSelection(lookup: lookup) == false
                && binding.SelectionLookup == null
                && AuthorizeMenuBinding(
                    authorizationProvider: authorizationProvider,
                    principal: principal,
                    binding: binding
                )
            )
            {
                return binding;
            }
            // get record - with selection

            if (
                value != null
                && binding.SelectionLookup != null
                && AuthorizeMenuBinding(
                    authorizationProvider: authorizationProvider,
                    principal: principal,
                    binding: binding
                )
            )
            {
                if (!selectionCache.Contains(key: binding.SelectionLookupId))
                {
                    selectionCache[key: binding.SelectionLookupId] = GetDisplayText(
                        lookupId: binding.SelectionLookupId,
                        lookupValue: value,
                        useCache: false,
                        returnMessageIfNull: false,
                        transactionId: null
                    );
                }
                IParameterService paramService =
                    ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
                    as IParameterService;

                if (binding.SelectionConstant == null)
                {
                    // if anything was found by the lookup, this is the right menu
                    if (selectionCache[key: binding.SelectionLookupId] != null)
                    {
                        return binding;
                    }
                }
                else
                {
                    // if specific value was found by the lookup, this is the right menu
                    object paramValue = paramService.GetParameterValue(
                        id: binding.SelectionConstantId
                    );
                    if (
                        selectionCache[key: binding.SelectionLookupId] != null
                        && selectionCache[key: binding.SelectionLookupId].Equals(obj: paramValue)
                    )
                    {
                        return binding;
                    }
                }
            }
        }
        return null;
    }

    public bool AuthorizeMenuBinding(
        IOrigamAuthorizationProvider authorizationProvider,
        IPrincipal principal,
        DataLookupMenuBinding binding
    )
    {
        IParameterService param =
            ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
            as IParameterService;
        return (
            authorizationProvider.Authorize(
                principal: principal,
                context: binding.AuthorizationContext
            )
            && authorizationProvider.Authorize(
                principal: principal,
                context: binding.MenuItem.AuthorizationContext
            )
            && param.IsFeatureOn(featureCode: binding.MenuItem.Features)
        );
    }
    #endregion
    #region Lookup Control Event Handlers
    private void lookupControl_LookupDisplayTextRequested(object sender, EventArgs e)
    {
        ILookupControl control = sender as ILookupControl;
        control.LookupDisplayText = GetDisplayText(
                lookupId: control.LookupId,
                lookupValue: control.LookupValue,
                transactionId: null
            )
            .ToString();
    }

    public DataTable GetList(LookupListRequest request)
    {
        DataServiceDataLookup lookup =
            GetLookup(lookupId: request.LookupId) as DataServiceDataLookup;
        DataStructureQuery query = GetQuery(lookup: lookup, queryType: QueryType.List);
        if (request.CurrentRow != null)
        {
            // set parameters
            foreach (
                DictionaryEntry entry in DatasetTools.RetrieveParameters(
                    parameterMappings: request.ParameterMappings,
                    rows: new List<DataRow> { request.CurrentRow }
                )
            )
            {
                query.Parameters.Add(
                    value: new QueryParameter(_parameterName: (string)entry.Key, value: entry.Value)
                );
            }
        }
        if (lookup.IsFilteredServerside)
        {
            if (lookup.ListMethod == null)
            {
                throw new ArgumentNullException(
                    paramName: "ListMethod",
                    message: "Lookup is defined as IsFilteredServerside but ListMethod is not set."
                );
            }
            if (lookup.ServersideFilterParameter == null)
            {
                throw new ArgumentNullException(
                    paramName: "ServersideFilterParameter",
                    message: "Lookup is defined as IsFilteredServerside but ServersideFilterParameter is not set."
                );
            }
            query.Parameters.Add(
                value: new QueryParameter(
                    _parameterName: lookup.ServersideFilterParameter,
                    value: request.SearchText
                )
            );
            if (request.PageNumber != -1)
            {
                query.Parameters.Add(
                    value: new QueryParameter(_parameterName: "_pageSize", value: request.PageSize)
                );
                query.Parameters.Add(
                    value: new QueryParameter(
                        _parameterName: "_pageNumber",
                        value: request.PageNumber
                    )
                );
            }
        }
        IServiceAgent dataServiceAgent = GetAgent();
        dataServiceAgent.TransactionId = request.TransactionId;
        dataServiceAgent.MethodName = "LoadDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: query);
        dataServiceAgent.Run();
        DataSet data = dataServiceAgent.Result as DataSet;
        DataTable lookupTable = data.Tables[index: 0];
        IStateMachineService stateMachine =
            ServiceManager.Services.GetService(serviceType: typeof(IStateMachineService))
            as IStateMachineService;
        object originalValue = DBNull.Value;
        if (request.CurrentRow != null) // only go to state machine evaluation, if the control can give us current row context
        {
            Guid entityId = (Guid)request.CurrentRow.Table.ExtendedProperties[key: "EntityId"];
            Guid fieldId = (Guid)
                request.CurrentRow.Table.Columns[name: request.FieldName].ExtendedProperties[
                    key: "Id"
                ];
            switch (request.CurrentRow.RowState)
            {
                case DataRowState.Detached:
                {
                    break;
                }
                case DataRowState.Added:
                {
                    break;
                }
                case DataRowState.Modified:
                {
                    originalValue = request.CurrentRow[
                        columnName: request.FieldName,
                        version: DataRowVersion.Original
                    ];
                    break;
                }

                default:
                {
                    originalValue = request.CurrentRow[
                        columnName: request.FieldName,
                        version: DataRowVersion.Current
                    ];
                    break;
                }
            }
            object[] allowedStates = stateMachine.AllowedStateValues(
                entityId: entityId,
                fieldId: fieldId,
                currentStateValue: originalValue,
                dataRow: request.CurrentRow,
                transactionId: null
            );
            if (allowedStates != null)
            {
                // this is a state machine, so we remove all non-allowed states from the list
                foreach (DataRow row in lookupTable.Rows)
                {
                    object rowValue = row[columnName: lookup.ListValueMember];
                    bool found = false;
                    foreach (object allowedValue in allowedStates)
                    {
                        if (allowedValue.Equals(obj: rowValue))
                        {
                            found = true;
                            break;
                        }
                    }
                    if (!found)
                    {
                        row.Delete();
                    }
                }
            }
        }
        PostProcessLookupList(lookup: lookup, lookupTable: lookupTable);
        // filter unique values
        if (request.ShowUniqueValues)
        {
            foreach (DataRow row in lookupTable.Rows)
            {
                if (request.CurrentRow != null)
                {
                    if (
                        request
                            .CurrentRow.Table.Select(
                                filterExpression: request.FieldName
                                    + "= '"
                                    + row[columnName: lookup.ValueValueMember]
                                    + "'"
                            )
                            .GetLength(dimension: 0) > 0
                    )
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
        if (lookup.RoleFilterMember != "" & lookup.RoleFilterMember != null)
        {
            if (!lookupTable.Columns.Contains(name: lookup.RoleFilterMember))
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "RoleFilterMember",
                    actualValue: lookup.RoleFilterMember,
                    message: ResourceUtils.GetString(key: "ErrorNoSuchColumn")
                );
            }
            IOrigamAuthorizationProvider authorizationProvider =
                SecurityManager.GetAuthorizationProvider();
            foreach (DataRow row in lookupTable.Rows)
            {
                object context = row[columnName: lookup.RoleFilterMember];
                if (
                    !authorizationProvider.Authorize(
                        principal: SecurityManager.CurrentPrincipal,
                        context: context == DBNull.Value ? null : (string)context
                    )
                )
                {
                    row.Delete();
                }
            }
        }
        // filter features
        if (lookup.FeatureFilterMember != "" & lookup.FeatureFilterMember != null)
        {
            if (!lookupTable.Columns.Contains(name: lookup.FeatureFilterMember))
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "FeatureFilterMember",
                    actualValue: lookup.FeatureFilterMember,
                    message: ResourceUtils.GetString(key: "ErrorNoSuchColumn")
                );
            }
            IParameterService param =
                ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
                as IParameterService;
            foreach (DataRow row in lookupTable.Rows)
            {
                if (!param.IsFeatureOn(featureCode: lookup.FeatureFilterMember))
                {
                    row.Delete();
                }
            }
        }
    }

    private void lookupControl_LookupListRefreshRequested(object sender, EventArgs e)
    {
        ILookupControl control = sender as ILookupControl;
        OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
        if (!settings.UseProgressiveCaching)
        {
            // clear cache
            if (_valueCache.ContainsKey(key: control.LookupId))
            {
                _valueCache.Remove(key: control.LookupId);
            }
            // refresh current text
            lookupControl_LookupDisplayTextRequested(sender: sender, e: e);
        }
        DataTable lookupTable = GetList(
            request: ILookupControlToLookupListRequest(control: control)
        );
        DataView view = new DataView(table: lookupTable);
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

    public object CreateRecord(
        Guid lookupId,
        Dictionary<string, object> values,
        string transactionId
    )
    {
        Guid newId = Guid.NewGuid();
        var lookup = GetLookup(lookupId: lookupId);
        DatasetGenerator dg = new DatasetGenerator(userDefinedParameters: true);
        DataSet data = dg.CreateDataSet(ds: lookup.ListDataStructure);
        Schema.EntityModel.DataStructureEntity entity =
            lookup.ListDataStructure.Entities[index: 0] as Schema.EntityModel.DataStructureEntity;
        if (!entity.AllFields)
        {
            throw new Exception(
                message: "Data structure entity "
                    + entity.Path
                    + " has to have AllFields = true in order to create new records through lookups."
            );
        }
        DataTable table = data.Tables[name: entity.Name];
        var row = table.NewRow();
        row[columnName: lookup.ListValueMember] = newId;
        DatasetTools.UpdateOrigamSystemColumns(
            row: row,
            isNew: true,
            profileId: SecurityManager.CurrentUserProfile().Id
        );
        foreach (var item in values)
        {
            string columnName = (string)item.Key;
            if (!table.Columns.Contains(name: columnName))
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "columnName",
                    actualValue: columnName,
                    message: "Field not found in the ListDataStructure of lookup " + lookup.Path
                );
            }
            Type type = table.Columns[name: columnName].DataType;
            object value = DBNull.Value;
            if (item.Value != null)
            {
                if (type == typeof(Guid))
                {
                    value = new Guid(g: item.Value.ToString());
                }
                else
                {
                    value = Convert.ChangeType(value: item.Value, conversionType: type);
                }
            }
            row[columnName: item.Key] = value;
        }
        table.Rows.Add(row: row);
        DataService.Instance.StoreData(
            dataStructureId: lookup.ListDataStructureId,
            data: data,
            loadActualValuesAfterUpdate: false,
            transactionId: transactionId
        );
        return newId;
    }
    #endregion
    public void RemoveFromCache(Guid id)
    {
        if (_valueCache.Contains(key: id))
        {
            _valueCache.Remove(key: id);
        }
    }
}
