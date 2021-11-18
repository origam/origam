import { ITableConfiguration } from "./IConfigurationManager";

export interface IColumnConfigurationDialog {
  columnsConfiguration: ITableConfiguration;
  onColumnConfClick(event: any): void;
  onColumnConfCancel(event: any): void;
  onColumnConfSubmit(event: any, configuration: ITableConfiguration): void;
  parent?: any;
}
