import { IOpenedScreen } from "./IOpenedScreen";

export interface IOpenedScreensData {}

export interface IOpenedScreens extends IOpenedScreensData {
  $type_IOpenedScreens: 1;

  items: Array<IOpenedScreen>;

  pushItem(item: IOpenedScreen): void;
  deleteItem(menuItemId: string, order: number): void;
  activateItem(menuItemId: string, order: number): void;
  findLastExistingItem(menuItemId: string): IOpenedScreen | undefined;
  findClosestItem(menuItemId: string, order: number): IOpenedScreen | undefined;

  parent?: any;
}

export const isIOpenedScreens = (o: any): o is IOpenedScreens =>
  o.$type_IOpenedScreens;
