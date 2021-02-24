import {IProperty} from "model/entities/types/IProperty";
import {ITableColumnConf} from "model/entities/TablePanelView/types/IConfigurationManager";
import {ConfigurationManager} from "model/entities/TablePanelView/configurationManager";
import {findStopping} from "xmlInterpreters/xmlUtils";
import {parseAggregationType} from "model/entities/types/AggregationType";
import {fixColumnWidth} from "xmlInterpreters/screenXml";
import {TableConfiguration} from "model/entities/TablePanelView/tableConfiguration";
import {TableColumnConfiguration} from "model/entities/TablePanelView/tableColumnConfiguration";

export function createConfigurationManager(configurationNodes: any, properties: IProperty[]) {
  const columnConfigurations: ITableColumnConf[] = [];

  function getColumnConfiguration(id: string) {
    let configIndex = columnConfigurations.findIndex(config => config.id === id);
    if (configIndex === -1) {
      columnConfigurations.unshift(new TableColumnConfiguration(id));
      configIndex = 0;
    }
    return columnConfigurations[configIndex];
  }

  if (configurationNodes.length === 0) {
    return new ConfigurationManager([], new TableConfiguration({
      tablePropertyIds: properties.map(prop => prop.id)
    }));
  } else if (configurationNodes.length > 1) {
    throw new Error("Can not process more than one configuration node")
  }
  let groupingColumnCounter = 1;

  let fixedColumnCount = 0;
  const configurationNode = configurationNodes[0];
  const defaultFixedColumns = findStopping(configurationNode, (n) => n.name === "lockedColumns");
  if (defaultFixedColumns && defaultFixedColumns.length > 0) {
    const fixedColumnsNode = findStopping(
      defaultFixedColumns?.[0],
      (n) => n.name === "lockedColumns"
    );
    const fixedColumnsStr = fixedColumnsNode?.[0]?.attributes?.["count"];
    const fixedColumnsInt = parseInt(fixedColumnsStr, 10);
    if (!isNaN(fixedColumnsStr)) {
      fixedColumnCount = fixedColumnsInt;
    }
  }

  const columnWidthsNodes = findStopping(configurationNode, (n) => n.name === "columnWidths");
  if (columnWidthsNodes.length === 0) {
    return new ConfigurationManager([], new TableConfiguration({
      tablePropertyIds: properties.map(prop => prop.id)
    }));
  }
  const columns = findStopping(columnWidthsNodes[0], (n) => n.name === "column");
  for (const column of columns) {
    if (column.attributes.property) {
      const prop = properties.find((prop) => prop.id === column.attributes.property);
      if (!prop) {
        continue;
      }
      const confuguration = getColumnConfiguration(column.attributes.property)

      // COLUMN WIDTH
      confuguration.width = fixColumnWidth(parseInt(column.attributes.width));

      // COLUMN HIDING
      if (column.attributes.isHidden === "true" || confuguration.width < 0) {
        confuguration.isVisible = false;
      }
      if (column.attributes.aggregationType !== "0") {
        confuguration.aggregationType = parseAggregationType(column.attributes.aggregationType);
      }
    } else if (column.attributes.groupingField) {
      const property = properties.find(
        (prop) => prop.id === column.attributes.groupingField
      );
      if (!property) {
        continue;
      }
      const confuguration = getColumnConfiguration(column.attributes.groupingField)
      if (!property?.isLookupColumn) {
        confuguration.groupingIndex = groupingColumnCounter;
        confuguration.timeGroupingUnit = isNaN(parseInt(column.attributes.groupingUnit))
          ? undefined
          : parseInt(column.attributes.groupingUnit)
        groupingColumnCounter++;
      }
    }
  }
  ;

  const tablePropertyIds = columns.map(columnNode => columnNode.attributes.property);
  return new ConfigurationManager(
    [],
    new TableConfiguration({
      name: "",
      fixedColumnCount: fixedColumnCount,
      columnConf: columnConfigurations,
      tablePropertyIds: tablePropertyIds
    })
  );
}