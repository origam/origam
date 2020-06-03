import { IAction } from "model/entities/types/IAction";
import { IDataView } from "./IDataView";
import {IAggregationInfo} from "./IAggregationInfo";
import {IOrdering} from "./IOrderingConfiguration";
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

  onWorkflowNextClick(event: any): Generator;
  onWorkflowAbortClick(event: any): Generator;
  onWorkflowRepeatClick(event: any): Generator;
  onWorkflowCloseClick(event: any): Generator;

  killForm(): void;
  start(initUIResult: any): Generator;

  loadGroups(rootDataView: IDataView, groupBy: string, groupByLookupId: string | undefined, aggregations: IAggregationInfo[] | undefined):  Promise<any[]>;
  loadChildGroups(rootDataView: IDataView, filter: string, groupByColumn: string,
                  aggregations: IAggregationInfo[] | undefined, lookupId: string | undefined): Promise<any[]>;
  loadChildRows(rootDataView: IDataView, filter: string, ordering: IOrdering | undefined): Promise<any[]>;
  loadAggregations(rootDataView: IDataView, aggregations: IAggregationInfo[]): Promise<any[]> ;
  parent?: any;

  registerDisposer(disposer: ()=>void): void;
}

export const isIFormScreenLifecycle = (o: any): o is IFormScreenLifecycle =>
  o.$type_IFormScreenLifecycle;
