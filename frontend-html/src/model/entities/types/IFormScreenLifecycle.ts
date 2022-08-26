import {IAction} from "model/entities/types/IAction";
import {IDataView} from "./IDataView";
import {IAggregationInfo} from "./IAggregationInfo";
import {IOrdering} from "./IOrderingConfiguration";
import { IGroupingSettings } from "./IGroupingConfiguration";
import { UpdateRequestAggregator } from "model/entities/FormScreenLifecycle/UpdateRequestAggregator";

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

  onRequestScreenClose(isDueToError?: boolean): void;

  start(): void;
  parent?: any;
}

export interface IFormScreenLifecycle02 extends IFormScreenLifecycleData {
  focusedDataViewId: string | undefined;
  $type_IFormScreenLifecycle: 1;

  isWorkingDelayed: boolean;
  isWorking: boolean;

  updateRequestAggregator: UpdateRequestAggregator;
  rowSelectedReactionsDisabled(dataView: IDataView): boolean;

  onFlushData(): Generator;
  throwChangesAway(dataView: IDataView): Generator;
  onCreateRow(entity: string, gridId: string): Generator;
  onDeleteRow(entity: string, rowId: string, dataView: IDataView): Generator;
  updateRadioButtonValue(dataView: IDataView, row: any, fieldName: string, newValue: string): Generator;

  onSaveSession(): Generator;
  onRequestScreenReload(): Generator;

  refreshSession(): Generator;
  loadInitialData(): void;
  onExecuteAction(
    gridId: string,
    entity: string,
    action: IAction,
    selectedItems: string[]
  ): Generator;

  onRequestScreenClose(isDueToError?: boolean): Generator;

  clearAutoRefreshInterval(): void;

  onWorkflowNextClick(event: any): Generator;
  onWorkflowAbortClick(event: any): Generator;
  onWorkflowRepeatClick(event: any): Generator;
  onWorkflowCloseClick(event: any): Generator;

  killForm(): void;
  start(initUIResult: any): Generator;

  loadGroups(rootDataView: IDataView, columnSettings: IGroupingSettings, groupByLookupId: string | undefined, aggregations: IAggregationInfo[] | undefined):  Promise<any[]>;
  loadChildGroups(rootDataView: IDataView, filter: string, groupingSettings: IGroupingSettings,
                  aggregations: IAggregationInfo[] | undefined, lookupId: string | undefined): Promise<any[]>;
  loadChildRows(rootDataView: IDataView, filter: string, ordering: IOrdering | undefined): Promise<any[]>;
  loadAggregations(rootDataView: IDataView, aggregations: IAggregationInfo[]): Promise<any[]> ;
  parent?: any;

  registerDisposer(disposer: ()=>void): void;

  onCopyRow(entity: any, gridId: string, rowId: string): any;
}

export const isIFormScreenLifecycle = (o: any): o is IFormScreenLifecycle =>
  o.$type_IFormScreenLifecycle;
