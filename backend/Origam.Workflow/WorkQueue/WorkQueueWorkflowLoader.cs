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
using System.Xml;
using System.Xml.XPath;
using System.Data;
using Origam.Extensions;
using Origam.Schema;
using Origam.Workbench.Services;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using core=Origam.Workbench.Services.CoreServices;

namespace Origam.Workflow.WorkQueue
{
	/// <summary>
	/// Connection string:
	/// name - name of the workflowLoader defined at the WorkQueueClass. The worklow called must implement
	///		   IWorkQueueLoder workflow defined in the OrigamRoot model.
	/// anything else - input workflow parameters - constant text values as defined in the connection string
	/// </summary>
	public class WorkQueueWorkflowLoader : WorkQueueLoaderAdapter
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		Hashtable _inputParameters = new Hashtable();
		IWorkflow _workflow;
		bool _executed = false;
		string _resultState;
	    IDataDocument _attachmentSource;
		WorkQueueClass _wqc;
		XPathNodeIterator _resultIterator;

		public WorkQueueWorkflowLoader()
		{
		}

		public override void Connect(IWorkQueueService service, Guid queueId, string workQueueClass, string connection, string userName, string password, string transactionId)
		{
			if(log.IsInfoEnabled)
			{
				log.Info("Connecting " + connection);
			}

			_inputParameters.Add("userName", userName);
			_inputParameters.Add("password", password);

			string name = null;
			string[] cnParts = connection.Split(";".ToCharArray());

			foreach (string part in cnParts)
			{
				string[] pair = part.Split("=".ToCharArray());
				if (pair.Length == 2)
				{
					switch (pair[0])
					{
						case "name":
							name = pair[1];
							break;
						default:
							_inputParameters[pair[0]] = pair[1];
							break;
					}
				}
			}

			_wqc = service.WQClass(queueId) as WorkQueueClass;
			WorkqueueLoader loader = _wqc.GetLoader(name);
			_workflow = loader.Workflow;
		}

		public override void Disconnect()
		{
			_inputParameters.Clear();
			_workflow = null;
		}

		public override WorkQueueAdapterResult GetItem(string lastState)
		{
			if(!_executed)
			{
				if(LoadData(lastState))
				{
					_executed = true;
				}
				else
				{
					// no data loaded, we exit
					return null;
				}
			}
			DataTable attachmentTable = _attachmentSource.DataSet.Tables["Attachment"];
			if(_resultIterator.Count > 1 && attachmentTable.Rows.Count > 0)
			{
				throw new Exception("Workflow returned multiple results and attachments. Returning attachments is not supported when returning multiple results by a workflow to the work queue.");
			}

			if (_resultIterator.MoveNext())
			{
				if(_resultIterator.CurrentPosition > _resultIterator.Count) return null;
			    IXmlContainer document = DataDocumentFactory.New(XmlTools.GetXmlSlice(_resultIterator));
				WorkQueueAdapterResult result = new WorkQueueAdapterResult(document);
				result.State = _resultState;
				result.Attachments = new WorkQueueAttachment[attachmentTable.Rows.Count];

				if(log.IsDebugEnabled)
				{
					log.Debug("Workflow loader returned " + attachmentTable.Rows.Count.ToString() + " attachments.");
					log.Debug(document.Xml.OuterXml);
				}
				for(int i=0; i<attachmentTable.Rows.Count; i++)
				{
					result.Attachments[i] = new WorkQueueAttachment();
					result.Attachments[i].Name = (string)attachmentTable.Rows[i]["FileName"];
					result.Attachments[i].Data = (byte[])attachmentTable.Rows[i]["Data"];
				}
			
				return result;
			}
			else
			{
				return null;
			}
		}

		private bool LoadData(string lastState)
		{
			_inputParameters["lastState"] = lastState;
			WorkflowHost host = WorkflowHost.DefaultHost;
			WorkflowEngine workflowEngine = WorkflowEngine.PrepareWorkflow(_workflow, _inputParameters, false, _workflow.Name);
			
			if(log.IsDebugEnabled)
			{
				log.Debug("Starting workflow " + _workflow.Name);
			}
			// execute workflow
			host.ExecuteWorkflow(workflowEngine);
			if(log.IsDebugEnabled)
			{
				log.Debug("Finishing workflow " + _workflow.Name);
			}
			// handle exception
			if(workflowEngine.WorkflowException != null)
			{
				if(log.IsErrorEnabled)
				{
					log.LogOrigamError(workflowEngine.WorkflowException.Message, workflowEngine.WorkflowException);
				}
				throw workflowEngine.WorkflowException;
			}
			XmlContainer resultData = workflowEngine.ReturnValue as XmlContainer;
			_resultState = (string)workflowEngine.RuleEngine.GetContext(new ModelElementKey(new Guid("f405cef2-2fad-4d58-a71c-10df3831e966")));
			_attachmentSource = (IDataDocument)workflowEngine.RuleEngine.GetContext((new ModelElementKey(new Guid("b0caa6ec-8a54-4524-8387-8504e34d206c"))));
			if(log.IsDebugEnabled)
			{
				log.Debug("Workflow loader result:");
				log.Debug(resultData?.Xml.OuterXml);
			}
			if(resultData == null)
			{
				if(log.IsDebugEnabled)
				{
					log.Debug("Workflow loader result was null.");
				}
				throw new Exception("Result of work queue loader must be an XMLContainer.");
			}
			else if(resultData.Xml.DocumentElement == null)
			{
				if(log.IsDebugEnabled)
				{
					log.Debug("Workflow loader result was an empty XML Document.");
				}
				return false;
			}
			string xpath = "/";
			if(_wqc.Entity == null)
			{
				XmlNode firstChild = resultData.Xml.FirstChild;
				XmlNode secondChild = firstChild.FirstChild;
				xpath += firstChild.Name;
				if(secondChild != null)
				{
					xpath += "/" + secondChild.Name;
				}
				else
				{
					// one empty node from the loader workflow,
					// e.g. <ROOT/> - nothing came
					return false;
				}
			}
			else
			{
				xpath = "/ROOT/" + _wqc.Entity.Name;
			}
			XPathNavigator nav = resultData.Xml.CreateNavigator();
			_resultIterator = nav.Select(xpath);
			return true;
		}
	}
}
