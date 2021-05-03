import { IProperty } from "./types/IProperty";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getDataView } from "model/selectors/DataView/getDataView";
import {getApi} from "model/selectors/getApi";
import {getMenuItemId} from "model/selectors/getMenuItemId";
import {getSessionId} from "model/selectors/getSessionId";
import {getDataStructureEntityId} from "model/selectors/DataView/getDataStructureEntityId";
import {isLazyLoading} from "model/selectors/isLazyLoading";
import {getUserFilters} from "model/selectors/DataView/getUserFilters";
import {getUserFilterLookups} from "model/selectors/DataView/getUserFilterLookups";

export function* getAllLookupIds(property: IProperty): Generator {
  const dataView = getDataView(property);
  if (dataView.isLazyLoading) {
      return yield getAllValuesOfProp(property);
  }
  else {
    const dataTable = getDataTable(property);
    return yield dataTable.getAllValuesOfProp(property);
  }
}

async function getAllValuesOfProp(property: IProperty): Promise<Set<any>> {
  const api = getApi(property);
  const listValues = await api.getFilterListValues({
    MenuId: getMenuItemId(property),
    SessionFormIdentifier: getSessionId(property),
    DataStructureEntityId: getDataStructureEntityId(property),
    Property: property.id,
    Filter: getUserFilters({ctx: property, excludePropertyId: property.id}),
    FilterLookups: getUserFilterLookups(property),
  });
  return new Set(
    listValues
      .map(listValue => listValue)
      .filter(listValue => listValue)
  );
}
