import {IFormScreenEnvelope} from "./IFormScreen";
import {IMainMenuItemType} from "./IMainMenu";

export interface IDialogInfo {
  width: number;
  height: number;
}


export interface IOpenedScreenData {
  menuItemId: string;
  menuItemType: IMainMenuItemType;
  dontRequestData: boolean;
  dialogInfo?: IDialogInfo;
  order: number;
  title: string;
  content: IFormScreenEnvelope;
  parameters: { [key: string]: any };
  isSleeping?: boolean;
  isSleepingDirty?: boolean;
}

export interface IOpenedScreen extends IOpenedScreenData {
  parentContext: IOpenedScreen | undefined;
  $type_IOpenedScreen: 1;

  isActive: boolean;
  isDialog: boolean;
  stackPosition: number;

  setActive(state: boolean): void;
  setContent(screen: IFormScreenEnvelope): void;
  screenUrl?: string;
  parent?: any;
  hasDynamicTitle: boolean;
}

export const isIOpenedScreen = (o: any): o is IOpenedScreen =>
  o.$type_IOpenedScreen;
