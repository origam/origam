export interface IRowStateData {}

export interface IRowState extends IRowStateData {
  $type_IRowState: 1;

  mayCauseFlicker: boolean;

  isWorking: boolean;

  getValue(key: string): IRowStateItem | undefined;
  putValue(state: any): void;
  hasValue(key: string): boolean;
  clearAll(): void;

  parent?: any;
}

export interface IRowStateItem {
  id: string;
  allowCreate: boolean;
  allowDelete: boolean;
  foregroundColor: string | undefined;
  backgroundColor: string | undefined;
  columns: Map<string, IRowStateColumnItem>;
  disabledActions: Set<string>;
}

export interface IRowStateColumnItem {
  name: string;
  dynamicLabel: string | null | undefined;
  foregroundColor: string | undefined;
  backgroundColor: string | undefined;
  allowRead: boolean;
  allowUpdate: boolean;
}

export interface IIdState {}
