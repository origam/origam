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
    public NewRecordSessionStore(
        IBasicUIService service,
        UIRequest request,
        string name,
        Analytics analytics
    )
        : base(service: service, request: request, name: name, analytics: analytics)
    {
        IsModalDialog = true;
    }

    public override void Init()
    {
        var persistence = ServiceManager.Services.GetService<IPersistenceService>();
        var schemaProvider = persistence.SchemaProvider;
        var formMenuItem = schemaProvider.RetrieveInstance<FormReferenceMenuItem>(
            instanceId: new Guid(g: Request.ObjectId)
        );
        var screen = schemaProvider.RetrieveInstance<FormControlSet>(
            instanceId: formMenuItem.ScreenId
        );
        var dataStructure = schemaProvider.RetrieveInstance<DataStructure>(
            instanceId: screen.DataSourceId
        );
        var rootEntity = ((DataStructureEntity)dataStructure.Entities[index: 0])!.RootEntity;
        var dataService = DataServiceFactory.GetDataService();
        var dataSet = dataService.GetEmptyDataSet(
            dataStructureId: rootEntity.ParentItemId,
            culture: CultureInfo.InvariantCulture
        );
        var table = dataSet.Tables[name: rootEntity.Name];
        var row = table!.NewRow();
        DatasetTools.ApplyPrimaryKey(row: row);
        DatasetTools.UpdateOrigamSystemColumns(
            row: row,
            isNew: true,
            profileId: SecurityManager.CurrentUserProfile().Id
        );
        dataSet.RemoveNullConstraints();
        table.Rows.Add(row: row);
        SetDataSource(dataSource: dataSet);
        FillInitialValues(row: row);
    }

    private void FillInitialValues(DataRow row)
    {
        try
        {
            RegisterEvents();
            // we're sorting column names in order to introduce
            // a level of predictability in the order of rule processing
            var sortedColumnNames = Request.NewRecordInitialValues.Keys.CastToList<string>();
            sortedColumnNames.Sort();
            foreach (
                var columnName in sortedColumnNames.Where(predicate: columnName =>
                    Request.NewRecordInitialValues[key: columnName] != null
                )
            )
            {
                row[columnName: columnName] = Request.NewRecordInitialValues[key: columnName]!;
            }
        }
        finally
        {
            UnregisterEvents();
        }
    }
}
