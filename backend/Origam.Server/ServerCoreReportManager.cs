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
using Origam.DA;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;

namespace Origam.Server;

public class ServerCoreReportManager : IReportManager
{
    private readonly SessionManager sessionManager;

    public ServerCoreReportManager(SessionManager sessionManager)
    {
        this.sessionManager = sessionManager;
    }

    public string GetReport(
        string sessionFormIdentifier,
        string entity,
        object id,
        string reportId,
        Hashtable parameterMappings
    )
    {
        Guid key = Guid.NewGuid();
        SessionStore sessionStore = sessionManager.GetSession(
            sessionFormIdentifier: new Guid(g: sessionFormIdentifier)
        );
        DataRow row = sessionStore.GetSessionRow(entity: entity, id: id);
        Hashtable resultParams = DatasetTools.RetrieveParameters(
            parameterMappings: parameterMappings,
            rows: new List<DataRow> { row }
        );
        sessionManager.AddReportRequest(
            key: key,
            request: new ReportRequest(reportId: reportId, parameters: resultParams)
        );
        return ReportRequestKeyToUrl(key: key);
    }

    public string GetReportFromMenu(Guid menuId)
    {
        Guid key = Guid.NewGuid();
        IPersistenceService persistenceService =
            ServiceManager.Services.GetService<IPersistenceService>();
        ReportReferenceMenuItem reportReferenceMenuItem =
            persistenceService.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractMenuItem),
                primaryKey: new ModelElementKey(id: menuId)
            ) as ReportReferenceMenuItem;
        sessionManager.AddReportRequest(
            key: key,
            request: new ReportRequest(
                reportId: reportReferenceMenuItem.ReportId.ToString(),
                parameters: new Hashtable(),
                dataReportExportFormatType: reportReferenceMenuItem.ExportFormatType
            )
        );
        return ReportRequestKeyToUrl(key: key);
    }

    public string GetReportStandalone(
        string reportId,
        Hashtable parameters,
        DataReportExportFormatType dataReportExportFormatType
    )
    {
        Guid key = Guid.NewGuid();
        sessionManager.AddReportRequest(
            key: key,
            request: new ReportRequest(
                reportId: reportId,
                parameters: parameters,
                dataReportExportFormatType: dataReportExportFormatType
            )
        );
        return ReportRequestKeyToUrl(key: key);
    }

    private static string ReportRequestKeyToUrl(Guid key)
    {
        return "internalApi/Report/" + key;
    }
}
