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
using System.Collections;


namespace Origam.Schema.GuiModel;
public abstract class AbstractSimpleDashboardWidget : AbstractDashboardWidget
{
	public AbstractSimpleDashboardWidget() : base() {Init();}
	public AbstractSimpleDashboardWidget(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public AbstractSimpleDashboardWidget(Key primaryKey) : base(primaryKey) {Init();}
	private void Init()
	{
	}
	public abstract OrigamDataType DataType {get; }
	public override ArrayList Properties
	{
		get
		{
			ArrayList result = new ArrayList();
			result.Add(new DashboardWidgetProperty("Value",
				ResourceUtils.GetString("DashboardWidgetValueProperty"),
				this.DataType));
			return result;
		}
	}
}
