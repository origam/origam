import {
  IColumnConfiguration,
  ITableConfiguration
} from "model/entities/TablePanelView/types/IConfigurationManager";
import {ITablePanelView} from "model/entities/TablePanelView/types/ITablePanelView";
import {getProperties} from "model/selectors/DataView/getProperties";
import {TableColumnConfiguration} from "model/entities/TablePanelView/tableColumnConfiguration";

export class TableConfiguration implements ITableConfiguration {

  public name: string | undefined;
  public fixedColumnCount: number;
  public columnConfiguration: IColumnConfiguration[];
  public tablePropertyIds: string[];
  public isActive: boolean = false;

  constructor(args: {
    name?: string | undefined,
    fixedColumnCount?: number,
    columnConf?: IColumnConfiguration[],
    tablePropertyIds: string[]
  }
  ) {
    this.name = args.name;
    this.fixedColumnCount = args.fixedColumnCount ?? 0;
    this.columnConfiguration = args.columnConf ?? args.tablePropertyIds.map(id => new TableColumnConfiguration(id));
    this.tablePropertyIds = args.tablePropertyIds;
  }

  get sortedColumnConfigurations(){
    return this.columnConfiguration
      .slice()
      .sort((columnConfigA, columnConfigB) => {
        const columnIdxA = this.tablePropertyIds.findIndex((id) => id === columnConfigA.propertyId);
        if (columnIdxA === -1) return 0;
        const columnIdxB = this.tablePropertyIds.findIndex((id) => id === columnConfigB.propertyId);
        if (columnIdxB === -1) return 0;
        return columnIdxA - columnIdxB;
      });
  }

  cloneAs(name: string){
    return new TableConfiguration({
      name: name,
      fixedColumnCount: this.fixedColumnCount,
      columnConf: this.columnConfiguration.map(columnConfifuration => columnConfifuration.clone()),
      tablePropertyIds: [...this.tablePropertyIds]
    });
  }

  apply(tablePanelView: ITablePanelView) {
    const properties = getProperties(tablePanelView);

    for (const columnConfiguration of this.columnConfiguration) {
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
          const columnIdxA = this.tablePropertyIds.findIndex((id) => id === columnIdA);
          if (columnIdxA === -1) return 0;
          const columnIdxB = this.tablePropertyIds.findIndex((id) => id === columnIdB);
          if (columnIdxB === -1) return 0;
          return columnIdxA - columnIdxB;
        });
    }
  }
}