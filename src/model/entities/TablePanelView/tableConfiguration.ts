import {
  ITableColumnConf,
  ITableColumnsConf
} from "model/entities/TablePanelView/types/IConfigurationManager";
import {ITablePanelView} from "model/entities/TablePanelView/types/ITablePanelView";
import {getProperties} from "model/selectors/DataView/getProperties";
import {TableColumnConfiguration} from "model/entities/TablePanelView/tableColumnConfiguration";

export class TableConfiguration implements ITableColumnsConf {

  public name: string | undefined;
  public fixedColumnCount: number;
  public columnConf: ITableColumnConf[];
  public tablePropertyIds: string[];
  public isActive: boolean = false;

  constructor(args: {
    name?: string | undefined,
    fixedColumnCount?: number,
    columnConf?: ITableColumnConf[],
    tablePropertyIds: string[]
  }
  ) {
    this.name = args.name;
    this.fixedColumnCount = args.fixedColumnCount ?? 0;
    this.columnConf = args.columnConf ?? args.tablePropertyIds.map(id => new TableColumnConfiguration(id));
    this.tablePropertyIds = args.tablePropertyIds;
  }

  cloneAs(name: string){
    return new TableConfiguration({
      name: name,
      fixedColumnCount: this.fixedColumnCount,
      columnConf: this.columnConf.map(columnConfifuration => columnConfifuration.clone()),
      tablePropertyIds: [...this.tablePropertyIds]
    });
  }

  apply(tablePanelView: ITablePanelView) {
    const properties = getProperties(tablePanelView);

    for (const columnConfiguration of this.columnConf) {
      if (!columnConfiguration.isVisible) {
        tablePanelView.setPropertyHidden(columnConfiguration.id, true);
      }
      if (columnConfiguration.aggregationType !== undefined) {
        tablePanelView.aggregations.setType(
          columnConfiguration.id,
          columnConfiguration.aggregationType
        );
      }
      if (columnConfiguration.groupingIndex > 0) {
        tablePanelView.groupingConfiguration.setGrouping(
          columnConfiguration.id,
          columnConfiguration.timeGroupingUnit,
          columnConfiguration.groupingIndex
        );
      }
      const property = properties.find(prop => prop.id === columnConfiguration.id)
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