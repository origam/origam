#region license
/*
Copyright 2005 - 2019 Advantage Solutions, s. r. o.

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
using System.Reflection;
using Origam.DA;

namespace Origam.Workbench.Services
{
	/// <summary>
	/// Summary description for TracingService.
	/// </summary>
	public class TracingService : ITracingService
	{
		private static readonly log4net.ILog log =
			log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

			
		
		private SchemaService _schema;
		IServiceAgent _dataServiceAgent;

		public TracingService()
		{
		}

		#region Public Methods
		public void TraceWorkflow(Guid workflowInstanceId, Guid workflowId, string workflowName)
		{
			if (IsTraceDisabled())
			{
				if (log.IsDebugEnabled)
				{
					log.Info("Trace is disabled, workflow is not traced.");
				}
				return;
			}

            UserProfile profile = SecurityManager.CurrentUserProfile();

			// create the record
			OrigamTraceWorkflowData data = new OrigamTraceWorkflowData();

			OrigamTraceWorkflowData.OrigamTraceWorkflowRow row = data.OrigamTraceWorkflow.NewOrigamTraceWorkflowRow();

			row.Id = workflowInstanceId;
			row.RecordCreated = DateTime.Now;
			row.RecordCreatedBy = profile.Id;
			row.WorkflowName = workflowName;
			row.WorkflowId = workflowId;

			data.OrigamTraceWorkflow.AddOrigamTraceWorkflowRow(row);

			// store to the database
			DataStructureQuery query = new DataStructureQuery(
				new Guid("309843cc-39ec-4eca-8848-8c69c885790c"));

			_dataServiceAgent.MethodName = "StoreDataByQuery";
			_dataServiceAgent.Parameters.Clear();
			_dataServiceAgent.Parameters.Add("Query", query);
			_dataServiceAgent.Parameters.Add("Data", data);

			_dataServiceAgent.Run();
		}

		public void TraceStep(Guid workflowInstanceId, string stepPath, Guid stepId,
			string category, string subCategory, string remark, string data1,
			string data2, string message)
		{
			if (IsTraceDisabled())
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("Trace is disabled, step is not traced.");
				}
				return;
			}
			
			try
			{
                UserProfile profile = SecurityManager.CurrentUserProfile();

				// create the record
				OrigamTraceWorkflowStepData data = new OrigamTraceWorkflowStepData();

				OrigamTraceWorkflowStepData.OrigamTraceWorkflowStepRow row = data.OrigamTraceWorkflowStep.NewOrigamTraceWorkflowStepRow();

				row.Id = Guid.NewGuid();
				row.RecordCreated = DateTime.Now;
				row.RecordCreatedBy = profile.Id;
			
				row.Category = category;
				if(data1 != null) row.Data1 = data1;
				if(data2 != null) row.Data2 = data2;
				if(message != null) row.Message = message;
				row.refOrigamTraceWorkflowId = workflowInstanceId;
				if(remark != null) row.Remark = remark;
				row.Subcategory = subCategory;
				row.WorkflowStepId = stepId;
				row.WorkflowStepPath = stepPath;

				data.OrigamTraceWorkflowStep.AddOrigamTraceWorkflowStepRow(row);

				// store to the database
				DataStructureQuery query = new DataStructureQuery(
					new Guid("4985a6b2-8bae-4c21-9a09-0c2480c4efe2"));

				_dataServiceAgent.MethodName = "StoreDataByQuery";
				_dataServiceAgent.Parameters.Clear();
				_dataServiceAgent.Parameters.Add("Query", query);
				_dataServiceAgent.Parameters.Add("Data", data);

				_dataServiceAgent.Run();
			}
			catch 
			{
			}
		}

		private bool IsTraceDisabled()
		{
			OrigamSettings settings 
				= (OrigamSettings)ConfigurationManager.GetActiveConfiguration();
			return !settings.TraceEnabled;
		}

		#endregion

		#region IService Members

		public void UnloadService()
		{
			_dataServiceAgent = null;
			_schema = null;
		}

		public void InitializeService()
		{
			_dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);
			_schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
		}

		#endregion
	}
}
