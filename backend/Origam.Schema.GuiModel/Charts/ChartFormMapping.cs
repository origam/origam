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
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using Origam.DA.ObjectPersistence;
using Origam.Schema.EntityModel;


namespace Origam.Schema.GuiModel;
[SchemaItemDescription("Screen Mapping", "Screen Mappings", "icon_screen-mapping.png")]
[HelpTopic("Chart+Screen+Mapping")]
[XmlModelRoot(CategoryConst)]
[ClassMetaVersion("6.0.0")]
public class ChartFormMapping : AbstractSchemaItem
{
	public const string CategoryConst = "ChartFormMapping";
	public ChartFormMapping() : base() {Init();}
	public ChartFormMapping(Guid schemaExtensionId) : base(schemaExtensionId) {Init();}
	public ChartFormMapping(Key primaryKey) : base(primaryKey) {Init();}
	private void Init()
	{
		
	}
	private void UpdateName()
	{
		if(this.Screen != null)
		{
			this.Name = this.Screen.Name;
			if(this.Entity != null)
			{
				this.Name += "_" + this.Entity.Name;
			}
		}
	}
	public override void GetExtraDependencies(List<ISchemaItem> dependencies)
	{
		dependencies.Add(this.Screen);
		dependencies.Add(this.Entity);
		base.GetExtraDependencies (dependencies);
	}
	#region Properties
	public Guid ScreenId;
	[Category("Screen Reference")]
	[TypeConverter(typeof(FormControlSetConverter))]
	[NotNullModelElementRule()]
    [XmlReference("screen", "ScreenId")]
	public FormControlSet Screen
	{
		get
		{
			return (FormControlSet)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), new ModelElementKey(this.ScreenId));
		}
		set
		{
			this.ScreenId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
		}
	}
	public Guid EntityId;
	[Category("Screen Reference")]
	[TypeConverter(typeof(ChartFormMappingEntityConverter))]
	[NotNullModelElementRule()]
    [XmlReference("entity", "EntityId")]
	public DataStructureEntity Entity
	{
		get
		{
			return (DataStructureEntity)this.PersistenceProvider.RetrieveInstance(typeof(ISchemaItem), new ModelElementKey(this.EntityId));
		}
		set
		{
			this.EntityId = value == null ? Guid.Empty : (Guid)value.PrimaryKey["Id"];
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
