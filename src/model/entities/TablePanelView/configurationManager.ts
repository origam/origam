import {IConfigurationManager, ITableConfiguration} from "model/entities/TablePanelView/types/IConfigurationManager";
import {TableConfiguration} from "model/entities/TablePanelView/tableConfiguration";
import {runGeneratorInFlowWithHandler} from "utils/runInFlowWithHandler";
import {saveColumnConfigurations} from "model/actions/DataView/TableView/saveColumnConfigurations";

export class ConfigurationManager implements IConfigurationManager {
  parent: any;

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

  get activeTableConfiguration(){
    const activeTableConfiguration = this.allTableConfigurations.find(config => config.isActive)
    if (activeTableConfiguration) {
      return activeTableConfiguration;
    } else {
      this.defaultTableConfiguration.isActive = true;
      return this.defaultTableConfiguration;
    }
  }

  set activeTableConfiguration(configToActivate: TableConfiguration) {
    for (const tableConfiguration of this.allTableConfigurations) {
      tableConfiguration.isActive = false;
    }
    configToActivate.isActive = true;
  }

  cloneAndActivate(configuration: ITableConfiguration, newName: string): void {
    const newConfig = configuration.cloneAs(newName);
    this.customTableConfigurations.push(newConfig);
    this.activeTableConfiguration = newConfig;
  }

  async deleteActiveTableConfiguration(): Promise<any> {
    if(this.defaultTableConfiguration.isActive){
      return;
    }
    this.customTableConfigurations.remove(this.activeTableConfiguration);
    this.defaultTableConfiguration.isActive = true;
    await this.saveTableConfigurations();
  }

  async saveTableConfigurations(): Promise<any> {
    const self = this;
    await runGeneratorInFlowWithHandler({
      ctx: this,
      generator: function* (){
        yield* saveColumnConfigurations(self)();
      }()
    })
  }
}