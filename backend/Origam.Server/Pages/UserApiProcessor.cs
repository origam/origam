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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Origam.DA;
using Origam.DA.Service;
using Origam.Extensions;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using SixLabors.ImageSharp;

namespace Origam.Server.Pages;

public class UserApiProcessor
{
    private readonly IHttpTools httpTools;
    private readonly IWebHostEnvironment environment;

    private static readonly log4net.ILog log = log4net.LogManager.GetLogger(
        type: System.Reflection.MethodBase.GetCurrentMethod().DeclaringType
    );
    private Lazy<UrlApiPageCache> urlApiPageCache = new Lazy<UrlApiPageCache>(valueFactory: () =>
        new UrlApiPageCache(
            _pageProvider: (
                ServiceManager.Services.GetService<SchemaService>()
            ).GetProvider<PagesSchemaItemProvider>()
        )
    );

    public UserApiProcessor(IHttpTools httpTools, IWebHostEnvironment environment)
    {
        this.httpTools = httpTools;
        this.environment = environment;
    }

    #region IHttpModule Members
    public void Dispose() { }

    public void Process(IHttpContextWrapper context)
    {
        if (context.Request.AppRelativeCurrentExecutionFilePath.StartsWith(value: "~/assets/"))
        {
            return;
        }
        string mimeType = context.Request.ContentType?.Split(separator: ";".ToCharArray())[0];
        if (mimeType == "application/x-amf")
        {
            return;
        }
        string resultContentType = "text/plain";
        if (Analytics.Instance.IsAnalyticsEnabled)
        {
            RequestAnalytics(context: context, mimeType: mimeType);
        }
        try
        {
            var (code, page) = ResolvePage(
                context: context,
                outUrlParameters: out var urlParameters
            );
            if (code == 404)
            {
                Handle404(context: context);
                return;
            }
            if (code != 200)
            {
                context.Response.StatusCode = code;
                context.Response.ContentType = resultContentType;
                context.Response.Clear();
                if (code == 500)
                {
                    context.Response.Write(message: "Multiple routes.");
                }
                context.Response.End();
                return;
            }
            if (Analytics.Instance.IsAnalyticsEnabled)
            {
                PageAnalytics(page: page, urlParameters: urlParameters);
            }
            if (page.MimeType != "?")
            {
                if (page.MimeType == "text/calendar")
                {
                    context.Response.BufferOutput = false;
                }
                context.Response.ContentType = page.MimeType;
                // set result content type for exception handling
                // so the exception is formatted as expected, e.g. in json
                resultContentType = page.MimeType;
            }
            if (page.CacheMaxAge == null)
            {
                context.Response.CacheSetMaxAge(timeSpan: new TimeSpan(ticks: 0));
            }
            else
            {
                IParameterService parameterService =
                    ServiceManager.Services.GetService<IParameterService>();
                double maxAge = Convert.ToDouble(
                    value: parameterService.GetParameterValue(id: page.CacheMaxAgeDataConstantId)
                );
                context.Response.CacheSetMaxAge(timeSpan: TimeSpan.FromSeconds(value: maxAge));
            }
            Dictionary<string, object> mappedParameters = MapParameters(
                context: context,
                urlParameters: urlParameters,
                page: page,
                requestMimeType: mimeType
            );
            IPageRequestHandler handler = HandlePage(page: page);
            handler.Execute(
                page: page,
                parameters: mappedParameters,
                request: context.Request,
                response: context.Response
            );
            context.Response.End();
        }
        catch (ThreadAbortException) { }
        catch (Exception ex)
        {
            if (log.IsErrorEnabled)
            {
                log.LogOrigamError(
                    message: $@"Error occured ({ex.GetType()}) for request: 
                    {context.Request?.AbsoluteUri}: {ex.Message}",
                    ex: ex
                );
            }
            if (log.IsDebugEnabled)
            {
                log.DebugFormat(format: "Result Content Type: {0}", arg0: resultContentType);
            }

            context.Response.Clear();
            context.Response.StatusCode = GetStatusCode(ex: ex);
            context.Response.ContentType = "application/json";
            context.Response.TrySkipIisCustomErrors = true;
            string message = GetErrorObject(ex: ex);
            context.Response.Write(message: message);
            context.Response.End();
        }
    }

    private string GetErrorObject(Exception ex)
    {
        if (ex is RuleException ruleEx)
        {
            return $@"{{""Message"" : 
                    {JsonConvert.SerializeObject(value: ruleEx.Message)}, 
                    ""RuleResult"" : 
                    {JsonConvert.SerializeObject(value: ruleEx.RuleResult)}}}";
        }
        if (environment.IsProduction())
        {
            return "There was an error, check log for details";
        }
        return JsonConvert.SerializeObject(value: ex);
    }

    private int GetStatusCode(Exception ex)
    {
        if (ex is RuleException ruleException)
        {
            var dataList = ruleException.RuleResult.OfType<RuleExceptionData>().ToList();
            if (dataList.Count == 1)
            {
                return dataList[index: 0].HttpStatusCode;
            }
        }
        return 500;
    }

    protected virtual void Handle404(IHttpContextWrapper context)
    {
        // nothing found, let others handle it
    }

    private static void PageAnalytics(AbstractPage page, Dictionary<string, string> urlParameters)
    {
        Analytics.Instance.SetProperty(propertyName: "OrigamPageId", value: page.Id);
        Analytics.Instance.SetProperty(propertyName: "OrigamPageName", value: page.Name);
        foreach (KeyValuePair<string, string> pair in urlParameters)
        {
            Analytics.Instance.SetProperty(
                propertyName: "Parameter_" + pair.Key,
                value: pair.Value
            );
        }
        Analytics.Instance.Log(message: "PAGE_ACCESS");
    }

    private static void RequestAnalytics(IHttpContextWrapper context, string mimeType)
    {
        Analytics.Instance.SetProperty(propertyName: "ContentType", value: mimeType);
        Analytics.Instance.SetProperty(
            propertyName: "HttpMethod",
            value: context.Request.HttpMethod
        );
        Analytics.Instance.SetProperty(propertyName: "RawUrl", value: context.Request.RawUrl);
        Analytics.Instance.SetProperty(propertyName: "Url", value: context.Request.Url);
        Analytics.Instance.SetProperty(
            propertyName: "UrlReferrer",
            value: context.Request.UrlReferrer
        );
        Analytics.Instance.SetProperty(propertyName: "UserAgent", value: context.Request.UserAgent);
        Analytics.Instance.SetProperty(propertyName: "BrowserName", value: context.Request.Browser);
        Analytics.Instance.SetProperty(
            propertyName: "BrowserVersion",
            value: context.Request.BrowserVersion
        );
        Analytics.Instance.SetProperty(
            propertyName: "UserHostAddress",
            value: context.Request.UserHostAddress
        );
        Analytics.Instance.SetProperty(
            propertyName: "UserHostName",
            value: context.Request.UserHostName
        );
        Analytics.Instance.SetProperty(
            propertyName: "UserLanguages",
            value: context.Request.UserLanguages
        );
        foreach (var key in context.Request.Params.Keys)
        {
            Analytics.Instance.SetProperty(
                propertyName: "Parameter_" + key,
                value: context.Request.Params[key: key]
            );
        }
    }

    private IPageRequestHandler HandlePage(AbstractPage page)
    {
        IPageRequestHandler handler;
        switch (page)
        {
            case XsltDataPage _:
            {
                handler = new XsltPageRequestHandler();
                break;
            }

            case WorkflowPage _:
            {
                handler = new WorkflowPageRequestHandler();
                break;
            }

            case FileDownloadPage _:
            {
                handler = new FileDownloadPageRequestHandler(httpTools: httpTools);
                break;
            }

            case ReportPage _:
            {
                handler = new ReportPageRequestHandler();
                break;
            }

            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(page),
                    actualValue: page,
                    message: Resources.ErrorUnknownPageType
                );
            }
        }
        return handler;
    }

    private static Dictionary<string, object> MapParameters(
        IHttpContextWrapper context,
        Dictionary<string, string> urlParameters,
        AbstractPage page,
        string requestMimeType
    )
    {
        IParameterService parameterService =
            ServiceManager.Services.GetService(serviceType: typeof(IParameterService))
            as IParameterService;
        Dictionary<string, object> mappedParameters = new Dictionary<string, object>();
        // add url and content parts mapped to parameters
        // firstly process normal parameters and then after that process
        // process content parameters, because filling a parameter from content
        // to datastructure could make a use of other parameters while applying
        // dynamic defaults to newly created dataset.
        List<PageParameterMapping> contentParameters = new List<PageParameterMapping>();
        foreach (
            var ppm in page.ChildItemsByType<PageParameterMapping>(
                itemType: PageParameterMapping.CategoryConst
            )
        )
        {
            PageParameterFileMapping fileMapping = ppm as PageParameterFileMapping;
            if (fileMapping != null)
            {
                // files
                MapFileToParameter(
                    context: context,
                    mappedParameters: mappedParameters,
                    ppm: ppm,
                    fileMapping: fileMapping
                );
            }
            else if (ppm.MappedParameter == null || ppm.MappedParameter == "")
            {
                contentParameters.Add(item: ppm);
            }
            else
            {
                MapOtherParameters(
                    context: context,
                    urlParameters: urlParameters,
                    parameterService: parameterService,
                    mappedParameters: mappedParameters,
                    ppm: ppm,
                    requestMimeType: requestMimeType
                );
            }
        }
        // now process content parameters
        foreach (PageParameterMapping ppm in contentParameters)
        {
            MapContentToParameter(
                context: context,
                page: page,
                requestMimeType: requestMimeType,
                mappedParameters: mappedParameters,
                ppm: ppm
            );
        }
        string pageSize = context.Request.Params[key: "_pageSize"];
        string pageNumber = context.Request.Params[key: "_pageNumber"];
        if (pageSize != null)
        {
            mappedParameters.Add(key: "_pageSize", value: pageSize);
        }
        if (pageNumber != null)
        {
            mappedParameters.Add(key: "_pageNumber", value: pageNumber);
        }
        return mappedParameters;
    }

    private static void MapOtherParameters(
        IHttpContextWrapper context,
        Dictionary<string, string> urlParameters,
        IParameterService parameterService,
        Dictionary<string, object> mappedParameters,
        PageParameterMapping ppm,
        string requestMimeType
    )
    {
        string paramValue = null;
        // URL parameter
        if (urlParameters.ContainsKey(key: ppm.MappedParameter))
        {
            paramValue = urlParameters[key: ppm.MappedParameter];
            if (log.IsDebugEnabled)
            {
                log.DebugFormat(
                    format: "Mapping URL parameter {0}, value: {1}.",
                    arg0: ppm.Name,
                    arg1: paramValue
                );
            }
        }
        else
        // POST parameters
        {
            paramValue = context.Request.Params[key: ppm.MappedParameter];
            if (paramValue != null)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat(
                        format: "Mapping POST parameter {0}, value: {1}.",
                        arg0: ppm.Name,
                        arg1: paramValue
                    );
                }
            }
        }
        // if empty, set default value
        if (paramValue == null && ppm.DefaultValue != null)
        {
            paramValue = (string)
                parameterService.GetParameterValue(
                    id: ppm.DataConstantId,
                    targetType: Origam.Schema.OrigamDataType.String
                );
        }
        if (paramValue != null && ppm.IsList)
        {
            // we convert csv to proper arrays
            var list = new List<string>();
            string separator = ",";
            if (ppm.ListSeparator != null)
            {
                separator = (string)
                    parameterService.GetParameterValue(
                        id: ppm.SeparatorDataConstantId,
                        targetType: Origam.Schema.OrigamDataType.String
                    );
            }
            list.AddRange(collection: paramValue.Split(separator: separator.ToCharArray()));
            mappedParameters.Add(key: ppm.Name, value: list);
        }
        else if (paramValue != null)
        {
            // simple data types
            mappedParameters.Add(key: ppm.Name, value: paramValue);
        }
        string systemParameterValue = null;
        switch (ppm.MappedParameter)
        {
            case "Server_ContentType":
            {
                systemParameterValue = requestMimeType;
                break;
            }

            case "Server_HttpMethod":
            {
                systemParameterValue = context.Request.HttpMethod;
                break;
            }

            case "Server_RawUrl":
            {
                systemParameterValue = context.Request.RawUrl;
                break;
            }

            case "Server_Url":
            {
                systemParameterValue = context.Request.Url;
                break;
            }

            case "Server_UrlReferrer":
            {
                systemParameterValue = context.Request.UrlReferrer;
                break;
            }

            case "Server_UserAgent":
            {
                systemParameterValue = context.Request.UserAgent;
                break;
            }

            case "Server_BrowserName":
            {
                systemParameterValue = context.Request.Browser;
                break;
            }

            case "Server_BrowserVersion":
            {
                systemParameterValue = context.Request.BrowserVersion;
                break;
            }

            case "Server_UserHostAddress":
            {
                systemParameterValue = context.Request.UserHostAddress;
                break;
            }

            case "Server_UserHostName":
            {
                systemParameterValue = context.Request.UserHostName;
                break;
            }

            case "Server_UserLanguages":
            {
                systemParameterValue = string.Join(
                    separator: ",",
                    values: context.Request.UserLanguages
                );
                break;
            }
        }
        if (systemParameterValue != null)
        {
            mappedParameters.Add(key: ppm.Name, value: systemParameterValue);
        }
    }

    private static void MapFileToParameter(
        IHttpContextWrapper context,
        Dictionary<string, object> mappedParameters,
        PageParameterMapping ppm,
        PageParameterFileMapping fileMapping
    )
    {
        PostedFile file = context.Request.FilesGet(name: ppm.MappedParameter);
        if (file != null)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat(
                    format: "Mapping File Parameter {0}, FileInfo: {1}",
                    arg0: fileMapping.Name,
                    arg1: fileMapping.FileInfoType
                );
            }
            switch (fileMapping.FileInfoType)
            {
                case PageParameterFileInfo.ContentType:
                {
                    mappedParameters.Add(key: ppm.Name, value: file.ContentType);
                    break;
                }

                case PageParameterFileInfo.FileContent:
                {
                    byte[] fileBytes = GetFileBytes(fileMapping: fileMapping, file: file);
                    if (fileBytes != null)
                    {
                        mappedParameters.Add(key: ppm.Name, value: fileBytes);
                    }
                    break;
                }

                case PageParameterFileInfo.FileName:
                {
                    mappedParameters.Add(key: ppm.Name, value: file.FileName);
                    break;
                }

                case PageParameterFileInfo.FileSize:
                {
                    mappedParameters.Add(key: ppm.Name, value: file.ContentLength);
                    break;
                }

                default:
                {
                    throw new ArgumentOutOfRangeException(
                        paramName: "FileInfoType",
                        actualValue: fileMapping.FileInfoType,
                        message: "Unknown Type"
                    );
                }
            }
        }
    }

    private static void MapContentToParameter(
        IHttpContextWrapper context,
        AbstractPage page,
        string requestMimeType,
        Dictionary<string, object> mappedParameters,
        PageParameterMapping ppm
    )
    {
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(
                format: "Mapping parameter {0}, Request content type: {1}",
                arg0: ppm.Name,
                arg1: requestMimeType
            );
        }
        System.IO.StreamReader reader = new System.IO.StreamReader(
            stream: context.Request.InputStream,
            encoding: context.Request.ContentEncoding
        );
        IXmlContainer doc = new XmlContainer();
        DataSet data = null;
        WorkflowPage wfPage = page as WorkflowPage;
        XsltDataPage dataPage = page as XsltDataPage;
        var workflow = wfPage?.Workflow;
        if (dataPage?.Method is DataStructureWorkflowMethod workflowMethod)
        {
            workflow = workflowMethod.LoadWorkflow;
        }
        if (
            dataPage != null
            && dataPage.DataStructure != null
            && context.Request.HttpMethod == "PUT"
        )
        {
            GetEmptyData(
                doc: ref doc,
                data: ref data,
                ds: dataPage.DataStructure,
                mappedParameters: mappedParameters,
                defaultSet: dataPage.DefaultSet
            );
            if (dataPage.DisableConstraintForInputValidation)
            {
                data.EnforceConstraints = false;
            }
        }
        else if (workflow != null)
        {
            ContextStore ctx =
                workflow.GetChildByName(name: ppm.Name, itemType: ContextStore.CategoryConst)
                as ContextStore;
            if (ctx == null)
            {
                throw new ArgumentException(
                    message: String.Format(
                        format: "Couldn't find a context store with "
                            + "the name `{0}' in a workflow `{1}' ({2})'.",
                        arg0: ppm.Name,
                        arg1: workflow.Path,
                        arg2: workflow.Id
                    )
                );
            }
            DataStructure ds = ctx.Structure as DataStructure;
            if (ds != null)
            {
                GetEmptyData(
                    doc: ref doc,
                    data: ref data,
                    ds: ds,
                    mappedParameters: mappedParameters,
                    defaultSet: ctx.DefaultSet
                );
                if (
                    ctx.DisableConstraints
                    || (wfPage != null && wfPage.DisableConstraintForInputValidation)
                )
                {
                    data.EnforceConstraints = false;
                }
            }
        }
        if (context.Request.ContentLength != 0)
        {
            // empty parameter name = deserialize body depending on the content-type
            try
            {
                switch (requestMimeType)
                {
                    case "text/xml":
                    {
                        doc.Xml.Load(txtReader: reader);
                        break;
                    }

                    case "application/json":
                    {
                        JsonSerializer js = new JsonSerializer();
                        string body = reader.ReadToEnd();
                        // DataSet ds = JsonConvert.DeserializeObject<DataSet>(body);
                        XmlDocument xd = new XmlDocument();
                        // deserialize from JSON to XML
                        if (string.IsNullOrEmpty(value: ppm.DatastructureEntityName))
                        {
                            xd = (XmlDocument)
                                JsonConvert.DeserializeXmlNode(
                                    value: body,
                                    deserializeRootElementName: "ROOT"
                                );
                        }
                        else
                        {
                            xd = (XmlDocument)
                                JsonConvert.DeserializeXmlNode(
                                    value: "{\""
                                        + ppm.DatastructureEntityName
                                        + "\" :"
                                        + body
                                        + "}",
                                    deserializeRootElementName: "ROOT"
                                );
                        }
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(message: "Intermediate JSON deserialized XML:");
                            log.Debug(message: xd.OuterXml);
                        }
                        // remove any empty elements because empty guids and dates would
                        // result in errors and empty strings should always be converted
                        // to nulls anyway
                        RemoveEmptyNodes(doc: ref xd);
                        if (log.IsDebugEnabled)
                        {
                            log.Debug(message: "Deserialized XML after removing empty elements:");
                            log.Debug(message: xd.OuterXml);
                        }
                        if (data == null)
                        {
                            doc = new XmlContainer(xmlDocument: xd);
                        }
                        else
                        {
                            foreach (DataTable table in data.Tables)
                            {
                                foreach (DataColumn col in table.Columns)
                                {
                                    if (col.DataType == typeof(DateTime))
                                    {
                                        col.DateTimeMode = DataSetDateTime.Local;
                                    }
                                }
                            }
                            // ignore-schema because the source might not contain some elements (nulls)
                            // and it would try to remove these columns from the target dataset
                            data.ReadXml(
                                reader: new XmlNodeReader(node: xd),
                                mode: XmlReadMode.IgnoreSchema
                            );
                        }
                        break;
                    }

                    default:
                    {
                        throw new ArgumentOutOfRangeException(
                            paramName: "ContentType",
                            actualValue: requestMimeType,
                            message: "Unknown content type. Use text/xml or application/json."
                        );
                    }
                }
            }
            catch (ConstraintException)
            {
                // make the exception far more verbose
                throw new ConstraintException(s: DatasetTools.GetDatasetErrors(dataset: data));
            }
        }
        mappedParameters.Add(key: ppm.Name, value: doc);
        if (log.IsDebugEnabled)
        {
            log.Debug(message: "Result XML:");
            log.Debug(message: doc.Xml.OuterXml);
        }
    }

    private static void GetEmptyData(
        ref IXmlContainer doc,
        ref DataSet data,
        DataStructure ds,
        Dictionary<string, object> mappedParameters
    )
    {
        GetEmptyData(
            doc: ref doc,
            data: ref data,
            ds: ds,
            mappedParameters: mappedParameters,
            defaultSet: null
        );
    }

    private static void GetEmptyData(
        ref IXmlContainer doc,
        ref DataSet data,
        DataStructure ds,
        Dictionary<string, object> mappedParameters,
        DataStructureDefaultSet defaultSet
    )
    {
        DatasetGenerator dsg = new DatasetGenerator(userDefinedParameters: true);
        data = dsg.CreateDataSet(ds: ds, defaultSet: defaultSet);
        DatasetGenerator.ApplyDynamicDefaults(data: data, parameters: mappedParameters);
        doc = DataDocumentFactory.New(dataSet: data);
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(format: "Mapping content to data structure: {0}", arg0: ds.Path);
        }
    }

    public static void RemoveEmptyNodes(ref XmlDocument doc)
    {
        XmlNodeList nodes = doc.SelectNodes(xpath: "//*");
        if (log.IsDebugEnabled)
        {
            foreach (XmlNode node in nodes)
            {
                log.DebugFormat(
                    format: "Node: {0} Value: {1} HasChildNodes: {2}",
                    arg0: node.Name,
                    arg1: node.Value == null ? "<null>" : node.Value,
                    arg2: node.HasChildNodes
                );
            }
        }
        foreach (XmlNode node in nodes)
        {
            XmlElement xe = node as XmlElement;
            if (
                (xe != null && xe.IsEmpty)
                || (
                    (
                        !node.HasChildNodes
                        || (
                            node.ChildNodes.Count == 1
                            && node.ChildNodes[i: 0].Name == "#text"
                            && node.ChildNodes[i: 0].Value == ""
                        )
                    )
                    && (node.Value == null || node.InnerText == "")
                    && ((node.Attributes == null) || (node.Attributes.Count == 0))
                )
            )
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat(format: "Removing empty node: {0}", arg0: node.Name);
                }
                node.ParentNode.RemoveChild(oldChild: node);
            }
        }
    }

    private static byte[] GetFileBytes(PageParameterFileMapping fileMapping, PostedFile file)
    {
        byte[] fileBytes = null;
        if (fileMapping.ThumbnailHeight == 0 && fileMapping.ThumbnailWidth == 0)
        {
            // get the original file
            using var memoryStream = new MemoryStream();
            file.InputStream.CopyTo(destination: memoryStream);
            fileBytes = memoryStream.ToArray();
        }
        else
        {
            // get a thumbnail
            try
            {
                using Image image = Image.Load(stream: file.InputStream);
                fileBytes = BlobUploadHandler.FixedSizeBytes(
                    image: image,
                    width: fileMapping.ThumbnailWidth,
                    height: fileMapping.ThumbnailHeight
                );
            }
            catch
            {
                fileBytes = null;
            }
        }
        return fileBytes;
    }

    private (int, AbstractPage) ResolvePage(
        IHttpContextWrapper context,
        out Dictionary<string, string> outUrlParameters
    )
    {
        outUrlParameters = new Dictionary<string, string>();
        var currentUrlParams = new Dictionary<string, string>();

        string path = context.Request.AppRelativeCurrentExecutionFilePath;
        if (log.IsDebugEnabled)
        {
            log.DebugFormat(format: "Resolving page {0}.", arg0: path);
        }
        IOrigamAuthorizationProvider auth = SecurityManager.GetAuthorizationProvider();
        SchemaService schemaService = ServiceManager.Services.GetService<SchemaService>();
        PagesSchemaItemProvider pages = schemaService.GetProvider<PagesSchemaItemProvider>();
        string[] requestPath = path.Split(
            separator: new string[] { "/" },
            options: StringSplitOptions.None
        );
        List<AbstractPage> validPagesByVerbAndPath = new List<AbstractPage>();
        // try to find parameterless page first, remove the preceding ~/ from the path
        AbstractPage parameterlessResultPage = urlApiPageCache.Value.GetParameterlessPage(
            incommingPath: String.Join(separator: "/", values: requestPath.Skip(count: 1))
        );
        if (parameterlessResultPage != null)
        {
            if (IsPageValidByVerb(page: parameterlessResultPage, context: context))
            {
                return ValidatePageSecurity(auth: auth, page: parameterlessResultPage);
            }
        }
        // try other parameterized endpoints with the old approach
        foreach (AbstractPage page in urlApiPageCache.Value.GetParameterPages())
        {
            currentUrlParams.Clear();
            if (!IsPageValidByVerb(page: page, context: context))
            {
                continue;
            }
            bool invalidRequest = false;
            string[] pagePath = page.Url.Split(
                separator: new string[] { "/" },
                options: StringSplitOptions.None
            );
            if ((requestPath.Length - 1) != pagePath.Length)
            {
                continue;
            }
            for (int i = 0; i < pagePath.Length; i++)
            {
                string pathPart = pagePath[i];
                // parameter
                if (pathPart.StartsWith(value: "{"))
                {
                    if (requestPath.Length <= i + 1)
                    {
                        continue;
                    }
                    string paramValue = HttpUtility.UrlDecode(str: requestPath[i + 1]);
                    if (paramValue.EndsWith(value: ".aspx") && (i == (requestPath.Length - 2)))
                    {
                        // remove .aspx from the end of the string
                        paramValue = paramValue.Substring(
                            startIndex: 0,
                            length: paramValue.Length - 5
                        );
                    }
                    string paramName;
                    if (pathPart.EndsWith(value: ".aspx"))
                    {
                        paramName = pathPart.Substring(startIndex: 1, length: pathPart.Length - 7);
                    }
                    else
                    {
                        paramName = pathPart.Substring(startIndex: 1, length: pathPart.Length - 2);
                    }
                    currentUrlParams.Add(key: paramName, value: paramValue);
                }
                // path
                else
                {
                    string paramValue = requestPath[i + 1];
                    if (paramValue.EndsWith(value: ".aspx") && (i == (requestPath.Length - 2)))
                    {
                        // remove .aspx from the end of the string
                        paramValue = paramValue.Substring(
                            startIndex: 0,
                            length: paramValue.Length - 5
                        );
                    }
                    if (pathPart != paramValue)
                    {
                        // this is not the right page, let's try another
                        invalidRequest = true;
                        break;
                    }
                }
            }
            if (!invalidRequest)
            {
                validPagesByVerbAndPath.Add(item: page);
                outUrlParameters = new Dictionary<string, string>(dictionary: currentUrlParams);
            }
        }
        switch (validPagesByVerbAndPath.Count)
        {
            case 0:
            {
                return (404, null);
            }
            case 1:
            {
                return ValidatePageSecurity(auth: auth, page: validPagesByVerbAndPath[index: 0]);
            }
            default:
            {
                log.ErrorFormat(
                    format: "Multiple routes detected '{0}' for request '{1}'",
                    arg0: validPagesByVerbAndPath
                        .Select(selector: x => x.Id.ToString() + ":" + x.Url)
                        .Aggregate(func: (res, i) => res + "," + i),
                    arg1: path
                );
                return (500, null);
            }
        }
    }

    private static bool IsPageValidByVerb(AbstractPage page, IHttpContextWrapper context)
    {
        switch (context.Request.HttpMethod)
        {
            case "PUT":
            {
                return page.AllowPUT;
            }
            case "DELETE":
            {
                return page.AllowDELETE;
            }
            case "OPTIONS":
            {
                return false;
            }
            default:
            {
                return true;
            }
        }
    }

    private static (int, AbstractPage) ValidatePageSecurity(
        IOrigamAuthorizationProvider auth,
        AbstractPage page
    )
    {
        try
        {
            return auth.Authorize(principal: SecurityManager.CurrentPrincipal, context: page.Roles)
                ? (200, page)
                : (403, null);
        }
        catch (UserNotLoggedInException)
        {
            return (401, null);
        }
    }
    #endregion
}

public class PostedFile
{
    public object ContentType { get; set; }
    public object FileName { get; set; }
    public long ContentLength { get; set; }
    public Stream InputStream { get; set; }
}

public interface IHttpContextWrapper
{
    IResponseWrapper Response { get; }
    IRequestWrapper Request { get; }
}

public interface IRequestWrapper
{
    string AppRelativeCurrentExecutionFilePath { get; }
    string ContentType { get; }
    string AbsoluteUri { get; }
    Stream InputStream { get; }
    string HttpMethod { get; }
    string RawUrl { get; }
    string Url { get; }
    string UrlReferrer { get; }
    string UserAgent { get; }
    string Browser { get; }
    string BrowserVersion { get; }
    string UserHostAddress { get; }
    string UserHostName { get; }
    IEnumerable<string> UserLanguages { get; }
    Encoding ContentEncoding { get; }
    long ContentLength { get; }
    IDictionary BrowserCapabilities { get; }
    string UrlReferrerAbsoluteUri { get; }
    Parameters Params { get; }
    PostedFile FilesGet(string name);
}

public interface IResponseWrapper
{
    bool BufferOutput { set; }
    string ContentType { set; }
    bool TrySkipIisCustomErrors { set; }
    int StatusCode { set; }
    string Charset { get; set; }
    void WriteToOutput(Action<TextWriter> writeAction);
    void CacheSetMaxAge(TimeSpan timeSpan);
    void End();
    void Clear();
    void Write(string message);
    void AddHeader(string name, string value);
    void BinaryWrite(byte[] bytes);
    void Redirect(string requestUrlReferrerAbsolutePath);
    void OutputStreamWrite(byte[] buffer, int offset, int count);
    void AppendHeader(string contentDisposition, string disposition);
}

public class Parameters
{
    private readonly Dictionary<string, string> paramsDictionary;

    public Parameters(Dictionary<string, string> parameters)
    {
        paramsDictionary = parameters;
    }

    public string this[string key]
    {
        get
        {
            paramsDictionary.TryGetValue(key: key, value: out string value);
            return value;
        }
    }
    public IEnumerable<string> Keys => paramsDictionary.Keys;
}
