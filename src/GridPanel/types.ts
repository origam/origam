import {
  IGridView,
  IGridSetup,
  IGridTopology,
  IGridCursorView,
  IGridInteractionActions
} from "../Grid/types";
import { IDataLoadingStrategyActions } from "../DataLoadingStrategy/types";
import { IEventSubscriber } from "../utils/events";

export interface IGridToolbarView {
  handleAddRecordClick(event: any): void;
  handleRemoveRecordClick(event: any): void;
}

export interface IGridPanelBacking {
  gridToolbarView: IGridToolbarView;
  gridView: IGridView;
  gridSetup: IGridSetup;
  gridTopology: IGridTopology;
  gridCursorView: IGridCursorView;
  gridInteractionActions: IGridInteractionActions;
  onStartGrid: IEventSubscriber;
  onStopGrid: IEventSubscriber;
  dataLoadingStrategyActions: IDataLoadingStrategyActions;
}
