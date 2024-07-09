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

using Origam.Workbench.Services;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Rule;
using Origam.DA.Service;
using System.Data;
using System.Collections.Generic;

namespace Origam.Workflow;
public abstract class AbstractServiceAgent : IServiceAgent
{
	public event EventHandler PersistenceProviderChanged;
	public event ServiceFinished Finished;
	public AbstractServiceAgent()
	{
	}
    public virtual void SetDataService(DA.IDataService dataService)
    {
    }
    protected TraceTaskInfo TraceTaskInfo
    {
        get
        {
            TraceTaskInfo traceTaskInfo = new TraceTaskInfo();
            traceTaskInfo.Trace = Trace;
            traceTaskInfo.TraceStepName = TraceStepName;
            traceTaskInfo.TraceStepId = TraceStepId;
            traceTaskInfo.TraceWorkflowId = TraceWorkflowId;
            return traceTaskInfo;
        }
    }
    public DataSet CreateEmptyOutputData()
    {
        DatasetGenerator dg = new DatasetGenerator(true);
        DataSet data = dg.CreateDataSet(OutputStructure as DataStructure);
        data.EnforceConstraints = !DisableOutputStructureConstraints;
        return data;
    }
    #region IServiceAgent Members
    public virtual string Info => this.ToString();
    Origam.DA.ObjectPersistence.IPersistenceProvider _persistence;
	public Origam.DA.ObjectPersistence.IPersistenceProvider PersistenceProvider
	{
		get => _persistence;
		set
		{
			_persistence = value;
			OnPersistenceProviderChanged(EventArgs.Empty);
		}
	}
	RuleEngine _ruleEngine;
	public object RuleEngine
	{
		get
		{
			return _ruleEngine;
		}
		set
		{
			_ruleEngine = value as RuleEngine;
		}
	}
	WorkflowEngine _workflowEngine;
	public object WorkflowEngine
	{
		get
		{
			return _workflowEngine;
		}
		set
		{
			_workflowEngine = value as WorkflowEngine;
		}
	}
	public virtual Hashtable Parameters { get; } = new Hashtable();
	public virtual string MethodName { get; set; }
	public virtual string TransactionId { get; set; } = null;
	private AbstractDataStructure _outputStructure;
	public ISchemaItem OutputStructure
	{
		get
		{
			return _outputStructure;
		}
		set
		{
			_outputStructure = value as AbstractDataStructure;
		}
	}
	public bool DisableOutputStructureConstraints { get; set; }
	public ServiceOutputMethod OutputMethod { get; set; }
	public string TraceStepName { get; set; }
	public Guid TraceStepId { get; set; }
	public Guid TraceWorkflowId { get; set; }
	public bool Trace { get; set; }
	public virtual string ExecuteUpdate(string command, string transactionId)
	{
		throw new Exception("ExecuteUpdate not implemented by the service.");
	}
	public abstract object Result	{get;}
	public abstract void Run();
	public void RunAsync()
	{
		Exception exception = null;
		try
		{
			this.Run();
		}
		catch(Exception ex)
		{
			exception = ex;
		}
		OnFinished(new ServiceFinishedEventArgs(exception));
	}
	public virtual IList<string> ExpectedParameterNames(ISchemaItem item, string method, string parameter)
	{
		return new List<string>();
	}
	#endregion
	#region Event
	void OnPersistenceProviderChanged(EventArgs e)
	{
		if (PersistenceProviderChanged != null) 
		{
			PersistenceProviderChanged(this, e);
		}
	}
	void OnFinished(ServiceFinishedEventArgs e)
	{
		if (Finished != null) 
		{
			Finished(this, e);
		}
	}
	#endregion
}
