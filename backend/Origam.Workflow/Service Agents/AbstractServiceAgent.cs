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

namespace Origam.Workflow
{
	/// <summary>
	/// Summary description for AbstractServiceAgent.
	/// </summary>
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

		public T TryGetParameter<T>(string parameterName)
		{
			if (Parameters.Contains(parameterName))
			{
				object value = Parameters[parameterName];
				if (value is T t)
				{
					return t;
				}
			}
			return default(T);
		}

		public T GetParameter<T>(string parameterName)
        {
            if (!Parameters.Contains(parameterName))
            {
				throw new ArgumentOutOfRangeException(
					$"Missing parameter {parameterName}");
            }
			object value = Parameters[parameterName];
            if (value is T t)
            {
				return t;
            }
            else
            {
				throw new ArgumentOutOfRangeException($"Parameter {parameterName} should be of type {typeof(T)}");
            }
        }

        #region IServiceAgent Members
        public virtual string Info
		{
			get
			{
				return this.ToString();
			}
		}

		Origam.DA.ObjectPersistence.IPersistenceProvider _persistence;
		public Origam.DA.ObjectPersistence.IPersistenceProvider PersistenceProvider
		{
			get
			{
				return _persistence;
			}
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

		Hashtable _parameters = new Hashtable();
		public System.Collections.Hashtable Parameters
		{
			get
			{
				return _parameters;
			}
		}

		private string _methodName;
		public string MethodName
		{
			get
			{
				return _methodName;
			}
			set
			{
				_methodName = value;
			}
		}

		private string _transactionId = null;
		public string TransactionId
		{
			get
			{
				return _transactionId;
			}
			set
			{
				_transactionId = value;
			}
		}

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

        private bool _disableOutputStructureConstraints;
        public bool DisableOutputStructureConstraints
        {
            get
            {
                return _disableOutputStructureConstraints;
            }
            set
            {
                _disableOutputStructureConstraints = value;
            }
        }

        private ServiceOutputMethod _outputMethod;
		public ServiceOutputMethod OutputMethod
		{
			get
			{
				return _outputMethod;
			}
			set
			{
				_outputMethod = value;
			}
		}


		string _traceStepName;
		public string TraceStepName
		{
			get
			{
				return _traceStepName;
			}
			set
			{
				_traceStepName = value;
			}
		}
		
		Guid _traceStepId;
		public Guid TraceStepId
		{
			get
			{
				return _traceStepId;
			}
			set
			{
				_traceStepId = value;
			}
		}

		Guid _traceWorkflowId;
		public Guid TraceWorkflowId
		{
			get
			{
				return _traceWorkflowId;
			}
			set
			{
				_traceWorkflowId = value;
			}
		}

		bool _trace;
		public bool Trace
		{
			get
			{
				return _trace;
			}
			set
			{
				_trace = value;
			}
		}

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

		public virtual IList<string> ExpectedParameterNames(AbstractSchemaItem item, string method, string parameter)
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
}
