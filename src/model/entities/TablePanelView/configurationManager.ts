import {IConfigurationManager} from "model/entities/TablePanelView/types/IConfigurationManager";
import {TableConfiguration} from "model/entities/TablePanelView/tableConfiguration";

export class ConfigurationManager implements IConfigurationManager {
  constructor(
    public tableConfigurations: TableConfiguration[],
    public defaultTableConfiguration: TableConfiguration
  ) {
  }
}