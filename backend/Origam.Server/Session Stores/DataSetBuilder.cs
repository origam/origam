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
using System.Collections.Generic;
using System.Data;
using Origam.DA;
using Origam.DA.Service;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server.Session_Stores;

public class DataSetBuilder
{
    private readonly object _lock = new object();

    public DataSet InitializeFullStructure(Guid id, DataStructureDefaultSet defaultSet)
    {
        return new DatasetGenerator(userDefinedParameters: true).CreateDataSet(
            ds: DataStructure(id: id),
            includeCalculatedColumns: true,
            defaultSet: defaultSet
        );
    }

    public DataSet InitializeListStructure(DataSet data, string listEntity, bool isDbSource)
    {
        DataSet listData = DatasetTools.CloneDataSet(dataset: data);
        DataTable listTable = listData.Tables[name: listEntity];
        // turn off constraints checking because we will be loading only partial
        // data to the list
        listData.EnforceConstraints = false;
        // remove all computed columns - list will calculate values from the database,
        // by having aggregation expressions the values would be recalculated to zeros
        // becuase we would not have any child records
        // row-level computed columns cause performance problems with lots of data
        foreach (DataColumn col in listTable.Columns)
        {
            col.Expression = "";
            col.ReadOnly = false;
            // default values would distract when debugging a partly loaded list
            col.DefaultValue = null;
        }
        // we add a column that will identify if the record has been loaded from
        // the database
        if (isDbSource)
        {
            DataColumn loadedFlagColumn = listTable.Columns.Add(
                columnName: "___ORIGAM_IsLoaded",
                type: typeof(bool)
            );
            loadedFlagColumn.AllowDBNull = false;
            loadedFlagColumn.DefaultValue = false;
        }
        // we cannot delete non-list tables because child entities will be
        // neccessary for array-type fields (TagInput etc.)
        return listData;
    }

    internal DataStructure DataStructure(Guid id)
    {
        IPersistenceService persistence =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        return persistence.SchemaProvider.RetrieveInstance(
                type: typeof(DataStructure),
                primaryKey: new ModelElementKey(id: id)
            ) as DataStructure;
    }

    public DataSet LoadListData(
        IList<string> dataListLoadedColumns,
        DataSet data,
        string listEntity,
        DataStructureSortSet sortSet,
        FormReferenceMenuItem _menuItem,
        QueryParameterCollection queryParameter = null
    )
    {
        dataListLoadedColumns.Clear();
        string initialColumns = ListPrimaryKeyColumns(data: data, listEntity: listEntity);
        if (sortSet != null)
        {
            foreach (
                var sortItem in sortSet.ChildItemsByType<DataStructureSortSetItem>(
                    itemType: DataStructureSortSetItem.CategoryConst
                )
            )
            {
                if (sortItem.Entity.Name == listEntity)
                {
                    initialColumns += ";" + sortItem.FieldName;
                    dataListLoadedColumns.Add(item: sortItem.FieldName);
                }
            }
        }
        // load list entity
        DataSet result = DataService.Instance.LoadData(
            dataStructureId: _menuItem.ListDataStructureId,
            methodId: _menuItem.ListMethodId,
            defaultSetId: Guid.Empty,
            sortSetId: _menuItem.ListSortSetId,
            transactionId: null,
            parameters: queryParameter,
            currentData: data,
            entity: listEntity,
            columnName: initialColumns
        );
        // load all array field child entities - there is no way how to read
        // only children of a specific record (inside LazyLoadListRowData) so
        // we preload all array fields here
        var arrayColumns = new List<string>();
        foreach (DataColumn col in result.Tables[name: listEntity].Columns)
        {
            if (IsColumnArray(dataColumn: col))
            {
                arrayColumns.Add(item: col.ColumnName);
            }
        }
        LoadArrayColumns(
            dataset: result,
            entity: listEntity,
            qparams: queryParameter,
            arrayColumns: arrayColumns,
            DataListLoadedColumns: dataListLoadedColumns,
            _menuItem: _menuItem
        );
        return result;
    }

    private void LoadArrayColumns(
        DataSet dataset,
        string entity,
        QueryParameterCollection qparams,
        List<string> arrayColumns,
        IList<string> DataListLoadedColumns,
        FormReferenceMenuItem _menuItem
    )
    {
        lock (_lock)
        {
            foreach (string column in arrayColumns)
            {
                if (!DataListLoadedColumns.Contains(item: column))
                {
                    DataColumn col = dataset.Tables[name: entity].Columns[name: column];
                    string relationName = (string)col.ExtendedProperties[key: Const.ArrayRelation];
                    DataService.Instance.LoadData(
                        dataStructureId: _menuItem.ListDataStructureId,
                        methodId: _menuItem.ListMethodId,
                        defaultSetId: Guid.Empty,
                        sortSetId: _menuItem.ListSortSetId,
                        transactionId: null,
                        parameters: qparams,
                        currentData: dataset,
                        entity: relationName,
                        columnName: null
                    );
                    DataListLoadedColumns.Add(item: column);
                }
            }
        }
    }

    public static bool IsColumnArray(DataColumn dataColumn)
    {
        if (dataColumn.ExtendedProperties.Contains(key: Const.OrigamDataType))
        {
            return ((Schema.OrigamDataType)dataColumn.ExtendedProperties[key: Const.OrigamDataType])
                == Schema.OrigamDataType.Array;
        }

        return false;
    }

    public string ListPrimaryKeyColumns(DataSet data, string listEntity)
    {
        string initialColumns = "";
        foreach (var pkCol in data.Tables[name: listEntity].PrimaryKey)
        {
            if (initialColumns != "")
            {
                initialColumns += ";";
            }
            initialColumns += pkCol.ColumnName;
        }
        return initialColumns;
    }
}
