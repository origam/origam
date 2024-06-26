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

namespace Origam.Workbench.Services;
/// <summary>
/// Summary description for ITracingService.
/// </summary>
public interface ITracingService : IWorkbenchService
{
	void TraceWorkflow(Guid workflowInstanceId, Guid workflowId, string workflowName);
	void TraceStep(Guid workflowInstanceId, string stepPath, Guid stepId, string category, string subCategory, string remark, string data1, string data2, string message);
	void TraceRule(Guid ruleId, string ruleName, string ruleInput,
		string ruleResult,  Guid workflowInstanceId);		
	void TraceRule(Guid ruleId, string ruleName, string ruleInput,
		string ruleResult);
	bool Enabled { get; set; }
}
