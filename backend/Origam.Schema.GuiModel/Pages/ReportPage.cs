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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Workbench.Services;

namespace Origam.Schema.GuiModel;
[SchemaItemDescription("Report Page", "report-page.png")]
[HelpTopic("Report+Page")]
[ClassMetaVersion("6.0.0")]
public class ReportPage : AbstractPage
{
	public ReportPage() : base() { Init(); }
	public ReportPage(Guid schemaExtensionId) : base(schemaExtensionId) { Init(); }
	public ReportPage(Key primaryKey) : base(primaryKey) { Init(); }
	private void Init()
	{
		this.ChildItemTypes.Add(typeof(PageParameterMapping));
	}
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		dependencies.Add(this.Report);
		base.GetExtraDependencies(dependencies);
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
				return new string[] { };
			}
		}
	}		
	#region Properties
	public Guid ReportId;
	[Category("Report")]
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
			this.ReportId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
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
