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
using Origam.Workbench.Services;
using Origam.DA.ObjectPersistence;
using Origam.Schema.GuiModel;
using Origam.Schema.EntityModel;
using Origam.Schema.RuleModel;
using System.Collections.Generic;

namespace Origam.Schema.MenuModel
{
	
	
	[SchemaItemDescription("Screen Reference", "menu_form.png")]
    [HelpTopic("Screen+Menu+Item")]
    [ClassMetaVersion("6.0.0")]
	public class FormReferenceMenuItem : AbstractMenuItem, ISchemaItemFactory
	{
		public FormReferenceMenuItem() : base() {}

		public FormReferenceMenuItem(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public FormReferenceMenuItem(Key primaryKey) : base(primaryKey)	{}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Screen);
			if(this.DefaultSet != null) dependencies.Add(this.DefaultSet);
			if(this.Method != null) dependencies.Add(this.Method);
			if(this.SortSet != null) dependencies.Add(this.SortSet);
			if(this.RecordEditMethod != null) dependencies.Add(this.RecordEditMethod);
			if(this.ListDataStructure != null) dependencies.Add(this.ListDataStructure);
			if(this.ListEntity != null) dependencies.Add(this.ListEntity);
			if(this.ListMethod != null) dependencies.Add(this.ListMethod);
			if(this.ListSortSet != null) dependencies.Add(this.ListSortSet);
			if(this.RuleSet != null) dependencies.Add(this.RuleSet);

			if(this.DefaultSet != null | this.Method != null | this.SortSet != null)
			{
				dependencies.Add(this.Screen.DataStructure);
			}

			if(this.SelectionDialogEndRule != null) dependencies.Add(this.SelectionDialogEndRule);
			if(this.SelectionDialogPanel != null) dependencies.Add(this.SelectionDialogPanel);
			if(this.TransformationAfterSelection != null) dependencies.Add(this.TransformationAfterSelection);
			if(this.TransformationBeforeSelection != null) dependencies.Add(this.TransformationBeforeSelection);

			if(this.TemplateSet != null) dependencies.Add(this.TemplateSet);
			if(this.DefaultTemplate != null) dependencies.Add(this.DefaultTemplate);
            if (this.ConfirmationRule != null) dependencies.Add(this.ConfirmationRule);

			base.GetExtraDependencies (dependencies);
		}

		public override Origam.UI.BrowserNodeCollection ChildNodes()
		{
#if ORIGAM_CLIENT
			return new Origam.UI.BrowserNodeCollection();
#else
			return base.ChildNodes ();
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
			get
			{
				return (FormControlSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ScreenId));
			}
			set
			{
				if((value == null) 
                || !ScreenId.Equals((Guid)value.PrimaryKey["Id"]))
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

				this.ScreenId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}

		public Guid TemplateSetId;

		[Category("Templates"), RefreshProperties(RefreshProperties.Repaint)]
		[TypeConverter(typeof(MenuFormReferenceTemplateSetConverter))]
		[XmlReference("templateSet", "TemplateSetId")]
		public DataStructureTemplateSet TemplateSet
		{
			get
			{
				return (DataStructureTemplateSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.TemplateSetId));
			}
			set
			{
				this.TemplateSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];

				this.DefaultTemplate = null;
			}
		}
 
		public Guid DefaultTemplateId;

		[Category("Templates")]
		[TypeConverter(typeof(MenuFormReferenceTemplateConverter))]
		[XmlReference("defaultTemplate", "DefaultTemplateId")]
		public DataStructureTemplate DefaultTemplate
		{
			get
			{
				return (DataStructureTemplate)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DefaultTemplateId));
			}
			set
			{
				this.DefaultTemplateId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}
		
		public Guid DefaultSetId;

		[Category("Screen Reference")]
		[TypeConverter(typeof(MenuFormReferenceDefaultSetConverter))]
		[XmlReference("defaultSet", "DefaultSetId")]
		public DataStructureDefaultSet DefaultSet
		{
			get
			{
				return (DataStructureDefaultSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DefaultSetId));
			}
			set
			{
				this.DefaultSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}
		
		public Guid MethodId;

		[Category("Data Loading")]
		[TypeConverter(typeof(MenuFormReferenceMethodConverter))]
		[NotNullModelElementRule("ListDataStructure")]
		[XmlReference("method", "MethodId")]
		public DataStructureMethod Method
		{
			get
			{
				return (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.MethodId));
			}
			set
			{
				this.MethodId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}
		
		public Guid ListDataStructureId;

		[Category("Data Loading"), RefreshProperties(RefreshProperties.Repaint)]
		[TypeConverter(typeof(DataStructureConverter))]
		[XmlReference("listDataStructure", "ListDataStructureId")]
		public DataStructure ListDataStructure
		{
			get
			{
				return (DataStructure)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ListDataStructureId));
			}
			set
			{
				this.ListDataStructureId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];

				this.ListMethod = null;
				this.ListEntity = null;
			}
		}
		
		public Guid ListMethodId;

		[Category("Data Loading")]
		[TypeConverter(typeof(MenuFormReferenceListMethodConverter))]
		[XmlReference("listMethod", "ListMethodId")]
		public DataStructureMethod ListMethod
		{
			get
			{
				return (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ListMethodId));
			}
			set
			{
				this.ListMethodId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}

		public Guid ListSortSetId;

		[Category("Data Loading")]
		[TypeConverter(typeof(MenuFormReferenceListSortSetConverter))]
		[NotNullModelElementRule("ListDataStructure")]
		[XmlReference("listSortSet", "ListSortSetId")]
		public DataStructureSortSet ListSortSet
		{
			get
			{
				return (DataStructureSortSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ListSortSetId));
			}
			set
			{
				this.ListSortSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}

		public Guid AutoRefreshIntervalConstantId;

		[TypeConverter(typeof(DataConstantConverter))]
		[Category("Data Loading")]
		[Description("Specifies delay in seconds after which data will be automatically refreshed.")]
		[XmlReference("autoRefreshInterval", "AutoRefreshIntervalConstantId")]
		public DataConstant AutoRefreshInterval
		{
			get
			{
				return (DataConstant)this.PersistenceProvider.RetrieveInstance(typeof(DataConstant), new ModelElementKey(AutoRefreshIntervalConstantId));
			}
			set
			{
				this.AutoRefreshIntervalConstantId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		private SaveRefreshType _refreshAfterSaveType = SaveRefreshType.RefreshChangedRecords;

		[Category("Data Loading"), DefaultValue(SaveRefreshType.RefreshChangedRecords)]
		[XmlAttribute("refreshAfterSaveType")]
		public SaveRefreshType RefreshAfterSaveType
		{
			get
			{
				return _refreshAfterSaveType;
			}
			set
			{
				_refreshAfterSaveType = value;
			}
		}
  
		public Guid RecordEditMethodId;

		[Category("Data Loading")]
		[TypeConverter(typeof(MenuFormReferenceMethodConverter))]
		[XmlReference("recordEditMethod", "RecordEditMethodId")]
		public DataStructureMethod RecordEditMethod
		{
			get
			{
				return (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.RecordEditMethodId));
			}
			set
			{
				this.RecordEditMethodId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}

		public Guid ListEntityId;

		[Category("Data Loading")]
		[TypeConverter(typeof(MenuFormReferenceListEntityConverter))]
		[XmlReference("listEntity", "ListEntityId")]
		public DataStructureEntity ListEntity
		{
			get
			{
				return (DataStructureEntity)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ListEntityId));
			}
			set
			{
				this.ListEntityId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}

		public Guid SelectionPanelId;

		[Category("Selection Dialog")]
		[TypeConverter(typeof(PanelControlSetConverter))]
		[XmlReference("selectionDialogPanel", "SelectionPanelId")]
		public PanelControlSet SelectionDialogPanel
		{
			get
			{
				return (PanelControlSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.SelectionPanelId));
			}
			set
			{
				this.SelectionPanelId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}
  
		public Guid SelectionPanelBeforeTransformationId;

		[Category("Selection Dialog")]
		[TypeConverter(typeof(TransformationConverter))]
		[XmlReference("transformationBeforeSelection", "SelectionPanelBeforeTransformationId")]
		public AbstractTransformation TransformationBeforeSelection
		{
			get
			{
				return (AbstractTransformation)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.SelectionPanelBeforeTransformationId));
			}
			set
			{
				this.SelectionPanelBeforeTransformationId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}

		public Guid SelectionPanelAfterTransformationId;

		[Category("Selection Dialog")]
		[TypeConverter(typeof(TransformationConverter))]
		[XmlReference("transformationAfterSelection", "SelectionPanelAfterTransformationId")]
		public AbstractTransformation TransformationAfterSelection
		{
			get
			{
				return (AbstractTransformation)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.SelectionPanelAfterTransformationId));
			}
			set
			{
				this.SelectionPanelAfterTransformationId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}
		
		public Guid SelectionEndRuleId;

		[Category("Selection Dialog")]
		[TypeConverter(typeof(EndRuleConverter))]
		[XmlReference("selectionDialogEndRule", "SelectionEndRuleId")]
		public IEndRule SelectionDialogEndRule
		{
			get
			{
				return (IEndRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.SelectionEndRuleId));
			}
			set
			{
				this.SelectionEndRuleId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}

		[Category("Screen Reference"), DefaultValue(false)]
		[XmlAttribute("readOnlyAccess")]
		public bool ReadOnlyAccess { get; set; } = false;

		[Browsable(false)]
		public string SelectionChangeEntity
		{
			get
			{
				return this.ListEntity != null ? this.ListEntity.Name : null;
			}
		}
		
		public Guid RuleSetId;

		[Category("Screen Reference")]
		[TypeConverter(typeof(MenuFormReferenceRuleSetConverter))]
		[XmlReference("ruleSet", "RuleSetId")]
		public DataStructureRuleSet RuleSet
		{
			get
			{
				return (DataStructureRuleSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.RuleSetId));
			}
			set
			{
				this.RuleSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}

		public Guid SortSetId;

		[Category("Data Loading")]
		[TypeConverter(typeof(MenuFormReferenceSortSetConverter))]
		[NotNullModelElementRule("ListDataStructure")]
		[XmlReference("sortSet", "SortSetId")]
		public DataStructureSortSet SortSet
		{
			get
			{
				return (DataStructureSortSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.SortSetId));
			}
			set
			{
				this.SortSetId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}
 
		public Guid ConfirmationRuleId;

		[Category("References")]
		[TypeConverter(typeof(EndRuleConverter))]
		[XmlReference("confirmationRule", "ConfirmationRuleId")]
		public IEndRule ConfirmationRule
		{
			get
			{
				return (IEndRule)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ConfirmationRuleId));
			}
			set
			{
				this.ConfirmationRuleId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		[Category("Data Loading"), DefaultValue(false)]
		[Description("If true screen will be refresh each time user selects it. If AutomaticRefreshInterval is set, this value is considered as true regardless the actual value.")]
		[XmlAttribute("refreshOnFocus")]
		public bool RefreshOnFocus { get; set; } = false;

		[DefaultValue(false)]
		[Description("If true and List* properties are set (delayed data loading) user will not be asked if she wants to save records before moving to another. Data will be saved automatically.")]
		[XmlAttribute("autoSaveOnListRecordChange")]
		public bool AutoSaveOnListRecordChange { get; set; } = false;

		[DefaultValue(false)]
		[Description("If true, the client will attempt to request save after each update if there are no errors in data.")]
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
            get
            {
                return (DataStructureEntity)PersistenceProvider.RetrieveInstance(
                    typeof(AbstractSchemaItem),
                    new ModelElementKey(DynamicFormLabelEntityId));
            }
            set
            {
                DynamicFormLabelEntityId 
                    = (value == null)
                    ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
                DynamicFormLabelField = null;
            }
        }

		[Category("Dynamic Form Label")]
		[Localizable(false)]
		[XmlAttribute("dynamicFormLabelField")]
        public string DynamicFormLabelField { get; set; }

		#endregion

        #region ISchemaItemFactory Members

        public override Type[] NewItemTypes
		{
			get
			{
				return new Type[] {
									  typeof(SelectionDialogParameterMapping)
								  };
			}
		}

		public override AbstractSchemaItem NewItem(Type type, Guid schemaExtensionId, SchemaItemGroup group)
		{
			AbstractSchemaItem item;

			if(type == typeof(SelectionDialogParameterMapping))
			{
				item = new SelectionDialogParameterMapping(schemaExtensionId);
				item.Name = "NewSelectionDialogParameterMapping";
			}
			else
				throw new ArgumentOutOfRangeException("type", type, ResourceUtils.GetString("ErrorMenuUnknownType"));

			item.Group = group;
			item.PersistenceProvider = this.PersistenceProvider;
			this.ChildItems.Add(item);
			return item;
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

		public bool IsLazyLoaded => ListDataStructure != null;

		#endregion
	}
}
