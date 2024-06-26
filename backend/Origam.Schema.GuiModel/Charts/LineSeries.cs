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
[SchemaItemDescription("Line Series", "Data Series", "icon_line-series-2.png")]
[HelpTopic("Line+Series")]
[ClassMetaVersion("6.0.0")]
public class LineSeries : AbstractCartesianSeries
{
	public LineSeries() : base() {Init();}
	public LineSeries(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public LineSeries(Key primaryKey) : base(primaryKey) {Init();}
	private void Init()
	{
		
	}
	#region Properties
	private LineSeriesForm _form = LineSeriesForm.Segment;
	[Category("Series"), DefaultValue(LineSeriesForm.Segment)]
	[XmlAttribute("form")]
	public LineSeriesForm Form
	{
		get
		{
			return _form;
		}
		set
		{
			_form = value;
		}
	}
	public override string Icon
	{
		get
		{
			return "74";
		}
	}
	#endregion			
}
