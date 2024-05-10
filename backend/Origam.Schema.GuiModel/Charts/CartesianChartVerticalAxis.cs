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
	[SchemaItemDescription("Vertical Axis", "Vertical Axes", "icon_vertical-axis.png")]
    [HelpTopic("Vertical+Axis")]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
    public class CartesianChartVerticalAxis : AbstractSchemaItem
	{
		public const string CategoryConst = "CartesianChartVerticalAxis";

		public CartesianChartVerticalAxis() : base() {Init();}
		public CartesianChartVerticalAxis(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
		public CartesianChartVerticalAxis(Key primaryKey) : base(primaryKey) {Init();}

		private void Init()
		{
			
		}

		#region Properties
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

		private int _min = 0;
		[Category("Limits"), DefaultValue(0)]
		[XmlAttribute("minimum")]
		public int Min
		{
			get
			{
				return _min;
			}
			set
			{
				_min = value;
			}
		}

		private int _max = 0;
		[Category("Limits"), DefaultValue(0)]
		[XmlAttribute("maximum")]
		public int Max
		{
			get
			{
				return _max;
			}
			set
			{
				_max = value;
			}
		}

		private bool _applyMin = false;
		[Category("Limits"), DefaultValue(false)]
		[XmlAttribute("applyMinimumLimit")]
		public bool ApplyMinLimit
		{
			get
			{
				return _applyMin;
			}
			set
			{
				_applyMin = value;
			}
		}

		private bool _applyMax = false;
		[Category("Limits"), DefaultValue(false)]
		[XmlAttribute("applyMaximumLimit")]
		public bool ApplyMaxLimit
		{
			get
			{
				return _applyMax;
			}
			set
			{
				_applyMax = value;
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
}