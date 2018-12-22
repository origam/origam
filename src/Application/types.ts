import { IComponentBindingsModel } from "src/componentBindings/types";

export interface IMainViews {  
  
  activeView: IOpenedView | undefined;
  openedViews: IOpenedView[];
  start(): void;

  // TODO: Move direct event handlers to some dedicated View class?
  handleMenuFormItemClick(event: any, id: string, label: string): void;
  closeView(id: string, subid: string): void;
  activateView(id: string, subid: string): void;
}

export interface IOpenedView {
  id: string;
  subid: string;
  label: string;
  reactTree: React.ReactNode;
  componentBindingsModel: IComponentBindingsModel;
  isActive: boolean;
  order: number;
  
  start(): void;
  stop(): void;
  unlockLoading(): void;
}

export interface IOpenedViewCollection {
  isActiveView(view: IOpenedView): boolean;
  getViewOrder(view: IOpenedView): number;
}

export interface IApplication {
  
}