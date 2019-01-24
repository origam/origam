using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Origam.DA;
using Origam.Schema;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Workbench.Services;

namespace Origam.Server
{
    
    internal class ReportManager: IReportManager
    {
        private readonly SessionManager sessionManager;

        public ReportManager(SessionManager sessionManager)
        {
            this.sessionManager = sessionManager;
        }

        public string GetReport(string sessionFormIdentifier, string entity, 
            object id, string reportId, Hashtable parameterMappings)
        {
            string key = Guid.NewGuid().ToString();

            SessionStore ss = sessionManager.GetSession(new Guid(sessionFormIdentifier));
            DataRow row = ss.GetSessionRow(entity, id);

            Hashtable resultParams = DatasetTools.RetrieveParemeters(
                parameterMappings, new List<DataRow>{row});

            System.Web.HttpContext.Current.Application[key] = new ReportRequest(reportId, resultParams);

            return "ReportViewer.aspx?id=" + key;
        }

        public string GetReportStandalone(string reportId, Hashtable parameters,
            DataReportExportFormatType dataReportExportFormatType)
        {
            string key = Guid.NewGuid().ToString();

            System.Web.HttpContext.Current.Application[key] = new ReportRequest(reportId, parameters, dataReportExportFormatType);

            return "ReportViewer.aspx?id=" + key;
        }

        public string GetReportFromMenu(string menuId)
        {
            string key = Guid.NewGuid().ToString();

            IPersistenceService ps = ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
            ReportReferenceMenuItem rr = ps.SchemaProvider.RetrieveInstance(typeof(AbstractMenuItem), new ModelElementKey(new Guid(menuId))) as ReportReferenceMenuItem;

            System.Web.HttpContext.Current.Application[key] = new ReportRequest(rr.ReportId.ToString(), new Hashtable(),
                rr.ExportFormatType);

            return "ReportViewer.aspx?id=" + key;
        }
    }
}