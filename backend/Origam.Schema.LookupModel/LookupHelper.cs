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

using Origam.Services;
using Origam.Schema.EntityModel;
using Origam.Workbench.Services;

namespace Origam.Schema.LookupModel
{
	public class LookupHelper
	{
		public static DataServiceDataLookup CreateDataServiceLookup(
			string name, 
			SchemaItemGroup group, 
			DataStructure listDataStructure, 
			DataStructureFilterSet listFilterSet, 
			string listValueMember, 
			string listDisplayMember, 
			DataStructure valueDataStructure, 
			DataStructureFilterSet valueFilterSet, 
			string valueValueMember, 
			string valueDisplayMember, 
			bool persist)
		{
			var schemaService 
				= ServiceManager.Services.GetService<ISchemaService>();
			var dataLookupSchemaItemProvider 
				= schemaService.GetProvider<DataLookupSchemaItemProvider>();
			var dataServiceDataLookup = dataLookupSchemaItemProvider
				.NewItem<DataServiceDataLookup>(
					schemaService.ActiveSchemaExtensionId, null);
			dataServiceDataLookup.Name = name;
			dataServiceDataLookup.Group = group;
			dataServiceDataLookup.ListDataStructure = listDataStructure;
			dataServiceDataLookup.ListMethod = listFilterSet;
			dataServiceDataLookup.ListValueMember = listValueMember;
			dataServiceDataLookup.ListDisplayMember = listDisplayMember;
			dataServiceDataLookup.ValueDataStructure = valueDataStructure;
			dataServiceDataLookup.ValueMethod = valueFilterSet;
			dataServiceDataLookup.ValueValueMember = valueValueMember;
			dataServiceDataLookup.ValueDisplayMember = valueDisplayMember;
			if(persist)
			{
				dataServiceDataLookup.Persist();
			}
			return dataServiceDataLookup;
		}

		public static DataServiceDataLookup CreateDataServiceLookup(
			string name, 
            IDataEntity fromEntity, 
			IDataEntityColumn idField, 
            IDataEntityColumn nameField, 
			IDataEntityColumn codeField, 
            EntityFilter idFilter, 
			EntityFilter listFilter, 
            string listDisplayMember)
		{
			var dataStructure = EntityHelper.CreateDataStructure(
                fromEntity, "Lookup" + name, true);
			var dataStructureEntity 
				= dataStructure.Entities[0] as DataStructureEntity;
			dataStructureEntity.AllFields = false;
			dataStructureEntity.Persist();
			var idColumn = EntityHelper.CreateDataStructureField(
				dataStructureEntity, idField, true);
            DataStructureColumn nameColumn;
            if(idField.PrimaryKey.Equals(nameField.PrimaryKey))
            {
                nameColumn = idColumn;
            }
            else
            {
                nameColumn = EntityHelper.CreateDataStructureField(
                    dataStructureEntity, nameField, true);
            }
            if ((codeField != null)
			&& !codeField.PrimaryKey.Equals(idField.PrimaryKey))
            {
                EntityHelper.CreateDataStructureField(
	                dataStructureEntity, codeField, true);
            }
            // DS Filter Sets
            var dataStructureFilterSetId = EntityHelper.CreateFilterSet(
                dataStructure, idFilter.Name, true);
			EntityHelper.CreateFilterSetFilter(
				dataStructureFilterSetId, dataStructureEntity, idFilter, true);
			DataStructureFilterSet dataStructureFilterSetList = null;
			if(listFilter != null)
			{
				dataStructureFilterSetList = EntityHelper.CreateFilterSet(
					dataStructure, listFilter.Name, true);
				EntityHelper.CreateFilterSetFilter(
					dataStructureFilterSetList, dataStructureEntity, 
					listFilter, true);
			}
			var schemaService 
				= ServiceManager.Services.GetService<ISchemaService>();
			var dataLookupSchemaItemProvider 
					= schemaService.GetProvider<DataLookupSchemaItemProvider>();
			var schemaItemGroup = dataLookupSchemaItemProvider.GetGroup(
				fromEntity.Group.Name);
			var dataServiceLookup = CreateDataServiceLookup(name, 
                schemaItemGroup, dataStructure, dataStructureFilterSetList, 
                idColumn.Name, listDisplayMember ?? nameColumn.Name, 
                dataStructure, dataStructureFilterSetId, idColumn.Name, 
                nameColumn.Name, true);
			return dataServiceLookup;
		}
	}
}
