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

import { IColumnConfiguration, ITableConfiguration } from "./IConfigurationManager";
import { GroupingUnit } from "model/entities/types/GroupingUnit";
import { IColumnOptions } from "model/entities/TablePanelView/ColumnConfigurationModel";

export interface IColumnConfigurationModel {
  columnsConfiguration: ITableConfiguration;

  onColumnConfClick(event: any): void;

  onColumnConfCancel(): void;

  onColumnConfSubmit(event: any, configuration: ITableConfiguration): void;

  sortedColumnConfigs: IColumnConfiguration[];

  onColumnConfSubmit(configuration: ITableConfiguration): void

  setVisible(rowIndex: number, state: boolean) :void;

  setGrouping(rowIndex: number, state: boolean, entity: string): void;

  setTimeGroupingUnit(rowIndex: number, groupingUnit: GroupingUnit | undefined): void;

  setAggregation(rowIndex: number, selectedAggregation: any): void;

  handleFixedColumnsCountChange(event: any): void;

  onColumnConfigurationSubmit(): void;

  onSaveAsClick(): void;

  columnOptions: Map<string, IColumnOptions>;

  parent?: any;
}
