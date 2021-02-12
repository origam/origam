import { IProperty } from "./types/IProperty";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getDataView } from "model/selectors/DataView/getDataView";
import { isInfiniteScrollLoader } from "gui/Workbench/ScreenArea/TableView/InfiniteScrollLoader";
import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import { getGrouper } from "model/selectors/DataView/getGrouper";
import {getApi} from "model/selectors/getApi";
import {getMenuItemId} from "model/selectors/getMenuItemId";
import {getSessionId} from "model/selectors/getSessionId";
import {getDataStructureEntityId} from "model/selectors/DataView/getDataStructureEntityId";
import {isLazyLoading} from "model/selectors/isLazyLoading";

export function* getAllLookupIds(property: IProperty): Generator {
  const dataView = getDataView(property);
  if (isLazyLoading(dataView)) {
    return yield getAllValuesOfProp(dataView, property);
  }
  else {
    const dataTable = getDataTable(property);
    return yield dataTable.getAllValuesOfProp(property);
  }
}

async function getAllValuesOfProp(ctx: any, property: IProperty): Promise<Set<any>> {
  const api = getApi(ctx);
  const listValues = await api.getFilterListValues({
    MenuId: getMenuItemId(ctx),
    SessionFormIdentifier: getSessionId(ctx),
    DataStructureEntityId: getDataStructureEntityId(ctx),
    Property: property.id
  });
  return new Set(
    listValues
      .map(listValue => listValue)
      .filter(listValue => listValue)
  );
}
