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
using System.Collections;

using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;


namespace Origam.Schema.GuiModel;
[ClassMetaVersion("6.0.0")]
public class AbstractDataDashboardWidget : AbstractDashboardWidget, IDataStructureReference
{
	public AbstractDataDashboardWidget() : base() {Init();}
	public AbstractDataDashboardWidget(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public AbstractDataDashboardWidget(Key primaryKey) : base(primaryKey) {Init();}
	private void Init()
	{
	}
	public override void GetExtraDependencies(System.Collections.ArrayList dependencies)
	{
		dependencies.Add(this.DataStructure);
		if(this.Method != null) dependencies.Add(this.Method);
		if(this.SortSet != null) dependencies.Add(this.SortSet);
		base.GetExtraDependencies (dependencies);
	}
	#region Properties
	public override string Icon
	{
		get
		{
			return "79";
		}
	}
	
	public Guid DataStructureId;
	[TypeConverter(typeof(DataStructureConverter))]
	[Category("Data"), RefreshProperties(RefreshProperties.Repaint)]
    [XmlReference("dataStructure", "DataStructureId")]
	public DataStructure DataStructure
	{
		get
		{
			return (DataStructure)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureId));
		}
		set
		{
			this.Method = null;
			this.SortSet = null;
			this.DataStructureId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
		}
	}
	public Guid DataStructureMethodId;
	[Category("Data"), TypeConverter(typeof(DataStructureReferenceMethodConverter))]
    [XmlReference("method", "DataStructureMethodId")]
	public DataStructureMethod Method
	{
		get
		{
			return (DataStructureMethod)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureMethodId));
		}
		set
		{
			this.DataStructureMethodId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
		}
	}
	public Guid DataStructureSortSetId;
	[Category("Data"), TypeConverter(typeof(DataStructureReferenceSortSetConverter))]
    [XmlReference("sortSet", "DataStructureSortSetId")]
	public DataStructureSortSet SortSet
	{
		get
		{
			return (DataStructureSortSet)this.PersistenceProvider.RetrieveInstance(typeof(AbstractSchemaItem), new ModelElementKey(this.DataStructureSortSetId));
		}
		set
		{
			this.DataStructureSortSetId = (value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"]);
		}
	}
	public override ArrayList Properties
	{
		get
		{
			ArrayList result = new ArrayList();
			DataStructureEntity entity = this.DataStructure.Entities[0] as DataStructureEntity;
			foreach(DataStructureColumn column in entity.Columns)
			{
				result.Add(new DashboardWidgetProperty(column.Name, (column.Caption == null ? column.Field.Caption : column.Caption), column.Field.DataType));
			}
			return result;
		}
	}
	#endregion			
}
