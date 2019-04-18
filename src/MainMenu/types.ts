import { ICommandType } from "./MainMenu";

export interface IMainMenu {
  setItems(items: any): void;
  resetItems(): void;
  items: any;
}

export interface IAOnItemClick {
  do(
    event: any,
    id: string,
    itemType: ICommandType,
    menuItemLabel: string
  ): void;
}

export interface IMainMenuFactory {
  create(menuObj: any): IMainMenu;
}
