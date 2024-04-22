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

using Origam.Schema;
using Origam.DA.ObjectPersistence;
using System.Collections.Generic;
using Origam.Workflow;

namespace Origam.Workbench.Services;

/// <summary>
/// Summary description for IServiceAgent.
/// </summary>
public interface IServiceAgent
{
	event ServiceFinished Finished;

	IPersistenceProvider PersistenceProvider{get; set;}
	object RuleEngine{get; set;}
	object WorkflowEngine{get; set;}
	Hashtable Parameters{get;}
	string MethodName{get; set;}
	ISchemaItem OutputStructure{get; set;}
	bool DisableOutputStructureConstraints { get; set; }
	ServiceOutputMethod OutputMethod{get; set;}
	void SetDataService(DA.IDataService dataService);

	string TransactionId{get; set;}

	string Info{get;}

	string TraceStepName{get; set;}
	Guid TraceStepId{get; set;}
	Guid TraceWorkflowId{get; set;}
	bool Trace{get; set;}
		
	string ExecuteUpdate(string command, string transactionId);

	object Result{get;}
	void Run();
	void RunAsync();

	IList<string> ExpectedParameterNames(AbstractSchemaItem item, string method, string parameter);
}