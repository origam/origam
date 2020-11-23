import { IProperty } from "./types/IProperty";
import { getDataTable } from "model/selectors/DataView/getDataTable";
import { getDataView } from "model/selectors/DataView/getDataView";
import { isInfiniteScrollLoader } from "gui/Workbench/ScreenArea/TableView/InfiniteScrollLoader";
import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import { getGrouper } from "model/selectors/DataView/getGrouper";

export function* getAllLookupIds(property: IProperty): Generator {
  const dataView = getDataView(property);
  if (getGroupingConfiguration(dataView).isGrouping) {
    const grouper = getGrouper(dataView);
    const newLocal = yield grouper.getAllValuesOfProp(property);
    return newLocal;
  }

  else {
    if (isInfiniteScrollLoader(dataView.infiniteScrollLoader)) {
      yield dataView.infiniteScrollLoader.getAllValuesOfProp(property);
    }

    else {
      const dataTable = getDataTable(property);
      yield Array.from(new Set(dataTable.getAllValuesOfProp(property)).values());
    }
  }
}
