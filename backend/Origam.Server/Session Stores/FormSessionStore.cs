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
using System.Xml;
using Origam.DA;
using Origam.DA.Service;
using Origam.Gui;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;
using CoreServices = Origam.Workbench.Services.CoreServices;
#pragma warning disable CS1573 // Parameter has no matching param tag in the XML comment (but other parameters do)

namespace Origam.Server;

public class FormSessionStore : SaveableSessionStore
{
    private string _delayedLoadingParameterName;
    private FormReferenceMenuItem _menuItem;
    private object _getRowDataLock = new object();
    private XmlDocument _preparedFormXml = null;
    public FormReferenceMenuItem MenuItem => _menuItem;

    public FormSessionStore(
        IBasicUIService service,
        UIRequest request,
        string name,
        FormReferenceMenuItem menuItem,
        Analytics analytics
    )
        : base(service: service, request: request, name: name, analytics: analytics)
    {
        _menuItem = menuItem;
        SetMenuProperties();
    }

    public FormSessionStore(
        IBasicUIService service,
        UIRequest request,
        string name,
        Analytics analytics
    )
        : base(service: service, request: request, name: name, analytics: analytics)
    {
        IPersistenceService ps =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        FormReferenceMenuItem fr = (FormReferenceMenuItem)
            ps.SchemaProvider.RetrieveInstance(
                type: typeof(FormReferenceMenuItem),
                primaryKey: new ModelElementKey(id: new Guid(g: this.Request.ObjectId))
            );
        _menuItem = fr;
        FormId = _menuItem.ScreenId;
        DataStructureId = _menuItem.Screen.DataSourceId;
        SetMenuProperties();
    }

    private void SetMenuProperties()
    {
        RuleSet = _menuItem.RuleSet;
        Template = _menuItem.DefaultTemplate;
        SortSet = _menuItem.SortSet;
        RefreshAfterSaveType = _menuItem.RefreshAfterSaveType;
        ConfirmationRule = _menuItem.ConfirmationRule;
        RefreshPortalAfterSave = _menuItem.RefreshPortalAfterSave;
    }

    #region Overriden SessionStore Methods
    public override bool SupportsFormXmlAsync => true;

    public override void Init()
    {
        LoadData();
    }

    private void LoadData()
    {
        if (dataRequested)
        {
            LoadDataFxServer();
        }
        else
        {
            PrepareDataCore();
        }
    }

    private void PrepareDataCore()
    {
        var data = InitializeFullStructure(defaultSet: _menuItem.DefaultSet);
        SetDataSource(dataSource: data);
        SetDelayedLoadingParameter(method: _menuItem.Method);
        this.IsDelayedLoading = true;
        this.DataListEntity = _menuItem.ListEntity.Name;
    }

    private void LoadDataFxServer()
    {
        DataSet data = null;
        if (this.Request.IsSingleRecordEdit && _menuItem.RecordEditMethod != null)
        {
            data = LoadSingleRecord();
            if (_menuItem.ListEntity != null)
            {
                SetDataList(
                    list: null,
                    entity: _menuItem.ListEntity.Name,
                    listDataStructure: _menuItem.ListDataStructure,
                    method: _menuItem.ListMethod
                );
            }
            SetDelayedLoadingParameter(method: _menuItem.RecordEditMethod);
        }
        else if (_menuItem.ListDataStructure == null)
        {
            // all data at once
            data = LoadCompleteData();
        }
        else
        {
            throw new Exception(
                message: "A screen is lazy loaded but the client requested session data on InitUI "
                    + "call by setting DataRequested=true. Instead the client should set DataRequested=false "
                    + "and call GetRows in order to get the list data and then MasterRecord to load "
                    + "one of the records and GetData to request entity data."
            );
        }
        if (data != null)
        {
            SetDataSource(dataSource: data);
        }
    }

    private void SetDelayedLoadingParameter(DataStructureMethod method)
    {
        // set the parameter for delayed data loading - there should be just 1
        DelayedLoadingParameterName = CustomParameterService.GetFirstNonCustomParameter(
            method: method
        );
    }

    private string ListPrimaryKeyColumns(DataSet data, string listEntity)
    {
        return GetDataSetBuilder().ListPrimaryKeyColumns(data: data, listEntity: listEntity);
    }

    private DataSet LoadCompleteData()
    {
        if (_menuItem.Method != null)
        {
            ResolveFormMethodParameters(method: _menuItem.Method);
        }
        DataSet data;
        QueryParameterCollection qparams = Request.QueryParameters;
        data = CoreServices.DataService.Instance.LoadData(
            dataStructureId: DataStructureId,
            methodId: _menuItem.MethodId,
            defaultSetId: _menuItem.DefaultSetId,
            sortSetId: _menuItem.SortSetId,
            transactionId: null,
            parameters: qparams
        );
        return data;
    }

    public override void LoadColumns(IList<string> columns)
    {
        QueryParameterCollection qparams = Request.QueryParameters;
        var finalColumns = new List<string>();
        var arrayColumns = new List<string>();
        foreach (var column in columns)
        {
            if (!DataListLoadedColumns.Contains(item: column))
            {
                if (
                    IsColumnArray(
                        dataColumn: DataList.Tables[name: DataListEntity].Columns[name: column]
                    )
                )
                {
                    arrayColumns.Add(item: column);
                }
                else
                {
                    finalColumns.Add(item: column);
                }
            }
        }
        LoadStandardColumns(qparams: qparams, finalColumns: finalColumns);
        LoadArrayColumns(
            dataset: this.DataList,
            entity: this.DataListEntity,
            qparams: qparams,
            arrayColumns: arrayColumns
        );
    }

    private void LoadArrayColumns(
        DataSet dataset,
        string entity,
        QueryParameterCollection qparams,
        List<string> arrayColumns
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
                    CoreServices.DataService.Instance.LoadData(
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

    private void LoadStandardColumns(QueryParameterCollection qparams, List<string> finalColumns)
    {
        lock (_lock)
        {
            if (finalColumns.Count == 0)
            {
                return;
            }
            finalColumns.Add(
                item: ListPrimaryKeyColumns(data: this.DataList, listEntity: this.DataListEntity)
            );
            DataSet columnData = DatasetTools.CloneDataSet(dataset: DataList);
            DataTable listTable = DataList.Tables[name: DataListEntity];
            CoreServices.DataService.Instance.LoadData(
                dataStructureId: _menuItem.ListDataStructureId,
                methodId: _menuItem.ListMethodId,
                defaultSetId: Guid.Empty,
                sortSetId: _menuItem.ListSortSetId,
                transactionId: null,
                parameters: qparams,
                currentData: columnData,
                entity: this.DataListEntity,
                columnName: string.Join(separator: ";", values: finalColumns)
            );
            listTable.BeginLoadData();
            try
            {
                foreach (DataRow columnRow in columnData.Tables[name: DataListEntity].Rows)
                {
                    DataRow listRow = listTable.Rows.Find(
                        keys: DatasetTools.PrimaryKey(row: columnRow)
                    );
                    if (listRow != null)
                    {
                        foreach (string column in finalColumns)
                        {
                            listRow[columnName: column] = columnRow[columnName: column];
                        }
                    }
                }
            }
            finally
            {
                listTable.EndLoadData();
                foreach (string column in finalColumns)
                {
                    DataListLoadedColumns.Add(item: column);
                }
            }
        }
    }

    private DataSet LoadSingleRecord()
    {
        DataSet data;
        // We use the RecordEdit filter set for single record editing.
        ResolveFormMethodParameters(method: _menuItem.RecordEditMethod);
        QueryParameterCollection qparams = Request.QueryParameters;
        data = CoreServices.DataService.Instance.LoadData(
            dataStructureId: DataStructureId,
            methodId: _menuItem.RecordEditMethodId,
            defaultSetId: _menuItem.DefaultSetId,
            sortSetId: _menuItem.SortSetId,
            transactionId: null,
            parameters: qparams
        );
        return data;
    }

    private void ResolveFormMethodParameters(DataStructureMethod method)
    {
        // And we have to get the real parameter names from the filters/defaults instead of the "id"
        // set by the client.
        if (this.Request.Parameters.Contains(key: "id"))
        {
            object value = this.Request.Parameters[key: "id"];
            this.Request.Parameters.Clear();
            foreach (var entry in method.ParameterReferences)
            {
                this.Request.Parameters[key: entry.Key] = value;
            }
            foreach (var entry in DataStructure().ParameterReferences)
            {
                this.Request.Parameters[key: entry.Key] = value;
            }
        }
    }

    internal override List<ChangeInfo> Save()
    {
        if (MenuItem.ReadOnlyAccess)
        {
            throw new Exception(message: "Read only session cannot be saved");
        }
        return base.Save();
    }

    public override object ExecuteActionInternal(string actionId)
    {
        switch (actionId)
        {
            case ACTION_SAVE:
            {
                object result = Save();
                //if (this.IsDelayedLoading)
                //{
                //    this.CurrentRecordId = null;
                //}
                return result;
            }

            case ACTION_REFRESH:
            {
                return Refresh();
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "actionId",
                    actualValue: actionId,
                    message: Resources.ErrorContextUnknownAction
                );
            }
        }
    }

    /// <summary>
    /// Called when moving to a new row to load the actual data from the data source
    /// (delayed loading).
    /// </summary>
    /// <param name="entity"></param>
    /// <param name="id"></param>
    /// <returns></returns>
    public override List<ChangeInfo> GetRowData(string entity, object id, bool ignoreDirtyState)
    {
        var result = new List<ChangeInfo>();
        lock (_getRowDataLock)
        {
            if (id == null)
            {
                CurrentRecordId = null;
                return result;
            }
            DataTable table = GetDataTable(entity: entity);
            DataRow row = GetSessionRow(entity: entity, id: id);
            // for new rows we don't even try to load the data from the database
            if (row == null || row.RowState != DataRowState.Added)
            {
                if (!ignoreDirtyState && this.Data.HasChanges())
                {
                    throw new Exception(message: Resources.ErrorDataNotSavedWhileChangingRow);
                }
                this.CurrentRecordId = null;
                SetDataSource(dataSource: LoadDataPiece(parentId: id));
            }
            this.CurrentRecordId = id;
            DataRow actualDataRow = GetSessionRow(entity: entity, id: id);
            UpdateListRow(r: actualDataRow);
            ChangeInfo ci = GetChangeInfo(requestingGrid: null, row: actualDataRow, operation: 0);
            result.Add(item: ci);
            if (actualDataRow.RowState == DataRowState.Unchanged)
            {
                result.Add(item: ChangeInfo.SavedChangeInfo());
            }
        }
        return result;
    }

    public override ChangeInfo GetRow(string entity, object id)
    {
        lock (_getRowDataLock)
        {
            if (id == null)
            {
                CurrentRecordId = null;
                return null;
            }
            DataRow row = GetSessionRow(entity: entity, id: id);
            return GetChangeInfo(requestingGrid: null, row: row, operation: Operation.Update);
        }
    }

    public override void RevertChanges()
    {
        lock (_getRowDataLock)
        {
            Data.RejectChanges();
        }
    }

    public override List<List<object>> GetData(
        string childEntity,
        object parentRecordId,
        object rootRecordId
    )
    {
        return GetDataImplementation(
            childEntity: childEntity,
            parentRecordId: parentRecordId,
            rootRecordId: rootRecordId
        );
    }

    internal override void OnNewRecord(string entity, object id)
    {
        if (IsLazyLoadedEntity(entity: entity))
        {
            this.CurrentRecordId = null;

            // convert guid to string, because if we get data from Fluorine,
            // it would come as a string, so we would not be able to compare later
            if (id is Guid)
            {
                id = id.ToString();
            }
            this.CurrentRecordId = id;
        }
    }

    public override bool HasChanges()
    {
        return !MenuItem.ReadOnlyAccess && Data != null && Data.HasChanges();
    }

    public override List<ChangeInfo> RestoreData(object recordId)
    {
        var result = new List<ChangeInfo>();
        // get the original row and return it to the client, so it updates to
        // the original state
        DataRow originalRow = this.GetSessionRow(entity: this.DataListEntity, id: recordId);
        if (originalRow.RowState == DataRowState.Added)
        {
            result.AddRange(
                collection: this.DeleteObject(entity: originalRow.Table.TableName, id: recordId)
            );
        }
        else
        {
            this.CurrentRecordId = null;

            // update the values from database
            this.GetRowData(entity: this.DataListEntity, id: recordId, ignoreDirtyState: true);

            // get the loaded data
            originalRow = this.GetSessionRow(entity: this.DataListEntity, id: recordId);
            // return the loaded data
            result.Add(item: GetChangeInfo(requestingGrid: null, row: originalRow, operation: 0));
        }
        if (recordId.Equals(obj: this.CurrentRecordId))
        {
            this.CurrentRecordId = null;
        }
        // add SAVED operation in the end, since we ignored the data and start with a fresh copy
        result.Add(item: ChangeInfo.SavedChangeInfo());
        return result;
    }

    private DataSet LoadDataPiece(object parentId)
    {
        return CoreServices.DataService.Instance.LoadData(
            dataStructureId: DataStructureId,
            methodId: _menuItem.MethodId,
            defaultSetId: _menuItem.DefaultSetId,
            sortSetId: Guid.Empty,
            transactionId: null,
            paramName1: DelayedLoadingParameterName,
            paramValue1: parentId
        );
    }

    public override XmlDocument GetFormXml()
    {
        // asynchronously prepared xml
        if (_preparedFormXml != null)
        {
            XmlDocument result = _preparedFormXml;
            _preparedFormXml = null;
            return result;
        }
        XmlDocument formXml = OrigamEngine
            .ModelXmlBuilders.FormXmlBuilder.GetXml(menuId: new Guid(g: this.Request.ObjectId))
            .Document;
        XmlNodeList list = formXml.SelectNodes(xpath: "/Window");
        XmlElement windowElement = list[i: 0] as XmlElement;
        // The SuppressSave attribute causes the Save button to disappear.
        // It should not be set to true if there is at least one editable field on the screen. The final result can be
        // determined only after the whole screen xml has been created.
        if (
            windowElement.GetAttribute(name: "SuppressSave") == "true"
            && formXml.SelectNodes(xpath: "//Property[@ReadOnly='false']")?.Count > 0
        )
        {
            windowElement.SetAttribute(name: "SuppressSave", value: "false");
        }
        if (windowElement.GetAttribute(name: "SuppressSave") == "true")
        {
            this.SuppressSave = true;
        }
        return formXml;
    }

    public override void PrepareFormXml()
    {
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Preparing XML...");
        }
        _preparedFormXml = GetFormXml();
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "XML prepared...");
        }
    }

    private object Refresh()
    {
        object currentRecordId = this.CurrentRecordId;
        this.CurrentRecordId = null;
        this.Clear();
        LoadData();
        // load current record again so CurrentRecordId remains set
        // this is important e.g. for paged data loading so we know
        // for which record initialPage should be returned
        if (currentRecordId != null && this.DataListEntity != null)
        {
            try
            {
                GetRowData(
                    entity: this.DataListEntity,
                    id: currentRecordId,
                    ignoreDirtyState: false
                );
            }
            catch (ArgumentOutOfRangeException)
            {
                // in case the original current record does not exist in the
                // context anymore after refresh, we do not set the current
                // record id
            }
        }
        if (this.DataList != null)
        {
            return this.DataList;
        }

        return this.Data;
    }
    #endregion
    #region Properties
    public string DelayedLoadingParameterName
    {
        get { return _delayedLoadingParameterName; }
        set { _delayedLoadingParameterName = value; }
    }
    #endregion
    public override void OnDispose()
    {
        base.OnDispose();
        this.CurrentRecordId = null;
    }
}
