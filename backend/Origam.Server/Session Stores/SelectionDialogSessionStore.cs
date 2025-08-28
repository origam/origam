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

#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM.

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with ORIGAM.  If not, see<http://www.gnu.org/licenses/>.
*/
#endregion

using System;
using System.Collections;
using System.Data;
using System.Xml;
using Origam.DA;
using Origam.Gui;
using Origam.OrigamEngine.ModelXmlBuilders;
using Origam.Schema;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.GuiModel;
using Origam.Schema.MenuModel;
using Origam.Service.Core;
using Origam.Workbench.Services;

namespace Origam.Server;

public class SelectionDialogSessionStore : SessionStore
{
    private Guid _dataStructureId;
    private IEndRule _endRule;

    public SelectionDialogSessionStore(
        IBasicUIService service,
        UIRequest request,
        Guid dataSourceId,
        Guid beforeTransformationId,
        Guid afterTransformationId,
        Guid panelId,
        string name,
        IEndRule endRule,
        Analytics analytics
    )
        : base(service, request, name, analytics)
    {
        DataStructureId = dataSourceId;
        BeforeTransformationId = beforeTransformationId;
        AfterTransformationId = afterTransformationId;
        PanelId = panelId;
        EndRule = endRule;
    }

    #region Overriden SessionStore Methods
    public override void Init()
    {
        LoadData();
    }

    private void LoadData()
    {
        Hashtable parameters = new Hashtable(Request.Parameters);
        DataSet data = FormTools.GetSelectionDialogData(
            DataStructureId,
            BeforeTransformationId,
            true,
            SecurityTools.CurrentUserProfile().Id,
            parameters
        );
        SetDataSource(data);
    }

    public override object ExecuteActionInternal(string actionId)
    {
        switch (actionId)
        {
            case ACTION_REFRESH:
                return Refresh();
            case ACTION_NEXT:
                return Next();
            default:
                throw new ArgumentOutOfRangeException(
                    "actionId",
                    actionId,
                    Resources.ErrorContextUnknownAction
                );
        }
    }

    public override XmlDocument GetFormXml()
    {
        XmlDocument formXml = FormXmlBuilder.GetXmlFromPanel(
            PanelId,
            "",
            new Guid(Request.ObjectId)
        );
        XmlNodeList list = formXml.SelectNodes("/Window");
        XmlElement windowElement = list[0] as XmlElement;
        windowElement.SetAttribute("SuppressDirtyNotification", "true");
        windowElement.SetAttribute("SuppressSave", "true");
        SuppressSave = true;
        windowElement.SetAttribute("SuppressRefresh", "true");
        //windowElement.SetAttribute("ShowWorkflowNextButton", "true");
        list = formXml.SelectNodes("//Actions");
        XmlElement actionsElement = list[0] as XmlElement;
        var config = new ActionConfiguration
        {
            Type = PanelActionType.SelectionDialogAction,
            Mode = PanelActionMode.ActiveRecord,
            Placement = ActionButtonPlacement.Toolbar,
            ActionId = ACTION_NEXT,
            GroupId = "",
            Caption = "OK",
            IconUrl = "",
            IsDefault = true,
        };
        AsPanelActionButtonBuilder.Build(actionsElement, config);
        return formXml;
    }

    public override string Title
    {
        get { return ""; }
        set { base.Title = value; }
    }

    private object Next()
    {
        if (EndRule != null)
        {
            RuleExceptionDataCollection ruleExceptions = RuleEngine.EvaluateEndRule(
                EndRule,
                XmlData
            );
            if (ruleExceptions != null && ruleExceptions.Count > 0)
            {
                throw new RuleException(ruleExceptions);
            }
        }
        switch (Request.Type)
        {
            case UIRequestType.FormReferenceMenuItem_WithSelection:
                return NextForm();
            case UIRequestType.ReportReferenceMenuItem_WithSelection:
                return NextReport();
            default:
                throw new Exception("Not supported by selection dialog: " + Request.Type);
        }
    }

    private object NextReport()
    {
        UserProfile profile = SecurityTools.CurrentUserProfile();
        PanelActionResult result = new PanelActionResult(ActionResultType.OpenUrl);
        result.Request = new UIRequest();
        result.Request.Caption = Request.Caption;
        DataRow row = FormTools.GetSelectionDialogResultRow(
            DataStructureId,
            AfterTransformationId,
            XmlData,
            profile.Id
        );
        IPersistenceService ps =
            ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
        ReportReferenceMenuItem item =
            ps.SchemaProvider.RetrieveInstance(
                typeof(AbstractMenuItem),
                new ModelElementKey(new Guid(Request.ObjectId))
            ) as ReportReferenceMenuItem;
        Hashtable parameters = new Hashtable();
        SetParameters(parameters, row, item);
        result.Url = Service.GetReportStandalone(
            item.ReportId.ToString(),
            parameters,
            item.ExportFormatType
        );
        WebReport wr = item.Report as WebReport;
        if (wr != null)
        {
            result.UrlOpenMethod = wr.OpenMethod.ToString();
        }
        return result;
    }

    private object NextForm()
    {
        UserProfile profile = SecurityTools.CurrentUserProfile();
        PanelActionResult result = new PanelActionResult(ActionResultType.OpenForm);
        UIRequest request = new UIRequest();
        request.Type = UIRequestType.FormReferenceMenuItem;
        request.IsDataOnly = false;
        request.Icon = Request.Icon;
        request.Caption = Request.Caption;
        request.IsStandalone = Request.IsStandalone;
        request.ObjectId = Request.ObjectId;
        DataRow row = FormTools.GetSelectionDialogResultRow(
            DataStructureId,
            AfterTransformationId,
            XmlData,
            profile.Id
        );
        IPersistenceService ps =
            ServiceManager.Services.GetService(typeof(IPersistenceService)) as IPersistenceService;
        ISchemaItem item =
            ps.SchemaProvider.RetrieveInstance(
                typeof(AbstractMenuItem),
                new ModelElementKey(new Guid(Request.ObjectId))
            ) as ISchemaItem;
        SetParameters(request.Parameters, row, item);
        FormReferenceMenuItem formRef = item as FormReferenceMenuItem;
        if (formRef != null)
        {
            // Request data for normal (non-lazily-loaded) screens
            request.DataRequested = !formRef.IsLazyLoaded;
        }
        result.Request = request;
        return result;
    }

    private void SetParameters(IDictionary parameters, DataRow row, ISchemaItem item)
    {
        // map the parameters from the selection dialog data row
        foreach (
            var mapping in item.ChildItemsByType<SelectionDialogParameterMapping>(
                SelectionDialogParameterMapping.CategoryConst
            )
        )
        {
            object value = row[mapping.SelectionDialogField.Name];
            DataColumn column = row.Table.Columns[mapping.SelectionDialogField.Name];
            if (column.ExtendedProperties.Contains(Const.ArrayRelation))
            {
                string childColumnName = (string)
                    column.ExtendedProperties[Const.ArrayRelationField];
                var list = new ArrayList();
                foreach (
                    DataRow childRow in row.GetChildRows(
                        (string)column.ExtendedProperties[Const.ArrayRelation]
                    )
                )
                {
                    list.Add(childRow[childColumnName]);
                }
                if (list.Count > 0)
                {
                    value = list;
                }
                else
                {
                    value = null;
                }
            }
            parameters.Add(mapping.Name, value);
        }
        // pass-through any parameters that were passed to the selection dialog
        foreach (DictionaryEntry entry in Request.Parameters)
        {
            parameters.Add(entry.Key, entry.Value);
        }
    }

    private object Refresh()
    {
        Clear();
        LoadData();
        return Data;
    }
    #endregion
    #region Properties
    public Guid DataStructureId
    {
        get { return _dataStructureId; }
        set { _dataStructureId = value; }
    }
    private Guid _panelId;
    public Guid PanelId
    {
        get { return _panelId; }
        set { _panelId = value; }
    }
    private Guid _beforeTransformationId;
    public Guid BeforeTransformationId
    {
        get { return _beforeTransformationId; }
        set { _beforeTransformationId = value; }
    }
    private Guid _afterTransformationId;
    public Guid AfterTransformationId
    {
        get { return _afterTransformationId; }
        set { _afterTransformationId = value; }
    }
    public IEndRule EndRule
    {
        get { return _endRule; }
        set { _endRule = value; }
    }
    #endregion
}
