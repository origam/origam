import { IOpenedScreen } from "./IOpenedScreen";

export interface IOpenedScreensData {}

export interface IOpenedScreens extends IOpenedScreensData {
  items: Array<IOpenedScreen>;

  pushItem(item: IOpenedScreen): void;
  deleteItem(menuItemId: string, order: number): void;
  activateItem(menuItemId: string, order: number): void;
  findLastExistingItem(menuItemId: string): IOpenedScreen | undefined;
  findClosestItem(menuItemId: string, order: number): IOpenedScreen | undefined;

  parent?: any;
}
