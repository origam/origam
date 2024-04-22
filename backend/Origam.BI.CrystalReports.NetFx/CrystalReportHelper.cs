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
using System.Data;

using Origam.Schema.GuiModel;
using Origam.DA;
using Origam.Workbench.Services;

using CrystalDecisions.CrystalReports.Engine;
using log4net.Core;
using Origam.Extensions;

namespace Origam.BI.CrystalReports;

/// <summary>
/// Summary description for CrystalReportHelper.
/// </summary>
public class CrystalReportHelper
{
	private IServiceAgent _dataServiceAgent;
	private static readonly log4net.ILog log
		= log4net.LogManager.GetLogger(
			System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

	public CrystalReportHelper()
	{
			IBusinessServicesService services = ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService;
			_dataServiceAgent = services.GetAgent("DataService", null, null);
		}
		
	#region Public Functions
	public ReportDocument CreateReport(Guid reportId, Hashtable parameters, string transactionId)
	{
			if(parameters == null) parameters = new Hashtable();
			// get report model element
			var report = ReportHelper.GetReportElement<CrystalReport>(reportId);
			ReportHelper.PopulateDefaultValues(report, parameters);
            ReportHelper.ComputeXsltValueParameters(report, parameters);
            // load data
            DataSet data = null;
			if(report.DataStructure != null)
			{
				data = LoadData(report.DataStructureId, report.DataStructureMethodId, report.DataStructureSortSetId, parameters, transactionId);
			}
			TraceReportData(data, report.Name);
			// get report
			return CreateReport(report.ReportFileName, data, parameters, report);
		}

	private void TraceReportData(DataSet data, string reportName)
	{
			try
			{
				OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
				if(data != null)
				{
					data.WriteXml(
						System.IO.Path.Combine(settings.ReportsFolder(), reportName +  ".xml"),
						XmlWriteMode.WriteSchema
						);
				}
			}
			catch (Exception e)
			{
				ReportHelper.LogError(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
					string.Format("Can't write Crystal Report debug info: {0}", e.Message));
			}
		}

	public ReportDocument CreateReport(Guid reportId, DataSet data, Hashtable parameters)
	{
			if(parameters == null) parameters = new Hashtable();
			// get report model element
			var report = ReportHelper.GetReportElement<CrystalReport>(reportId);
			TraceReportData(data, report.Name);
			ReportHelper.PopulateDefaultValues(report, parameters);
            ReportHelper.ComputeXsltValueParameters(report, parameters);
            // get report
            return CreateReport(report.ReportFileName, data, parameters, report);
		}

	public ReportDocument CreateReport(string fileName, DataSet data)
	{
			return this.CreateReport(fileName, data, new Hashtable(), null);
		}
	#endregion
		
	private ReportDocument CreateReport(string fileName, DataSet data, Hashtable parameters, CrystalReport reportElement)
	{
            if (log.IsInfoEnabled)
            {
                WriteInfoLog(reportElement, "Generating report started");
            }
            if (parameters == null) throw new NullReferenceException(ResourceUtils.GetString("CreateReport: Parameters cannot be null."));
			ReportDocument result = null;
            string path = fileName;
            try
			{
				result = new AsReportDocument();
			}
			catch (Exception ex)
            {
                log.LogOrigamError("Error occured while initializing Crystal Report " + path, ex);
            }
			try
			{
				OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
				path = System.IO.Path.Combine(settings.ReportsFolder(), fileName);
				result.Load(path, CrystalDecisions.Shared.OpenReportMethod.OpenReportByTempCopy);
			}
			catch (Exception ex)
			{
                log.Fatal("Failed loading Crystal Report " + path, ex);
				throw new Exception(ResourceUtils.GetString("FailedToLoadReport", path + Environment.NewLine + ex.Message + Environment.NewLine), ex);
			}
			
			try
			{
				if(data == null)
				{
					OrigamSettings settings = ConfigurationManager.GetActiveConfiguration() ;
					Hashtable cn = OrigamSettings.ParseConnectionString(settings.ReportConnectionString);
					SetLogonInfo(result, cn);
					foreach(ReportDocument subreport in result.Subreports)
					{
						SetLogonInfo(subreport, cn);
					}
				}
				else
				{
					// we set the data source to the main report
					result.SetDataSource(data);
					// we set the same data source to all subreports
					foreach(ReportDocument subreport in result.Subreports)
					{
						if(subreport.DataSourceConnections.Count > 0)
						{
							subreport.SetDataSource(data);
						}
						//subreport.Refresh();
					}
				}

				// set report's parameters
				SetReportParameters(parameters, result, reportElement);
				result.Refresh();
				// once again
				SetReportParameters(parameters, result, reportElement);
			}
			catch(Exception ex)
			{
				throw new Exception(ResourceUtils.GetString("CouldNotActivate", Environment.NewLine, ex.Message), ex);
			}
            if (log.IsInfoEnabled)
            {
                WriteInfoLog(reportElement, "Generating report finished");
            }
            return result;
		}

	private void WriteInfoLog(CrystalReport reportElement, string message)
	{
            LoggingEvent loggingEvent = new LoggingEvent(
              this.GetType(),
              log.Logger.Repository,
              log.Logger.Name,
              Level.Info,
              string.Format("{0}: {1}", message, reportElement.Name),
              null);
            loggingEvent.Properties["Caption"] = reportElement.Caption;
            log.Logger.Log(loggingEvent);
        }

	private void SetLogonInfo(ReportDocument report, Hashtable connection)
	{
			foreach(CrystalDecisions.CrystalReports.Engine.Table table in report.Database.Tables)
			{
				CrystalDecisions.Shared.TableLogOnInfo logon = table.LogOnInfo;
				if(connection["DatabaseName"] != null) logon.ConnectionInfo.DatabaseName = Convert.ToString(connection["DatabaseName"]);
				if(connection["IntegratedSecurity"] != null) logon.ConnectionInfo.IntegratedSecurity = Convert.ToBoolean(connection["IntegratedSecurity"]);
				if(connection["ServerName"] != null) logon.ConnectionInfo.ServerName = Convert.ToString(connection["ServerName"]);
				if(connection["UserID"] != null) logon.ConnectionInfo.UserID = Convert.ToString(connection["UserID"]);
				if(connection["Password"] != null) logon.ConnectionInfo.Password = Convert.ToString(connection["Password"]);
				table.ApplyLogOnInfo(logon);
			}
		}

	private void SetReportParameters(Hashtable parameters, ReportDocument report, CrystalReport reportElement)
	{
			if(parameters != null)
			{
				foreach(CrystalDecisions.Shared.ParameterField paramDef in report.ParameterFields)
				{
					foreach(DictionaryEntry entry in parameters)
					{
						if(paramDef.Name == (string)entry.Key)
						{
							object val = entry.Value;
							if(val is Guid) val = entry.Value.ToString();
							report.SetParameterValue(paramDef.Name, val);
						}
					}
				}
			}
		}

	private DataSet LoadData(Guid dataStructureId, Guid methodId, Guid sortSetId, Hashtable parameters, string transactionId)
	{
			DataStructureQuery query = new DataStructureQuery(dataStructureId, methodId, Guid.Empty, sortSetId);
			foreach(DictionaryEntry entry in parameters)
			{
				query.Parameters.Add(new QueryParameter((string)entry.Key, entry.Value));
			}
			return LoadData(query, transactionId);
		}

	private DataSet LoadData(DataStructureQuery query, string transactionId)
	{
			_dataServiceAgent.MethodName = "LoadDataByQuery";
			_dataServiceAgent.Parameters.Clear();
			_dataServiceAgent.Parameters.Add("Query", query);
			_dataServiceAgent.TransactionId = transactionId;
			_dataServiceAgent.Run();
			DataSet reportData = _dataServiceAgent.Result as DataSet;
			return reportData;
		}
}

public class AsReportDocument : ReportDocument
{
	protected override bool CheckLicenseStatus()
	{
			return true;
		}
}