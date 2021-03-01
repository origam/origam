import {
  IColumnConfiguration,
  ITableConfiguration
} from "model/entities/TablePanelView/types/IConfigurationManager";
import {ITablePanelView} from "model/entities/TablePanelView/types/ITablePanelView";
import {getProperties} from "model/selectors/DataView/getProperties";
import {TableColumnConfiguration} from "model/entities/TablePanelView/tableColumnConfiguration";
import { IProperty } from "../types/IProperty";
import {getGroupingConfiguration} from "model/selectors/TablePanelView/getGroupingConfiguration";
import { observable } from "mobx";

export class TableConfiguration implements ITableConfiguration {

  public static DefaultConfigId = "default";
  public name: string | undefined;
  @observable
  public fixedColumnCount: number = 0;
  public columnConfigurations: IColumnConfiguration[] = [];
  @observable
  public isActive: boolean = false;
  id: string = "";

  private constructor() {
  }

  static create(
    args:{
      name: string | undefined,
      isActive: boolean,
      id: string,
      fixedColumnCount: number,
      columnConfigurations: IColumnConfiguration[]
    }
  ){
    const newInstance = new TableConfiguration();
    newInstance.name = args.name;
    newInstance.id = args.id;
    newInstance.isActive = args.isActive;
    newInstance.fixedColumnCount = args.fixedColumnCount ?? 0;
    newInstance.columnConfigurations = args.columnConfigurations;
    return newInstance;
  }


  static createDefault(properties: IProperty[]){
    const newInstance = new TableConfiguration();
    newInstance.id = this.DefaultConfigId
    newInstance.columnConfigurations = properties
      .map(property => new TableColumnConfiguration(property.id));
    return newInstance;
  }

  public get isGrouping(){
    return this.columnConfigurations.some(columnConfig => columnConfig.groupingIndex > 0);
  }

  deepClone(){
    const newinstance =  new TableConfiguration();
    newinstance.name = this.name;
    newinstance.id = this.id;
    newinstance.fixedColumnCount = this.fixedColumnCount;
    newinstance.columnConfigurations = this.columnConfigurations
      .map(columnConfifuration => columnConfifuration.deepClone());
    return newinstance;
  }

  apply(tablePanelView: ITablePanelView) {
    const properties = getProperties(tablePanelView);

    tablePanelView.fixedColumnCount = this.fixedColumnCount;
    tablePanelView.hiddenPropertyIds.clear();
    tablePanelView.groupingConfiguration.clearGrouping();

    for (const columnConfiguration of this.columnConfigurations) {
      if (!columnConfiguration.isVisible) {
        tablePanelView.setPropertyHidden(columnConfiguration.propertyId, true);
      }
      if (columnConfiguration.aggregationType !== undefined) {
        tablePanelView.aggregations.setType(
          columnConfiguration.propertyId,
          columnConfiguration.aggregationType
        );
      }
      if (columnConfiguration.groupingIndex > 0) {
        tablePanelView.groupingConfiguration.setGrouping(
          columnConfiguration.propertyId,
          columnConfiguration.timeGroupingUnit,
          columnConfiguration.groupingIndex
        );
      }
      const property = properties.find(prop => prop.id === columnConfiguration.propertyId)
      if (property && columnConfiguration.width > 0) {
        property.setColumnWidth(columnConfiguration.width);
      }
      tablePanelView.tablePropertyIds = tablePanelView.tablePropertyIds
        .slice()
        .sort((columnIdA, columnIdB) => {
          const columnIdxA = this.columnConfigurations.findIndex(config => config.propertyId === columnIdA);
          if (columnIdxA === -1) return 0;
          const columnIdxB = this.columnConfigurations.findIndex(config => config.propertyId === columnIdB);
          if (columnIdxB === -1) return 0;
          return columnIdxA - columnIdxB;
        });
    }
  }

  sortColumnConfiguartions(propertyIds: string[]){
    this.columnConfigurations
      .sort((congigA, configB) => {
        const columnIdxA = propertyIds.findIndex(id => id === congigA.propertyId);
        if (columnIdxA === -1) return 0;
        const columnIdxB = propertyIds.findIndex(id => id === configB.propertyId);
        if (columnIdxB === -1) return 0;
        return columnIdxA - columnIdxB;
      });
  }

  updateColumnWidth(propertyId: string, width: number) {
    const columnConfiguration = this.columnConfigurations
      .find(configuration => configuration.propertyId === propertyId);
    if (columnConfiguration) {
      columnConfiguration.width = width;
    }
  }
}