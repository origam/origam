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
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Data;
using System.Linq;
using Origam.Gui;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.Server.Session_Stores;
using core = Origam.Workbench.Services.CoreServices;
using Origam.Workbench.Services;

namespace Origam.Server;
public class WorkQueueSessionStore : SessionStore
{
    private object _getRowDataLock = new object();
    private DataSetBuilder datasetbuilder = new DataSetBuilder();
    
    public WorkQueueSessionStore(IBasicUIService service, UIRequest request, string name,
        Analytics analytics)
        : base(service, request, name, analytics)
    {
        IWorkQueueService wqs = ServiceManager.Services.GetService(typeof(IWorkQueueService)) as IWorkQueueService;
        Guid workQueueId = new Guid(request.ObjectId);
        this.WQClass = wqs.WQClass(workQueueId) as WorkQueueClass;
        this.SortSet = this.WQClass.WorkQueueStructureSortSet;
        this.RefreshOnInitUI = true;
    }
    #region Overriden SessionStore Methods
    private void PrepareData()
    {
        var data = InitializeFullStructure(null);
        SetDataSource(data);
        IsDelayedLoading = true;
        DataListEntity = "WorkQueueEntry";
    }
    private DataSet InitializeFullStructure(DataStructureDefaultSet defaultSet)
    {
        return datasetbuilder.InitializeFullStructure(WQClass.WorkQueueStructureId, defaultSet);
    }
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
    
            DataRow row = GetSessionRow(entity, id);
    
            // for new rows we don't even try to load the data from the database
            if (row == null || row.RowState != DataRowState.Added)
            {
                if (!ignoreDirtyState && this.Data.HasChanges())
                {
                    throw new Exception(Resources.ErrorDataNotSavedWhileChangingRow);
                }
    
                this.CurrentRecordId = null;
    
                SetDataSource(LoadDataPiece(id));
            }
    
            this.CurrentRecordId = id;
    
            DataRow actualDataRow = GetSessionRow(entity, id);
            if (actualDataRow == null)
            {
                throw new RowNotFoundException();
            }
            UpdateListRow(actualDataRow);
    
            ChangeInfo ci = GetChangeInfo(null, actualDataRow, 0);
            result.Add(ci);
    
            if (actualDataRow.RowState == DataRowState.Unchanged)
            {
                result.Add(ChangeInfo.SavedChangeInfo());
            }
        }
    
        return result;
    }
   
    private DataSet LoadDataPiece(object parentId)
    {
        Guid? methodId = WQClass
            .WorkQueueStructure.Methods.Cast<DataStructureFilterSet>()
            .FirstOrDefault(x => x.Name == "GetById")
            ?.Id;
        if (methodId == null)
        {
            throw new Exception($"Data structure filter set with name GetById was not found under WorkQueueStructure {WQClass.WorkQueueStructure.Id}");
        }
        return core.DataService.Instance.LoadData(WQClass.WorkQueueStructureId, methodId.Value, 
            Guid.Empty, WQClass.WorkQueueStructureSortSetId, null, 
            "WorkQueueEntry_parId", parentId);
    }
    public override void Init()
    {
        PrepareData();
    }
    public override object ExecuteActionInternal(string actionId)
    {
        switch (actionId)
        {
            case ACTION_REFRESH:
                return Refresh();
            default:
                throw new ArgumentOutOfRangeException("actionId", actionId, Resources.ErrorContextUnknownAction);
        }
    }
    public override XmlDocument GetFormXml()
    {
        XmlDocument formXml = OrigamEngine.ModelXmlBuilders.FormXmlBuilder.GetXml(WQClass, Data, Request.Caption, new Guid(Request.ObjectId));
        return formXml;
    } 
    private object Refresh()
    {
        Init();
        return this.Data;
    }
    #endregion
    private WorkQueueClass _wqClass;
    public WorkQueueClass WQClass
    {
        get { return _wqClass; }
        set { _wqClass = value; }
    }
}
