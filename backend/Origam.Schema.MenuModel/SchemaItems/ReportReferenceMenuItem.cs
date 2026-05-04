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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Schema.MenuModel;

[SchemaItemDescription(name: "Report Reference", iconName: "menu_report.png")]
[HelpTopic(topic: "Report+Menu+Item")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class ReportReferenceMenuItem : AbstractMenuItem
{
    public ReportReferenceMenuItem() { }

    public ReportReferenceMenuItem(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public ReportReferenceMenuItem(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: Report);
        if (SelectionDialogEndRule != null)
        {
            dependencies.Add(item: SelectionDialogEndRule);
        }
        if (SelectionDialogPanel != null)
        {
            dependencies.Add(item: SelectionDialogPanel);
        }
        if (TransformationBeforeSelection != null)
        {
            dependencies.Add(item: TransformationBeforeSelection);
        }
        if (TransformationAfterSelection != null)
        {
            dependencies.Add(item: TransformationAfterSelection);
        }
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override UI.BrowserNodeCollection ChildNodes()
    {
#if ORIGAM_CLIENT
        return new Origam.UI.BrowserNodeCollection();
#else
        return base.ChildNodes();
#endif
    }

    #region Properties
    public Guid ReportId;

    [Category(category: "Report Reference")]
    [TypeConverter(type: typeof(ReportConverter))]
    [XmlReference(attributeName: "report", idField: "ReportId")]
    [NotNullModelElementRule]
    public AbstractReport Report
    {
        get =>
            (AbstractReport)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ReportId)
                );
        set => ReportId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid SelectionPanelId;

    [Category(category: "Selection Dialog")]
    [TypeConverter(type: typeof(PanelControlSetConverter))]
    [XmlReference(attributeName: "selectionDialogScreenSection", idField: "SelectionPanelId")]
    public PanelControlSet SelectionDialogPanel
    {
        get =>
            (PanelControlSet)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: SelectionPanelId)
                );
        set => SelectionPanelId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid SelectionPanelBeforeTransformationId;

    [Category(category: "Selection Dialog")]
    [TypeConverter(type: typeof(TransformationConverter))]
    [XmlReference(
        attributeName: "transformationBeforeSelection",
        idField: "SelectionPanelBeforeTransformationId"
    )]
    public AbstractTransformation TransformationBeforeSelection
    {
        get =>
            (AbstractTransformation)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: SelectionPanelBeforeTransformationId)
                );
        set =>
            SelectionPanelBeforeTransformationId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid SelectionPanelAfterTransformationId;

    [Category(category: "Selection Dialog")]
    [TypeConverter(type: typeof(TransformationConverter))]
    [XmlReference(
        attributeName: "transformationAfterSelection",
        idField: "SelectionPanelAfterTransformationId"
    )]
    public AbstractTransformation TransformationAfterSelection
    {
        get =>
            (AbstractTransformation)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: SelectionPanelAfterTransformationId)
                );
        set =>
            SelectionPanelAfterTransformationId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid SelectionEndRuleId;

    [Category(category: "Selection Dialog")]
    [TypeConverter(type: typeof(EndRuleConverter))]
    [XmlReference(attributeName: "selectionDialogEndRule", idField: "SelectionEndRuleId")]
    public IEndRule SelectionDialogEndRule
    {
        get =>
            (IEndRule)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: SelectionEndRuleId)
                );
        set =>
            SelectionEndRuleId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    private DataReportExportFormatType _exportFormatType;

    [Category(category: "Data Report")]
    [Description(description: "Export Format Type")]
    [XmlAttribute(attributeName: "exportFormatType")]
    public DataReportExportFormatType ExportFormatType
    {
        get => _exportFormatType;
        set => _exportFormatType = value;
    }
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes => new[] { typeof(SelectionDialogParameterMapping) };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        return base.NewItem<T>(
            schemaExtensionId: schemaExtensionId,
            group: group,
            itemName: typeof(T) == typeof(SelectionDialogParameterMapping)
                ? "NewSelectionDialogParameterMapping"
                : null
        );
    }

    public override IList<string> NewTypeNames
    {
        get
        {
            try
            {
                var businessServicesService =
                    ServiceManager.Services.GetService<IBusinessServicesService>();
                var serviceAgent = businessServicesService.GetAgent(
                    serviceType: "DataService",
                    ruleEngine: null,
                    workflowEngine: null
                );
                return serviceAgent.ExpectedParameterNames(
                    item: Report,
                    method: "LoadData",
                    parameter: "Parameters"
                );
            }
            catch
            {
                return new string[] { };
            }
        }
    }
    #endregion
}
