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

[SchemaItemDescription("Horizontal Axis", "Horizontal Axes", "icon_horizontal-axis.png")]
[HelpTopic("Horizontal+Axis")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class CartesianChartHorizontalAxis : AbstractSchemaItem
{
	public const string CategoryConst = "CartesianChartHorizontalAxis";

	public CartesianChartHorizontalAxis() : base() {Init();}
	public CartesianChartHorizontalAxis(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public CartesianChartHorizontalAxis(Key primaryKey) : base(primaryKey) {Init();}

	private void Init()
	{
			
		}

	#region Properties
	private string _field = "";
	[Category("Field")]
	[StringNotEmptyModelElementRule()]
	[XmlAttribute("field")]
	public string Field
	{
		get
		{
				return _field;
			}
		set
		{
				_field = value;
			}
	}

	private string _caption = "";
	[Category("Axis")]
	[Localizable(true)]
	[XmlAttribute("label")]
	public string Caption
	{
		get
		{
				return _caption;
			}
		set
		{
				_caption = value;
			}
	}

	private ChartAggregationType _aggregationType = ChartAggregationType.Distinct;
	[Category("Limits"), DefaultValue(ChartAggregationType.Distinct)]
	[XmlAttribute("aggregationType")]
	public ChartAggregationType AggregationType
	{
		get
		{
				return _aggregationType;
			}
		set
		{
				_aggregationType = value;
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