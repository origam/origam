import { IFormScreen } from "./IFormScreen";

export interface IOpenedScreenData {
  menuItemId: string;
  order: number;
  title: string;
  content: IFormScreen;
}

export interface IOpenedScreen extends IOpenedScreenData {
  $type_IOpenedScreen: 1;

  isActive: boolean;

  setActive(state: boolean): void;
  setContent(screen: IFormScreen): void;
  parent?: any;
}

export const isIOpenedScreen = (o: any): o is IOpenedScreen =>
  o.$type_IOpenedScreen;
