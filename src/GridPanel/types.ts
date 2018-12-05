import {
  IGridView,
  IGridSetup,
  IGridTopology,
  IGridCursorView,
  IGridInteractionActions,
  IFormView,
  IFormSetup,
  GridViewType,
  IGridInteractionSelectors,
  IFormTopology,
  IFormActions
} from "../Grid/types";
import { IDataLoadingStrategyActions, IDataLoadingStrategySelectors, IDataLoader } from "../DataLoadingStrategy/types";
import { IEventSubscriber } from "../utils/events";
import { IGridOrderingSelectors, IGridOrderingActions } from "src/GridOrdering/types";
import { IDataTableSelectors, IDataTableActions } from "src/DataTable/types";

export interface IGridToolbarView {
  activeView: GridViewType;
  handleAddRecordClick(event: any): void;
  handleRemoveRecordClick(event: any): void;
  handleSetGridViewClick(event: any): void;
  handleSetFormViewClick(event: any): void;
  handlePrevRecordClick(event: any): void
  handleNextRecordClick(event: any): void
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
  dataLoadingStrategySelectors: IDataLoadingStrategySelectors;
  gridOrderingSelectors: IGridOrderingSelectors;
  gridOrderingActions: IGridOrderingActions;
  dataTableSelectors: IDataTableSelectors;
  dataTableActions: IDataTableActions;
  dataLoader: IDataLoader;

  dataStructureEntityId: string;
  modelInstanceId: string;

  formView: IFormView;
  formSetup: IFormSetup;
  formTopology: IFormTopology;
  formActions: IFormActions;
}
