import { IFormScreen } from "./IFormScreen";

export const COpenedScreen = "COpenedScreen";


export interface IOpenedScreenData {
  menuItemId: string;
  order: number;
  title: string;
  content: IFormScreen;
}

export interface IOpenedScreen extends IOpenedScreenData {
  $type: typeof COpenedScreen;
  isActive: boolean;

  setActive(state: boolean): void;
  setContent(screen: IFormScreen): void;
  parent?: any;
}
