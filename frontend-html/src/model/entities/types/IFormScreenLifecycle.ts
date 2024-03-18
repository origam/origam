/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/

import { IAction } from "model/entities/types/IAction";
import { IDataView } from "./IDataView";
import { IAggregationInfo } from "./IAggregationInfo";
import { IOrdering } from "./IOrderingConfiguration";
import { IGroupingSettings } from "./IGroupingConfiguration";
import { UpdateRequestAggregator } from "model/entities/FormScreenLifecycle/UpdateRequestAggregator";

export interface IFormScreenLifecycleData {
}

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
  onClose: (() => void) | undefined;
  focusedDataViewId: string | undefined;
  $type_IFormScreenLifecycle: 1;

  isWorkingDelayed: boolean;
  isWorking: boolean;

  workflowNextActive: number;
  workflowAbortActive: number;

  updateRequestAggregator: UpdateRequestAggregator;
  rowSelectedReactionsDisabled(dataView: IDataView): boolean;

  onFlushData(): Generator;

  throwChangesAway(dataView: IDataView): Generator;

  onCreateRow(entity: string, gridId: string): Generator;

  onDeleteRow(entity: string, rowId: string, dataView: IDataView, doNotAskForConfirmation?: boolean): Generator;

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

  updateTotalRowCount(dataView: IDataView): Promise<any>;

  onRequestScreenClose(closeWithoutSaving?: boolean): Generator;

  clearAutoRefreshInterval(): void;

  onWorkflowNextClick(event: any): Generator;

  onWorkflowAbortClick(event: any): Generator;

  onWorkflowRepeatClick(event: any): Generator;

  onWorkflowCloseClick(event: any): Generator;

  killForm(): void;

  closeForm(): Generator;

  start(args:{initUIResult: any, preloadIsDirty?: boolean, createNewRecord?: boolean}): Generator;

  loadGroups(rootDataView: IDataView, columnSettings: IGroupingSettings, groupByLookupId: string | undefined, aggregations: IAggregationInfo[] | undefined): Promise<any[]>;

  loadChildGroups(rootDataView: IDataView, filter: string, groupingSettings: IGroupingSettings,
                  aggregations: IAggregationInfo[] | undefined, lookupId: string | undefined): Promise<any[]>;

  loadChildRows(rootDataView: IDataView, filter: string, ordering: IOrdering | undefined): Promise<any[]>;

  loadAggregations(rootDataView: IDataView, aggregations: IAggregationInfo[]): Promise<any[]>;

  parent?: any;

  registerDisposer(disposer: () => void): void;

  onCopyRow(entity: any, gridId: string, rowId: string): any;
}

export const isIFormScreenLifecycle = (o: any): o is IFormScreenLifecycle =>
  o.$type_IFormScreenLifecycle;
