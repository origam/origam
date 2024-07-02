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
using System.Collections.Generic;
using System.ComponentModel;
using Origam.DA.ObjectPersistence;


namespace Origam.Schema.GuiModel;
[SchemaItemDescription("Panel Widget", "icon_panel-widget.png")]
public class PanelDashboardWidget : AbstractDataDashboardWidget
{
	public PanelDashboardWidget() : base() {Init();}
	public PanelDashboardWidget(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public PanelDashboardWidget(Key primaryKey) : base(primaryKey) {Init();}
	private void Init()
	{
	}
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		dependencies.Add(this.Panel);
		base.GetExtraDependencies (dependencies);
	}
	#region Properties
	public Guid PanelId;
	[Category("UI")]
	[TypeConverter(typeof(PanelControlSetConverter))]
    [XmlReference("screenSection", "PanelId")]
	public PanelControlSet Panel
	{
		get
		{
			return (PanelControlSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.PanelId));
		}
		set
		{
			this.PanelId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
		}
	}
	#endregion			
}
