#region license
/*
Copyright 2005 - 2025 Advantage Solutions, s. r. o.

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
using System.Data;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Extensions.Logging;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Server.Controller;
using Origam.Workbench.Services;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

public class DataServiceSoap : IDataServiceSoap
{
    private readonly ILogger<AbstractController> log;

    public DataServiceSoap(ILogger<AbstractController> log)
    {
        this.log = log;
    }

    public Task<DataSet> LoadDataAsync(
        string dataStructureId,
        string filterId,
        string defaultSetId,
        string sortSetId,
        Parameter[] parameters
    )
    {
        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.Log(logLevel: LogLevel.Information, message: "LoadData");
        }
        Guid dsId = new Guid(g: dataStructureId);
        Guid fId = string.IsNullOrWhiteSpace(value: filterId) ? Guid.Empty : new Guid(g: filterId);
        Guid dId = string.IsNullOrWhiteSpace(value: defaultSetId)
            ? Guid.Empty
            : new Guid(g: defaultSetId);
        Guid sId = string.IsNullOrWhiteSpace(value: sortSetId)
            ? Guid.Empty
            : new Guid(g: sortSetId);
        var parameterCollection = ParameterUtils.ToQueryParameterCollection(parameters: parameters);
        var dataSet = CoreServices.DataService.Instance.LoadData(
            dataStructureId: dsId,
            methodId: fId,
            defaultSetId: dId,
            sortSetId: sId,
            transactionId: null,
            parameters: parameterCollection
        );
        return Task.FromResult(result: dataSet);
    }

    public Task<DataSet> LoadData0Async(
        string dataStructureId,
        string filterId,
        string sortSetId,
        string defaultSetId
    )
    {
        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.Log(logLevel: LogLevel.Information, message: "LoadData0");
        }
        Guid dsId = new Guid(g: dataStructureId);
        Guid fId = string.IsNullOrWhiteSpace(value: filterId) ? Guid.Empty : new Guid(g: filterId);
        Guid dId = string.IsNullOrWhiteSpace(value: defaultSetId)
            ? Guid.Empty
            : new Guid(g: defaultSetId);
        Guid sId = string.IsNullOrWhiteSpace(value: sortSetId)
            ? Guid.Empty
            : new Guid(g: sortSetId);
        var dataSet = CoreServices.DataService.Instance.LoadData(
            dataStructureId: dsId,
            methodId: fId,
            defaultSetId: dId,
            sortSetId: sId,
            transactionId: null
        );
        return Task.FromResult(result: dataSet);
    }

    public Task<DataSet> LoadData1Async(
        string dataStructureId,
        string filterId,
        string defaultSetId,
        string sortSetId,
        string paramName1,
        string paramValue1
    )
    {
        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.Log(logLevel: LogLevel.Information, message: "LoadData1");
        }
        Guid dsId = new Guid(g: dataStructureId);
        Guid fId = string.IsNullOrWhiteSpace(value: filterId) ? Guid.Empty : new Guid(g: filterId);
        Guid dId = string.IsNullOrWhiteSpace(value: defaultSetId)
            ? Guid.Empty
            : new Guid(g: defaultSetId);
        Guid sId = string.IsNullOrWhiteSpace(value: sortSetId)
            ? Guid.Empty
            : new Guid(g: sortSetId);
        var dataSet = CoreServices.DataService.Instance.LoadData(
            dataStructureId: dsId,
            methodId: fId,
            defaultSetId: dId,
            sortSetId: sId,
            transactionId: null,
            paramName1: paramName1,
            paramValue1: paramValue1
        );
        return Task.FromResult(result: dataSet);
    }

    public Task<DataSet> LoadData2Async(
        string dataStructureId,
        string filterId,
        string defaultSetId,
        string sortSetId,
        string paramName1,
        string paramValue1,
        string paramName2,
        string paramValue2
    )
    {
        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.Log(logLevel: LogLevel.Information, message: "LoadData2");
        }
        Guid dsId = new Guid(g: dataStructureId);
        Guid fId = string.IsNullOrWhiteSpace(value: filterId) ? Guid.Empty : new Guid(g: filterId);
        Guid dId = string.IsNullOrWhiteSpace(value: defaultSetId)
            ? Guid.Empty
            : new Guid(g: defaultSetId);
        Guid sId = string.IsNullOrWhiteSpace(value: sortSetId)
            ? Guid.Empty
            : new Guid(g: sortSetId);
        var dataSet = CoreServices.DataService.Instance.LoadData(
            dataStructureId: dsId,
            methodId: fId,
            defaultSetId: dId,
            sortSetId: sId,
            transactionId: null,
            paramName1: paramName1,
            paramValue1: paramValue1,
            paramName2: paramName2,
            paramValue2: paramValue2
        );
        return Task.FromResult(result: dataSet);
    }

    public Task<DataSet> ExecuteProcedureAsync(string procedureName, Parameter[] parameters)
    {
        log.Log(logLevel: LogLevel.Information, message: "ExecuteProcedure");

        var parameterCollection = ParameterUtils.ToQueryParameterCollection(parameters: parameters);
        var dataSet = CoreServices.DataService.Instance.ExecuteProcedure(
            procedureName: procedureName,
            parameters: parameterCollection,
            transactionId: null
        );
        return Task.FromResult(result: dataSet);
    }

    public Task<DataSet> StoreDataAsync(
        string dataStructureId,
        DataSet data,
        bool loadActualValuesAfterUpdate
    )
    {
        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.Log(logLevel: LogLevel.Information, message: "StoreData");
            log.Log(logLevel: LogLevel.Information, message: data.GetXml());
        }

        Guid guid = new Guid(g: dataStructureId);
        IPersistenceService service =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        DataStructure structure =
            service.SchemaProvider.RetrieveInstance(
                type: typeof(DataStructure),
                primaryKey: new ModelElementKey(id: guid)
            ) as DataStructure;
        DataSet set = new DatasetGenerator(userDefinedParameters: true).CreateDataSet(
            ds: structure
        );
        foreach (DataTable table in data.Tables)
        {
            if (set.Tables.Contains(name: table.TableName))
            {
                DataTable setTable = set.Tables[name: table.TableName];
                table.ExtendedProperties.Clear();
                foreach (DictionaryEntry entry in setTable.ExtendedProperties)
                {
                    table.ExtendedProperties.Add(key: entry.Key, value: entry.Value);
                }
                foreach (DataColumn col in table.Columns)
                {
                    if (setTable.Columns.Contains(name: col.ColumnName))
                    {
                        col.ExtendedProperties.Clear();
                        foreach (
                            DictionaryEntry entry in setTable
                                .Columns[name: col.ColumnName]
                                .ExtendedProperties
                        )
                        {
                            col.ExtendedProperties.Add(key: entry.Key, value: entry.Value);
                        }
                    }
                }
            }
        }
        DataSet returnDataSet = CoreServices.DataService.Instance.StoreData(
            dataStructureId: guid,
            data: data,
            loadActualValuesAfterUpdate: loadActualValuesAfterUpdate,
            transactionId: null
        );
        return Task.FromResult(result: returnDataSet);
    }

    public Task<DataSet> StoreXmlAsync(
        string dataStructureId,
        XmlNode xml,
        bool loadActualValuesAfterUpdate
    )
    {
        if (log.IsEnabled(logLevel: LogLevel.Information))
        {
            log.Log(logLevel: LogLevel.Information, message: "StoreXml");
            log.Log(logLevel: LogLevel.Information, message: xml.OuterXml);
        }
        Guid guid = new Guid(g: dataStructureId);
        IPersistenceService service =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        DataStructure structure =
            service.SchemaProvider.RetrieveInstance(
                type: typeof(DataStructure),
                primaryKey: new ModelElementKey(id: guid)
            ) as DataStructure;
        DataSet set = new DatasetGenerator(userDefinedParameters: true).CreateDataSet(
            ds: structure
        );
        set.EnforceConstraints = false;
        set.ReadXml(reader: new XmlNodeReader(node: xml));
        DataSet set2 = new DatasetGenerator(userDefinedParameters: true).CreateDataSet(
            ds: structure
        );
        object obj2 = SecurityManager.CurrentUserProfile().Id;
        MergeParams mergeParams = new MergeParams
        {
            PreserveChanges = true,
            PreserveNewRowState = true,
            SourceIsFragment = false,
            ProfileId = obj2,
            TrueDelete = false,
        };
        DatasetTools.MergeDataSet(
            inout_dsTarget: set2,
            in_dsSource: set,
            changeList: null,
            mergeParams: mergeParams
        );
        if (log.IsEnabled(logLevel: LogLevel.Debug))
        {
            log.Log(logLevel: LogLevel.Debug, message: "StoreXml - merged xml below");
            log.Log(logLevel: LogLevel.Debug, message: set2.GetXml());
        }
        DataSet dataSet = CoreServices.DataService.Instance.StoreData(
            dataStructureId: guid,
            data: set2,
            loadActualValuesAfterUpdate: loadActualValuesAfterUpdate,
            transactionId: null
        );
        return Task.FromResult(result: dataSet);
    }
}
