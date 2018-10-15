using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;

using Origam.DA;
using core=Origam.Workbench.Services.CoreServices;
using System.Xml;
using System.Text;
using System.IO;

namespace OrigamEngineWebAPI
{
	/// <summary>
	/// Summary description for WorkflowService.
	/// </summary>
	[WebService(Namespace="http://origamenginewebapi.advantages.cz/", Description="Using this service you can execute workflows defined in the loaded model.")]
	public class WorkflowService : System.Web.Services.WebService
	{
		public WorkflowService()
		{
			//CODEGEN: This call is required by the ASP.NET Web Services Designer
			InitializeComponent();
		}

		#region Component Designer generated code
		
		//Required by the Web Services Designer 
		private IContainer components = null;
				
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if(disposing && components != null)
			{
				components.Dispose();
			}
			base.Dispose(disposing);		
		}
		
		#endregion

        private static object ConvertData(object result)
        {
            XmlDataDocument document = result as XmlDataDocument;
            if (document != null)
            {
                StringBuilder sb = new StringBuilder();
                XmlTextWriter writer = new XmlTextWriter(new StringWriter(sb));
                document.DataSet.WriteXml(writer, XmlWriteMode.DiffGram);
                return sb.ToString();
            }
            return result;
        }

        [WebMethod(MessageName = "ExecuteWorkflow0", Description = "Executes workflow without passing any parameters.")]
        public object ExecuteWorkflow(string workflowId)
        {
            Guid guid = new Guid(workflowId);
            return ConvertData(core.WorkflowService.ExecuteWorkflow(guid));
        }

        [WebMethod(Description = "Executes workflow passing an array of parameters.")]
        public object ExecuteWorkflow(string workflowId, QueryParameterCollection parameters)
        {
            Guid guid = new Guid(workflowId);
            return ConvertData(core.WorkflowService.ExecuteWorkflow(guid, parameters, null));
        }

        [WebMethod(MessageName = "ExecuteWorkflow1", Description = "Executes workflow passing 1 parameter.")]
        public object ExecuteWorkflow(string workflowId, string paramName, string paramValue)
        {
            Guid guid = new Guid(workflowId);
            QueryParameterCollection parameters = new QueryParameterCollection();
            parameters.Add(new QueryParameter(paramName, paramValue));
            return ConvertData(core.WorkflowService.ExecuteWorkflow(guid, parameters, null));
        }
    }
}
