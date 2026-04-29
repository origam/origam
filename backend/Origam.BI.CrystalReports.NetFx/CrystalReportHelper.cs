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
using CrystalDecisions.CrystalReports.Engine;
using log4net.Core;
using Origam.DA;
using Origam.Extensions;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.BI.CrystalReports;

/// <summary>
/// Summary description for CrystalReportHelper.
/// </summary>
public class CrystalReportHelper
{
    private IServiceAgent _dataServiceAgent;
    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );

    public CrystalReportHelper()
    {
        IBusinessServicesService services =
            ServiceManager.Services.GetService(serviceType: typeof(IBusinessServicesService))
            as IBusinessServicesService;
        _dataServiceAgent = services.GetAgent(
            serviceType: "DataService",
            ruleEngine: null,
            workflowEngine: null
        );
    }

    #region Public Functions
    public ReportDocument CreateReport(Guid reportId, Hashtable parameters, string transactionId)
    {
        if (parameters == null)
        {
            parameters = new Hashtable();
        }
        // get report model element
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId: reportId);
        ReportHelper.PopulateDefaultValues(report: report, parameters: parameters);
        ReportHelper.ComputeXsltValueParameters(report: report, parameters: parameters);
        // load data
        DataSet data = null;
        if (report.DataStructure != null)
        {
            data = LoadData(
                dataStructureId: report.DataStructureId,
                methodId: report.DataStructureMethodId,
                sortSetId: report.DataStructureSortSetId,
                parameters: parameters,
                transactionId: transactionId
            );
        }
        TraceReportData(data: data, reportName: report.Name);
        // get report
        return CreateReport(
            fileName: report.ReportFileName,
            data: data,
            parameters: parameters,
            reportElement: report
        );
    }

    private void TraceReportData(DataSet data, string reportName)
    {
        try
        {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            if (data != null)
            {
                data.WriteXml(
                    fileName: System.IO.Path.Combine(
                        path1: settings.ReportsFolder(),
                        path2: reportName + ".xml"
                    ),
                    mode: XmlWriteMode.WriteSchema
                );
            }
        }
        catch (Exception e)
        {
            ReportHelper.LogError(
                type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                message: string.Format(
                    format: "Can't write Crystal Report debug info: {0}",
                    arg0: e.Message
                )
            );
        }
    }

    public ReportDocument CreateReport(Guid reportId, DataSet data, Hashtable parameters)
    {
        if (parameters == null)
        {
            parameters = new Hashtable();
        }
        // get report model element
        var report = ReportHelper.GetReportElement<CrystalReport>(reportId: reportId);
        TraceReportData(data: data, reportName: report.Name);
        ReportHelper.PopulateDefaultValues(report: report, parameters: parameters);
        ReportHelper.ComputeXsltValueParameters(report: report, parameters: parameters);
        // get report
        string reportFileName = ReportHelper.ExpandCurlyBracketPlaceholdersWithParameters(
            input: report.ReportFileName,
            parameters: parameters
        );
        return CreateReport(
            fileName: reportFileName,
            data: data,
            parameters: parameters,
            reportElement: report
        );
    }

    public ReportDocument CreateReport(string fileName, DataSet data)
    {
        return this.CreateReport(
            fileName: fileName,
            data: data,
            parameters: new Hashtable(),
            reportElement: null
        );
    }
    #endregion

    private ReportDocument CreateReport(
        string fileName,
        DataSet data,
        Hashtable parameters,
        CrystalReport reportElement
    )
    {
        if (log.IsInfoEnabled)
        {
            WriteInfoLog(reportElement: reportElement, message: "Generating report started");
        }
        if (parameters == null)
        {
            throw new NullReferenceException(
                message: ResourceUtils.GetString(key: "CreateReport: Parameters cannot be null.")
            );
        }

        ReportDocument result = null;
        string path = fileName;
        try
        {
            result = new AsReportDocument();
        }
        catch (Exception ex)
        {
            log.LogOrigamError(
                message: "Error occured while initializing Crystal Report " + path,
                ex: ex
            );
        }
        try
        {
            OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
            path = System.IO.Path.Combine(path1: settings.ReportsFolder(), path2: fileName);
            if (!IOTools.IsSubPathOf(path: path, basePath: settings.ReportsFolder()))
            {
                throw new Exception(message: Strings.PathNotOnReportPath);
            }
            result.Load(
                filename: path,
                openMethod: CrystalDecisions.Shared.OpenReportMethod.OpenReportByTempCopy
            );
        }
        catch (Exception ex)
        {
            log.Fatal(message: "Failed loading Crystal Report " + path, exception: ex);
            throw new Exception(
                message: ResourceUtils.GetString(
                    key: "FailedToLoadReport",
                    args: path + Environment.NewLine + ex.Message + Environment.NewLine
                ),
                innerException: ex
            );
        }

        try
        {
            if (data == null)
            {
                OrigamSettings settings = ConfigurationManager.GetActiveConfiguration();
                Hashtable cn = OrigamSettings.ParseConnectionString(
                    connectionString: settings.ReportConnectionString
                );
                SetLogonInfo(report: result, connection: cn);
                foreach (ReportDocument subreport in result.Subreports)
                {
                    SetLogonInfo(report: subreport, connection: cn);
                }
            }
            else
            {
                // we set the data source to the main report
                result.SetDataSource(dataSet: data);
                // we set the same data source to all subreports
                foreach (ReportDocument subreport in result.Subreports)
                {
                    if (subreport.DataSourceConnections.Count > 0)
                    {
                        subreport.SetDataSource(dataSet: data);
                    }
                    //subreport.Refresh();
                }
            }
            // set report's parameters
            SetReportParameters(
                parameters: parameters,
                report: result,
                reportElement: reportElement
            );
            result.Refresh();
            // once again
            SetReportParameters(
                parameters: parameters,
                report: result,
                reportElement: reportElement
            );
        }
        catch (Exception ex)
        {
            throw new Exception(
                message: ResourceUtils.GetString(
                    key: "CouldNotActivate",
                    args: new object[] { Environment.NewLine, ex.Message }
                ),
                innerException: ex
            );
        }
        if (log.IsInfoEnabled)
        {
            WriteInfoLog(reportElement: reportElement, message: "Generating report finished");
        }
        return result;
    }

    private void WriteInfoLog(CrystalReport reportElement, string message)
    {
        LoggingEvent loggingEvent = new LoggingEvent(
            callerStackBoundaryDeclaringType: this.GetType(),
            repository: log.Logger.Repository,
            loggerName: log.Logger.Name,
            level: Level.Info,
            message: string.Format(format: "{0}: {1}", arg0: message, arg1: reportElement.Name),
            exception: null
        );
        loggingEvent.Properties[key: "Caption"] = reportElement.Caption;
        log.Logger.Log(logEvent: loggingEvent);
    }

    private void SetLogonInfo(ReportDocument report, Hashtable connection)
    {
        foreach (CrystalDecisions.CrystalReports.Engine.Table table in report.Database.Tables)
        {
            CrystalDecisions.Shared.TableLogOnInfo logon = table.LogOnInfo;
            if (connection[key: "DatabaseName"] != null)
            {
                logon.ConnectionInfo.DatabaseName = Convert.ToString(
                    value: connection[key: "DatabaseName"]
                );
            }

            if (connection[key: "IntegratedSecurity"] != null)
            {
                logon.ConnectionInfo.IntegratedSecurity = Convert.ToBoolean(
                    value: connection[key: "IntegratedSecurity"]
                );
            }

            if (connection[key: "ServerName"] != null)
            {
                logon.ConnectionInfo.ServerName = Convert.ToString(
                    value: connection[key: "ServerName"]
                );
            }

            if (connection[key: "UserID"] != null)
            {
                logon.ConnectionInfo.UserID = Convert.ToString(value: connection[key: "UserID"]);
            }

            if (connection[key: "Password"] != null)
            {
                logon.ConnectionInfo.Password = Convert.ToString(
                    value: connection[key: "Password"]
                );
            }

            table.ApplyLogOnInfo(logonInfo: logon);
        }
    }

    private void SetReportParameters(
        Hashtable parameters,
        ReportDocument report,
        CrystalReport reportElement
    )
    {
        if (parameters != null)
        {
            foreach (CrystalDecisions.Shared.ParameterField paramDef in report.ParameterFields)
            {
                foreach (DictionaryEntry entry in parameters)
                {
                    if (paramDef.Name == (string)entry.Key)
                    {
                        object val = entry.Value;
                        if (val is Guid)
                        {
                            val = entry.Value.ToString();
                        }

                        report.SetParameterValue(name: paramDef.Name, val: val);
                    }
                }
            }
        }
    }

    private DataSet LoadData(
        Guid dataStructureId,
        Guid methodId,
        Guid sortSetId,
        Hashtable parameters,
        string transactionId
    )
    {
        DataStructureQuery query = new DataStructureQuery(
            dataStructureId: dataStructureId,
            methodId: methodId,
            defaultSetId: Guid.Empty,
            sortSetId: sortSetId
        );
        foreach (DictionaryEntry entry in parameters)
        {
            query.Parameters.Add(
                value: new QueryParameter(_parameterName: (string)entry.Key, value: entry.Value)
            );
        }
        return LoadData(query: query, transactionId: transactionId);
    }

    private DataSet LoadData(DataStructureQuery query, string transactionId)
    {
        _dataServiceAgent.MethodName = "LoadDataByQuery";
        _dataServiceAgent.Parameters.Clear();
        _dataServiceAgent.Parameters.Add(key: "Query", value: query);
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
