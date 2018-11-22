import { IDataTableRecord, IRecordId } from "src/DataTable/types";

export interface ILookupResolverDR {
  dataLoader: IDataLoader;
}

export interface IDataLoadingStategyState {
  headLoadingActive: boolean;
  tailLoadingActive: boolean;

  setHeadLoadingActive(state: boolean): void;
  setTailLoadingActive(state: boolean): void;
}

export interface IDataLoadingStrategyActions {
  requestLoadFresh(): Promise<any>;
}

export interface IDataLoadingStrategySelectors {
  headLoadingNeeded: boolean;
  tailLoadingNeeded: boolean;
  incrementLoadingNeeded: boolean;
  headLoadingActive: boolean;
  tailLoadingActive: boolean;
  recordsNeedTrimming: boolean;
}

export interface IDataLoader {
  loadDataTable(args: {
    limit?: number;
    orderBy?: Array<[string, string]>;
    filter?: Array<[string, string, string]>;
    columns?: string[];
  }): Promise<any>;
  loadLookup(table: string, label: string, ids: string[]): Promise<any>;
}

export interface IDataSaver {
  updateRecords(records: IDataTableRecord[]): Promise<IDataTableRecord[]>;
  deleteRecords(recordIds: IRecordId[]): Promise<any>;
  createRecords(records: IDataTableRecord[]): Promise<IDataTableRecord[]>;
}

export interface IAPI {
  loadDataTable({
    tableId,
    columns,
    limit,
    filter,
    orderBy,
    token
  }: {
    tableId: string;
    token: string;
    columns?: string[] | undefined;
    limit?: number | undefined;
    filter?: Array<[string, string, string]> | undefined;
    orderBy?: Array<[string, string]> | undefined;
  }): Promise<any>;
  loadMenu({ token }: { token: string }): Promise<any>;
  loadScreen({ id, token }: { id: string; token: string }): Promise<any>;
}
