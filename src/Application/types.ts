import { IComponentBindingsModel } from "src/componentBindings/types";

export interface IMainViews {  
  reactMenu: React.ReactNode;
  handleMenuFormItemClick(event: any, id: string, label: string): void;
  activeView: IOpenedView | undefined;
  openedViews: Array<{ order: number; view: IOpenedView }>;
  start(): void;
}

export interface IOpenedView {
  id: string;
  subid: string;
  label: string;
  reactTree: React.ReactNode;
  start(): void;
  stop(): void;
  componentBindingsModel: IComponentBindingsModel;
  unlockLoading(): void;
}

export interface IApplication {
  
}