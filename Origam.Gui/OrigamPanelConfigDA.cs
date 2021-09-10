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

namespace Origam.Gui
{
	public enum OrigamPanelViewMode
	{
		Form = 0,
		Grid = 1,
		Calendar = 2,
		Pipeline = 3,
		Chart = 4,
		Map = 5,
		VisualEditor = 6
}

	/// <summary>
	/// Summary description for OrigamPanelConfigDA.
	/// </summary>
	public class OrigamPanelConfigDA
	{
		public static DataRow CreatePanelConfigRow(DataTable configTable, Guid panelInstanceId, Guid workflowId, Guid profileId, OrigamPanelViewMode defaultView)
		{
			DataRow row = configTable.NewRow();
			row["Id"] = Guid.NewGuid();
			row["RecordCreated"] = DateTime.Now;
			row["RecordCreatedBy"] = profileId;
			row["FormPanelId"] = panelInstanceId;
			if (workflowId != Guid.Empty) row["WorkflowId"] = workflowId;
			row["ProfileId"] = profileId;
			row["DefaultView"] = defaultView;

			configTable.Rows.Add(row);

			return row;
		}

		public static DataSet LoadConfigData(Guid panelInstanceId, Guid workflowId, Guid profileId)
		{
			// here we load the user's defaults
			// DS: 218890ad-cd12-43a9-9166-4a02028d6125
			// Filter Form: 59d68a5a-0f9c-405b-957b-1d698535310e (OrigamFormPanelConfig_parFormPanelId, OrigamFormPanelConfig_parProfileId)
			// Filter Workflow: 3d5bb832-50d7-44d8-a7c5-abdf88d5df74 (OrigamFormPanelConfig_parFormPanelId, OrigamFormPanelConfig_parProfileId, OrigamFormPanelConfig_parWorkflowId)

			DataStructureQuery query;

			if (workflowId == Guid.Empty)
			{
				// we are in a form view
				query = new DataStructureQuery(
					new Guid("218890ad-cd12-43a9-9166-4a02028d6125"),
					new Guid("59d68a5a-0f9c-405b-957b-1d698535310e"));
			}
			else
			{
				// we are in a workflow view
				query = new DataStructureQuery(
					new Guid("218890ad-cd12-43a9-9166-4a02028d6125"),
					new Guid("3d5bb832-50d7-44d8-a7c5-abdf88d5df74"));

				query.Parameters.Add(new QueryParameter("OrigamFormPanelConfig_parWorkflowId", workflowId));
			}

			query.Parameters.Add(new QueryParameter("OrigamFormPanelConfig_parFormPanelId", panelInstanceId));
			query.Parameters.Add(new QueryParameter("OrigamFormPanelConfig_parProfileId", profileId));

			IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);

			dataServiceAgent.MethodName = "LoadDataByQuery";
			dataServiceAgent.Parameters.Clear();
			dataServiceAgent.Parameters.Add("Query", query);

			dataServiceAgent.Run();

			return dataServiceAgent.Result as DataSet;
		}

		public static void SaveUserConfig(DataSet userConfig, Guid panelInstanceId, Guid workflowId, Guid profileId)
		{
			try
			{
				if (userConfig == null) return;

				DataSet configCopy = userConfig.Copy();
				DataSet actualConfig = LoadConfigData(panelInstanceId, workflowId, profileId);

				if (actualConfig.Tables[0].Rows.Count > 0)
				{
					configCopy.AcceptChanges();
				}
				DatasetTools.MergeDataSet(userConfig, configCopy, null, new MergeParams(profileId));
				SaveConfigData(userConfig);
			}
			catch { }
		}

		public static void DeleteUserConfig(Guid screenSectionId, Guid workflowId, Guid profileId)
		{
			DataSet actualConfig = LoadConfigData(screenSectionId, workflowId, profileId);
			if (actualConfig.Tables["OrigamFormPanelConfig"].Rows.Count > 0)
			{
				actualConfig.Tables["OrigamFormPanelConfig"].Rows[0].Delete();
			}
			SaveConfigData(actualConfig);
		}

		private static void SaveConfigData(DataSet userConfig)
		{
			try
			{
				DataStructureQuery query = new DataStructureQuery(
					new Guid("218890ad-cd12-43a9-9166-4a02028d6125"));
				IServiceAgent dataServiceAgent = (ServiceManager.Services.GetService<IBusinessServicesService>()).GetAgent("DataService", null, null);

				dataServiceAgent.MethodName = "StoreDataByQuery";
				dataServiceAgent.Parameters.Clear();
				dataServiceAgent.Parameters.Add("Query", query);
				dataServiceAgent.Parameters.Add("Data", userConfig);

				dataServiceAgent.Run();
			}
			catch { }
		}
	}
}
