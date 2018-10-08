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
  }): Promise<any>;
  loadLookup(table: string, label: string, ids: string[]): Promise<any>;
}
