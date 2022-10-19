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
	[SchemaItemDescription("Folder", 68)]
	[XmlModelRoot(CategoryConst)]
    [ClassMetaVersion("6.0.0")]
	public class DashboardWidgetFolder : AbstractDashboardWidget
	{
		public DashboardWidgetFolder() : base() {Init();}
		public DashboardWidgetFolder(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
		public DashboardWidgetFolder(Key primaryKey) : base(primaryKey) {Init();}

		private void Init()
		{
			this.ChildItemTypes.Add(typeof(PanelDashboardWidget));
			this.ChildItemTypes.Add(typeof(ChartDashboardWidget));
			this.ChildItemTypes.Add(typeof(DashboardWidgetFolder));

			this.ChildItemTypes.Add(typeof(TextDashboardWidget));
			this.ChildItemTypes.Add(typeof(DateDashboardWidget));
			this.ChildItemTypes.Add(typeof(CurrencyDashboardWidget));
			this.ChildItemTypes.Add(typeof(LookupDashboardWidget));
			this.ChildItemTypes.Add(typeof(CheckBoxDashboardWidget));

			this.ChildItemTypes.Add(typeof(HorizontalContainerDashboardWidget));
			this.ChildItemTypes.Add(typeof(VerticalContainerDashboardWidget));
		}
		
		public override bool CanMove(Origam.UI.IBrowserNode2 newNode)
		{
			return newNode is DashboardWidgetFolder || newNode is DashboardWidgetsSchemaItemProvider;
		}

		public override System.Collections.ArrayList Properties
		{
			get
			{
				return null;
			}
		}


		#region Properties
		public override string Icon
		{
			get
			{
				return "68";
			}
		}

		public override string ItemType
		{
			get
			{
				return CategoryConst;
			}
		}

		[Browsable(false)]
		public new string Roles
		{
			get
			{
				return null;
			}
			set
			{
			}
		}

		[Browsable(false)]
		public new string Features
		{
			get
			{
				return null;
			}
			set
			{
			}
		}
		#endregion			
	}
}

