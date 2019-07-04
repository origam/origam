export const CLoadedOpenedScreen = "CLoadedOpenedScreen";
export const CLoadingOpenedScreen = "CLoadingOpenedScreen";

export interface IOpenedScreenData {
  menuItemId: string;
  order: number;
  title: string;
}

export interface IOpenedScreen extends IOpenedScreenData {
  $type: typeof CLoadedOpenedScreen;
  isActive: boolean;

  setActive(state: boolean): void;
  parent?: any;
}
