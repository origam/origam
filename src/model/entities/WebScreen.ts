import {IReloader, IWebScreen} from "./types/IWebScreen";
import {IOpenedScreen} from "./types/IOpenedScreen";
import {action, observable} from "mobx";
import {IFormScreenEnvelope} from "./types/IFormScreen";
import {IMainMenuItemType} from "./types/IMainMenu";

export class WebScreen implements IWebScreen, IOpenedScreen {
  $type_IOpenedScreen: 1 = 1;
  $type_IWebScreen: 1 = 1;
  parentSessionId: string | undefined;

  isBeingClosed = false;

  constructor(
    title: string,
    public screenUrl: string,
    public menuItemId: string,
    public order: number
  ) {
    this.tabTitle = title;
    this.formTitle = title;
  }

  reloader: IReloader | null = null;
  @observable stackPosition: number = 0;
  @observable tabTitle = "";
  @observable formTitle = "";
  @observable isActive = false;
  isDialog = false;
  isClosed = false;

  @action.bound
  setActive(state: boolean): void {
    this.isActive = state;
  }

  setContent(screen: IFormScreenEnvelope): void {}

  setTitle(title: string): void {
    this.tabTitle = title;
  }

  setReloader(reloader: IReloader | null): void {
    this.reloader = reloader;
  }

  reload() {
    this.reloader && this.reloader.reload();
  }

  parent?: any;

  menuItemType: IMainMenuItemType = null as any;

  lazyLoading = false;
  dialogInfo = undefined;
  content: IFormScreenEnvelope = null as any;
  parameters: { [key: string]: any } = {};
  hasDynamicTitle: boolean = false;
  parentContext: IOpenedScreen | undefined;
}
