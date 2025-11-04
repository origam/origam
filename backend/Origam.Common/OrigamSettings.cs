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
using System.ComponentModel;
using System.Xml.Serialization;

namespace Origam;

/// <summary>
/// Holds settings for ORIGAM Architect
/// </summary>
public class OrigamSettings : ICloneable
{
    [XmlIgnore]
    private string BaseFolder { get; } = AppContext.BaseDirectory;

    public OrigamSettings() { }

    public OrigamSettings(string name)
    {
        Name = name;
    }

    public OrigamSettings(
        string name,
        string schemaConnectionString,
        string dataConnectionString,
        string schemaDataService,
        string dataDataService,
        string securityDomain,
        Guid defaultSchemaExtensionId,
        Guid extraSchemaExtensionId,
        string titleText,
        int dataServiceSelectTimeout,
        int dataServiceExecuteProcedureTimeout,
        bool useProgressiveCaching,
        bool checkAttachmentsOnRecordSelection,
        int workQueueListRefreshPeriod,
        bool loadExternalWorkQueues,
        int externalWorkQueueCheckPeriod,
        WorkQueueProcessingMode workQueueProcessingMode,
        int roundRobinBatchSize,
        string slogan,
        string localizationFolder,
        string localizationIncludedDocumentationElements,
        bool executeUpgradeScriptsOnStart,
        int exportRecordsLimit,
        string helpUrl,
        bool disableAttachments,
        int maxOpenTabs,
        bool activateReadOnlyRoles,
        string schedulerFilter,
        string serverLogUrl,
        bool traceEnabled,
        string authorizationProvider,
        string profileProvider,
        string pathToRuntimeModelConfig,
        string pathToSmsHandler
    )
    {
        this.Name = name;
        this.SchemaConnectionString = schemaConnectionString;
        this.DataConnectionString = dataConnectionString;
        this.SchemaDataService = schemaDataService;
        this.DataDataService = dataDataService;
        this.SecurityDomain = securityDomain;
        this.DefaultSchemaExtensionId = defaultSchemaExtensionId;
        this.TitleText = titleText;
        this.DataServiceSelectTimeout = dataServiceSelectTimeout;
        this.DataServiceExecuteProcedureTimeout = dataServiceExecuteProcedureTimeout;
        this.UseProgressiveCaching = useProgressiveCaching;
        this.CheckAttachmentsOnRecordSelection = checkAttachmentsOnRecordSelection;
        this.WorkQueueListRefreshPeriod = workQueueListRefreshPeriod;
        this.LoadExternalWorkQueues = loadExternalWorkQueues;
        this.ExternalWorkQueueCheckPeriod = externalWorkQueueCheckPeriod;
        this.RoundRobinBatchSize = roundRobinBatchSize;
        this.WorkQueueProcessingMode = workQueueProcessingMode;
        this.Slogan = slogan;
        this.LocalizationFolder = localizationFolder;
        this.LocalizationIncludedDocumentationElements = localizationIncludedDocumentationElements;
        this.ExecuteUpgradeScriptsOnStart = executeUpgradeScriptsOnStart;
        this.ExportRecordsLimit = exportRecordsLimit;
        this.HelpUrl = helpUrl;
        this.DisableAttachments = disableAttachments;
        this.MaxOpenTabs = maxOpenTabs;
        this.ActivateReadOnlyRoles = activateReadOnlyRoles;
        this.SchedulerFilter = schedulerFilter;
        this.ServerLogUrl = serverLogUrl;
        this.TraceEnabled = traceEnabled;
        this.AuthorizationProvider = authorizationProvider;
        this.ProfileProvider = profileProvider;
        this.PathToRuntimeModelConfig = pathToRuntimeModelConfig;
        this.SmsService = pathToSmsHandler;
    }

    public override string ToString()
    {
        return this.Name;
    }

    [Category("Server Connection")]
    public string ServerLogUrl { get; set; }

    [Category("Server Connection")]
    public string ServerUrl { get; set; }

    [Category("Model Connection")]
    public string SchemaConnectionString { get; set; } =
        "Server=?;database=?;Integrated Security=SSPI;";

    [Category("Model Connection")]
    public string ModelSourceControlLocation { get; set; } = "";

    [Category("Data Connection")]
    public string DataConnectionString { get; set; } =
        "Server=?;database=?;Integrated Security=SSPI;";

    [Category("Model Connection")]
    [Browsable(false)]
    public string SchemaDataService { get; set; } =
        "Origam.DA.Service.MsSqlDataService, Origam.DA.Service";

    [Category("Data Connection")]
    public string DataDataService { get; set; } =
        "Origam.DA.Service.MsSqlDataService, Origam.DA.Service";

    [Category("Security")]
    public string SecurityDomain { get; set; } = "";

    [Category("Reports")]
    [DefaultValue("Reports")]
    public string ReportDefinitionsPath { get; set; } = "Reports";

    [Category("Reports")]
    public string ReportConnectionString { get; set; } = "";

    [Category("Reports")]
    public string PrintItServiceUrl { get; set; } = "";

    [Category("Reports")]
    public string SQLReportServiceUrl { get; set; } = "";

    [Category("Reports")]
    public string SQLReportServiceAccount { get; set; } = "";

    [Category("Reports")]
    public string SQLReportServicePassword { get; set; } = "";

    [Category("Reports")]
    public int SQLReportServiceTimeout { get; set; } = 60000;

    [Category("Reports")]
    [Description(
        "Format of exported Excel files from GUI. Accepted values are XLS and XLSX. Default value is XLS."
    )]
    public string GUIExcelExportFormat { get; set; } = "XLS";

    [Category("Client Configuration")]
    public Guid DefaultSchemaExtensionId { get; set; } = Guid.Empty;

    [Category("Client Configuration")]
    public Guid ExtraSchemaExtensionId { get; set; } = Guid.Empty;

    [Category("Client Configuration")]
    public string TitleText { get; set; } = "ORIGAM APPLICATION";

    [Category("Client Configuration")]
    public string Slogan { get; set; } = "";

    [Category("(Configuration)")]
    public string Name { get; set; } = "New Configuration";
    private string _localization = "";

    [Category("Localization")]
    public string LocalizationFolder
    {
        get { return _localization; }
        set { _localization = value?.Trim(); }
    }

    [Category("Localization")]
    [Description(
        "Comma separated names of documentation categories to be include in the generated localization files e.g. USER_SHORT_HELP,USER_LONG_HELP"
    )]
    public string LocalizationIncludedDocumentationElements { get; set; } = "";

    [Category("Localization")]
    [Description(
        "List of languages that will be used when generating translation files in Architect. Comma separated e.g. en-US,de-DE."
    )]
    public string TranslationBuilderLanguages { get; set; } = "";

    [Category("(Configuration)")]
    public string HelpUrl { get; set; } = "http://origam.com/doc";

    [Category("Data Connection"), DefaultValue(60)]
    public int DataServiceSelectTimeout { get; set; } = 60;

    [Category("Data Connection"), DefaultValue(0)]
    public int DataUpdateBatchSize { get; set; } = 0;

    [Category("Model Connection"), DefaultValue(5000)]
    public int ModelUpdateBatchSize { get; set; } = 5000;

    [Category("Data Connection"), DefaultValue(0)]
    public int DataBulkInsertThreshold { get; set; } = 0;

    [Category("Model Connection"), DefaultValue(100)]
    public int ModelBulkInsertThreshold { get; set; } = 100;

    [Category("Model Connection")]
    public string AuthorizationProvider { get; set; } =
        "Origam.Security.OrigamDatabaseAuthorizationProvider, Origam.Security";

    [Category("Model Connection")]
    public string ProfileProvider { get; set; } =
        "Origam.Security.OrigamProfileProvider, Origam.Security";

    [Category("Model Connection")]
    public string PathToRuntimeModelConfig { get; set; }

    [Category("Data Connection"), DefaultValue(2000)]
    public int DataServiceExecuteProcedureTimeout { get; set; } = 2000;

    [Category("Data Connection"), DefaultValue(false)]
    public bool UseProgressiveCaching { get; set; } = false;

    [Category("User Interface"), DefaultValue(true)]
    public bool CheckAttachmentsOnRecordSelection { get; set; } = true;

    [Category("User Interface"), DefaultValue(false)]
    public bool DisableAttachments { get; set; } = false;

    [Category("User Interface"), DefaultValue(true)]
    public bool ShowEditorMenusInAppToolStrip { get; set; } = true;

    [Category("User Interface"), DefaultValue(0)]
    public int MaxOpenTabs { get; set; } = 0;

    [Category("User Interface"), DefaultValue(false)]
    public bool ActivateReadOnlyRoles { get; set; } = false;

    [Category("Work Queue"), DefaultValue(60)]
    public int WorkQueueListRefreshPeriod { get; set; } = 60;

    [Category("Work Queue"), DefaultValue(false)]
    public bool LoadExternalWorkQueues { get; set; } = false;

    [Category("Work Queue"), DefaultValue(60)]
    public int ExternalWorkQueueCheckPeriod { get; set; } = 60;

    [Category("Work Queue"), DefaultValue(false)]
    public bool AutoProcessWorkQueues { get; set; } = false;

    [Category("Work Queue"), DefaultValue(WorkQueueProcessingMode.Linear)]
    public WorkQueueProcessingMode WorkQueueProcessingMode { get; set; } =
        WorkQueueProcessingMode.Linear;

    [Category("Work Queue"), DefaultValue(3)]
    public int RoundRobinBatchSize { get; set; } = 3;

    [Category("Services"), DefaultValue(-1)]
    public int ExportRecordsLimit { get; set; } = -1;

    [Category("Services"), DefaultValue(false)]
    public bool ExecuteUpgradeScriptsOnStart { get; set; } = false;

    [Category("Services"), DefaultValue(true)]
    public bool TraceEnabled { get; set; } = true;

    [Category("Scheduler")]
    [Description(
        "When set it defines on which folders current scheduler instance will process schedules."
    )]
    public string SchedulerFilter { get; set; }

    [Category("Model Connection")]
    public string ModelProvider { get; set; } =
        "Origam.OrigamEngine.FilePersistenceBuilder, Origam.OrigamEngine";

    [Category("Services"), DefaultValue("")]
    public string GsPath { get; set; }
    public Platform[] DeployPlatforms { get; set; }

    [Category("Model Connection")]
    public bool CheckFileHashesAfterModelLoad { get; set; } = true;

    [Category("SMS Service")]
    public string SmsService { get; set; }

    public string ReportsFolder()
    {
        return System.IO.Path.Combine(BaseFolder, this.ReportDefinitionsPath);
    }

    public static Hashtable ParseConnectionString(string connectionString)
    {
        Hashtable result = new Hashtable();
        string[] elements = connectionString.Split(";".ToCharArray());
        foreach (string element in elements)
        {
            if (element.IndexOf("=") > 0)
            {
                string[] pair = element.Split("=".ToCharArray());
                result.Add(pair[0], pair[1]);
            }
        }
        return result;
    }

    #region ICloneable Members
    public object Clone()
    {
        return new OrigamSettings(
            this.Name,
            this.SchemaConnectionString,
            this.DataConnectionString,
            this.SchemaDataService,
            this.DataDataService,
            this.SecurityDomain,
            this.DefaultSchemaExtensionId,
            this.ExtraSchemaExtensionId,
            this.TitleText,
            this.DataServiceSelectTimeout,
            this.DataServiceExecuteProcedureTimeout,
            this.UseProgressiveCaching,
            this.CheckAttachmentsOnRecordSelection,
            this.WorkQueueListRefreshPeriod,
            this.LoadExternalWorkQueues,
            this.ExternalWorkQueueCheckPeriod,
            this.WorkQueueProcessingMode,
            this.RoundRobinBatchSize,
            this.Slogan,
            this.LocalizationFolder,
            this.LocalizationIncludedDocumentationElements,
            this.ExecuteUpgradeScriptsOnStart,
            this.ExportRecordsLimit,
            this.HelpUrl,
            this.DisableAttachments,
            this.MaxOpenTabs,
            this.ActivateReadOnlyRoles,
            this.SchedulerFilter,
            this.ServerLogUrl,
            this.TraceEnabled,
            this.AuthorizationProvider,
            this.ProfileProvider,
            this.PathToRuntimeModelConfig,
            this.SmsService
        );
    }

    public Platform[] GetAllPlatform()
    {
        Platform[] platforms = DeployPlatforms ?? new Platform[0];
        Platform platform = new Platform { IsPrimary = true, DataService = DataDataService };
        platform.Name = platform.GetParseEnum(DataDataService);
        Array.Resize(ref platforms, platforms.Length + 1);
        platforms[platforms.Length - 1] = platform;
        return platforms;
    }
    #endregion
}

public class Platform
{
    private string nameField;
    private string dataConnectionStringField;
    private string dataServiceField;

    /// <remarks/>
    public string Name
    {
        get { return this.nameField; }
        set { this.nameField = value; }
    }
    public string DataConnectionString
    {
        get { return this.dataConnectionStringField; }
        set { this.dataConnectionStringField = value; }
    }
    public string DataService
    {
        get { return this.dataServiceField; }
        set { this.dataServiceField = value; }
    }
    public bool IsPrimary { get; set; } = false;

    public override string ToString()
    {
        string _name = Name;
        if (IsPrimary)
        {
            _name += " - Primary";
        }

        return _name;
    }

    public string GetParseEnum(string dataDataService)
    {
        return dataDataService
            .Split(",".ToCharArray())[0]
            .Trim()
            .Split("\\.".ToCharArray())[3]
            .Trim()
            .Replace("DataService", "");
    }
}

public enum WorkQueueProcessingMode
{
    Linear,
    RoundRobin,
}
