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
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;


namespace Origam.Schema.GuiModel;

[SchemaItemDescription("Lookup Widget", "icon_lookup-widget.png")]
[ClassMetaVersion("6.0.0")]
public class LookupDashboardWidget : AbstractSimpleDashboardWidget
{
	public LookupDashboardWidget() : base() {Init();}
	public LookupDashboardWidget(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public LookupDashboardWidget(Key primaryKey) : base(primaryKey) {Init();}

	private void Init()
	{
		}

	public override OrigamDataType DataType
	{
		get
		{
				return this.Lookup.ValueColumn.DataType;
			}
	}

	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
			dependencies.Add(this.Lookup);

			base.GetExtraDependencies (dependencies);
		}

	#region Properties
	public Guid LookupId;

	[Category("Reference")]
	[TypeConverter(typeof(DataLookupConverter))]
	[RefreshProperties(RefreshProperties.Repaint)]
	[NotNullModelElementRule()]
	[XmlReference("lookup", "LookupId")]
	public IDataLookup Lookup
	{
		get
		{
				return (AbstractSchemaItem)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.LookupId)) as IDataLookup;
			}
		set
		{
				this.LookupId = (Guid)value.PrimaryKey["Id"];

				this.Name = this.Lookup.Name;
			}
	}
	#endregion			
}