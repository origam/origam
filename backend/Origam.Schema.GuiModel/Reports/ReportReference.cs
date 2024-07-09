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
using Origam.Schema.ItemCollection;

namespace Origam.Schema.GuiModel;
/// <summary>
/// Summary description for RuleReference.
/// </summary>
[SchemaItemDescription("Report Reference", "icon_report-reference.png")]
[HelpTopic("Report+Reference")]
[DefaultProperty("Report")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class ReportReference : AbstractSchemaItem
{
	public const string CategoryConst = "ReportReference";
	public ReportReference() : base() {}
	public ReportReference(Guid schemaExtensionId) : base(schemaExtensionId) {}
	public ReportReference(Key primaryKey) : base(primaryKey)	{}

	#region Overriden AbstractDataEntityColumn Members
	
	public override string ItemType
	{
		get
		{
			return CategoryConst;
		}
	}
	public override void GetParameterReferences(ISchemaItem parentItem, Dictionary<string, ParameterReference> list)
	{
		if(this.Report != null)
			base.GetParameterReferences(Report, list);
	}
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		dependencies.Add(this.Report);
		base.GetExtraDependencies (dependencies);
	}
	public override ISchemaItemCollection ChildItems
	{
		get
		{
			return SchemaItemCollection.Create();
		}
	}
	#endregion
	#region Properties
	public Guid ReportId;
	[Category("Reference")]
	[TypeConverter(typeof(ReportConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("report", "ReportId")]
	public AbstractReport Report
	{
		get
		{
			ModelElementKey key = new ModelElementKey();
			key.Id = this.ReportId;
			return (ISchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), key) as AbstractReport;
		}
		set
		{
			this.ReportId = (Guid)value.PrimaryKey["Id"];
			//this.Name = this.Report.Name;
		}
	}
	#endregion
}
