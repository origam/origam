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
using System.Data;
using Origam.DA;
using Origam.Workbench.Services;

namespace Origam.Gui;

/// <summary>
/// Summary description for OrigamPanelColumnConfigDA.
/// </summary>
public class OrigamPanelColumnConfigDA
{
    public static void PersistColumnConfig(
        Guid panelId,
        string columnName,
        int position,
        int width,
        bool hidden
    )
    {
        try
        {
            // store column width to the database
            UserProfile profile = SecurityManager.CurrentUserProfile();
            // see if this column already has a config
            OrigamPanelColumnConfig config = LoadColumnConfig(
                columnName: columnName,
                profileId: profile.Id,
                panelId: panelId
            );
            OrigamPanelColumnConfig.PanelColumnConfigRow configRow;
            if (config.PanelColumnConfig.Rows.Count == 0)
            {
                configRow = config.PanelColumnConfig.NewPanelColumnConfigRow();
                configRow.Id = Guid.NewGuid();
                configRow.ColumnName = columnName;
                configRow.PanelId = panelId;
                configRow.ProfileId = profile.Id;
                configRow.RecordCreated = DateTime.Now;
                configRow.RecordCreatedBy = profile.Id;
                configRow.ColumnWidth = 0;
                config.PanelColumnConfig.AddPanelColumnConfigRow(row: configRow);
            }
            else
            {
                configRow =
                    config.PanelColumnConfig.Rows[index: 0]
                    as OrigamPanelColumnConfig.PanelColumnConfigRow;
                configRow.RecordUpdated = DateTime.Now;
                configRow.RecordUpdatedBy = profile.Id;
            }

            configRow.ColumnWidth = width;
            configRow.IsHidden = hidden;
            configRow.Position = position;
            // store the new width
            PersistColumnConfig(config: config);
        }
        catch
        {
            // it is not really important to store the column's width, so we just consume the exception
        }
    }

    public static void PersistColumnConfig(OrigamPanelColumnConfig config)
    {
        IServiceAgent dataServiceAgent = (
            ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
            as IBusinessServicesService
        ).GetAgent(serviceType: "DataService", ruleEngine: null, workflowEngine: null);
        // persist filters to the database
        DataStructureQuery query = new DataStructureQuery(
            dataStructureId: new Guid(g: "e3966697-e2d6-4538-b7e7-30973ae1af54")
        );
        dataServiceAgent.MethodName = "StoreDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: query);
        dataServiceAgent.Parameters.Add(key: "Data", value: config);
        dataServiceAgent.Run();
    }

    public static OrigamPanelColumnConfig LoadColumnConfig(
        string columnName,
        Guid profileId,
        Guid panelId
    )
    {
        IServiceAgent dataServiceAgent = (
            ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
            as IBusinessServicesService
        ).GetAgent(serviceType: "DataService", ruleEngine: null, workflowEngine: null);
        OrigamPanelColumnConfig result = new OrigamPanelColumnConfig();
        // retrieve filters for the current panel from the database
        DataStructureQuery query = new DataStructureQuery(
            dataStructureId: new Guid(g: "e3966697-e2d6-4538-b7e7-30973ae1af54"),
            methodId: new Guid(g: "c621a8e7-a13a-414d-9db8-192698916ed7")
        );
        query.Parameters.Add(
            value: new QueryParameter(
                _parameterName: "PanelColumnConfig_parPanelId",
                value: panelId
            )
        );
        query.Parameters.Add(
            value: new QueryParameter(
                _parameterName: "PanelColumnConfig_parProfileId",
                value: profileId
            )
        );
        query.Parameters.Add(
            value: new QueryParameter(
                _parameterName: "PanelColumnConfig_parColumnName",
                value: columnName
            )
        );
        dataServiceAgent.MethodName = "LoadDataByQuery";
        dataServiceAgent.Parameters.Clear();
        dataServiceAgent.Parameters.Add(key: "Query", value: query);
        dataServiceAgent.Run();
        result.Merge(dataSet: dataServiceAgent.Result as DataSet);
        return result;
    }

    public static OrigamPanelColumnConfig LoadUserConfig(Guid panelId, Guid profileId)
    {
        IServiceAgent dataServiceAgent = (
            ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
            as IBusinessServicesService
        ).GetAgent(serviceType: "DataService", ruleEngine: null, workflowEngine: null);
        OrigamPanelColumnConfig result = new OrigamPanelColumnConfig();
        try
        {
            // retrieve filters for the current panel from the database
            DataStructureQuery query = new DataStructureQuery(
                dataStructureId: new Guid(g: "e3966697-e2d6-4538-b7e7-30973ae1af54"),
                methodId: new Guid(g: "97b0ab43-e17a-45ab-b2a6-49f0944d604c")
            );
            query.Parameters.Add(
                value: new QueryParameter(
                    _parameterName: "PanelColumnConfig_parPanelId",
                    value: panelId
                )
            );
            query.Parameters.Add(
                value: new QueryParameter(
                    _parameterName: "PanelColumnConfig_parProfileId",
                    value: profileId
                )
            );
            dataServiceAgent.MethodName = "LoadDataByQuery";
            dataServiceAgent.Parameters.Clear();
            dataServiceAgent.Parameters.Add(key: "Query", value: query);
            dataServiceAgent.Run();
            result.Merge(dataSet: dataServiceAgent.Result as DataSet);
        }
        catch { }
        return result;
    }
}
