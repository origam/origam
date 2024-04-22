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
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using Newtonsoft.Json;
using Origam.DA;
using Origam.DA.Service;
using Origam.Rule;
using Origam.Schema.EntityModel;
using Origam.Schema.GuiModel;
using Origam.Schema.WorkflowModel;
using Origam.Server;
using Origam.Workbench.Services;
using System.Linq;
using System.Web;
using Origam.Extensions;
using Origam.Service.Core;
using ImageMagick;
using IdentityServer4.Extensions;

namespace Origam.Server.Pages;

public class UserApiProcessor
{
    private readonly IHttpTools httpTools;
    private static readonly log4net.ILog log 
        = log4net.LogManager.GetLogger(
            System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
    private Lazy<UrlApiPageCache> urlApiPageCache =
        new Lazy<UrlApiPageCache>(() => new UrlApiPageCache(                    
            (ServiceManager.Services.GetService<SchemaService>())
            .GetProvider<PagesSchemaItemProvider>()
        ));

    public UserApiProcessor(IHttpTools httpTools)
    {
            this.httpTools = httpTools;
        }

    #region IHttpModule Members

    public void Dispose()
    {
        }

    public void Process(IHttpContextWrapper context)
    {
            if (context.Request.AppRelativeCurrentExecutionFilePath.StartsWith(
                "~/assets/"))
            {
                return;
            }
            string mimeType = context.Request.ContentType?.Split(
                ";".ToCharArray())[0];
            if (mimeType == "application/x-amf")
            {
                return;
            }
            string resultContentType = "text/plain";
            if (Analytics.Instance.IsAnalyticsEnabled)
            {
                RequestAnalytics(context, mimeType);
            }
            try
            {
                var (code, page) = ResolvePage(context, out var urlParameters);
                if (code == 404)
                {
                    Handle404(context);
                    return;
                }
                if (code != 200)
                {
                    context.Response.StatusCode = code;
                    context.Response.ContentType = resultContentType;
                    context.Response.Clear();
                    if (code == 500)
                    {
                        context.Response.Write("Multiple routes.");
                    }
                    context.Response.End();
                    return;
                }
                if (Analytics.Instance.IsAnalyticsEnabled)
                {
                    PageAnalytics(page, urlParameters);
                }
                if (page.MimeType != "?")
                {
                    if (page.MimeType == "text/calendar")
                    {
                        context.Response.BufferOutput = false;
                    }
                    context.Response.ContentType 
                        = page.MimeType;
                    // set result content type for exception handling
                    // so the exception is formatted as expected, e.g. in json
                    resultContentType = page.MimeType;
                }
                if (page.CacheMaxAge == null)
                {
                    context.Response.CacheSetMaxAge(new TimeSpan(0));
                }
                else
                {
                    IParameterService parameterService 
                        = ServiceManager.Services
                            .GetService<IParameterService>();
                    double maxAge = Convert.ToDouble(
                        parameterService.GetParameterValue(
                            page.CacheMaxAgeDataConstantId));
                    context.Response.CacheSetMaxAge(
                        TimeSpan.FromSeconds(maxAge));
                }
                Dictionary<string, object> mappedParameters = MapParameters(
                    context, urlParameters, page, mimeType);
                IPageRequestHandler handler = HandlePage(page);
                handler.Execute(
                    page, mappedParameters, context.Request, 
                    context.Response);
                context.Response.End();
            }
            catch (ThreadAbortException)
            {
            }
            catch (Exception ex)
            {
                if (log.IsErrorEnabled) 
                {
                    log.LogOrigamError(
                        $@"Error occured ({ex.GetType()}) for request: 
                        {context.Request?.AbsoluteUri}: {ex.Message}"
                        , ex);
                }
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Result Content Type: {0}",
                        resultContentType);
                }
                string message;
                context.Response.TrySkipIisCustomErrors = true;
                if (ex is RuleException ruleEx)
                {
                    message =
                        $@"{{""Message"" : 
                        {JsonConvert.SerializeObject(ruleEx.Message)}, 
                        ""RuleResult"" : 
                        {JsonConvert.SerializeObject(ruleEx.RuleResult)}}}";
                }
                else
                {
                    message = JsonConvert.SerializeObject(ex);
                }
                context.Response.Clear();
                context.Response.StatusCode = GetStausCode(ex);
                context.Response.ContentType = "application/json";   
                context.Response.Write(message);
                context.Response.End();
            }
        }

    private int GetStausCode(Exception ex)
    {
            if (ex is RuleException ruleException)
            {
                var dataList = ruleException.RuleResult
                    .OfType<RuleExceptionData>()
                    .ToList();
                if (dataList.Count == 1)
                {
                    return dataList[0].HttpStatusCode;
                }
            }
            return 400;
        }

    protected virtual void Handle404(IHttpContextWrapper context)
    { 
            // nothing found, let others handle it
        }

    private static void PageAnalytics(
        AbstractPage page, Dictionary<string, string> urlParameters)
    {
            Analytics.Instance.SetProperty("OrigamPageId", page.Id);
            Analytics.Instance.SetProperty("OrigamPageName", page.Name);
            foreach (KeyValuePair<string, string> pair in urlParameters)
            {
                Analytics.Instance.SetProperty(
                    "Parameter_" + pair.Key, pair.Value);
            }
            Analytics.Instance.Log("PAGE_ACCESS");
        }
        
    private static void RequestAnalytics(
        IHttpContextWrapper context, string mimeType)
    {
            Analytics.Instance.SetProperty(
                "ContentType", mimeType);
            Analytics.Instance.SetProperty(
                "HttpMethod", context.Request.HttpMethod);
            Analytics.Instance.SetProperty(
                "RawUrl", context.Request.RawUrl);
            Analytics.Instance.SetProperty(
                "Url", context.Request.Url);
            Analytics.Instance.SetProperty(
                "UrlReferrer", context.Request.UrlReferrer);
            Analytics.Instance.SetProperty(
                "UserAgent", context.Request.UserAgent);
            Analytics.Instance.SetProperty(
                "BrowserName", context.Request.Browser);
            Analytics.Instance.SetProperty(
                "BrowserVersion", context.Request.BrowserVersion);
            Analytics.Instance.SetProperty(
                "UserHostAddress", context.Request.UserHostAddress);
            Analytics.Instance.SetProperty(
                "UserHostName", context.Request.UserHostName);
            Analytics.Instance.SetProperty(
                "UserLanguages", context.Request.UserLanguages);
            foreach (var key in context.Request.Params.Keys)
            {
                Analytics.Instance.SetProperty(
                    "Parameter_" + key, context.Request.Params[key]);
            }
        }

    private IPageRequestHandler HandlePage(AbstractPage page)
    {
            IPageRequestHandler handler;
            switch (page)
            {
                case XsltDataPage _:
                    handler = new XsltPageRequestHandler();
                    break;
                case WorkflowPage _:
                    handler = new WorkflowPageRequestHandler();
                    break;
                case FileDownloadPage _:
                    handler = new FileDownloadPageRequestHandler(httpTools);
                    break;
                case ReportPage _:
                    handler = new ReportPageRequestHandler();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(page), page, Resources.ErrorUnknownPageType);
            }
            return handler;
        }

    private static Dictionary<string, object> MapParameters(IHttpContextWrapper context, Dictionary<string, string> urlParameters, AbstractPage page, string requestMimeType)
    {
            IParameterService parameterService = ServiceManager.Services.GetService(typeof(IParameterService)) as IParameterService;
            Dictionary<string, object> mappedParameters = new Dictionary<string, object>();

            // add url and content parts mapped to parameters

            // firstly process normal parameters and then after that process
            // process content parameters, because filling a parameter from content
            // to datastructure could make a use of other parameters while applying
            // dynamic defaults to newly created dataset.
            List<PageParameterMapping> contentParameters = new List<PageParameterMapping>();
            foreach (PageParameterMapping ppm in page.ChildItemsByType(PageParameterMapping.CategoryConst))
            {
                PageParameterFileMapping fileMapping = ppm as PageParameterFileMapping;

                if (fileMapping != null)
                {
                    // files
                    MapFileToParameter(context, mappedParameters, ppm, fileMapping);

                }
                else if (ppm.MappedParameter == null || ppm.MappedParameter == "")
                {
                    contentParameters.Add(ppm);
                }
                else
                {
                    MapOtherParameters(context, urlParameters, parameterService, mappedParameters, ppm, requestMimeType);
                }
            }
            // now process content parameters
            foreach (PageParameterMapping ppm in contentParameters)
            {
                MapContentToParameter(context, page, requestMimeType, mappedParameters, ppm);
            }

            string pageSize = context.Request.Params["_pageSize"];
            string pageNumber = context.Request.Params["_pageNumber"];
            if (pageSize != null)
            {
                mappedParameters.Add("_pageSize", pageSize);
            }
            if (pageNumber != null)
            {
                mappedParameters.Add("_pageNumber", pageNumber);
            }
            return mappedParameters;
        }

    private static void MapOtherParameters(IHttpContextWrapper context, Dictionary<string, string> urlParameters,
        IParameterService parameterService, Dictionary<string, object> mappedParameters, PageParameterMapping ppm,
        string requestMimeType)
    {
            string paramValue = null;

            // URL parameter
            if (urlParameters.ContainsKey(ppm.MappedParameter))
            {
                paramValue = urlParameters[ppm.MappedParameter];
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Mapping URL parameter {0}, value: {1}.", ppm.Name, paramValue);
                }
            }
            else
            // POST parameters
            {
                paramValue = context.Request.Params[ppm.MappedParameter];

                if (paramValue != null)
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Mapping POST parameter {0}, value: {1}.", ppm.Name, paramValue);
                    }
                }
            }

            // if empty, set default value
            if (paramValue == null && ppm.DefaultValue != null)
            {
                paramValue = (string)parameterService.GetParameterValue(ppm.DataConstantId, Origam.Schema.OrigamDataType.String);
            }

            if (paramValue != null && ppm.IsList)
            {
                // we convert csv to proper arrays
                ArrayList list = new ArrayList();
                string separator = ",";

                if (ppm.ListSeparator != null)
                {
                    separator = (string)parameterService.GetParameterValue(ppm.SeparatorDataConstantId, Origam.Schema.OrigamDataType.String);
                }

                list.AddRange(paramValue.Split(separator.ToCharArray()));

                mappedParameters.Add(ppm.Name, list);
            }
            else if (paramValue != null)
            {
                // simple data types
                mappedParameters.Add(ppm.Name, paramValue);
            }

            string systemParameterValue = null;
            switch (ppm.MappedParameter)
            {
                case "Server_ContentType":
                    systemParameterValue = requestMimeType;
                    break;
                case "Server_HttpMethod":
                    systemParameterValue = context.Request.HttpMethod;
                    break;
                case "Server_RawUrl":
                    systemParameterValue = context.Request.RawUrl;
                    break;
                case "Server_Url":
                    systemParameterValue = context.Request.Url;
                    break;
                case "Server_UrlReferrer":
                    systemParameterValue = context.Request.UrlReferrer;
                    break;
                case "Server_UserAgent":
                    systemParameterValue = context.Request.UserAgent;
                    break;
                case "Server_BrowserName":
                    systemParameterValue = context.Request.Browser;
                    break;
                case "Server_BrowserVersion":
                    systemParameterValue = context.Request.BrowserVersion;
                    break;
                case "Server_UserHostAddress":
                    systemParameterValue = context.Request.UserHostAddress;
                    break;
                case "Server_UserHostName":
                    systemParameterValue = context.Request.UserHostName;
                    break;
                case "Server_UserLanguages":
                    systemParameterValue = string.Join(",", context.Request.UserLanguages);
                    break;
            }

            if (systemParameterValue != null)
            {
                mappedParameters.Add(ppm.Name, systemParameterValue);
            }
        }

    private static void MapFileToParameter(IHttpContextWrapper context, Dictionary<string, object> mappedParameters, PageParameterMapping ppm, PageParameterFileMapping fileMapping)
    {
            PostedFile file = context.Request.FilesGet(ppm.MappedParameter);

            if (file != null)
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat("Mapping File Parameter {0}, FileInfo: {1}", fileMapping.Name, fileMapping.FileInfoType);
                }

                switch (fileMapping.FileInfoType)
                {
                    case PageParameterFileInfo.ContentType:
                        mappedParameters.Add(ppm.Name, file.ContentType);
                        break;
                    case PageParameterFileInfo.FileContent:
                        byte[] fileBytes = GetFileBytes(fileMapping, file);

                        if (fileBytes != null)
                        {
                            mappedParameters.Add(ppm.Name, fileBytes);
                        }
                        break;
                    case PageParameterFileInfo.FileName:
                        mappedParameters.Add(ppm.Name, file.FileName);
                        break;
                    case PageParameterFileInfo.FileSize:
                        mappedParameters.Add(ppm.Name, file.ContentLength);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException("FileInfoType", fileMapping.FileInfoType, "Unknown Type");
                }
            }
        }

    private static void MapContentToParameter(IHttpContextWrapper context,
        AbstractPage page, string requestMimeType,
        Dictionary<string, object> mappedParameters,
        PageParameterMapping ppm)
    {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Mapping parameter {0}, Request content type: {1}", ppm.Name, requestMimeType);
            }

            System.IO.StreamReader reader = new System.IO.StreamReader(context.Request.InputStream, context.Request.ContentEncoding);
            IXmlContainer doc = new XmlContainer();
            DataSet data = null;
            WorkflowPage wfPage = page as WorkflowPage;
            XsltDataPage dataPage = page as XsltDataPage;
            var workflow = wfPage?.Workflow;
            if (dataPage?.Method is DataStructureWorkflowMethod workflowMethod)
            {
                workflow = workflowMethod.LoadWorkflow;
            }
            if (dataPage != null && dataPage.DataStructure != null && context.Request.HttpMethod == "PUT")
            {
                GetEmptyData(ref doc, ref data, dataPage.DataStructure, mappedParameters, dataPage.DefaultSet);
                if (dataPage.DisableConstraintForInputValidation)
                {
                    data.EnforceConstraints = false;
                }
            }
            else if (workflow != null)
            {
                ContextStore ctx = workflow.GetChildByName(ppm.Name, ContextStore.CategoryConst) as ContextStore;
                if (ctx == null)
                {
                    throw new ArgumentException(String.Format("Couldn't find a context store with " +
                        "the name `{0}' in a workflow `{1}' ({2})'.", ppm.Name, workflow.Path, workflow.Id));
                }
                DataStructure ds = ctx.Structure as DataStructure;
                if (ds != null)
                {
                    GetEmptyData(ref doc, ref data, ds, mappedParameters, ctx.DefaultSet);
                    if (ctx.DisableConstraints || (wfPage != null && wfPage.DisableConstraintForInputValidation))
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
                            doc.Xml.Load(reader);
                            break;

                        case "application/json":
                            JsonSerializer js = new JsonSerializer();
                            string body = reader.ReadToEnd();
                            // DataSet ds = JsonConvert.DeserializeObject<DataSet>(body);
                            XmlDocument xd = new XmlDocument();
                            // deserialize from JSON to XML
                            if (ppm.DatastructureEntityName.IsNullOrEmpty())
                            {
                                xd = (XmlDocument)JsonConvert
                                    .DeserializeXmlNode(body, "ROOT");
                            }
                            else
                            {
                                xd = (XmlDocument)JsonConvert
                                    .DeserializeXmlNode(
                                    	"{\"" + ppm.DatastructureEntityName
                                     	+ "\" :" + body + "}"
                                    , "ROOT");
                            }
                            if (log.IsDebugEnabled)
                            {
                                log.Debug("Intermediate JSON deserialized XML:");
                                log.Debug(xd.OuterXml);
                            }
                            // remove any empty elements because empty guids and dates would
                            // result in errors and empty strings should always be converted
                            // to nulls anyway
                            RemoveEmptyNodes(ref xd);

                            if (log.IsDebugEnabled)
                            {
                                log.Debug("Deserialized XML after removing empty elements:");
                                log.Debug(xd.OuterXml);
                            }
                            if (data == null)
                            {
                                doc = new XmlContainer(xd);
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
                                data.ReadXml(new XmlNodeReader(xd), XmlReadMode.IgnoreSchema);
                            }
                            break;

                        default:
                            throw new ArgumentOutOfRangeException("ContentType", requestMimeType, "Unknown content type. Use text/xml or application/json.");
                    }
                }
                catch (ConstraintException)
                {
                    // make the exception far more verbose
                    throw new ConstraintException(DatasetTools.GetDatasetErrors(data));
                }
            }

            mappedParameters.Add(ppm.Name, doc);
            if (log.IsDebugEnabled)
            {
                log.Debug("Result XML:");
                log.Debug(doc.Xml.OuterXml);
            }
        }

    private static void GetEmptyData(ref IXmlContainer doc,
        ref DataSet data, DataStructure ds,
        Dictionary<string, object> mappedParameters)
    {
            GetEmptyData(ref doc, ref data, ds, mappedParameters, null);
        }

    private static void GetEmptyData(ref IXmlContainer doc,
        ref DataSet data, DataStructure ds,
        Dictionary<string, object> mappedParameters,
        DataStructureDefaultSet defaultSet)
    {
            DatasetGenerator dsg = new DatasetGenerator(true);
            data = dsg.CreateDataSet(ds, defaultSet);
            DatasetGenerator.ApplyDynamicDefaults(data, mappedParameters);

            doc = DataDocumentFactory.New(data);

            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Mapping content to data structure: {0}", ds.Path);
            }
        }

    public static void RemoveEmptyNodes(ref XmlDocument doc)
    {
            XmlNodeList nodes = doc.SelectNodes("//*");

            if (log.IsDebugEnabled)
            {
                foreach (XmlNode node in nodes)
                {
                    log.DebugFormat("Node: {0} Value: {1} HasChildNodes: {2}", node.Name, node.Value == null ? "<null>" : node.Value, node.HasChildNodes);
                }
            }

            foreach (XmlNode node in nodes)
            {
                XmlElement xe = node as XmlElement;

                if ((xe != null && xe.IsEmpty) ||
                    (
                        (!node.HasChildNodes || (node.ChildNodes.Count == 1 && node.ChildNodes[0].Name == "#text" && node.ChildNodes[0].Value == ""))
                        && (node.Value == null || node.InnerText == "") &&
                        ((node.Attributes == null) || (node.Attributes.Count == 0)))
                    )
                {
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Removing empty node: {0}", node.Name);
                    }

                    node.ParentNode.RemoveChild(node);
                }
            }
        }

    private static byte[] GetFileBytes(PageParameterFileMapping fileMapping, PostedFile file)
    {
            byte[] fileBytes = null;

            if (fileMapping.ThumbnailHeight == 0 && fileMapping.ThumbnailWidth == 0)
            {
                // get the original file
                fileBytes = StreamTools.ReadToEnd(file.InputStream);
            }
            else
            {
                // get a thumbnail
                MagickImage img = null;

                try
                {
                    img = new MagickImage(file.InputStream);
                }
                catch
                {
                    // file is not an image, we return null
                }

                if (img != null)
                {
                    try
                    {
                        fileBytes = BlobUploadHandler.FixedSizeBytes(img, fileMapping.ThumbnailWidth, fileMapping.ThumbnailHeight);
                    }
                    finally
                    {
                        if (img != null) img.Dispose();
                    }
                }
            }
            return fileBytes;
        }

    private (int, AbstractPage) ResolvePage(
        IHttpContextWrapper context, 
        out Dictionary<string, string> outUrlParameters)
    {
            outUrlParameters = new Dictionary<string, string>();
            var currentUrlParams = new Dictionary<string, string>();
            
            string path = context.Request.AppRelativeCurrentExecutionFilePath;
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("Resolving page {0}.", path);
            }
            IOrigamAuthorizationProvider auth 
                = SecurityManager.GetAuthorizationProvider();
            SchemaService schemaService 
                = ServiceManager.Services.GetService<SchemaService>();
            PagesSchemaItemProvider pages 
                = schemaService.GetProvider<PagesSchemaItemProvider>();
            string[] requestPath = path.Split(
                new string[] { "/" }, StringSplitOptions.None);
            List<AbstractPage> validPagesByVerbAndPath =
                new List<AbstractPage>();

            // try to find parameterless page first, remove the preceding ~/ from the path
            AbstractPage parameterlessResultPage =
                urlApiPageCache.Value.GetParameterlessPage(
                    String.Join("/", requestPath.Skip(1)));
            if (parameterlessResultPage != null)
            {
                if (IsPageValidByVerb(parameterlessResultPage, context))
                {
                    return ValidatePageSecurity(
                        auth, parameterlessResultPage);
                }
            }

            // try other parameterized endpoints with the old approach
            foreach (AbstractPage page in
                urlApiPageCache.Value.GetParameterPages())
            {
                currentUrlParams.Clear();
                if (!IsPageValidByVerb(page, context))
                {
                    continue;
                }
                bool invalidRequest = false;
                string[] pagePath = page.Url.Split(
                    new string[] { "/" }, StringSplitOptions.None);
                if ((requestPath.Length - 1) != pagePath.Length)
                {
                    continue;
                }
                for (int i = 0; i < pagePath.Length; i++)
                {
                    string pathPart = pagePath[i];
                    // parameter
                    if (pathPart.StartsWith("{"))
                    {
                        if (requestPath.Length <= i + 1)
                        {
                            continue;
                        }
                        string paramValue 
                            = HttpUtility.UrlDecode(requestPath[i + 1]);
                        if (paramValue.EndsWith(".aspx") 
                            && (i == (requestPath.Length - 2)))
                        {
                            // remove .aspx from the end of the string
                            paramValue = paramValue.Substring(
                                0, paramValue.Length - 5);
                        }
                        string paramName;
                        if (pathPart.EndsWith(".aspx"))
                        {
                            paramName = pathPart.Substring(
                                1, pathPart.Length - 7);
                        }
                        else
                        {
                            paramName = pathPart.Substring(
                                1, pathPart.Length - 2);
                        }
                        currentUrlParams.Add(paramName, paramValue);
                    }
                    // path
                    else
                    {
                        string paramValue = requestPath[i + 1];
                        if (paramValue.EndsWith(".aspx") 
                            && (i == (requestPath.Length - 2)))
                        {
                            // remove .aspx from the end of the string
                            paramValue = paramValue.Substring(
                                0, paramValue.Length - 5);
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
                    validPagesByVerbAndPath.Add(page);
                    outUrlParameters = new Dictionary<string, string>(currentUrlParams);

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
                    return ValidatePageSecurity(
                        auth, validPagesByVerbAndPath[0]);
                }
                default:
                {
                    log.ErrorFormat("Multiple routes detected '{0}' for request '{1}'",
                        validPagesByVerbAndPath.Select(x => x.Id.ToString() + ":" + x.Url ).Aggregate((res, i) => res + "," + i),
                        path);
                    return (500, null);
                }
            }
        }

    private static bool IsPageValidByVerb(
        AbstractPage page, IHttpContextWrapper context)
    {
            switch (context.Request.HttpMethod)
            {
                case "PUT":
                    return page.AllowPUT;
                case "DELETE":
                    return page.AllowDELETE;
                case "OPTIONS":
                    return false;
                default:
                    return true;
            }
        }

    private static (int, AbstractPage) ValidatePageSecurity(
        IOrigamAuthorizationProvider auth, AbstractPage page)
    {
            try
            {
                return auth.Authorize(SecurityManager.CurrentPrincipal, 
                    page.Roles) ? (200, page) : (403, null);
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
    string ContentType { get;  }
    string AbsoluteUri { get;  }
    Stream InputStream { get;  }
    string HttpMethod { get;  }
    string RawUrl { get;  }
    string Url { get;  }
    string UrlReferrer { get; }
    string UserAgent { get;  }
    string Browser { get;  }
    string BrowserVersion { get;  }
    string UserHostAddress { get;  }
    string UserHostName { get;}
    IEnumerable<string> UserLanguages { get; }
    Encoding ContentEncoding { get;  }
    long ContentLength { get;  }
    IDictionary BrowserCapabilities { get;  }
    string UrlReferrerAbsoluteUri { get;  }
    Parameters Params { get; }
    PostedFile FilesGet(string name);
}

public interface IResponseWrapper
{
    bool BufferOutput {  set; }
    string ContentType {  set; }
    bool TrySkipIisCustomErrors {  set; }
    int StatusCode {  set; }
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
                paramsDictionary.TryGetValue(key, out string value);
                return value;
            }
    }

    public IEnumerable<string> Keys => paramsDictionary.Keys;
}