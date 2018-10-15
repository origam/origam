using System;
using System.Data;

namespace Origam.Workbench.Services
{
	public enum StateMachineServiceStatelessEventType
	{
		RecordCreated = 0,
		RecordUpdated = 1,
		RecordDeleted = 2
	}

	/// <summary>
	/// Summary description for IStateMachineService.
	/// </summary>
	public interface IStateMachineService
	{
		object[] AllowedStateValues(Guid entityId, Guid fieldId, object currentStateValue, DataRow dataRow, string transactionId);
		bool IsStateAllowed(Guid entityId, Guid fieldId, object currentStateValue, object newStateValue, DataRow dataRow, string transactionId);
		bool ExecutePreEvents(Guid entityId, Guid fieldId, object currentStateValue, object newStateValue, DataRow dataRow, string transactionId);
		bool ExecutePostEvents(Guid entityId, Guid fieldId, object currentStateValue, object newStateValue, DataRow dataRow, string transactionId);
		bool ExecuteStatelessEvents(Guid entityId, StateMachineServiceStatelessEventType eventType, DataRow dataRow, string transactionId);
	}
}
