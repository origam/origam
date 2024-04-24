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
using System.Xml.Serialization;
using System.Collections.Generic;

namespace Origam.Schema.MenuModel
{
	/// <summary>
	/// Summary description for EntitySecurityRule.
	/// </summary>
	[SchemaItemDescription("Report Action", "UI Actions", "icon_report-action.png")]
    [HelpTopic("Report+Action")]
    [ClassMetaVersion("6.0.0")]
	public class EntityReportAction : EntityUIAction
	{
		public EntityReportAction() : base() {}

		public EntityReportAction(Guid schemaExtensionId) : base(schemaExtensionId) {}

		public EntityReportAction(Key primaryKey) : base(primaryKey)	{}
	
		#region Overriden AbstractDataEntityColumn Members
		
		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Report);

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
					return agent.ExpectedParameterNames(this.Report, "LoadData", "Parameters");
				}
				catch
				{
					return new string[] {};
				}
			}
		}
		#endregion

		#region Properties
		[Browsable(false)]
		public override PanelActionType ActionType
		{
			get
			{
				return PanelActionType.Report;
			}
			set
			{
				throw new InvalidOperationException();
			}
		}

		public Guid ReportId;

		[Category("References")]
		[TypeConverter(typeof(ReportConverter))]
        [XmlReference("report", "ReportId")]
		[NotNullModelElementRule]
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
	}
}
