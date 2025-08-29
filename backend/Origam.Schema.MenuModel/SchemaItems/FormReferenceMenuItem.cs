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

[SchemaItemDescription("Screen Reference", "menu_form.png")]
[HelpTopic("Screen+Menu+Item")]
[ClassMetaVersion("6.0.0")]
public class FormReferenceMenuItem : AbstractMenuItem
{
    public FormReferenceMenuItem() { }

    public FormReferenceMenuItem(Guid schemaExtensionId)
        : base(schemaExtensionId) { }

    public FormReferenceMenuItem(Key primaryKey)
        : base(primaryKey) { }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        dependencies.Add(Screen);
        if (DefaultSet != null)
        {
            dependencies.Add(DefaultSet);
        }
        if (Method != null)
        {
            dependencies.Add(Method);
        }
        if (SortSet != null)
        {
            dependencies.Add(SortSet);
        }
        if (RecordEditMethod != null)
        {
            dependencies.Add(RecordEditMethod);
        }
        if (ListDataStructure != null)
        {
            dependencies.Add(ListDataStructure);
        }
        if (ListEntity != null)
        {
            dependencies.Add(ListEntity);
        }
        if (ListMethod != null)
        {
            dependencies.Add(ListMethod);
        }
        if (ListSortSet != null)
        {
            dependencies.Add(ListSortSet);
        }
        if (RuleSet != null)
        {
            dependencies.Add(RuleSet);
        }
        if ((DefaultSet != null) || (Method != null) || (SortSet != null))
        {
            dependencies.Add(Screen.DataStructure);
        }
        if (SelectionDialogEndRule != null)
        {
            dependencies.Add(SelectionDialogEndRule);
        }
        if (SelectionDialogPanel != null)
        {
            dependencies.Add(SelectionDialogPanel);
        }
        if (TransformationAfterSelection != null)
        {
            dependencies.Add(TransformationAfterSelection);
        }
        if (TransformationBeforeSelection != null)
        {
            dependencies.Add(TransformationBeforeSelection);
        }
        if (TemplateSet != null)
        {
            dependencies.Add(TemplateSet);
        }
        if (DefaultTemplate != null)
        {
            dependencies.Add(DefaultTemplate);
        }
        if (ConfirmationRule != null)
        {
            dependencies.Add(ConfirmationRule);
        }
        base.GetExtraDependencies(dependencies);
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

    [Category("Screen Reference")]
    [TypeConverter(typeof(FormControlSetConverter))]
    [NotNullModelElementRule()]
    [XmlReference("screen", "ScreenId")]
    public FormControlSet Screen
    {
        get =>
            (FormControlSet)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(ScreenId)
                );
        set
        {
            if ((value == null) || !ScreenId.Equals((Guid)value.PrimaryKey["Id"]))
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
            ScreenId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
        }
    }
    public Guid TemplateSetId;

    [Category("Templates"), RefreshProperties(RefreshProperties.Repaint)]
    [TypeConverter(typeof(MenuFormReferenceTemplateSetConverter))]
    [XmlReference("templateSet", "TemplateSetId")]
    public DataStructureTemplateSet TemplateSet
    {
        get =>
            (DataStructureTemplateSet)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(TemplateSetId)
                );
        set
        {
            TemplateSetId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
            DefaultTemplate = null;
        }
    }
    public Guid DefaultTemplateId;

    [Category("Templates")]
    [TypeConverter(typeof(MenuFormReferenceTemplateConverter))]
    [XmlReference("defaultTemplate", "DefaultTemplateId")]
    public DataStructureTemplate DefaultTemplate
    {
        get =>
            (DataStructureTemplate)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(DefaultTemplateId)
                );
        set => DefaultTemplateId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    public Guid DefaultSetId;

    [Category("Screen Reference")]
    [TypeConverter(typeof(MenuFormReferenceDefaultSetConverter))]
    [XmlReference("defaultSet", "DefaultSetId")]
    public DataStructureDefaultSet DefaultSet
    {
        get =>
            (DataStructureDefaultSet)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(DefaultSetId)
                );
        set => DefaultSetId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    public Guid MethodId;

    [Category("Data Loading")]
    [TypeConverter(typeof(MenuFormReferenceMethodConverter))]
    [NotNullModelElementRule("ListDataStructure")]
    [XmlReference("method", "MethodId")]
    public DataStructureMethod Method
    {
        get =>
            (DataStructureMethod)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(MethodId)
                );
        set => MethodId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    public Guid ListDataStructureId;

    [Category("Data Loading"), RefreshProperties(RefreshProperties.Repaint)]
    [TypeConverter(typeof(DataStructureConverter))]
    [XmlReference("listDataStructure", "ListDataStructureId")]
    public DataStructure ListDataStructure
    {
        get =>
            (DataStructure)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(ListDataStructureId)
                );
        set
        {
            ListDataStructureId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
            ListMethod = null;
            ListEntity = null;
        }
    }

    public Guid ListMethodId;

    [Category("Data Loading")]
    [TypeConverter(typeof(MenuFormReferenceListMethodConverter))]
    [XmlReference("listMethod", "ListMethodId")]
    public DataStructureMethod ListMethod
    {
        get =>
            (DataStructureMethod)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(ListMethodId)
                );
        set => ListMethodId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }
    public Guid ListSortSetId;

    [Category("Data Loading")]
    [TypeConverter(typeof(MenuFormReferenceListSortSetConverter))]
    [NotNullModelElementRule("ListDataStructure")]
    [XmlReference("listSortSet", "ListSortSetId")]
    public DataStructureSortSet ListSortSet
    {
        get =>
            (DataStructureSortSet)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(ListSortSetId)
                );
        set => ListSortSetId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }
    public Guid AutoRefreshIntervalConstantId;

    [TypeConverter(typeof(DataConstantConverter))]
    [Category("Data Loading")]
    [Description("Specifies delay in seconds after which data will be automatically refreshed.")]
    [XmlReference("autoRefreshInterval", "AutoRefreshIntervalConstantId")]
    public DataConstant AutoRefreshInterval
    {
        get =>
            (DataConstant)
                PersistenceProvider.RetrieveInstance(
                    typeof(DataConstant),
                    new ModelElementKey(AutoRefreshIntervalConstantId)
                );
        set =>
            AutoRefreshIntervalConstantId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }
    private SaveRefreshType _refreshAfterSaveType = SaveRefreshType.RefreshChangedRecords;

    [Category("Data Loading"), DefaultValue(SaveRefreshType.RefreshChangedRecords)]
    [XmlAttribute("refreshAfterSaveType")]
    public SaveRefreshType RefreshAfterSaveType
    {
        get => _refreshAfterSaveType;
        set => _refreshAfterSaveType = value;
    }
    public Guid RecordEditMethodId;

    [Category("Data Loading")]
    [TypeConverter(typeof(MenuFormReferenceMethodConverter))]
    [XmlReference("recordEditMethod", "RecordEditMethodId")]
    public DataStructureMethod RecordEditMethod
    {
        get =>
            (DataStructureMethod)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(RecordEditMethodId)
                );
        set => RecordEditMethodId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }
    public Guid ListEntityId;

    [Category("Data Loading")]
    [TypeConverter(typeof(MenuFormReferenceListEntityConverter))]
    [XmlReference("listEntity", "ListEntityId")]
    [MergeIgnoreEntityActionsOnlyRule]
    [RequireListEntityIfListDataStructureDefinedRule]
    public DataStructureEntity ListEntity
    {
        get =>
            (DataStructureEntity)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(ListEntityId)
                );
        set => ListEntityId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }
    public Guid SelectionPanelId;

    [Category("Selection Dialog")]
    [TypeConverter(typeof(PanelControlSetConverter))]
    [XmlReference("selectionDialogPanel", "SelectionPanelId")]
    public PanelControlSet SelectionDialogPanel
    {
        get =>
            (PanelControlSet)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(SelectionPanelId)
                );
        set => SelectionPanelId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }
    public Guid SelectionPanelBeforeTransformationId;

    [Category("Selection Dialog")]
    [TypeConverter(typeof(TransformationConverter))]
    [XmlReference("transformationBeforeSelection", "SelectionPanelBeforeTransformationId")]
    public AbstractTransformation TransformationBeforeSelection
    {
        get =>
            (AbstractTransformation)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(SelectionPanelBeforeTransformationId)
                );
        set =>
            SelectionPanelBeforeTransformationId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }
    public Guid SelectionPanelAfterTransformationId;

    [Category("Selection Dialog")]
    [TypeConverter(typeof(TransformationConverter))]
    [XmlReference("transformationAfterSelection", "SelectionPanelAfterTransformationId")]
    public AbstractTransformation TransformationAfterSelection
    {
        get =>
            (AbstractTransformation)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(SelectionPanelAfterTransformationId)
                );
        set =>
            SelectionPanelAfterTransformationId =
                (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    public Guid SelectionEndRuleId;

    [Category("Selection Dialog")]
    [TypeConverter(typeof(EndRuleConverter))]
    [XmlReference("selectionDialogEndRule", "SelectionEndRuleId")]
    public IEndRule SelectionDialogEndRule
    {
        get =>
            (IEndRule)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(SelectionEndRuleId)
                );
        set => SelectionEndRuleId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    [Category("Screen Reference"), DefaultValue(false)]
    [XmlAttribute("readOnlyAccess")]
    public bool ReadOnlyAccess { get; set; } = false;

    [Browsable(false)]
    public string SelectionChangeEntity => ListEntity?.Name;
    public Guid RuleSetId;

    [Category("Screen Reference")]
    [TypeConverter(typeof(MenuFormReferenceRuleSetConverter))]
    [XmlReference("ruleSet", "RuleSetId")]
    public DataStructureRuleSet RuleSet
    {
        get =>
            (DataStructureRuleSet)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(RuleSetId)
                );
        set => RuleSetId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }
    public Guid SortSetId;

    [Category("Data Loading")]
    [TypeConverter(typeof(MenuFormReferenceSortSetConverter))]
    [NotNullModelElementRule("ListDataStructure")]
    [XmlReference("sortSet", "SortSetId")]
    public DataStructureSortSet SortSet
    {
        get =>
            (DataStructureSortSet)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(SortSetId)
                );
        set => SortSetId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }
    public Guid ConfirmationRuleId;

    [Category("References")]
    [TypeConverter(typeof(EndRuleConverter))]
    [XmlReference("confirmationRule", "ConfirmationRuleId")]
    public IEndRule ConfirmationRule
    {
        get =>
            (IEndRule)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(ConfirmationRuleId)
                );
        set => ConfirmationRuleId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    [Category("Data Loading"), DefaultValue(false)]
    [Description(
        "If true screen will be refresh each time user selects it. If AutomaticRefreshInterval is set, this value is considered as true regardless the actual value."
    )]
    [XmlAttribute("refreshOnFocus")]
    public bool RefreshOnFocus { get; set; } = false;

    [DefaultValue(false)]
    [Description(
        "If true and List* properties are set (delayed data loading) user will not be asked if she wants to save records before moving to another. Data will be saved automatically."
    )]
    [XmlAttribute("autoSaveOnListRecordChange")]
    public bool AutoSaveOnListRecordChange { get; set; } = false;

    [DefaultValue(false)]
    [Description(
        "If true, the client will attempt to request save after each update if there are no errors in data."
    )]
    [XmlAttribute("requestSaveAfterUpdate")]
    public bool RequestSaveAfterUpdate { get; set; } = false;

    [DefaultValue(false)]
    [Description("If true, the client will refresh its menu after saving data.")]
    [XmlAttribute("refreshPortalAfterSave")]
    public bool RefreshPortalAfterSave { get; set; } = false;

    public Guid DynamicFormLabelEntityId;

    [Category("Dynamic Form Label")]
    [TypeConverter(typeof(MenuFormReferenceDynamicFormLabelEntityConverter))]
    [XmlReference("dynamicFormLabelEntity", "DynamicFormLabelEntityId")]
    public DataStructureEntity DynamicFormLabelEntity
    {
        get =>
            (DataStructureEntity)
                PersistenceProvider.RetrieveInstance(
                    typeof(ISchemaItem),
                    new ModelElementKey(DynamicFormLabelEntityId)
                );
        set
        {
            DynamicFormLabelEntityId = (value == null) ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
            DynamicFormLabelField = null;
        }
    }

    [Category("Dynamic Form Label")]
    [Localizable(false)]
    [XmlAttribute("dynamicFormLabelField")]
    public string DynamicFormLabelField { get; set; }
    #endregion
    #region ISchemaItemFactory Members
    public override Type[] NewItemTypes => new[] { typeof(SelectionDialogParameterMapping) };

    public override T NewItem<T>(Guid schemaExtensionId, SchemaItemGroup group)
    {
        return base.NewItem<T>(
            schemaExtensionId,
            group,
            typeof(T) == typeof(SelectionDialogParameterMapping)
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
                var serviceAgent = businessServicesService.GetAgent("DataService", null, null);
                return serviceAgent.ExpectedParameterNames(this, "LoadData", "Parameters");
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

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class MergeIgnoreEntityActionsOnlyRule : AbstractModelElementRuleAttribute
{
    public MergeIgnoreEntityActionsOnlyRule() { }

    public override Exception CheckRule(object instance)
    {
        return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (string.IsNullOrEmpty(memberName))
        {
            CheckRule(instance);
        }

        if (memberName != "ListEntity")
        {
            throw new Exception(
                $"{nameof(MergeIgnoreEntityActionsOnlyRule)} can be only applied to ListEntity property"
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
            "\n",
            menuItem
                .ListEntity.Entity.ChildItems.OfType<EntityWorkflowAction>()
                .Select(action => GetErrorOrNull(action, menuItem))
                .Where(error => error != null)
        );
        if (string.IsNullOrWhiteSpace(errorMessages))
        {
            return null;
        }
        return new Exception(
            $"All {nameof(EntityWorkflowAction)}s defined under the ListEntity must have their MergeType se to \"Ignore\"\n{errorMessages}"
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
        bool shouldShowOnScreen = screenConditions.Any(screenCondition =>
            screenCondition.ScreenId == menuItem.ScreenId
        );
        if (screenConditions.Count > 0 && !shouldShowOnScreen)
        {
            return null;
        }
        var screenSectionIds = menuItem
            .Screen.ChildItems.OfType<ControlSetItem>()
            .Select(x => x.Id)
            .ToList();
        var screenSectionConditions = action.ChildItems.OfType<ScreenSectionCondition>().ToList();
        bool shouldShowOnScreenSection = screenSectionConditions.Any(screenCondition =>
            screenSectionIds.Contains(screenCondition.ScreenSectionId)
        );
        if (screenSectionConditions.Count > 0 && !shouldShowOnScreenSection)
        {
            return null;
        }
        return $"Action {action.Name} ({action.Id}) does not have the MergeType set to \"Ignore\"";
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class RequireListEntityIfListDataStructureDefinedRule : AbstractModelElementRuleAttribute
{
    public RequireListEntityIfListDataStructureDefinedRule() { }

    public override Exception CheckRule(object instance)
    {
        return new NotSupportedException(ResourceUtils.GetString("MemberNameRequired"));
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (string.IsNullOrEmpty(memberName))
        {
            CheckRule(instance);
        }
        if (memberName != "ListEntity")
        {
            throw new Exception(
                $"{nameof(RequireListEntityIfListDataStructureDefinedRule)}"
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
                $"{nameof(FormReferenceMenuItem.ListEntity)} must be "
                    + $"set if {nameof(FormReferenceMenuItem.ListDataStructure)} is set"
            );
        }
        return null;
    }
}
