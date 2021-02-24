import {IConfigurationManager, ITableConfiguration} from "model/entities/TablePanelView/types/IConfigurationManager";
import {TableConfiguration} from "model/entities/TablePanelView/tableConfiguration";

export class ConfigurationManager implements IConfigurationManager {
  constructor(
    public customTableConfigurations: TableConfiguration[],
    public defaultTableConfiguration: TableConfiguration
  ) {
  }

  get allTableConfigurations(){
    return this.defaultTableConfiguration
      ? [this.defaultTableConfiguration, ...this.customTableConfigurations]
      : this.customTableConfigurations;
  }

  setAsCurrent(configToActivate: TableConfiguration): void {
    for (const tableConfiguration of this.allTableConfigurations) {
      tableConfiguration.isActive = false;
    }
    configToActivate.isActive = true;
  }
}