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

import {IProperty} from "model/entities/types/IProperty";
import {ConfigurationManager} from "model/entities/TablePanelView/configurationManager";
import {findStopping} from "xmlInterpreters/xmlUtils";
import {tryParseAggregationType} from "model/entities/types/AggregationType";
import {fixColumnWidth} from "xmlInterpreters/screenXml";
import {TableConfiguration} from "model/entities/TablePanelView/tableConfiguration";
import {TableColumnConfiguration} from "model/entities/TablePanelView/tableColumnConfiguration";

function makeColumnConfigurations(properties: IProperty[], tableConfigNode: any, isLazyLoading: boolean) {
  const columnConfigurations: TableColumnConfiguration[] = tableConfigNode.elements
    .map((element: any) => {
      if(!element.attributes?.propertyId){
        return undefined;
      }
      const property = properties.find(prop => prop.id === element.attributes?.propertyId);
      if(!property){
        return undefined;
      }
      return parseColumnConfigurationNode(element, property, isLazyLoading)
    })
    .filter((columnConfig: any) => columnConfig);
  const parsedColumnConfigurationIds = columnConfigurations.map(columnConfig => columnConfig.propertyId);

  const newColumnConfigurations = properties
    .filter(property => !parsedColumnConfigurationIds.includes(property.id))
    .map(property => new TableColumnConfiguration(property.id));
  return columnConfigurations.concat(newColumnConfigurations);
}

export function createConfigurationManager(configurationNodes: any, properties: IProperty[], isLazyLoading: boolean) {
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
        columnConfigurations: makeColumnConfigurations(properties, tableConfigNode, isLazyLoading),
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

function parseColumnConfigurationNode(columnConfigNode: any, property: IProperty, isLazyLoading: boolean) {
  const tableConfiguration = new TableColumnConfiguration(property.id);
  tableConfiguration.width = fixColumnWidth(parseInt(columnConfigNode.attributes.width));

  if (columnConfigNode.attributes.isVisible === "false" || tableConfiguration.width < 0) {
    tableConfiguration.isVisible = false;
  }
  tableConfiguration.aggregationType = tryParseAggregationType(columnConfigNode.attributes.aggregationType);

  // It is possible that the configuration will contain grouping by detached field if the same screen is referenced by two
  // menu items. One opens it as lazy loaded the other as eager loaded.
  if (!isLazyLoading || property.fieldType !== "DetachedField") {
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