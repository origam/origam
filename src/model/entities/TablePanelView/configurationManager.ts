import {IConfigurationManager, ITableColumnsConf} from "model/entities/TablePanelView/types/IConfigurationManager";
import {TableConfiguration} from "model/entities/TablePanelView/tableConfiguration";

export class ConfigurationManager implements IConfigurationManager {
  constructor(
    public tableConfigurations: TableConfiguration[],
    public defaultTableConfiguration: TableConfiguration
  ) {
  }

  setAsCurrent(configToActivate: TableConfiguration): void {
    for (const tableConfiguration of this.tableConfigurations) {
      tableConfiguration.isActive = false;
    }
    this.defaultTableConfiguration.isActive = false;

    configToActivate.isActive = true;
  }
}