#region license
/*
Copyright 2005 - 2018 Advantage Solutions, s. r. o.

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

using System;


namespace Origam.Schema.GuiModel
{
	[SchemaItemDescription("Cartesian Chart", 78)]
    [HelpTopic("Cartesian+Charts")]
	public class CartesianChart : AbstractChart
	{
		public CartesianChart() : base() {Init();}
		public CartesianChart(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
		public CartesianChart(Key primaryKey) : base(primaryKey) {Init();}

		private void Init()
		{
			this.ChildItemTypes.Add(typeof(CartesianChartVerticalAxis));
			this.ChildItemTypes.Add(typeof(CartesianChartHorizontalAxis));
			this.ChildItemTypes.Add(typeof(ColumnSeries));
			this.ChildItemTypes.Add(typeof(LineSeries));
		}

		#region Properties
		public override string Icon
		{
			get
			{
				return "78";
			}
		}
		#endregion			
	}
}