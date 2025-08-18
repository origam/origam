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
import { ICustomConfiguration, TableConfiguration } from "model/entities/TablePanelView/tableConfiguration";
import { observable } from "mobx";
import { v4 as uuidv4 } from 'uuid';
import { getTablePanelView } from "model/selectors/TablePanelView/getTablePanelView";
import { getFormScreenLifecycle } from "model/selectors/FormScreen/getFormScreenLifecycle";
import { Layout } from "model/entities/TablePanelView/layout";

export class ConfigurationManager implements IConfigurationManager {
  parent: any;

  @observable.shallow
  customTableConfigurations: TableConfiguration[];

  @observable.shallow
  defaultTableConfiguration: TableConfiguration;

  customConfigurationsMap: Map<string, string>;

  constructor(
    customTableConfigurations: TableConfiguration[],
    defaultTableConfiguration: TableConfiguration,
    customConfigurations: ICustomConfiguration[],
    private layout: Layout // for debugging
  ) {
    this.defaultTableConfiguration = defaultTableConfiguration;
    this.customTableConfigurations = customTableConfigurations;
    this.customConfigurationsMap = new Map(customConfigurations.map(i => [i.name, i.value]));
  }

  get allTableConfigurations() {
    return this.defaultTableConfiguration
      ? [this.defaultTableConfiguration, ...this.customTableConfigurations]
      : this.customTableConfigurations;
  }

  get activeTableConfiguration() {
    const activeTableConfiguration = this.allTableConfigurations.find(config => config.isActive)
    if (activeTableConfiguration) {
      return activeTableConfiguration;
    } else {
      this.defaultTableConfiguration.isActive = true;
      return this.defaultTableConfiguration;
    }
  }

  set activeTableConfiguration(configToActivate: TableConfiguration) {
    const groupingWasActive = this.activeTableConfiguration.isGrouping;
    this.replace(configToActivate);

    for (const tableConfiguration of this.allTableConfigurations) {
      tableConfiguration.isActive = false;
    }
    configToActivate.isActive = true;
    const tablePanelView = getTablePanelView(this);
    configToActivate.apply(tablePanelView);
    if (groupingWasActive !== configToActivate.isGrouping) {
      getFormScreenLifecycle(this).loadInitialData();
    }
  }

  private replace(newConfiguration: TableConfiguration) {
    const index = this.customTableConfigurations
      .findIndex(config => config.id === newConfiguration.id);
    if (index > -1) {
      this.customTableConfigurations[index] = newConfiguration;
    } else {
      if (newConfiguration.id === TableConfiguration.DefaultConfigId) {
        this.defaultTableConfiguration = newConfiguration;
      } else {
        this.customTableConfigurations.push(newConfiguration);
      }
    }
  }

  cloneAndActivate(configuration: ITableConfiguration, newName: string): void {
    const newConfig = configuration.deepClone();
    newConfig.name = newName;
    newConfig.id = uuidv4();
    this.customTableConfigurations.push(newConfig);
    this.activeTableConfiguration = newConfig;
  }

  getCustomConfiguration(configName: string) {
    return this.customConfigurationsMap.get(configName);
  }

  setCustomConfiguration(configName: string, configuration: string){
    this.customConfigurationsMap.set(configName, configuration);
  }

  async deleteActiveTableConfiguration(): Promise<any> {
    if (this.defaultTableConfiguration.isActive) {
      return;
    }
    this.customTableConfigurations.remove(this.activeTableConfiguration);
    this.activeTableConfiguration = this.defaultTableConfiguration;
  }

  *onColumnWidthChanged(propertyId: string, width: number): Generator {
    if (!this.defaultTableConfiguration.isActive) {
      return;
    }
    this.activeTableConfiguration.updateColumnWidth(propertyId, width);
  }

  *onColumnOrderChanged() {
    if (!this.defaultTableConfiguration.isActive) {
      return;
    }
    const tablePanelView = getTablePanelView(this);
    this.activeTableConfiguration.sortColumnConfigurations(tablePanelView.tablePropertyIds);
  }
}

