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
    return yield grouper.getAllValuesOfProp(property);
  }

  else {
    if (isInfiniteScrollLoader(dataView.infiniteScrollLoader)) {
      return yield dataView.infiniteScrollLoader.getAllValuesOfProp(property);
    }

    else {
      const dataTable = getDataTable(property);
      return yield dataTable.getAllValuesOfProp(property);
    }
  }
}
