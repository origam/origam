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
using System.Data;
using System.Globalization;
using System.Linq;
using Origam.DA;
using Origam.Extensions;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;
using Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

public class NewRecordSessionStore : FormSessionStore
{
    public NewRecordSessionStore(IBasicUIService service, UIRequest request,
        string name, Analytics analytics)
        : base(service, request, name, analytics)
    {
        IsModalDialog = true;
    }

    public override void Init()
    {
        var persistence 
            = ServiceManager.Services.GetService<IPersistenceService>();
        var schemaProvider = persistence.SchemaProvider;
        var formMenuItem = schemaProvider
            .RetrieveInstance<FormReferenceMenuItem>(
                new Guid(Request.ObjectId));
        var screen = schemaProvider.RetrieveInstance<FormControlSet>(
            formMenuItem.ScreenId);
        var dataStructure = schemaProvider.RetrieveInstance<DataStructure>(
            screen.DataSourceId);
        var rootEntity 
            = ((DataStructureEntity)dataStructure.Entities[0])!.RootEntity;
        var dataService  = DataServiceFactory.GetDataService();
        var dataSet = dataService.GetEmptyDataSet(
            rootEntity.ParentItemId, CultureInfo.InvariantCulture);
        var table = dataSet.Tables[rootEntity.Name];
        var row = table!.NewRow();
        DatasetTools.ApplyPrimaryKey(row);
        DatasetTools.UpdateOrigamSystemColumns(
            row, true, SecurityManager.CurrentUserProfile().Id);
        dataSet.RemoveNullConstraints();
        table.Rows.Add(row);
        SetDataSource(dataSet);
        FillInitialValues(row);
    }

    private void FillInitialValues(DataRow row)
    {
        try
        {
            RegisterEvents();
            // we're sorting column names in order to introduce
            // a level of predictability in the order of rule processing
            var sortedColumnNames = Request.NewRecordInitialValues.Keys.ToList<string>();
            sortedColumnNames.Sort();
            foreach (var columnName in sortedColumnNames
                         .Where(columnName 
                             => Request.NewRecordInitialValues[columnName] != null))
            {
                row[columnName] = Request.NewRecordInitialValues[columnName]!;
            }
        }
        finally
        {
            UnregisterEvents();
        }
    }
}