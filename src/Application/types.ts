import { IComponentBindingsModel } from "src/componentBindings/types";

export interface IMainViews {  
  
  activeView: IMainView | undefined;
  openedViews: IMainView[];
  start(): void;

  // TODO: Move direct event handlers to some dedicated View class?
  handleMenuFormItemClick(event: any, id: string, label: string): void;
  closeView(id: string, subid: string): void;
  activateView(id: string, subid: string): void;
}

export interface IMainView {
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

export interface IMainViewsCollection {
  isActiveView(view: IMainView): boolean;
  getViewOrder(view: IMainView): number;
}

export interface IApplication {
  
}