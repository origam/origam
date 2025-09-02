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
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Origam.DA;
using Origam.Extensions;
using Origam.Gui;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Schema;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.MenuModel;
using Origam.Schema.WorkflowModel;
using Origam.Service.Core;
using Origam.Workbench.Services;
using Origam.Workflow;
using CoreServices = Origam.Workbench.Services.CoreServices;

namespace Origam.Server;

public class WorkflowSessionStore : SaveableSessionStore
{
    private bool _disposeAfterAction;
    private WorkflowHost _actualHost;
    private WorkflowHost _host;
    private Guid _taskId;
    private string _stepDescription;
    private IEndRule _endRule;
    private Guid _workflowId;
    private Guid _workflowInstanceId;
    private string _finishMessage;
    private AbstractDataStructure _dataStructure;
    private DataStructureMethod _refreshMethod;
    private Hashtable _parameters = new Hashtable();
    private bool _allowSave;
    private bool _isFinalForm;
    private bool _isAutoNext;
    private bool _isFirstSaveDone;

    public WorkflowSessionStore(
        IBasicUIService service,
        UIRequest request,
        Guid workflowId,
        string name,
        Analytics analytics
    )
        : base(service, request, name, analytics)
    {
        WorkflowId = workflowId;
    }

    #region Overriden SessionStore Methods
    public override bool HasChanges()
    {
        return AllowSave && Data != null && Data.HasChanges();
    }

    public override void Init()
    {
        IPersistenceService ps =
            ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
        IWorkflow workflow =
            ps.SchemaProvider.RetrieveInstance(typeof(ISchemaItem), new ModelElementKey(WorkflowId))
            as IWorkflow;
        WorkflowEngine engine = WorkflowEngine.PrepareWorkflow(
            workflow,
            new Hashtable(Request.Parameters),
            false,
            Request.Caption
        );
        _host = new WorkflowHost();
        _host.SupportsUI = true;
        WorkflowCallbackHandler handler = new WorkflowCallbackHandler(
            _host,
            engine.WorkflowInstanceId
        );
        handler.Subscribe();
        _host.ExecuteWorkflow(engine);
        handler.Event.WaitOne();
        HandleWorkflow(handler);
    }

    public override object ExecuteActionInternal(string actionId)
    {
        object result;
        switch (actionId)
        {
            case ACTION_QUERYNEXT:
            {
                result = EvaluateEndRule();
                break;
            }

            case ACTION_NEXT:
            {
                result = HandleWorkflowNextAsync().Result;
                break;
            }

            case ACTION_ABORT:
            {
                result = HandleAbortAsync().Result;
                break;
            }

            case ACTION_SAVE:
            {
                result = Save();
                IsFirstSaveDone = true;
                break;
            }

            case ACTION_REFRESH:
            {
                result = HandleRefresh();
                break;
            }

            default:
                throw new ArgumentOutOfRangeException(
                    "actionId",
                    actionId,
                    Resources.ErrorContextUnknownAction
                );
        }
        if (_disposeAfterAction)
        {
            Dispose();
        }
        return result;
    }

    public override XmlDocument GetFormXml()
    {
        XmlDocument formXml;
        formXml = FormXmlBuilder.GetXml(
            FormId,
            Title,
            true,
            new Guid(Request.ObjectId),
            FinishMessage,
            (DataStructure == null ? Guid.Empty : DataStructure.Id),
            false,
            ""
        );
        XmlNodeList list = formXml.SelectNodes("/Window");
        XmlElement windowElement = list[0] as XmlElement;
        XmlElement uiRootElement = windowElement.SelectSingleNode("UIRoot") as XmlElement;
        if (RefreshMethod == null)
        {
            windowElement.SetAttribute("SuppressRefresh", "true");
        }
        if (IsAutoNext)
        {
            windowElement.SetAttribute("AutoWorkflowNext", "true");
        }
        if (!AllowSave)
        {
            windowElement.SetAttribute("SuppressSave", "true");
            // we MUST not turn on this.SupressSave here because that would
            // automatically accept all changes in workflow screens, resulting
            // in data not being saved by the workflow
            windowElement.SetAttribute("SuppressDirtyNotification", "true");
        }
        windowElement.SetAttribute("AskWorkflowClose", XmlConvert.ToString(AskWorkflowClose));
        if (IsFinalForm)
        {
            if (Request.Parameters.Count > 0)
            {
                uiRootElement.SetAttribute("showWorkflowRepeatButton", "false");
            }
        }
        else
        {
            windowElement.SetAttribute("ShowWorkflowNextButton", "true");
            windowElement.SetAttribute("ShowWorkflowCancelButton", "true");
        }
        return formXml;
    }

    private DataSet LoadData()
    {
        DataSet data;
        QueryParameterCollection qparams = new QueryParameterCollection();
        foreach (DictionaryEntry entry in Parameters)
        {
            qparams.Add(new QueryParameter((string)entry.Key, entry.Value));
        }
        Guid methodId = Guid.Empty;
        if (_refreshMethod != null)
        {
            methodId = _refreshMethod.Id;
        }

        Guid sortSetId = Guid.Empty;
        if (SortSet != null)
        {
            sortSetId = SortSet.Id;
        }

        data = CoreServices.DataService.Instance.LoadData(
            _dataStructure.Id,
            methodId,
            Guid.Empty,
            sortSetId,
            null,
            qparams
        );
        return data;
    }

    public override void OnDispose()
    {
        if (TaskId != Guid.Empty)
        {
            ExecuteAction(ACTION_ABORT);
        }
        _host.Dispose();
        base.OnDispose();
    }

    public override string Title
    {
        get
        {
            if (IsModalDialog && IsFinalForm)
            {
                return "";
            }

            return base.Title;
        }
        set { base.Title = value; }
    }
    #endregion
    #region Properties
    public WorkflowHost Host
    {
        get { return _actualHost; }
        set { _actualHost = value; }
    }
    public string StepDescription
    {
        get { return _stepDescription; }
        set
        {
            _stepDescription = value;
            Title = value;
        }
    }
    public IEndRule EndRule
    {
        get { return _endRule; }
        set { _endRule = value; }
    }
    public new AbstractDataStructure DataStructure
    {
        get { return _dataStructure; }
        set { _dataStructure = value; }
    }
    public DataStructureMethod RefreshMethod
    {
        get { return _refreshMethod; }
        set { _refreshMethod = value; }
    }
    public Hashtable Parameters
    {
        get { return _parameters; }
        set { _parameters = value; }
    }
    public Guid TaskId
    {
        get { return _taskId; }
        set { _taskId = value; }
    }
    public Guid WorkflowId
    {
        get { return _workflowId; }
        set { _workflowId = value; }
    }
    public Guid WorkflowInstanceId
    {
        get { return _workflowInstanceId; }
        set { _workflowInstanceId = value; }
    }
    public string FinishMessage
    {
        get { return _finishMessage; }
        set { _finishMessage = value; }
    }
    public bool IsFirstSaveDone
    {
        get { return _isFirstSaveDone; }
        set { _isFirstSaveDone = value; }
    }
    public bool IsFinalForm
    {
        get { return _isFinalForm; }
        set { _isFinalForm = value; }
    }
    public bool IsAutoNext
    {
        get { return _isAutoNext; }
        set { _isAutoNext = value; }
    }
    public bool AllowSave
    {
        get { return _allowSave; }
        set { _allowSave = value; }
    }
    public bool AskWorkflowClose
    {
        get { return !IsFinalForm; }
    }
    #endregion
    #region Private Methods
    private void HandleWorkflow(WorkflowCallbackHandler handler)
    {
        WorkflowHostFormEventArgs formArgs = handler.Result as WorkflowHostFormEventArgs;
        WorkflowHostMessageEventArgs messageArgs = handler.Result as WorkflowHostMessageEventArgs;
        if (handler.Result.Exception != null)
        {
            SetDataSource(null);
            FormId = new Guid(FormXmlBuilder.WORKFLOW_FINISHED_FORMID);
            RuleSet = null;
            StepDescription = Resources.WorkflowErrorTitle;
            EndRule = null;
            DataStructure = null;
            DataStructureId = Guid.Empty;
            SortSet = null;
            RefreshMethod = null;
            Parameters = new Hashtable();
            Host = handler.Result.Engine.Host;
            TaskId = Guid.Empty;
            WorkflowInstanceId = handler.Result.Engine.WorkflowInstanceId;
            IsFinalForm = true;
            AllowSave = false;
            IsAutoNext = false;
            ConfirmationRule = null;
            Notifications.Clear();
            RefreshPortalAfterSave = false;
            // get the messages in reverse order
            Exception ex = handler.Result.Exception;
            if (ex != null)
            {
                StringBuilder message = new StringBuilder();
                message.Append("<HTML><BODY>");
                message.Append("<FONT COLOR=\"#FF0000\" SIZE=\"16\"><DIV>");
                message.Append(ex.Message);
                message.Append("</DIV></FONT>");
                message.Append("</HTML></BODY>");
                FinishMessage = message.ToString();
            }
        }
        else if (formArgs != null)
        {
            SetDataSource(formArgs.Data);
            FormId = formArgs.Form.Id;
            RuleSet = formArgs.RuleSet;
            StepDescription = formArgs.Description;
            EndRule = formArgs.EndRule;
            DataStructure = formArgs.DataStructure;
            SortSet = formArgs.RefreshSort;
            RefreshMethod = formArgs.RefreshMethod;
            DataStructureId = (
                formArgs.SaveDataStructure == null
                    ? formArgs.DataStructure.Id
                    : formArgs.SaveDataStructure.Id
            );
            Parameters = formArgs.Parameters;
            Host = formArgs.Engine.Host;
            TaskId = formArgs.TaskId;
            WorkflowInstanceId = formArgs.Engine.WorkflowInstanceId;
            IsFinalForm = formArgs.IsFinalForm;
            AllowSave = formArgs.AllowSave;
            IsAutoNext = formArgs.IsAutoNext;
            IsFirstSaveDone = !formArgs.IsRefreshSuppressedBeforeFirstSave;
            ConfirmationRule = formArgs.SaveConfirmationRule;
            RefreshPortalAfterSave = formArgs.RefreshPortalAfterSave;
            Notifications.Clear();
            if (!string.IsNullOrWhiteSpace(formArgs.Notification))
            {
                ParseNotifications(formArgs.Notification);
            }
        }
        else
        {
            // not form, no exception - workflow is finished
            SetDataSource(null);
            FormId = new Guid(FormXmlBuilder.WORKFLOW_FINISHED_FORMID);
            RuleSet = null;
            StepDescription = Resources.WorkflowFinishedTitle;
            EndRule = null;
            DataStructure = null;
            DataStructureId = Guid.Empty;
            SortSet = null;
            RefreshMethod = null;
            Parameters = new Hashtable();
            Host = handler.Result.Engine.Host;
            TaskId = Guid.Empty;
            WorkflowInstanceId = handler.Result.Engine.WorkflowInstanceId;
            FinishMessage = (
                handler.Result.Engine.ResultMessage == ""
                    ? Resources.WorkflowFinished
                    : handler.Result.Engine.ResultMessage
            );
            IsFinalForm = true;
            AllowSave = false;
            IsAutoNext = false;
            ConfirmationRule = null;
            Clear();
        }
    }

    private void ParseNotifications(string notifications)
    {
        foreach (string notification in notifications.Split("\n".ToCharArray()))
        {
            FormNotification fn = new FormNotification();
            if (notification.StartsWith("! "))
            {
                fn.Icon = "warning";
                fn.Text = notification.Substring(2);
            }
            else if (notification.StartsWith("!! "))
            {
                fn.Icon = "error";
                fn.Text = notification.Substring(3);
            }
            else
            {
                fn.Icon = "info";
                fn.Text = notification;
            }
            Notifications.Add(fn);
        }
    }

    private RuleExceptionDataCollection EvaluateEndRule()
    {
        if (Data.HasErrors)
        {
            if (log.IsDebugEnabled)
            {
                log.Debug(Resources.ErrorInForm);
                log.Debug(DebugClass.ListRowErrors(Data));
            }
            throw new Exception(Resources.ErrorInForm);
        }
        // check end rule
        if (EndRule != null)
        {
            if (XmlData == null)
            {
                throw new NullReferenceException("Workflow context does not contain valid data.");
            }
            return RuleEngine.EvaluateEndRule(EndRule, XmlData, Parameters);
        }
        return new RuleExceptionDataCollection();
    }

    private async Task<UIResult> HandleWorkflowNextAsync()
    {
        XmlData.DataSet.ReEnableNullConstraints();
        RuleExceptionDataCollection results = EvaluateEndRule();
        if (results != null)
        {
            bool isError = false;
            foreach (RuleExceptionData rule in results)
            {
                if (rule.Severity == RuleExceptionSeverity.High)
                {
                    isError = true;
                    break;
                }
            }
            if (isError)
            {
                throw new RuleException(results);
            }
        }
        WorkflowCallbackHandler handler = new WorkflowCallbackHandler(Host, WorkflowInstanceId);
        handler.Subscribe();
        Host.FinishWorkflowForm(TaskId, XmlData);
        handler.Event.WaitOne();
        HandleWorkflow(handler);
        UIRequest request = GetRequest();
        UIResult result = Service.InitUI(request);
        result.WorkflowTaskId = TaskId.ToString();
        await Task.CompletedTask; //CS1998
        return result;
    }

    private object HandleRefresh()
    {
        if (!IsFirstSaveDone)
        {
            throw new UserOrigamException(Resources.ErrorCannotRefreshFormNotSaved);
        }

        DataSet data = LoadData();
        SetDataSource(data);
        return data;
    }

    private async Task<UIResult> HandleAbortAsync()
    {
        UserProfile profile = SecurityTools.CurrentUserProfile();
        IPersistenceService ps =
            ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
        AbstractMenuItem menuItem =
            ps.SchemaProvider.RetrieveInstance(
                typeof(AbstractMenuItem),
                new ModelElementKey(new Guid(Request.ObjectId))
            ) as AbstractMenuItem;
        // abort workflow
        WorkflowCallbackHandler handler = new WorkflowCallbackHandler(Host, WorkflowInstanceId);
        handler.Subscribe();
        Host.AbortWorkflowForm(TaskId);
        handler.Event.WaitOne();
        HandleWorkflow(handler);
        await Task.CompletedTask; //CS1998
        // call InitUI
        UIRequest request = GetRequest();
        return Service.InitUI(request);
    }

    private UIRequest GetRequest()
    {
        bool die = (FormId == new Guid(FormXmlBuilder.WORKFLOW_FINISHED_FORMID));
        SessionStore ss;
        if (die && ParentSession != null)
        {
            ss = ParentSession;
            ParentSession.ChildSessions.Remove(this);
            ParentSession.ActiveSession = null;
            _disposeAfterAction = true;
        }
        else
        {
            ss = this;
        }
        UIRequest request = new UIRequest();
        request.FormSessionId = ss.Id.ToString();
        request.IsStandalone = ss.Request.IsStandalone;
        request.ObjectId = ss.Request.ObjectId;
        request.IsNewSession = false;
        request.IsDataOnly = false;
        return request;
    }
    #endregion
}
