import { IGridPanelBacking } from "../GridPanel/types";
export interface IComponentBindingsModel {
  registerGridPaneBacking(backing: IGridPanelBacking): void;
  start(): void;
  stop(): void;
}
