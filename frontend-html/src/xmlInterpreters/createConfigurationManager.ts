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

import { IProperty } from "model/entities/types/IProperty";
import { ConfigurationManager} from "model/entities/TablePanelView/configurationManager";
import { findStopping } from "xmlInterpreters/xmlUtils";
import { tryParseAggregationType } from "model/entities/types/AggregationType";
import { fixColumnWidth } from "xmlInterpreters/screenXml";
import { ICustomConfiguration, TableConfiguration } from "model/entities/TablePanelView/tableConfiguration";
import { TableColumnConfiguration } from "model/entities/TablePanelView/tableColumnConfiguration";
import { Layout } from "model/entities/TablePanelView/layout";
import { MainConfigurationManager } from "model/entities/TablePanelView/mainConfigurationManager";

function makeColumnConfigurations(properties: IProperty[], tableConfigNode: any, isLazyLoading: boolean) {
  const columnConfigurations: TableColumnConfiguration[] = tableConfigNode.elements
    .map((element: any) => {
      if (!element.attributes?.propertyId) {
        return undefined;
      }
      const property = properties.find(prop => prop.id === element.attributes?.propertyId);
      if (!property) {
        return undefined;
      }
      return parseColumnConfigurationNode(element, property, isLazyLoading)
    })
    .filter((columnConfig: any) => columnConfig);
  const parsedColumnConfigurationIds = columnConfigurations.map(columnConfig => columnConfig.propertyId);

  const newColumnConfigurations = properties
    .filter(property => !parsedColumnConfigurationIds.includes(property.id))
    .map(property => {
      const columnConfig = new TableColumnConfiguration(property.id, property.columnWidth);
      columnConfig.isVisible = property.gridColumnWidth >= 0;
      return columnConfig;
    });
  return columnConfigurations.concat(newColumnConfigurations);
}

export function createConfigurationManager(configurationNodes: any, properties: IProperty[], isLazyLoading: boolean) {

  const desktopConfigurationManager = createConfigurationManagerForLayout(
    configurationNodes, properties, isLazyLoading, Layout.Desktop
  );
  const mobileConfigurationManager = createConfigurationManagerForLayout(
    configurationNodes, properties, isLazyLoading, Layout.Mobile
  );
  return new MainConfigurationManager(desktopConfigurationManager, mobileConfigurationManager);
}

export function createConfigurationManagerForLayout(configurationNodes: any, properties: IProperty[], isLazyLoading: boolean, layout: Layout) {
  const defaultConfiguration = TableConfiguration.createDefault(properties, layout);
  if (configurationNodes.length === 0) {
    return new ConfigurationManager(
      [], defaultConfiguration, [],false,  layout);
  } else if (configurationNodes.length > 1) {
    throw new Error("Can not process more than one configuration node")
  }

  const tableConfigurationNodes = findStopping(configurationNodes[0], (n) => n.name === "tableConfigurations")?.[0]?.elements;
  const customConfigurations = layout === Layout.Desktop
    ? parseCustomConfigurations(configurationNodes[0])
    : [];
  const alwaysShowFilters = findStopping(configurationNodes[0], (n) => n.name === "alwaysShowFilters")?.[0]?.elements[0]?.text === 'true';
  if (!tableConfigurationNodes) {
    return new ConfigurationManager(
      [], defaultConfiguration, customConfigurations, alwaysShowFilters, layout);
  }
  const tableConfigurations: TableConfiguration[] = tableConfigurationNodes
    .map((tableConfigNode: any) => {
        return TableConfiguration.create(
          {
            name: tableConfigNode.attributes.name,
            id: tableConfigNode.attributes.id,
            isActive: tableConfigNode.attributes.isActive === "true",
            fixedColumnCount: parseIntOrZero(tableConfigNode.attributes.fixedColumnCount),
            columnConfigurations: makeColumnConfigurations(properties, tableConfigNode, isLazyLoading),
            layout: tableConfigNode.attributes.layout
          }
        )
      }
    )
    .filter((tabConfig: TableConfiguration) => tabConfig.layout === layout);

  const defaultTableConfiguration = tableConfigurations.find(tableConfig => tableConfig.name === "")
    ?? defaultConfiguration;

  const noConfigIsActive = tableConfigurations.every(tableConfig => !tableConfig.isActive);
  if (noConfigIsActive) {
    defaultTableConfiguration.isActive = true;
  }

  return new ConfigurationManager(
    tableConfigurations
      .filter((tableConfig: TableConfiguration) => tableConfig !== defaultTableConfiguration),
    defaultTableConfiguration,
    customConfigurations,
    alwaysShowFilters,
    layout
  );
}

function parseColumnConfigurationNode(columnConfigNode: any, property: IProperty, isLazyLoading: boolean) {
  const tableConfiguration = new TableColumnConfiguration(property.id, property.columnWidth);
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

function parseCustomConfigurations(configurationNode: any): ICustomConfiguration[] {
  const customConfigurationNodes = findStopping(configurationNode, (n) => n.name === "customConfigurations")
    ?.[0]?.elements
    ?.[0]?.elements;
  if (!customConfigurationNodes) {
    return [];
  }
  return customConfigurationNodes
    .map((customConfigNode: any) => {
      const config = customConfigNode.elements[0]?.text ?? "";
      const decodedConfig = decodeURIComponent(encodeURIComponent(window.atob(config)))
      return{
        name: customConfigNode.name.replace("Configuration", ""),
        value: decodedConfig
      }
    });
}