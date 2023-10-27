#region license
/*
Copyright 2005 - 2023 Advantage Solutions, s. r. o.

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
using System.Globalization;
using Origam.DA;
using Origam.DA.ObjectPersistence;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;

namespace Origam.Server;

public class NewRecordSessionStore : FormSessionStore
{
    public NewRecordSessionStore(IBasicUIService service, UIRequest request,
        string name, FormReferenceMenuItem menuItem, Analytics analytics) 
        : base(service, request, name, menuItem, analytics)
    {
        IsModalDialog = true;
    }

    public NewRecordSessionStore(IBasicUIService service, UIRequest request,
        string name, Analytics analytics)
        : base(service, request, name, analytics)
    {
        IsModalDialog = true;
    }

    public override void Init()
    {
        IPersistenceService persistence = ServiceManager.Services.GetService<IPersistenceService>();
        IPersistenceProvider schemaProvider = persistence.SchemaProvider;
        var formMenuItem = schemaProvider.RetrieveInstance<FormReferenceMenuItem>(new Guid(Request.ObjectId));
        var screen = schemaProvider.RetrieveInstance<FormControlSet>(formMenuItem.ScreenId);
        var dataStructure = schemaProvider.RetrieveInstance<DataStructure>(screen.DataSourceId);
        var rootEntity = ((DataStructureEntity)dataStructure.Entities[0])!.RootEntity;
           
        var dataService  = Workbench.Services.CoreServices.DataServiceFactory.GetDataService();
        var dataSet = dataService.GetEmptyDataSet(
            rootEntity.ParentItemId, CultureInfo.InvariantCulture);
        var table = dataSet.Tables[rootEntity.Name];
        var row = table.NewRow();
            
        DatasetTools.ApplyPrimaryKey(row);
        DatasetTools.UpdateOrigamSystemColumns(
            row, true, SecurityManager.CurrentUserProfile().Id);
        row.Table.NewRow();
        
        Workbench.Services.CoreServices.DataService.Instance.StoreData(
            dataStructureId: rootEntity.ParentItemId,
            data: row.Table.DataSet,
            loadActualValuesAfterUpdate: false,
            transactionId: null);
        dataSet.Tables[rootEntity.Name].Rows.Add(row);
        SetDataSource(dataSet);
    }
}