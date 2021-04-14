import {IProperty} from "model/entities/types/IProperty";
import {ConfigurationManager} from "model/entities/TablePanelView/configurationManager";
import {findStopping} from "xmlInterpreters/xmlUtils";
import {tryParseAggregationType} from "model/entities/types/AggregationType";
import {fixColumnWidth} from "xmlInterpreters/screenXml";
import {TableConfiguration} from "model/entities/TablePanelView/tableConfiguration";
import {TableColumnConfiguration} from "model/entities/TablePanelView/tableColumnConfiguration";

function makeColumnConfigurations(properties: IProperty[], tableConfigNode: any) {
  return properties.map(property => {
        const configElement = tableConfigNode.elements
            .find((element: any) => element.attributes?.propertyId === property.id)
        return configElement
            ? parseColumnConfigurationNode(configElement, property)
            : new TableColumnConfiguration(property.id);
      }
  );
}

export function createConfigurationManager(configurationNodes: any, properties: IProperty[]) {
  const defaultConfiguration = TableConfiguration.createDefault(properties);
  if (configurationNodes.length === 0) {
    return new ConfigurationManager(
      [], defaultConfiguration);
  } else if (configurationNodes.length > 1) {
    throw new Error("Can not process more than one configuration node")
  }

  const tableConfigurationNodes = findStopping(configurationNodes[0], (n) => n.name === "tableConfigurations")?.[0]?.elements;
  if(!tableConfigurationNodes){
    return new ConfigurationManager(
      [], defaultConfiguration);
  }
  const tableConfigurations: TableConfiguration[] = tableConfigurationNodes.map((tableConfigNode: any) => {
    return TableConfiguration.create(
      {
        name: tableConfigNode.attributes.name,
        id: tableConfigNode.attributes.id,
        isActive: tableConfigNode.attributes.isActive === "true",
        fixedColumnCount: parseIntOrZero(tableConfigNode.attributes.fixedColumnCount),
        columnConfigurations: makeColumnConfigurations(properties, tableConfigNode),
      }
      )
    }
  );

  const defaultTableConfiguration = tableConfigurations.find(tableConfig => tableConfig.name === "")
          ?? defaultConfiguration;

   const noConfigIsActive = tableConfigurations.every(tableConfig => !tableConfig.isActive);
   if(noConfigIsActive){
     defaultTableConfiguration.isActive = true;
   }

  return new ConfigurationManager(
    tableConfigurations
      .filter((tableConfig: TableConfiguration) => tableConfig !== defaultTableConfiguration),
      defaultTableConfiguration
  );
}

function parseColumnConfigurationNode(columnConfigNode: any, property: IProperty) {
  const tableConfiguration = new TableColumnConfiguration(property.id);
  tableConfiguration.width = fixColumnWidth(parseInt(columnConfigNode.attributes.width));

  if (columnConfigNode.attributes.isVisible === "false" || tableConfiguration.width < 0) {
    tableConfiguration.isVisible = false;
  }
  tableConfiguration.aggregationType = tryParseAggregationType(columnConfigNode.attributes.aggregationType);

  if (!property?.isLookupColumn) {
    tableConfiguration.groupingIndex = parseIntOrZero(columnConfigNode.attributes.groupingIndex);
    tableConfiguration.timeGroupingUnit = isNaN(parseInt(columnConfigNode.attributes.groupingUnit))
      ? undefined
      : parseInt(columnConfigNode.attributes.groupingUnit)
  }
  return tableConfiguration;
}

function parseIntOrZero(value: string): number {
  const intValue = parseInt(value, 10);
  return isNaN(intValue) ? 0 : intValue;
}