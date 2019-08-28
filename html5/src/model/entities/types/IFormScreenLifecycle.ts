import { IAction } from "model/entities/types/IAction";
export interface IFormScreenLifecycleData {}

export interface IFormScreenLifecycle extends IFormScreenLifecycleData {
  $type_IFormScreenLifecycle: 1;

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

  run(): void;
  parent?: any;
}

export const isIFormScreenLifecycle = (o: any): o is IFormScreenLifecycle =>
  o.$type_IFormScreenLifecycle;
