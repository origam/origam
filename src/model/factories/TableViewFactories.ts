import { CellRenderer } from "gui/Components/ScreenElements/Table/CellDrivers/CellRenderer";
import { DataRowColumnDriversCollection } from "gui/Components/ScreenElements/Table/CellDrivers/DataRowColumnDriversCollection";
import { DataRowDriver } from "gui/Components/ScreenElements/Table/CellDrivers/DataRowDriver";
import { GroupHeaderColumnDriversCollection } from "gui/Components/ScreenElements/Table/CellDrivers/GroupHeaderColumnDriversCollection";
import { GroupHeaderRowDriver } from "gui/Components/ScreenElements/Table/CellDrivers/GroupHeaderRowDriver";
import { TableViewInfo } from "gui/Components/ScreenElements/Table/CellDrivers/TableViewInfo";
import { TableRowsTransform } from "model/entities/TablePanelView/TableRowsTransform";
import { getIsSelectionCheckboxesShown } from "model/selectors/DataView/getIsSelectionCheckboxesShown";
import { getGroupingConfiguration } from "model/selectors/TablePanelView/getGroupingConfiguration";
import { getTableViewProperties } from "model/selectors/TablePanelView/getTableViewProperties";
import { ColumnDriverFactories } from "./ColumnDriverFactories";

export function createCellRenderer(ctx: any) {
  const tableRowsTransform = new TableRowsTransform(0 as any, 0 as any);

  const tableViewInfo = new TableViewInfo(
    getGroupingConfiguration(ctx),
    () => getIsSelectionCheckboxesShown(ctx),
    () => getTableViewProperties(ctx),
    () => getTableViewProperties(ctx).length
  );

  const columnDriverFactories = new ColumnDriverFactories();

  const dataRowColumnDriversCollection = new DataRowColumnDriversCollection(
    tableViewInfo,
    columnDriverFactories
  );
  const groupHeaderRowColumnDriversCollection = new GroupHeaderColumnDriversCollection(
    tableViewInfo,
    columnDriverFactories
  );

  const dataRowDriver = new DataRowDriver(() => dataRowColumnDriversCollection.drivers);
  const groupHeaderRowDriver = new GroupHeaderRowDriver(
    () => groupHeaderRowColumnDriversCollection.drivers
  );

  const cellRenderer = new CellRenderer(
    [dataRowDriver, groupHeaderRowDriver],
    (index) => tableRowsTransform.tableRows[index]
  );

  return cellRenderer;
}
