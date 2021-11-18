#region license
/*
Copyright 2005 - 2020 Advantage Solutions, s. r. o.

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
	/// <summary>
	/// Summary description for LookupHelper.
	/// </summary>
	public class LookupHelper
	{
		public static DataServiceDataLookup CreateDataServiceLookup(string name, SchemaItemGroup group, DataStructure listDS, DataStructureFilterSet listFS, string listValueMember, string listDisplayMember, DataStructure valueDS, DataStructureFilterSet valueFS, string valueValueMember, string valueDisplayMember, bool persist)
		{
			ISchemaService schema = ServiceManager.Services.GetService(typeof(ISchemaService)) as ISchemaService;
			DataLookupSchemaItemProvider lookupprovider = schema.GetProvider(typeof(DataLookupSchemaItemProvider)) as DataLookupSchemaItemProvider;

			DataServiceDataLookup lookup = lookupprovider.NewItem(typeof(DataServiceDataLookup), schema.ActiveSchemaExtensionId, null) as DataServiceDataLookup;
			lookup.Name = name;
			lookup.Group = group;
				
			lookup.ListDataStructure = listDS;
			lookup.ListMethod = listFS;
			lookup.ListValueMember = listValueMember;
			lookup.ListDisplayMember = listDisplayMember;

			lookup.ValueDataStructure = valueDS;
			lookup.ValueMethod = valueFS;
			lookup.ValueValueMember = valueValueMember;
			lookup.ValueDisplayMember = valueDisplayMember;

			if(persist) lookup.Persist();

			return lookup;
		}

		public static DataServiceDataLookup CreateDataServiceLookup(string name, 
            IDataEntity fromEntity, IDataEntityColumn idField, 
            IDataEntityColumn nameField, IDataEntityColumn codeField, 
            EntityFilter idFilter, EntityFilter listFilter, 
            string listDisplayMember)
		{
			DataStructure ds = EntityHelper.CreateDataStructure(
                fromEntity, "Lookup" + name, true);
			DataStructureEntity entity = ds.Entities[0] as DataStructureEntity;
			entity.AllFields = false;
			entity.Persist();
			DataStructureColumn idColumn = 
                EntityHelper.CreateDataStructureField(entity, idField, true);
            DataStructureColumn nameColumn;
            if (idField.PrimaryKey.Equals(nameField.PrimaryKey))
            {
                nameColumn = idColumn;
            }
            else
            {
                nameColumn = EntityHelper.CreateDataStructureField(
                    entity, nameField, true);
            }
            if (codeField != null 
                && !codeField.PrimaryKey.Equals(idField.PrimaryKey))
            {
                EntityHelper.CreateDataStructureField(entity, codeField, true);
            }
            // DS Fiter Sets
            DataStructureFilterSet fsId = EntityHelper.CreateFilterSet(
                ds, idFilter.Name, true);
			EntityHelper.CreateFilterSetFilter(fsId, entity, idFilter, true);
			DataStructureFilterSet fsList = null;
			if(listFilter != null)
			{
				fsList = EntityHelper.CreateFilterSet(ds, listFilter.Name, true);
				EntityHelper.CreateFilterSetFilter(fsList, entity, listFilter, true);
			}
			ISchemaService schema = ServiceManager.Services.GetService(
                typeof(ISchemaService)) as ISchemaService;
			DataLookupSchemaItemProvider lookupprovider = 
                schema.GetProvider(typeof(DataLookupSchemaItemProvider)) 
                as DataLookupSchemaItemProvider;
			SchemaItemGroup group = lookupprovider.GetGroup(fromEntity.Group.Name);
			DataServiceDataLookup lookup = CreateDataServiceLookup(name, 
                group, ds, fsList, idColumn.Name, 
                listDisplayMember ?? nameColumn.Name, ds, fsId, 
                idColumn.Name, nameColumn.Name, true);
			return lookup;
		}
	}
}
