export enum IMainMenuItemType {
  FormRef = "FormReferenceMenuItem",
  FormRefWithSelection = "FormReferenceMenuItem_WithSelection",
  WorkflowRef = "WorkflowReferenceMenuItem",
  WorkQueue = "WorkQueue"
}

export interface IMainMenuData {
  menuUI: any;
}

export interface IMainMenuContent extends IMainMenuData {
  $type_IMainMenuContent: 1;
  getItemById(id: string): any;
  parent?: any;
}

export interface IMainMenuEnvelope {
  $type_IMainMenuEnvelope: 1;

  mainMenu?: IMainMenu;
  isLoading: boolean;

  setMainMenu(mainMenu: IMainMenuContent | undefined): void;
  setLoading(state: boolean): void;

  parent?: any;
}

export type IMainMenu = IMainMenuEnvelope & IMainMenuContent;

export const isIMainMenuContent = (o: any): o is IMainMenuContent => o.$type_IMainMenuContent;
export const isIMainMenuEnvelope = (o: any): o is IMainMenuEnvelope =>
  o.$type_IMainMenuEnvelope;

