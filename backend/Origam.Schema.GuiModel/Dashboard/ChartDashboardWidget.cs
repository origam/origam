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

using System;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;


namespace Origam.Schema.GuiModel
{
	[SchemaItemDescription("Chart Widget", "icon_chart-widget.png")]
	public class ChartDashboardWidget : AbstractDataDashboardWidget
	{
		public ChartDashboardWidget() : base() {Init();}
		public ChartDashboardWidget(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
		public ChartDashboardWidget(Key primaryKey) : base(primaryKey) {Init();}

		private void Init()
		{
		}

		public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
		{
			dependencies.Add(this.Chart);

			base.GetExtraDependencies (dependencies);
		}

		#region Properties
		public Guid ChartId;

		[Category("UI")]
		[TypeConverter(typeof(ChartsConverter))]
        [XmlReference("chart", "ChartId")]
		public AbstractChart Chart
		{
			get
			{
				return (AbstractChart)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.ChartId));
			}
			set
			{
				this.ChartId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
			}
		}
		#endregion			
	}
}