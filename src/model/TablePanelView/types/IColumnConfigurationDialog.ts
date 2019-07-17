import { ITableColumnsConf } from "../../../gui/Components/Dialogs/ColumnsDialog";
export interface IColumnConfigurationDialog {
  columnsConfiguration: ITableColumnsConf;
  onColumnConfClick(event: any): void;
  onColumnConfCancel(event: any): void;
  onColumnConfSubmit(event: any, configuration: ITableColumnsConf): void;
  parent?: any;
}
