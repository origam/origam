import { IFilterConfiguration } from "../../types/IFilterConfiguration";
import { IProperty } from "../../types/IProperty";
import { IColumnConfigurationDialog } from "./IColumnConfigurationDialog";
import { IOrderingConfiguration } from "model/entities/types/IOrderingConfiguration";
import { IGroupingConfiguration } from "model/entities/types/IGroupingConfiguration";
import {AggregationContainer} from "../TablePanelView";

export interface ITablePanelViewData {
  tablePropertyIds: string[];
  columnConfigurationDialog: IColumnConfigurationDialog;
  filterConfiguration: IFilterConfiguration;
  orderingConfiguration: IOrderingConfiguration;
  groupingConfiguration: IGroupingConfiguration;
}

export interface ITableCanvas {
  firstVisibleRowIndex: number;
  lastVisibleRowIndex: number;
}

export interface ITablePanelView extends ITablePanelViewData {
  $type_ITablePanelView: 1;
  selectedColumnId: string | undefined;
  selectedColumnIndex: number | undefined;
  selectedProperty: IProperty | undefined;
  selectedRowIndex: number | undefined;
  isEditing: boolean;
  fixedColumnCount: number;

  tableCanvas: ITableCanvas | null;
  setTableCanvas(tableCanvas: ITableCanvas | null): void;
  firstVisibleRowIndex: number;
  lastVisibleRowIndex: number;

  tableProperties: IProperty[];
  allTableProperties: IProperty[];
  propertyMap: Map<string, IProperty>;

  hiddenPropertyIds: Map<string, boolean>;
  aggregations: AggregationContainer;

  getCellValueByIdx(rowIdx: number, columnIdx: number): any;
  getCellTextByIdx(rowIdx: number, columnIdx: number): any;

  onCellClick(event: any, row: any[], columnId: string): Generator;
  onNoCellClick(): Generator;
  onOutsideTableClick(): Generator;

  setEditing(state: boolean): void;
  selectNextColumn(nextRowWhenEnd?: boolean): void;
  selectPrevColumn(prevRowWhenStart?: boolean): void;

  setSelectedColumnId(id: string | undefined): void;
  swapColumns(id1: string, id2: string): void;

  columnOrderChangingTargetId: string | undefined;
  columnOrderChangingSourceId: string | undefined;
  setColumnOrderChangeAttendants(
    idSource: string | undefined,
    idTarget: string | undefined
  ): void;

  subOnScrollToCellShortest(
    fn: (rowIdx: number, columnIdx: number) => void
  ): () => void;
  subOnFocusTable(fn: () => void): () => void;

  scrollToCurrentCell(): void;
  triggerOnFocusTable(): void;
  triggerOnScrollToCellShortest(rowIdx: number, columnIdx: number): void;
  setPropertyHidden(propertyId: string, state: boolean): void;

  parent?: any;
}
