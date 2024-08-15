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

import { ITablePanelView } from "model/entities/TablePanelView/types/ITablePanelView";
import { AggregationType } from "model/entities/types/AggregationType";
import { GroupingUnit } from "model/entities/types/GroupingUnit";
import { Layout } from "model/entities/TablePanelView/layout";

export interface IConfigurationManager {
  onColumnOrderChanged(suppressSave?: boolean): Generator;

  onColumnWidthChanged(id: string, width: number): Generator;

  deleteActiveTableConfiguration(): Promise<any>;

  cloneAndActivate(configuration: ITableConfiguration, newName: string): void;

  getCustomConfiguration(configName: string): string | undefined;

  setCustomConfiguration(configName: string, configuration: string): void;

  activeTableConfiguration: ITableConfiguration;
  customTableConfigurations: ITableConfiguration[];
  defaultTableConfiguration: ITableConfiguration;
  allTableConfigurations: ITableConfiguration[];
  alwaysShowFilters: boolean;
  parent: any;
}

export interface ITableConfiguration {
  id: string;
  isGrouping: boolean;
  name: string | undefined
  fixedColumnCount: number;
  columnConfigurations: IColumnConfiguration[];
  isActive: boolean;
  layout: Layout;

  sortColumnConfigurations(propertyIds: string[]): void;

  updateColumnWidth(propertyId: string, width: number): void;

  apply(tablePanelView: ITablePanelView): void;

  deepClone(): ITableConfiguration;
}

export interface IColumnConfiguration {
  propertyId: string;
  isVisible: boolean;
  groupingIndex: number;
  aggregationType: AggregationType | undefined;
  timeGroupingUnit: GroupingUnit | undefined;
  width: number;

  deepClone(): IColumnConfiguration;
}