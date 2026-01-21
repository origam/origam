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

[SchemaItemDescription("Data Page", "data-page.png")]
[HelpTopic("Data+Page")]
[ClassMetaVersion("6.2.0")]
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
        : base(schemaExtensionId)
    {
        Init();
    }

    public XsltDataPage(Key primaryKey)
        : base(primaryKey)
    {
        Init();
    }

    private void Init()
    {
        ChildItemTypes.Add(typeof(PageParameterMapping));
    }

    public override void GetExtraDependencies(List<ISchemaItem> dependencies)
    {
        if (Transformation != null)
        {
            dependencies.Add(Transformation);
        }
        if (DataStructure != null)
        {
            dependencies.Add(DataStructure);
        }
        if (Method != null)
        {
            dependencies.Add(Method);
        }
        if (SortSet != null)
        {
            dependencies.Add(SortSet);
        }
        if (SaveValidationBeforeMerge != null)
        {
            dependencies.Add(SaveValidationBeforeMerge);
        }
        if (SaveValidationAfterMerge != null)
        {
            dependencies.Add(SaveValidationAfterMerge);
        }
        if (LogTransformation != null)
        {
            dependencies.Add(LogTransformation);
        }
        if (DefaultSet != null)
        {
            dependencies.Add(DefaultSet);
        }
        XsltDependencyHelper.GetDependencies(this, dependencies, ResultXPath);
        base.GetExtraDependencies(dependencies);
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

    [TypeConverter(typeof(DataStructureConverter))]
    [RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("dataStructure", "DataStructureId")]
    public DataStructure DataStructure
    {
        get => PersistenceProvider.RetrieveInstance<DataStructure>(DataStructureId);
        set
        {
            Method = null;
            SortSet = null;
            DefaultSet = null;
            DataStructureId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
        }
    }

    public Guid DataStructureMethodId;

    [TypeConverter(typeof(DataStructureReferenceMethodConverter))]
    [XmlReference("method", "DataStructureMethodId")]
    public DataStructureMethod Method
    {
        get => PersistenceProvider.RetrieveInstance<DataStructureMethod>(DataStructureMethodId);
        set => DataStructureMethodId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    public Guid DataStructureSortSetId;

    [TypeConverter(typeof(DataStructureReferenceSortSetConverter))]
    [XmlReference("sortSet", "DataStructureSortSetId")]
    [SortSetRequiredForCustomFiltersRule]
    public DataStructureSortSet SortSet
    {
        get => PersistenceProvider.RetrieveInstance<DataStructureSortSet>(DataStructureSortSetId);
        set => DataStructureSortSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    public Guid DefaultSetId;

    [TypeConverter(typeof(DataStructureReferenceDefaultSetConverter))]
    [RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("defaultSet", "DefaultSetId")]
    public DataStructureDefaultSet DefaultSet
    {
        get => PersistenceProvider.RetrieveInstance<DataStructureDefaultSet>(DefaultSetId);
        set => DefaultSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    public Guid TransformationId;

    [Category("Xslt")]
    [TypeConverter(typeof(TransformationConverter))]
    [Description(
        "A transformation to be applied on output data."
            + " When a field MimeType is application/json, please consider"
            + " to define also TransformationOutputStructure."
    )]
    [XmlReference("transformation", "TransformationId")]
    public AbstractTransformation Transformation
    {
        get => PersistenceProvider.RetrieveInstance<AbstractTransformation>(TransformationId);
        set => TransformationId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    public Guid TransformationOutputStructureId;

    [Category("Xslt")]
    [TypeConverter(typeof(DataStructureConverter))]
    [RefreshProperties(RefreshProperties.Repaint)]
    [Description(
        "A data structure where an output of a Transformation"
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
    [XmlReference("transformationOutputStructure", "TransformationOutputStructureId")]
    public DataStructure TransformationOutputStructure
    {
        get => PersistenceProvider.RetrieveInstance<DataStructure>(TransformationOutputStructureId);
        set =>
            TransformationOutputStructureId =
                value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    public Guid SaveValidationBeforeMergeRuleId;

    [Category("Updating")]
    [TypeConverter(typeof(EndRuleConverter))]
    [XmlReference("saveValidationBeforeMerge", "SaveValidationBeforeMergeRuleId")]
    public IEndRule SaveValidationBeforeMerge
    {
        get => PersistenceProvider.RetrieveInstance<IEndRule>(SaveValidationBeforeMergeRuleId);
        set =>
            SaveValidationBeforeMergeRuleId =
                value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    public Guid SaveValidationAfterMergeRuleId;

    [Category("Updating")]
    [TypeConverter(typeof(EndRuleConverter))]
    [XmlReference("saveValidationAfterMerge", "SaveValidationAfterMergeRuleId")]
    public IEndRule SaveValidationAfterMerge
    {
        get => PersistenceProvider.RetrieveInstance<IEndRule>(SaveValidationAfterMergeRuleId);
        set =>
            SaveValidationAfterMergeRuleId =
                value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    public Guid LogTransformationId;

    [Category("Logging")]
    [TypeConverter(typeof(TransformationConverter))]
    [XmlReference("logTransformation", "LogTransformationId")]
    public AbstractTransformation LogTransformation
    {
        get => PersistenceProvider.RetrieveInstance<AbstractTransformation>(LogTransformationId);
        set => LogTransformationId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
    }

    [Category("Xslt")]
    [Description(
        "An xpath to be run on a result. A string"
            + " value of the resulting Xpath navigator is used."
            + " It's mainly used for"
            + " extracting pure text out of the result xml."
            + " If it's set and application/json mime-type is set too,"
            + " then resulting JSON conversion is always done as"
            + " a non-typed XML->JSON conversion"
    )]
    [XmlAttribute("resultXPath")]
    public string ResultXPath { get; set; }

    [Category("JSON")]
    [Description(
        "Applicable to media type application/json."
            + " If true 'ROOT' element is removed from the output."
    )]
    [XmlAttribute("omitJsonRootElement")]
    public bool OmitJsonRootElement { get; set; }

    [Category("JSON")]
    [Description(
        "Applicable to media type application/json."
            + " If true the main element is removed from the output."
    )]
    [XmlAttribute("omitJsonMainElement")]
    public bool OmitJsonMainElement { get; set; }

    [Category("InputValidation")]
    [XmlAttribute("disableConstraintForInputValidation")]
    public bool DisableConstraintForInputValidation { get; set; }

    [Category("Security")]
    [XmlAttribute("processGetReadRowLevelRules")]
    [Description(
        "Enable checking of field-based row level security rules"
            + " on the output data for GET requests."
            + " Actually only DENY READ field based rules will be"
            + " checked and applied if this is turned on."
    )]
    public bool ProcessReadFieldRowLevelRulesForGetRequests { get; set; }

    [XmlAttribute("allowCustomFilters")]
    [Description("If true can you send a custom filter nad orderings in the request body.")]
    [SingleEntityDataStructureForCustomFiltersRule]
    public bool AllowCustomFilters { get; set; }

    #endregion
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class SingleEntityDataStructureForCustomFiltersRuleAttribute
    : AbstractModelElementRuleAttribute
{
    public override Exception CheckRule(object instance)
    {
        if (instance is not XsltDataPage page)
        {
            throw new Exception($"Instance must be of type {nameof(XsltDataPage)}.");
        }

        if (!page.AllowCustomFilters)
        {
            return null;
        }

        var topEntities = page
            .DataStructure.Entities.Where(entity => entity.Parents.Count() == 1)
            .ToList();
        if (topEntities.Count != 1)
        {
            return new InvalidOperationException(
                $"{nameof(XsltDataPage.AllowCustomFilters)} can only be enabled when the data structure contains a single top entity."
            );
        }
        var topEntity = topEntities.First();
        var otherEntities = page
            .DataStructure.Entities.Where(entity => entity.Id != topEntity.Id)
            .ToList();
        if (otherEntities.Any(entity => entity.Columns.Count > 0))
        {
            return new InvalidOperationException(
                "The selected data structure can only contain one top entity. Only that entity can select fields. It can have sub entities but these can only be used for filtering."
            );
        }

        return null;
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (memberName != nameof(XsltDataPage.AllowCustomFilters))
        {
            throw new Exception(
                $"{nameof(SingleEntityDataStructureForCustomFiltersRuleAttribute)} can only be applied to {nameof(XsltDataPage.AllowCustomFilters)}."
            );
        }
        return CheckRule(instance);
    }
}

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class SortSetRequiredForCustomFiltersRuleAttribute : AbstractModelElementRuleAttribute
{
    public override Exception CheckRule(object instance)
    {
        if (instance is not XsltDataPage page)
        {
            throw new Exception($"Instance must be of type {nameof(XsltDataPage)}.");
        }

        if (page.AllowCustomFilters && page.SortSet == null)
        {
            return new InvalidOperationException(
                $"{nameof(XsltDataPage.SortSet)} cannot be null if {nameof(XsltDataPage.AllowCustomFilters)} is set to true."
            );
        }

        return null;
    }

    public override Exception CheckRule(object instance, string memberName)
    {
        if (memberName != nameof(XsltDataPage.SortSet))
        {
            throw new Exception(
                $"{nameof(SortSetRequiredForCustomFiltersRuleAttribute)} can only be applied to {nameof(XsltDataPage.SortSet)}."
            );
        }
        return CheckRule(instance);
    }
}
