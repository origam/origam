import { IAction } from "model/entities/types/IAction";
import { IDataView } from "./IDataView";
export interface IFormScreenLifecycleData {}

export interface IFormScreenLifecycle extends IFormScreenLifecycleData {
  $type_IFormScreenLifecycle: 1;

  isWorking: boolean;

  onFlushData(): void;
  onCreateRow(entity: string, gridId: string): void;
  onDeleteRow(entity: string, rowId: string): void;

  onSaveSession(): void;
  onRefreshSession(): void;

  onExecuteAction(
    gridId: string,
    entity: string,
    action: IAction,
    selectedItems: string[]
  ): Promise<any>;

  onRequestScreenClose(): void;

  start(): void;
  parent?: any;
}

export interface IFormScreenLifecycle02 extends IFormScreenLifecycleData {
  $type_IFormScreenLifecycle: 1;

  isWorking: boolean;

  onFlushData(): Generator;
  onCreateRow(entity: string, gridId: string): Generator;
  onDeleteRow(entity: string, rowId: string): Generator;

  onSaveSession(): Generator;
  onRequestScreenReload(): Generator;

  refreshSession(): Generator;

  onExecuteAction(
    gridId: string,
    entity: string,
    action: IAction,
    selectedItems: string[]
  ): Generator;

  onRequestScreenClose(): Generator;

  clearAutorefreshInterval(): void;

  start(initUIResult: any): Generator;

  loadGroups(rootDataView: IDataView, groupBy: string):  Promise<any[]>;
  loadChildGroups(rootDataView: IDataView, filter: string, groupByColumn: string): Promise<any[]>;
  loadChildRows(rootDataView: IDataView, filter: string): Promise<any[]>;

  parent?: any;
}

export const isIFormScreenLifecycle = (o: any): o is IFormScreenLifecycle =>
  o.$type_IFormScreenLifecycle;
