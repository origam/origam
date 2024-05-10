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


namespace Origam.Schema.GuiModel
{
	[SchemaItemDescription("SVG Chart", "icon_svg-chart.png")]
    [HelpTopic("SVG+Chart")]
    [ClassMetaVersion("6.0.0")]
	public class SvgChart : AbstractChart
	{
		public SvgChart() : base() {Init();}
		public SvgChart(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
		public SvgChart(Key primaryKey) : base(primaryKey) {Init();}

		private void Init()
		{
		}

		#region Properties
		private string _svgFileName = "";
		[Category("SVG Chart")]
		[StringNotEmptyModelElementRule()]
        [XmlAttribute("svgFileName")]
		public string SvgFileName
		{
			get
			{
				return _svgFileName;
			}
			set
			{
				_svgFileName = value;
			}
		}

		private string _svgObjectField = "";
		[Category("SVG Chart")]
		[StringNotEmptyModelElementRule()]
        [XmlAttribute("svgObjectField")]
        public string SvgObjectField
		{
			get
			{
				return _svgObjectField;
			}
			set
			{
				_svgObjectField = value;
			}
		}

		private string _valueField = "";
		[Category("SVG Chart")]
		[StringNotEmptyModelElementRule()]
        [XmlAttribute("valueField")]
        public string ValueField
		{
			get
			{
				return _valueField;
			}
			set
			{
				_valueField = value;
			}
		}

		private string _titleField = "";
		[Category("SVG Chart")]
		[StringNotEmptyModelElementRule()]
        [XmlAttribute("titleField")]
        public string TitleField
		{
			get
			{
				return _titleField;
			}
			set
			{
				_titleField = value;
			}
		}

		private SvgChartType _type = SvgChartType.HeatMap;
		[Category("SVG Chart"), DefaultValue(SvgChartType.HeatMap)]
		[XmlAttribute("type")]
        public SvgChartType Type
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
		#endregion			
	}
}