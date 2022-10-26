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

using Origam.Workbench.Services;
using Origam.DA.ObjectPersistence;
using Origam.Schema.GuiModel;
using Origam.Schema.RuleModel;
using Origam.Schema.EntityModel;
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Origam.Schema.MenuModel
{
	/// <summary>
	/// Summary description for FormReferenceMenuItem.
	/// </summary>
	[SchemaItemDescription("Report Reference", "menu_report.png")]
    [HelpTopic("Report+Menu+Item")]
    [ClassMetaVersion("6.0.0")]
	public class ReportReferenceMenuItem : AbstractMenuItem
	{
		public ReportReferenceMenuItem() : base() {}

		public ReportReferenceMenuItem(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public ReportReferenceMenuItem(Key primaryKey) : base(primaryKey)	{}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Report);
			if(this.SelectionDialogEndRule != null) dependencies.Add(this.SelectionDialogEndRule);
			if(this.SelectionDialogPanel != null) dependencies.Add(this.SelectionDialogPanel);
			if(this.TransformationBeforeSelection != null) dependencies.Add(this.TransformationBeforeSelection);
			if(this.TransformationAfterSelection != null) dependencies.Add(this.TransformationAfterSelection);

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
		public Guid ReportId;

		[Category("Report Reference")]
		[TypeConverter(typeof(ReportConverter))]
        [XmlReference("report", "ReportId")]
		public AbstractReport Report
		{
			get
			{
				return (AbstractReport)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ReportId));
			}
			set
			{
				this.ReportId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
			}
		}

		public Guid SelectionPanelId;

		[Category("Selection Dialog")]
		[TypeConverter(typeof(PanelControlSetConverter))]
        [XmlReference("selectionDialogScreenSection", "SelectionPanelId")]
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
        [XmlReference("transformationBeforeSelection", 
            "SelectionPanelBeforeTransformationId")]
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
        [XmlReference("transformationAfterSelection",
            "SelectionPanelAfterTransformationId")]
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

		private DataReportExportFormatType _exportFormatType;
		[Category("Data Report")]
		[Description("Export Format Type")]
        [XmlAttribute("exportFormatType")]
		public DataReportExportFormatType ExportFormatType
		{
			get
			{
				return _exportFormatType;
			}
			set
			{
				_exportFormatType = value;
			}
		}
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
					return agent.ExpectedParameterNames(this.Report, "LoadData", "Parameters");
				}
				catch
				{
					return new string[] {};
				}
			}
		}
		#endregion
	}
}
