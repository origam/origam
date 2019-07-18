import { IProperty } from "../../types/IProperty";
import { ITableColumnsConf } from "../../../gui/Components/Dialogs/ColumnsDialog";
import { IColumnConfigurationDialog } from "./IColumnConfigurationDialog";

export interface ITablePanelViewData {
  tablePropertyIds: string[];
  columnConfigurationDialog: IColumnConfigurationDialog;
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

  hiddenPropertyIds: Map<string, boolean>;
  groupingIndices: Map<string, number>;

  getCellValueByIdx(rowIdx: number, columnIdx: number): any;
  getCellTextByIdx(rowIdx: number, columnIdx: number): any;

  onCellClick(rowIndex: number, columnIndex: number): void;
  onNoCellClick(): void;
  onOutsideTableClick(): void;

  setSelectedColumnId(id: string | undefined): void;
  swapColumns(id1: string, id2: string): void;

  columnOrderChangingTargetId: string | undefined;
  columnOrderChangingSourceId: string | undefined;
  setColumnOrderChangeAttendants(
    idSource: string | undefined,
    idTarget: string | undefined
  ): void;

  parent?: any;
}
