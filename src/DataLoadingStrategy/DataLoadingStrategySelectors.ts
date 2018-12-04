import { computed } from "mobx";
import {
  IDataLoadingStategyState,
  IDataLoadingStrategySelectors,
  ILoadingGate
} from "./types";
import { IGridSelectors } from "../Grid/types";
import { IDataTableSelectors } from "../DataTable/types";

const LOADING_THRESHOLD = 1000;

export class DataLoadingStrategySelectors
  implements IDataLoadingStrategySelectors {
  constructor(
    public state: IDataLoadingStategyState,
    public gridViewSelectors: IGridSelectors,
    public dataTableSelectors: IDataTableSelectors
  ) {}

  @computed
  get headLoadingActive() {
    return this.state.headLoadingActive;
  }

  @computed
  get tailLoadingActive() {
    return this.state.tailLoadingActive;
  }

  @computed
  get loadingActive() {
    return this.state.loadingActive;
  }

  @computed
  public get loadingGates(): ILoadingGate[] {
    return Array.from(this.state.loadingGates.values());
  }

  @computed
  public get loadingGatesOpen(): boolean {
    for (const gate of this.loadingGates) {
      if (!gate.isLoadingAllowed) {
        return false;
      }
    }
    return true;
  }

  @computed
  get isLoading() {
    return this.state.isLoading;
  }

  @computed
  get distanceToStart() {
    const { visibleRowsFirstIndex } = this.gridViewSelectors;
    return visibleRowsFirstIndex;
  }

  @computed
  get distanceToEnd() {
    const { rowCount, visibleRowsLastIndex } = this.gridViewSelectors;
    return rowCount - visibleRowsLastIndex;
  }

  @computed
  get headLoadingNeeded() {
    return this.distanceToStart <= LOADING_THRESHOLD;
  }

  @computed
  get tailLoadingNeeded() {
    return this.distanceToEnd <= LOADING_THRESHOLD;
  }

  @computed
  get incrementLoadingNeeded() {
    return this.headLoadingNeeded || this.tailLoadingNeeded;
  }

  @computed
  get recordsNeedTrimming() {
    return this.dataTableSelectors.fullRecordCount > 50000;
  }
}
