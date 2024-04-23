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

namespace Origam.Schema.MenuModel;

[SchemaItemDescription("Report Reference", "menu_report.png")]
[HelpTopic("Report+Menu+Item")]
[ClassMetaVersion("6.0.0")]
public class ReportReferenceMenuItem : AbstractMenuItem
{
	public ReportReferenceMenuItem() {}

	public ReportReferenceMenuItem(Guid schemaExtensionId) 
		: base(schemaExtensionId) {}

	public ReportReferenceMenuItem(Key primaryKey) : base(primaryKey) {}

	public override void GetExtraDependencies(
		System.Collections.ArrayList dependencies)
	{
			dependencies.Add(Report);
			if(SelectionDialogEndRule != null)
			{
				dependencies.Add(SelectionDialogEndRule);
			}
			if(SelectionDialogPanel != null)
			{
				dependencies.Add(SelectionDialogPanel);
			}
			if(TransformationBeforeSelection != null)
			{
				dependencies.Add(TransformationBeforeSelection);
			}
			if(TransformationAfterSelection != null)
			{
				dependencies.Add(TransformationAfterSelection);
			}
			base.GetExtraDependencies(dependencies);
		}

	public override UI.BrowserNodeCollection ChildNodes()
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
	[NotNullModelElementRule]
	public AbstractReport Report
	{
		get => (AbstractReport)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), new ModelElementKey(ReportId));
		set => ReportId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}

	public Guid SelectionPanelId;

	[Category("Selection Dialog")]
	[TypeConverter(typeof(PanelControlSetConverter))]
	[XmlReference("selectionDialogScreenSection", "SelectionPanelId")]
	public PanelControlSet SelectionDialogPanel
	{
		get => (PanelControlSet)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), 
			new ModelElementKey(SelectionPanelId));
		set => SelectionPanelId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}

	public Guid SelectionPanelBeforeTransformationId;

	[Category("Selection Dialog")]
	[TypeConverter(typeof(TransformationConverter))]
	[XmlReference("transformationBeforeSelection", 
		"SelectionPanelBeforeTransformationId")]
	public AbstractTransformation TransformationBeforeSelection
	{
		get => (AbstractTransformation)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), 
			new ModelElementKey(SelectionPanelBeforeTransformationId));
		set => SelectionPanelBeforeTransformationId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}

	public Guid SelectionPanelAfterTransformationId;

	[Category("Selection Dialog")]
	[TypeConverter(typeof(TransformationConverter))]
	[XmlReference("transformationAfterSelection",
		"SelectionPanelAfterTransformationId")]
	public AbstractTransformation TransformationAfterSelection
	{
		get => (AbstractTransformation)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), 
			new ModelElementKey(SelectionPanelAfterTransformationId));
		set => SelectionPanelAfterTransformationId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}

	public Guid SelectionEndRuleId;

	[Category("Selection Dialog")]
	[TypeConverter(typeof(EndRuleConverter))]
	[XmlReference("selectionDialogEndRule", "SelectionEndRuleId")]
	public IEndRule SelectionDialogEndRule
	{
		get => (IEndRule)PersistenceProvider.RetrieveInstance(
			typeof(AbstractSchemaItem), 
			new ModelElementKey(SelectionEndRuleId));
		set => SelectionEndRuleId = (value == null) 
			? Guid.Empty : (Guid)value.PrimaryKey["Id"];
	}

	private DataReportExportFormatType _exportFormatType;
	[Category("Data Report")]
	[Description("Export Format Type")]
	[XmlAttribute("exportFormatType")]
	public DataReportExportFormatType ExportFormatType
	{
		get => _exportFormatType;
		set => _exportFormatType = value;
	}
	#endregion

	#region ISchemaItemFactory Members

	public override Type[] NewItemTypes => new[] 
		{ 
			typeof(SelectionDialogParameterMapping)
		};

	public override T NewItem<T>(
		Guid schemaExtensionId, SchemaItemGroup group)
	{
			return base.NewItem<T>(schemaExtensionId, group, 
				typeof(T) == typeof(SelectionDialogParameterMapping) ?
					"NewSelectionDialogParameterMapping" : null);
		}

	public override IList<string> NewTypeNames
	{
		get
		{
				try
				{
					var businessServicesService = ServiceManager.Services
						.GetService<IBusinessServicesService>();
					var serviceAgent = businessServicesService.GetAgent(
						"DataService", null, null);
					return serviceAgent.ExpectedParameterNames(
						Report, "LoadData", "Parameters");
				}
				catch
				{
					return new string[] {};
				}
			}
	}
	#endregion
}