import {IConfigurationManager, ITableConfiguration} from "model/entities/TablePanelView/types/IConfigurationManager";
import {TableConfiguration} from "model/entities/TablePanelView/tableConfiguration";
import {runGeneratorInFlowWithHandler} from "utils/runInFlowWithHandler";
import {saveColumnConfigurations} from "model/actions/DataView/TableView/saveColumnConfigurations";
import { observable } from "mobx";
import {uuidv4} from "utils/uuid";
import {getTablePanelView} from "model/selectors/TablePanelView/getTablePanelView";
import {getFormScreenLifecycle} from "model/selectors/FormScreen/getFormScreenLifecycle";

export class ConfigurationManager implements IConfigurationManager {
  parent: any;

  @observable.shallow
  customTableConfigurations: TableConfiguration[];

  @observable.shallow
  defaultTableConfiguration: TableConfiguration;

  constructor(
    customTableConfigurations: TableConfiguration[],
    defaultTableConfiguration: TableConfiguration
  ) {
    this.defaultTableConfiguration = defaultTableConfiguration;
    this.customTableConfigurations = customTableConfigurations;
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
    this.replace(configToActivate);

    for (const tableConfiguration of this.allTableConfigurations) {
      tableConfiguration.isActive = false;
    }
    configToActivate.isActive = true;
    const tablePanelView = getTablePanelView(this);
    configToActivate.apply(tablePanelView);
    getFormScreenLifecycle(this).loadInitialData();
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