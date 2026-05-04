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
using Origam.Server.Common;
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
        : base(service: service, request: request, name: name, analytics: analytics)
    {
        this.DataStructureId = dataSourceId;
        this.BeforeTransformationId = beforeTransformationId;
        this.AfterTransformationId = afterTransformationId;
        this.PanelId = panelId;
        this.EndRule = endRule;
    }

    #region Overriden SessionStore Methods
    public override void Init()
    {
        LoadData();
    }

    private void LoadData()
    {
        Hashtable parameters = new Hashtable(d: this.Request.Parameters);
        DataSet data = FormTools.GetSelectionDialogData(
            entityId: this.DataStructureId,
            transformationBeforeId: this.BeforeTransformationId,
            createEmptyRow: true,
            profileId: SecurityTools.CurrentUserProfile().Id,
            parameters: parameters
        );
        SetDataSource(dataSource: data);
    }

    public override object ExecuteActionInternal(string actionId)
    {
        switch (actionId)
        {
            case ACTION_REFRESH:
            {
                return Refresh();
            }
            case ACTION_NEXT:
            {
                return Next();
            }
            default:
            {
                throw new ArgumentOutOfRangeException(
                    paramName: "actionId",
                    actualValue: actionId,
                    message: Resources.ErrorContextUnknownAction
                );
            }
        }
    }

    public override XmlDocument GetFormXml()
    {
        XmlDocument formXml = FormXmlBuilder.GetXmlFromPanel(
            panelId: this.PanelId,
            name: "",
            menuId: new Guid(g: this.Request.ObjectId)
        );
        XmlNodeList list = formXml.SelectNodes(xpath: "/Window");
        XmlElement windowElement = list[i: 0] as XmlElement;
        windowElement.SetAttribute(name: "SuppressDirtyNotification", value: "true");
        windowElement.SetAttribute(name: "SuppressSave", value: "true");
        this.SuppressSave = true;
        windowElement.SetAttribute(name: "SuppressRefresh", value: "true");
        //windowElement.SetAttribute("ShowWorkflowNextButton", "true");
        list = formXml.SelectNodes(xpath: "//Actions");
        XmlElement actionsElement = list[i: 0] as XmlElement;
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
        AsPanelActionButtonBuilder.Build(actionsElement: actionsElement, config: config);
        return formXml;
    }

    public override string Title
    {
        get { return ""; }
        set { base.Title = value; }
    }

    private object Next()
    {
        if (this.EndRule != null)
        {
            RuleExceptionDataCollection ruleExceptions = this.RuleEngine.EvaluateEndRule(
                rule: this.EndRule,
                data: this.XmlData
            );
            if (ruleExceptions != null && ruleExceptions.Count > 0)
            {
                throw new RuleException(result: ruleExceptions);
            }
        }
        switch (this.Request.Type)
        {
            case UIRequestType.FormReferenceMenuItem_WithSelection:
            {
                return this.NextForm();
            }
            case UIRequestType.ReportReferenceMenuItem_WithSelection:
            {
                return this.NextReport();
            }
            default:
            {
                throw new Exception(
                    message: "Not supported by selection dialog: " + this.Request.Type
                );
            }
        }
    }

    private object NextReport()
    {
        UserProfile profile = SecurityTools.CurrentUserProfile();
        PanelActionResult result = new PanelActionResult(type: ActionResultType.OpenUrl);
        result.Request = new UIRequest();
        result.Request.Caption = this.Request.Caption;
        DataRow row = FormTools.GetSelectionDialogResultRow(
            entityId: this.DataStructureId,
            transformationAfterId: this.AfterTransformationId,
            dataDoc: this.XmlData,
            profileId: profile.Id
        );
        IPersistenceService ps =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        ReportReferenceMenuItem item =
            ps.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractMenuItem),
                primaryKey: new ModelElementKey(id: new Guid(g: this.Request.ObjectId))
            ) as ReportReferenceMenuItem;
        Hashtable parameters = new Hashtable();
        SetParameters(parameters: parameters, row: row, item: item);
        result.Url = this.Service.GetReportStandalone(
            reportId: item.ReportId.ToString(),
            parameters: parameters,
            dataReportExportFormatType: item.ExportFormatType
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
        PanelActionResult result = new PanelActionResult(type: ActionResultType.OpenForm);
        UIRequest request = new UIRequest();
        request.Type = UIRequestType.FormReferenceMenuItem;
        request.IsDataOnly = false;
        request.Icon = this.Request.Icon;
        request.Caption = this.Request.Caption;
        request.IsStandalone = this.Request.IsStandalone;
        request.ObjectId = this.Request.ObjectId;
        DataRow row = FormTools.GetSelectionDialogResultRow(
            entityId: this.DataStructureId,
            transformationAfterId: this.AfterTransformationId,
            dataDoc: this.XmlData,
            profileId: profile.Id
        );
        IPersistenceService ps =
            ServiceManager.Services.GetService(serviceType: typeof(IPersistenceService))
            as IPersistenceService;
        ISchemaItem item =
            ps.SchemaProvider.RetrieveInstance(
                type: typeof(AbstractMenuItem),
                primaryKey: new ModelElementKey(id: new Guid(g: this.Request.ObjectId))
            ) as ISchemaItem;
        SetParameters(parameters: request.Parameters, row: row, item: item);
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
                itemType: SelectionDialogParameterMapping.CategoryConst
            )
        )
        {
            object value = row[columnName: mapping.SelectionDialogField.Name];
            DataColumn column = row.Table.Columns[name: mapping.SelectionDialogField.Name];
            if (column.ExtendedProperties.Contains(key: Const.ArrayRelation))
            {
                string childColumnName = (string)
                    column.ExtendedProperties[key: Const.ArrayRelationField];
                var list = new ArrayList();
                foreach (
                    DataRow childRow in row.GetChildRows(
                        relationName: (string)column.ExtendedProperties[key: Const.ArrayRelation]
                    )
                )
                {
                    list.Add(value: childRow[columnName: childColumnName]);
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
            parameters.Add(key: mapping.Name, value: value);
        }
        // pass-through any parameters that were passed to the selection dialog
        foreach (DictionaryEntry entry in this.Request.Parameters)
        {
            parameters.Add(key: entry.Key, value: entry.Value);
        }
    }

    private object Refresh()
    {
        this.Clear();
        LoadData();
        return this.Data;
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
