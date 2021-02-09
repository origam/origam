import {IReloader, IWebScreen} from "./types/IWebScreen";
import {IOpenedScreen} from "./types/IOpenedScreen";
import {action, observable} from "mobx";
import {IFormScreenEnvelope} from "./types/IFormScreen";
import {IMainMenuItemType} from "./types/IMainMenu";

export class WebScreen implements IWebScreen, IOpenedScreen {
  $type_IOpenedScreen: 1 = 1;
  $type_IWebScreen: 1 = 1;
  parentSessionId: string | undefined;

  constructor(
    title: string,
    public screenUrl: string,
    public menuItemId: string,
    public order: number
  ) {
    this.title = title;
  }

  reloader: IReloader | null = null;
  @observable stackPosition: number = 0;
  @observable title = "";
  @observable isActive = false;
  isDialog = false;
  isClosed = false;

  @action.bound
  setActive(state: boolean): void {
    this.isActive = state;
  }

  setContent(screen: IFormScreenEnvelope): void {}

  setTitle(title: string): void {
    this.title = title;
  }

  get formTitle(){
    return this.content.formScreen?.title ?? this.title;
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
