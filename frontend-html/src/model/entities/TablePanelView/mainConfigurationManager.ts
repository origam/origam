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

import { IConfigurationManager, ITableConfiguration } from "model/entities/TablePanelView/types/IConfigurationManager";
import { isMobileLayoutActive } from "model/selectors/isMobileLayoutActive";
import { TableConfiguration } from "model/entities/TablePanelView/tableConfiguration";
import {
  saveColumnConfigurations,
  saveColumnConfigurationsAsync
} from "model/actions/DataView/TableView/saveColumnConfigurations";

export class MainConfigurationManager implements IConfigurationManager {

  get defaultTableConfiguration(): ITableConfiguration {
    return this.activeManager.defaultTableConfiguration;
  }

  set defaultTableConfiguration(value: ITableConfiguration) {
    this.activeManager.defaultTableConfiguration = value;
  }

  get customTableConfigurations(): ITableConfiguration[] {
    return this.activeManager.customTableConfigurations;
  }

  set customTableConfigurations(value: ITableConfiguration[]) {
    this.activeManager.customTableConfigurations = value;
  }

  constructor(
    private desktopConfigurationManager: IConfigurationManager,
    private mobileConfigurationManager: IConfigurationManager,
  ) {
    desktopConfigurationManager.parent = this;
    mobileConfigurationManager.parent = this;
  }

  get alwaysShowFilters() {
    return this.activeManager.alwaysShowFilters;
  }

  set alwaysShowFilters(value: boolean) {
    this.activeManager.alwaysShowFilters = value;
  }

  get activeManager() {
    return isMobileLayoutActive(this)
      ? this.mobileConfigurationManager
      : this.desktopConfigurationManager;
  }

  get allTableConfigurations() {
    return [
      ...this.desktopConfigurationManager.allTableConfigurations,
      ...this.mobileConfigurationManager.allTableConfigurations
    ];
  }

  get activeTableConfiguration() {
    return this.activeManager.activeTableConfiguration;
  }

  set activeTableConfiguration(configToActivate: TableConfiguration) {
    this.activeManager.activeTableConfiguration = configToActivate;
  }

  cloneAndActivate(configuration: ITableConfiguration, newName: string): void {
    this.activeManager.cloneAndActivate(configuration, newName);
  }

  getCustomConfiguration(configName: string) {
    return this.desktopConfigurationManager.getCustomConfiguration(configName);
  }

  setCustomConfiguration(configName: string, configuration: string) {
    this.desktopConfigurationManager.setCustomConfiguration(configName, configuration);
  }

  async deleteActiveTableConfiguration(): Promise<any> {
    await this.activeManager.deleteActiveTableConfiguration();
    await saveColumnConfigurationsAsync(this);
  }

  *onColumnWidthChanged(propertyId: string, width: number): Generator {
    yield*this.activeManager.onColumnWidthChanged(propertyId, width)
    yield*saveColumnConfigurations(this)();
  }

  *onColumnOrderChanged(suppressSave?: boolean): Generator {
    yield*this.activeManager.onColumnOrderChanged(suppressSave);
    if(!suppressSave)yield*saveColumnConfigurations(this)();
  }

  parent: any;
}