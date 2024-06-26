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

using Origam.DA;

namespace Origam.Workbench.Services;
/// <summary>
/// Summary description for LoggingService.
/// </summary>
public class LoggingService : ILoggingService
{
	private SchemaService _schema = ServiceManager.Services.GetService(typeof(SchemaService)) as SchemaService;
	IServiceAgent _dataServiceAgent;
	public LoggingService()
	{
		_dataServiceAgent = (ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService).GetAgent("DataService", null, null);
	}
	
	#region ILoggingService Members
	public void LogProcess(Guid id, Guid workflowId, Guid formId, string processName, DateTime start, DateTime finish, string remark, string errorInfo)
	{
        UserProfile profile = SecurityManager.CurrentUserProfile();
		OrigamProcessLogData log = new OrigamProcessLogData();
		OrigamProcessLogData.OrigamProcessLogRow row = log.OrigamProcessLog.NewOrigamProcessLogRow();
		row.Id = id;
		row.WorkflowId = workflowId;
		row.FormId = formId;
		row.ProcessName = processName;
		row.Start = start;
		row.Finish = finish;
		row.Remark = remark;
		row.ErrorInfo = errorInfo;
		row.FinishError = (errorInfo != null);
		row.RecordCreated = DateTime.Now;
		row.RecordCreatedBy = profile.Id;
		log.OrigamProcessLog.AddOrigamProcessLogRow(row);
		SaveLog(log);
	}
	public void LogProcessStart(Guid id, Guid workflowId, Guid formId, string processName, DateTime start, string remark)
	{
		LogProcess(id, workflowId, formId, processName, start, DateTime.MinValue, remark, null);
	}
	public void LogProcessEnd(Guid logId, DateTime finish, string remark, string errorInfo)
	{
        UserProfile profile = SecurityManager.CurrentUserProfile();
    }
	#endregion
	#region Private Methods
	private void SaveLog(OrigamProcessLogData log)
	{
		// store to the database
		DataStructureQuery query = new DataStructureQuery(
			new Guid("9b8e3021-e9ac-447e-8107-703382c740b1"));
		_dataServiceAgent.MethodName = "StoreDataByQuery";
		_dataServiceAgent.Parameters.Clear();
		_dataServiceAgent.Parameters.Add("Query", query);
		_dataServiceAgent.Parameters.Add("Data", log);
		_dataServiceAgent.Run();
	}
	private OrigamProcessLogData LoadLog(Guid id)
	{
		return null;
	}
	#endregion
}
