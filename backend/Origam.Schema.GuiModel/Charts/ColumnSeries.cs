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

[SchemaItemDescription("Column Series", "Data Series", "icon_column-series.png")]
[HelpTopic("Column+Series")]
[ClassMetaVersion("6.0.0")]
public class ColumnSeries : AbstractCartesianSeries
{
	public ColumnSeries() : base() {Init();}
	public ColumnSeries(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public ColumnSeries(Key primaryKey) : base(primaryKey) {Init();}

	private void Init()
	{
			
		}

	#region Properties
	private ColumnSeriesType _type = ColumnSeriesType.Clustered;
	[Category("Series"), DefaultValue(ColumnSeriesType.Clustered)]
	[XmlAttribute("type")]
	public ColumnSeriesType Type
	{
		get
		{
				return _type;
			}
		set
		{
				_type = value;
			}
	}

	public override string ItemType
	{
		get
		{
				return CategoryConst;
			}
	}
	#endregion			
}