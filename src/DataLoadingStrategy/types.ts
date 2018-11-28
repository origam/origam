import { IDataTableRecord, IRecordId } from "src/DataTable/types";

export interface ILookupResolverDR {
  dataLoader: IDataLoader;
}

export interface IDataLoadingStategyState {
  headLoadingActive: boolean;
  tailLoadingActive: boolean;
  loadingActive: boolean;
  isLoading: boolean;

  setHeadLoadingActive(state: boolean): void;
  setTailLoadingActive(state: boolean): void;
  setLoadingActive(state: boolean): void;
  setLoading(state: boolean): void;
}

export interface IDataLoadingStrategyActions {
  requestLoadFresh(): Promise<any>;
  reloadRow(id: IRecordId): Promise<any>;
  setLoadingActive(state: boolean): void;
}

export interface IDataLoadingStrategySelectors {
  headLoadingNeeded: boolean;
  tailLoadingNeeded: boolean;
  loadingActive: boolean;
  isLoading: boolean;
  incrementLoadingNeeded: boolean;
  headLoadingActive: boolean;
  tailLoadingActive: boolean;
  recordsNeedTrimming: boolean;
}

export interface IDataLoader {
  loadDataTable(args: {
    limit?: number;
    orderBy?: Array<[string, string]>;
    filter?: any;
    columns?: string[];
  }): Promise<any>;
  loadLookup(table: string, label: string, ids: string[]): Promise<any>;
}

export interface IDataSaver {
  updateRecord(record: IDataTableRecord): Promise<any>;
  deleteRecord(recordId: IRecordId): Promise<any>;
  createRecord(record: IDataTableRecord): Promise<any>;
}

export interface IAPI {
  loadDataTable({
    tableId,
    columns,
    limit,
    filter,
    orderBy,
    token,
    menuId
  }: {
    tableId: string;
    token: string;
    columns?: string[] | undefined;
    limit?: number | undefined;
    filter?: Array<[string, string, string]> | undefined;
    orderBy?: Array<[string, string]> | undefined;
    menuId: string;
  }): Promise<any>;
  loadMenu({ token }: { token: string }): Promise<any>;
  loadScreen({ id, token }: { id: string; token: string }): Promise<any>;
}
