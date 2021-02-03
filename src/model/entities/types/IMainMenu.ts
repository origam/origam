import { RefObject } from "react";

export enum IMainMenuItemType {
  FormRef = "FormReferenceMenuItem",
  FormRefWithSelection = "FormReferenceMenuItem_WithSelection",
  ReportRefWithSelection = "ReportReferenceMenuItem_WithSelection",
  WorkflowRef = "WorkflowReferenceMenuItem",
  WorkQueue = "WorkQueue",
  ReportReferenceMenuItem = "ReportReferenceMenuItem"
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


export interface IMainMenuState {
  hightLightedItemId: string | undefined;
  highlightItem(itemId: string): void;
  closeAll(): void;
  isOpen(menuId: string): boolean;
  setIsOpen(menuId: string, state: boolean): void;
  flipIsOpen(menuId: string): void;
  setReference(id: string, ref: RefObject<HTMLElement>): void;
  scrollToItem(id: string): void
}

export type IMainMenu = IMainMenuEnvelope & IMainMenuContent;

export const isIMainMenuContent = (o: any): o is IMainMenuContent => o.$type_IMainMenuContent;
export const isIMainMenuEnvelope = (o: any): o is IMainMenuEnvelope =>
  o.$type_IMainMenuEnvelope;

