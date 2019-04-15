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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Web;
using System.Web.Services;

using Origam.DA;
using core = Origam.Workbench.Services.CoreServices;
using System.Xml;
using System.Text;
using System.IO;
using Origam;

namespace OrigamEngineWebAPI
{
    /// <summary>
    /// Summary description for WorkflowService.
    /// </summary>
    [WebService(Namespace = "http://origamenginewebapi.advantages.cz/", Description = "Using this service you can execute workflows defined in the loaded model.")]
    public class WorkflowService : System.Web.Services.WebService
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        protected override void Dispose(bool disposing)
        {
            if (disposing && components != null)
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        private static object ConvertData(object result)
        {
            IDataDocument document = result as IDataDocument;
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
            if (log.IsInfoEnabled)
            {
                log.Info("ExecuteWorkflow0");
            }

            Guid guid = new Guid(workflowId);
            return ConvertData(core.WorkflowService.ExecuteWorkflow(guid));
        }

        [WebMethod(Description = "Executes workflow passing an array of parameters.")]
        public object ExecuteWorkflow(string workflowId, QueryParameterCollection parameters)
        {
            if (log.IsInfoEnabled)
            {
                log.Info("ExecuteWorkflow");
            }

            Guid guid = new Guid(workflowId);
            return ConvertData(core.WorkflowService.ExecuteWorkflow(guid, parameters, null));
        }

        [WebMethod(MessageName = "ExecuteWorkflow1", Description = "Executes workflow passing 1 parameter.")]
        public object ExecuteWorkflow(string workflowId, string paramName, string paramValue)
        {
            if (log.IsInfoEnabled)
            {
                log.Info("ExecuteWorkflow1");
            }

            Guid guid = new Guid(workflowId);
            QueryParameterCollection parameters = new QueryParameterCollection();
            parameters.Add(new QueryParameter(paramName, paramValue));
            return ConvertData(core.WorkflowService.ExecuteWorkflow(guid, parameters, null));
        }
    }
}
