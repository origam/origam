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
using Origam.Workbench.Services;

namespace Origam.Schema.GuiModel;

[SchemaItemDescription(name: "Data Page", iconName: "data-page.png")]
[HelpTopic(topic: "Data+Page")]
[ClassMetaVersion(versionStr: "6.2.0")]
public class XsltDataPage : AbstractPage, IDataStructureReference
{
    public static readonly string FiltersParameterName = "filters";
    public static readonly string FilterLookupsParameterName = "filterLookups";

    public XsltDataPage()
        : base()
    {
        Init();
    }

    public XsltDataPage(Guid schemaExtensionId)
        : base(schemaExtensionId: schemaExtensionId)
    {
        Init();
    }

    public XsltDataPage(Key primaryKey)
        : base(primaryKey: primaryKey)
    {
        Init();
    }

    private void Init()
    {
        ChildItemTypes.Add(item: typeof(PageParameterMapping));
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (Transformation != null)
        {
            dependencies.Add(item: Transformation);
        }
        if (DataStructure != null)
        {
            dependencies.Add(item: DataStructure);
        }
        if (Method != null)
        {
            dependencies.Add(item: Method);
        }
        if (SortSet != null)
        {
            dependencies.Add(item: SortSet);
        }
        if (SaveValidationBeforeMerge != null)
        {
            dependencies.Add(item: SaveValidationBeforeMerge);
        }
        if (SaveValidationAfterMerge != null)
        {
            dependencies.Add(item: SaveValidationAfterMerge);
        }
        if (LogTransformation != null)
        {
            dependencies.Add(item: LogTransformation);
        }
        if (DefaultSet != null)
        {
            dependencies.Add(item: DefaultSet);
        }
        XsltDependencyHelper.GetDependencies(
            item: this,
            dependencies: dependencies,
            text: ResultXPath
        );
        base.GetExtraDependencies(dependencies: dependencies);
    }

    public override IList<string> NewTypeNames
    {
        get
        {
            try
            {
                var businessServicesService =
                    ServiceManager.Services.GetService<IBusinessServicesService>();
                IServiceAgent dataServiceAgent = businessServicesService.GetAgent(
                    serviceType: "DataService",
                    ruleEngine: null,
                    workflowEngine: null
                );
                return dataServiceAgent.ExpectedParameterNames(
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

    #region Properties
    public Guid DataStructureId;

    [TypeConverter(type: typeof(DataStructureConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "dataStructure", idField: "DataStructureId")]
    public DataStructure DataStructure
    {
        get => PersistenceProvider.RetrieveInstance<DataStructure>(instanceId: DataStructureId);
        set
        {
            Method = null;
            SortSet = null;
            DefaultSet = null;
            DataStructureId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
        }
    }

    public Guid DataStructureMethodId;

    [TypeConverter(type: typeof(DataStructureReferenceMethodConverter))]
    [XmlReference(attributeName: "method", idField: "DataStructureMethodId")]
    public DataStructureMethod Method
    {
        get =>
            PersistenceProvider.RetrieveInstance<DataStructureMethod>(
                instanceId: DataStructureMethodId
            );
        set =>
            DataStructureMethodId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid DataStructureSortSetId;

    [TypeConverter(type: typeof(DataStructureReferenceSortSetConverter))]
    [XmlReference(attributeName: "sortSet", idField: "DataStructureSortSetId")]
    [SortSetRequiredForCustomFiltersRule]
    public DataStructureSortSet SortSet
    {
        get =>
            PersistenceProvider.RetrieveInstance<DataStructureSortSet>(
                instanceId: DataStructureSortSetId
            );
        set =>
            DataStructureSortSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid DefaultSetId;

    [TypeConverter(type: typeof(DataStructureReferenceDefaultSetConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [XmlReference(attributeName: "defaultSet", idField: "DefaultSetId")]
    public DataStructureDefaultSet DefaultSet
    {
        get =>
            PersistenceProvider.RetrieveInstance<DataStructureDefaultSet>(instanceId: DefaultSetId);
        set => DefaultSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid TransformationId;

    [Category(category: "Xslt")]
    [TypeConverter(type: typeof(TransformationConverter))]
    [Description(
        description: "A transformation to be applied on output data."
            + " When a field MimeType is application/json, please consider"
            + " to define also TransformationOutputStructure."
    )]
    [XmlReference(attributeName: "transformation", idField: "TransformationId")]
    public AbstractTransformation Transformation
    {
        get =>
            PersistenceProvider.RetrieveInstance<AbstractTransformation>(
                instanceId: TransformationId
            );
        set => TransformationId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid TransformationOutputStructureId;

    [Category(category: "Xslt")]
    [TypeConverter(type: typeof(DataStructureConverter))]
    [RefreshProperties(refresh: RefreshProperties.Repaint)]
    [Description(
        description: "A data structure where an output of a Transformation"
            + " will be merged into."
            + " It's applied only when a ResultXpath is not set and"
            + " a MimeType field is set to application/json."
            + " If not defined, the final XML->JSON conversion"
            + " after transformation works the following way:"
            + " DataTypes - float, int, boolen, etc. are converted"
            + " to string, attributes are prefixed with @."
            + " But if defined, conversion is DataSet->JSON"
            + " conversion, which performs the standard way"
            + " (as without transformation)."
    )]
    [XmlReference(
        attributeName: "transformationOutputStructure",
        idField: "TransformationOutputStructureId"
    )]
    public DataStructure TransformationOutputStructure
    {
        get =>
            PersistenceProvider.RetrieveInstance<DataStructure>(
                instanceId: TransformationOutputStructureId
            );
        set =>
            TransformationOutputStructureId =
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid SaveValidationBeforeMergeRuleId;

    [Category(category: "Updating")]
    [TypeConverter(type: typeof(EndRuleConverter))]
    [XmlReference(
        attributeName: "saveValidationBeforeMerge",
        idField: "SaveValidationBeforeMergeRuleId"
    )]
    public IEndRule SaveValidationBeforeMerge
    {
        get =>
            PersistenceProvider.RetrieveInstance<IEndRule>(
                instanceId: SaveValidationBeforeMergeRuleId
            );
        set =>
            SaveValidationBeforeMergeRuleId =
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid SaveValidationAfterMergeRuleId;

    [Category(category: "Updating")]
    [TypeConverter(type: typeof(EndRuleConverter))]
    [XmlReference(
        attributeName: "saveValidationAfterMerge",
        idField: "SaveValidationAfterMergeRuleId"
    )]
    public IEndRule SaveValidationAfterMerge
    {
        get =>
            PersistenceProvider.RetrieveInstance<IEndRule>(
                instanceId: SaveValidationAfterMergeRuleId
            );
        set =>
            SaveValidationAfterMergeRuleId =
                value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    public Guid LogTransformationId;

    [Category(category: "Logging")]
    [TypeConverter(type: typeof(TransformationConverter))]
    [XmlReference(attributeName: "logTransformation", idField: "LogTransformationId")]
    public AbstractTransformation LogTransformation
    {
        get =>
            PersistenceProvider.RetrieveInstance<AbstractTransformation>(
                instanceId: LogTransformationId
            );
        set => LogTransformationId = value == null ? Guid.Empty : (Guid)value.PrimaryKey[key: "Id"];
    }

    [Category(category: "Xslt")]
    [Description(
        description: "An xpath to be run on a result. A string"
            + " value of the resulting Xpath navigator is used."
            + " It's mainly used for"
            + " extracting pure text out of the result xml."
            + " If it's set and application/json mime-type is set too,"
            + " then resulting JSON conversion is always done as"
            + " a non-typed XML->JSON conversion"
    )]
    [XmlAttribute(attributeName: "resultXPath")]
    public string ResultXPath { get; set; }

    [Category(category: "JSON")]
    [Description(
        description: "Applicable to media type application/json."
            + " If true 'ROOT' element is removed from the output."
    )]
    [XmlAttribute(attributeName: "omitJsonRootElement")]
    public bool OmitJsonRootElement { get; set; }

    [Category(category: "JSON")]
    [Description(
        description: "Applicable to media type application/json."
            + " If true the main element is removed from the output."
    )]
    [XmlAttribute(attributeName: "omitJsonMainElement")]
    public bool OmitJsonMainElement { get; set; }

    [Category(category: "InputValidation")]
    [XmlAttribute(attributeName: "disableConstraintForInputValidation")]
    public bool DisableConstraintForInputValidation { get; set; }

    [Category(category: "Security")]
    [XmlAttribute(attributeName: "processGetReadRowLevelRules")]
    [Description(
        description: "Enable checking of field-based row level security rules"
            + " on the output data for GET requests."
            + " Actually only DENY READ field based rules will be"
            + " checked and applied if this is turned on."
    )]
    public bool ProcessReadFieldRowLevelRulesForGetRequests { get; set; }

    [XmlAttribute(attributeName: "allowCustomFilters")]
    [LocalizedDescription(
        resourceKey: nameof(Strings.AllowCustomFiltersDescription),
        resourceType: typeof(Strings)
    )]
    [SingleEntityDataStructureForCustomFiltersRule]
    public bool AllowCustomFilters { get; set; }

    #endregion
}

[AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class SingleEntityDataStructureForCustomFiltersRuleAttribute
    : AbstractModelElementRuleAttribute
{
    public override Exception CheckRule(object instance)
    {
        if (instance is not XsltDataPage page)
        {
            throw new Exception(
                message: string.Format(
                    format: Strings.ErrorXsltDataPageInstanceType,
                    arg0: nameof(XsltDataPage)
                )
            );
        }

        if (!page.AllowCustomFilters)
        {
            return null;
        }

        var topEntities = page
            .DataStructure.Entities.Where(predicate: entity => entity.Parents.Count() == 1)
            .ToList();
        if (topEntities.Count != 1)
        {
            return new InvalidOperationException(
                message: string.Format(
                    format: Strings.ErrorAllowCustomFiltersSingleTopEntity,
                    arg0: nameof(XsltDataPage.AllowCustomFilters)
                )
            );
        }
        var topEntity = topEntities.First();
        var otherEntities = page
            .DataStructure.Entities.Where(predicate: entity => entity.Id != topEntity.Id)
            .ToList();
        if (otherEntities.Any(predicate: entity => entity.Columns.Count > 0))
        {
            return new InvalidOperationException(
                message: Strings.ErrorAllowCustomFiltersSingleTopEntityFieldsOnly
            );
        }

        return null;
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (memberName != nameof(XsltDataPage.AllowCustomFilters))
        {
            throw new Exception(
                message: string.Format(
                    format: Strings.ErrorSingleEntityDataStructureForCustomFiltersRuleAttributeInvalidTarget,
                    arg0: nameof(SingleEntityDataStructureForCustomFiltersRuleAttribute),
                    arg1: nameof(XsltDataPage.AllowCustomFilters)
                )
            );
        }
        return CheckRule(instance: instance);
    }
}

[AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class SortSetRequiredForCustomFiltersRuleAttribute : AbstractModelElementRuleAttribute
{
    public override Exception CheckRule(object instance)
    {
        if (instance is not XsltDataPage page)
        {
            throw new Exception(
                message: string.Format(
                    format: Strings.ErrorXsltDataPageInstanceType,
                    arg0: nameof(XsltDataPage)
                )
            );
        }

        if (page.AllowCustomFilters && page.SortSet == null)
        {
            return new InvalidOperationException(
                message: string.Format(
                    format: Strings.ErrorSortSetRequiredForCustomFilters,
                    arg0: nameof(XsltDataPage.SortSet),
                    arg1: nameof(XsltDataPage.AllowCustomFilters)
                )
            );
        }

        return null;
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (memberName != nameof(XsltDataPage.SortSet))
        {
            throw new Exception(
                message: string.Format(
                    format: Strings.ErrorSortSetRequiredForCustomFiltersRuleAttributeInvalidTarget,
                    arg0: nameof(SortSetRequiredForCustomFiltersRuleAttribute),
                    arg1: nameof(XsltDataPage.SortSet)
                )
            );
        }
        return CheckRule(instance: instance);
    }
}
