import { IFieldId } from "src/DataTable/types";

export interface IGridOrderingSelectors {
  ordering: Array<[string, string]>;
  getColumnOrdering(columnId: string): {order: number | undefined, direction: string | undefined};
  cycleDirection(oldOrdering: string | undefined): string | undefined;
}

export interface IGridOrderingActions {
  cycleOrderByExclusive(columnId: string): void
  cycleOrderByPreserving(columnId: string): void;
  setOrdering(columnId: string, direction: string | undefined): void;
}

export interface IGridOrderingState {
  ordering: Array<[string, string]>;
  clearOrdering(): void;
  setOrdering(columnId: string, newOrdering: string | undefined): void;
}