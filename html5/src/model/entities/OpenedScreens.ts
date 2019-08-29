import { IOpenedScreens } from "./types/IOpenedScreens";
import { IOpenedScreen } from "./types/IOpenedScreen";
import { action, observable, computed } from "mobx";
import { IAction } from "./types/IAction";
import { isILoadedFormScreen } from "./types/IFormScreen";

export class OpenedScreens implements IOpenedScreens {
  $type_IOpenedScreens: 1 = 1;

  parent?: any;
  @observable items: Array<IOpenedScreen> = [];

  @computed get screenItems(): IOpenedScreen[] {
    return this.items.filter(item => !item.isDialog);
  }

  @computed get dialogItems(): IOpenedScreen[] {
    return this.items.filter(item => item.isDialog);
  }

  @action.bound
  pushItem(item: IOpenedScreen): void {
    this.items.push(item);
    item.parent = this;
  }

  @action.bound
  deleteItem(menuItemId: string, order: number): void {
    const idx = this.items.findIndex(
      item => item.menuItemId === menuItemId && item.order === order
    );
    idx > -1 && this.items.splice(idx, 1);
  }

  @action.bound activateItem(menuItemId: string, order: number) {
    this.items.forEach(item => item.setActive(false));
    const item = this.items.find(
      item => item.menuItemId === menuItemId && item.order === order
    );
    item && item.setActive(true);
  }

  @computed get activeItem(): IOpenedScreen | undefined {
    return this.items.find(item => item.isActive);
  }

  @computed get activeScreenActions(): Array<{
    section: string;
    actions: IAction[];
  }> {
    const activeScreen = this.activeItem;
    return activeScreen && isILoadedFormScreen(activeScreen.content)
      ? activeScreen.content.toolbarActions
      : [];
  }

  findLastExistingItem(menuItemId: string): IOpenedScreen | undefined {
    const items = this.items.filter(item => item.menuItemId === menuItemId);
    items.sort((a, b) => a.order - b.order);
    return items.slice(-1)[0];
  }

  findClosestItem(
    menuItemId: string,
    order: number
  ): IOpenedScreen | undefined {
    let idx = this.items.findIndex(
      item => item.menuItemId === menuItemId && item.order === order
    );
    if (idx === -1 || this.items.length === 1) {
      return undefined;
    }
    if (idx === 0) {
      idx = 1;
    } else {
      idx--;
    }
    return this.items[idx];
  }
}
