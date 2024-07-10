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
using Origam.UI;
using Origam.Workbench.Services;
using Origam.Services;

namespace Origam.Schema.EntityModel.UI.Wizards;
public class LocalizeDatastructureCommand : AbstractMenuCommand
{
	public override bool IsEnabled
	{
		get
		{
            DataStructure ds = Owner as DataStructure;
			return ds != null && !ds.IsLocalized && ds.LocalizableEntities.Count > 0;
		}
		set
		{
			throw new ArgumentException(ResourceUtils.GetString("ErrorSetProperty"), "IsEnabled");
		}
	}
	public override void Run()
	{
		DataStructure ds = Owner as DataStructure;
		ds.IsLocalized = true;
		// find all entities in datastructure and create language relations for them if they are localized
		// (all fields = true)
		foreach (DataStructureEntity dsEntity in ds.LocalizableEntities)
		{
			TableMappingItem table = dsEntity.Entity as TableMappingItem;
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
			// create new datastructure entity using localization relation
			DataStructureEntity localizationDSEntity 
				= dsEntity.NewItem<DataStructureEntity>(
					schema.ActiveSchemaExtensionId, null);
			localizationDSEntity.RelationType = RelationType.LeftJoin;
			localizationDSEntity.AllFields = false;
			localizationDSEntity.Entity = table.LocalizationRelation;
			localizationDSEntity.Persist();
            GeneratedModelElements.Add(localizationDSEntity);
		}
		ds.Persist();
	}		
}
