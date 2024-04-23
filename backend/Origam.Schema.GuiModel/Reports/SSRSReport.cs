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

namespace Origam.Schema.GuiModel;

[SchemaItemDescription("SQL Server Report", "icon_sql-server-report.png")]
[HelpTopic("SQL+Server+Report")]
[ClassMetaVersion("6.0.0")]
public class SSRSReport : AbstractReport
{
	public SSRSReport() : base() { }

	public SSRSReport(Guid schemaExtensionId) : base(schemaExtensionId) { }

	public SSRSReport(Key primaryKey) : base(primaryKey) { }

	private string _reportPath;

	[Description("Path to the report. The Path starts with forward slash. The Path supports a curly-bracket parameter expansion, e.g '/my-report-{language}', where 'language' is a name of a report parameter.")]
	[XmlAttribute("reportPath")]
	public string ReportPath
	{
		get
		{
				return _reportPath;
			}
		set
		{
				_reportPath = value;
			}
	}
}