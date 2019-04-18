import { ICommandType } from "../MainMenu/MainMenu";
import { ML } from "../utils/types";
import { IApi } from "../Api/IApi";

export interface IAActivateView {
  do(id: string, order: number): void;
}

export interface IACloseView {
  do(id: string, order: number): void;
}

export interface IAOpenView {
  do(id: string, itemType: ICommandType, menuItemLabel: string): void;
}

export interface IAActivateOrOpenView {
  do(id: string, itemType: ICommandType, menuItemLabel: string): void;
}

export interface IMainViewFactory {
  create(
    menuItemId: string,
    order: number,
    itemType: ICommandType,
    menuItemLabel: string
  ): IMainView;
}

export interface IAOnHandleClick {
  do(event: any, menuItemId: string, order: number): void;
}

export interface IAOnCloseClick {
  do(event: any, menuItemId: string, order: number): void;
}

export interface IMainViews {
  activeViewId: string | undefined;
  activeViewOrder: number | undefined;
  activeView: IMainView | undefined;
  openedViews: IMainView[];
  findView(id: string, order: number): IMainView | undefined;
  findFirstById(id: string): IMainView | undefined;
  findLastById(id: string): IMainView | undefined;
  findClosest(id: string, order: number): IMainView | undefined;
  isActiveView(view: IMainView): boolean;
  pushView(view: IMainView): void;
  deleteView(id: string, order: number): void;
  activateView(id: string, order: number): void;
}

export interface IMainView {
  menuItemId: string;
  order: number;
  label: string;
  activate(): void;
  deactivate(): void;
  open(): void;
  close(): void;
}

export enum IScreenType {
  FormRef = "FormReferenceMenuItem",
  FormRefWithSelection = "FormReferenceMenuItem_WithSelection",
  WorkflowRef = "WorkflowReferenceMenuItem"
}

export interface IDataSources {
  getByEntityName(name: string): IDataSource | undefined;
  getDataSourceEntityIdByEntityName(name: string): string | undefined;
}

export interface IDataSource {
  id: string;
  dataStructureEntityId: string;
  fieldById(name: string): IDataSourceField | undefined;
  reorderedIds(ids: string[]): string[];
}

export interface IDataSourceField {
  id: string;
  idx: number;
}
