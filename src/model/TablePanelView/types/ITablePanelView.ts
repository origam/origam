import { IProperty } from "../../types/IProperty";
import { ITableColumnsConf } from "../../../gui/Components/Dialogs/ColumnsDialog";

export interface ITablePanelViewData {
  tablePropertyIds: string[];
}

export interface ITablePanelView extends ITablePanelViewData {
  $type_ITablePanelView: 1;
  selectedColumnId: string | undefined;
  selectedColumnIndex: number | undefined;
  selectedProperty: IProperty | undefined;
  selectedRowIndex: number | undefined;
  isEditing: boolean;
  isColumnConfigurationDialogVisible: boolean;
  tableProperties: IProperty[];

  columnsConfiguration: ITableColumnsConf;

  getCellValueByIdx(rowIdx: number, columnIdx: number): any;

  onCellClick(rowIndex: number, columnIndex: number): void;
  onNoCellClick(): void;
  onOutsideTableClick(): void;

  onColumnConfClick(event: any): void;
  onColumnConfCancel(event: any): void;
  onColumnConfSubmit(event: any, configuration: ITableColumnsConf): void;

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
