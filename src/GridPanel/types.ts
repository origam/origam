import {
  IGridView,
  IGridSetup,
  IGridTopology,
  IGridCursorView,
  IGridInteractionActions,
  IFormView,
  IFormSetup,
  IGridPaneView,
  IGridInteractionSelectors
} from "../Grid/types";
import { IDataLoadingStrategyActions } from "../DataLoadingStrategy/types";
import { IEventSubscriber } from "../utils/events";

export interface IGridToolbarView {
  activeView: IGridPaneView;
  handleAddRecordClick(event: any): void;
  handleRemoveRecordClick(event: any): void;
  handleSetGridViewClick(event: any): void;
  handleSetFormViewClick(event: any): void;
}

export interface IGridPanelBacking {
  gridToolbarView: IGridToolbarView;
  gridView: IGridView;
  gridSetup: IGridSetup;
  gridTopology: IGridTopology;
  gridCursorView: IGridCursorView;
  gridInteractionActions: IGridInteractionActions;
  gridInteractionSelectors: IGridInteractionSelectors;
  onStartGrid: IEventSubscriber;
  onStopGrid: IEventSubscriber;
  dataLoadingStrategyActions: IDataLoadingStrategyActions;

  formView: IFormView;
  formSetup: IFormSetup;
}
