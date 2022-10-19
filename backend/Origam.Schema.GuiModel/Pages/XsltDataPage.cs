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

using Origam.DA.Common;
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;
using Origam.Schema.RuleModel;
using System.Collections.Generic;

namespace Origam.Schema.GuiModel
{
	[SchemaItemDescription("Data Page", "data-page.png")]
    [HelpTopic("Data+Page")]
    [ClassMetaVersion("6.0.0")]
	public class XsltDataPage : AbstractPage, IDataStructureReference
	{
		public XsltDataPage() : base() {Init();}
		public XsltDataPage(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
		public XsltDataPage(Key primaryKey) : base(primaryKey) {Init();}

		private void Init()
		{
			this.ChildItemTypes.Add(typeof(PageParameterMapping));
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			if(this.Transformation != null) dependencies.Add(this.Transformation);
			if(this.DataStructure != null) dependencies.Add(this.DataStructure);
			if(this.Method != null) dependencies.Add(this.Method);
			if(this.SortSet != null) dependencies.Add(this.SortSet);
			if(this.SaveValidationBeforeMerge != null) dependencies.Add(this.SaveValidationBeforeMerge);
			if(this.SaveValidationAfterMerge != null) dependencies.Add(this.SaveValidationAfterMerge);
			if(this.LogTransformation != null) dependencies.Add(this.LogTransformation);
                        if (this.DefaultSet != null) dependencies.Add(this.DefaultSet);                        
			XsltDependencyHelper.GetDependencies(this, dependencies, this.ResultXPath);

			base.GetExtraDependencies (dependencies);
		}

		public override IList<string> NewTypeNames
		{
			get
			{
				try
				{
					IBusinessServicesService agents = ServiceManager.Services.GetService(typeof(IBusinessServicesService)) as IBusinessServicesService;
					IServiceAgent agent = agents.GetAgent("DataService", null, null);
					return agent.ExpectedParameterNames(this, "LoadData", "Parameters");
				}
				catch
				{
					return new string[] {};
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
			get => (DataStructure)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureId));
			set
			{
				this.Method = null;
				this.SortSet = null;
                                this.DefaultSet = null;
				this.DataStructureId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		public Guid DataStructureMethodId;

		[TypeConverter(typeof(DataStructureReferenceMethodConverter))]
		[XmlReference("method", "DataStructureMethodId")]
		public DataStructureMethod Method
		{
			get => (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureMethodId));
			set
			{
				this.DataStructureMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
 
		public Guid DataStructureSortSetId;

		[TypeConverter(typeof(DataStructureReferenceSortSetConverter))]
		[XmlReference("sortSet", "DataStructureSortSetId")]
		public DataStructureSortSet SortSet
		{
			get => (DataStructureSortSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureSortSetId));
			set
			{
				this.DataStructureSortSetId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		public Guid DefaultSetId;

		[TypeConverter(typeof(DataStructureReferenceDefaultSetConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[XmlReference("defaultSet", "DefaultSetId")]
		public DataStructureDefaultSet DefaultSet
		{
			get => (DataStructureDefaultSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DefaultSetId));
			set
			{
				this.DefaultSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}
		
		public Guid TransformationId;

		[Category("Xslt")]
		[TypeConverter(typeof(TransformationConverter))]
		[Description("A transformation to be applied on output data. "
			+ " When a field MimeType is application/json, please consider"
			+ " to define also TransformationOutputStructure.")]
		[XmlReference("transformation", "TransformationId")]
		public AbstractTransformation Transformation
		{
			get => (AbstractTransformation)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.TransformationId));
			set
			{
				this.TransformationId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}

		public Guid TransformationOutputStructureId;

		[Category("Xslt")]
		[TypeConverter(typeof(DataStructureConverter))]
		[RefreshProperties(RefreshProperties.Repaint)]
		[Description("A data structure where an output of a Transformation"
			+ " will be merged into. "
			+ "It's applied only when a ResultXpath is not set and "
			+ "a MimeType field is set to application/json."
			+ "If not defined, the final XML->JSON conversion "
			+ "after transformation works the following way: "
			+ "DataTypes - float, int, boolen, etc. are converted "
			+ "to string, attributes are prefixed with @. "
			+ "But if defined, conversion is DataSet->JSON "
			+ "conversion, which performs the standard way "
			+ "(as without transformation).")]	
		[XmlReference("transformationOutputStructure", "TransformationOutputStructureId")]
		public DataStructure TransformationOutputStructure
		{
			get =>
				(DataStructure)this.PersistenceProvider.RetrieveInstance(
					typeof(AbstractSchemaItem),
					new ModelElementKey(this.TransformationOutputStructureId));
			set
			{
				this.TransformationOutputStructureId =
					(value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}
 
		public Guid SaveValidationBeforeMergeRuleId;

		[Category("Updating")]
		[TypeConverter(typeof(EndRuleConverter))]
		[XmlReference("saveValidationBeforeMerge", "SaveValidationBeforeMergeRuleId")]
		public IEndRule SaveValidationBeforeMerge
		{
			get => (IEndRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.SaveValidationBeforeMergeRuleId));
			set
			{
				this.SaveValidationBeforeMergeRuleId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}
 
		public Guid SaveValidationAfterMergeRuleId;

		[Category("Updating")]
		[TypeConverter(typeof(EndRuleConverter))]
		[XmlReference("saveValidationAfterMerge", "SaveValidationAfterMergeRuleId")]
		public IEndRule SaveValidationAfterMerge
		{
			get => (IEndRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.SaveValidationAfterMergeRuleId));
			set
			{
				this.SaveValidationAfterMergeRuleId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}

		public Guid LogTransformationId;

		[Category("Logging")]
		[TypeConverter(typeof(TransformationConverter))]
		[XmlReference("logTransformation", "LogTransformationId")]
		public AbstractTransformation LogTransformation
		{
			get => (AbstractTransformation)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.LogTransformationId));
			set
			{
				this.LogTransformationId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}


		[Category("Xslt")]
		[Description("An xpath to be run on a result. A string "
		             + "value of the resulting Xpath navigator is used. "
		             + "It's mainly used for "
		             + "extracting pure text out of the result xml. "
		             + "If it's set and application/json mime-type is set too, "
		             + "then resulting JSON conversion is always done as "
		             + "a non-typed XML->JSON conversion")]
		[XmlAttribute ("resultXPath")]
		public string ResultXPath { get; set; }

		[Category("JSON")]
		[Description("Tells whether to remove root 'ROOT' element. "
		             + "It's applied only if a MimeType is application/json and "
		             + " a non-typed XML->JSON conversion is used (Transformation is"
		             + " filled while TransformationOutputDatastructure is not)")]
		[XmlAttribute ("omitJsonRootElement")]
		public bool OmitJsonRootElement { get; set; } = false;

		[Category("InputValidation")]
		[XmlAttribute ("disableConstraintForInputValidation")]
		public bool DisableConstraintForInputValidation { get; set; }
		[Category("Security")]
		[XmlAttribute("processGetReadRowLevelRules")]
		[Description("Enable checking of field-based row level security rules on the output data for GET requests." +
			". Actually only DENY READ field based rules will be checked and applied if this is turned on.")]
		public bool ProcessReadFieldRowLevelRulesForGETRequests { get; set; } = false;
		#endregion
	}
}