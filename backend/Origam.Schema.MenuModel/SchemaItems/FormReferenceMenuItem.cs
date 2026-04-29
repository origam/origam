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
using System.Linq;
using System.Xml.Serialization;
using Origam.DA.Common;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Schema.EntityModel.Interfaces;
using Origam.Schema.GuiModel;
using Origam.Workbench.Services;

namespace Origam.Schema.MenuModel;

[SchemaItemDescription(name: "Screen Reference", iconName: "menu_form.png")]
[HelpTopic(topic: "Screen+Menu+Item")]
[ClassMetaVersion(versionStr: "6.0.0")]
public class FormReferenceMenuItem : AbstractMenuItem
{
    public FormReferenceMenuItem() { }

    public FormReferenceMenuItem(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId) { }

    public FormReferenceMenuItem(Key primaryKey)
        : base(primaryKey: primaryKey) { }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(item: Screen);
        if (DefaultSet != null)
        {
            dependencies.Add(item: DefaultSet);
        }
        if (Method != null)
        {
            dependencies.Add(item: Method);
        }
        if (SortSet != null)
        {
            dependencies.Add(item: SortSet);
        }
        if (RecordEditMethod != null)
        {
            dependencies.Add(item: RecordEditMethod);
        }
        if (ListDataStructure != null)
        {
            dependencies.Add(item: ListDataStructure);
        }
        if (ListEntity != null)
        {
            dependencies.Add(item: ListEntity);
        }
        if (ListMethod != null)
        {
            dependencies.Add(item: ListMethod);
        }
        if (ListSortSet != null)
        {
            dependencies.Add(item: ListSortSet);
        }
        if (RuleSet != null)
        {
            dependencies.Add(item: RuleSet);
        }
        if ((DefaultSet != null) || (Method != null) || (SortSet != null))
        {
            dependencies.Add(item: Screen.DataStructure);
        }
        if (SelectionDialogEndRule != null)
        {
            dependencies.Add(item: SelectionDialogEndRule);
        }
        if (SelectionDialogPanel != null)
        {
            dependencies.Add(item: SelectionDialogPanel);
        }
        if (TransformationAfterSelection != null)
        {
            dependencies.Add(item: TransformationAfterSelection);
        }
        if (TransformationBeforeSelection != null)
        {
            dependencies.Add(item: TransformationBeforeSelection);
        }
        if (TemplateSet != null)
        {
            dependencies.Add(item: TemplateSet);
        }
        if (DefaultTemplate != null)
        {
            dependencies.Add(item: DefaultTemplate);
        }
        if (ConfirmationRule != null)
        {
            dependencies.Add(item: ConfirmationRule);
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
    public Guid ScreenId;

    [Category(category: "Screen Reference")]
    [TypeConverter(type: typeof(FormControlSetConverter))]
    [NotNullModelElementRule()]
    [XmlReference(attributeName: "screen", idField: "ScreenId")]
    public FormControlSet Screen
    {
        get =>
            (FormControlSet)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ScreenId)
                );
        set
        {
            if ((value == null) || !ScreenId.Equals(g: (Guid)value.PrimaryKey[key: "Id"]))
            {
                DefaultSet = null;
                DefaultTemplate = null;
                Method = null;
                RecordEditMethod = null;
                RuleSet = null;
                SortSet = null;
                TemplateSet = null;
                DynamicFormLabelEntity = null;
            }
            ScreenId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
        }
    }
    public Guid TemplateSetId;

    [Category(category: "Templates"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [TypeConverter(type: typeof(MenuFormReferenceTemplateSetConverter))]
    [XmlReference(attributeName: "templateSet", idField: "TemplateSetId")]
    public DataStructureTemplateSet TemplateSet
    {
        get =>
            (DataStructureTemplateSet)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: TemplateSetId)
                );
        set
        {
            TemplateSetId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
            DefaultTemplate = null;
        }
    }
    public Guid DefaultTemplateId;

    [Category(category: "Templates")]
    [TypeConverter(type: typeof(MenuFormReferenceTemplateConverter))]
    [XmlReference(attributeName: "defaultTemplate", idField: "DefaultTemplateId")]
    public DataStructureTemplate DefaultTemplate
    {
        get =>
            (DataStructureTemplate)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: DefaultTemplateId)
                );
        set => DefaultTemplateId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid DefaultSetId;

    [Category(category: "Screen Reference")]
    [TypeConverter(type: typeof(MenuFormReferenceDefaultSetConverter))]
    [XmlReference(attributeName: "defaultSet", idField: "DefaultSetId")]
    public DataStructureDefaultSet DefaultSet
    {
        get =>
            (DataStructureDefaultSet)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: DefaultSetId)
                );
        set => DefaultSetId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid MethodId;

    [Category(category: "Data Loading")]
    [TypeConverter(type: typeof(MenuFormReferenceMethodConverter))]
    [NotNullModelElementRule(conditionField: "ListDataStructure")]
    [XmlReference(attributeName: "method", idField: "MethodId")]
    public DataStructureMethod Method
    {
        get =>
            (DataStructureMethod)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: MethodId)
                );
        set => MethodId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid ListDataStructureId;

    [Category(category: "Data Loading"), RefreshProperties(refresh: RefreshProperties.Repaint)]
    [TypeConverter(type: typeof(DataStructureConverter))]
    [XmlReference(attributeName: "listDataStructure", idField: "ListDataStructureId")]
    public DataStructure ListDataStructure
    {
        get =>
            (DataStructure)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ListDataStructureId)
                );
        set
        {
            ListDataStructureId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
            ListMethod = null;
            ListEntity = null;
        }
    }

    public Guid ListMethodId;

    [Category(category: "Data Loading")]
    [TypeConverter(type: typeof(MenuFormReferenceListMethodConverter))]
    [XmlReference(attributeName: "listMethod", idField: "ListMethodId")]
    public DataStructureMethod ListMethod
    {
        get =>
            (DataStructureMethod)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ListMethodId)
                );
        set => ListMethodId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid ListSortSetId;

    [Category(category: "Data Loading")]
    [TypeConverter(type: typeof(MenuFormReferenceListSortSetConverter))]
    [NotNullModelElementRule(conditionField: "ListDataStructure")]
    [XmlReference(attributeName: "listSortSet", idField: "ListSortSetId")]
    public DataStructureSortSet ListSortSet
    {
        get =>
            (DataStructureSortSet)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ListSortSetId)
                );
        set => ListSortSetId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid AutoRefreshIntervalConstantId;

    [TypeConverter(type: typeof(DataConstantConverter))]
    [Category(category: "Data Loading")]
    [Description(
        description: "Specifies delay in seconds after which data will be automatically refreshed."
    )]
    [XmlReference(attributeName: "autoRefreshInterval", idField: "AutoRefreshIntervalConstantId")]
    public DataConstant AutoRefreshInterval
    {
        get =>
            (DataConstant)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(DataConstant),
                    primaryKey: new ModelElementKey(id: AutoRefreshIntervalConstantId)
                );
        set =>
            AutoRefreshIntervalConstantId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    private SaveRefreshType _refreshAfterSaveType = SaveRefreshType.RefreshChangedRecords;

    [Category(category: "Data Loading"), DefaultValue(value: SaveRefreshType.RefreshChangedRecords)]
    [XmlAttribute(attributeName: "refreshAfterSaveType")]
    public SaveRefreshType RefreshAfterSaveType
    {
        get => _refreshAfterSaveType;
        set => _refreshAfterSaveType = value;
    }
    public Guid RecordEditMethodId;

    [Category(category: "Data Loading")]
    [TypeConverter(type: typeof(MenuFormReferenceMethodConverter))]
    [XmlReference(attributeName: "recordEditMethod", idField: "RecordEditMethodId")]
    public DataStructureMethod RecordEditMethod
    {
        get =>
            (DataStructureMethod)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: RecordEditMethodId)
                );
        set =>
            RecordEditMethodId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid ListEntityId;

    [Category(category: "Data Loading")]
    [TypeConverter(type: typeof(MenuFormReferenceListEntityConverter))]
    [XmlReference(attributeName: "listEntity", idField: "ListEntityId")]
    [MergeIgnoreEntityActionsOnlyRule]
    [RequireListEntityIfListDataStructureDefinedRule]
    public DataStructureEntity ListEntity
    {
        get =>
            (DataStructureEntity)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ListEntityId)
                );
        set => ListEntityId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid SelectionPanelId;

    [Category(category: "Selection Dialog")]
    [TypeConverter(type: typeof(PanelControlSetConverter))]
    [XmlReference(attributeName: "selectionDialogPanel", idField: "SelectionPanelId")]
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

    [Category(category: "Screen Reference"), DefaultValue(value: false)]
    [XmlAttribute(attributeName: "readOnlyAccess")]
    public bool ReadOnlyAccess { get; set; } = false;

    [Browsable(browsable: false)]
    public string SelectionChangeEntity => ListEntity?.Name;
    public Guid RuleSetId;

    [Category(category: "Screen Reference")]
    [TypeConverter(type: typeof(MenuFormReferenceRuleSetConverter))]
    [XmlReference(attributeName: "ruleSet", idField: "RuleSetId")]
    public DataStructureRuleSet RuleSet
    {
        get =>
            (DataStructureRuleSet)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: RuleSetId)
                );
        set => RuleSetId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid SortSetId;

    [Category(category: "Data Loading")]
    [TypeConverter(type: typeof(MenuFormReferenceSortSetConverter))]
    [NotNullModelElementRule(conditionField: "ListDataStructure")]
    [XmlReference(attributeName: "sortSet", idField: "SortSetId")]
    public DataStructureSortSet SortSet
    {
        get =>
            (DataStructureSortSet)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: SortSetId)
                );
        set => SortSetId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }
    public Guid ConfirmationRuleId;

    [Category(category: "References")]
    [TypeConverter(type: typeof(EndRuleConverter))]
    [XmlReference(attributeName: "confirmationRule", idField: "ConfirmationRuleId")]
    public IEndRule ConfirmationRule
    {
        get =>
            (IEndRule)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: ConfirmationRuleId)
                );
        set =>
            ConfirmationRuleId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    [Category(category: "Data Loading"), DefaultValue(value: false)]
    [Description(
        description: "If true screen will be refresh each time user selects it. If AutomaticRefreshInterval is set, this value is considered as true regardless the actual value."
    )]
    [XmlAttribute(attributeName: "refreshOnFocus")]
    public bool RefreshOnFocus { get; set; } = false;

    [DefaultValue(value: false)]
    [Description(
        description: "If true and List* properties are set (delayed data loading) user will not be asked if she wants to save records before moving to another. Data will be saved automatically."
    )]
    [XmlAttribute(attributeName: "autoSaveOnListRecordChange")]
    public bool AutoSaveOnListRecordChange { get; set; } = false;

    [DefaultValue(value: false)]
    [Description(
        description: "If true, the client will attempt to request save after each update if there are no errors in data."
    )]
    [XmlAttribute(attributeName: "requestSaveAfterUpdate")]
    public bool RequestSaveAfterUpdate { get; set; } = false;

    [DefaultValue(value: false)]
    [Description(description: "If true, the client will refresh its menu after saving data.")]
    [XmlAttribute(attributeName: "refreshPortalAfterSave")]
    public bool RefreshPortalAfterSave { get; set; } = false;

    public Guid DynamicFormLabelEntityId;

    [Category(category: "Dynamic Form Label")]
    [TypeConverter(type: typeof(MenuFormReferenceDynamicFormLabelEntityConverter))]
    [XmlReference(attributeName: "dynamicFormLabelEntity", idField: "DynamicFormLabelEntityId")]
    public DataStructureEntity DynamicFormLabelEntity
    {
        get =>
            (DataStructureEntity)
                PersistenceProvider.RetrieveInstance(
                    type: typeof(ISchemaItem),
                    primaryKey: new ModelElementKey(id: DynamicFormLabelEntityId)
                );
        set
        {
            DynamicFormLabelEntityId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
            DynamicFormLabelField = null;
        }
    }

    [Category(category: "Dynamic Form Label")]
    [Localizable(isLocalizable: false)]
    [XmlAttribute(attributeName: "dynamicFormLabelField")]
    public string DynamicFormLabelField { get; set; }
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
                    item: this,
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
    public bool IsLazyLoaded => ListDataStructure != null;
    #endregion
}

[AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class MergeIgnoreEntityActionsOnlyRule : AbstractModelElementRuleAttribute
{
    public MergeIgnoreEntityActionsOnlyRule() { }

    public override Exception CheckRule(object instance)
    {
        return new NotSupportedException(
            message: ResourceUtils.GetString(key: "MemberNameRequired")
        );
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (string.IsNullOrEmpty(value: memberName))
        {
            CheckRule(instance: instance);
        }

        if (memberName != "ListEntity")
        {
            throw new Exception(
                message: $"{nameof(MergeIgnoreEntityActionsOnlyRule)} can be only applied to ListEntity property"
            );
        }

        if (instance is not FormReferenceMenuItem menuItem)
        {
            return null;
        }

        if (menuItem.ListEntity == null)
        {
            return null;
        }

        string errorMessages = string.Join(
            separator: "\n",
            values: menuItem
                .ListEntity.Entity.ChildItems.OfType<EntityWorkflowAction>()
                .Select(selector: action => GetErrorOrNull(action: action, menuItem: menuItem))
                .Where(predicate: error => error != null)
        );
        if (string.IsNullOrWhiteSpace(value: errorMessages))
        {
            return null;
        }
        return new Exception(
            message: $"All {nameof(EntityWorkflowAction)}s defined under the ListEntity must have their MergeType se to \"Ignore\"\n{errorMessages}"
        );
    }

    private string GetErrorOrNull(EntityWorkflowAction action, FormReferenceMenuItem menuItem)
    {
        if (
            (menuItem.Screen == null)
            || (action.MergeType == ServiceOutputMethod.Ignore)
            || (action.Mode != PanelActionMode.MultipleCheckboxes)
            || (menuItem.ListEntity.Entity.Id != action.ParentItem.Id)
        )
        {
            return null;
        }

        var screenConditions = action.ChildItems.OfType<ScreenCondition>().ToList();
        bool shouldShowOnScreen = screenConditions.Any(predicate: screenCondition =>
            screenCondition.ScreenId == menuItem.ScreenId
        );
        if (screenConditions.Count > 0 && !shouldShowOnScreen)
        {
            return null;
        }
        var screenSectionIds = menuItem
            .Screen.ChildItems.OfType<ControlSetItem>()
            .Select(selector: x => x.Id)
            .ToList();
        var screenSectionConditions = action.ChildItems.OfType<ScreenSectionCondition>().ToList();
        bool shouldShowOnScreenSection = screenSectionConditions.Any(predicate: screenCondition =>
            screenSectionIds.Contains(item: screenCondition.ScreenSectionId)
        );
        if (screenSectionConditions.Count > 0 && !shouldShowOnScreenSection)
        {
            return null;
        }
        return $"Action {action.Name} ({action.Id}) does not have the MergeType set to \"Ignore\"";
    }
}

[AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class RequireListEntityIfListDataStructureDefinedRule : AbstractModelElementRuleAttribute
{
    public RequireListEntityIfListDataStructureDefinedRule() { }

    public override Exception CheckRule(object instance)
    {
        return new NotSupportedException(
            message: ResourceUtils.GetString(key: "MemberNameRequired")
        );
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (string.IsNullOrEmpty(value: memberName))
        {
            CheckRule(instance: instance);
        }
        if (memberName != "ListEntity")
        {
            throw new Exception(
                message: $"{nameof(RequireListEntityIfListDataStructureDefinedRule)}"
                    + $" can be only applied to ListEntity property"
            );
        }
        if (instance is not FormReferenceMenuItem menuItem)
        {
            return null;
        }
        if (menuItem.ListDataStructure != null && menuItem.ListEntity == null)
        {
            return new Exception(
                message: $"{nameof(FormReferenceMenuItem.ListEntity)} must be "
                    + $"set if {nameof(FormReferenceMenuItem.ListDataStructure)} is set"
            );
        }
        return null;
    }
}
