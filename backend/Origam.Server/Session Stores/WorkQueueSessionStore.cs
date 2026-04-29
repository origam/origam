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
using System.Xml;
using Origam.Gui;
using Origam.Schema.EntityModel;
using Origam.Schema.WorkflowModel;
using Origam.Server.Session_Stores;
using Origam.Workbench.Services;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

public class WorkQueueSessionStore : SessionStore
{
    private readonly object getRowDataLock = new();
    private readonly DataSetBuilder dataSetBuilder = new();

    private WorkQueueClass workQueueClass;
    public WorkQueueClass WorkQueueClass => workQueueClass;

    private WorkQueueCustomScreen customScreen;
    private Guid dataStructureId;
    private Guid? methodId;

    public WorkQueueSessionStore(
        IBasicUIService service,
        UIRequest request,
        string name,
        Analytics analytics
    )
        : base(service: service, request: request, name: name, analytics: analytics)
    {
        RefreshOnInitUI = true;
        RetrieveWorkQueueClassAndCustomScreen(workQueueId: new Guid(g: request.ObjectId));
        InitializeMethodId();
        InitializeDataStructureId();
    }

    private void RetrieveWorkQueueClassAndCustomScreen(Guid workQueueId)
    {
        var workQueueService = ServiceManager.Services.GetService<IWorkQueueService>();
        workQueueClass = workQueueService.WQClass(queueId: workQueueId) as WorkQueueClass;
        if (workQueueClass == null)
        {
            throw new Exception(
                message: string.Format(
                    format: Resources.ErrorWorkQueueClassNotFound,
                    arg0: workQueueId
                )
            );
        }
        SortSet = workQueueClass.WorkQueueStructureSortSet;
        string customScreenName = workQueueService.CustomScreenName(queueId: workQueueId);
        if (string.IsNullOrEmpty(value: customScreenName))
        {
            return;
        }
        customScreen =
            workQueueClass.GetChildByName(
                name: customScreenName,
                itemType: WorkQueueCustomScreen.CategoryConst
            ) as WorkQueueCustomScreen;
        if (customScreen is null)
        {
            throw new Exception(
                message: string.Format(
                    format: Resources.ErrorSpecifiedCustomWorkQueueScreenNotFound,
                    arg0: customScreenName
                )
            );
        }
    }

    private void InitializeMethodId()
    {
        methodId =
            customScreen?.MethodId
            ?? workQueueClass
                .WorkQueueStructure.Methods.Cast<DataStructureFilterSet>()
                .FirstOrDefault(predicate: x => x.Name == "GetById")
                ?.Id;
        if (methodId == null)
        {
            throw new Exception(
                message: string.Format(
                    format: Resources.ErrorNoGetByIdFilterSet,
                    arg0: workQueueClass.WorkQueueStructure.Id
                )
            );
        }
    }

    private void InitializeDataStructureId()
    {
        dataStructureId =
            customScreen?.Screen.DataStructure.Id ?? workQueueClass.WorkQueueStructureId;
    }

    private void PrepareData()
    {
        var data = InitializeFullStructure();
        SetDataSource(dataSource: data);
        IsDelayedLoading = true;
        DataListEntity = "WorkQueueEntry";
    }

    private DataSet InitializeFullStructure()
    {
        return dataSetBuilder.InitializeFullStructure(id: dataStructureId, defaultSet: null);
    }

    public override List<ChangeInfo> GetRowData(string entity, object id, bool ignoreDirtyState)
    {
        var result = new List<ChangeInfo>();
        lock (getRowDataLock)
        {
            if (id == null)
            {
                CurrentRecordId = null;
                return result;
            }
            DataRow row = GetSessionRow(entity: entity, id: id);
            // for new rows we don't even try to load the data from the database
            if (row is not { RowState: DataRowState.Added })
            {
                if (!ignoreDirtyState && Data.HasChanges())
                {
                    throw new Exception(message: Resources.ErrorDataNotSavedWhileChangingRow);
                }
                CurrentRecordId = null;
                SetDataSource(
                    dataSource: CoreServices.DataService.Instance.LoadData(
                        dataStructureId: dataStructureId,
                        methodId: methodId!.Value,
                        defaultSetId: Guid.Empty,
                        sortSetId: Guid.Empty,
                        transactionId: null,
                        paramName1: "WorkQueueEntry_parId",
                        paramValue1: id
                    )
                );
            }
            CurrentRecordId = id;
            DataRow actualDataRow = GetSessionRow(entity: entity, id: id);
            if (actualDataRow == null)
            {
                throw new RowNotFoundException();
            }
            UpdateListRow(r: actualDataRow);
            ChangeInfo changeInfo = GetChangeInfo(
                requestingGrid: null,
                row: actualDataRow,
                operation: 0
            );
            result.Add(item: changeInfo);
            if (actualDataRow.RowState == DataRowState.Unchanged)
            {
                result.Add(item: ChangeInfo.SavedChangeInfo());
            }
        }
        return result;
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

    public override void Init()
    {
        PrepareData();
    }

    public override object ExecuteActionInternal(string actionId)
    {
        return actionId switch
        {
            ACTION_REFRESH => Refresh(),
            _ => throw new ArgumentOutOfRangeException(
                paramName: nameof(actionId),
                actualValue: actionId,
                message: Resources.ErrorContextUnknownAction
            ),
        };
    }

    public override XmlDocument GetFormXml()
    {
        XmlDocument formXml = OrigamEngine.ModelXmlBuilders.FormXmlBuilder.GetXml(
            workQueueClass: workQueueClass,
            dataset: Data,
            screenTitle: Request.Caption,
            customScreen: customScreen,
            queueId: new Guid(g: Request.ObjectId)
        );
        return formXml;
    }

    private object Refresh()
    {
        Init();
        return Data;
    }
}
